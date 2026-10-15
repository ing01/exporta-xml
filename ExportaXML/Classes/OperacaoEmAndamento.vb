Imports System.IO
Imports System.Threading

''' <summary>
''' Controla "tem algo importante rodando agora?" para três coisas que antes
''' podiam atropelar umas às outras:
''' (1) duas execuções do agendamento ao mesmo tempo (timer de 1 min do app + Windows Service);
''' (2) atualização automática / "Sair" derrubando o programa no meio de um envio;
''' (3) agendamento e exportação manual mexendo juntos.
''' </summary>
Public Class OperacaoEmAndamento

    Private Shared _contador As Integer

    ''' <summary>True se há exportação manual ou agendamento rodando neste processo.</summary>
    Public Shared ReadOnly Property HaOperacao As Boolean
        Get
            Return Volatile.Read(_contador) > 0
        End Get
    End Property

    ''' <summary>Marca o início de uma operação longa; descarte (Using) ao terminar.</summary>
    Public Shared Function Iniciar() As IDisposable
        Interlocked.Increment(_contador)
        Return New Marca()
    End Function

    Private NotInheritable Class Marca
        Implements IDisposable
        Private _liberado As Integer
        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.Exchange(_liberado, 1) = 0 Then Interlocked.Decrement(_contador)
        End Sub
    End Class

    Private Shared ReadOnly CaminhoTrava As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ExportaXML", "agendamento.lock")

    ''' <summary>
    ''' Tenta ser a única execução do agendamento em TODA a máquina (app + serviço).
    ''' Devolve Nothing se outra já está rodando (ou se o programa já está no meio
    ''' de uma exportação manual) — quem chama simplesmente tenta de novo no
    ''' próximo minuto. O arquivo de trava é liberado pelo Windows mesmo se o
    ''' processo cair, então nunca "trava" para sempre.
    ''' </summary>
    Public Shared Function TentarObterTravaAgendamento() As IDisposable
        If HaOperacao Then Return Nothing

        Dim arquivo As FileStream = Nothing
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(CaminhoTrava))
            arquivo = New FileStream(CaminhoTrava, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose)
        Catch ex As IOException
            Return Nothing ' outro processo está com a trava
        Catch ex As UnauthorizedAccessException
            ' Sem permissão na pasta compartilhada: melhor rodar só com a trava interna do que nunca rodar.
            LogService.Registrar(Path.Combine(LogService.PastaLogs, "Agendamento_Geral.log"),
                                 "AVISO: sem permissão para criar a trava de execução em ProgramData; seguindo sem proteção entre processos.")
        End Try

        Dim marca = Iniciar()
        Return New TravaAgendamento(arquivo, marca)
    End Function

    Private NotInheritable Class TravaAgendamento
        Implements IDisposable
        Private ReadOnly _arquivo As FileStream
        Private ReadOnly _marca As IDisposable
        Public Sub New(arquivo As FileStream, marca As IDisposable)
            _arquivo = arquivo
            _marca = marca
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                _arquivo?.Dispose()
            Catch
            End Try
            _marca.Dispose()
        End Sub
    End Class

End Class
