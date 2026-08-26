Imports System.IO
Imports System.Text.Json

''' <summary>
''' Dados de uma falha do agendamento mensal que ainda não foi vista por
''' ninguém na tela. Ver <see cref="PendenciaAgendamentoService"/>.
''' </summary>
Public Class PendenciaAgendamento
    Public Property DataHora As DateTime
    Public Property Competencia As String
    Public Property Mensagem As String
    Public Property Notificada As Boolean
End Class

''' <summary>
''' Guarda, num arquivo compartilhado em %ProgramData%, a última falha do
''' agendamento mensal que ainda não foi avisada na tela — pra alguém saber
''' que o envio não aconteceu mesmo sem precisar abrir o app (só precisa
''' estar com ele na bandeja; ver <see cref="FrmPrincipal.VerificarPendenciaEAvisar"/>).
''' </summary>
''' <remarks>
''' Fica em %ProgramData% (não %LocalAppData%) de propósito: é o único lugar
''' que tanto o app interativo (usuário normal) quanto o Windows Service
''' (LocalSystem) conseguem ler e escrever da mesma forma — ver o mesmo
''' raciocínio em <see cref="LogService.PastaLogs"/>.
''' </remarks>
Public Class PendenciaAgendamentoService

    Private Shared ReadOnly Caminho As String =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ExportaXML", "pendencia_agendamento.json")

    ''' <summary>Grava uma nova pendência (substituindo qualquer uma anterior), marcada como ainda não notificada.</summary>
    Public Shared Sub Registrar(competencia As String, mensagem As String)
        Try
            Dim pasta As String = Path.GetDirectoryName(Caminho)
            If Not Directory.Exists(pasta) Then Directory.CreateDirectory(pasta)

            Dim pendencia As New PendenciaAgendamento With {
                .DataHora = DateTime.Now,
                .Competencia = competencia,
                .Mensagem = mensagem,
                .Notificada = False
            }

            File.WriteAllText(Caminho, JsonSerializer.Serialize(pendencia))
        Catch
            ' Melhor esforço: se não conseguir gravar a pendência, a falha
            ' original já foi registrada no log e no e-mail de alerta.
        End Try
    End Sub

    ''' <summary>Lê a pendência atual, ou Nothing se não houver nenhuma (arquivo inexistente ou corrompido).</summary>
    Public Shared Function Obter() As PendenciaAgendamento
        Try
            If Not File.Exists(Caminho) Then Return Nothing
            Return JsonSerializer.Deserialize(Of PendenciaAgendamento)(File.ReadAllText(Caminho))
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>Marca a pendência atual como já avisada, pra não repetir o balão de novo pela mesma falha.</summary>
    Public Shared Sub MarcarNotificada()
        Try
            Dim pendencia = Obter()
            If pendencia Is Nothing Then Return

            pendencia.Notificada = True
            File.WriteAllText(Caminho, JsonSerializer.Serialize(pendencia))
        Catch
        End Try
    End Sub

    ''' <summary>Remove a pendência — chamado quando um agendamento roda com sucesso.</summary>
    Public Shared Sub Limpar()
        Try
            If File.Exists(Caminho) Then File.Delete(Caminho)
        Catch
        End Try
    End Sub

End Class
