<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmProvedoresEmail
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
        lblTitulo = New Label()
        rtbConteudo = New RichTextBox()
        btnFechar = New Button()
        SuspendLayout()
        '
        ' lblTitulo
        '
        lblTitulo.AutoSize = True
        lblTitulo.Font = New Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblTitulo.Location = New Point(12, 9)
        lblTitulo.Name = "lblTitulo"
        lblTitulo.Size = New Size(300, 19)
        lblTitulo.TabIndex = 0
        lblTitulo.Text = "Configurações comuns de e-mail (SMTP)"
        '
        ' rtbConteudo
        '
        rtbConteudo.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        rtbConteudo.BackColor = SystemColors.Window
        rtbConteudo.Location = New Point(12, 34)
        rtbConteudo.Name = "rtbConteudo"
        rtbConteudo.ReadOnly = True
        rtbConteudo.Size = New Size(560, 430)
        rtbConteudo.TabIndex = 1
        rtbConteudo.Text = ""
        '
        ' btnFechar
        '
        btnFechar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnFechar.Location = New Point(472, 472)
        btnFechar.Name = "btnFechar"
        btnFechar.Size = New Size(100, 27)
        btnFechar.TabIndex = 2
        btnFechar.Text = "Fechar"
        btnFechar.UseVisualStyleBackColor = True
        '
        ' FrmProvedoresEmail
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(584, 511)
        Controls.Add(lblTitulo)
        Controls.Add(rtbConteudo)
        Controls.Add(btnFechar)
        MinimumSize = New Size(450, 350)
        Name = "FrmProvedoresEmail"
        StartPosition = FormStartPosition.CenterParent
        Text = "Provedores de E-mail"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblTitulo As Label
    Friend WithEvents rtbConteudo As RichTextBox
    Friend WithEvents btnFechar As Button
End Class
