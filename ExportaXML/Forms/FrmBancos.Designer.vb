<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmBancos
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
        dgvBancos = New DataGridView()
        colNome = New DataGridViewTextBoxColumn()
        colServidor = New DataGridViewTextBoxColumn()
        colBanco = New DataGridViewTextBoxColumn()
        btnAdicionar = New Button()
        btnEditar = New Button()
        btnRemover = New Button()
        btnFechar = New Button()
        CType(dgvBancos, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' dgvBancos
        '
        dgvBancos.AllowUserToAddRows = False
        dgvBancos.AllowUserToDeleteRows = False
        dgvBancos.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        dgvBancos.AutoGenerateColumns = False
        dgvBancos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvBancos.Columns.AddRange(New DataGridViewColumn() {colNome, colServidor, colBanco})
        dgvBancos.Location = New Point(12, 12)
        dgvBancos.MultiSelect = False
        dgvBancos.Name = "dgvBancos"
        dgvBancos.ReadOnly = True
        dgvBancos.RowHeadersVisible = False
        dgvBancos.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvBancos.Size = New Size(400, 300)
        dgvBancos.TabIndex = 0
        '
        ' colNome
        '
        colNome.DataPropertyName = "Nome"
        colNome.HeaderText = "Nome"
        colNome.Name = "colNome"
        colNome.ReadOnly = True
        '
        ' colServidor
        '
        colServidor.DataPropertyName = "Servidor"
        colServidor.HeaderText = "Servidor"
        colServidor.Name = "colServidor"
        colServidor.ReadOnly = True
        '
        ' colBanco
        '
        colBanco.DataPropertyName = "Banco"
        colBanco.HeaderText = "Banco"
        colBanco.Name = "colBanco"
        colBanco.ReadOnly = True
        '
        ' btnAdicionar
        '
        btnAdicionar.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnAdicionar.Location = New Point(424, 12)
        btnAdicionar.Name = "btnAdicionar"
        btnAdicionar.Size = New Size(100, 28)
        btnAdicionar.TabIndex = 1
        btnAdicionar.Text = "Adicionar"
        btnAdicionar.UseVisualStyleBackColor = True
        '
        ' btnEditar
        '
        btnEditar.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnEditar.Location = New Point(424, 46)
        btnEditar.Name = "btnEditar"
        btnEditar.Size = New Size(100, 28)
        btnEditar.TabIndex = 2
        btnEditar.Text = "Editar"
        btnEditar.UseVisualStyleBackColor = True
        '
        ' btnRemover
        '
        btnRemover.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRemover.Location = New Point(424, 80)
        btnRemover.Name = "btnRemover"
        btnRemover.Size = New Size(100, 28)
        btnRemover.TabIndex = 3
        btnRemover.Text = "Remover"
        btnRemover.UseVisualStyleBackColor = True
        '
        ' btnFechar
        '
        btnFechar.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnFechar.Location = New Point(424, 284)
        btnFechar.Name = "btnFechar"
        btnFechar.Size = New Size(100, 28)
        btnFechar.TabIndex = 4
        btnFechar.Text = "Fechar"
        btnFechar.UseVisualStyleBackColor = True
        '
        ' FrmBancos
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(536, 324)
        Controls.Add(dgvBancos)
        Controls.Add(btnAdicionar)
        Controls.Add(btnEditar)
        Controls.Add(btnRemover)
        Controls.Add(btnFechar)
        MinimumSize = New Size(420, 260)
        Name = "FrmBancos"
        StartPosition = FormStartPosition.CenterParent
        Text = "Bancos de Dados"
        CType(dgvBancos, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents dgvBancos As DataGridView
    Friend WithEvents colNome As DataGridViewTextBoxColumn
    Friend WithEvents colServidor As DataGridViewTextBoxColumn
    Friend WithEvents colBanco As DataGridViewTextBoxColumn
    Friend WithEvents btnAdicionar As Button
    Friend WithEvents btnEditar As Button
    Friend WithEvents btnRemover As Button
    Friend WithEvents btnFechar As Button
End Class
