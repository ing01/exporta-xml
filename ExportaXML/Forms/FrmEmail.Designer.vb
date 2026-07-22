<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmEmail
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: o procedimento a seguir é exigido pelo Windows Form Designer
    'Pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        GroupBox2 = New GroupBox()
        chkSSL = New CheckBox()
        txtRemetente = New TextBox()
        lblRemetente = New Label()
        btnSalvar = New Button()
        lbServidorSMTP = New Label()
        txtServidorSMTP = New TextBox()
        lbPortaSMTP = New Label()
        btnTestarEnvio = New Button()
        txtPortaSMTP = New TextBox()
        txtUsuario = New TextBox()
        lbUsuario = New Label()
        txtSenha = New TextBox()
        lbSenha = New Label()
        GroupBox2.SuspendLayout()
        SuspendLayout()
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(chkSSL)
        GroupBox2.Controls.Add(txtRemetente)
        GroupBox2.Controls.Add(lblRemetente)
        GroupBox2.Controls.Add(btnSalvar)
        GroupBox2.Controls.Add(lbServidorSMTP)
        GroupBox2.Controls.Add(txtServidorSMTP)
        GroupBox2.Controls.Add(lbPortaSMTP)
        GroupBox2.Controls.Add(btnTestarEnvio)
        GroupBox2.Controls.Add(txtPortaSMTP)
        GroupBox2.Controls.Add(txtUsuario)
        GroupBox2.Controls.Add(lbUsuario)
        GroupBox2.Controls.Add(txtSenha)
        GroupBox2.Controls.Add(lbSenha)
        GroupBox2.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        GroupBox2.Location = New Point(12, 12)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(292, 227)
        GroupBox2.TabIndex = 34
        GroupBox2.TabStop = False
        GroupBox2.Text = "Conexão"
        ' 
        ' chkSSL
        ' 
        chkSSL.AutoSize = True
        chkSSL.Location = New Point(160, 52)
        chkSSL.Name = "chkSSL"
        chkSSL.Size = New Size(44, 19)
        chkSSL.TabIndex = 25
        chkSSL.Text = "SSL"
        chkSSL.UseVisualStyleBackColor = True
        ' 
        ' txtRemetente
        ' 
        txtRemetente.Location = New Point(99, 135)
        txtRemetente.Name = "txtRemetente"
        txtRemetente.Size = New Size(172, 23)
        txtRemetente.TabIndex = 24
        ' 
        ' lblRemetente
        ' 
        lblRemetente.AutoSize = True
        lblRemetente.Location = New Point(21, 143)
        lblRemetente.Name = "lblRemetente"
        lblRemetente.Size = New Size(67, 15)
        lblRemetente.TabIndex = 23
        lblRemetente.Text = "Remetente:"
        ' 
        ' btnSalvar
        ' 
        btnSalvar.Location = New Point(125, 193)
        btnSalvar.Name = "btnSalvar"
        btnSalvar.Size = New Size(119, 23)
        btnSalvar.TabIndex = 22
        btnSalvar.Text = "Salvar"
        btnSalvar.UseVisualStyleBackColor = True
        ' 
        ' lbServidorSMTP
        ' 
        lbServidorSMTP.AutoSize = True
        lbServidorSMTP.Location = New Point(1, 19)
        lbServidorSMTP.Name = "lbServidorSMTP"
        lbServidorSMTP.Size = New Size(87, 15)
        lbServidorSMTP.TabIndex = 0
        lbServidorSMTP.Text = "Servidor SMTP:"
        ' 
        ' txtServidorSMTP
        ' 
        txtServidorSMTP.Location = New Point(99, 16)
        txtServidorSMTP.Name = "txtServidorSMTP"
        txtServidorSMTP.Size = New Size(167, 23)
        txtServidorSMTP.TabIndex = 8
        ' 
        ' lbPortaSMTP
        ' 
        lbPortaSMTP.AutoSize = True
        lbPortaSMTP.Location = New Point(50, 53)
        lbPortaSMTP.Name = "lbPortaSMTP"
        lbPortaSMTP.Size = New Size(38, 15)
        lbPortaSMTP.TabIndex = 1
        lbPortaSMTP.Text = "Porta:"
        ' 
        ' btnTestarEnvio
        ' 
        btnTestarEnvio.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnTestarEnvio.ForeColor = SystemColors.Highlight
        btnTestarEnvio.Location = New Point(125, 164)
        btnTestarEnvio.Name = "btnTestarEnvio"
        btnTestarEnvio.Size = New Size(119, 23)
        btnTestarEnvio.TabIndex = 21
        btnTestarEnvio.Text = "Testar Envio"
        btnTestarEnvio.UseVisualStyleBackColor = True
        ' 
        ' txtPortaSMTP
        ' 
        txtPortaSMTP.Location = New Point(99, 48)
        txtPortaSMTP.Name = "txtPortaSMTP"
        txtPortaSMTP.Size = New Size(55, 23)
        txtPortaSMTP.TabIndex = 9
        ' 
        ' txtUsuario
        ' 
        txtUsuario.Location = New Point(99, 77)
        txtUsuario.Name = "txtUsuario"
        txtUsuario.Size = New Size(167, 23)
        txtUsuario.TabIndex = 11
        ' 
        ' lbUsuario
        ' 
        lbUsuario.AutoSize = True
        lbUsuario.Location = New Point(38, 85)
        lbUsuario.Name = "lbUsuario"
        lbUsuario.Size = New Size(50, 15)
        lbUsuario.TabIndex = 3
        lbUsuario.Text = "Usuário:"
        ' 
        ' txtSenha
        ' 
        txtSenha.Location = New Point(99, 106)
        txtSenha.Name = "txtSenha"
        txtSenha.Size = New Size(167, 23)
        txtSenha.TabIndex = 12
        txtSenha.UseSystemPasswordChar = True
        ' 
        ' lbSenha
        ' 
        lbSenha.AutoSize = True
        lbSenha.Location = New Point(46, 114)
        lbSenha.Name = "lbSenha"
        lbSenha.Size = New Size(42, 15)
        lbSenha.TabIndex = 4
        lbSenha.Text = "Senha:"
        ' 
        ' FrmEmail
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(316, 267)
        Controls.Add(GroupBox2)
        Name = "FrmEmail"
        Text = "FrmEmail"
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents txtRemetente As TextBox
    Friend WithEvents lblRemetente As Label
    Friend WithEvents btnSalvar As Button
    Friend WithEvents lbServidorSMTP As Label
    Friend WithEvents txtServidorSMTP As TextBox
    Friend WithEvents lbPortaSMTP As Label
    Friend WithEvents btnTestarEnvio As Button
    Friend WithEvents txtPortaSMTP As TextBox
    Friend WithEvents txtUsuario As TextBox
    Friend WithEvents lbUsuario As Label
    Friend WithEvents txtSenha As TextBox
    Friend WithEvents lbSenha As Label
    Friend WithEvents chkSSL As CheckBox
End Class
