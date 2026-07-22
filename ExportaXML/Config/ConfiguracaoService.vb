Imports System.IO
Imports System.Text.Json

Public Class ConfiguracaoService

    Private Shared ReadOnly Caminho As String =
        Path.Combine(Application.StartupPath, "config.json")

    Public Shared Sub Salvar(config As Configuracoes)

        Dim json As String =
            JsonSerializer.Serialize(config,
            New JsonSerializerOptions With {
                .WriteIndented = True
            })

        File.WriteAllText(Caminho, json)

    End Sub

    Public Shared Function Carregar() As Configuracoes

        If Not File.Exists(Caminho) Then
            Return New Configuracoes()
        End If

        Dim json As String = File.ReadAllText(Caminho)

        Return JsonSerializer.Deserialize(Of Configuracoes)(json)

    End Function

End Class