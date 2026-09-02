Imports System.IO
Imports System.Text.Json

''' <summary>
''' Persiste as configurações do aplicativo (conexão, e-mail, agendamento,
''' últimas escolhas do usuário) num arquivo "config.json" simples — não usa
''' banco de dados nem registro do Windows.
''' </summary>
''' <remarks>
''' ATENÇÃO: "config.json" fica em %ProgramData%\ExportaXML\, um local FIXO,
''' independente de qual cópia do executável está rodando — mesmo raciocínio
''' de <see cref="LogService.PastaLogs"/>. Isso é essencial porque o app
''' interativo (rodando de "current\", atualizado pelo Velopack) e o Windows
''' Service (rodando de uma cópia própria em "Servico\", ver
''' <see cref="ServicoWindowsService"/>) são processos diferentes, em pastas
''' diferentes — se o config.json ficasse "ao lado do executável"
''' (<see cref="Application.StartupPath"/>, como era antes), cada um leria um
''' arquivo diferente, e o serviço NUNCA veria o que foi configurado pela
''' tela (bug real, já aconteceu). <see cref="MigrarConfigAntiga"/> resgata
''' automaticamente um config.json de uma versão anterior a essa mudança.
''' </remarks>
Public Class ConfiguracaoService

    Private Shared ReadOnly Caminho As String =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ExportaXML", "config.json")

    ''' <summary>Local antigo (ao lado do executável) — só usado por <see cref="MigrarConfigAntiga"/>.</summary>
    Private Shared ReadOnly CaminhoAntigo As String =
        Path.Combine(Application.StartupPath, "config.json")

    ''' <summary>
    ''' Salva (sobrescrevendo) o config.json inteiro a partir do objeto informado.
    ''' Sempre grave a partir de um <see cref="Carregar"/> recente para não perder
    ''' campos alterados por outra parte do código entre o load e o save.
    ''' </summary>
    Public Shared Sub Salvar(config As Configuracoes)

        Dim pasta As String = Path.GetDirectoryName(Caminho)
        If Not Directory.Exists(pasta) Then
            Directory.CreateDirectory(pasta)
        End If

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

        MigrarConfigAntiga()

        If Not File.Exists(Caminho) Then
            Return New Configuracoes()
        End If

        Dim json As String = File.ReadAllText(Caminho)

        Dim config = JsonSerializer.Deserialize(Of Configuracoes)(json)
        MigrarConexaoUnica(config)

        ' Migração: se havia um destinatário legado em UltimoDestinatario, converte para DestinatariosLocais (Global) para não perder o valor após atualização
        If config.DestinatariosLocais Is Nothing Then
            config.DestinatariosLocais = New List(Of DestinatarioLocal)()
        End If

        If (config.DestinatariosLocais.Count = 0) AndAlso Not String.IsNullOrWhiteSpace(config.UltimoDestinatario) Then
            Try
                config.DestinatariosLocais.Add(New DestinatarioLocal With {
                    .CodigoEmpresa = 0,
                    .Email = config.UltimoDestinatario.Trim(),
                    .Descricao = "Migrado do campo destinatário antigo",
                    .Ativo = True
                })
                ' Salva a migração imediatamente para que a UI passe a mostrar esta entrada
                Salvar(config)
            Catch
                ' Silencioso: se falhar a gravação, ainda retornamos o objeto em memória
            End Try
        End If

        Return config

    End Function

    ''' <summary>
    ''' Se o config.json novo (%ProgramData%) ainda não existir, mas houver um
    ''' antigo (ao lado do executável de ONDE ESTE PROCESSO está rodando),
    ''' copia pra cá — resgata a configuração já feita ao atualizar de uma
    ''' versão anterior a essa mudança. Melhor esforço: qualquer falha aqui
    ''' (permissão, etc.) simplesmente deixa <see cref="Carregar"/> seguir
    ''' pro caminho normal (config nova, em branco).
    ''' </summary>
    Private Shared Sub MigrarConfigAntiga()
        If File.Exists(Caminho) Then Return
        If Not File.Exists(CaminhoAntigo) Then Return

        Try
            Dim pasta As String = Path.GetDirectoryName(Caminho)
            If Not Directory.Exists(pasta) Then
                Directory.CreateDirectory(pasta)
            End If

            File.Copy(CaminhoAntigo, Caminho)
        Catch
        End Try
    End Sub

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