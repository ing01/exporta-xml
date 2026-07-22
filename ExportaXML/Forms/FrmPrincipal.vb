Imports Npgsql
Partial Class FrmPrincipal

    Private Sub btnExportar_Click(sender As Object, e As EventArgs) Handles btnExportar.Click

        Try

            If txtDestino.Text = "" Then
                MessageBox.Show("Escolha onde salvar o arquivo.")
                Exit Sub
            End If

            If String.IsNullOrWhiteSpace(txtCodEmpresa.Text) Then
                MessageBox.Show("Informe o código da empresa.")
                Exit Sub
            End If

            Dim codigoEmpresa As Integer

            If Not Integer.TryParse(txtCodEmpresa.Text, codigoEmpresa) Then
                MessageBox.Show("Código da empresa inválido.")
                Exit Sub
            End If

            If String.IsNullOrWhiteSpace(lblRazao.Text) Then
                MessageBox.Show("Selecione uma empresa válida.")
                Exit Sub
            End If

            lblStatus.Text = "Contando XMLs..."
            Application.DoEvents()

            Dim cfg = ConfiguracaoService.Carregar()

            Using conn = Conexao.Abrir(
            cfg.Servidor,
            cfg.Porta,
            cfg.Banco,
            cfg.Usuario,
            cfg.Senha)

                Dim total As Integer = ExportadorXML.ContarXMLs(
                conn,
                codigoEmpresa,
                dtInicio.Value,
                dtFim.Value)

                pbExportacao.Minimum = 0
                pbExportacao.Maximum = total
                pbExportacao.Value = 0

                lblQuantidade.Text = $"0 / {total}"
                lblStatus.Text = "Exportando..."
                Application.DoEvents()

                ExportadorXML.Exportar(
                conn,
                codigoEmpresa,
                dtInicio.Value,
                dtFim.Value,
                txtDestino.Text)

            End Using

            pbExportacao.Value = pbExportacao.Maximum
            lblQuantidade.Text = $"{pbExportacao.Maximum} / {pbExportacao.Maximum}"
            lblQuantidade.Visible = True
            lblStatus.Text = "Concluído!"
            lblStatus.Visible = True

            Dim resposta = MessageBox.Show(
            "Exportação concluída." &
            vbCrLf &
            vbCrLf &
            "Deseja enviar o ZIP por e-mail?",
            "Exportação",
            MessageBoxButtons.YesNo)

            If resposta = DialogResult.Yes Then

                Dim cfgEmail = ConfiguracaoService.Carregar()

                EmailService.Enviar(
                cfgEmail.ServidorSMTP,
                cfgEmail.PortaSMTP,
                cfgEmail.UsuarioSMTP,
                cfgEmail.SenhaSMTP,
                cfgEmail.EmailRemetente,
                txtDestinatario.Text,
                "XMLs Exportados",
                "Segue em anexo o arquivo ZIP.",
                txtDestino.Text,
                cfgEmail.UsarSSL)

                MessageBox.Show("E-mail enviado com sucesso!")

            End If

        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub

    Private Sub btnDestino_Click(sender As Object, e As EventArgs) Handles btnDestino.Click

        SaveFileDialog1.Filter = "Arquivo ZIP (*.zip)|*.zip"

        SaveFileDialog1.Title = "Salvar Exportação"

        SaveFileDialog1.FileName =
        $"XML_{Date.Today:yyyyMMdd}.zip"

        If SaveFileDialog1.ShowDialog = DialogResult.OK Then

            txtDestino.Text = SaveFileDialog1.FileName

        End If

    End Sub

    Private Sub AtualizarConfiguracoes()

        Dim cfg = ConfiguracaoService.Carregar()

        lbServ.Text = cfg.Servidor
        lbPort.Text = cfg.Porta.ToString()
        lbBan.Text = cfg.Banco
        lbUser.Text = cfg.Usuario

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        lblCNPJ.Visible = False
        lblRazao.Visible = False
        lblQuantidade.Visible = False
        lblStatus.Visible = False

        dtInicio.Format = DateTimePickerFormat.Custom
        dtInicio.CustomFormat = "dd/MM/yyyy"

        dtFim.Format = DateTimePickerFormat.Custom
        dtFim.CustomFormat = "dd/MM/yyyy"

        Dim cfg As Configuracoes = ConfiguracaoService.Carregar()

        lbServ.Text = cfg.Servidor
        lbPort.Text = cfg.Porta.ToString()
        lbBan.Text = cfg.Banco
        lbUser.Text = cfg.Usuario

        lblRemetente.Text = cfg.UsuarioSMTP

        AtualizarConfiguracoes()
    End Sub

    Private Sub btnBuscarEmpresa_Click(sender As Object, e As EventArgs) Handles btnBuscarEmpresa.Click

        If Integer.Parse(txtCodEmpresa.Text) = 0 Then

            lblCNPJ.Text = "TODAS AS EMPRESAS"
            lblRazao.Text = "SERÃO EXPORTADOS TODOS OS XMLS"

            lblCNPJ.Visible = True
            lblRazao.Visible = True

            Exit Sub

        End If

        If String.IsNullOrWhiteSpace(txtCodEmpresa.Text) Then
            MessageBox.Show("Informe o código da empresa.")
            txtCodEmpresa.Focus()
            Exit Sub
        End If



        Dim cfg = ConfiguracaoService.Carregar()

        Using conn = Conexao.Abrir(
        cfg.Servidor,
        cfg.Porta,
        cfg.Banco,
        cfg.Usuario,
        cfg.Senha)

            Dim empresa = EmpresaService.Buscar(
                    conn,
                    Integer.Parse(txtCodEmpresa.Text))

            If empresa Is Nothing Then

                MessageBox.Show("Empresa não encontrada.")

                lblCNPJ.Text = ""
                lblRazao.Text = ""

            Else

                lblCNPJ.Text = empresa.CNPJ
                lblCNPJ.Visible = True
                lblRazao.Text = empresa.Razao
                lblRazao.Visible = True

            End If

        End Using

    End Sub

    Private Sub btnConfigurarServidor_Click(sender As Object, e As EventArgs) Handles btnConfigurarServidor.Click

        Dim frm As New FrmServidor

        frm.ShowDialog()

        AtualizarConfiguracoes()

    End Sub

    Private Sub btnConfigurarEmail_Click(sender As Object, e As EventArgs) Handles btnConfigurarEmail.Click

        Dim frm As New FrmEmail

        frm.ShowDialog()

        AtualizarConfiguracoes()

    End Sub

End Class
