Imports Microsoft.Extensions.Hosting

''' <summary>
''' Versão do polling de agendamento (<see cref="AgendamentoService"/>) para
''' rodar dentro do Windows Service, sem UI. Equivalente ao par
''' tmrAgendamento_Tick/VerificarEExecutarAgendamento de FrmPrincipal.vb, mas
''' hospedado pelo Generic Host em vez de um Timer de formulário.
''' </summary>
Public Class AgendamentoWorker
    Inherits BackgroundService

    Private Shared ReadOnly IntervaloChecagem As TimeSpan = TimeSpan.FromHours(1)

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
    Private Shared Sub VerificarEExecutarAgendamento()
        Try
            Dim cfg = ConfiguracaoService.Carregar()
            If Not AgendamentoService.DeveExecutar(cfg) Then Return

            AgendamentoService.ExecutarAgendamentoMensal(cfg)
        Catch
        End Try
    End Sub

End Class
