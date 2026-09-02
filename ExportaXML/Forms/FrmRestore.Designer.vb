<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmRestore
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lbReleases = New System.Windows.Forms.ListBox()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.btnRestore = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lbReleases
        '
        Me.lbReleases.FormattingEnabled = True
        Me.lbReleases.ItemHeight = 15
        Me.lbReleases.Location = New System.Drawing.Point(12, 12)
        Me.lbReleases.Name = "lbReleases"
        Me.lbReleases.Size = New System.Drawing.Size(460, 259)
        Me.lbReleases.TabIndex = 0
        '
        'btnRefresh
        '
        Me.btnRefresh.Location = New System.Drawing.Point(12, 280)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(100, 27)
        Me.btnRefresh.TabIndex = 1
        Me.btnRefresh.Text = "Atualizar"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'btnRestore
        '
        Me.btnRestore.Location = New System.Drawing.Point(372, 280)
        Me.btnRestore.Name = "btnRestore"
        Me.btnRestore.Size = New System.Drawing.Size(100, 27)
        Me.btnRestore.TabIndex = 2
        Me.btnRestore.Text = "Restaurar"
        Me.btnRestore.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(120, 286)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(39, 15)
        Me.lblStatus.TabIndex = 3
        Me.lblStatus.Text = "Status"
        '
        'FrmRestore
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(484, 321)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnRestore)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.lbReleases)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmRestore"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Restaurar versão (GitHub)"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lbReleases As ListBox
    Friend WithEvents btnRefresh As Button
    Friend WithEvents btnRestore As Button
    Friend WithEvents lblStatus As Label
End Class