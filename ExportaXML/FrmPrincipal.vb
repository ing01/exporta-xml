Imports Npgsql
Partial Class FrmPrincipal
    Private Sub btnTestar_Click(sender As Object, e As EventArgs) Handles btnTestar.Click

        Try

            Dim conn = Conexao.Abrir(
            txtServidor.Text,
            Integer.Parse(txtPorta.Text),
            txtBanco.Text,
            txtUsuario.Text,
            txtSenha.Text)

            MessageBox.Show("Conectado com sucesso!")

            conn.Close()

        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub

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

            Using conn = Conexao.Abrir(
            txtServidor.Text,
            Integer.Parse(txtPorta.Text),
            txtBanco.Text,
            txtUsuario.Text,
            txtSenha.Text)

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

            MessageBox.Show("Exportação concluída!")

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

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        lblCNPJ.Visible = False
        lblRazao.Visible = False
        lblQuantidade.Visible = False
        lblStatus.Visible = False

        dtInicio.Format = DateTimePickerFormat.Custom
        dtInicio.CustomFormat = "dd/MM/yyyy"

        dtFim.Format = DateTimePickerFormat.Custom
        dtFim.CustomFormat = "dd/MM/yyyy"

        Dim cfg As Configuracao = ConfiguracaoService.Carregar()

        txtServidor.Text = cfg.Servidor

        If cfg.Porta = 0 Then
            txtPorta.Text = "5434"
        Else
            txtPorta.Text = cfg.Porta.ToString()
        End If

        txtBanco.Text = cfg.Banco
        txtUsuario.Text = cfg.Usuario
        txtSenha.Text = cfg.Senha

    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        Dim cfg As New Configuracao

        cfg.Servidor = txtServidor.Text
        cfg.Banco = txtBanco.Text
        cfg.Usuario = txtUsuario.Text
        cfg.Senha = txtSenha.Text

        Integer.TryParse(txtPorta.Text, cfg.Porta)

        ConfiguracaoService.Salvar(cfg)

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs)

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



        Using conn = Conexao.Abrir(
        txtServidor.Text,
        Integer.Parse(txtPorta.Text),
        txtBanco.Text,
        txtUsuario.Text,
        txtSenha.Text)

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
End Class
