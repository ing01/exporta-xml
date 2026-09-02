<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmDestinatarios
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.cbEmpresas = New System.Windows.Forms.ComboBox()
        Me.lbEmails = New System.Windows.Forms.ListBox()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.btnRemove = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'cbEmpresas
        '
        Me.cbEmpresas.FormattingEnabled = True
        Me.cbEmpresas.Location = New System.Drawing.Point(12, 12)
        Me.cbEmpresas.Name = "cbEmpresas"
        Me.cbEmpresas.Size = New System.Drawing.Size(360, 23)
        Me.cbEmpresas.TabIndex = 0
        '
        'lbEmails
        '
        Me.lbEmails.FormattingEnabled = True
        Me.lbEmails.ItemHeight = 15
        Me.lbEmails.Location = New System.Drawing.Point(12, 45)
        Me.lbEmails.Name = "lbEmails"
        Me.lbEmails.Size = New System.Drawing.Size(360, 184)
        Me.lbEmails.TabIndex = 1
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(12, 235)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(110, 25)
        Me.btnAdd.TabIndex = 2
        Me.btnAdd.Text = "Adicionar"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'btnRemove
        '
        Me.btnRemove.Location = New System.Drawing.Point(128, 235)
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.Size = New System.Drawing.Size(110, 25)
        Me.btnRemove.TabIndex = 3
        Me.btnRemove.Text = "Remover"
        Me.btnRemove.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(262, 235)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(110, 25)
        Me.btnClose.TabIndex = 4
        Me.btnClose.Text = "Fechar"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'FrmDestinatarios
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(384, 272)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnRemove)
        Me.Controls.Add(Me.btnAdd)
        Me.Controls.Add(Me.lbEmails)
        Me.Controls.Add(Me.cbEmpresas)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmDestinatarios"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Destinatários por Empresa"
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents cbEmpresas As ComboBox
    Friend WithEvents lbEmails As ListBox
    Friend WithEvents btnAdd As Button
    Friend WithEvents btnRemove As Button
    Friend WithEvents btnClose As Button
End Class