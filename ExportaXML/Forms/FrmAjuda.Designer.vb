<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmAjuda
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
        trvTopicos = New TreeView()
        rtbConteudo = New RichTextBox()
        btnFechar = New Button()
        SuspendLayout()
        '
        ' trvTopicos
        '
        trvTopicos.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        trvTopicos.Location = New Point(12, 12)
        trvTopicos.Name = "trvTopicos"
        trvTopicos.Size = New Size(220, 396)
        trvTopicos.TabIndex = 0
        '
        ' rtbConteudo
        '
        rtbConteudo.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        rtbConteudo.Location = New Point(244, 12)
        rtbConteudo.Name = "rtbConteudo"
        rtbConteudo.ReadOnly = True
        rtbConteudo.Size = New Size(444, 396)
        rtbConteudo.TabIndex = 1
        rtbConteudo.Text = ""
        '
        ' btnFechar
        '
        btnFechar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnFechar.Location = New Point(581, 420)
        btnFechar.Name = "btnFechar"
        btnFechar.Size = New Size(107, 27)
        btnFechar.TabIndex = 2
        btnFechar.Text = "Fechar"
        btnFechar.UseVisualStyleBackColor = True
        '
        ' FrmAjuda
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(700, 459)
        Controls.Add(trvTopicos)
        Controls.Add(rtbConteudo)
        Controls.Add(btnFechar)
        MinimumSize = New Size(500, 350)
        Name = "FrmAjuda"
        StartPosition = FormStartPosition.CenterParent
        Text = "Guia de Ajuda"
        ResumeLayout(False)
    End Sub

    Friend WithEvents trvTopicos As TreeView
    Friend WithEvents rtbConteudo As RichTextBox
    Friend WithEvents btnFechar As Button
End Class
