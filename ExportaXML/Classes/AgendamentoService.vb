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
    Public Shared Sub ExecutarAgendamentoMensal(
        cfg As Configuracoes,
        Optional atualizarStatus As Action(Of String) = Nothing)

        Dim dataFinal As Date = New Date(Date.Today.Year, Date.Today.Month, 1).AddDays(-1)
        Dim dataInicial As New Date(dataFinal.Year, dataFinal.Month, 1)
        Dim competencia As String = dataFinal.ToString("yyyy-MM")

        Dim caminhoLog As String = Path.Combine(LogService.PastaLogs, $"Agendamento_{competencia}.log")

        LogService.Registrar(caminhoLog, $"===== Início da exportação automática ({competencia}) =====")

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
            Dim nomeArquivo As String = ExportadorXML.NomeArquivoValido($"XMLs {competencia}") & ".zip"
            Dim caminhoZip As String = Path.Combine(pasta, nomeArquivo)

            If File.Exists(caminhoZip) Then
                File.Delete(caminhoZip)
            End If

            For Each banco As ConexaoBanco In cfg.Conexoes
                Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                    Dim empresas As List(Of EmpresaItem) = EmpresaService.Listar(conn).Where(Function(emp) emp.Codigo <> 0).ToList()

                    LogService.Registrar(caminhoLog, $"{banco.Nome}: {empresas.Count} empresa(s) encontrada(s).")

                    ' Para cada empresa, gera um ZIP específico e envia para o destinatário configurado para ela.
                    For Each empresa In empresas
                        atualizarStatus?.Invoke($"Exportando empresa {empresa.Nome} ({empresa.Codigo})...")
                        Dim nomeZipEmpresa As String = $"{empresa.Codigo:000}_{ExportadorXML.NomeArquivoValido(empresa.Nome)}.zip"
                        Dim caminhoZipEmpresa As String = Path.Combine(pasta, nomeZipEmpresa)
                        If File.Exists(caminhoZipEmpresa) Then File.Delete(caminhoZipEmpresa)

                        ' Exporta apenas esta empresa para o ZIP temporário
                        ExportadorXML.ExportarTodasEmpresas(
                            conn,
                            New List(Of EmpresaItem) From {empresa},
                            dataInicial,
                            dataFinal,
                            caminhoZipEmpresa,
                            True,
                            True,
                            True,
                            0,
                            Nothing,
                            Nothing,
                            "",
                            Sub(status As String)
                                LogService.Registrar(caminhoLog, $"{banco.Nome} - {empresa.Nome}: {status}")
                                atualizarStatus?.Invoke(status)
                            End Sub)

                        If Not File.Exists(caminhoZipEmpresa) Then
                            LogService.Registrar(caminhoLog, $"Arquivo não gerado para empresa {empresa.Nome} ({empresa.Codigo}), ignorando envio.")
                            Continue For
                        End If

                        ' Determina destinatário com a ordem de prioridade: locais (config), DB, locais global, cfg.UltimoDestinatario
                        Dim destinatario As String = ObterDestinatarioParaEmpresa(cfg, conn, empresa.Codigo)
                        If String.IsNullOrWhiteSpace(destinatario) Then
                            LogService.Registrar(caminhoLog, $"AVISO: nenhum destinatário configurado para empresa {empresa.Nome} ({empresa.Codigo}); e-mail não enviado.")
                            ' Apaga arquivo temporário para não acumular
                            Try
                                File.Delete(caminhoZipEmpresa)
                            Catch
                            End Try
                            Continue For
                        End If

                        Dim mensagem As String =
    "Prezados," & vbCrLf & vbCrLf &
    String.Format("Segue em anexo a exportação automática dos arquivos XML da empresa {0} referentes à competência {1}.", empresa.Nome, competencia) & vbCrLf & vbCrLf &
    "Este e-mail foi enviado automaticamente pelo sistema."

                        Try
                            EmailService.Enviar(
                                cfg.ServidorSMTP,
                                cfg.PortaSMTP,
                                cfg.UsuarioSMTP,
                                cfg.SenhaSMTP,
                                cfg.EmailRemetente.Trim(),
                                destinatario.Trim(),
                                $"XMLs Exportados - {empresa.Nome} - {competencia}",
                                mensagem,
                                caminhoZipEmpresa,
                                cfg.UsarSSL)

                            LogService.Registrar(caminhoLog, $"E-mail enviado para {destinatario} (Empresa: {empresa.Nome}).")
                        Catch exEnv As Exception
                            LogService.Registrar(caminhoLog, $"ERRO ao enviar e-mail para {destinatario} (Empresa: {empresa.Nome}): {exEnv.Message}")
                        Finally
                            Try
                                File.Delete(caminhoZipEmpresa)
                            Catch
                            End Try
                        End Try
                    Next
                End Using
            Next

            ' Marca a competência como executada e salva configuração
            cfg.UltimaCompetenciaExecutada = competencia
            ConfiguracaoService.Salvar(cfg)

            LogService.Registrar(caminhoLog, "===== Concluído com sucesso. =====")
            atualizarStatus?.Invoke("Agendamento concluído com sucesso.")
            PendenciaAgendamentoService.Limpar()

        Catch ex As Exception
            LogService.Registrar(caminhoLog, "ERRO: " & ex.ToString())
            atualizarStatus?.Invoke("Falha no agendamento automático. Veja o log.")

            ' Além do log e do e-mail de alerta (que também pode falhar, se o
            ' problema for justamente no SMTP): grava uma pendência visível
            ' (ver PendenciaAgendamentoService) e registra no Log de Eventos
            ' do Windows — duas formas de alguém saber da falha que não
            ' dependem do e-mail estar funcionando.
            PendenciaAgendamentoService.Registrar(competencia, ex.Message)
            RegistrarNoEventLog($"Falha no agendamento automático do ExportaXML (competência {competencia}): {ex.Message}")

            EnviarAlertaFalha(cfg, competencia, caminhoLog, ex)
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
    ''' silenciosamente, já que alerta de falha é opcional). Qualquer erro ao
    ''' tentar enviar o próprio alerta é só registrado no log, nunca lançado.
    ''' </summary>
    Private Shared Sub EnviarAlertaFalha(
        cfg As Configuracoes,
        competencia As String,
        caminhoLog As String,
        ex As Exception)

        If String.IsNullOrWhiteSpace(cfg.EmailAlertaFalha) Then Return

        Try
            EmailService.Enviar(
                cfg.ServidorSMTP,
                cfg.PortaSMTP,
                cfg.UsuarioSMTP,
                cfg.SenhaSMTP,
                cfg.EmailRemetente.Trim(),
                cfg.EmailAlertaFalha.Trim(),
                $"[ExportaXML] Falha no agendamento automático - {competencia}",
                "Ocorreu uma falha na exportação automática da competência " & competencia & "." & vbCrLf & vbCrLf &
                "Erro: " & ex.Message & vbCrLf & vbCrLf &
                "Consulte o log em: " & caminhoLog,
                Nothing,
                cfg.UsarSSL)

            LogService.Registrar(caminhoLog, $"E-mail de alerta enviado para {cfg.EmailAlertaFalha}.")
        Catch exAlerta As Exception
            LogService.Registrar(caminhoLog, "ERRO ao enviar e-mail de alerta: " & exAlerta.ToString())
        End Try
    End Sub


    Private Shared Function ObterDestinatarioParaEmpresa(cfg As Configuracoes, conn As NpgsqlConnection, codigoEmpresa As Integer) As String
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
                Return localG.Email
            End If
        End If

        ' 4) fallback para cfg.UltimoDestinatario
        If cfg IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cfg.UltimoDestinatario) Then
            Return cfg.UltimoDestinatario
        End If

        Return String.Empty
    End Function
End Class
