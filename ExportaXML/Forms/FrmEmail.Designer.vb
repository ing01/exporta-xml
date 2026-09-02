<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmEmail
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        GroupBox2 = New GroupBox()
        lblAlerta = New Label()
        chkSSL = New CheckBox()
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
        cboEmpresaSinc = New GroupBox()
        lbEmpresa = New Label()
        cbEmpresaSinc = New ComboBox()
        rbSincronizar = New RadioButton()
        rbManual = New RadioButton()
        grpDestinatarios = New GroupBox()
        lbDestinatarios = New ListBox()
        btnAddDest = New Button()
        btnEditDest = New Button()
        btnRemoveDest = New Button()
        lblStatus = New Label()
        GroupBox2.SuspendLayout()
        cboEmpresaSinc.SuspendLayout()
        grpDestinatarios.SuspendLayout()
        SuspendLayout()
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(lblAlerta)
        GroupBox2.Controls.Add(chkSSL)
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
        GroupBox2.Size = New Size(386, 227)
        GroupBox2.TabIndex = 34
        GroupBox2.TabStop = False
        GroupBox2.Text = "Conexão"
        ' 
        ' lblAlerta
        ' 
        lblAlerta.AutoSize = True
        lblAlerta.BackColor = SystemColors.Control
        lblAlerta.ForeColor = Color.Red
        lblAlerta.Location = New Point(27, 147)
        lblAlerta.Name = "lblAlerta"
        lblAlerta.Size = New Size(35, 15)
        lblAlerta.TabIndex = 10
        lblAlerta.Text = "Label"
        ' 
        ' chkSSL
        ' 
        chkSSL.AutoSize = True
        chkSSL.Enabled = False
        chkSSL.Location = New Point(206, 52)
        chkSSL.Name = "chkSSL"
        chkSSL.Size = New Size(44, 19)
        chkSSL.TabIndex = 25
        chkSSL.Text = "SSL"
        chkSSL.UseVisualStyleBackColor = True
        ' 
        ' btnSalvar
        ' 
        btnSalvar.Location = New Point(163, 168)
        btnSalvar.Name = "btnSalvar"
        btnSalvar.Size = New Size(119, 23)
        btnSalvar.TabIndex = 22
        btnSalvar.Text = "Salvar"
        btnSalvar.UseVisualStyleBackColor = True
        ' 
        ' lbServidorSMTP
        ' 
        lbServidorSMTP.AutoSize = True
        lbServidorSMTP.Location = New Point(47, 19)
        lbServidorSMTP.Name = "lbServidorSMTP"
        lbServidorSMTP.Size = New Size(87, 15)
        lbServidorSMTP.TabIndex = 0
        lbServidorSMTP.Text = "Servidor SMTP:"
        ' 
        ' txtServidorSMTP
        ' 
        txtServidorSMTP.Location = New Point(145, 16)
        txtServidorSMTP.Name = "txtServidorSMTP"
        txtServidorSMTP.Size = New Size(167, 23)
        txtServidorSMTP.TabIndex = 8
        ' 
        ' lbPortaSMTP
        ' 
        lbPortaSMTP.AutoSize = True
        lbPortaSMTP.Location = New Point(96, 53)
        lbPortaSMTP.Name = "lbPortaSMTP"
        lbPortaSMTP.Size = New Size(38, 15)
        lbPortaSMTP.TabIndex = 1
        lbPortaSMTP.Text = "Porta:"
        ' 
        ' btnTestarEnvio
        ' 
        btnTestarEnvio.Font = New Font("Segoe UI Semibold", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnTestarEnvio.ForeColor = SystemColors.Highlight
        btnTestarEnvio.Location = New Point(163, 139)
        btnTestarEnvio.Name = "btnTestarEnvio"
        btnTestarEnvio.Size = New Size(119, 23)
        btnTestarEnvio.TabIndex = 21
        btnTestarEnvio.Text = "Testar Envio"
        btnTestarEnvio.UseVisualStyleBackColor = True
        ' 
        ' txtPortaSMTP
        ' 
        txtPortaSMTP.Location = New Point(145, 48)
        txtPortaSMTP.Name = "txtPortaSMTP"
        txtPortaSMTP.Size = New Size(55, 23)
        txtPortaSMTP.TabIndex = 9
        ' 
        ' txtUsuario
        ' 
        txtUsuario.Location = New Point(145, 77)
        txtUsuario.Name = "txtUsuario"
        txtUsuario.Size = New Size(167, 23)
        txtUsuario.TabIndex = 11
        ' 
        ' lbUsuario
        ' 
        lbUsuario.AutoSize = True
        lbUsuario.Location = New Point(84, 85)
        lbUsuario.Name = "lbUsuario"
        lbUsuario.Size = New Size(50, 15)
        lbUsuario.TabIndex = 3
        lbUsuario.Text = "Usuário:"
        ' 
        ' txtSenha
        ' 
        txtSenha.Location = New Point(145, 106)
        txtSenha.Name = "txtSenha"
        txtSenha.Size = New Size(167, 23)
        txtSenha.TabIndex = 12
        txtSenha.UseSystemPasswordChar = True
        ' 
        ' lbSenha
        ' 
        lbSenha.AutoSize = True
        lbSenha.Location = New Point(92, 114)
        lbSenha.Name = "lbSenha"
        lbSenha.Size = New Size(42, 15)
        lbSenha.TabIndex = 4
        lbSenha.Text = "Senha:"
        ' 
        ' cboEmpresaSinc
        ' 
        cboEmpresaSinc.Controls.Add(lbEmpresa)
        cboEmpresaSinc.Controls.Add(cbEmpresaSinc)
        cboEmpresaSinc.Controls.Add(rbSincronizar)
        cboEmpresaSinc.Controls.Add(rbManual)
        cboEmpresaSinc.Location = New Point(12, 245)
        cboEmpresaSinc.Name = "cboEmpresaSinc"
        cboEmpresaSinc.Size = New Size(386, 77)
        cboEmpresaSinc.TabIndex = 35
        cboEmpresaSinc.TabStop = False
        cboEmpresaSinc.Text = "Origem"
        ' 
        ' lbEmpresa
        ' 
        lbEmpresa.AutoSize = True
        lbEmpresa.Location = New Point(245, 24)
        lbEmpresa.Name = "lbEmpresa"
        lbEmpresa.Size = New Size(117, 15)
        lbEmpresa.TabIndex = 37
        lbEmpresa.Text = "Selecione a empresa:"
        ' 
        ' cbEmpresaSinc
        ' 
        cbEmpresaSinc.FormattingEnabled = True
        cbEmpresaSinc.Location = New Point(245, 41)
        cbEmpresaSinc.Name = "cbEmpresaSinc"
        cbEmpresaSinc.Size = New Size(121, 23)
        cbEmpresaSinc.TabIndex = 36
        ' 
        ' rbSincronizar
        ' 
        rbSincronizar.AutoSize = True
        rbSincronizar.Location = New Point(6, 48)
        rbSincronizar.Name = "rbSincronizar"
        rbSincronizar.Size = New Size(136, 19)
        rbSincronizar.TabIndex = 2
        rbSincronizar.Text = "Sincronizar do Banco"
        rbSincronizar.UseVisualStyleBackColor = True
        ' 
        ' rbManual
        ' 
        rbManual.AutoSize = True
        rbManual.Location = New Point(6, 22)
        rbManual.Name = "rbManual"
        rbManual.Size = New Size(65, 19)
        rbManual.TabIndex = 1
        rbManual.Text = "Manual"
        rbManual.UseVisualStyleBackColor = True
        ' 
        ' grpDestinatarios
        ' 
        grpDestinatarios.Controls.Add(lbDestinatarios)
        grpDestinatarios.Controls.Add(btnAddDest)
        grpDestinatarios.Controls.Add(btnEditDest)
        grpDestinatarios.Controls.Add(btnRemoveDest)
        grpDestinatarios.Location = New Point(12, 328)
        grpDestinatarios.Name = "grpDestinatarios"
        grpDestinatarios.Size = New Size(386, 140)
        grpDestinatarios.TabIndex = 36
        grpDestinatarios.TabStop = False
        grpDestinatarios.Text = "Destinatários (por empresa)"
        ' 
        ' lbDestinatarios
        ' 
        lbDestinatarios.FormattingEnabled = True
        lbDestinatarios.Location = New Point(12, 22)
        lbDestinatarios.Name = "lbDestinatarios"
        lbDestinatarios.Size = New Size(260, 109)
        lbDestinatarios.TabIndex = 0
        ' 
        ' btnAddDest
        ' 
        btnAddDest.Location = New Point(280, 22)
        btnAddDest.Name = "btnAddDest"
        btnAddDest.Size = New Size(90, 23)
        btnAddDest.TabIndex = 1
        btnAddDest.Text = "Adicionar"
        btnAddDest.UseVisualStyleBackColor = True
        ' 
        ' btnEditDest
        ' 
        btnEditDest.Location = New Point(280, 51)
        btnEditDest.Name = "btnEditDest"
        btnEditDest.Size = New Size(90, 23)
        btnEditDest.TabIndex = 2
        btnEditDest.Text = "Editar"
        btnEditDest.UseVisualStyleBackColor = True
        ' 
        ' btnRemoveDest
        ' 
        btnRemoveDest.Location = New Point(280, 80)
        btnRemoveDest.Name = "btnRemoveDest"
        btnRemoveDest.Size = New Size(90, 23)
        btnRemoveDest.TabIndex = 3
        btnRemoveDest.Text = "Remover"
        btnRemoveDest.UseVisualStyleBackColor = True
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(12, 475)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(39, 15)
        lblStatus.TabIndex = 37
        lblStatus.Text = "Status"
        ' 
        ' FrmEmail
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(410, 500)
        Controls.Add(lblStatus)
        Controls.Add(grpDestinatarios)
        Controls.Add(cboEmpresaSinc)
        Controls.Add(GroupBox2)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmEmail"
        StartPosition = FormStartPosition.CenterParent
        Text = "FrmEmail"
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        cboEmpresaSinc.ResumeLayout(False)
        cboEmpresaSinc.PerformLayout()
        grpDestinatarios.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents lblAlerta As Label
    Friend WithEvents chkSSL As CheckBox
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
    Friend WithEvents cboEmpresaSinc As GroupBox
    Friend WithEvents lbEmpresa As Label
    Friend WithEvents cbEmpresaSinc As ComboBox
    Friend WithEvents rbSincronizar As RadioButton
    Friend WithEvents rbManual As RadioButton
    Friend WithEvents grpDestinatarios As GroupBox
    Friend WithEvents lbDestinatarios As ListBox
    Friend WithEvents btnAddDest As Button
    Friend WithEvents btnEditDest As Button
    Friend WithEvents btnRemoveDest As Button
    Friend WithEvents lblStatus As Label
End Class