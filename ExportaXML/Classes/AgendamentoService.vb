Imports Npgsql
Imports System.IO

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
    ''' (<c>UltimaCompetenciaExecutada</c> diferente da competência alvo); e já
    ''' tiver passado do horário configurado (<c>HoraAgendamento</c>/<c>MinutoAgendamento</c>) no dia de hoje.
    ''' </returns>
    ''' <remarks>
    ''' De propósito NÃO exige que hoje seja exatamente o dia 01: se o
    ''' computador ficou desligado/suspenso no dia 01 e só foi ligado no dia 05,
    ''' esta função ainda retorna True (a competência continua sendo a mesma,
    ''' "mês anterior"), então o agendamento "atrasado" roda na próxima chance.
    ''' </remarks>
    Public Shared Function DeveExecutar(cfg As Configuracoes) As Boolean
        If Not cfg.AgendamentoAtivo Then Return False

        Dim competenciaAlvo As String = CompetenciaAnterior(Date.Today)
        If cfg.UltimaCompetenciaExecutada = competenciaAlvo Then Return False

        Dim horarioAgendado As New TimeSpan(cfg.HoraAgendamento, cfg.MinutoAgendamento, 0)
        Return DateTime.Now.TimeOfDay >= horarioAgendado
    End Function

    ''' <summary>
    ''' Executa o agendamento de verdade: calcula o período do mês anterior,
    ''' exporta o ZIP consolidado de todas as empresas, envia o e-mail com o
    ''' relatório e atualiza <c>UltimaCompetenciaExecutada</c> em config.json.
    ''' </summary>
    ''' <param name="conn">Conexão já aberta com o banco.</param>
    ''' <param name="cfg">
    ''' Configuração atual. É modificada (campo <c>UltimaCompetenciaExecutada</c>)
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
    ''' algo falhar no meio do caminho, o erro é capturado, logado, e um e-mail
    ''' de alerta é disparado (ver <see cref="EnviarAlertaFalha"/>) — esta rotina
    ''' nunca deixa uma exceção subir pra quem chamou.
    ''' </remarks>
    Public Shared Sub ExecutarAgendamentoMensal(
        conn As NpgsqlConnection,
        cfg As Configuracoes,
        Optional atualizarStatus As Action(Of String) = Nothing)

        Dim dataFinal As Date = New Date(Date.Today.Year, Date.Today.Month, 1).AddDays(-1)
        Dim dataInicial As New Date(dataFinal.Year, dataFinal.Month, 1)
        Dim competencia As String = dataFinal.ToString("yyyy-MM")

        Dim caminhoLog As String = Path.Combine(LogService.PastaLogs, $"Agendamento_{competencia}.log")

        LogService.Registrar(caminhoLog, $"===== Início da exportação automática ({competencia}) =====")

        Try
            atualizarStatus?.Invoke("Executando agendamento automático...")

            Dim empresas As List(Of EmpresaItem) =
                EmpresaService.Listar(conn).Where(Function(emp) emp.Codigo <> 0).ToList()

            LogService.Registrar(caminhoLog, $"{empresas.Count} empresa(s) encontrada(s).")

            Dim pasta As String = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            Dim nomeArquivo As String = ExportadorXML.NomeArquivoValido($"XMLs {competencia}") & ".zip"
            Dim caminhoZip As String = Path.Combine(pasta, nomeArquivo)

            If File.Exists(caminhoZip) Then
                File.Delete(caminhoZip)
            End If

            ExportadorXML.ExportarTodasEmpresas(
                conn,
                empresas,
                dataInicial,
                dataFinal,
                caminhoZip,
                True,
                True,
                True,
                0,
                Nothing,
                Nothing,
                "",
                Sub(status As String)
                    LogService.Registrar(caminhoLog, status)
                    atualizarStatus?.Invoke(status)
                End Sub)

            LogService.Registrar(caminhoLog, $"ZIP gerado em: {caminhoZip}")

            If String.IsNullOrWhiteSpace(cfg.UltimoDestinatario) Then
                LogService.Registrar(caminhoLog, "AVISO: nenhum destinatário configurado; e-mail não enviado.")
            Else
                Dim mensagem As String =
                    "Prezados," & vbCrLf & vbCrLf &
                    $"Segue em anexo a exportação automática dos arquivos XML referentes à competência {competencia}." & vbCrLf & vbCrLf &
                    "O arquivo ZIP contém os arquivos XML separados por empresa." & vbCrLf & vbCrLf &
                    "Este e-mail foi enviado automaticamente pelo sistema."

                EmailService.Enviar(
                    cfg.ServidorSMTP,
                    cfg.PortaSMTP,
                    cfg.UsuarioSMTP,
                    cfg.SenhaSMTP,
                    cfg.EmailRemetente.Trim(),
                    cfg.UltimoDestinatario.Trim(),
                    $"XMLs Exportados - Todas as empresas - {competencia}",
                    mensagem,
                    caminhoZip,
                    cfg.UsarSSL)

                LogService.Registrar(caminhoLog, $"E-mail enviado para {cfg.UltimoDestinatario}.")
            End If

            cfg.UltimaCompetenciaExecutada = competencia
            ConfiguracaoService.Salvar(cfg)

            LogService.Registrar(caminhoLog, "===== Concluído com sucesso. =====")
            atualizarStatus?.Invoke("Agendamento concluído com sucesso.")

        Catch ex As Exception
            LogService.Registrar(caminhoLog, "ERRO: " & ex.ToString())
            atualizarStatus?.Invoke("Falha no agendamento automático. Veja o log.")

            EnviarAlertaFalha(cfg, competencia, caminhoLog, ex)
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

End Class
