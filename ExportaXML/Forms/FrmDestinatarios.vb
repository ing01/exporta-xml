Public Class FrmDestinatarios
    Private Sub FrmDestinatarios_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Carrega empresas no combo
        cbEmpresas.DisplayMember = "Nome"
        cbEmpresas.ValueMember = "Codigo"

        cbEmpresas.Items.Clear()
        cbEmpresas.Items.Add(New EmpresaItem With {.Codigo = 0, .Nome = "Global"})

        Dim cfg = ConfiguracaoService.Carregar()
        If cfg.Conexoes.Count > 0 Then
            Using conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta, cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario, cfg.Conexoes(0).Senha)
                Dim empresas = EmpresaService.Listar(conn)
                For Each emp In empresas
                    If emp.Codigo <> 0 Then cbEmpresas.Items.Add(emp)
                Next
            End Using
        End If

        If cbEmpresas.Items.Count > 0 Then cbEmpresas.SelectedIndex = 0
        ' Garantir que ao abrir a tela os destinatários locais/DB sejam carregados imediatamente
        LoadDestinatariosForSelectedCompany()
    End Sub

    Private Sub cbEmpresas_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbEmpresas.SelectedIndexChanged
        LoadDestinatariosForSelectedCompany()
    End Sub

    Private Sub LoadDestinatariosForSelectedCompany()
        lbEmails.Items.Clear()
        Dim item = TryCast(cbEmpresas.SelectedItem, EmpresaItem)
        Dim cfg = ConfiguracaoService.Carregar()

        ' Se não houve seleção, nada a fazer
        If item Is Nothing Then Exit Sub

        ' Se selecionado Global (0) listar todos os mapeamentos: do DB (por empresa) e os locais
        If item.Codigo = 0 Then
            ' 1) tentar listar do DB por empresa (se houver conexão)
            If cfg.Conexoes.Count > 0 Then
                Using conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta, cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario, cfg.Conexoes(0).Senha)
                    Try
                        Dim empresas = EmpresaService.Listar(conn)
                        For Each emp In empresas
                            Try
                                Dim lista = DestinatarioService.ListarPorEmpresa(conn, emp.Codigo)
                                For Each d In lista
                                    Dim display = $"{emp.Nome} - {d}"
                                    If Not lbEmails.Items.Contains(display) Then lbEmails.Items.Add(display)
                                Next
                            Catch
                                ' ignora falhas por empresa
                            End Try
                        Next
                    Catch ex As Exception
                        LogService.RegistrarAtividade($"Erro ao carregar destinatários do banco: {ex.Message}")
                    End Try
                End Using
            End If

            ' 2) adicionar os locais do config.json, com o nome da empresa quando possível
            Dim locais = cfg.DestinatariosLocais
            If locais IsNot Nothing Then
                For Each d In locais.Where(Function(x) x.Ativo = True)
                    Dim nomeEmp As String = d.CodigoEmpresa.ToString()
                    ' tenta mapear o nome pela lista carregada no combo
                    For Each it In cbEmpresas.Items
                        Dim ei = TryCast(it, EmpresaItem)
                        If ei IsNot Nothing AndAlso ei.Codigo = d.CodigoEmpresa Then
                            nomeEmp = ei.Nome
                            Exit For
                        End If
                    Next
                    Dim display = $"{nomeEmp} - {d.Email}"
                    If Not lbEmails.Items.Contains(display) Then lbEmails.Items.Add(display)
                Next
            End If

            Exit Sub
        End If

        ' Caso empresa específica selecionada: carregar do DB (SELECT) e locais do config
        If cfg.Conexoes.Count > 0 Then
            Using conn = Conexao.Abrir(cfg.Conexoes(0).Servidor, cfg.Conexoes(0).Porta, cfg.Conexoes(0).Banco, cfg.Conexoes(0).Usuario, cfg.Conexoes(0).Senha)
                Try
                    Dim lista = DestinatarioService.ListarPorEmpresa(conn, item.Codigo)
                    For Each d In lista
                        If Not lbEmails.Items.Contains(d) Then lbEmails.Items.Add(d)
                    Next
                Catch ex As Exception
                    LogService.RegistrarAtividade($"Erro ao carregar destinatários do banco: {ex.Message}")
                End Try
            End Using
        End If

        ' Sempre adicionar os locais do config.json (se houver)
        Dim locais2 = cfg.DestinatariosLocais
        If locais2 IsNot Nothing Then
            For Each d In locais2.Where(Function(x) x.CodigoEmpresa = item.Codigo AndAlso x.Ativo = True)
                If Not lbEmails.Items.Contains(d.Email) Then lbEmails.Items.Add(d.Email)
            Next
        End If
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim item = TryCast(cbEmpresas.SelectedItem, EmpresaItem)
        If item Is Nothing Then
            MessageBox.Show("Selecione uma empresa válida.")
            Exit Sub
        End If
        Dim email = InputBox("Informe o e-mail:", "Adicionar destinatário")
        If String.IsNullOrWhiteSpace(email) Then Exit Sub
        Dim descricao = InputBox("Descrição (opcional):", "Adicionar destinatário")
        Dim cfg = ConfiguracaoService.Carregar()
        ' Salva localmente em config.json (sem permissão de escrita no banco)
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.Add(New DestinatarioLocal With {.CodigoEmpresa = item.Codigo, .Email = email.Trim(), .Descricao = descricao, .Ativo = True})
        ConfiguracaoService.Salvar(locaisCfg)
        LoadDestinatariosForSelectedCompany()
    End Sub

    Private Sub btnRemove_Click(sender As Object, e As EventArgs) Handles btnRemove.Click
        If lbEmails.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione um email para remover.")
            Exit Sub
        End If
        If MessageBox.Show("Confirmar remoção?", "Confirmar", MessageBoxButtons.YesNo) <> DialogResult.Yes Then Exit Sub
        Dim email = lbEmails.SelectedItem.ToString()
        Dim item = TryCast(cbEmpresas.SelectedItem, EmpresaItem)
        ' Remove de local config se existir; não tenta deletar no banco (somente select permitido)
        Dim locaisCfg = ConfiguracaoService.Carregar()
        If locaisCfg.DestinatariosLocais Is Nothing Then locaisCfg.DestinatariosLocais = New List(Of DestinatarioLocal)()
        locaisCfg.DestinatariosLocais.RemoveAll(Function(d) d.CodigoEmpresa = item.Codigo AndAlso d.Email = email)
        ConfiguracaoService.Salvar(locaisCfg)
        LoadDestinatariosForSelectedCompany()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class