Imports System.IO
Imports MailKit.Net.Smtp
Imports MailKit.Security
Imports MimeKit

''' <summary>
''' Envio de e-mails via SMTP (MailKit), usado tanto pelo envio manual após uma
''' exportação quanto pelo agendamento automático e pelos alertas de falha.
''' </summary>
Public Class EmailService

    ''' <summary>
    ''' Monta e envia um e-mail com anexo opcional.
    ''' </summary>
    ''' <param name="servidor">Host do servidor SMTP (ex.: smtp.gmail.com).</param>
    ''' <param name="porta">Porta do servidor SMTP (ex.: 587).</param>
    ''' <param name="usuario">Usuário usado para autenticar no SMTP.</param>
    ''' <param name="senha">Senha (ou senha de app) do usuário SMTP.</param>
    ''' <param name="remetente">Endereço que aparece como remetente do e-mail.</param>
    ''' <param name="destinatario">Endereço que vai receber o e-mail.</param>
    ''' <param name="assunto">Assunto do e-mail.</param>
    ''' <param name="mensagem">Corpo do e-mail em texto simples.</param>
    ''' <param name="caminhoAnexo">
    ''' Caminho de um arquivo a anexar. Se vazio/Nothing ou o arquivo não existir,
    ''' o e-mail é enviado sem anexo (não gera erro).
    ''' </param>
    ''' <param name="usarSSL">
    ''' True (padrão/recomendado pra praticamente qualquer provedor — Gmail,
    ''' Outlook/Office365, webmail de domínio próprio): usa
    ''' <see cref="SecureSocketOptions.Auto"/>, que decide sozinha entre
    ''' SSL/STARTTLS de acordo com a porta. False: conexão sem nenhuma
    ''' criptografia (<see cref="SecureSocketOptions.None"/>) — só pra algum
    ''' servidor interno/legado que não suporte TLS de jeito nenhum; não
    ''' desmarque pra provedores comuns.
    ''' </param>
    ''' <remarks>
    ''' ATENÇÃO: <c>ServerCertificateValidationCallback</c> está fixado para aceitar
    ''' qualquer certificado do servidor SMTP, mesmo inválido/expirado/autoassinado.
    ''' Isso evita erros de certificado em ambientes mal configurados, mas abre
    ''' brecha para um ataque man-in-the-middle se o SMTP for acessado por uma rede
    ''' não confiável. Lança <see cref="ArgumentException"/> se remetente,
    ''' destinatário ou o formato de algum dos dois for inválido.
    ''' </remarks>
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

            smtp.ServerCertificateValidationCallback =
        Function(sender, certificate, chain, errors) True

            ' Escolhe a opção de segurança explicitamente por porta — evita ambiguidade com provedores que exigem SslOnConnect (465) ou StartTls (587).
            Dim opcaoSeguranca As SecureSocketOptions
            If porta = 465 Then
                opcaoSeguranca = SecureSocketOptions.SslOnConnect
            ElseIf porta = 587 Then
                opcaoSeguranca = SecureSocketOptions.StartTls
            Else
                opcaoSeguranca = If(usarSSL, SecureSocketOptions.Auto, SecureSocketOptions.None)
            End If

            smtp.Connect(
        servidor,
        porta,
        opcaoSeguranca)

            ' Alguns servidores anunciam mecanismos de autenticação (ex: XOAUTH2) que não aplicam quando usamos usuário/senha.
            ' Removemos XOAUTH2 para forçar mecanismos tradicionais (PLAIN/LOGIN) e evitar falhas em servidores mal configurados.
            Try
                smtp.AuthenticationMechanisms.Remove("XOAUTH2")
            Catch
            End Try

            smtp.Authenticate(usuario, senha)

            smtp.Send(email)

            smtp.Disconnect(True)

        End Using

    End Sub

    ''' <summary>
    ''' Atalho para validar uma configuração de SMTP: envia um e-mail de teste
    ''' fixo, sem anexo, usando as mesmas credenciais que seriam usadas de verdade.
    ''' Usado pelo botão "Testar Envio" da tela de configuração de e-mail.
    ''' </summary>
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