<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
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
        lbSenha = New Label()
        Label6 = New Label()
        Label7 = New Label()
        Label8 = New Label()
        txtServidor = New TextBox()
        txtPorta = New TextBox()
        txtBanco = New TextBox()
        txtUsuario = New TextBox()
        txtSenha = New TextBox()
        dtInicio = New DateTimePicker()
        dtFim = New DateTimePicker()
        txtDestino = New TextBox()
        btnDestino = New Button()
        btnExportar = New Button()
        pbExportacao = New ProgressBar()
        lblStatus = New Label()
        lblQuantidade = New Label()
        btnTestar = New Button()
        SaveFileDialog1 = New SaveFileDialog()
        Label2 = New Label()
        txtCodEmpresa = New TextBox()
        btnBuscarEmpresa = New Button()
        Label3 = New Label()
        lblCNPJ = New Label()
        Label4 = New Label()
        lblRazao = New Label()
        GroupBox1 = New GroupBox()
        Label5 = New Label()
        GroupBox2 = New GroupBox()
        GroupBox3 = New GroupBox()
        Label1 = New Label()
        GroupBox1.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox3.SuspendLayout()
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
        lbPorta.Location = New Point(21, 53)
        lbPorta.Name = "lbPorta"
        lbPorta.Size = New Size(38, 15)
        lbPorta.TabIndex = 1
        lbPorta.Text = "Porta:"
        ' 
        ' lbBanco
        ' 
        lbBanco.AutoSize = True
        lbBanco.Location = New Point(16, 85)
        lbBanco.Name = "lbBanco"
        lbBanco.Size = New Size(43, 15)
        lbBanco.TabIndex = 2
        lbBanco.Text = "Banco:"
        ' 
        ' lbUsuario
        ' 
        lbUsuario.AutoSize = True
        lbUsuario.Location = New Point(9, 117)
        lbUsuario.Name = "lbUsuario"
        lbUsuario.Size = New Size(50, 15)
        lbUsuario.TabIndex = 3
        lbUsuario.Text = "Usuário:"
        ' 
        ' lbSenha
        ' 
        lbSenha.AutoSize = True
        lbSenha.Location = New Point(17, 149)
        lbSenha.Name = "lbSenha"
        lbSenha.Size = New Size(42, 15)
        lbSenha.TabIndex = 4
        lbSenha.Text = "Senha:"
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
        ' txtServidor
        ' 
        txtServidor.Location = New Point(65, 15)
        txtServidor.Name = "txtServidor"
        txtServidor.Size = New Size(100, 23)
        txtServidor.TabIndex = 8
        ' 
        ' txtPorta
        ' 
        txtPorta.Location = New Point(65, 45)
        txtPorta.Name = "txtPorta"
        txtPorta.Size = New Size(100, 23)
        txtPorta.TabIndex = 9
        ' 
        ' txtBanco
        ' 
        txtBanco.Location = New Point(65, 77)
        txtBanco.Name = "txtBanco"
        txtBanco.Size = New Size(100, 23)
        txtBanco.TabIndex = 10
        ' 
        ' txtUsuario
        ' 
        txtUsuario.Location = New Point(65, 109)
        txtUsuario.Name = "txtUsuario"
        txtUsuario.Size = New Size(100, 23)
        txtUsuario.TabIndex = 11
        ' 
        ' txtSenha
        ' 
        txtSenha.Location = New Point(65, 141)
        txtSenha.Name = "txtSenha"
        txtSenha.Size = New Size(100, 23)
        txtSenha.TabIndex = 12
        txtSenha.UseSystemPasswordChar = True
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
        btnDestino.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnDestino.Location = New Point(119, 145)
        btnDestino.Name = "btnDestino"
        btnDestino.Size = New Size(75, 23)
        btnDestino.TabIndex = 14
        btnDestino.Text = "Procurar"
        btnDestino.UseVisualStyleBackColor = True
        ' 
        ' btnExportar
        ' 
        btnExportar.Font = New Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnExportar.ForeColor = Color.FromArgb(CByte(40), CByte(167), CByte(69))
        btnExportar.Location = New Point(204, 192)
        btnExportar.Name = "btnExportar"
        btnExportar.Size = New Size(260, 41)
        btnExportar.TabIndex = 16
        btnExportar.Text = "Exportar"
        btnExportar.UseVisualStyleBackColor = True
        ' 
        ' pbExportacao
        ' 
        pbExportacao.Location = New Point(210, 266)
        pbExportacao.Name = "pbExportacao"
        pbExportacao.Size = New Size(265, 34)
        pbExportacao.TabIndex = 18
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        lblStatus.Location = New Point(204, 236)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(55, 21)
        lblStatus.TabIndex = 19
        lblStatus.Text = "Status"
        ' 
        ' lblQuantidade
        ' 
        lblQuantidade.AutoSize = True
        lblQuantidade.Font = New Font("Segoe UI Semibold", 12F, FontStyle.Bold)
        lblQuantidade.Location = New Point(481, 273)
        lblQuantidade.Name = "lblQuantidade"
        lblQuantidade.Size = New Size(58, 21)
        lblQuantidade.TabIndex = 20
        lblQuantidade.Text = "Quant."
        ' 
        ' btnTestar
        ' 
        btnTestar.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnTestar.ForeColor = SystemColors.Highlight
        btnTestar.Location = New Point(35, 175)
        btnTestar.Name = "btnTestar"
        btnTestar.Size = New Size(119, 23)
        btnTestar.TabIndex = 21
        btnTestar.Text = "Testar Conexão"
        btnTestar.UseVisualStyleBackColor = True
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
        Label3.Location = New Point(6, 77)
        Label3.Name = "Label3"
        Label3.Size = New Size(37, 15)
        Label3.TabIndex = 26
        Label3.Text = "CNPJ:"
        ' 
        ' lblCNPJ
        ' 
        lblCNPJ.AutoSize = True
        lblCNPJ.Location = New Point(44, 77)
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
        GroupBox1.Size = New Size(198, 174)
        GroupBox1.TabIndex = 30
        GroupBox1.TabStop = False
        GroupBox1.Text = "Empresa"
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
        GroupBox2.Controls.Add(lbServidor)
        GroupBox2.Controls.Add(txtServidor)
        GroupBox2.Controls.Add(lbPorta)
        GroupBox2.Controls.Add(btnTestar)
        GroupBox2.Controls.Add(txtPorta)
        GroupBox2.Controls.Add(txtBanco)
        GroupBox2.Controls.Add(lbBanco)
        GroupBox2.Controls.Add(txtUsuario)
        GroupBox2.Controls.Add(lbUsuario)
        GroupBox2.Controls.Add(txtSenha)
        GroupBox2.Controls.Add(lbSenha)
        GroupBox2.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        GroupBox2.Location = New Point(12, 12)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(186, 208)
        GroupBox2.TabIndex = 32
        GroupBox2.TabStop = False
        GroupBox2.Text = "Conexão"
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
        GroupBox3.Location = New Point(408, 12)
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
        Label1.Location = New Point(116, 273)
        Label1.Name = "Label1"
        Label1.Size = New Size(88, 21)
        Label1.TabIndex = 34
        Label1.Text = "Progresso:"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.Control
        ClientSize = New Size(624, 320)
        Controls.Add(Label1)
        Controls.Add(GroupBox3)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        Controls.Add(lblQuantidade)
        Controls.Add(lblStatus)
        Controls.Add(pbExportacao)
        Controls.Add(btnExportar)
        Name = "Form1"
        Text = "Exportador de XML"
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
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
    Friend WithEvents btnTestar As Button
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

End Class
