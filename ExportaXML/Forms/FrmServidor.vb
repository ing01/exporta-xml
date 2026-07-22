Public Class FrmServidor
    Private Sub FrmServidor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim cfg = ConfiguracaoService.Carregar()

        txtServidor.Text = cfg.Servidor
        txtPorta.Text = cfg.Porta
        txtBanco.Text = cfg.Banco
        txtUsuario.Text = cfg.Usuario
        txtSenha.Text = cfg.Senha

    End Sub

    Private Sub btnSalvar_Click(sender As Object, e As EventArgs) Handles btnSalvar.Click
        Dim cfg = ConfiguracaoService.Carregar()

        cfg.Servidor = txtServidor.Text

        Dim porta As Integer
        If Integer.TryParse(txtPorta.Text, porta) Then
            cfg.Porta = porta
        Else
            MessageBox.Show("Porta inválida. Informe um número inteiro.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        cfg.Banco = txtBanco.Text
        cfg.Usuario = txtUsuario.Text
        cfg.Senha = txtSenha.Text

        ConfiguracaoService.Salvar(cfg)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnTestar_Click(sender As Object, e As EventArgs) Handles btnTestar.Click
        Try

            Using conn = Conexao.Abrir(
            txtServidor.Text,
            Integer.Parse(txtPorta.Text),
            txtBanco.Text,
            txtUsuario.Text,
            txtSenha.Text)

                MessageBox.Show("Conexão realizada com sucesso!")

            End Using

        Catch ex As Exception

            MessageBox.Show("Erro ao conectar:" & vbCrLf & ex.Message)

        End Try

    End Sub
End Class