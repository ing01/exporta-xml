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
        lbServidor = New Label()
        lbPorta = New Label()
        lbBanco = New Label()
        lbUsuario = New Label()
        Label6 = New Label()
        Label7 = New Label()
        Label8 = New Label()
        dtInicio = New DateTimePicker()
        dtFim = New DateTimePicker()
        txtDestino = New TextBox()
        btnDestino = New Button()
        btnExportar = New Button()
        pbExportacao = New ProgressBar()
        lblStatus = New Label()
        lblQuantidade = New Label()
        btnConfigurarServidor = New Button()
        SaveFileDialog1 = New SaveFileDialog()
        Label2 = New Label()
        txtCodEmpresa = New TextBox()
        btnBuscarEmpresa = New Button()
        Label3 = New Label()
        lblCNPJ = New Label()
        Label4 = New Label()
        lblRazao = New Label()
        GroupBox1 = New GroupBox()
        Label11 = New Label()
        Label5 = New Label()
        GroupBox2 = New GroupBox()
        lbUser = New Label()
        lbBan = New Label()
        lbPort = New Label()
        lbServ = New Label()
        GroupBox3 = New GroupBox()
        Label1 = New Label()
        Label9 = New Label()
        Label10 = New Label()
        txtDestinatario = New TextBox()
        btnConfigurarEmail = New Button()
        GroupBox4 = New GroupBox()
        lblRemetente = New Label()
        chkTodos = New CheckBox()
        chkEmitidas = New CheckBox()
        chkCancelados = New CheckBox()
        chkInutilizados = New CheckBox()
        btnPesquisar = New Button()
        dgvCupons = New DataGridView()
        Numero = New DataGridViewTextBoxColumn()
        Codigo = New DataGridViewTextBoxColumn()
        Serie = New DataGridViewTextBoxColumn()
        Chave = New DataGridViewTextBoxColumn()
        Status = New DataGridViewTextBoxColumn()
        Data = New DataGridViewTextBoxColumn()
        GroupBox1.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox3.SuspendLayout()
        GroupBox4.SuspendLayout()
        CType(dgvCupons, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lbServidor
        ' 
        lbServidor.AutoSize = True
        lbServidor.Location = New Point(6, 19)
        lbServidor.Name = "lbServidor"
        lbServidor.Size = New Size(53, 15)
        lbServidor.TabIndex = 0
        lbServidor.Text = "Servidor:"
        ' 
        ' lbPorta
        ' 
        lbPorta.AutoSize = True
        lbPorta.Location = New Point(21, 34)
        lbPorta.Name = "lbPorta"
        lbPorta.Size = New Size(38, 15)
        lbPorta.TabIndex = 1
        lbPorta.Text = "Porta:"
        ' 
        ' lbBanco
        ' 
        lbBanco.AutoSize = True
        lbBanco.Location = New Point(16, 49)
        lbBanco.Name = "lbBanco"
        lbBanco.Size = New Size(43, 15)
        lbBanco.TabIndex = 2
        lbBanco.Text = "Banco:"
        ' 
        ' lbUsuario
        ' 
        lbUsuario.AutoSize = True
        lbUsuario.Location = New Point(9, 64)
        lbUsuario.Name = "lbUsuario"
        lbUsuario.Size = New Size(50, 15)
        lbUsuario.TabIndex = 3
        lbUsuario.Text = "Usuário:"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(6, 25)
        Label6.Name = "Label6"
        Label6.Size = New Size(68, 15)
        Label6.TabIndex = 5
        Label6.Text = "Data Inicial:"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(6, 53)
        Label7.Name = "Label7"
        Label7.Size = New Size(62, 15)
        Label7.TabIndex = 6
        Label7.Text = "Data Final:"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(6, 99)
        Label8.Name = "Label8"
        Label8.Size = New Size(50, 15)
        Label8.TabIndex = 7
        Label8.Text = "Destino:"
        ' 
        ' dtInicio
        ' 
        dtInicio.Location = New Point(80, 19)
        dtInicio.Name = "dtInicio"
        dtInicio.Size = New Size(99, 23)
        dtInicio.TabIndex = 0
        ' 
        ' dtFim
        ' 
        dtFim.Location = New Point(80, 48)
        dtFim.Name = "dtFim"
        dtFim.Size = New Size(99, 23)
        dtFim.TabIndex = 0
        ' 
        ' txtDestino
        ' 
        txtDestino.Location = New Point(59, 95)
        txtDestino.Name = "txtDestino"
        txtDestino.Size = New Size(100, 23)
        txtDestino.TabIndex = 13
        ' 
        ' btnDestino
        ' 
        btnDestino.BackColor = Color.Gold
        btnDestino.FlatStyle = FlatStyle.Flat
        btnDestino.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnDestino.ForeColor = SystemColors.ActiveCaptionText
        btnDestino.Location = New Point(31, 139)
        btnDestino.Name = "btnDestino"
        btnDestino.Size = New Size(135, 26)
        btnDestino.TabIndex = 14
        btnDestino.Text = "Selecionar Pasta"
        btnDestino.UseVisualStyleBackColor = False
        ' 
        ' btnExportar
        ' 
        btnExportar.BackColor = Color.ForestGreen
        btnExportar.FlatStyle = FlatStyle.Flat
        btnExportar.Font = New Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnExportar.ForeColor = Color.Honeydew
        btnExportar.Location = New Point(172, 433)
        btnExportar.Name = "btnExportar"
        btnExportar.Size = New Size(260, 41)
        btnExportar.TabIndex = 16
        btnExportar.Text = "Exportar"
        btnExportar.UseVisualStyleBackColor = False
        ' 
        ' pbExportacao
        ' 
        pbExportacao.Location = New Point(172, 480)
        pbExportacao.Name = "pbExportacao"
        pbExportacao.Size = New Size(265, 34)
        pbExportacao.TabIndex = 18
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        lblStatus.Location = New Point(172, 517)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(55, 21)
        lblStatus.TabIndex = 19
        lblStatus.Text = "Status"
        ' 
        ' lblQuantidade
        ' 
        lblQuantidade.AutoSize = True
        lblQuantidade.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        lblQuantidade.Location = New Point(443, 487)
        lblQuantidade.Name = "lblQuantidade"
        lblQuantidade.Size = New Size(58, 21)
        lblQuantidade.TabIndex = 20
        lblQuantidade.Text = "Quant."
        ' 
        ' btnConfigurarServidor
        ' 
        btnConfigurarServidor.BackColor = SystemColors.HotTrack
        btnConfigurarServidor.Cursor = Cursors.Hand
        btnConfigurarServidor.FlatStyle = FlatStyle.Flat
        btnConfigurarServidor.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold)
        btnConfigurarServidor.ForeColor = SystemColors.GradientActiveCaption
        btnConfigurarServidor.Location = New Point(6, 81)
        btnConfigurarServidor.Name = "btnConfigurarServidor"
        btnConfigurarServidor.Size = New Size(148, 26)
        btnConfigurarServidor.TabIndex = 21
        btnConfigurarServidor.Text = "Configurar Servidor"
        btnConfigurarServidor.UseVisualStyleBackColor = False
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(6, 23)
        Label2.Name = "Label2"
        Label2.Size = New Size(46, 15)
        Label2.TabIndex = 23
        Label2.Text = "Código"
        ' 
        ' txtCodEmpresa
        ' 
        txtCodEmpresa.Location = New Point(6, 41)
        txtCodEmpresa.Name = "txtCodEmpresa"
        txtCodEmpresa.Size = New Size(100, 23)
        txtCodEmpresa.TabIndex = 24
        ' 
        ' btnBuscarEmpresa
        ' 
        btnBuscarEmpresa.Location = New Point(112, 41)
        btnBuscarEmpresa.Name = "btnBuscarEmpresa"
        btnBuscarEmpresa.Size = New Size(75, 23)
        btnBuscarEmpresa.TabIndex = 25
        btnBuscarEmpresa.Text = "Buscar"
        btnBuscarEmpresa.UseVisualStyleBackColor = True
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold)
        Label3.Location = New Point(6, 98)
        Label3.Name = "Label3"
        Label3.Size = New Size(37, 15)
        Label3.TabIndex = 26
        Label3.Text = "CNPJ:"
        ' 
        ' lblCNPJ
        ' 
        lblCNPJ.AutoSize = True
        lblCNPJ.Location = New Point(44, 98)
        lblCNPJ.Name = "lblCNPJ"
        lblCNPJ.Size = New Size(41, 15)
        lblCNPJ.TabIndex = 27
        lblCNPJ.Text = "Label4"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold)
        Label4.Location = New Point(6, 119)
        Label4.Name = "Label4"
        Label4.Size = New Size(77, 15)
        Label4.TabIndex = 28
        Label4.Text = "Razão Social:"
        ' 
        ' lblRazao
        ' 
        lblRazao.AutoSize = True
        lblRazao.Location = New Point(87, 119)
        lblRazao.Name = "lblRazao"
        lblRazao.Size = New Size(41, 15)
        lblRazao.TabIndex = 29
        lblRazao.Text = "Label5"
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(Label11)
        GroupBox1.Controls.Add(lblRazao)
        GroupBox1.Controls.Add(Label2)
        GroupBox1.Controls.Add(Label4)
        GroupBox1.Controls.Add(txtCodEmpresa)
        GroupBox1.Controls.Add(lblCNPJ)
        GroupBox1.Controls.Add(btnBuscarEmpresa)
        GroupBox1.Controls.Add(Label3)
        GroupBox1.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        GroupBox1.Location = New Point(204, 12)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(202, 174)
        GroupBox1.TabIndex = 30
        GroupBox1.TabStop = False
        GroupBox1.Text = "Empresa"
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.ForeColor = Color.Red
        Label11.Location = New Point(6, 67)
        Label11.Name = "Label11"
        Label11.Size = New Size(192, 15)
        Label11.TabIndex = 32
        Label11.Text = "'0' para puxar de todas as empresas"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.ForeColor = SystemColors.ActiveBorder
        Label5.Location = New Point(6, 121)
        Label5.Name = "Label5"
        Label5.Size = New Size(120, 15)
        Label5.TabIndex = 31
        Label5.Text = "Exemplo: C:\Duesoft\"
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(lbUser)
        GroupBox2.Controls.Add(lbBan)
        GroupBox2.Controls.Add(lbPort)
        GroupBox2.Controls.Add(lbServ)
        GroupBox2.Controls.Add(lbServidor)
        GroupBox2.Controls.Add(lbPorta)
        GroupBox2.Controls.Add(btnConfigurarServidor)
        GroupBox2.Controls.Add(lbBanco)
        GroupBox2.Controls.Add(lbUsuario)
        GroupBox2.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        GroupBox2.Location = New Point(12, 12)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(186, 114)
        GroupBox2.TabIndex = 32
        GroupBox2.TabStop = False
        GroupBox2.Text = "Conexão"
        ' 
        ' lbUser
        ' 
        lbUser.AutoSize = True
        lbUser.Location = New Point(65, 63)
        lbUser.Name = "lbUser"
        lbUser.Size = New Size(40, 15)
        lbUser.TabIndex = 25
        lbUser.Text = "lbUser"
        ' 
        ' lbBan
        ' 
        lbBan.AutoSize = True
        lbBan.Location = New Point(65, 48)
        lbBan.Name = "lbBan"
        lbBan.Size = New Size(50, 15)
        lbBan.TabIndex = 24
        lbBan.Text = "lbBanco"
        ' 
        ' lbPort
        ' 
        lbPort.AutoSize = True
        lbPort.Location = New Point(65, 34)
        lbPort.Name = "lbPort"
        lbPort.Size = New Size(39, 15)
        lbPort.TabIndex = 23
        lbPort.Text = "lbPort"
        ' 
        ' lbServ
        ' 
        lbServ.AutoSize = True
        lbServ.Location = New Point(65, 19)
        lbServ.Name = "lbServ"
        lbServ.Size = New Size(39, 15)
        lbServ.TabIndex = 22
        lbServ.Text = "lbServ"
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(Label6)
        GroupBox3.Controls.Add(dtInicio)
        GroupBox3.Controls.Add(Label5)
        GroupBox3.Controls.Add(Label7)
        GroupBox3.Controls.Add(dtFim)
        GroupBox3.Controls.Add(Label8)
        GroupBox3.Controls.Add(txtDestino)
        GroupBox3.Controls.Add(btnDestino)
        GroupBox3.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        GroupBox3.Location = New Point(412, 12)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(200, 174)
        GroupBox3.TabIndex = 33
        GroupBox3.TabStop = False
        GroupBox3.Text = "Data e Destino"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(78, 487)
        Label1.Name = "Label1"
        Label1.Size = New Size(88, 21)
        Label1.TabIndex = 34
        Label1.Text = "Progresso:"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Location = New Point(6, 19)
        Label9.Name = "Label9"
        Label9.Size = New Size(67, 15)
        Label9.TabIndex = 22
        Label9.Text = "Remetente:"
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Location = New Point(6, 59)
        Label10.Name = "Label10"
        Label10.Size = New Size(73, 15)
        Label10.TabIndex = 23
        Label10.Text = "Destinatário:"
        ' 
        ' txtDestinatario
        ' 
        txtDestinatario.Location = New Point(80, 51)
        txtDestinatario.Name = "txtDestinatario"
        txtDestinatario.Size = New Size(100, 23)
        txtDestinatario.TabIndex = 30
        ' 
        ' btnConfigurarEmail
        ' 
        btnConfigurarEmail.BackColor = SystemColors.HotTrack
        btnConfigurarEmail.FlatStyle = FlatStyle.Flat
        btnConfigurarEmail.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConfigurarEmail.ForeColor = SystemColors.GradientActiveCaption
        btnConfigurarEmail.Location = New Point(6, 88)
        btnConfigurarEmail.Name = "btnConfigurarEmail"
        btnConfigurarEmail.Size = New Size(148, 26)
        btnConfigurarEmail.TabIndex = 31
        btnConfigurarEmail.Text = "Configurar E-mail"
        btnConfigurarEmail.UseVisualStyleBackColor = False
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(lblRemetente)
        GroupBox4.Controls.Add(Label9)
        GroupBox4.Controls.Add(btnConfigurarEmail)
        GroupBox4.Controls.Add(txtDestinatario)
        GroupBox4.Controls.Add(Label10)
        GroupBox4.Location = New Point(12, 133)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(186, 129)
        GroupBox4.TabIndex = 35
        GroupBox4.TabStop = False
        GroupBox4.Text = "E-mail"
        ' 
        ' lblRemetente
        ' 
        lblRemetente.AutoSize = True
        lblRemetente.Location = New Point(79, 19)
        lblRemetente.Name = "lblRemetente"
        lblRemetente.Size = New Size(93, 15)
        lblRemetente.TabIndex = 32
        lblRemetente.Text = "emailRemetente"
        ' 
        ' chkTodos
        ' 
        chkTodos.AutoSize = True
        chkTodos.Location = New Point(215, 192)
        chkTodos.Name = "chkTodos"
        chkTodos.Size = New Size(58, 19)
        chkTodos.TabIndex = 36
        chkTodos.Text = "Todos"
        chkTodos.UseVisualStyleBackColor = True
        ' 
        ' chkEmitidas
        ' 
        chkEmitidas.AutoSize = True
        chkEmitidas.Location = New Point(291, 192)
        chkEmitidas.Name = "chkEmitidas"
        chkEmitidas.Size = New Size(72, 19)
        chkEmitidas.TabIndex = 37
        chkEmitidas.Text = "Emitidos"
        chkEmitidas.UseVisualStyleBackColor = True
        ' 
        ' chkCancelados
        ' 
        chkCancelados.AutoSize = True
        chkCancelados.Location = New Point(381, 192)
        chkCancelados.Name = "chkCancelados"
        chkCancelados.Size = New Size(87, 19)
        chkCancelados.TabIndex = 38
        chkCancelados.Text = "Cancelados"
        chkCancelados.UseVisualStyleBackColor = True
        ' 
        ' chkInutilizados
        ' 
        chkInutilizados.AutoSize = True
        chkInutilizados.Location = New Point(486, 192)
        chkInutilizados.Name = "chkInutilizados"
        chkInutilizados.Size = New Size(86, 19)
        chkInutilizados.TabIndex = 39
        chkInutilizados.Text = "Inutilizados"
        chkInutilizados.UseVisualStyleBackColor = True
        ' 
        ' btnPesquisar
        ' 
        btnPesquisar.Location = New Point(208, 221)
        btnPesquisar.Name = "btnPesquisar"
        btnPesquisar.Size = New Size(124, 41)
        btnPesquisar.TabIndex = 40
        btnPesquisar.Text = "Pesquisar"
        btnPesquisar.UseVisualStyleBackColor = True
        ' 
        ' dgvCupons
        ' 
        dgvCupons.AllowUserToAddRows = False
        dgvCupons.AllowUserToDeleteRows = False
        dgvCupons.BackgroundColor = SystemColors.ControlLight
        dgvCupons.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvCupons.Columns.AddRange(New DataGridViewColumn() {Numero, Codigo, Serie, Chave, Status, Data})
        dgvCupons.GridColor = SystemColors.ActiveCaptionText
        dgvCupons.Location = New Point(7, 277)
        dgvCupons.Name = "dgvCupons"
        dgvCupons.ReadOnly = True
        dgvCupons.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvCupons.Size = New Size(605, 150)
        dgvCupons.TabIndex = 41
        ' 
        ' Numero
        ' 
        Numero.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Numero.DataPropertyName = "Cupom"
        Numero.HeaderText = "Cupom"
        Numero.Name = "Numero"
        Numero.ReadOnly = True
        ' 
        ' Codigo
        ' 
        Codigo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Codigo.DataPropertyName = "Empresa"
        Codigo.HeaderText = "Empresa"
        Codigo.Name = "Codigo"
        Codigo.ReadOnly = True
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
        Chave.HeaderText = "Chave CFe"
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
        ' FrmPrincipal
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.Control
        ClientSize = New Size(624, 548)
        Controls.Add(dgvCupons)
        Controls.Add(btnPesquisar)
        Controls.Add(chkInutilizados)
        Controls.Add(chkCancelados)
        Controls.Add(chkEmitidas)
        Controls.Add(chkTodos)
        Controls.Add(GroupBox4)
        Controls.Add(Label1)
        Controls.Add(GroupBox3)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        Controls.Add(lblQuantidade)
        Controls.Add(lblStatus)
        Controls.Add(pbExportacao)
        Controls.Add(btnExportar)
        Name = "FrmPrincipal"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Exportador de XML"
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
        GroupBox4.ResumeLayout(False)
        GroupBox4.PerformLayout()
        CType(dgvCupons, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lbServidor As Label
    Friend WithEvents lbPorta As Label
    Friend WithEvents lbBanco As Label
    Friend WithEvents lbUsuario As Label
    Friend WithEvents lbSenha As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label

    Friend WithEvents txtServidor As TextBox
    Friend WithEvents txtPorta As TextBox
    Friend WithEvents txtBanco As TextBox
    Friend WithEvents txtUsuario As TextBox
    Friend WithEvents txtSenha As TextBox
    Private dtInicio As DateTimePicker
    Friend WithEvents DateTimePicker1 As DateTimePicker
    Friend WithEvents dtFim As DateTimePicker
    Friend WithEvents txtDestino As TextBox
    Friend WithEvents btnDestino As Button
    Friend WithEvents btnExportar As Button
    Friend WithEvents pbExportacao As ProgressBar
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblQuantidade As Label
    Friend WithEvents btnConfigurarServidor As Button
    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents Label2 As Label
    Friend WithEvents txtCodEmpresa As TextBox
    Friend WithEvents btnBuscarEmpresa As Button
    Friend WithEvents Label3 As Label
    Friend WithEvents lblCNPJ As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents lblRazao As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label5 As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents txtDestinatario As TextBox
    Friend WithEvents btnConfigurarEmail As Button
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents lbUser As Label
    Friend WithEvents lbBan As Label
    Friend WithEvents lbPort As Label
    Friend WithEvents lbServ As Label
    Friend WithEvents lblRemetente As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents chkTodos As CheckBox
    Friend WithEvents chkEmitidas As CheckBox
    Friend WithEvents chkCancelados As CheckBox
    Friend WithEvents chkInutilizados As CheckBox
    Friend WithEvents btnPesquisar As Button
    Friend WithEvents dgvCupons As DataGridView
    Friend WithEvents Numero As DataGridViewTextBoxColumn
    Friend WithEvents Codigo As DataGridViewTextBoxColumn
    Friend WithEvents Serie As DataGridViewTextBoxColumn
    Friend WithEvents Chave As DataGridViewTextBoxColumn
    Friend WithEvents Status As DataGridViewTextBoxColumn
    Friend WithEvents Data As DataGridViewTextBoxColumn

End Class
