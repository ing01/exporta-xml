Public Class FrmEmail

    Private Sub FrmEmail_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim cfg = ConfiguracaoService.Carregar()

        txtServidorSMTP.Text = cfg.ServidorSMTP
        txtPortaSMTP.Text = cfg.PortaSMTP.ToString()
        txtUsuario.Text = cfg.UsuarioSMTP
        txtSenha.Text = cfg.SenhaSMTP
        txtRemetente.Text = cfg.EmailRemetente

    End Sub

    Private Sub btnSalvar_Click(sender As Object, e As EventArgs) Handles btnSalvar.Click

        Try

            Dim cfg = ConfiguracaoService.Carregar()

            cfg.ServidorSMTP = txtServidorSMTP.Text

            Dim porta As Integer
            If Integer.TryParse(txtPortaSMTP.Text, porta) Then
                cfg.PortaSMTP = porta
            End If

            cfg.UsuarioSMTP = txtUsuario.Text
            cfg.SenhaSMTP = txtSenha.Text
            cfg.EmailRemetente = txtRemetente.Text
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
                MessageBox.Show("Informe o usuário.")
                txtUsuario.Focus()
                Exit Sub
            End If

            If String.IsNullOrWhiteSpace(txtSenha.Text) Then
                MessageBox.Show("Informe a senha.")
                txtSenha.Focus()
                Exit Sub
            End If

            ' If String.IsNullOrWhiteSpace(txtRemetente.Text) Then
            'MessageBox.Show("Informe o e-mail remetente.")
            'txtRemetente.Focus()
            'Exit Sub
            'End If

            Dim destinatario As String =
                InputBox(
                    "Informe o e-mail que receberá o teste:",
                    "Teste de Envio")

            If String.IsNullOrWhiteSpace(destinatario) Then
                Exit Sub
            End If

            EmailService.Testar(
            txtServidorSMTP.Text,
            porta,
            txtUsuario.Text,
            txtSenha.Text,
            txtRemetente.Text,
            destinatario,
            chkSSL.Checked)

            MessageBox.Show("E-mail enviado com sucesso!")

        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub

End Class