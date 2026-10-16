''' <summary>
''' Tela "Configurar E-mail" — lista TODAS as empresas (+ "Global", a
''' configuração padrão) com um resumo de como cada uma está configurada hoje,
''' pra deixar claro que a configuração é POR EMPRESA. Selecione uma linha e
''' clique em "Configurar" (ou dê duplo clique) pra abrir
''' <see cref="FrmConfiguracaoEmailEmpresa"/> e editar só aquela.
''' </summary>
''' <remarks>
''' Montar a grade abre uma conexão POR BANCO cadastrado — se algum estiver
''' lento ou inacessível, isso pode demorar (até o timeout padrão do Npgsql
''' por banco com problema). Por isso a consulta roda em <see cref="Task.Run"/>,
''' disparada só depois que a janela já apareceu (evento Shown, não Load) —
''' mesmo padrão de <see cref="FrmPrincipal.FrmPrincipal_Shown"/> — pra nunca
''' travar a tela enquanto conecta.
''' </remarks>
Public Class FrmEmail

    ''' <summary>Uma linha da grade: resumo de como UMA empresa está configurada hoje.</summary>
    Private Class LinhaResumo
        Public Property Item As EmpresaItem
        Public Property NomeExibicao As String
        Public Property Modo As String
        Public Property Destinatarios As String
    End Class

    Private Sub FrmEmail_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dgvEmpresasEmail.Enabled = False
        btnConfigurar.Enabled = False
        lblExplicacao.Text = "Carregando empresas..."
        lblExplicacao.MaximumSize = New Size(Math.Max(300, ClientSize.Width - 24), 0)
    End Sub

    Private Async Sub FrmEmail_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Await AtualizarGradeAsync()
    End Sub

    Private Async Function AtualizarGradeAsync() As Task
        dgvEmpresasEmail.Enabled = False
        btnConfigurar.Enabled = False
        lblExplicacao.Text = "Carregando empresas..."
        lblExplicacao.MaximumSize = New Size(Math.Max(300, ClientSize.Width - 24), 0)

        Dim cfg = ConfiguracaoService.Carregar()
        Dim resultado = Await Task.Run(Function() MontarLinhas(cfg))

        dgvEmpresasEmail.DataSource = Nothing
        dgvEmpresasEmail.DataSource = resultado.Linhas

        dgvEmpresasEmail.Enabled = True
        btnConfigurar.Enabled = True

        If resultado.Aviso <> "" Then
            lblExplicacao.Text = resultado.Aviso
        ElseIf resultado.Erros.Count > 0 Then
            lblExplicacao.Text = $"Erro ao conectar em: {String.Join("; ", resultado.Erros)}"
        Else
            lblExplicacao.Text = "Cada empresa tem sua própria configuração. Selecione uma e clique em Configurar."
        End If
    End Function

    ''' <summary>
    ''' Só I/O (abrir conexão, consultar, montar as linhas) — de propósito não
    ''' toca em nenhum controle da tela, pra poder rodar dentro de
    ''' <see cref="Task.Run"/> com segurança.
    ''' </summary>
    Private Function MontarLinhas(cfg As Configuracoes) As (Linhas As List(Of LinhaResumo), Erros As List(Of String), Aviso As String)
        Dim linhas As New List(Of LinhaResumo)
        Dim erros As New List(Of String)
        Dim aviso As String = ""

        ' Fase 1: lista as empresas de TODOS os bancos (empresa = banco + código).
        Dim todas As New List(Of EmpresaItem)
        For Each banco As ConexaoBanco In cfg.Conexoes
            Try
                Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                    For Each emp In EmpresaService.Listar(conn).Where(Function(e) e.Codigo <> 0)
                        emp.Conexao = banco
                        emp.NomeExibicao = If(cfg.Conexoes.Count > 1, $"{emp.Nome} ({banco.Nome})", emp.Nome)
                        todas.Add(emp)
                    Next
                End Using
            Catch ex As Exception
                LogService.RegistrarAtividade($"Erro ao carregar empresas do banco {banco.Nome}: {ex}")
                erros.Add($"{banco.Nome} ({ErroAmigavel.Traduzir(ex)})")
            End Try
        Next

        ' Cadastros antigos (só com o código da empresa) são associados ao banco quando o
        ' código existe em um único banco. Só roda com TODOS os bancos acessíveis: com um
        ' banco fora do ar a lista está incompleta e a associação poderia ficar errada.
        If erros.Count = 0 AndAlso EscopoEmpresa.TemLegadosAmbiguos(cfg) Then
            Dim r = ConfiguracaoService.MigrarEscopoLegado(todas)
            If r.Associados > 0 Then
                LogService.RegistrarAtividade($"Cadastros de e-mail/destinatário associados ao banco da empresa: {r.Associados}")
            End If
            If r.Ambiguos > 0 Then
                aviso = $"Atenção: {r.Ambiguos} cadastro(s) antigo(s) de destinatário/e-mail não indicavam o banco e o mesmo código de empresa existe em mais de um banco; por segurança estão DESATIVADOS. Recadastre-os em cada empresa."
                LogService.RegistrarAtividade(aviso)
            End If
            cfg = ConfiguracaoService.Carregar()
        End If

        ' Fase 2: monta as linhas.
        Dim global_ As New EmpresaItem With {.Codigo = 0, .Nome = "Global", .NomeExibicao = "Global (padrão)"}
        linhas.Add(New LinhaResumo With {
            .Item = global_,
            .NomeExibicao = global_.NomeExibicao,
            .Modo = If(cfg.OrigemConfiguracao = "Banco", "Sincronizado do banco", "Manual"),
            .Destinatarios = ContarDestinatarios(0, Nothing, cfg)
        })

        For Each emp In todas
            Dim override = EscopoEmpresa.ConfigEmailDaEmpresa(cfg, emp.Codigo, emp.Conexao.Nome)
            Dim modo As String =
                If(override Is Nothing OrElse override.Origem = "Padrao", "Padrão (Global)",
                If(override.Origem = "Banco", "Sincronizado do banco", "Manual"))

            linhas.Add(New LinhaResumo With {
                .Item = emp,
                .NomeExibicao = emp.NomeExibicao,
                .Modo = modo,
                .Destinatarios = ContarDestinatarios(emp.Codigo, emp.Conexao.Nome, cfg)
            })
        Next

        Return (linhas, erros, aviso)
    End Function

    ''' <summary>Só conta os destinatários locais (config.json) — os do banco exigiriam abrir uma conexão por linha, custo desnecessário só pra listar.</summary>
    Private Function ContarDestinatarios(codigoEmpresa As Integer, nomeBanco As String, cfg As Configuracoes) As String
        Dim qtd = EscopoEmpresa.DestinatariosDaEmpresa(cfg, codigoEmpresa, nomeBanco).Count
        If qtd = 0 Then Return "(nenhum)"
        Return $"{qtd} cadastrado(s)"
    End Function

    Private Function ItemSelecionado() As EmpresaItem
        If dgvEmpresasEmail.CurrentRow Is Nothing Then Return Nothing
        Dim linha = TryCast(dgvEmpresasEmail.CurrentRow.DataBoundItem, LinhaResumo)
        Return linha?.Item
    End Function

    Private Async Sub AbrirConfiguracao()
        Dim item = ItemSelecionado()
        If item Is Nothing Then
            MessageBox.Show("Selecione uma empresa para configurar.", "Configurar E-mail", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim frm As New FrmConfiguracaoEmailEmpresa(item)
        frm.ShowDialog(Me)

        If frm.Salvo Then
            Await AtualizarGradeAsync()
        End If
    End Sub

    Private Sub btnConfigurar_Click(sender As Object, e As EventArgs) Handles btnConfigurar.Click
        AbrirConfiguracao()
    End Sub

    Private Sub dgvEmpresasEmail_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvEmpresasEmail.CellDoubleClick
        If e.RowIndex >= 0 Then AbrirConfiguracao()
    End Sub

    Private Sub btnFechar_Click(sender As Object, e As EventArgs) Handles btnFechar.Click
        Me.Close()
    End Sub

End Class
