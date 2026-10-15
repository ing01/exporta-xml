<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmConfiguracaoEmailEmpresa
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblTituloFixo = New Label()
        lblEmpresaNome = New Label()
        grpModo = New GroupBox()
        rbSincronizar = New RadioButton()
        rbManual = New RadioButton()
        rbPadrao = New RadioButton()
        grpConexao = New GroupBox()
        lblAlerta = New Label()
        chkSSL = New CheckBox()
        lbServidorSMTP = New Label()
        txtServidorSMTP = New TextBox()
        lbPortaSMTP = New Label()
        btnTestarEnvio = New Button()
        txtPortaSMTP = New TextBox()
        txtUsuario = New TextBox()
        lbUsuario = New Label()
        txtSenha = New TextBox()
        lbSenha = New Label()
        btnProvedoresComuns = New Button()
        grpDestinatarios = New GroupBox()
        lbDestinatarios = New ListBox()
        btnAddDest = New Button()
        btnEditDest = New Button()
        btnRemoveDest = New Button()
        lblStatus = New Label()
        btnSalvar = New Button()
        btnCancelar = New Button()
        grpModo.SuspendLayout()
        grpConexao.SuspendLayout()
        grpDestinatarios.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblTituloFixo
        ' 
        lblTituloFixo.AutoSize = True
        lblTituloFixo.ForeColor = SystemColors.GrayText
        lblTituloFixo.Location = New Point(12, 9)
        lblTituloFixo.Name = "lblTituloFixo"
        lblTituloFixo.Size = New Size(140, 15)
        lblTituloFixo.TabIndex = 0
        lblTituloFixo.Text = "Configurando e-mail de:"
        ' 
        ' lblEmpresaNome
        ' 
        lblEmpresaNome.AutoEllipsis = True
        lblEmpresaNome.Font = New Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblEmpresaNome.ForeColor = Color.FromArgb(CByte(30), CByte(60), CByte(120))
        lblEmpresaNome.Location = New Point(12, 26)
        lblEmpresaNome.Name = "lblEmpresaNome"
        lblEmpresaNome.Size = New Size(410, 26)
        lblEmpresaNome.TabIndex = 1
        lblEmpresaNome.Text = "(empresa)"
        ' 
        ' grpModo
        ' 
        grpModo.Controls.Add(rbSincronizar)
        grpModo.Controls.Add(rbManual)
        grpModo.Controls.Add(rbPadrao)
        grpModo.Location = New Point(12, 58)
        grpModo.Name = "grpModo"
        grpModo.Size = New Size(410, 105)
        grpModo.TabIndex = 2
        grpModo.TabStop = False
        grpModo.Text = "Modo de envio desta empresa"
        ' 
        ' rbSincronizar
        ' 
        rbSincronizar.AutoSize = True
        rbSincronizar.Location = New Point(10, 72)
        rbSincronizar.Name = "rbSincronizar"
        rbSincronizar.Size = New Size(265, 19)
        rbSincronizar.TabIndex = 2
        rbSincronizar.Text = "Sincronizar do sistema do cliente (conf_nfe)"
        rbSincronizar.UseVisualStyleBackColor = True
        ' 
        ' rbManual
        ' 
        rbManual.AutoSize = True
        rbManual.Location = New Point(10, 47)
        rbManual.Name = "rbManual"
        rbManual.Size = New Size(301, 19)
        rbManual.TabIndex = 1
        rbManual.Text = "Manual (servidor/usuário próprios desta empresa)"
        rbManual.UseVisualStyleBackColor = True
        ' 
        ' rbPadrao
        ' 
        rbPadrao.AutoSize = True
        rbPadrao.Checked = True
        rbPadrao.Location = New Point(10, 22)
        rbPadrao.Name = "rbPadrao"
        rbPadrao.Size = New Size(215, 19)
        rbPadrao.TabIndex = 0
        rbPadrao.TabStop = True
        rbPadrao.Text = "Usar configuração padrão (Global)"
        rbPadrao.UseVisualStyleBackColor = True
        ' 
        ' grpConexao
        ' 
        grpConexao.Controls.Add(lblAlerta)
        grpConexao.Controls.Add(chkSSL)
        grpConexao.Controls.Add(lbServidorSMTP)
        grpConexao.Controls.Add(txtServidorSMTP)
        grpConexao.Controls.Add(lbPortaSMTP)
        grpConexao.Controls.Add(btnTestarEnvio)
        grpConexao.Controls.Add(txtPortaSMTP)
        grpConexao.Controls.Add(txtUsuario)
        grpConexao.Controls.Add(lbUsuario)
        grpConexao.Controls.Add(txtSenha)
        grpConexao.Controls.Add(lbSenha)
        grpConexao.Controls.Add(btnProvedoresComuns)
        grpConexao.ForeColor = Color.FromArgb(CByte(51), CByte(51), CByte(51))
        grpConexao.Location = New Point(12, 169)
        grpConexao.Name = "grpConexao"
        grpConexao.Size = New Size(410, 195)
        grpConexao.TabIndex = 3
        grpConexao.TabStop = False
        grpConexao.Text = "Servidor SMTP"
        ' 
        ' lblAlerta
        ' 
        lblAlerta.AutoSize = True
        lblAlerta.BackColor = SystemColors.Control
        lblAlerta.ForeColor = Color.DimGray
        lblAlerta.Location = New Point(240, 147)
        lblAlerta.Name = "lblAlerta"
        lblAlerta.Size = New Size(38, 15)
        lblAlerta.TabIndex = 10
        lblAlerta.Text = "Label"
        ' 
        ' chkSSL
        ' 
        chkSSL.AutoSize = True
        chkSSL.Enabled = False
        chkSSL.Location = New Point(206, 52)
        chkSSL.Name = "chkSSL"
        chkSSL.Size = New Size(49, 19)
        chkSSL.TabIndex = 5
        chkSSL.Text = "SSL"
        chkSSL.UseVisualStyleBackColor = True
        ' 
        ' lbServidorSMTP
        ' 
        lbServidorSMTP.AutoSize = True
        lbServidorSMTP.Location = New Point(20, 19)
        lbServidorSMTP.Name = "lbServidorSMTP"
        lbServidorSMTP.Size = New Size(90, 15)
        lbServidorSMTP.TabIndex = 0
        lbServidorSMTP.Text = "Servidor SMTP:"
        ' 
        ' txtServidorSMTP
        ' 
        txtServidorSMTP.Location = New Point(118, 16)
        txtServidorSMTP.Name = "txtServidorSMTP"
        txtServidorSMTP.Size = New Size(167, 21)
        txtServidorSMTP.TabIndex = 1
        ' 
        ' lbPortaSMTP
        ' 
        lbPortaSMTP.AutoSize = True
        lbPortaSMTP.Location = New Point(69, 53)
        lbPortaSMTP.Name = "lbPortaSMTP"
        lbPortaSMTP.Size = New Size(39, 15)
        lbPortaSMTP.TabIndex = 4
        lbPortaSMTP.Text = "Porta:"
        ' 
        ' btnTestarEnvio
        ' 
        btnTestarEnvio.Font = New Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnTestarEnvio.ForeColor = SystemColors.Highlight
        btnTestarEnvio.Location = New Point(118, 139)
        btnTestarEnvio.Name = "btnTestarEnvio"
        btnTestarEnvio.Size = New Size(119, 23)
        btnTestarEnvio.TabIndex = 6
        btnTestarEnvio.Text = "Testar Envio"
        btnTestarEnvio.UseVisualStyleBackColor = True
        ' 
        ' txtPortaSMTP
        ' 
        txtPortaSMTP.Location = New Point(118, 48)
        txtPortaSMTP.Name = "txtPortaSMTP"
        txtPortaSMTP.Size = New Size(55, 21)
        txtPortaSMTP.TabIndex = 2
        ' 
        ' txtUsuario
        ' 
        txtUsuario.Location = New Point(118, 77)
        txtUsuario.Name = "txtUsuario"
        txtUsuario.Size = New Size(167, 21)
        txtUsuario.TabIndex = 3
        ' 
        ' lbUsuario
        ' 
        lbUsuario.AutoSize = True
        lbUsuario.Location = New Point(57, 85)
        lbUsuario.Name = "lbUsuario"
        lbUsuario.Size = New Size(54, 15)
        lbUsuario.TabIndex = 2
        lbUsuario.Text = "Usuário:"
        ' 
        ' txtSenha
        ' 
        txtSenha.Location = New Point(118, 106)
        txtSenha.Name = "txtSenha"
        txtSenha.Size = New Size(167, 21)
        txtSenha.TabIndex = 4
        txtSenha.UseSystemPasswordChar = True
        ' 
        ' lbSenha
        ' 
        lbSenha.AutoSize = True
        lbSenha.Location = New Point(65, 114)
        lbSenha.Name = "lbSenha"
        lbSenha.Size = New Size(46, 15)
        lbSenha.TabIndex = 6
        lbSenha.Text = "Senha:"
        ' 
        ' btnProvedoresComuns
        ' 
        btnProvedoresComuns.BackColor = Color.DarkOrange
        btnProvedoresComuns.Font = New Font("Arial", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnProvedoresComuns.ForeColor = Color.White
        btnProvedoresComuns.Location = New Point(118, 166)
        btnProvedoresComuns.Name = "btnProvedoresComuns"
        btnProvedoresComuns.Size = New Size(167, 23)
        btnProvedoresComuns.TabIndex = 7
        btnProvedoresComuns.Text = "Provedores Comuns"
        btnProvedoresComuns.UseVisualStyleBackColor = False
        ' 
        ' grpDestinatarios
        ' 
        grpDestinatarios.Controls.Add(lbDestinatarios)
        grpDestinatarios.Controls.Add(btnAddDest)
        grpDestinatarios.Controls.Add(btnEditDest)
        grpDestinatarios.Controls.Add(btnRemoveDest)
        grpDestinatarios.Location = New Point(12, 371)
        grpDestinatarios.Name = "grpDestinatarios"
        grpDestinatarios.Size = New Size(410, 140)
        grpDestinatarios.TabIndex = 4
        grpDestinatarios.TabStop = False
        grpDestinatarios.Text = "Destinatários"
        ' 
        ' lbDestinatarios
        ' 
        lbDestinatarios.FormattingEnabled = True
        lbDestinatarios.Location = New Point(12, 22)
        lbDestinatarios.Name = "lbDestinatarios"
        lbDestinatarios.Size = New Size(284, 109)
        lbDestinatarios.TabIndex = 0
        ' 
        ' btnAddDest
        ' 
        btnAddDest.Location = New Point(304, 22)
        btnAddDest.Name = "btnAddDest"
        btnAddDest.Size = New Size(94, 23)
        btnAddDest.TabIndex = 1
        btnAddDest.Text = "Adicionar"
        btnAddDest.UseVisualStyleBackColor = True
        ' 
        ' btnEditDest
        ' 
        btnEditDest.Location = New Point(304, 51)
        btnEditDest.Name = "btnEditDest"
        btnEditDest.Size = New Size(94, 23)
        btnEditDest.TabIndex = 2
        btnEditDest.Text = "Editar"
        btnEditDest.UseVisualStyleBackColor = True
        ' 
        ' btnRemoveDest
        ' 
        btnRemoveDest.Location = New Point(304, 80)
        btnRemoveDest.Name = "btnRemoveDest"
        btnRemoveDest.Size = New Size(94, 23)
        btnRemoveDest.TabIndex = 3
        btnRemoveDest.Text = "Remover"
        btnRemoveDest.UseVisualStyleBackColor = True
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(12, 518)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(42, 15)
        lblStatus.TabIndex = 5
        lblStatus.Text = "Status"
        ' 
        ' btnSalvar
        ' 
        btnSalvar.Location = New Point(216, 541)
        btnSalvar.Name = "btnSalvar"
        btnSalvar.Size = New Size(100, 28)
        btnSalvar.TabIndex = 6
        btnSalvar.Text = "Salvar"
        btnSalvar.UseVisualStyleBackColor = True
        ' 
        ' btnCancelar
        ' 
        btnCancelar.Location = New Point(322, 541)
        btnCancelar.Name = "btnCancelar"
        btnCancelar.Size = New Size(100, 28)
        btnCancelar.TabIndex = 7
        btnCancelar.Text = "Cancelar"
        btnCancelar.UseVisualStyleBackColor = True
        ' 
        ' FrmConfiguracaoEmailEmpresa
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(434, 578)
        Controls.Add(lblTituloFixo)
        Controls.Add(lblEmpresaNome)
        Controls.Add(grpModo)
        Controls.Add(grpConexao)
        Controls.Add(grpDestinatarios)
        Controls.Add(lblStatus)
        Controls.Add(btnSalvar)
        Controls.Add(btnCancelar)
        Font = New Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmConfiguracaoEmailEmpresa"
        StartPosition = FormStartPosition.CenterParent
        Text = "Configuração de E-mail"
        grpModo.ResumeLayout(False)
        grpModo.PerformLayout()
        grpConexao.ResumeLayout(False)
        grpConexao.PerformLayout()
        grpDestinatarios.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblTituloFixo As Label
    Friend WithEvents lblEmpresaNome As Label
    Friend WithEvents grpModo As GroupBox
    Friend WithEvents rbSincronizar As RadioButton
    Friend WithEvents rbManual As RadioButton
    Friend WithEvents rbPadrao As RadioButton
    Friend WithEvents grpConexao As GroupBox
    Friend WithEvents lblAlerta As Label
    Friend WithEvents chkSSL As CheckBox
    Friend WithEvents lbServidorSMTP As Label
    Friend WithEvents txtServidorSMTP As TextBox
    Friend WithEvents lbPortaSMTP As Label
    Friend WithEvents btnTestarEnvio As Button
    Friend WithEvents txtPortaSMTP As TextBox
    Friend WithEvents txtUsuario As TextBox
    Friend WithEvents lbUsuario As Label
    Friend WithEvents txtSenha As TextBox
    Friend WithEvents lbSenha As Label
    Friend WithEvents btnProvedoresComuns As Button
    Friend WithEvents grpDestinatarios As GroupBox
    Friend WithEvents lbDestinatarios As ListBox
    Friend WithEvents btnAddDest As Button
    Friend WithEvents btnEditDest As Button
    Friend WithEvents btnRemoveDest As Button
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnSalvar As Button
    Friend WithEvents btnCancelar As Button
End Class
