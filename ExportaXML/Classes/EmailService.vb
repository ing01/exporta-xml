Imports System.IO
Imports MailKit.Net.Smtp
Imports MailKit.Security
Imports MimeKit

Public Class EmailService

    Public Shared Sub Enviar(
        servidor As String,
        porta As Integer,
        usuario As String,
        senha As String,
        remetente As String,
        destinatario As String,
        assunto As String,
        mensagem As String,
        caminhoAnexo As String,
        usarSSL As Boolean)
        ' Validar emails simples
        If String.IsNullOrWhiteSpace(remetente) OrElse String.IsNullOrWhiteSpace(destinatario) Then
            Throw New ArgumentException("Remetente ou destinatário inválido.")
        End If

        Dim email As New MimeMessage()
        Try
            email.From.Add(New MailboxAddress(String.Empty, remetente))
            email.To.Add(New MailboxAddress(String.Empty, destinatario))
        Catch ex As FormatException
            Throw New ArgumentException("Endereço de e-mail inválido: " & ex.Message, ex)
        End Try

        email.Subject = assunto

        Dim builder As New BodyBuilder()
        builder.TextBody = mensagem

        If Not String.IsNullOrWhiteSpace(caminhoAnexo) AndAlso File.Exists(caminhoAnexo) Then
            builder.Attachments.Add(caminhoAnexo)
        End If

        email.Body = builder.ToMessageBody()

        Using smtp As New SmtpClient()
            smtp.ServerCertificateValidationCallback = Function(sender, certificate, chain, errors) True

            Dim opcao As SecureSocketOptions

            If usarSSL Then
                If porta = 465 Then
                    opcao = SecureSocketOptions.SslOnConnect
                Else
                    opcao = SecureSocketOptions.StartTls
                End If
            Else
                opcao = SecureSocketOptions.None
            End If

            Try
                smtp.Connect(servidor, porta, opcao)
                smtp.Authenticate(usuario, senha)
                smtp.Send(email)
                smtp.Disconnect(True)
            Catch ex As Exception
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Falha ao enviar e-mail", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)
                Throw
            End Try
        End Using

    End Sub

    Public Shared Sub Testar(
        servidor As String,
        porta As Integer,
        usuario As String,
        senha As String,
        remetente As String,
        destinatario As String,
        usarSSL As Boolean)

        Enviar(
            servidor,
            porta,
            usuario,
            senha,
            remetente,
            destinatario,
            "Teste de configuração SMTP",
            "Este e-mail foi enviado para validar a configuração do Exportador XML.",
            Nothing,
            usarSSL)

    End Sub

End Class