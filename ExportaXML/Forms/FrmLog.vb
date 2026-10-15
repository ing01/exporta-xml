Imports System.IO

''' <summary>
''' Tela "Ver log" — mostra pro usuário o que o agendamento (e as demais ações
''' do programa) registraram em <see cref="LogService.PastaLogs"/>, sem precisar
''' abrir o Explorer. No topo, destaca a última falha pendente do agendamento
''' (<see cref="PendenciaAgendamentoService"/>), se houver.
''' </summary>
''' <remarks>
''' Montada por código (sem Designer) de propósito: é só uma caixa de texto
''' somente-leitura, um combo de arquivos e alguns botões.
''' </remarks>
Public Class FrmLog
    Inherits Form

    Private ReadOnly lblAviso As New Label()
    Private ReadOnly cboArquivos As New ComboBox()
    Private ReadOnly txtLog As New TextBox()
    Private ReadOnly btnAtualizar As New Button()
    Private ReadOnly btnPasta As New Button()
    Private ReadOnly btnCopiar As New Button()
    Private ReadOnly btnFechar As New Button()

    Public Sub New()
        Text = "Log do aplicativo"
        Font = New Font("Arial", 9.0F)
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(760, 520)
        MinimumSize = New Size(560, 380)

        lblAviso.Location = New Point(12, 10)
        lblAviso.Size = New Size(736, 40)
        lblAviso.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblAviso.Font = New Font("Arial", 9.0F, FontStyle.Bold)

        cboArquivos.Location = New Point(12, 56)
        cboArquivos.Size = New Size(736, 23)
        cboArquivos.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cboArquivos.DropDownStyle = ComboBoxStyle.DropDownList

        txtLog.Location = New Point(12, 88)
        txtLog.Size = New Size(736, 384)
        txtLog.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtLog.Multiline = True
        txtLog.ReadOnly = True
        txtLog.ScrollBars = ScrollBars.Both
        txtLog.WordWrap = False
        txtLog.Font = New Font("Consolas", 9.0F)

        ConfigurarBotao(btnAtualizar, "Atualizar", 12, AnchorStyles.Bottom Or AnchorStyles.Left)
        ConfigurarBotao(btnPasta, "Abrir pasta", 112, AnchorStyles.Bottom Or AnchorStyles.Left)
        ConfigurarBotao(btnCopiar, "Copiar", 212, AnchorStyles.Bottom Or AnchorStyles.Left)
        ConfigurarBotao(btnFechar, "Fechar", 648, AnchorStyles.Bottom Or AnchorStyles.Right)

        Controls.AddRange(New Control() {lblAviso, cboArquivos, txtLog, btnAtualizar, btnPasta, btnCopiar, btnFechar})

        AddHandler Load, AddressOf FrmLog_Load
        AddHandler cboArquivos.SelectedIndexChanged, Sub(s, e) MostrarArquivoSelecionado()
        AddHandler btnAtualizar.Click, Sub(s, e) CarregarArquivos()
        AddHandler btnCopiar.Click, Sub(s, e)
                                        If txtLog.TextLength > 0 Then Clipboard.SetText(txtLog.Text)
                                    End Sub
        AddHandler btnFechar.Click, Sub(s, e) Close()
        AddHandler btnPasta.Click, AddressOf btnPasta_Click
    End Sub

    Private Sub ConfigurarBotao(btn As Button, texto As String, x As Integer, ancora As AnchorStyles)
        btn.Text = texto
        btn.Location = New Point(x, 482)
        btn.Size = New Size(100, 28)
        btn.Anchor = ancora
        btn.UseVisualStyleBackColor = True
    End Sub

    Private Sub FrmLog_Load(sender As Object, e As EventArgs)
        CarregarArquivos()
    End Sub

    Private Sub CarregarArquivos()
        Dim pendencia = PendenciaAgendamentoService.Obter()
        If pendencia IsNot Nothing AndAlso pendencia.HaFalha Then
            lblAviso.ForeColor = Color.Firebrick
            lblAviso.Text = $"Última falha do agendamento ({pendencia.Competencia}, {pendencia.DataHora:dd/MM/yyyy HH:mm}): {pendencia.Mensagem}"
        Else
            lblAviso.ForeColor = SystemColors.GrayText
            lblAviso.Text = "Nenhuma falha pendente no agendamento."
        End If

        Dim anterior As String = TryCast(cboArquivos.SelectedItem, String)

        cboArquivos.Items.Clear()
        If Directory.Exists(LogService.PastaLogs) Then
            ' Mais recentes primeiro. Agendamento_*.log e Atividade_*.log.
            For Each arq In New DirectoryInfo(LogService.PastaLogs).GetFiles("*.log").OrderByDescending(Function(f) f.LastWriteTime)
                cboArquivos.Items.Add(arq.Name)
            Next
        End If

        If cboArquivos.Items.Count = 0 Then
            txtLog.Text = "Nenhum log encontrado ainda." & Environment.NewLine &
                          $"Pasta: {LogService.PastaLogs}"
            Return
        End If

        Dim idx As Integer = If(anterior Is Nothing, -1, cboArquivos.Items.IndexOf(anterior))
        cboArquivos.SelectedIndex = If(idx >= 0, idx, 0)
    End Sub

    Private Sub MostrarArquivoSelecionado()
        Dim nome As String = TryCast(cboArquivos.SelectedItem, String)
        If nome Is Nothing Then Return

        Try
            ' FileShare.ReadWrite: o agendamento pode estar gravando no mesmo arquivo agora.
            Using fs As New FileStream(Path.Combine(LogService.PastaLogs, nome), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(fs)
                    txtLog.Text = sr.ReadToEnd()
                End Using
            End Using
            txtLog.SelectionStart = txtLog.TextLength
            txtLog.ScrollToCaret()
        Catch ex As Exception
            txtLog.Text = $"Não foi possível ler o log: {ErroAmigavel.Traduzir(ex)}"
        End Try
    End Sub

    Private Sub btnPasta_Click(sender As Object, e As EventArgs)
        Try
            Directory.CreateDirectory(LogService.PastaLogs)
            Process.Start(New ProcessStartInfo("explorer.exe", $"""{LogService.PastaLogs}""") With {.UseShellExecute = True})
        Catch ex As Exception
            MessageBox.Show($"Não foi possível abrir a pasta: {ErroAmigavel.Traduzir(ex)}", "Log", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
