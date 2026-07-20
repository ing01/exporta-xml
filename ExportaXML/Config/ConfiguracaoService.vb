Imports System.IO
Imports System.Text.Json

Public Class ConfiguracaoService

    Private Shared ReadOnly Caminho As String =
        Path.Combine(Application.StartupPath, "config.json")

    Public Shared Sub Salvar(config As Configuracao)

        Dim json As String =
            JsonSerializer.Serialize(config,
            New JsonSerializerOptions With {
                .WriteIndented = True
            })

        File.WriteAllText(Caminho, json)

    End Sub

    Public Shared Function Carregar() As Configuracao

        If Not File.Exists(Caminho) Then
            Return New Configuracao()
        End If

        Dim json As String = File.ReadAllText(Caminho)

        Return JsonSerializer.Deserialize(Of Configuracao)(json)

    End Function

End Class