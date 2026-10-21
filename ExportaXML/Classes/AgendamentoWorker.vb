Imports System.IO
Imports Microsoft.Extensions.Hosting

''' <summary>
''' Versão do polling de agendamento (<see cref="AgendamentoService"/>) para
''' rodar dentro do Windows Service, sem UI. Equivalente ao par
''' tmrAgendamento_Tick/VerificarEExecutarAgendamento de FrmPrincipal.vb, mas
''' hospedado pelo Generic Host em vez de um Timer de formulário.
''' </summary>
Public Class AgendamentoWorker
    Inherits BackgroundService

    ''' <summary>
    ''' Curto de propósito — o custo de checar (só ler config.json e comparar
    ''' datas) é desprezível, e mantém o atraso máximo entre "chegou a hora
    ''' configurada" e "executou de fato" baixo mesmo sem ninguém reiniciar o
    ''' serviço depois de reconfigurar (ver mesmo raciocínio em
    ''' FrmPrincipal.tmrAgendamento).
    ''' </summary>
    Private Shared ReadOnly IntervaloChecagem As TimeSpan = TimeSpan.FromMinutes(1)

    Protected Overrides Async Function ExecuteAsync(stoppingToken As Threading.CancellationToken) As Threading.Tasks.Task
        Do While Not stoppingToken.IsCancellationRequested
            VerificarEExecutarAgendamento()

            Try
                Await Threading.Tasks.Task.Delay(IntervaloChecagem, stoppingToken)
            Catch ex As TaskCanceledException
                Exit Do
            End Try
        Loop
    End Function

    ''' <summary>
    ''' Mesma lógica de FrmPrincipal.VerificarEExecutarAgendamento: carrega a
    ''' configuração atual e, se for a hora, executa o agendamento mensal.
    ''' Qualquer falha na checagem em si é engolida silenciosamente — falhas
    ''' na execução de verdade já são tratadas e logadas dentro de
    ''' AgendamentoService.ExecutarAgendamentoMensal.
    ''' </summary>
    Private Shared _ultimaSituacao As String

    ''' <summary>Registra em Servico.log, em português, a situação do agendamento (só quando muda ou vira o dia).</summary>
    Private Shared Sub LogarSituacao(cfg As Configuracoes)
        Dim situacao As String = $"{Date.Today:yyyyMMdd}|{AgendamentoService.DescreverSituacao(cfg)}"
        If situacao = _ultimaSituacao Then Return
        _ultimaSituacao = situacao
        LogService.Registrar(Path.Combine(LogService.PastaLogs, "Servico.log"), "Agendamento: " & situacao.Substring(situacao.IndexOf("|"c) + 1))
    End Sub

    Private Shared Sub VerificarEExecutarAgendamento()
        Try
            Dim cfg = ConfiguracaoService.Carregar()
            LogarSituacao(cfg)
            If Not AgendamentoService.DeveExecutar(cfg) Then Return

            AgendamentoService.ExecutarAgendamentoMensal(cfg)
        Catch ex As Exception
            ' Antes isto era engolido sem rastro e ninguém descobria que o serviço nem tentava rodar.
            ErroAmigavel.Registrar(Path.Combine(LogService.PastaLogs, "Servico.log"), "ERRO na checagem do agendamento", ex)
        End Try
    End Sub

End Class
