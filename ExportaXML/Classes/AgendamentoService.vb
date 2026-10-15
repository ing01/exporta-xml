Imports Npgsql
Imports System.IO
Imports Microsoft.Extensions.Hosting.WindowsServices

''' <summary>
''' Agendamento automático de exportação mensal. Uma vez por mês (a partir do
''' dia 01, no horário configurado), exporta os XMLs de TODAS as empresas
''' referentes ao mês anterior inteiro num único ZIP, e envia um único e-mail
''' com esse ZIP anexado. Quem decide QUANDO chamar isso é a tela principal
''' (Timer de 1h + verificação também ao abrir o app); esta classe só sabe
''' calcular "é hora de rodar?" e "rodar".
''' </summary>
Public Class AgendamentoService

    ''' <summary>
    ''' Calcula a competência (mês/ano) que deve ser exportada: sempre o mês
    ''' anterior ao mês da data informada, no formato "yyyy-MM".
    ''' </summary>
    ''' <param name="referencia">Normalmente <c>Date.Today</c> — a data "de hoje" usada como base.</param>
    ''' <example>Se <paramref name="referencia"/> for qualquer dia de setembro/2026, retorna "2026-08".</example>
    Public Shared Function CompetenciaAnterior(referencia As Date) As String
        Dim primeiroDiaMesAtual As New Date(referencia.Year, referencia.Month, 1)
        Dim ultimoDiaMesAnterior As Date = primeiroDiaMesAtual.AddDays(-1)
        Return ultimoDiaMesAnterior.ToString("yyyy-MM")
    End Function

    ''' <summary>
    ''' Indica se o agendamento deve rodar agora.
    ''' </summary>
    ''' <param name="cfg">Configuração atual (lida com <see cref="ConfiguracaoService.Carregar"/>).</param>
    ''' <returns>
    ''' True somente se: o agendamento estiver habilitado (<c>AgendamentoAtivo</c>);
    ''' a competência do mês anterior AINDA NÃO tiver sido executada
    ''' (<c>UltimaCompetenciaExecutada</c> diferente da competência alvo); hoje
    ''' já tiver chegado no dia configurado (<c>DiaAgendamento</c>, ajustado
    ''' pelo mês atual — ver <see cref="DiaEfetivo"/>); e já tiver passado do
    ''' horário configurado (<c>HoraAgendamento</c>/<c>MinutoAgendamento</c>) no dia de hoje.
    ''' </returns>
    ''' <remarks>
    ''' De propósito NÃO exige que hoje seja exatamente o dia configurado: se o
    ''' computador ficou desligado/suspenso nesse dia e só foi ligado depois,
    ''' esta função ainda retorna True (a competência continua sendo a mesma,
    ''' "mês anterior"), então o agendamento "atrasado" roda na próxima chance.
    ''' </remarks>
    Public Shared Function DeveExecutar(cfg As Configuracoes) As Boolean
        If Not cfg.AgendamentoAtivo Then Return False

        Dim competenciaAlvo As String = CompetenciaAnterior(Date.Today)
        If cfg.UltimaCompetenciaExecutada = competenciaAlvo Then Return False

        If Date.Today.Day < DiaEfetivo(cfg.DiaAgendamento, Date.Today) Then Return False

        Dim horarioAgendado As New TimeSpan(cfg.HoraAgendamento, cfg.MinutoAgendamento, 0)
        Return DateTime.Now.TimeOfDay >= horarioAgendado
    End Function

    ''' <summary>
    ''' Ajusta <paramref name="diaConfigurado"/> (1 a 31) pro mês de
    ''' <paramref name="referencia"/>: se esse dia não existir nesse mês (ex.:
    ''' 31 em abril, 30/31 em fevereiro), usa o último dia válido do mês em
    ''' vez de "pular" pro mês seguinte.
    ''' </summary>
    Public Shared Function DiaEfetivo(diaConfigurado As Integer, referencia As Date) As Integer
        Dim ultimoDiaDoMes As Integer = Date.DaysInMonth(referencia.Year, referencia.Month)
        Return Math.Min(diaConfigurado, ultimoDiaDoMes)
    End Function

    ''' <summary>
    ''' Executa o agendamento de verdade: calcula o período do mês anterior,
    ''' exporta o ZIP consolidado de todas as empresas, envia o e-mail com o
    ''' relatório e atualiza <c>UltimaCompetenciaExecutada</c> em config.json.
    ''' </summary>
    ''' <param name="cfg">
    ''' Configuração atual, incluindo <see cref="Configuracoes.Conexoes"/> — TODOS
    ''' os bancos configurados são exportados, um de cada vez, acumulando tudo no
    ''' mesmo ZIP final. É modificada (campo <c>UltimaCompetenciaExecutada</c>)
    ''' e regravada em disco quando a execução termina com sucesso.
    ''' </param>
    ''' <param name="atualizarStatus">
    ''' Callback opcional chamado com mensagens de progresso (ex.: pra atualizar
    ''' um Label na tela) — pode ser chamado a partir de qualquer thread, então
    ''' quem passar este callback deve tratar isso (ex.: via Invoke) se for
    ''' tocar em controles de UI diretamente. Também é a mesma callback repassada
    ''' internamente para <see cref="ExportadorXML.ExportarTodasEmpresas"/>.
    ''' </param>
    ''' <remarks>
    ''' Não valida se já deveria rodar — quem decide isso é
    ''' <see cref="DeveExecutar"/> (chamado antes, pelo código da tela) ou o
    ''' botão "Testar agora" (que ignora De propósito essa checagem). Cada
    ''' etapa e qualquer erro são gravados em
    ''' <c>%LocalAppData%\ExportaXML\Logs\Agendamento_{competencia}.log</c>. Se
    ''' algo falhar no meio do caminho (mesmo que só num dos bancos), o erro é
    ''' capturado, logado, e um e-mail de alerta é disparado (ver
    ''' <see cref="EnviarAlertaFalha"/>) — esta rotina nunca deixa uma exceção
    ''' subir pra quem chamou.
    ''' </remarks>
    Public Shared Function ExecutarAgendamentoMensal(
        cfg As Configuracoes,
        Optional atualizarStatus As Action(Of String) = Nothing,
        Optional forcar As Boolean = False) As Boolean

        Dim dataFinal As Date = New Date(Date.Today.Year, Date.Today.Month, 1).AddDays(-1)
        Dim dataInicial As New Date(dataFinal.Year, dataFinal.Month, 1)
        Dim competencia As String = dataFinal.ToString("yyyy-MM")

        Dim caminhoLog As String = Path.Combine(LogService.PastaLogs, $"Agendamento_{competencia}.log")

        ' Só UMA execução por vez em toda a máquina (app na bandeja + Windows
        ' Service, ou dois ticks seguidos do timer). Sem isso, a segunda
        ' execução reenviava os mesmos e-mails enquanto a primeira ainda rodava.
        Using trava = OperacaoEmAndamento.TentarObterTravaAgendamento()
            If trava Is Nothing Then
                If forcar Then
                    LogService.Registrar(caminhoLog, "Execução manual recusada: já existe uma exportação/agendamento em andamento.")
                End If
                Return False
            End If

            If Not forcar Then
                ' Releitura depois de obter a trava: a configuração recebida pode estar velha
                ' (outra execução pode ter acabado de concluir a competência).
                cfg = ConfiguracaoService.Carregar()
                If Not DeveExecutar(cfg) Then Return False

                ' Houve falha antes nesta competência: espera o intervalo entre tentativas
                ' (ou já esgotou as tentativas) — evita repetir a falha e o e-mail de alerta a cada minuto.
                If PendenciaAgendamentoService.DeveAguardar(competencia) Then Return False
            End If

            AvisarCompetenciasPuladas(cfg, competencia, caminhoLog)

            LogService.Registrar(caminhoLog, $"===== Início da exportação automática ({competencia}){If(forcar, " [execução manual]", "")} =====")

            Try
                atualizarStatus?.Invoke("Executando agendamento automático...")

                ' Área de Trabalho não existe/não faz sentido rodando como Windows
                ' Service sem sessão de usuário (LocalSystem) — nesse caso usa uma
                ' pasta fixa em %ProgramData%, sempre acessível independente de
                ' quem está logado.
                Dim pasta As String
                If WindowsServiceHelpers.IsWindowsService() Then
                    pasta = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "ExportaXML", "Exportacoes")
                    Directory.CreateDirectory(pasta)
                Else
                    pasta = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                End If

                ' Falhas por empresa (sem destinatário, erro de SMTP): não abortam as
                ' demais, mas precisam aparecer pro usuário no fim — e viram nova
                ' tentativa automática só das empresas que faltaram.
                Dim falhasEnvio As New List(Of String)

                For Each banco As ConexaoBanco In cfg.Conexoes
                    Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                        Dim empresas As List(Of EmpresaItem) = EmpresaService.Listar(conn).Where(Function(emp) emp.Codigo <> 0).ToList()

                        LogService.Registrar(caminhoLog, $"{banco.Nome}: {empresas.Count} empresa(s) encontrada(s).")

                        For Each empresa In empresas
                            Dim chaveEmpresa As String = $"{banco.Nome}|{empresa.Codigo}"

                            If Not forcar AndAlso PendenciaAgendamentoService.JaEnviada(competencia, chaveEmpresa) Then
                                LogService.Registrar(caminhoLog, $"{empresa.Nome}: e-mail desta competência já foi enviado antes, não será repetido.")
                                Continue For
                            End If

                            Dim erroEmpresa As String = EnviarEmpresa(cfg, conn, banco, empresa, dataInicial, dataFinal, competencia, pasta, caminhoLog, atualizarStatus)
                            If erroEmpresa IsNot Nothing Then
                                falhasEnvio.Add($"{empresa.Nome}: {erroEmpresa}")
                            Else
                                ' (inclui "sem movimento": não é falha, e não precisa tentar de novo)
                                PendenciaAgendamentoService.MarcarEnviada(competencia, chaveEmpresa)
                            End If
                        Next
                    End Using
                Next

                If falhasEnvio.Count = 0 Then
                    ConfiguracaoService.SalvarCompetenciaExecutada(competencia)
                    LogService.Registrar(caminhoLog, "===== Concluído com sucesso. =====")
                    atualizarStatus?.Invoke("Agendamento concluído com sucesso.")
                    PendenciaAgendamentoService.Limpar()
                Else
                    Dim resumo As String = $"{falhasEnvio.Count} empresa(s) sem envio — " & String.Join("; ", falhasEnvio)
                    Dim estado = PendenciaAgendamentoService.Registrar(competencia, resumo)
                    Dim desistiu As Boolean = estado.Tentativas >= PendenciaAgendamentoService.MaximoTentativas

                    If desistiu OrElse forcar Then
                        ConfiguracaoService.SalvarCompetenciaExecutada(competencia)
                    End If

                    LogService.Registrar(caminhoLog,
                        $"===== Concluído COM FALHAS (tentativa {estado.Tentativas} de {PendenciaAgendamentoService.MaximoTentativas}): {resumo} =====" &
                        If(desistiu OrElse forcar, "", $" Nova tentativa automática (só das empresas pendentes) após {estado.ProximaTentativa:dd/MM/yyyy HH:mm}."))
                    atualizarStatus?.Invoke("Agendamento concluído com falhas no envio de e-mail. Veja o log.")
                    RegistrarNoEventLog($"Agendamento do ExportaXML (competência {competencia}) com falhas no envio: {resumo}")

                    If estado.Tentativas = 1 OrElse desistiu Then
                        EnviarAlertaFalha(cfg, competencia, caminhoLog, resumo)
                    End If
                End If

            Catch ex As Exception
                ErroAmigavel.Registrar(caminhoLog, "ERRO", ex)
                atualizarStatus?.Invoke("Falha no agendamento automático. Veja o log.")

                ' Além do log e do e-mail de alerta (que também pode falhar, se o
                ' problema for justamente no SMTP): grava uma pendência visível
                ' (ver PendenciaAgendamentoService) e registra no Log de Eventos
                ' do Windows — duas formas de alguém saber da falha que não
                ' dependem do e-mail estar funcionando.
                Dim amigavel As String = ErroAmigavel.Traduzir(ex)
                Dim estado = PendenciaAgendamentoService.Registrar(competencia, amigavel)
                Dim desistiu As Boolean = estado.Tentativas >= PendenciaAgendamentoService.MaximoTentativas
                RegistrarNoEventLog($"Falha no agendamento automático do ExportaXML (competência {competencia}): {amigavel}")

                ' Alerta só na 1ª falha e na desistência — antes era um e-mail por minuto enquanto o problema durasse.
                If estado.Tentativas = 1 OrElse desistiu Then
                    EnviarAlertaFalha(cfg, competencia, caminhoLog, amigavel)
                End If
            End Try

            Return True
        End Using
    End Function

    ''' <summary>
    ''' Exporta e envia o e-mail de UMA empresa. Devolve Nothing se deu certo
    ''' (ou se a empresa não teve movimento — nada a enviar), ou o motivo da
    ''' falha em português.
    ''' </summary>
    Private Shared Function EnviarEmpresa(
        cfg As Configuracoes,
        conn As NpgsqlConnection,
        banco As ConexaoBanco,
        empresa As EmpresaItem,
        dataInicial As Date,
        dataFinal As Date,
        competencia As String,
        pasta As String,
        caminhoLog As String,
        atualizarStatus As Action(Of String)) As String

        Dim caminhoZipEmpresa As String = Path.Combine(pasta, $"{empresa.Codigo:000}_{ExportadorXML.NomeArquivoValido(empresa.Nome)}.zip")

        Try
            atualizarStatus?.Invoke($"Exportando empresa {empresa.Nome} ({empresa.Codigo})...")

            Dim total As Integer = ExportadorXML.ContarXMLs(conn, empresa.Codigo, dataInicial, dataFinal, True, True, True, 0, Nothing, Nothing, "")
            If total = 0 Then
                LogService.Registrar(caminhoLog, $"{banco.Nome} - {empresa.Nome}: sem XMLs em {competencia}; nenhum e-mail enviado.")
                Return Nothing
            End If

            If File.Exists(caminhoZipEmpresa) Then File.Delete(caminhoZipEmpresa)

            ' Grava direto no ZIP da empresa (antes passava por ExportarTodasEmpresas, que
            ' embrulhava esse ZIP dentro de outro ZIP com o mesmo nome).
            ExportadorXML.ExportarSaida(
                conn, empresa.Codigo, dataInicial, dataFinal, caminhoZipEmpresa,
                True, True, True, 0, Nothing, Nothing, "", Nothing, True)

            If Not File.Exists(caminhoZipEmpresa) Then
                LogService.Registrar(caminhoLog, $"{banco.Nome} - {empresa.Nome}: arquivo não gerado ({total} XML(s) esperados).")
                Return "o arquivo ZIP não foi gerado"
            End If

            LogService.Registrar(caminhoLog, $"{banco.Nome} - {empresa.Nome}: {total} XML(s) exportados.")

            Dim origemDestinatario As String = ""
            Dim destinatario As String = ObterDestinatarioParaEmpresa(cfg, conn, empresa.Codigo, origemDestinatario)
            ' Envio automático só vai para destinatário PRÓPRIO da empresa: cair no Global
            ' ou no "último usado" mandaria XML fiscal de uma empresa para outra pessoa.
            If String.IsNullOrWhiteSpace(destinatario) OrElse origemDestinatario <> "empresa" Then
                LogService.Registrar(caminhoLog, $"ERRO: a empresa {empresa.Nome} ({empresa.Codigo}) não tem destinatário próprio; e-mail NÃO enviado" &
                    If(String.IsNullOrWhiteSpace(destinatario), ".", $" (existe apenas {origemDestinatario}: {destinatario}, que não é usado no envio automático)."))
                Return "sem destinatário próprio cadastrado (cadastre em Configurar E-mail > Destinatários da empresa)"
            End If

            Dim mensagem As String =
                "Prezados," & vbCrLf & vbCrLf &
                $"Segue em anexo a exportação automática dos arquivos XML da empresa {empresa.Nome} referentes à competência {competencia}." & vbCrLf & vbCrLf &
                "Este e-mail foi enviado automaticamente pelo sistema."

            ' Cada empresa pode ter sua própria configuração de SMTP (manual ou
            ' sincronizada do banco DELA) — ver ConfiguracaoEmailService.ResolverConfiguracaoParaEmpresa.
            Dim configEmail = ConfiguracaoEmailService.ResolverConfiguracaoParaEmpresa(cfg, conn, empresa.Codigo)

            EmailService.Enviar(
                configEmail.Servidor,
                configEmail.Porta,
                configEmail.Usuario,
                configEmail.Senha,
                configEmail.Remetente.Trim(),
                destinatario.Trim(),
                $"XMLs Exportados - {empresa.Nome} - {competencia}",
                mensagem,
                caminhoZipEmpresa,
                configEmail.UsarSSL)

            LogService.Registrar(caminhoLog, $"E-mail enviado para {destinatario} (Empresa: {empresa.Nome}).")
            Return Nothing

        Catch ex As Exception
            ErroAmigavel.Registrar(caminhoLog, $"ERRO na empresa {empresa.Nome}", ex)
            Return ErroAmigavel.Traduzir(ex)
        Finally
            Try
                If File.Exists(caminhoZipEmpresa) Then File.Delete(caminhoZipEmpresa)
            Catch
            End Try
        End Try
    End Function

    ''' <summary>
    ''' Avisa no log e no Log de Eventos se um ou mais meses ficaram sem envio
    ''' (computador desligado/app fechado por mais de um mês) — o agendamento só
    ''' exporta o mês anterior, então os meses intermediários precisam ser
    ''' exportados à mão. Não bloqueia a execução.
    ''' </summary>
    Private Shared Sub AvisarCompetenciasPuladas(cfg As Configuracoes, competencia As String, caminhoLog As String)
        Try
            If String.IsNullOrWhiteSpace(cfg.UltimaCompetenciaExecutada) Then Return

            Dim ultima As Date = Date.ParseExact(cfg.UltimaCompetenciaExecutada, "yyyy-MM", Globalization.CultureInfo.InvariantCulture)
            Dim atual As Date = Date.ParseExact(competencia, "yyyy-MM", Globalization.CultureInfo.InvariantCulture)
            Dim meses As Integer = (atual.Year - ultima.Year) * 12 + atual.Month - ultima.Month

            If meses > 1 Then
                Dim msg As String = $"AVISO: {meses - 1} mês(es) entre {ultima.AddMonths(1):MM/yyyy} e {atual.AddMonths(-1):MM/yyyy} não foram enviados automaticamente (programa fechado ou computador desligado). Exporte esses períodos manualmente."
                LogService.Registrar(caminhoLog, msg)
                RegistrarNoEventLog(msg)
            End If
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Registra a falha no Log de Eventos do Windows (Visualizador de
    ''' Eventos, categoria "Application"), como reforço pro cenário em que
    ''' ninguém está com o app aberto na bandeja pra ver o balão de aviso.
    ''' Melhor esforço: se a fonte de evento ainda não existir e o processo
    ''' atual não tiver privilégio pra criá-la (app interativo sem elevação,
    ''' antes do Windows Service ter rodado alguma vez), simplesmente não
    ''' registra nada — não é uma falha crítica.
    ''' </summary>
    Private Shared Sub RegistrarNoEventLog(mensagem As String)
        Try
            Const origem As String = "ExportaXML"

            If Not Diagnostics.EventLog.SourceExists(origem) Then
                Diagnostics.EventLog.CreateEventSource(origem, "Application")
            End If

            Diagnostics.EventLog.WriteEntry(origem, mensagem, Diagnostics.EventLogEntryType.Error)
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Envia um e-mail avisando que o agendamento falhou, para
    ''' <c>cfg.EmailAlertaFalha</c> (se esse campo estiver vazio, não faz nada —
    ''' silenciosamente, já que alerta de falha é opcional). Usa a mesma conta
    ''' de e-mail resolvida para a configuração Global (manual OU sincronizada do
    ''' banco). Qualquer erro ao tentar enviar o próprio alerta é só registrado
    ''' no log, nunca lançado.
    ''' </summary>
    Private Shared Sub EnviarAlertaFalha(
        cfg As Configuracoes,
        competencia As String,
        caminhoLog As String,
        motivo As String)

        If String.IsNullOrWhiteSpace(cfg.EmailAlertaFalha) Then Return

        Dim conn As NpgsqlConnection = Nothing
        Try
            ' Conta "Global" pode estar configurada como sincronizada do banco: nesse
            ' caso os campos SMTP do config.json estão vazios e precisa consultar o banco.
            If cfg.OrigemConfiguracao = "Banco" AndAlso cfg.Conexoes IsNot Nothing AndAlso cfg.Conexoes.Count > 0 Then
                Try
                    Dim b = cfg.Conexoes(0)
                    conn = Conexao.Abrir(b.Servidor, b.Porta, b.Banco, b.Usuario, b.Senha)
                Catch
                    conn = Nothing
                End Try
            End If

            Dim c = ConfiguracaoEmailService.ResolverConfiguracaoParaEmpresa(cfg, conn, 0)

            EmailService.Enviar(
                c.Servidor,
                c.Porta,
                c.Usuario,
                c.Senha,
                c.Remetente.Trim(),
                cfg.EmailAlertaFalha.Trim(),
                $"[ExportaXML] Falha no agendamento automático - {competencia}",
                "Ocorreu uma falha na exportação automática da competência " & competencia & "." & vbCrLf & vbCrLf &
                "Motivo: " & motivo & vbCrLf & vbCrLf &
                "O programa tenta novamente sozinho (somente as empresas que faltaram). Para detalhes, abra o programa, aba Configurações, botão Ver log." & vbCrLf &
                "Arquivo de log: " & caminhoLog,
                Nothing,
                c.UsarSSL)

            LogService.Registrar(caminhoLog, $"E-mail de alerta enviado para {cfg.EmailAlertaFalha}.")
        Catch exAlerta As Exception
            ErroAmigavel.Registrar(caminhoLog, "ERRO ao enviar e-mail de alerta", exAlerta)
        Finally
            conn?.Dispose()
        End Try
    End Sub


    ''' <summary>
    ''' Resolve o destinatário de e-mail de uma empresa seguindo a mesma ordem de
    ''' prioridade usada pelo agendamento automático: destinatário local por empresa
    ''' (config.json) -> destinatário cadastrado no banco -> destinatário local
    ''' "Global" (CodigoEmpresa=0) -> campo legado <see cref="Configuracoes.UltimoDestinatario"/>.
    ''' Usado tanto pelo agendamento quanto pela exportação manual (<see cref="FrmPrincipal"/>),
    ''' pra não duplicar essa lógica em dois lugares.
    ''' </summary>
    Public Shared Function ObterDestinatarioParaEmpresa(cfg As Configuracoes, conn As NpgsqlConnection, codigoEmpresa As Integer) As String
        Dim origem As String = ""
        Return ObterDestinatarioParaEmpresa(cfg, conn, codigoEmpresa, origem)
    End Function

    ''' <summary>
    ''' Igual à sobrecarga acima, mas também informa em <paramref name="origem"/> DE ONDE
    ''' veio o endereço: "empresa" (cadastrado para a própria empresa, local ou no banco),
    ''' "o destinatário Global" ou "o último destinatário usado manualmente". O agendamento
    ''' usa isso para avisar no log quando um XML vai para um endereço que NÃO é da empresa.
    ''' </summary>
    Public Shared Function ObterDestinatarioParaEmpresa(cfg As Configuracoes, conn As NpgsqlConnection, codigoEmpresa As Integer, ByRef origem As String) As String
        origem = "empresa"

        ' 1) procurar em locais (config.json)
        If cfg IsNot Nothing AndAlso cfg.DestinatariosLocais IsNot Nothing Then
            Dim local = cfg.DestinatariosLocais.FirstOrDefault(Function(d) d.CodigoEmpresa = codigoEmpresa AndAlso d.Ativo)
            If local IsNot Nothing Then
                Return local.Email
            End If
        End If

        ' 2) tentar no DB (SELECT)
        Try
            If conn IsNot Nothing Then
                Dim dbVal = DestinatarioService.ObterPadrao(conn, codigoEmpresa)
                If Not String.IsNullOrWhiteSpace(dbVal) Then Return dbVal
            End If
        Catch
        End Try

        ' 3) procurar local Global (codigo 0)
        If cfg IsNot Nothing AndAlso cfg.DestinatariosLocais IsNot Nothing Then
            Dim localG = cfg.DestinatariosLocais.FirstOrDefault(Function(d) d.CodigoEmpresa = 0 AndAlso d.Ativo)
            If localG IsNot Nothing Then
                origem = If(codigoEmpresa = 0, "empresa", "o destinatário Global")
                Return localG.Email
            End If
        End If

        ' 4) fallback para cfg.UltimoDestinatario
        If cfg IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cfg.UltimoDestinatario) Then
            origem = "o último destinatário usado manualmente"
            Return cfg.UltimoDestinatario
        End If

        Return String.Empty
    End Function
End Class
