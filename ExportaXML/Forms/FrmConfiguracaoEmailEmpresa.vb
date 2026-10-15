''' <summary>
''' Tela modal "Configuração de E-mail" de UMA empresa específica (ou
''' "Global", a configuração padrão) — aberta por <see cref="FrmEmail"/> ao
''' selecionar uma linha da lista e clicar em "Configurar". O título grande no
''' topo (<see cref="lblEmpresaNome"/>) deixa explícito de qual empresa é essa
''' configuração, evitando a confusão de uma tela só com um combo pequeno.
''' </summary>
''' <remarks>
''' Tudo que precisa de banco (modo "Sincronizar" e a lista de destinatários
''' cadastrados no banco) roda em <see cref="Task.Run"/>, disparado só depois
''' que a janela já apareceu (evento Shown) — mesmo padrão de
''' <see cref="FrmPrincipal.FrmPrincipal_Shown"/> — pra um banco lento/fora do
''' ar nunca travar a tela. O Load só mexe em config.json (rápido, local).
''' </remarks>
Public Class FrmConfiguracaoEmailEmpresa

    Private ReadOnly empresa As EmpresaItem

    ''' <summary>True se algo foi salvo — <see cref="FrmEmail"/> usa isso pra saber se precisa atualizar a grade.</summary>
    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property Salvo As Boolean = False

    Public Sub New(empresaAlvo As EmpresaItem)
        InitializeComponent()
        empresa = empresaAlvo
    End Sub

    Private Sub FrmConfiguracaoEmailEmpresa_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Text = "Configuração de E-mail"
        lblEmpresaNome.Text = If(empresa.Codigo = 0, "Global (configuração padrão)", empresa.NomeExibicao)

        ' "Global" não tem pra quem herdar — só faz sentido Manual ou Banco.
        rbPadrao.Enabled = (empresa.Codigo <> 0)

        CarregarConfiguracaoLocal()
        AtualizarHabilitacaoCampos()

        grpDestinatarios.Enabled = False
        lbDestinatarios.Items.Clear()
        lbDestinatarios.Items.Add("Carregando...")
        lblStatus.Text = ""
    End Sub

    Private Async Sub FrmConfiguracaoEmailEmpresa_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If rbSincronizar.Checked Then
            Await CarregarConfiguracaoDoBancoAsync()
        End If

        Await LoadDestinatariosAsync()
        grpDestinatarios.Enabled = True
    End Sub

    ''' <summary>Só lê config.json (rápido) — decide o modo e, se Manual, já preenche os campos. Não toca em banco nenhum.</summary>
    Private Sub CarregarConfiguracaoLocal()
        Dim cfg = ConfiguracaoService.Carregar()

        If empresa.Codigo = 0 Then
            txtServidorSMTP.Text = cfg.ServidorSMTP
            txtPortaSMTP.Text = cfg.PortaSMTP.ToString()
            txtUsuario.Text = cfg.UsuarioSMTP
            txtSenha.Text = cfg.SenhaSMTP
            chkSSL.Checked = cfg.UsarSSL

            If cfg.OrigemConfiguracao = "Banco" Then
                rbSincronizar.Checked = True
            Else
                rbManual.Checked = True
            End If
        Else
            Dim override = cfg.ConfiguracoesEmailPorEmpresa?.FirstOrDefault(Function(c) c.CodigoEmpresa = empresa.Codigo)

            If override Is Nothing OrElse override.Origem = "Padrao" Then
                LimparCampos()
                rbPadrao.Checked = True
            ElseIf override.Origem = "Manual" Then
                txtServidorSMTP.Text = override.ServidorSMTP
                txtPortaSMTP.Text = override.PortaSMTP.ToString()
                txtUsuario.Text = override.UsuarioSMTP
                txtSenha.Text = override.SenhaSMTP
                chkSSL.Checked = override.UsarSSL
                rbManual.Checked = True
            Else ' "Banco"
                rbSincronizar.Checked = True
            End If
        End If
    End Sub

    Private Sub rbPadrao_CheckedChanged(sender As Object, e As EventArgs) Handles rbPadrao.CheckedChanged
        If rbPadrao.Checked Then
            LimparCampos()
            AtualizarHabilitacaoCampos()
            lblStatus.Text = "Esta empresa vai usar a configuração Global."
        End If
    End Sub

    Private Sub rbManual_CheckedChanged(sender As Object, e As EventArgs) Handles rbManual.CheckedChanged
        If rbManual.Checked Then
            AtualizarHabilitacaoCampos()
            lblStatus.Text = "Modo manual – edite os campos livremente."
        End If
    End Sub

    Private Async Sub rbSincronizar_CheckedChanged(sender As Object, e As EventArgs) Handles rbSincronizar.CheckedChanged
        If rbSincronizar.Checked Then
            AtualizarHabilitacaoCampos()
            Await CarregarConfiguracaoDoBancoAsync()
        End If
    End Sub

    Private Sub AtualizarHabilitacaoCampos()
        Dim habilitaManual As Boolean = rbManual.Checked
        txtServidorSMTP.Enabled = habilitaManual
        txtPortaSMTP.Enabled = habilitaManual
        txtUsuario.Enabled = habilitaManual
        txtSenha.Enabled = habilitaManual
        chkSSL.Enabled = habilitaManual

        lblAlerta.Text =
            If(rbPadrao.Checked, "Sem configuração própria — usa a config. Global.",
            If(rbSincronizar.Checked, "Buscado automaticamente do sistema do cliente.",
               "Informe o SMTP do provedor de e-mail do cliente."))
    End Sub

    ''' <summary>Abre a referência rápida (só leitura) de servidor/porta/autenticação dos provedores de e-mail mais comuns.</summary>
    Private Sub btnProvedoresComuns_Click(sender As Object, e As EventArgs) Handles btnProvedoresComuns.Click
        Dim frm As New FrmProvedoresEmail()
        frm.ShowDialog(Me)
    End Sub

    ''' <summary>
    ''' Conexão a usar pra consultar o banco — a do PRÓPRIO banco desta
    ''' empresa; pra "Global" (sem conexão própria), usa a primeira cadastrada
    ''' como aproximação.
    ''' </summary>
    Private Function ObterBancoParaConsulta() As ConexaoBanco
        If empresa.Conexao IsNot Nothing Then Return empresa.Conexao

        Dim cfg = ConfiguracaoService.Carregar()
        Return If(cfg.Conexoes.Count > 0, cfg.Conexoes(0), Nothing)
    End Function

    ''' <summary>Só I/O — sem tocar em controles — pra rodar dentro de <see cref="Task.Run"/>.</summary>
    Private Function BuscarConfiguracaoDoBanco() As (Encontrado As Boolean, Servidor As String, Porta As Integer, Usuario As String, Senha As String, UsarSSL As Boolean, Erro As String)
        Dim banco = ObterBancoParaConsulta()
        If banco Is Nothing Then
            Return (False, "", 0, "", "", False, "Nenhum banco configurado.")
        End If

        Try
            Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                Dim servidor As String = "", usuario As String = "", senha As String = ""
                Dim porta As Integer = 0
                Dim usarSSL As Boolean = False

                If ConfiguracaoEmailService.BuscarConfiguracao(conn, empresa.Codigo, servidor, porta, usuario, senha, usarSSL) Then
                    Return (True, servidor, porta, usuario, senha, usarSSL, "")
                End If

                Return (False, "", 0, "", "", False, "")
            End Using
        Catch ex As Exception
            Return (False, "", 0, "", "", False, ErroAmigavel.Traduzir(ex))
        End Try
    End Function

    Private Async Function CarregarConfiguracaoDoBancoAsync() As Task
        lblStatus.Text = "Consultando banco..."
        grpConexao.Enabled = False

        Dim resultado = Await Task.Run(Function() BuscarConfiguracaoDoBanco())

        grpConexao.Enabled = True
        AtualizarHabilitacaoCampos()

        If Not String.IsNullOrEmpty(resultado.Erro) Then
            LogService.RegistrarAtividade($"Erro ao buscar configuração de e-mail no banco: {resultado.Erro}")
            MessageBox.Show("Não foi possível consultar o banco: " & resultado.Erro, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            lblStatus.Text = ""
            Return
        End If

        If resultado.Encontrado Then
            txtServidorSMTP.Text = resultado.Servidor
            txtPortaSMTP.Text = resultado.Porta.ToString()
            txtUsuario.Text = resultado.Usuario
            txtSenha.Text = "********"
            chkSSL.Checked = resultado.UsarSSL
            lblStatus.Text = "Configuração carregada do banco (Empresa: " & empresa.Nome & ")"
        Else
            LimparCampos()
            lblStatus.Text = "Nenhuma configuração encontrada em conf_nfe para esta empresa."
        End If
    End Function

    Private Sub LimparCampos()
        txtServidorSMTP.Clear()
        txtPortaSMTP.Clear()
        txtUsuario.Clear()
        txtSenha.Clear()
        chkSSL.Checked = False
    End Sub

    Private Sub btnSalvar_Click(sender As Object, e As EventArgs) Handles btnSalvar.Click
        Try
            Dim cfg = ConfiguracaoService.Carregar()

            If empresa.Codigo = 0 Then
                If rbSincronizar.Checked Then
                    cfg.OrigemConfiguracao = "Banco"
                    cfg.SenhaSMTP = ""   ' não grava a senha — é buscada do banco de novo a cada envio
                Else
                    cfg.OrigemConfiguracao = "Manual"
                    cfg.SenhaSMTP = txtSenha.Text
                End If

                cfg.ServidorSMTP = txtServidorSMTP.Text.Trim()
                Dim portaValor As Integer = 0
                Integer.TryParse(txtPortaSMTP.Text, portaValor)
                cfg.PortaSMTP = portaValor
                cfg.UsuarioSMTP = txtUsuario.Text.Trim()
                cfg.EmailRemetente = txtUsuario.Text.Trim()
                cfg.UsarSSL = chkSSL.Checked
            Else
                If cfg.ConfiguracoesEmailPorEmpresa Is Nothing Then
                    cfg.ConfiguracoesEmailPorEmpresa = New List(Of ConfiguracaoEmailEmpresa)()
                End If

                cfg.ConfiguracoesEmailPorEmpresa.RemoveAll(Function(c) c.CodigoEmpresa = empresa.Codigo)

                If Not rbPadrao.Checked Then
                    Dim nova As New ConfiguracaoEmailEmpresa With {.CodigoEmpresa = empresa.Codigo}

                    If rbSincronizar.Checked Then
                        nova.Origem = "Banco"
                    Else
                        nova.Origem = "Manual"
                        nova.ServidorSMTP = txtServidorSMTP.Text.Trim()
                        Dim portaValor As Integer = 0
                        Integer.TryParse(txtPortaSMTP.Text, portaValor)
                        nova.PortaSMTP = portaValor
                        nova.UsuarioSMTP = txtUsuario.Text.Trim()
                        nova.SenhaSMTP = txtSenha.Text
                        nova.EmailRemetente = txtUsuario.Text.Trim()
                        nova.UsarSSL = chkSSL.Checked
                    End If

                    cfg.ConfiguracoesEmailPorEmpresa.Add(nova)
                End If
            End If

            ConfiguracaoService.Salvar(cfg)
            Salvo = True
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Não foi possível salvar a configuração." & vbCrLf & vbCrLf & ErroAmigavel.Traduzir(ex), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCancelar_Click(sender As Object, e As EventArgs) Handles btnCancelar.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Async Sub btnTestarEnvio_Click(sender As Object, e As EventArgs) Handles btnTestarEnvio.Click
        Try
            Dim servidor As String = txtServidorSMTP.Text.Trim()
            Dim porta As Integer
            If Not Integer.TryParse(txtPortaSMTP.Text, porta) Then
                MessageBox.Show("Porta inválida.")
                Exit Sub
            End If
            Dim usuario As String = txtUsuario.Text.Trim()
            Dim senha As String = txtSenha.Text
            Dim usarSSL As Boolean = chkSSL.Checked

            ' Sincronizado: a senha mostrada é "********", busca a real de novo do banco.
            If rbSincronizar.Checked Then
                btnTestarEnvio.Enabled = False
                lblStatus.Text = "Consultando banco..."
                Dim resultado = Await Task.Run(Function() BuscarConfiguracaoDoBanco())
                btnTestarEnvio.Enabled = True
                lblStatus.Text = ""

                If Not resultado.Encontrado Then
                    Dim msg = If(String.IsNullOrEmpty(resultado.Erro), "Não foi possível obter a senha do banco para o teste.", resultado.Erro)
                    MessageBox.Show(msg, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Exit Sub
                End If

                servidor = resultado.Servidor
                porta = resultado.Porta
                usuario = resultado.Usuario
                senha = resultado.Senha
                usarSSL = resultado.UsarSSL
            End If

            If String.IsNullOrWhiteSpace(servidor) Then Throw New Exception("Servidor SMTP não informado.")
            If String.IsNullOrWhiteSpace(usuario) Then Throw New Exception("Usuário SMTP não informado.")
            If String.IsNullOrWhiteSpace(senha) Then Throw New Exception("Senha SMTP não informada.")

            Dim destinatario As String = InputBox("Informe o e-mail que receberá o teste:", "Teste de Envio")
            If String.IsNullOrWhiteSpace(destinatario) Then Exit Sub
            Try
                Dim addr = New System.Net.Mail.MailAddress(destinatario)
            Catch
                MessageBox.Show("E-mail destinatário inválido.")
                Exit Sub
            End Try

            btnTestarEnvio.Enabled = False
            lblStatus.Text = "Enviando..."
            Dim servidorFinal = servidor, usuarioFinal = usuario, senhaFinal = senha
            Dim portaFinal = porta, sslFinal = usarSSL, erroEnvio As String = Nothing
            Await Task.Run(Sub()
                                Try
                                    EmailService.Testar(servidorFinal, portaFinal, usuarioFinal, senhaFinal, usuarioFinal, destinatario, sslFinal)
                                Catch exEnvio As Exception
                                    erroEnvio = ErroAmigavel.TraduzirComDetalhe(exEnvio)
                                End Try
                            End Sub)
            btnTestarEnvio.Enabled = True
            lblStatus.Text = ""

            If erroEnvio IsNot Nothing Then
                LogService.RegistrarAtividade($"Erro no teste de e-mail: {erroEnvio.Replace(vbCrLf, " ")}")
                MessageBox.Show("Não foi possível enviar o e-mail de teste." & vbCrLf & vbCrLf & erroEnvio, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                LogService.RegistrarAtividade($"Teste de e-mail enviado para {destinatario} com sucesso.")
                MessageBox.Show("E-mail enviado com sucesso!")
            End If

        Catch ex As Exception
            btnTestarEnvio.Enabled = True
            LogService.RegistrarAtividade($"Erro no teste de e-mail: {ex}")
            MessageBox.Show("Não foi possível enviar o e-mail de teste." & vbCrLf & vbCrLf & ErroAmigavel.Traduzir(ex), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Só I/O — sem tocar em controles — pra rodar dentro de <see cref="Task.Run"/>.</summary>
    Private Function BuscarDestinatariosDoBanco() As List(Of String)
        Dim resultado As New List(Of String)
        Try
            Dim banco = ObterBancoParaConsulta()
            If banco IsNot Nothing Then
                Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                    Dim lista = DestinatarioService.ListarPorEmpresa(conn, empresa.Codigo)
                    resultado.AddRange(lista)
                End Using
            End If
        Catch ex As Exception
            LogService.RegistrarAtividade($"Erro ao carregar destinatários do banco: {ex.Message}")
        End Try
        Return resultado
    End Function

    Private Async Function LoadDestinatariosAsync() As Task
        Dim cfg = ConfiguracaoService.Carregar()
        Dim doBanco = Await Task.Run(Function() BuscarDestinatariosDoBanco())

        lbDestinatarios.Items.Clear()
        For Each d In doBanco
            lbDestinatarios.Items.Add(d)
        Next

        Dim locais = cfg.DestinatariosLocais
        If locais IsNot Nothing Then
            For Each d In locais.Where(Function(x) x.CodigoEmpresa = empresa.Codigo AndAlso x.Ativo = True)
                If Not lbDestinatarios.Items.Contains(d.Email) Then lbDestinatarios.Items.Add(d.Email)
            Next
        End If
    End Function

    Private Async Sub btnAddDest_Click(sender As Object, e As EventArgs) Handles btnAddDest.Click
        Dim email = InputBox("Informe o e-mail do destinatário:", "Adicionar Destinatário")
        If String.IsNullOrWhiteSpace(email) Then Exit Sub
        Dim descricao = InputBox("Descrição (opcional):", "Adicionar Destinatário")
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.Add(New DestinatarioLocal With {.CodigoEmpresa = empresa.Codigo, .Email = email.Trim(), .Descricao = descricao, .Ativo = True})
        ConfiguracaoService.Salvar(locaisCfg)
        Salvo = True
        Await LoadDestinatariosAsync()
    End Sub

    Private Async Sub btnEditDest_Click(sender As Object, e As EventArgs) Handles btnEditDest.Click
        If lbDestinatarios.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione um destinatário para editar.")
            Exit Sub
        End If
        Dim oldEmail = lbDestinatarios.SelectedItem.ToString()
        Dim newEmail = InputBox("Editar e-mail:", "Editar Destinatário", oldEmail)
        If String.IsNullOrWhiteSpace(newEmail) Then Exit Sub
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.RemoveAll(Function(d) d.CodigoEmpresa = empresa.Codigo AndAlso d.Email = oldEmail)
        locaisCfg.DestinatariosLocais.Add(New DestinatarioLocal With {.CodigoEmpresa = empresa.Codigo, .Email = newEmail.Trim(), .Descricao = String.Empty, .Ativo = True})
        ConfiguracaoService.Salvar(locaisCfg)
        Salvo = True
        Await LoadDestinatariosAsync()
    End Sub

    Private Async Sub btnRemoveDest_Click(sender As Object, e As EventArgs) Handles btnRemoveDest.Click
        If lbDestinatarios.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione um destinatário para remover.")
            Exit Sub
        End If
        If MessageBox.Show("Confirmar remoção do destinatário selecionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Exit Sub
        End If
        Dim email = lbDestinatarios.SelectedItem.ToString()
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.RemoveAll(Function(d) d.CodigoEmpresa = empresa.Codigo AndAlso d.Email = email)
        ConfiguracaoService.Salvar(locaisCfg)
        Salvo = True
        Await LoadDestinatariosAsync()
    End Sub

End Class
