''' <summary>
''' Tela modal "Adicionar/Editar Banco" (aberta por <see cref="FrmBancos"/>):
''' edita e testa os dados de UMA conexão Postgres. Não grava nada em
''' config.json diretamente — só devolve a <see cref="ConexaoBanco"/> editada
''' via <see cref="Resultado"/> quando fechada com <see cref="DialogResult.OK"/>;
''' quem persiste a lista inteira é o chamador (<see cref="FrmBancos"/>).
''' </summary>
Public Class FrmServidor

    Private ReadOnly conexaoOriginal As ConexaoBanco

    ''' <summary>Conexão editada, disponível depois que a tela fecha com DialogResult.OK.</summary>
    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Resultado As ConexaoBanco

    ''' <summary>
    ''' </summary>
    ''' <param name="conexaoExistente">
    ''' A conexão a editar, ou Nothing para cadastrar uma nova.
    ''' </param>
    Public Sub New(Optional conexaoExistente As ConexaoBanco = Nothing)
        InitializeComponent()
        conexaoOriginal = conexaoExistente
    End Sub

    Private Sub FrmServidor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If conexaoOriginal Is Nothing Then
            txtPorta.Text = "5432"
            Exit Sub
        End If

        txtNome.Text = conexaoOriginal.Nome
        txtServidor.Text = conexaoOriginal.Servidor
        txtPorta.Text = conexaoOriginal.Porta.ToString()
        txtBanco.Text = conexaoOriginal.Banco
        txtUsuario.Text = conexaoOriginal.Usuario
        txtSenha.Text = conexaoOriginal.Senha
    End Sub

    ''' <summary>
    ''' Valida nome/porta e devolve os dados digitados via <see cref="Resultado"/>,
    ''' fechando a tela com DialogResult.OK.
    ''' </summary>
    Private Sub btnSalvar_Click(sender As Object, e As EventArgs) Handles btnSalvar.Click
        If String.IsNullOrWhiteSpace(txtNome.Text) Then
            MessageBox.Show("Informe um nome para identificar este banco.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim porta As Integer
        If Not Integer.TryParse(txtPorta.Text, porta) Then
            MessageBox.Show("Porta inválida. Informe um número inteiro.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Resultado = New ConexaoBanco With {
            .Nome = txtNome.Text.Trim(),
            .Servidor = txtServidor.Text,
            .Porta = porta,
            .Banco = txtBanco.Text,
            .Usuario = txtUsuario.Text,
            .Senha = txtSenha.Text
        }

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    ''' <summary>
    ''' Testa a conexão com os valores atualmente digitados na tela (não precisa
    ''' ter salvo antes) — abre e fecha a conexão só pra confirmar que os dados
    ''' estão certos, sem alterar nada no banco.
    ''' </summary>
    Private Sub btnTestar_Click(sender As Object, e As EventArgs) Handles btnTestar.Click
        Try

            Using conn = Conexao.Abrir(
            txtServidor.Text,
            Integer.Parse(txtPorta.Text),
            txtBanco.Text,
            txtUsuario.Text,
            txtSenha.Text)

                LogService.RegistrarAtividade($"Testar Conexão: {txtServidor.Text}:{txtPorta.Text}/{txtBanco.Text} -> sucesso")
                MessageBox.Show("Conexão realizada com sucesso!")

            End Using

        Catch ex As Exception

            LogService.RegistrarAtividade($"Testar Conexão: {txtServidor.Text}:{txtPorta.Text}/{txtBanco.Text} -> ERRO: {ex.Message}")
            MessageBox.Show("Erro ao conectar:" & vbCrLf & ex.Message)

        End Try

    End Sub
End Class
