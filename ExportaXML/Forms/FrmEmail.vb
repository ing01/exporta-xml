Public Class FrmEmail

    Private Sub FrmEmail_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim cfg = ConfiguracaoService.Carregar()

        txtServidorSMTP.Text = cfg.ServidorSMTP
        txtPortaSMTP.Text = cfg.PortaSMTP.ToString()
        txtUsuario.Text = cfg.UsuarioSMTP
        txtSenha.Text = cfg.SenhaSMTP
        chkSSL.Checked = cfg.UsarSSL

    End Sub


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

            MessageBox.Show("Configuração salva com sucesso!")

            Me.Close()

        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub


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


            MessageBox.Show("E-mail enviado com sucesso!")


        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub

End Class