''' <summary>
''' Tela modal "Bancos de Dados" (aba Configurações → Conexão): gerencia a
''' lista de bancos Postgres cadastrados (<see cref="Configuracoes.Conexoes"/>).
''' Cada linha é editada em <see cref="FrmServidor"/>; toda alteração (adicionar/
''' editar/remover) é persistida imediatamente via <see cref="ConfiguracaoService.Salvar"/>,
''' mesmo padrão que as demais telas de configuração já usam.
''' </summary>
Public Class FrmBancos

    Private cfg As Configuracoes

    Private Sub FrmBancos_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cfg = ConfiguracaoService.Carregar()
        AtualizarGrade()
    End Sub

    Private Sub AtualizarGrade()
        dgvBancos.DataSource = Nothing
        dgvBancos.DataSource = cfg.Conexoes
    End Sub

    Private Function ConexaoSelecionada() As ConexaoBanco
        If dgvBancos.CurrentRow Is Nothing Then Return Nothing
        Return TryCast(dgvBancos.CurrentRow.DataBoundItem, ConexaoBanco)
    End Function

    Private Sub btnAdicionar_Click(sender As Object, e As EventArgs) Handles btnAdicionar.Click
        Dim frm As New FrmServidor()

        If frm.ShowDialog() = DialogResult.OK Then
            cfg.Conexoes.Add(frm.Resultado)
            ConfiguracaoService.Salvar(cfg)
            AtualizarGrade()
            LogService.RegistrarAtividade($"Banco adicionado: ""{frm.Resultado.Nome}"" ({frm.Resultado.Servidor}/{frm.Resultado.Banco})")
        End If
    End Sub

    Private Sub btnEditar_Click(sender As Object, e As EventArgs) Handles btnEditar.Click
        Dim selecionada = ConexaoSelecionada()
        If selecionada Is Nothing Then
            MessageBox.Show("Selecione um banco para editar.", "Bancos de Dados", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim indice = cfg.Conexoes.IndexOf(selecionada)
        Dim frm As New FrmServidor(selecionada)

        If frm.ShowDialog() = DialogResult.OK Then
            cfg.Conexoes(indice) = frm.Resultado
            ConfiguracaoService.Salvar(cfg)
            AtualizarGrade()
            LogService.RegistrarAtividade($"Banco editado: ""{frm.Resultado.Nome}"" ({frm.Resultado.Servidor}/{frm.Resultado.Banco})")
        End If
    End Sub

    Private Sub btnRemover_Click(sender As Object, e As EventArgs) Handles btnRemover.Click
        Dim selecionada = ConexaoSelecionada()
        If selecionada Is Nothing Then
            MessageBox.Show("Selecione um banco para remover.", "Bancos de Dados", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim resposta = MessageBox.Show(
            $"Remover o banco ""{selecionada.Nome}""?",
            "Bancos de Dados",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

        If resposta <> DialogResult.Yes Then Return

        cfg.Conexoes.Remove(selecionada)
        ConfiguracaoService.Salvar(cfg)
        AtualizarGrade()
        LogService.RegistrarAtividade($"Banco removido: ""{selecionada.Nome}"" ({selecionada.Servidor}/{selecionada.Banco})")
    End Sub

    Private Sub btnFechar_Click(sender As Object, e As EventArgs) Handles btnFechar.Click
        Me.Close()
    End Sub
End Class
