Imports Npgsql
Public Class FrmPrincipal
    Private carregando As Boolean = True
    ' LOAD

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dtInicio.Value =
New Date(Today.Year, Today.Month, 1)

        dtFim.Value =
New Date(
    Today.Year,
    Today.Month,
    Date.DaysInMonth(Today.Year, Today.Month))
        dgvCupons.AutoGenerateColumns = False

        Dim cfg = ConfiguracaoService.Carregar()

        txtDestinatario.Text = cfg.UltimoDestinatario

        Select Case cfg.UltimoModelo

            Case 55
                rbNFe.Checked = True

            Case 65
                rbNFCe.Checked = True

            Case Else
                rbAmbos.Checked = True

        End Select

        ' Não tenta conectar na inicialização se usuário/senha estiverem vazios
        If String.IsNullOrWhiteSpace(cfg.Usuario) OrElse String.IsNullOrWhiteSpace(cfg.Senha) Then
            lblStatus.Text = "Conexão não configurada. Configure servidor, usuário e senha."
            cboEmpresa.Enabled = False
        Else
            Try
                Using conn = Conexao.Abrir(
                    cfg.Servidor,
                    cfg.Porta,
                    cfg.Banco,
                    cfg.Usuario,
                    cfg.Senha)

                    cboEmpresa.DataSource = EmpresaService.Listar(conn)
                    cboEmpresa.DisplayMember = "Nome"
                    cboEmpresa.ValueMember = "Codigo"

                    If cboEmpresa.Items.Count > 0 Then
                        cboEmpresa.SelectedValue = cfg.UltimaEmpresa
                    End If

                End Using

                cboEmpresa.Enabled = True
            Catch ex As Exception
                lblStatus.Text = $"Erro ao conectar: {ex.Message}"
                cboEmpresa.Enabled = False
            End Try
        End If
        carregando = False

        lblQuantidade.Visible = False
        lblStatus.Visible = False
        lblQtd.Visible = False

        dtInicio.Format = DateTimePickerFormat.Custom
        dtInicio.CustomFormat = "dd/MM/yyyy"

        dtFim.Format = DateTimePickerFormat.Custom
        dtFim.CustomFormat = "dd/MM/yyyy"

        lbServ.Text = cfg.Servidor

        AtualizarConfiguracoes()
        chkTodos.Checked = True
    End Sub

    ' BOTÕES
    Private Sub btnPesquisar_Click(sender As Object, e As EventArgs) Handles btnPesquisar.Click
        Dim cod_empresa As Integer =
    CInt(cboEmpresa.SelectedValue)

        Dim incluirEmitidos = chkEmitidas.Checked
        Dim incluirCancelados = chkCancelados.Checked
        Dim incluirInutilizados = chkInutilizados.Checked

        If chkTodos.Checked Or Not (incluirEmitidos Or incluirCancelados Or incluirInutilizados) Then
            incluirEmitidos = True
            incluirCancelados = True
            incluirInutilizados = True
        End If

        Dim cfg = ConfiguracaoService.Carregar()

        Using conn = Conexao.Abrir(
        cfg.Servidor,
        cfg.Porta,
        cfg.Banco,
        cfg.Usuario,
        cfg.Senha)

            Dim modelo As Integer

            If rbNFCe.Checked Then
                modelo = 65
            ElseIf rbNFe.Checked Then
                modelo = 55
            Else
                modelo = 0
            End If

            dgvCupons.DataSource = ExportadorXML.BuscarCupons(
    conn,
    cod_empresa,
    dtInicio.Value,
    dtFim.Value,
    incluirEmitidos,
    incluirCancelados,
    incluirInutilizados,
    modelo)

        End Using

        Dim dt As DataTable = CType(dgvCupons.DataSource, DataTable)
        lblQtd.Text = $"XMLs encontrados: {dt.Rows.Count}"
        lblQtd.Visible = True

    End Sub

    Private Sub btnExportar_Click(sender As Object, e As EventArgs) Handles btnExportar.Click

        Try

            If txtDestino.Text = "" Then
                MessageBox.Show("Escolha onde salvar o arquivo.")
                Exit Sub
            End If

            Dim codigoEmpresa As Integer =
    CInt(cboEmpresa.SelectedValue)

            lblStatus.Text = "Contando XMLs..."
            Application.DoEvents()

            Dim cfg = ConfiguracaoService.Carregar()

            Using conn = Conexao.Abrir(
            cfg.Servidor,
            cfg.Porta,
            cfg.Banco,
            cfg.Usuario,
            cfg.Senha)
                Dim incluirEmitidos = chkEmitidas.Checked
                Dim incluirCancelados = chkCancelados.Checked
                Dim incluirInutilizados = chkInutilizados.Checked

                If chkTodos.Checked Or Not (incluirEmitidos Or incluirCancelados Or incluirInutilizados) Then
                    incluirEmitidos = True
                    incluirCancelados = True
                    incluirInutilizados = True
                End If

                Dim modelo As Integer

                If rbNFCe.Checked Then
                    modelo = 65
                ElseIf rbNFe.Checked Then
                    modelo = 55
                Else
                    modelo = 0 'Ambos
                End If

                Dim total As Integer = ExportadorXML.ContarXMLs(
                conn,
                codigoEmpresa,
                dtInicio.Value,
                dtFim.Value,
                incluirEmitidos,
                incluirCancelados,
                incluirInutilizados,
                modelo)

                pbExportacao.Minimum = 0
                pbExportacao.Maximum = total
                pbExportacao.Value = 0

                lblQuantidade.Text = $"0 / {total}"
                lblStatus.Text = "Exportando..."
                Application.DoEvents()

                If IO.File.Exists(txtDestino.Text) Then
                    IO.File.Delete(txtDestino.Text)
                End If

                Select Case modelo

                    Case 65

                        ExportadorXML.ExportarNFCe(
         conn,
         codigoEmpresa,
         dtInicio.Value,
         dtFim.Value,
         txtDestino.Text,
         incluirEmitidos,
         incluirCancelados,
         incluirInutilizados)


                    Case 55

                        ExportadorXML.ExportarNFe(
        conn,
        codigoEmpresa,
        dtInicio.Value,
        dtFim.Value,
        txtDestino.Text,
        incluirEmitidos,
        incluirCancelados,
        modelo)

                    Case Else

                        ExportadorXML.ExportarNFCe(
        conn,
        codigoEmpresa,
        dtInicio.Value,
        dtFim.Value,
        txtDestino.Text,
        incluirEmitidos,
        incluirCancelados,
        incluirInutilizados)

                        ExportadorXML.ExportarNFe(
        conn,
        codigoEmpresa,
        dtInicio.Value,
        dtFim.Value,
        txtDestino.Text,
        incluirEmitidos,
        incluirCancelados,
        modelo)

                End Select
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
            "Deseja enviar o arquivo por e-mail?",
            "Exportação",
            MessageBoxButtons.YesNo)

            If resposta = DialogResult.Yes Then

                Dim cfgEmail = ConfiguracaoService.Carregar()
                Dim mensagem As String
                Dim competencia As String = dtInicio.Value.ToString("yyyyMM")

                mensagem =
                $"Prezados,

                Segue em anexo os arquivos XML das notas referentes à competência {competencia}.

                Empresa: {cboEmpresa.Text}

                Em caso de dúvidas, permanecemos à disposição.

                Atenciosamente,
                {cboEmpresa.Text}"

                EmailService.Enviar(
                cfgEmail.ServidorSMTP,
                cfgEmail.PortaSMTP,
                cfgEmail.UsuarioSMTP,
                cfgEmail.SenhaSMTP,
                cfgEmail.EmailRemetente.Trim(),
                txtDestinatario.Text.Trim(),
                "XMLs Exportados - " & cboEmpresa.Text & competencia,
                mensagem,
                txtDestino.Text,
                cfgEmail.UsarSSL)

                MessageBox.Show("E-mail enviado com sucesso!")

            End If

        Catch ex As Exception

            MessageBox.Show(ex.Message)

        End Try

    End Sub

    Private Sub btnDestino_Click(sender As Object, e As EventArgs) Handles btnDestino.Click

        Dim cfg = ConfiguracaoService.Carregar()

        SaveFileDialog1.Filter = "Arquivo ZIP (*.zip)|*.zip"
        SaveFileDialog1.Title = "Salvar Exportação"

        If IO.Directory.Exists(cfg.UltimaPastaExportacao) Then
            SaveFileDialog1.InitialDirectory = cfg.UltimaPastaExportacao
        End If

        SaveFileDialog1.FileName = $"XML_{Date.Today:yyyyMMdd}.zip"

        If SaveFileDialog1.ShowDialog = DialogResult.OK Then

            txtDestino.Text = SaveFileDialog1.FileName

            cfg.UltimaPastaExportacao =
        IO.Path.GetDirectoryName(txtDestino.Text)

            ConfiguracaoService.Salvar(cfg)

        End If

    End Sub

    Private Sub AtualizarConfiguracoes()

        Dim cfg = ConfiguracaoService.Carregar()

        lbServ.Text = cfg.Servidor

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


    '=========================
    ' EVENTOS
    '=========================

    Private Sub cboEmpresa_SelectedValueChanged(sender As Object, e As EventArgs) Handles cboEmpresa.SelectedValueChanged

        If carregando Then Exit Sub

        If cboEmpresa.SelectedValue Is Nothing Then Exit Sub

        If Not Integer.TryParse(cboEmpresa.SelectedValue.ToString(), Nothing) Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()

        cfg.UltimaEmpresa = Convert.ToInt32(cboEmpresa.SelectedValue)

        ConfiguracaoService.Salvar(cfg)

    End Sub

    Private Sub txtDestinatario_Leave(sender As Object, e As EventArgs) Handles txtDestinatario.Leave
        If carregando Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()

        cfg.UltimoDestinatario = txtDestinatario.Text.Trim()

        ConfiguracaoService.Salvar(cfg)

    End Sub

    Private Sub rbNFCe_CheckedChanged(sender As Object, e As EventArgs) Handles rbNFCe.CheckedChanged
        If carregando Then Exit Sub

        If Not rbNFCe.Checked Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()

        cfg.UltimoModelo = 65

        ConfiguracaoService.Salvar(cfg)

    End Sub

    Private Sub rbNFe_CheckedChanged(sender As Object, e As EventArgs) Handles rbNFe.CheckedChanged
        If carregando Then Exit Sub

        If Not rbNFe.Checked Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()

        cfg.UltimoModelo = 55

        ConfiguracaoService.Salvar(cfg)

    End Sub

    Private Sub rbAmbos_CheckedChanged(sender As Object, e As EventArgs) Handles rbAmbos.CheckedChanged
        If carregando Then Exit Sub

        If Not rbAmbos.Checked Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()

        cfg.UltimoModelo = 0

        ConfiguracaoService.Salvar(cfg)

    End Sub

    Private Sub chkTodos_CheckedChanged(sender As Object, e As EventArgs) Handles chkTodos.CheckedChanged
        If chkTodos.Checked Then
            chkEmitidas.Checked = False
            chkCancelados.Checked = False
            chkInutilizados.Checked = False
        End If
    End Sub

    Private Sub chkStatus_CheckedChanged(sender As Object, e As EventArgs) Handles chkEmitidas.CheckedChanged, chkCancelados.CheckedChanged, chkInutilizados.CheckedChanged
        If chkEmitidas.Checked Or chkCancelados.Checked Or chkInutilizados.Checked Then
            chkTodos.Checked = False
        Else
            ' Se nenhum dos filtros específicos estiver marcado, voltar para Todos
            chkTodos.Checked = True
        End If
    End Sub
End Class
