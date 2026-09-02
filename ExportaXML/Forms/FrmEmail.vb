Imports System.Reflection.Emit
Imports Org.BouncyCastle.Asn1.Cmp

Public Class FrmEmail
    Private _configuracaoSalva As Configuracoes
    Private _origemSincronizada As Boolean = False
    Private _codEmpresaSinc As Integer = 0

    Private Sub FrmEmail_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Configuração inicial dos radiobuttons
        rbManual.Checked = True
        rbSincronizar.Checked = False

        ' Preenche o combobox com empresas
        CarregarEmpresasNoCombo()

        ' Carrega configuração salva
        Dim cfg = ConfiguracaoService.Carregar()
        _configuracaoSalva = cfg

        ' Preenche campos com o que está salvo
        txtServidorSMTP.Text = cfg.ServidorSMTP
        txtPortaSMTP.Text = cfg.PortaSMTP.ToString()
        txtUsuario.Text = cfg.UsuarioSMTP
        txtSenha.Text = cfg.SenhaSMTP
        chkSSL.Checked = cfg.UsarSSL

        ' Se a origem salva for "Banco", marca o radio correspondente e carrega
        If cfg.OrigemConfiguracao = "Banco" Then
            rbSincronizar.Checked = True
            ' Tenta selecionar a empresa correspondente
            For Each item As EmpresaItem In cbEmpresaSinc.Items
                If item.Codigo = cfg.CodEmpresaSincronizada Then
                    cbEmpresaSinc.SelectedItem = item
                    Exit For
                End If
            Next
            ' Se não encontrou, seleciona Global (codigo 0)
            If cbEmpresaSinc.SelectedItem Is Nothing Then
                For Each item As EmpresaItem In cbEmpresaSinc.Items
                    If item.Codigo = 0 Then
                        cbEmpresaSinc.SelectedItem = item
                        Exit For
                    End If
                Next
            End If
            CarregarConfiguracaoDoBanco()
        Else
            rbManual.Checked = True
        End If

        ' Garantir que combo e painel de destinatários estejam sempre habilitados (permite Global)
        cbEmpresaSinc.Enabled = True
        grpDestinatarios.Enabled = True

        lblAlerta.ForeColor = SystemColors.ControlText
        lblAlerta.Text = "Informe o SMTP do" & vbCrLf & "provedor de e-mail" & vbCrLf & "do cliente."
        lblStatus.Text = ""
    End Sub

    Private Sub CarregarEmpresasNoCombo()
        ' Configura exibição
        cbEmpresaSinc.DisplayMember = "Nome"
        cbEmpresaSinc.ValueMember = "Codigo"

        ' Limpa e adiciona a opção Global
        cbEmpresaSinc.Items.Clear()
        cbEmpresaSinc.Items.Add(New EmpresaItem With {.Codigo = 0, .Nome = "Global"})

        ' Carrega as empresas do banco
        Dim cfg = ConfiguracaoService.Carregar()
        If cfg.Conexoes.Count > 0 Then
            Using conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta,
                                   cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario,
                                   cfg.Conexoes(0).Senha)
                Dim empresas = EmpresaService.Listar(conn)
                For Each emp In empresas
                    If emp.Codigo <> 0 Then
                        cbEmpresaSinc.Items.Add(emp)
                    End If
                Next
            End Using
        End If

        ' Seleciona o primeiro item
        If cbEmpresaSinc.Items.Count > 0 Then
            cbEmpresaSinc.SelectedIndex = 0
        End If
    End Sub

    Private Sub rbManual_CheckedChanged(sender As Object, e As EventArgs) Handles rbManual.CheckedChanged
        If rbManual.Checked Then
            txtServidorSMTP.Enabled = True
            txtPortaSMTP.Enabled = True
            txtUsuario.Enabled = True
            txtSenha.Enabled = True
            chkSSL.Enabled = True
            grpDestinatarios.Enabled = True
            lblStatus.Text = "Modo manual – edite os campos livremente. Use o painel de destinatários para mapear por empresa."
        End If
    End Sub

    Private Sub rbSincronizar_CheckedChanged(sender As Object, e As EventArgs) Handles rbSincronizar.CheckedChanged
        If rbSincronizar.Checked Then
            txtServidorSMTP.Enabled = False
            txtPortaSMTP.Enabled = False
            txtUsuario.Enabled = False
            txtSenha.Enabled = False
            chkSSL.Enabled = False
            grpDestinatarios.Enabled = True
            CarregarConfiguracaoDoBanco()
        End If
    End Sub

    Private Sub cbEmpresaSinc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbEmpresaSinc.SelectedIndexChanged
        ' Sempre carregar a lista de destinatários para a empresa selecionada, independente do modo (Manual/Banco)
        If cbEmpresaSinc.SelectedItem IsNot Nothing Then
            Dim item = DirectCast(cbEmpresaSinc.SelectedItem, EmpresaItem)
            LoadDestinatarios(item.Codigo)
        End If

        ' Se o usuário escolheu sincronizar do banco, também carregar a configuração SMTP
        If rbSincronizar.Checked AndAlso cbEmpresaSinc.SelectedItem IsNot Nothing Then
            CarregarConfiguracaoDoBanco()
        End If
    End Sub

    Private Sub CarregarConfiguracaoDoBanco()
        If cbEmpresaSinc.SelectedItem Is Nothing Then Exit Sub

        Dim item = DirectCast(cbEmpresaSinc.SelectedItem, EmpresaItem)
        Dim codEmpresa = item.Codigo

        Dim cfg = ConfiguracaoService.Carregar()
        If cfg.Conexoes.Count = 0 Then
            MessageBox.Show("Nenhum banco configurado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta,
                                 cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario,
                                 cfg.Conexoes(0).Senha)
        Using conn
            Dim servidor As String = ""
            Dim porta As Integer = 0
            Dim usuario As String = ""
            Dim senha As String = ""
            Dim usarSSL As Boolean = False

            ' Se Global (código 0) selecionado, não tentar sincronizar — pedir seleção de empresa
            If codEmpresa = 0 Then
                ' Não mostra erro; informa para o usuário selecionar uma empresa válida
                txtServidorSMTP.Clear()
                txtPortaSMTP.Clear()
                txtUsuario.Clear()
                txtSenha.Clear()
                chkSSL.Checked = False
                _origemSincronizada = False
                _codEmpresaSinc = 0
                lblStatus.Text = "Selecione uma empresa para sincronizar (Global não é válida)."
                Exit Sub
            End If

            If ConfiguracaoEmailService.BuscarConfiguracao(conn, codEmpresa, servidor, porta, usuario, senha, usarSSL) Then
                txtServidorSMTP.Text = If(servidor, String.Empty)
                txtPortaSMTP.Text = If(porta > 0, porta.ToString(), String.Empty)
                txtUsuario.Text = If(usuario, String.Empty)
                ' Mostra placeholder se houver senha, caso contrário vazio
                txtSenha.Text = If(String.IsNullOrEmpty(senha), String.Empty, "********")
                chkSSL.Checked = usarSSL
                _origemSincronizada = True
                _codEmpresaSinc = codEmpresa
                lblStatus.Text = "Configuração carregada do banco (Empresa: " & item.Nome & ")"
                ' Carrega destinatários para esta empresa (do DB e locais) imediatamente
                LoadDestinatarios(codEmpresa)
            Else
                ' Se não encontrou configuração para a empresa, apenas limpa campos sem erro
                txtServidorSMTP.Clear()
                txtPortaSMTP.Clear()
                txtUsuario.Clear()
                txtSenha.Clear()
                chkSSL.Checked = False
                _origemSincronizada = False
                lblStatus.Text = "Nenhuma configuração encontrada para esta empresa."
                ' Mesmo sem configuração no DB, carregar destinatários locais se houver
                LoadDestinatarios(codEmpresa)
            End If
        End Using
    End Sub

    Private Sub btnSalvar_Click(sender As Object, e As EventArgs) Handles btnSalvar.Click
        Try
            Dim cfg = ConfiguracaoService.Carregar()

            If rbManual.Checked Then
                cfg.ServidorSMTP = txtServidorSMTP.Text.Trim()
                Dim portaValor As Integer = 0
                If Integer.TryParse(txtPortaSMTP.Text, portaValor) Then
                    cfg.PortaSMTP = portaValor
                Else
                    cfg.PortaSMTP = 0
                End If
                cfg.UsuarioSMTP = txtUsuario.Text.Trim()
                cfg.SenhaSMTP = txtSenha.Text
                cfg.EmailRemetente = txtUsuario.Text.Trim()
                cfg.UsarSSL = chkSSL.Checked
                cfg.OrigemConfiguracao = "Manual"
                cfg.CodEmpresaSincronizada = 0
                _origemSincronizada = False
            ElseIf rbSincronizar.Checked Then
                cfg.OrigemConfiguracao = "Banco"
                cfg.CodEmpresaSincronizada = _codEmpresaSinc
                ' Não salvamos a senha do banco; salvamos os outros campos apenas para exibição futura
                cfg.ServidorSMTP = txtServidorSMTP.Text.Trim()
                Dim portaValor As Integer = 0
                If Integer.TryParse(txtPortaSMTP.Text, portaValor) Then
                    cfg.PortaSMTP = portaValor
                Else
                    cfg.PortaSMTP = 0
                End If
                cfg.UsuarioSMTP = txtUsuario.Text.Trim()
                cfg.SenhaSMTP = ""   ' não guarda senha
                cfg.UsarSSL = chkSSL.Checked
                cfg.EmailRemetente = txtUsuario.Text.Trim()
            End If

            ConfiguracaoService.Salvar(cfg)
            LogService.RegistrarAtividade($"E-mail salvo: Origem={cfg.OrigemConfiguracao}, EmpresaSinc={cfg.CodEmpresaSincronizada}")
            MessageBox.Show("Configuração salva com sucesso!")
            Me.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub btnTestarEnvio_Click(sender As Object, e As EventArgs) Handles btnTestarEnvio.Click
        Try
            ' Para testar, usamos os valores atuais da tela (já que o usuário pode ter editado)
            ' Se estiver sincronizado, as credenciais foram carregadas (mas a senha está oculta).
            ' Precisamos pegar a senha real do banco novamente se a origem for "Banco".
            Dim servidor As String = txtServidorSMTP.Text.Trim()
            Dim porta As Integer
            If Not Integer.TryParse(txtPortaSMTP.Text, porta) Then
                MessageBox.Show("Porta inválida.")
                Exit Sub
            End If
            Dim usuario As String = txtUsuario.Text.Trim()
            Dim senha As String = txtSenha.Text
            Dim usarSSL As Boolean = chkSSL.Checked

            ' Se a origem for sincronizada, a senha mostrada é "********", então precisamos buscar do banco.
            If rbSincronizar.Checked AndAlso _origemSincronizada Then
                ' Busca novamente do banco para obter a senha real
                Dim cfg = ConfiguracaoService.Carregar()
                If cfg.Conexoes.Count > 0 Then
                    Dim conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta,
                                             cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario,
                                             cfg.Conexoes(0).Senha)
                    Using conn
                        Dim servidorB As String = "", usuarioB As String = "", senhaB As String = ""
                        Dim portaB As Integer = 0, sslB As Boolean = False
                        If ConfiguracaoEmailService.BuscarConfiguracao(conn, _codEmpresaSinc, servidorB, portaB, usuarioB, senhaB, sslB) Then
                            senha = senhaB
                            ' Atualiza também os demais campos, caso tenham mudado
                            servidor = servidorB
                            porta = portaB
                            usuario = usuarioB
                            usarSSL = sslB
                        Else
                            MessageBox.Show("Não foi possível obter a senha do banco para o teste.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                    End Using
                Else
                    MessageBox.Show("Nenhum banco configurado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Exit Sub
                End If
            End If

            ' Validações básicas
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

            MessageBox.Show(senha)

            EmailService.Testar(servidor, porta, usuario, senha, usuario, destinatario, usarSSL)
            LogService.RegistrarAtividade($"Teste de e-mail enviado para {destinatario} com sucesso.")
            MessageBox.Show("E-mail enviado com sucesso!")

        Catch ex As Exception
            LogService.RegistrarAtividade($"Erro no teste de e-mail: {ex.Message}")
            MessageBox.Show("Erro ao enviar e-mail: " & ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadDestinatarios(codigoEmpresa As Integer)
        lbDestinatarios.Items.Clear()
        Dim cfg = ConfiguracaoService.Carregar()
        Try
            If cfg.Conexoes.Count > 0 Then
                Using conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta, cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario, cfg.Conexoes(0).Senha)
                    Try
                        ' Lista do DB para o código informado (0 = global entries no DB, se existirem)
                        Dim lista = DestinatarioService.ListarPorEmpresa(conn, codigoEmpresa)
                        For Each d In lista
                            lbDestinatarios.Items.Add(d)
                        Next
                    Catch ex As Exception
                        LogService.RegistrarAtividade($"Erro ao carregar destinatários do banco: {ex.Message}")
                    End Try
                End Using
            End If
        Catch ex As Exception
            LogService.RegistrarAtividade($"Erro ao carregar destinatários: {ex.Message}")
        End Try

        ' Adicionar os locais do config.json (se houver) — inclui entries com CodigoEmpresa = 0 (Global)
        Dim locais = cfg.DestinatariosLocais
        If locais IsNot Nothing Then
            For Each d In locais.Where(Function(x) x.CodigoEmpresa = codigoEmpresa AndAlso x.Ativo = True)
                If Not lbDestinatarios.Items.Contains(d.Email) Then lbDestinatarios.Items.Add(d.Email)
            Next
        End If
    End Sub

    Private Sub btnAddDest_Click(sender As Object, e As EventArgs) Handles btnAddDest.Click
        If cbEmpresaSinc.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione uma empresa antes de adicionar destinatários.")
            Exit Sub
        End If
        Dim item = DirectCast(cbEmpresaSinc.SelectedItem, EmpresaItem)

        Dim email = InputBox("Informe o e-mail do destinatário:", "Adicionar Destinatário")
        If String.IsNullOrWhiteSpace(email) Then Exit Sub
        Dim descricao = InputBox("Descrição (opcional):", "Adicionar Destinatário")
        ' Salva localmente em config.json (sem permissão de escrita no banco)
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.Add(New DestinatarioLocal With {.CodigoEmpresa = item.Codigo, .Email = email.Trim(), .Descricao = descricao, .Ativo = True})
        ConfiguracaoService.Salvar(locaisCfg)
        LoadDestinatarios(item.Codigo)
    End Sub

    Private Sub btnEditDest_Click(sender As Object, e As EventArgs) Handles btnEditDest.Click
        If lbDestinatarios.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione um destinatário para editar.")
            Exit Sub
        End If
        Dim oldEmail = lbDestinatarios.SelectedItem.ToString()
        Dim newEmail = InputBox("Editar e-mail:", "Editar Destinatário", oldEmail)
        If String.IsNullOrWhiteSpace(newEmail) Then Exit Sub
        ' Simplificação: remover o antigo e inserir o novo (não tracking de id)
        Dim item = DirectCast(cbEmpresaSinc.SelectedItem, EmpresaItem)
        ' Edição simplificada: remove local e insere novo nos locais (não tenta alterar o DB)
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        ' Remove local com oldEmail e mesma empresa
        locaisCfg.DestinatariosLocais.RemoveAll(Function(d) d.CodigoEmpresa = item.Codigo AndAlso d.Email = oldEmail)
        locaisCfg.DestinatariosLocais.Add(New DestinatarioLocal With {.CodigoEmpresa = item.Codigo, .Email = newEmail.Trim(), .Descricao = String.Empty, .Ativo = True})
        ConfiguracaoService.Salvar(locaisCfg)
        LoadDestinatarios(item.Codigo)
    End Sub

    Private Sub btnRemoveDest_Click(sender As Object, e As EventArgs) Handles btnRemoveDest.Click
        If lbDestinatarios.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione um destinatário para remover.")
            Exit Sub
        End If
        If MessageBox.Show("Confirmar remoção do destinatário selecionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Exit Sub
        End If
        Dim email = lbDestinatarios.SelectedItem.ToString()
        Dim item = DirectCast(cbEmpresaSinc.SelectedItem, EmpresaItem)
        Dim cfg = ConfiguracaoService.Carregar()
        If cfg.Conexoes.Count = 0 Then
            MessageBox.Show("Nenhum banco configurado.")
            Exit Sub
        End If
        ' Remove local mapping apenas
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.RemoveAll(Function(d) d.CodigoEmpresa = item.Codigo AndAlso d.Email = email)
        ConfiguracaoService.Salvar(locaisCfg)
        LoadDestinatarios(item.Codigo)
    End Sub

End Class