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

        Dim config = JsonSerializer.Deserialize(Of Configuracoes)(json)
        MigrarConexaoUnica(config)

        Return config

    End Function

    ''' <summary>
    ''' Upgrade transparente de um config.json de antes do suporte a múltiplos
    ''' bancos: se <see cref="Configuracoes.Conexoes"/> ainda estiver vazia mas
    ''' os campos legados de conexão única tiverem algo preenchido, cria uma
    ''' única <see cref="ConexaoBanco"/> ("Padrão") a partir deles. Não grava
    ''' nada em disco aqui — só preenche o objeto em memória; a próxima
    ''' chamada a <see cref="Salvar"/> (por qualquer tela) já persiste no
    ''' formato novo.
    ''' </summary>
    Private Shared Sub MigrarConexaoUnica(config As Configuracoes)
        If config.Conexoes Is Nothing Then
            config.Conexoes = New List(Of ConexaoBanco)
        End If

        If config.Conexoes.Count > 0 Then Exit Sub

        Dim temConexaoLegada =
            Not String.IsNullOrWhiteSpace(config.Servidor) OrElse
            Not String.IsNullOrWhiteSpace(config.Usuario)

        If Not temConexaoLegada Then Exit Sub

        config.Conexoes.Add(New ConexaoBanco With {
            .Nome = "Padrão",
            .Servidor = config.Servidor,
            .Porta = config.Porta,
            .Banco = config.Banco,
            .Usuario = config.Usuario,
            .Senha = config.Senha
        })
    End Sub

End Class