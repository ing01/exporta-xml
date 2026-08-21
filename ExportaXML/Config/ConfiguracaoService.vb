Imports System.IO
Imports System.Text.Json

''' <summary>
''' Persiste as configurações do aplicativo (conexão, e-mail, agendamento,
''' últimas escolhas do usuário) num arquivo "config.json" simples ao lado do
''' executável — não usa banco de dados nem registro do Windows.
''' </summary>
''' <remarks>
''' ATENÇÃO: "config.json" fica em <see cref="Application.StartupPath"/>, ou seja,
''' na pasta da versão instalada pelo Velopack. Diferente dos logs (que foram
''' movidos para %LocalAppData% justamente por isso), esse arquivo SOBREVIVE a
''' atualizações porque o instalador do Velopack não apaga arquivos que não
''' fazem parte do pacote publicado — mas se algum dia a estratégia de update
''' mudar para "pasta limpa a cada versão", isso precisa ser revisto.
''' </remarks>
Public Class ConfiguracaoService

    Private Shared ReadOnly Caminho As String =
        Path.Combine(Application.StartupPath, "config.json")

    ''' <summary>
    ''' Salva (sobrescrevendo) o config.json inteiro a partir do objeto informado.
    ''' Sempre grave a partir de um <see cref="Carregar"/> recente para não perder
    ''' campos alterados por outra parte do código entre o load e o save.
    ''' </summary>
    Public Shared Sub Salvar(config As Configuracoes)

        Dim json As String =
            JsonSerializer.Serialize(config,
            New JsonSerializerOptions With {
                .WriteIndented = True
            })

        File.WriteAllText(Caminho, json)

    End Sub

    ''' <summary>
    ''' Lê o config.json e retorna um objeto novo. Se o arquivo não existir ainda
    ''' (primeira execução), retorna uma <see cref="Configuracoes"/> com os valores
    ''' padrão do construtor, sem lançar erro.
    ''' </summary>
    Public Shared Function Carregar() As Configuracoes

        If Not File.Exists(Caminho) Then
            Return New Configuracoes()
        End If

        Dim json As String = File.ReadAllText(Caminho)

        Return JsonSerializer.Deserialize(Of Configuracoes)(json)

    End Function

End Class