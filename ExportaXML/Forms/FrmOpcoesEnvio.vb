Imports MimeKit
Imports Npgsql

''' <summary>
''' Janela mostrada quando o usuário responde "Sim" em "Deseja enviar o arquivo
''' por e-mail?" após uma exportação. Deixa escolher (e CONFERIR) de qual conta
''' de e-mail o envio sai e pra qual endereço vai — em especial em "Todas as
''' empresas", onde antes o envio usava silenciosamente a configuração Global
''' e podia falhar sem o usuário saber por quê.
''' </summary>
''' <remarks>
''' Montada por código (sem Designer), mesmo padrão de <see cref="FrmLog"/>.
''' Depois do <c>ShowDialog</c> == OK, ler <see cref="Destinatario"/> e
''' <see cref="ConfigEmail"/>; ambos já foram validados.
''' </remarks>
Public Class FrmOpcoesEnvio
    Inherits Form

    ''' <summary>Uma opção do combo "Conta de envio": a configuração Global ou a de uma das empresas exportadas.</summary>
    Private Class OpcaoConta
        Public Property Nome As String
        Public Property Codigo As Integer
        Public Property Conexao As ConexaoBanco
        Public Overrides Function ToString() As String
            Return Nome
        End Function
    End Class

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Destinatario As String

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property ConfigEmail As (Servidor As String, Porta As Integer, Usuario As String, Senha As String, Remetente As String, UsarSSL As Boolean)

    Private ReadOnly _cfg As Configuracoes
    Private ReadOnly cboConta As New ComboBox()
    Private ReadOnly lblResumoConta As New Label()
    Private ReadOnly cboDestinatario As New ComboBox()
    Private ReadOnly lblDica As New Label()
    Private ReadOnly btnEnviar As New Button()
    Private ReadOnly btnCancelar As New Button()

    Public Sub New(cfg As Configuracoes, empresasExportadas As List(Of EmpresaItem), todasEmpresas As Boolean)
        _cfg = cfg

        Text = "Enviar por e-mail"
        Font = New Font("Arial", 9.0F)
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ClientSize = New Size(560, 290)

        Dim lblConta As New Label With {.Text = "Enviar usando a conta de e-mail de:", .Location = New Point(12, 14), .AutoSize = True}
        cboConta.Location = New Point(12, 34)
        cboConta.Size = New Size(536, 23)
        cboConta.DropDownStyle = ComboBoxStyle.DropDownList

        lblResumoConta.Location = New Point(12, 62)
        lblResumoConta.Size = New Size(536, 48)
        lblResumoConta.ForeColor = SystemColors.GrayText

        Dim lblDest As New Label With {.Text = "Destinatário(s) — separe vários com ponto e vírgula:", .Location = New Point(12, 120), .AutoSize = True}
        cboDestinatario.Location = New Point(12, 140)
        cboDestinatario.Size = New Size(536, 23)
        cboDestinatario.DropDownStyle = ComboBoxStyle.DropDown

        lblDica.Location = New Point(12, 170)
        lblDica.Size = New Size(536, 60)
        lblDica.ForeColor = SystemColors.GrayText
        lblDica.Text = If(todasEmpresas,
            """Todas as empresas"" gera um único ZIP. Escolha acima a conta que deve enviá-lo e confira o destinatário.",
            "Confira a conta e o destinatário antes de enviar.")

        btnEnviar.Text = "Enviar"
        btnEnviar.Location = New Point(340, 245)
        btnEnviar.Size = New Size(100, 30)
        btnEnviar.UseVisualStyleBackColor = True

        btnCancelar.Text = "Cancelar"
        btnCancelar.Location = New Point(448, 245)
        btnCancelar.Size = New Size(100, 30)
        btnCancelar.DialogResult = DialogResult.Cancel
        btnCancelar.UseVisualStyleBackColor = True

        Controls.AddRange(New Control() {lblConta, cboConta, lblResumoConta, lblDest, cboDestinatario, lblDica, btnEnviar, btnCancelar})
        CancelButton = btnCancelar

        PreencherOpcoes(empresasExportadas, todasEmpresas)

        AddHandler cboConta.SelectedIndexChanged, Sub(s, e) AtualizarResumoConta()
        AddHandler btnEnviar.Click, AddressOf btnEnviar_Click
        AddHandler Shown, Sub(s, e) AtualizarResumoConta()
    End Sub

    Private Sub PreencherOpcoes(empresas As List(Of EmpresaItem), todasEmpresas As Boolean)
        Dim conexaoGlobal As ConexaoBanco = If(_cfg.Conexoes IsNot Nothing AndAlso _cfg.Conexoes.Count > 0, _cfg.Conexoes(0), Nothing)

        cboConta.Items.Add(New OpcaoConta With {.Nome = "Global (padrão)", .Codigo = 0, .Conexao = conexaoGlobal})
        For Each emp In empresas
            cboConta.Items.Add(New OpcaoConta With {
                .Nome = If(emp.Conexao IsNot Nothing AndAlso _cfg.Conexoes.Count > 1, $"{emp.Nome} ({emp.Conexao.Nome})", emp.Nome),
                .Codigo = emp.Codigo,
                .Conexao = emp.Conexao})
        Next

        ' Uma empresa só: já vem selecionada a conta dela (cai pra Global sozinha se ela não tiver uma própria).
        cboConta.SelectedIndex = If(Not todasEmpresas AndAlso cboConta.Items.Count > 1, 1, 0)

        ' Sugestões de destinatário: cada empresa exportada (local -> banco -> global -> legado) + todos os locais ativos.
        Dim sugestoes As New List(Of String)
        Dim conexoes As New Dictionary(Of String, NpgsqlConnection)
        Try
            Dim candidatas As New List(Of OpcaoConta)
            For Each o As OpcaoConta In cboConta.Items
                candidatas.Add(o)
            Next

            For Each o In candidatas
                Dim conn As NpgsqlConnection = AbrirConexao(o.Conexao, conexoes)
                Dim email As String = AgendamentoService.ObterDestinatarioParaEmpresa(_cfg, conn, o.Codigo).Trim()
                If email <> "" AndAlso Not sugestoes.Contains(email, StringComparer.OrdinalIgnoreCase) Then sugestoes.Add(email)
            Next
        Finally
            For Each c In conexoes.Values
                c?.Dispose()
            Next
        End Try

        If _cfg.DestinatariosLocais IsNot Nothing Then
            For Each d In _cfg.DestinatariosLocais.Where(Function(x) x.Ativo AndAlso Not String.IsNullOrWhiteSpace(x.Email))
                If Not sugestoes.Contains(d.Email.Trim(), StringComparer.OrdinalIgnoreCase) Then sugestoes.Add(d.Email.Trim())
            Next
        End If

        cboDestinatario.Items.AddRange(sugestoes.Cast(Of Object).ToArray())
        If sugestoes.Count > 0 Then cboDestinatario.Text = sugestoes(0)
    End Sub

    ''' <summary>Abre (e reaproveita) uma conexão por banco; Nothing se não houver banco ou se falhar — os chamadores seguem só com o que é local.</summary>
    Private Shared Function AbrirConexao(banco As ConexaoBanco, cache As Dictionary(Of String, NpgsqlConnection)) As NpgsqlConnection
        If banco Is Nothing Then Return Nothing
        Dim chave As String = $"{banco.Servidor}|{banco.Porta}|{banco.Banco}"
        Dim conn As NpgsqlConnection = Nothing
        If cache IsNot Nothing AndAlso cache.TryGetValue(chave, conn) Then Return conn
        Try
            conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
        Catch
            conn = Nothing
        End Try
        If cache IsNot Nothing Then cache(chave) = conn
        Return conn
    End Function

    Private Function ResolverContaSelecionada() As (Servidor As String, Porta As Integer, Usuario As String, Senha As String, Remetente As String, UsarSSL As Boolean)
        Dim opcao = DirectCast(cboConta.SelectedItem, OpcaoConta)
        Dim conn As NpgsqlConnection = Nothing
        Try
            conn = AbrirConexao(opcao.Conexao, Nothing)
            Return ConfiguracaoEmailService.ResolverConfiguracaoParaEmpresa(_cfg, conn, opcao.Codigo)
        Finally
            conn?.Dispose()
        End Try
    End Function

    Private Sub AtualizarResumoConta()
        If cboConta.SelectedItem Is Nothing Then Return
        Try
            Dim c = ResolverContaSelecionada()
            Dim opcao = DirectCast(cboConta.SelectedItem, OpcaoConta)

            ' Empresa sem conta própria usa a Global — avisa, senão parece que a troca de conta não fez nada.
            Dim usaGlobal As Boolean = False
            If opcao.Codigo <> 0 Then
                Dim ov = _cfg.ConfiguracoesEmailPorEmpresa?.FirstOrDefault(Function(x) x.CodigoEmpresa = opcao.Codigo)
                usaGlobal = (ov Is Nothing OrElse ov.Origem = "Padrao")
            End If
            Dim aviso As String = If(usaGlobal, "Esta empresa não tem conta própria: está usando a configuração Global." & vbCrLf, "")

            Dim problema As String = ValidarConta(c)
            If problema <> "" Then
                lblResumoConta.ForeColor = Color.Firebrick
                lblResumoConta.Text = aviso & problema
            Else
                lblResumoConta.ForeColor = SystemColors.GrayText
                lblResumoConta.Text = aviso & $"Servidor: {c.Servidor}:{c.Porta}   |   Usuário: {c.Usuario}" & vbCrLf &
                                      $"Remetente: {c.Remetente}   |   SSL/TLS: {If(c.UsarSSL, "sim", "não")}"
            End If
        Catch ex As Exception
            lblResumoConta.ForeColor = Color.Firebrick
            lblResumoConta.Text = $"Não foi possível ler essa conta: {ErroAmigavel.Traduzir(ex)}"
        End Try
    End Sub

    ''' <summary>Retorna o motivo pelo qual a conta não serve pra enviar, ou "" se está completa.</summary>
    Private Shared Function ValidarConta(c As (Servidor As String, Porta As Integer, Usuario As String, Senha As String, Remetente As String, UsarSSL As Boolean)) As String
        If String.IsNullOrWhiteSpace(c.Servidor) Then Return "Essa conta não tem servidor SMTP configurado. Configure em Configurar E-mail ou escolha outra conta."
        If c.Porta <= 0 Then Return "Essa conta não tem porta SMTP configurada. Configure em Configurar E-mail ou escolha outra conta."
        If String.IsNullOrWhiteSpace(c.Usuario) OrElse String.IsNullOrWhiteSpace(c.Senha) Then Return "Essa conta não tem usuário/senha configurados. Configure em Configurar E-mail ou escolha outra conta."
        If String.IsNullOrWhiteSpace(c.Remetente) Then Return "Essa conta não tem remetente configurado. Configure em Configurar E-mail ou escolha outra conta."
        Return ""
    End Function

    Private Sub btnEnviar_Click(sender As Object, e As EventArgs)
        Dim enderecos = cboDestinatario.Text.Split({";"c, ","c}, StringSplitOptions.RemoveEmptyEntries).
            Select(Function(x) x.Trim()).Where(Function(x) x <> "").ToList()

        If enderecos.Count = 0 Then
            MessageBox.Show("Informe pelo menos um destinatário.", "Enviar por e-mail", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cboDestinatario.Focus()
            Return
        End If

        For Each end_ In enderecos
            Dim ignorado As MailboxAddress = Nothing
            If Not MailboxAddress.TryParse(end_, ignorado) Then
                MessageBox.Show($"Endereço de e-mail inválido: {end_}", "Enviar por e-mail", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                cboDestinatario.Focus()
                Return
            End If
        Next

        Dim conta = ResolverContaSelecionada()
        Dim problema = ValidarConta(conta)
        If problema <> "" Then
            MessageBox.Show(problema, "Enviar por e-mail", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cboConta.Focus()
            Return
        End If

        Destinatario = String.Join(",", enderecos)
        ConfigEmail = conta
        DialogResult = DialogResult.OK
    End Sub

End Class
