<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmPrincipal
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmPrincipal))
        tabPrincipal = New TabControl()
        tabExportar = New TabPage()
        Label1 = New Label()
        grpFiltros = New GroupBox()
        GroupBox4 = New GroupBox()
        lblNumDoc = New Label()
        txbInicio = New TextBox()
        lblA2 = New Label()
        txbFim = New TextBox()
        lblSerieFiltro = New Label()
        txbSerie = New TextBox()
        GroupBox3 = New GroupBox()
        lblStatusFiltro = New Label()
        chkTodos = New CheckBox()
        chkEmitidas = New CheckBox()
        chkCancelados = New CheckBox()
        chkInutilizados = New CheckBox()
        GroupBox2 = New GroupBox()
        lblModelo = New Label()
        rbNFCe = New RadioButton()
        rbNFe = New RadioButton()
        rbAmbos = New RadioButton()
        GroupBox1 = New GroupBox()
        lblDirecao = New Label()
        rbSaida = New RadioButton()
        rbEntrada = New RadioButton()
        lblEmpresaFiltro = New Label()
        cboEmpresa = New ComboBox()
        lblPeriodo = New Label()
        dtInicio = New DateTimePicker()
        lblA1 = New Label()
        dtFim = New DateTimePicker()
        lblFornecedor = New Label()
        cboFornecedor = New ComboBox()
        grpAcao = New GroupBox()
        lblDestino = New Label()
        txtDestino = New TextBox()
        btnDestino = New Button()
        btnPesquisar = New Button()
        lblQtd = New Label()
        lblStatus = New Label()
        lblQuantidade = New Label()
        pbExportacao = New ProgressBar()
        btnExportar = New Button()
        dgvCupons = New DataGridView()
        Modelo = New DataGridViewTextBoxColumn()
        Documento = New DataGridViewTextBoxColumn()
        Codigo = New DataGridViewTextBoxColumn()
        Fornecedor = New DataGridViewTextBoxColumn()
        Serie = New DataGridViewTextBoxColumn()
        Chave = New DataGridViewTextBoxColumn()
        Status = New DataGridViewTextBoxColumn()
        Data = New DataGridViewTextBoxColumn()
        tabConfiguracoes = New TabPage()
        grpConexao = New GroupBox()
        lbServidor = New Label()
        lbServ = New Label()
        btnConfigurarServidor = New Button()
        grpEmail = New GroupBox()
        Label2 = New Label()
        btnConfigurarEmail = New Button()
        grpAgendamento = New GroupBox()
        chkAgendamentoAtivo = New CheckBox()
        lblHoraAgendamento = New Label()
        dtpHoraAgendamento = New DateTimePicker()
        chkIniciarComWindows = New CheckBox()
        chkManterSempreAtivo = New CheckBox()
        chkDiaFixo = New CheckBox()
        lblDiaPersonalizado = New Label()
        nudDiaAgendamento = New NumericUpDown()
        lblEmailAlerta = New Label()
        txtEmailAlertaFalha = New TextBox()
        btnTestarAgendamento = New Button()
        btnRestaurarVersao = New Button()
        Label10 = New Label()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        lblVersao = New Label()
        btnVerificarAtualizacao = New Button()
        btnAjuda = New Button()
        tmrAtualizacao = New Timer(components)
        tmrAgendamento = New Timer(components)
        notifyIcon1 = New NotifyIcon(components)
        contextMenuTray = New ContextMenuStrip(components)
        AbrirToolStripMenuItem = New ToolStripMenuItem()
        SairToolStripMenuItem = New ToolStripMenuItem()
        tabPrincipal.SuspendLayout()
        tabExportar.SuspendLayout()
        grpFiltros.SuspendLayout()
        GroupBox4.SuspendLayout()
        GroupBox3.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox1.SuspendLayout()
        grpAcao.SuspendLayout()
        CType(dgvCupons, ComponentModel.ISupportInitialize).BeginInit()
        tabConfiguracoes.SuspendLayout()
        grpConexao.SuspendLayout()
        grpEmail.SuspendLayout()
        grpAgendamento.SuspendLayout()
        CType(nudDiaAgendamento, ComponentModel.ISupportInitialize).BeginInit()
        contextMenuTray.SuspendLayout()
        SuspendLayout()
        ' 
        ' tabPrincipal
        ' 
        tabPrincipal.Controls.Add(tabExportar)
        tabPrincipal.Controls.Add(tabConfiguracoes)
        tabPrincipal.Location = New Point(8, 8)
        tabPrincipal.Name = "tabPrincipal"
        tabPrincipal.SelectedIndex = 0
        tabPrincipal.Size = New Size(624, 695)
        tabPrincipal.TabIndex = 0
        ' 
        ' tabExportar
        ' 
        tabExportar.Controls.Add(Label1)
        tabExportar.Controls.Add(grpFiltros)
        tabExportar.Controls.Add(grpAcao)
        tabExportar.Controls.Add(lblStatus)
        tabExportar.Controls.Add(lblQuantidade)
        tabExportar.Controls.Add(pbExportacao)
        tabExportar.Controls.Add(btnExportar)
        tabExportar.Controls.Add(dgvCupons)
        tabExportar.Location = New Point(4, 24)
        tabExportar.Name = "tabExportar"
        tabExportar.Padding = New Padding(3)
        tabExportar.Size = New Size(616, 667)
        tabExportar.TabIndex = 0
        tabExportar.Text = "Exportar / Pesquisar"
        tabExportar.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Arial", 11.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(101, 367)
        Label1.Name = "Label1"
        Label1.Size = New Size(86, 18)
        Label1.TabIndex = 6
        Label1.Text = "Progresso:"
        ' 
        ' grpFiltros
        ' 
        grpFiltros.Controls.Add(GroupBox4)
        grpFiltros.Controls.Add(GroupBox3)
        grpFiltros.Controls.Add(GroupBox2)
        grpFiltros.Controls.Add(GroupBox1)
        grpFiltros.Controls.Add(lblEmpresaFiltro)
        grpFiltros.Controls.Add(cboEmpresa)
        grpFiltros.Controls.Add(lblPeriodo)
        grpFiltros.Controls.Add(dtInicio)
        grpFiltros.Controls.Add(lblA1)
        grpFiltros.Controls.Add(dtFim)
        grpFiltros.Controls.Add(lblFornecedor)
        grpFiltros.Controls.Add(cboFornecedor)
        grpFiltros.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        grpFiltros.Location = New Point(8, 8)
        grpFiltros.Name = "grpFiltros"
        grpFiltros.Size = New Size(600, 207)
        grpFiltros.TabIndex = 0
        grpFiltros.TabStop = False
        grpFiltros.Text = "Filtros"
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(lblNumDoc)
        GroupBox4.Controls.Add(txbInicio)
        GroupBox4.Controls.Add(lblA2)
        GroupBox4.Controls.Add(txbFim)
        GroupBox4.Controls.Add(lblSerieFiltro)
        GroupBox4.Controls.Add(txbSerie)
        GroupBox4.Location = New Point(4, 161)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(299, 40)
        GroupBox4.TabIndex = 29
        GroupBox4.TabStop = False
        ' 
        ' lblNumDoc
        ' 
        lblNumDoc.AutoSize = True
        lblNumDoc.Location = New Point(4, 14)
        lblNumDoc.Name = "lblNumDoc"
        lblNumDoc.Size = New Size(51, 15)
        lblNumDoc.TabIndex = 15
        lblNumDoc.Text = "Nº Doc.:"
        ' 
        ' txbInicio
        ' 
        txbInicio.Location = New Point(60, 11)
        txbInicio.Name = "txbInicio"
        txbInicio.Size = New Size(48, 21)
        txbInicio.TabIndex = 16
        ' 
        ' lblA2
        ' 
        lblA2.AutoSize = True
        lblA2.Location = New Point(113, 14)
        lblA2.Name = "lblA2"
        lblA2.Size = New Size(14, 15)
        lblA2.TabIndex = 17
        lblA2.Text = "a"
        ' 
        ' txbFim
        ' 
        txbFim.Location = New Point(132, 11)
        txbFim.Name = "txbFim"
        txbFim.Size = New Size(48, 21)
        txbFim.TabIndex = 18
        ' 
        ' lblSerieFiltro
        ' 
        lblSerieFiltro.AutoSize = True
        lblSerieFiltro.Location = New Point(195, 14)
        lblSerieFiltro.Name = "lblSerieFiltro"
        lblSerieFiltro.Size = New Size(39, 15)
        lblSerieFiltro.TabIndex = 19
        lblSerieFiltro.Text = "Série:"
        ' 
        ' txbSerie
        ' 
        txbSerie.Location = New Point(239, 11)
        txbSerie.Name = "txbSerie"
        txbSerie.Size = New Size(45, 21)
        txbSerie.TabIndex = 20
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(lblStatusFiltro)
        GroupBox3.Controls.Add(chkTodos)
        GroupBox3.Controls.Add(chkEmitidas)
        GroupBox3.Controls.Add(chkCancelados)
        GroupBox3.Controls.Add(chkInutilizados)
        GroupBox3.Location = New Point(203, 95)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(386, 58)
        GroupBox3.TabIndex = 28
        GroupBox3.TabStop = False
        ' 
        ' lblStatusFiltro
        ' 
        lblStatusFiltro.AutoSize = True
        lblStatusFiltro.Location = New Point(7, 24)
        lblStatusFiltro.Name = "lblStatusFiltro"
        lblStatusFiltro.Size = New Size(45, 15)
        lblStatusFiltro.TabIndex = 21
        lblStatusFiltro.Text = "Status:"
        ' 
        ' chkTodos
        ' 
        chkTodos.AutoSize = True
        chkTodos.Location = New Point(56, 22)
        chkTodos.Name = "chkTodos"
        chkTodos.Size = New Size(60, 19)
        chkTodos.TabIndex = 22
        chkTodos.Text = "Todos"
        chkTodos.UseVisualStyleBackColor = True
        ' 
        ' chkEmitidas
        ' 
        chkEmitidas.AutoSize = True
        chkEmitidas.Location = New Point(120, 22)
        chkEmitidas.Name = "chkEmitidas"
        chkEmitidas.Size = New Size(75, 19)
        chkEmitidas.TabIndex = 23
        chkEmitidas.Text = "Emitidos"
        chkEmitidas.UseVisualStyleBackColor = True
        ' 
        ' chkCancelados
        ' 
        chkCancelados.AutoSize = True
        chkCancelados.Location = New Point(199, 22)
        chkCancelados.Name = "chkCancelados"
        chkCancelados.Size = New Size(93, 19)
        chkCancelados.TabIndex = 24
        chkCancelados.Text = "Cancelados"
        chkCancelados.UseVisualStyleBackColor = True
        ' 
        ' chkInutilizados
        ' 
        chkInutilizados.AutoSize = True
        chkInutilizados.Location = New Point(296, 22)
        chkInutilizados.Name = "chkInutilizados"
        chkInutilizados.Size = New Size(88, 19)
        chkInutilizados.TabIndex = 25
        chkInutilizados.Text = "Inutilizados"
        chkInutilizados.UseVisualStyleBackColor = True
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(lblModelo)
        GroupBox2.Controls.Add(rbNFCe)
        GroupBox2.Controls.Add(rbNFe)
        GroupBox2.Controls.Add(rbAmbos)
        GroupBox2.Location = New Point(4, 95)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(200, 58)
        GroupBox2.TabIndex = 27
        GroupBox2.TabStop = False
        ' 
        ' lblModelo
        ' 
        lblModelo.AutoSize = True
        lblModelo.Location = New Point(7, 12)
        lblModelo.Name = "lblModelo"
        lblModelo.Size = New Size(50, 15)
        lblModelo.TabIndex = 11
        lblModelo.Text = "Modelo:"
        ' 
        ' rbNFCe
        ' 
        rbNFCe.AutoSize = True
        rbNFCe.Location = New Point(10, 30)
        rbNFCe.Name = "rbNFCe"
        rbNFCe.Size = New Size(61, 19)
        rbNFCe.TabIndex = 12
        rbNFCe.TabStop = True
        rbNFCe.Text = "NFC-e"
        rbNFCe.UseVisualStyleBackColor = True
        ' 
        ' rbNFe
        ' 
        rbNFe.AutoSize = True
        rbNFe.Location = New Point(77, 30)
        rbNFe.Name = "rbNFe"
        rbNFe.Size = New Size(52, 19)
        rbNFe.TabIndex = 13
        rbNFe.TabStop = True
        rbNFe.Text = "NF-e"
        rbNFe.UseVisualStyleBackColor = True
        ' 
        ' rbAmbos
        ' 
        rbAmbos.AutoSize = True
        rbAmbos.Location = New Point(129, 30)
        rbAmbos.Name = "rbAmbos"
        rbAmbos.Size = New Size(64, 19)
        rbAmbos.TabIndex = 14
        rbAmbos.TabStop = True
        rbAmbos.Text = "Ambos"
        rbAmbos.UseVisualStyleBackColor = True
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(lblDirecao)
        GroupBox1.Controls.Add(rbSaida)
        GroupBox1.Controls.Add(rbEntrada)
        GroupBox1.Location = New Point(4, 46)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(200, 38)
        GroupBox1.TabIndex = 26
        GroupBox1.TabStop = False
        ' 
        ' lblDirecao
        ' 
        lblDirecao.AutoSize = True
        lblDirecao.Location = New Point(2, 12)
        lblDirecao.Name = "lblDirecao"
        lblDirecao.Size = New Size(53, 15)
        lblDirecao.TabIndex = 6
        lblDirecao.Text = "Direção:"
        ' 
        ' rbSaida
        ' 
        rbSaida.AutoSize = True
        rbSaida.Location = New Point(61, 10)
        rbSaida.Name = "rbSaida"
        rbSaida.Size = New Size(57, 19)
        rbSaida.TabIndex = 7
        rbSaida.TabStop = True
        rbSaida.Text = "Saída"
        rbSaida.UseVisualStyleBackColor = True
        ' 
        ' rbEntrada
        ' 
        rbEntrada.AutoSize = True
        rbEntrada.Location = New Point(124, 10)
        rbEntrada.Name = "rbEntrada"
        rbEntrada.Size = New Size(68, 19)
        rbEntrada.TabIndex = 8
        rbEntrada.Text = "Entrada"
        rbEntrada.UseVisualStyleBackColor = True
        ' 
        ' lblEmpresaFiltro
        ' 
        lblEmpresaFiltro.AutoSize = True
        lblEmpresaFiltro.Location = New Point(5, 22)
        lblEmpresaFiltro.Name = "lblEmpresaFiltro"
        lblEmpresaFiltro.Size = New Size(61, 15)
        lblEmpresaFiltro.TabIndex = 0
        lblEmpresaFiltro.Text = "Empresa:"
        ' 
        ' cboEmpresa
        ' 
        cboEmpresa.FormattingEnabled = True
        cboEmpresa.Location = New Point(68, 17)
        cboEmpresa.Name = "cboEmpresa"
        cboEmpresa.Size = New Size(215, 23)
        cboEmpresa.TabIndex = 1
        ' 
        ' lblPeriodo
        ' 
        lblPeriodo.AutoSize = True
        lblPeriodo.Location = New Point(297, 22)
        lblPeriodo.Name = "lblPeriodo"
        lblPeriodo.Size = New Size(53, 15)
        lblPeriodo.TabIndex = 2
        lblPeriodo.Text = "Período:"
        ' 
        ' dtInicio
        ' 
        dtInicio.CustomFormat = "dd/MM/yyyy"
        dtInicio.Format = DateTimePickerFormat.Custom
        dtInicio.Location = New Point(356, 17)
        dtInicio.Name = "dtInicio"
        dtInicio.Size = New Size(92, 21)
        dtInicio.TabIndex = 3
        ' 
        ' lblA1
        ' 
        lblA1.AutoSize = True
        lblA1.Location = New Point(452, 20)
        lblA1.Name = "lblA1"
        lblA1.Size = New Size(14, 15)
        lblA1.TabIndex = 4
        lblA1.Text = "a"
        ' 
        ' dtFim
        ' 
        dtFim.CustomFormat = "dd/MM/yyyy"
        dtFim.Format = DateTimePickerFormat.Custom
        dtFim.Location = New Point(466, 17)
        dtFim.Name = "dtFim"
        dtFim.Size = New Size(92, 21)
        dtFim.TabIndex = 5
        ' 
        ' lblFornecedor
        ' 
        lblFornecedor.AutoSize = True
        lblFornecedor.Location = New Point(210, 58)
        lblFornecedor.Name = "lblFornecedor"
        lblFornecedor.Size = New Size(73, 15)
        lblFornecedor.TabIndex = 9
        lblFornecedor.Text = "Fornecedor:"
        ' 
        ' cboFornecedor
        ' 
        cboFornecedor.FormattingEnabled = True
        cboFornecedor.Location = New Point(289, 53)
        cboFornecedor.Name = "cboFornecedor"
        cboFornecedor.Size = New Size(300, 23)
        cboFornecedor.TabIndex = 10
        ' 
        ' grpAcao
        ' 
        grpAcao.Controls.Add(lblDestino)
        grpAcao.Controls.Add(txtDestino)
        grpAcao.Controls.Add(btnDestino)
        grpAcao.Controls.Add(btnPesquisar)
        grpAcao.Controls.Add(lblQtd)
        grpAcao.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        grpAcao.Location = New Point(8, 213)
        grpAcao.Name = "grpAcao"
        grpAcao.Size = New Size(600, 96)
        grpAcao.TabIndex = 1
        grpAcao.TabStop = False
        ' 
        ' lblDestino
        ' 
        lblDestino.AutoSize = True
        lblDestino.Location = New Point(8, 17)
        lblDestino.Name = "lblDestino"
        lblDestino.Size = New Size(53, 15)
        lblDestino.TabIndex = 0
        lblDestino.Text = "Destino:"
        ' 
        ' txtDestino
        ' 
        txtDestino.Location = New Point(68, 14)
        txtDestino.Name = "txtDestino"
        txtDestino.Size = New Size(360, 21)
        txtDestino.TabIndex = 1
        ' 
        ' btnDestino
        ' 
        btnDestino.BackColor = Color.Gold
        btnDestino.FlatStyle = FlatStyle.Flat
        btnDestino.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnDestino.ForeColor = Color.Black
        btnDestino.Location = New Point(438, 13)
        btnDestino.Name = "btnDestino"
        btnDestino.Size = New Size(150, 24)
        btnDestino.TabIndex = 2
        btnDestino.Text = "Selecionar Pasta"
        btnDestino.UseVisualStyleBackColor = False
        ' 
        ' btnPesquisar
        ' 
        btnPesquisar.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnPesquisar.Location = New Point(11, 41)
        btnPesquisar.Name = "btnPesquisar"
        btnPesquisar.Size = New Size(180, 32)
        btnPesquisar.TabIndex = 3
        btnPesquisar.Text = "Pesquisar"
        btnPesquisar.UseVisualStyleBackColor = True
        ' 
        ' lblQtd
        ' 
        lblQtd.AutoSize = True
        lblQtd.Font = New Font("Arial", 11.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblQtd.Location = New Point(197, 48)
        lblQtd.Name = "lblQtd"
        lblQtd.Size = New Size(55, 18)
        lblQtd.TabIndex = 4
        lblQtd.Text = "Quant."
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Font = New Font("Arial", 11.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblStatus.Location = New Point(193, 388)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(53, 18)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Status"
        ' 
        ' lblQuantidade
        ' 
        lblQuantidade.AutoSize = True
        lblQuantidade.Font = New Font("Arial", 11.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblQuantidade.Location = New Point(444, 367)
        lblQuantidade.Name = "lblQuantidade"
        lblQuantidade.Size = New Size(55, 18)
        lblQuantidade.TabIndex = 3
        lblQuantidade.Text = "Quant."
        ' 
        ' pbExportacao
        ' 
        pbExportacao.Location = New Point(193, 359)
        pbExportacao.Name = "pbExportacao"
        pbExportacao.Size = New Size(245, 26)
        pbExportacao.TabIndex = 5
        ' 
        ' btnExportar
        ' 
        btnExportar.BackColor = Color.ForestGreen
        btnExportar.FlatStyle = FlatStyle.Flat
        btnExportar.Font = New Font("Arial", 13.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnExportar.ForeColor = Color.Honeydew
        btnExportar.Location = New Point(193, 315)
        btnExportar.Name = "btnExportar"
        btnExportar.Size = New Size(245, 38)
        btnExportar.TabIndex = 4
        btnExportar.Text = "Exportar"
        btnExportar.UseVisualStyleBackColor = False
        ' 
        ' dgvCupons
        ' 
        dgvCupons.AllowUserToAddRows = False
        dgvCupons.AllowUserToDeleteRows = False
        dgvCupons.BackgroundColor = SystemColors.ControlLight
        dgvCupons.BorderStyle = BorderStyle.Fixed3D
        dgvCupons.CellBorderStyle = DataGridViewCellBorderStyle.Raised
        dgvCupons.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvCupons.Columns.AddRange(New DataGridViewColumn() {Modelo, Documento, Codigo, Fornecedor, Serie, Chave, Status, Data})
        dgvCupons.GridColor = SystemColors.ActiveCaptionText
        dgvCupons.Location = New Point(8, 412)
        dgvCupons.Name = "dgvCupons"
        dgvCupons.ReadOnly = True
        dgvCupons.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvCupons.Size = New Size(600, 249)
        dgvCupons.TabIndex = 5
        ' 
        ' Modelo
        ' 
        Modelo.DataPropertyName = "Modelo"
        Modelo.HeaderText = "Modelo"
        Modelo.Name = "Modelo"
        Modelo.ReadOnly = True
        ' 
        ' Documento
        ' 
        Documento.DataPropertyName = "Documento"
        Documento.HeaderText = "Documento"
        Documento.Name = "Documento"
        Documento.ReadOnly = True
        ' 
        ' Codigo
        ' 
        Codigo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Codigo.DataPropertyName = "Empresa"
        Codigo.HeaderText = "Empresa"
        Codigo.Name = "Codigo"
        Codigo.ReadOnly = True
        ' 
        ' Fornecedor
        ' 
        Fornecedor.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Fornecedor.DataPropertyName = "Fornecedor"
        Fornecedor.HeaderText = "Fornecedor"
        Fornecedor.Name = "Fornecedor"
        Fornecedor.ReadOnly = True
        Fornecedor.Visible = False
        ' 
        ' Serie
        ' 
        Serie.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Serie.DataPropertyName = "Serie"
        Serie.HeaderText = "Série"
        Serie.Name = "Serie"
        Serie.ReadOnly = True
        ' 
        ' Chave
        ' 
        Chave.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Chave.DataPropertyName = "Chave"
        Chave.HeaderText = "Chave"
        Chave.Name = "Chave"
        Chave.ReadOnly = True
        ' 
        ' Status
        ' 
        Status.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Status.DataPropertyName = "Status"
        Status.HeaderText = "Status"
        Status.Name = "Status"
        Status.ReadOnly = True
        ' 
        ' Data
        ' 
        Data.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Data.DataPropertyName = "Data"
        Data.HeaderText = "Data"
        Data.Name = "Data"
        Data.ReadOnly = True
        ' 
        ' tabConfiguracoes
        ' 
        tabConfiguracoes.Controls.Add(grpConexao)
        tabConfiguracoes.Controls.Add(lblVersao)
        tabConfiguracoes.Controls.Add(btnVerificarAtualizacao)
        tabConfiguracoes.Controls.Add(grpEmail)
        tabConfiguracoes.Controls.Add(btnAjuda)
        tabConfiguracoes.Controls.Add(grpAgendamento)
        tabConfiguracoes.Controls.Add(btnRestaurarVersao)
        tabConfiguracoes.Location = New Point(4, 24)
        tabConfiguracoes.Name = "tabConfiguracoes"
        tabConfiguracoes.Padding = New Padding(3)
        tabConfiguracoes.Size = New Size(616, 667)
        tabConfiguracoes.TabIndex = 1
        tabConfiguracoes.Text = "Configurações"
        tabConfiguracoes.UseVisualStyleBackColor = True
        ' 
        ' grpConexao
        ' 
        grpConexao.Controls.Add(lbServidor)
        grpConexao.Controls.Add(lbServ)
        grpConexao.Controls.Add(btnConfigurarServidor)
        grpConexao.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        grpConexao.Location = New Point(8, 8)
        grpConexao.Name = "grpConexao"
        grpConexao.Size = New Size(290, 79)
        grpConexao.TabIndex = 0
        grpConexao.TabStop = False
        grpConexao.Text = "Bancos de Dados"
        ' 
        ' lbServidor
        ' 
        lbServidor.AutoSize = True
        lbServidor.Location = New Point(10, 25)
        lbServidor.Name = "lbServidor"
        lbServidor.Size = New Size(60, 15)
        lbServidor.TabIndex = 0
        lbServidor.Text = "Banco(s):"
        ' 
        ' lbServ
        ' 
        lbServ.AutoSize = True
        lbServ.Location = New Point(70, 25)
        lbServ.Name = "lbServ"
        lbServ.Size = New Size(41, 15)
        lbServ.TabIndex = 1
        lbServ.Text = "lbServ"
        ' 
        ' btnConfigurarServidor
        ' 
        btnConfigurarServidor.BackColor = SystemColors.HotTrack
        btnConfigurarServidor.Cursor = Cursors.Hand
        btnConfigurarServidor.FlatStyle = FlatStyle.Flat
        btnConfigurarServidor.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConfigurarServidor.ForeColor = SystemColors.GradientActiveCaption
        btnConfigurarServidor.Location = New Point(10, 45)
        btnConfigurarServidor.Name = "btnConfigurarServidor"
        btnConfigurarServidor.Size = New Size(190, 26)
        btnConfigurarServidor.TabIndex = 2
        btnConfigurarServidor.Text = "Configurar Bancos"
        btnConfigurarServidor.UseVisualStyleBackColor = False
        ' 
        ' grpEmail
        ' 
        grpEmail.Controls.Add(Label2)
        grpEmail.Controls.Add(btnConfigurarEmail)
        grpEmail.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        grpEmail.Location = New Point(306, 8)
        grpEmail.Name = "grpEmail"
        grpEmail.Size = New Size(302, 79)
        grpEmail.TabIndex = 1
        grpEmail.TabStop = False
        grpEmail.Text = "E-mail"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(6, 27)
        Label2.Name = "Label2"
        Label2.Size = New Size(247, 15)
        Label2.TabIndex = 3
        Label2.Text = "Configure os e-mails para envio automático"
        ' 
        ' btnConfigurarEmail
        ' 
        btnConfigurarEmail.BackColor = SystemColors.HotTrack
        btnConfigurarEmail.FlatStyle = FlatStyle.Flat
        btnConfigurarEmail.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConfigurarEmail.ForeColor = SystemColors.GradientActiveCaption
        btnConfigurarEmail.Location = New Point(10, 45)
        btnConfigurarEmail.Name = "btnConfigurarEmail"
        btnConfigurarEmail.Size = New Size(199, 26)
        btnConfigurarEmail.TabIndex = 2
        btnConfigurarEmail.Text = "Configurar E-mail"
        btnConfigurarEmail.UseVisualStyleBackColor = False
        ' 
        ' grpAgendamento
        ' 
        grpAgendamento.Controls.Add(chkAgendamentoAtivo)
        grpAgendamento.Controls.Add(lblHoraAgendamento)
        grpAgendamento.Controls.Add(dtpHoraAgendamento)
        grpAgendamento.Controls.Add(chkIniciarComWindows)
        grpAgendamento.Controls.Add(chkManterSempreAtivo)
        grpAgendamento.Controls.Add(chkDiaFixo)
        grpAgendamento.Controls.Add(lblDiaPersonalizado)
        grpAgendamento.Controls.Add(nudDiaAgendamento)
        grpAgendamento.Controls.Add(lblEmailAlerta)
        grpAgendamento.Controls.Add(txtEmailAlertaFalha)
        grpAgendamento.Controls.Add(btnTestarAgendamento)
        grpAgendamento.Location = New Point(6, 93)
        grpAgendamento.Name = "grpAgendamento"
        grpAgendamento.Size = New Size(600, 170)
        grpAgendamento.TabIndex = 2
        grpAgendamento.TabStop = False
        grpAgendamento.Text = "Agendamento Automático"
        ' 
        ' chkAgendamentoAtivo
        ' 
        chkAgendamentoAtivo.AutoSize = True
        chkAgendamentoAtivo.Location = New Point(10, 25)
        chkAgendamentoAtivo.Name = "chkAgendamentoAtivo"
        chkAgendamentoAtivo.Size = New Size(369, 19)
        chkAgendamentoAtivo.TabIndex = 0
        chkAgendamentoAtivo.Text = "Habilitar agendamento mensal (competência do mês anterior)"
        chkAgendamentoAtivo.UseVisualStyleBackColor = True
        ' 
        ' lblHoraAgendamento
        ' 
        lblHoraAgendamento.AutoSize = True
        lblHoraAgendamento.Location = New Point(10, 58)
        lblHoraAgendamento.Name = "lblHoraAgendamento"
        lblHoraAgendamento.Size = New Size(51, 15)
        lblHoraAgendamento.TabIndex = 1
        lblHoraAgendamento.Text = "Horário:"
        ' 
        ' dtpHoraAgendamento
        ' 
        dtpHoraAgendamento.Format = DateTimePickerFormat.Time
        dtpHoraAgendamento.Location = New Point(65, 55)
        dtpHoraAgendamento.Name = "dtpHoraAgendamento"
        dtpHoraAgendamento.ShowUpDown = True
        dtpHoraAgendamento.Size = New Size(70, 21)
        dtpHoraAgendamento.TabIndex = 2
        ' 
        ' chkIniciarComWindows
        ' 
        chkIniciarComWindows.AutoSize = True
        chkIniciarComWindows.Location = New Point(150, 58)
        chkIniciarComWindows.Name = "chkIniciarComWindows"
        chkIniciarComWindows.Size = New Size(150, 19)
        chkIniciarComWindows.TabIndex = 3
        chkIniciarComWindows.Text = "Iniciar com o Windows"
        chkIniciarComWindows.UseVisualStyleBackColor = True
        ' 
        ' chkManterSempreAtivo
        ' 
        chkManterSempreAtivo.AutoSize = True
        chkManterSempreAtivo.Location = New Point(310, 58)
        chkManterSempreAtivo.Name = "chkManterSempreAtivo"
        chkManterSempreAtivo.Size = New Size(185, 19)
        chkManterSempreAtivo.TabIndex = 7
        chkManterSempreAtivo.Text = "Manter sempre em execução"
        chkManterSempreAtivo.UseVisualStyleBackColor = True
        ' 
        ' chkDiaFixo
        ' 
        chkDiaFixo.AutoSize = True
        chkDiaFixo.Checked = True
        chkDiaFixo.CheckState = CheckState.Checked
        chkDiaFixo.Location = New Point(10, 85)
        chkDiaFixo.Name = "chkDiaFixo"
        chkDiaFixo.Size = New Size(90, 19)
        chkDiaFixo.TabIndex = 8
        chkDiaFixo.Text = "Todo dia 01"
        chkDiaFixo.UseVisualStyleBackColor = True
        ' 
        ' lblDiaPersonalizado
        ' 
        lblDiaPersonalizado.AutoSize = True
        lblDiaPersonalizado.Location = New Point(129, 87)
        lblDiaPersonalizado.Name = "lblDiaPersonalizado"
        lblDiaPersonalizado.Size = New Size(110, 15)
        lblDiaPersonalizado.TabIndex = 9
        lblDiaPersonalizado.Text = "Dia personalizado:"
        ' 
        ' nudDiaAgendamento
        ' 
        nudDiaAgendamento.Location = New Point(245, 85)
        nudDiaAgendamento.Maximum = New Decimal(New Integer() {31, 0, 0, 0})
        nudDiaAgendamento.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        nudDiaAgendamento.Name = "nudDiaAgendamento"
        nudDiaAgendamento.Size = New Size(50, 21)
        nudDiaAgendamento.TabIndex = 10
        nudDiaAgendamento.Value = New Decimal(New Integer() {1, 0, 0, 0})
        ' 
        ' lblEmailAlerta
        ' 
        lblEmailAlerta.AutoSize = True
        lblEmailAlerta.Location = New Point(10, 120)
        lblEmailAlerta.Name = "lblEmailAlerta"
        lblEmailAlerta.Size = New Size(155, 15)
        lblEmailAlerta.TabIndex = 4
        lblEmailAlerta.Text = "E-mail para alerta de falha:"
        ' 
        ' txtEmailAlertaFalha
        ' 
        txtEmailAlertaFalha.Location = New Point(175, 117)
        txtEmailAlertaFalha.Name = "txtEmailAlertaFalha"
        txtEmailAlertaFalha.Size = New Size(230, 21)
        txtEmailAlertaFalha.TabIndex = 5
        ' 
        ' btnTestarAgendamento
        ' 
        btnTestarAgendamento.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnTestarAgendamento.Location = New Point(415, 115)
        btnTestarAgendamento.Name = "btnTestarAgendamento"
        btnTestarAgendamento.Size = New Size(120, 26)
        btnTestarAgendamento.TabIndex = 6
        btnTestarAgendamento.Text = "Testar agora"
        btnTestarAgendamento.UseVisualStyleBackColor = True
        ' 
        ' btnRestaurarVersao
        ' 
        btnRestaurarVersao.BackColor = SystemColors.Control
        btnRestaurarVersao.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnRestaurarVersao.ForeColor = Color.DarkRed
        btnRestaurarVersao.Location = New Point(8, 269)
        btnRestaurarVersao.Name = "btnRestaurarVersao"
        btnRestaurarVersao.Size = New Size(181, 26)
        btnRestaurarVersao.TabIndex = 4
        btnRestaurarVersao.Text = "Restaurar Versão Anterior"
        btnRestaurarVersao.UseVisualStyleBackColor = True
        ' 
        ' Label10
        ' 
        Label10.Location = New Point(0, 0)
        Label10.Name = "Label10"
        Label10.Size = New Size(100, 23)
        Label10.TabIndex = 0
        ' 
        ' lblVersao
        ' 
        lblVersao.AutoSize = True
        lblVersao.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblVersao.Location = New Point(539, 324)
        lblVersao.Name = "lblVersao"
        lblVersao.Size = New Size(67, 15)
        lblVersao.TabIndex = 30
        lblVersao.Text = "Versão 1.0"
        ' 
        ' btnVerificarAtualizacao
        ' 
        btnVerificarAtualizacao.Location = New Point(466, 271)
        btnVerificarAtualizacao.Name = "btnVerificarAtualizacao"
        btnVerificarAtualizacao.Size = New Size(140, 22)
        btnVerificarAtualizacao.TabIndex = 31
        btnVerificarAtualizacao.Text = "Verificar Atualizações"
        btnVerificarAtualizacao.UseVisualStyleBackColor = True
        ' 
        ' btnAjuda
        ' 
        btnAjuda.Font = New Font("Arial", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnAjuda.ForeColor = SystemColors.HotTrack
        btnAjuda.Location = New Point(506, 299)
        btnAjuda.Name = "btnAjuda"
        btnAjuda.Size = New Size(100, 22)
        btnAjuda.TabIndex = 32
        btnAjuda.Text = "Ajuda (F1)"
        btnAjuda.UseVisualStyleBackColor = True
        ' 
        ' tmrAtualizacao
        ' 
        tmrAtualizacao.Interval = 1800000
        ' 
        ' tmrAgendamento
        ' 
        tmrAgendamento.Interval = 60000
        ' 
        ' notifyIcon1
        ' 
        notifyIcon1.ContextMenuStrip = contextMenuTray
        notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), Icon)
        notifyIcon1.Text = "Exportador XML"
        notifyIcon1.Visible = True
        ' 
        ' contextMenuTray
        ' 
        contextMenuTray.Items.AddRange(New ToolStripItem() {AbrirToolStripMenuItem, SairToolStripMenuItem})
        contextMenuTray.Name = "contextMenuTray"
        contextMenuTray.Size = New Size(101, 48)
        ' 
        ' AbrirToolStripMenuItem
        ' 
        AbrirToolStripMenuItem.Name = "AbrirToolStripMenuItem"
        AbrirToolStripMenuItem.Size = New Size(100, 22)
        AbrirToolStripMenuItem.Text = "Abrir"
        ' 
        ' SairToolStripMenuItem
        ' 
        SairToolStripMenuItem.Name = "SairToolStripMenuItem"
        SairToolStripMenuItem.Size = New Size(100, 22)
        SairToolStripMenuItem.Text = "Sair"
        ' 
        ' FrmPrincipal
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.Control
        ClientSize = New Size(640, 709)
        Controls.Add(tabPrincipal)
        Font = New Font("Arial", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        KeyPreview = True
        Name = "FrmPrincipal"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Exportador de XML"
        tabPrincipal.ResumeLayout(False)
        tabExportar.ResumeLayout(False)
        tabExportar.PerformLayout()
        grpFiltros.ResumeLayout(False)
        grpFiltros.PerformLayout()
        GroupBox4.ResumeLayout(False)
        GroupBox4.PerformLayout()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        grpAcao.ResumeLayout(False)
        grpAcao.PerformLayout()
        CType(dgvCupons, ComponentModel.ISupportInitialize).EndInit()
        tabConfiguracoes.ResumeLayout(False)
        tabConfiguracoes.PerformLayout()
        grpConexao.ResumeLayout(False)
        grpConexao.PerformLayout()
        grpEmail.ResumeLayout(False)
        grpEmail.PerformLayout()
        grpAgendamento.ResumeLayout(False)
        grpAgendamento.PerformLayout()
        CType(nudDiaAgendamento, ComponentModel.ISupportInitialize).EndInit()
        contextMenuTray.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tabPrincipal As TabControl
    Friend WithEvents tabExportar As TabPage
    Friend WithEvents grpFiltros As GroupBox
    Friend WithEvents lblEmpresaFiltro As Label
    Friend WithEvents cboEmpresa As ComboBox
    Friend WithEvents lblPeriodo As Label
    Private dtInicio As DateTimePicker
    Friend WithEvents lblA1 As Label
    Friend WithEvents dtFim As DateTimePicker
    Friend WithEvents lblDirecao As Label
    Friend WithEvents rbSaida As RadioButton
    Friend WithEvents rbEntrada As RadioButton
    Friend WithEvents lblFornecedor As Label
    Friend WithEvents cboFornecedor As ComboBox
    Friend WithEvents lblModelo As Label
    Friend WithEvents rbNFCe As RadioButton
    Friend WithEvents rbNFe As RadioButton
    Friend WithEvents rbAmbos As RadioButton
    Friend WithEvents lblNumDoc As Label
    Friend WithEvents txbInicio As TextBox
    Friend WithEvents lblA2 As Label
    Friend WithEvents txbFim As TextBox
    Friend WithEvents lblSerieFiltro As Label
    Friend WithEvents txbSerie As TextBox
    Friend WithEvents lblStatusFiltro As Label
    Friend WithEvents chkTodos As CheckBox
    Friend WithEvents chkEmitidas As CheckBox
    Friend WithEvents chkCancelados As CheckBox
    Friend WithEvents chkInutilizados As CheckBox
    Friend WithEvents grpAcao As GroupBox
    Friend WithEvents lblDestino As Label
    Friend WithEvents txtDestino As TextBox
    Friend WithEvents btnDestino As Button
    Friend WithEvents btnPesquisar As Button
    Friend WithEvents btnExportar As Button
    Friend WithEvents pbExportacao As ProgressBar
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblQuantidade As Label
    Friend WithEvents lblQtd As Label
    Friend WithEvents dgvCupons As DataGridView
    Friend WithEvents Modelo As DataGridViewTextBoxColumn
    Friend WithEvents Documento As DataGridViewTextBoxColumn
    Friend WithEvents Codigo As DataGridViewTextBoxColumn
    Friend WithEvents Fornecedor As DataGridViewTextBoxColumn
    Friend WithEvents Serie As DataGridViewTextBoxColumn
    Friend WithEvents Chave As DataGridViewTextBoxColumn
    Friend WithEvents Status As DataGridViewTextBoxColumn
    Friend WithEvents Data As DataGridViewTextBoxColumn
    Friend WithEvents tabConfiguracoes As TabPage
    Friend WithEvents grpConexao As GroupBox
    Friend WithEvents lbServidor As Label
    Friend WithEvents lbServ As Label
    Friend WithEvents btnConfigurarServidor As Button
    Friend WithEvents grpEmail As GroupBox
    Friend WithEvents Label10 As Label
    Friend WithEvents btnConfigurarEmail As Button
    Friend WithEvents btnMapearDestinatarios As Button
    Friend WithEvents btnRestaurarVersao As Button
    Friend WithEvents grpAgendamento As GroupBox
    Friend WithEvents chkAgendamentoAtivo As CheckBox
    Friend WithEvents lblHoraAgendamento As Label
    Friend WithEvents dtpHoraAgendamento As DateTimePicker
    Friend WithEvents chkIniciarComWindows As CheckBox
    Friend WithEvents chkManterSempreAtivo As CheckBox
    Friend WithEvents chkDiaFixo As CheckBox
    Friend WithEvents lblDiaPersonalizado As Label
    Friend WithEvents nudDiaAgendamento As NumericUpDown
    Friend WithEvents lblEmailAlerta As Label
    Friend WithEvents txtEmailAlertaFalha As TextBox
    Friend WithEvents btnTestarAgendamento As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents lblVersao As Label
    Friend WithEvents btnVerificarAtualizacao As Button
    Friend WithEvents btnAjuda As Button
    Friend WithEvents tmrAtualizacao As Timer
    Friend WithEvents tmrAgendamento As Timer
    Friend WithEvents notifyIcon1 As NotifyIcon
    Friend WithEvents contextMenuTray As ContextMenuStrip
    Friend WithEvents AbrirToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SairToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Label2 As Label

End Class
