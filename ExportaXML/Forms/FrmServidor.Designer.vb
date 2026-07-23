<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmServidor
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
        btnSalvar = New Button()
        lbServidor = New Label()
        txtServidor = New TextBox()
        lbPorta = New Label()
        btnTestar = New Button()
        txtPorta = New TextBox()
        txtBanco = New TextBox()
        lbBanco = New Label()
        txtUsuario = New TextBox()
        lbUsuario = New Label()
        txtSenha = New TextBox()
        lbSenha = New Label()
        GroupBox2.SuspendLayout()
        SuspendLayout()
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(btnSalvar)
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
        GroupBox2.Size = New Size(186, 240)
        GroupBox2.TabIndex = 33
        GroupBox2.TabStop = False
        GroupBox2.Text = "Conexão"
        ' 
        ' btnSalvar
        ' 
        btnSalvar.Location = New Point(35, 204)
        btnSalvar.Name = "btnSalvar"
        btnSalvar.Size = New Size(119, 23)
        btnSalvar.TabIndex = 22
        btnSalvar.Text = "Salvar"
        btnSalvar.UseVisualStyleBackColor = True
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
        ' txtServidor
        ' 
        txtServidor.Location = New Point(65, 15)
        txtServidor.Name = "txtServidor"
        txtServidor.Size = New Size(100, 23)
        txtServidor.TabIndex = 8
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
        ' lbBanco
        ' 
        lbBanco.AutoSize = True
        lbBanco.Location = New Point(16, 85)
        lbBanco.Name = "lbBanco"
        lbBanco.Size = New Size(43, 15)
        lbBanco.TabIndex = 2
        lbBanco.Text = "Banco:"
        ' 
        ' txtUsuario
        ' 
        txtUsuario.Location = New Point(65, 109)
        txtUsuario.Name = "txtUsuario"
        txtUsuario.Size = New Size(100, 23)
        txtUsuario.TabIndex = 11
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
        ' txtSenha
        ' 
        txtSenha.Location = New Point(65, 141)
        txtSenha.Name = "txtSenha"
        txtSenha.Size = New Size(100, 23)
        txtSenha.TabIndex = 12
        txtSenha.UseSystemPasswordChar = True
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
        ' FrmServidor
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(215, 267)
        Controls.Add(GroupBox2)
        Name = "FrmServidor"
        StartPosition = FormStartPosition.CenterScreen
        Text = "FrmServidor"
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents btnSalvar As Button
    Friend WithEvents lbServidor As Label
    Friend WithEvents txtServidor As TextBox
    Friend WithEvents lbPorta As Label
    Friend WithEvents btnTestar As Button
    Friend WithEvents txtPorta As TextBox
    Friend WithEvents txtBanco As TextBox
    Friend WithEvents lbBanco As Label
    Friend WithEvents txtUsuario As TextBox
    Friend WithEvents lbUsuario As Label
    Friend WithEvents txtSenha As TextBox
    Friend WithEvents lbSenha As Label
End Class
