Imports System.Reflection.Emit

''' <summary>
''' Tela modal "Configurar E-mail" (aba Configurações → E-mail): edita e testa
''' o SMTP usado tanto pelo envio manual quanto pelo Agendamento automático.
''' </summary>
Public Class FrmEmail

    ''' <summary>Preenche os campos com o que já está salvo em config.json.</summary>
    Private Sub FrmEmail_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        lblAlerta.Text = "A configuração deve " & vbCrLf & "ser feita com o" & vbCrLf & "servidor do GMAIL!"

        Dim cfg = ConfiguracaoService.Carregar()

        txtServidorSMTP.Text = cfg.ServidorSMTP
        txtPortaSMTP.Text = cfg.PortaSMTP.ToString()
        txtUsuario.Text = cfg.UsuarioSMTP
        txtSenha.Text = cfg.SenhaSMTP
        chkSSL.Checked = cfg.UsarSSL

    End Sub


    ''' <summary>
    ''' Valida a porta e grava a configuração SMTP em config.json. O "usuário SMTP"
    ''' digitado também vira o e-mail remetente (<c>EmailRemetente</c>) — a tela
    ''' não tem um campo de remetente separado.
    ''' </summary>
    Private Sub btnSalvar_Click(sender As Object, e As EventArgs) Handles btnSalvar.Click

        Try

            Dim cfg = ConfiguracaoService.Carregar()

            cfg.ServidorSMTP = txtServidorSMTP.Text.Trim()

            Dim porta As Integer

            If Integer.TryParse(txtPortaSMTP.Text, porta) Then
                cfg.PortaSMTP = porta
            Else
                MessageBox.Show("Informe uma porta válida.")
                Exit Sub
            End If

            cfg.UsuarioSMTP = txtUsuario.Text.Trim()
            cfg.SenhaSMTP = txtSenha.Text
            cfg.EmailRemetente = txtUsuario.Text.Trim()
            cfg.UsarSSL = chkSSL.Checked

            ConfiguracaoService.Salvar(cfg)

            LogService.RegistrarAtividade($"E-mail salvo: SMTP {cfg.ServidorSMTP}:{cfg.PortaSMTP}, remetente ""{cfg.EmailRemetente}""")
            MessageBox.Show("Configuração salva com sucesso!")

            Me.Close()

        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub


    ''' <summary>
    ''' Valida os campos da tela, pergunta (via InputBox) um e-mail de destino, e
    ''' dispara um envio de teste real através de <see cref="EmailService.Testar"/>.
    ''' Usa os valores digitados na tela, não precisa ter salvo antes.
    ''' </summary>
    Private Sub btnTestarEnvio_Click(sender As Object, e As EventArgs) Handles btnTestarEnvio.Click

        Try

            If String.IsNullOrWhiteSpace(txtServidorSMTP.Text) Then
                MessageBox.Show("Informe o servidor SMTP.")
                txtServidorSMTP.Focus()
                Exit Sub
            End If


            Dim porta As Integer

            If Not Integer.TryParse(txtPortaSMTP.Text, porta) Then
                MessageBox.Show("Informe uma porta válida.")
                txtPortaSMTP.Focus()
                Exit Sub
            End If


            If String.IsNullOrWhiteSpace(txtUsuario.Text) Then
                MessageBox.Show("Informe o usuário SMTP.")
                txtUsuario.Focus()
                Exit Sub
            End If


            If String.IsNullOrWhiteSpace(txtSenha.Text) Then
                MessageBox.Show("Informe a senha.")
                txtSenha.Focus()
                Exit Sub
            End If


            Dim destinatario As String =
                InputBox(
                    "Informe o e-mail que receberá o teste:",
                    "Teste de Envio")


            If String.IsNullOrWhiteSpace(destinatario) Then
                Exit Sub
            End If


            Try

                Dim addr = New System.Net.Mail.MailAddress(destinatario)

            Catch

                MessageBox.Show("Informe um e-mail destinatário válido.")
                Exit Sub

            End Try


            EmailService.Testar(
                txtServidorSMTP.Text.Trim(),
                porta,
                txtUsuario.Text.Trim(),
                txtSenha.Text,
                txtUsuario.Text.Trim(),
                destinatario.Trim(),
                chkSSL.Checked)


            LogService.RegistrarAtividade($"Testar Envio de E-mail: para ""{destinatario.Trim()}"" -> sucesso")
            MessageBox.Show("E-mail enviado com sucesso!")


        Catch ex As Exception

            LogService.RegistrarAtividade($"Testar Envio de E-mail -> ERRO: {ex.Message}")
            MessageBox.Show(ex.Message)

        End Try

    End Sub

End Class