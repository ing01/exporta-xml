<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmEmail
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
        dgvEmpresasEmail = New DataGridView()
        colEmpresa = New DataGridViewTextBoxColumn()
        colModo = New DataGridViewTextBoxColumn()
        colDestinatarios = New DataGridViewTextBoxColumn()
        lblExplicacao = New Label()
        btnConfigurar = New Button()
        btnFechar = New Button()
        CType(dgvEmpresasEmail, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' lblExplicacao
        '
        lblExplicacao.AutoSize = True
        lblExplicacao.ForeColor = SystemColors.GrayText
        lblExplicacao.Location = New Point(12, 9)
        lblExplicacao.Name = "lblExplicacao"
        lblExplicacao.Size = New Size(430, 15)
        lblExplicacao.TabIndex = 0
        lblExplicacao.Text = "Cada empresa tem sua própria configuração. Selecione uma e clique em Configurar."
        '
        ' dgvEmpresasEmail
        '
        dgvEmpresasEmail.AllowUserToAddRows = False
        dgvEmpresasEmail.AllowUserToDeleteRows = False
        dgvEmpresasEmail.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        dgvEmpresasEmail.AutoGenerateColumns = False
        dgvEmpresasEmail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvEmpresasEmail.Columns.AddRange(New DataGridViewColumn() {colEmpresa, colModo, colDestinatarios})
        dgvEmpresasEmail.Location = New Point(12, 32)
        dgvEmpresasEmail.MultiSelect = False
        dgvEmpresasEmail.Name = "dgvEmpresasEmail"
        dgvEmpresasEmail.ReadOnly = True
        dgvEmpresasEmail.RowHeadersVisible = False
        dgvEmpresasEmail.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvEmpresasEmail.Size = New Size(560, 360)
        dgvEmpresasEmail.TabIndex = 1
        '
        ' colEmpresa
        '
        colEmpresa.DataPropertyName = "NomeExibicao"
        colEmpresa.HeaderText = "Empresa"
        colEmpresa.Name = "colEmpresa"
        colEmpresa.ReadOnly = True
        '
        ' colModo
        '
        colModo.DataPropertyName = "Modo"
        colModo.HeaderText = "Envio"
        colModo.Name = "colModo"
        colModo.ReadOnly = True
        '
        ' colDestinatarios
        '
        colDestinatarios.DataPropertyName = "Destinatarios"
        colDestinatarios.HeaderText = "Destinatários"
        colDestinatarios.Name = "colDestinatarios"
        colDestinatarios.ReadOnly = True
        '
        ' btnConfigurar
        '
        btnConfigurar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnConfigurar.Font = New Font("Arial", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConfigurar.Location = New Point(12, 400)
        btnConfigurar.Name = "btnConfigurar"
        btnConfigurar.Size = New Size(140, 30)
        btnConfigurar.TabIndex = 2
        btnConfigurar.Text = "Configurar"
        btnConfigurar.UseVisualStyleBackColor = True
        '
        ' btnFechar
        '
        btnFechar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnFechar.Location = New Point(472, 400)
        btnFechar.Name = "btnFechar"
        btnFechar.Size = New Size(100, 30)
        btnFechar.TabIndex = 3
        btnFechar.Text = "Fechar"
        btnFechar.UseVisualStyleBackColor = True
        '
        ' FrmEmail
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(584, 442)
        Controls.Add(lblExplicacao)
        Controls.Add(dgvEmpresasEmail)
        Controls.Add(btnConfigurar)
        Controls.Add(btnFechar)
        Font = New Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        MinimumSize = New Size(500, 380)
        Name = "FrmEmail"
        StartPosition = FormStartPosition.CenterParent
        Text = "Configurar E-mail"
        CType(dgvEmpresasEmail, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents dgvEmpresasEmail As DataGridView
    Friend WithEvents colEmpresa As DataGridViewTextBoxColumn
    Friend WithEvents colModo As DataGridViewTextBoxColumn
    Friend WithEvents colDestinatarios As DataGridViewTextBoxColumn
    Friend WithEvents lblExplicacao As Label
    Friend WithEvents btnConfigurar As Button
    Friend WithEvents btnFechar As Button
End Class
