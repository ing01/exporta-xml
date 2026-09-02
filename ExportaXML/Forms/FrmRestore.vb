Imports System.Net.Http
Imports System.IO
Imports System.IO.Compression
Imports System.ServiceProcess
Imports System.Text.Json

Public Class FrmRestore

    Private ReadOnly owner As String = "ing01"
    Private ReadOnly repo As String = "exporta-xml"

    Private Async Sub FrmRestore_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Await LoadReleasesAsync()
    End Sub

    Private Async Function LoadReleasesAsync() As Threading.Tasks.Task
        lbReleases.Items.Clear()
        lblStatus.Text = "Carregando versões..."
        Try
            Using client As New HttpClient()
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ExportaXML-Restore-Agent")
                ' Tenta listar releases; se não houver, lista tags
                Dim url As String = $"https://api.github.com/repos/{owner}/{repo}/releases"
                Dim resp = Await client.GetAsync(url)
                If resp.IsSuccessStatusCode Then
                    Dim json = Await resp.Content.ReadAsStringAsync()
                    ' Parse JSON safely para releases (campo tag_name)
                    Try
                        Using doc = JsonDocument.Parse(json)
                            If doc.RootElement.ValueKind = JsonValueKind.Array Then
                                For Each el In doc.RootElement.EnumerateArray()
                                    If el.TryGetProperty("tag_name", Nothing) Then
                                        Dim tag = el.GetProperty("tag_name").GetString()
                                        If Not String.IsNullOrWhiteSpace(tag) AndAlso Not lbReleases.Items.Contains(tag) Then lbReleases.Items.Add(tag)
                                    End If
                                Next
                            End If
                        End Using
                    Catch
                        ' se falhar, fallback para tags endpoint abaixo
                    End Try
                Else
                    ' fallback para tags
                    url = $"https://api.github.com/repos/{owner}/{repo}/tags"
                    resp = Await client.GetAsync(url)
                    If resp.IsSuccessStatusCode Then
                        Dim json2 = Await resp.Content.ReadAsStringAsync()
                        Try
                            Using doc2 = JsonDocument.Parse(json2)
                                If doc2.RootElement.ValueKind = JsonValueKind.Array Then
                                    For Each el In doc2.RootElement.EnumerateArray()
                                        If el.TryGetProperty("name", Nothing) Then
                                            Dim tag = el.GetProperty("name").GetString()
                                            If Not String.IsNullOrWhiteSpace(tag) AndAlso Not lbReleases.Items.Contains(tag) Then lbReleases.Items.Add(tag)
                                        End If
                                    Next
                                End If
                            End Using
                        Catch
                        End Try
                    End If
                End If
            End Using
            If lbReleases.Items.Count = 0 Then
                lblStatus.Text = "Nenhuma versão encontrada"
            Else
                lblStatus.Text = $"{lbReleases.Items.Count} versões encontradas"
            End If
        Catch ex As Exception
            lblStatus.Text = "Falha ao listar versões: " & ex.Message
        End Try
    End Function

    Private Async Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Await LoadReleasesAsync()
    End Sub

    Private Async Sub btnRestore_Click(sender As Object, e As EventArgs) Handles btnRestore.Click
        If lbReleases.SelectedItem Is Nothing Then
            MessageBox.Show("Selecione uma versão para restaurar.")
            Exit Sub
        End If
        Dim tag As String = lbReleases.SelectedItem.ToString()
        If MessageBox.Show($"Restaurar versão {tag}? Isso substituirá os arquivos da instalação atual. Continuar?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Exit Sub
        End If

        lblStatus.Text = "Baixando..."
        Try
            Using client As New HttpClient()
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ExportaXML-Restore-Agent")
                Dim downloadUrl = $"https://github.com/{owner}/{repo}/archive/refs/tags/{tag}.zip"
                Dim tempZip = Path.Combine(Path.GetTempPath(), $"exporta_restore_{tag}.zip")
                Using s = Await client.GetStreamAsync(downloadUrl)
                    Using fs As New FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None)
                        Await s.CopyToAsync(fs)
                    End Using
                End Using

                lblStatus.Text = "Extraindo..."
                Dim extractDir = Path.Combine(Path.GetTempPath(), $"exporta_restore_{tag}")
                If Directory.Exists(extractDir) Then Directory.Delete(extractDir, True)
                ZipFile.ExtractToDirectory(tempZip, extractDir)

                ' A pasta do repo extraído normalmente tem nome repo-tag
                Dim extractedRoot = Directory.GetDirectories(extractDir).FirstOrDefault()
                If extractedRoot Is Nothing Then Throw New Exception("Conteúdo do zip inválido")

                ' Tentar parar serviço "ExportaXML" se existir
                Dim svcStopped As Boolean = False
                Try
                    Dim sc = New ServiceController("ExportaXML")
                    If sc.Status <> ServiceControllerStatus.Stopped Then
                        lblStatus.Text = "Parando serviço ExportaXML..."
                        sc.Stop()
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15))
                        svcStopped = True
                    End If
                Catch
                    ' não crítico — talvez não seja serviço, ou sem permissão
                End Try

                ' Copiar arquivos para pasta de instalação
                Dim target As String = Application.StartupPath
                lblStatus.Text = "Copiando arquivos para " & target
                Try
                    For Each srcPath In Directory.GetFiles(extractedRoot, "*", SearchOption.AllDirectories)
                        Dim rel = Path.GetRelativePath(extractedRoot, srcPath)
                        Dim destPath = Path.Combine(target, rel)
                        Dim destDir = Path.GetDirectoryName(destPath)
                        If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)
                        File.Copy(srcPath, destPath, True)
                    Next
                Catch ex As Exception
                    Throw New Exception("Falha ao copiar arquivos: " & ex.Message)
                End Try

                ' Reiniciar serviço se foi parado
                Try
                    If svcStopped Then
                        Dim sc2 = New ServiceController("ExportaXML")
                        sc2.Start()
                        sc2.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15))
                    End If
                Catch
                End Try

                lblStatus.Text = "Restauração concluída. Reinicie o aplicativo se necessário."
                MessageBox.Show("Restauração concluída.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Using
        Catch ex As Exception
            lblStatus.Text = "Erro: " & ex.Message
            MessageBox.Show("Falha ao restaurar versão: " & ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
