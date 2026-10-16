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
        SyncLock TravaArquivo
            Dim pasta As String = Path.GetDirectoryName(Caminho)
            If Not Directory.Exists(pasta) Then
                Directory.CreateDirectory(pasta)
            End If

            Dim json As String =
                JsonSerializer.Serialize(config,
                New JsonSerializerOptions With {
                    .WriteIndented = True
                })

            ' Grava num arquivo temporário e troca no final: quem estiver lendo ao
            ' mesmo tempo (o app e o Windows Service dividem este arquivo) nunca
            ' vê um JSON pela metade. O arquivo anterior vira o .bak.
            Dim temporario As String = Caminho & ".tmp"
            File.WriteAllText(temporario, json)

            If File.Exists(Caminho) Then
                File.Replace(temporario, Caminho, CaminhoBackup)
            Else
                File.Move(temporario, Caminho)
            End If

            ' O Replace/Move deixa o arquivo com o dono de quem gravou por último (app OU
            ' serviço). Sem isto, depois que o serviço salvava a competência, o app do
            ' usuário não conseguia mais salvar configurações.
            PermissoesArquivo.LiberarParaUsuarios(Caminho)
            PermissoesArquivo.LiberarParaUsuarios(CaminhoBackup)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Associa a um banco os cadastros ANTIGOS de destinatário/e-mail (que só
    ''' tinham o código da empresa). Só associa quando o código existe em UM único
    ''' banco; se o mesmo código aparece em mais de um banco, é ambíguo (não dá
    ''' para saber a qual empresa o cadastro se destinava) e o cadastro continua
    ''' sem uso até ser recadastrado — melhor isso do que enviar o XML para a
    ''' empresa errada. Chame SÓ quando conseguiu listar as empresas de TODOS os
    ''' bancos (se um banco estava fora do ar, a lista está incompleta).
    ''' </summary>
    ''' <returns>(quantos foram associados, quantos continuam ambíguos)</returns>
    Public Shared Function MigrarEscopoLegado(empresasDeTodosOsBancos As IEnumerable(Of EmpresaItem)) As (Associados As Integer, Ambiguos As Integer)
        SyncLock TravaArquivo
            Dim cfg = Carregar()
            If cfg.Conexoes Is Nothing OrElse cfg.Conexoes.Count <= 1 Then Return (0, 0)

            Dim empresas = empresasDeTodosOsBancos.Where(Function(e) e.Codigo <> 0 AndAlso e.Conexao IsNot Nothing).ToList()
            Dim associados As Integer = 0, ambiguos As Integer = 0

            Dim resolver As Func(Of Integer, String) =
                Function(codigo)
                    Dim bancos = empresas.Where(Function(e) e.Codigo = codigo).Select(Function(e) e.Conexao.Nome).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    Return If(bancos.Count = 1, bancos(0), Nothing)
                End Function

            If cfg.DestinatariosLocais IsNot Nothing Then
                For Each d In cfg.DestinatariosLocais.Where(Function(x) x.CodigoEmpresa <> 0 AndAlso String.IsNullOrWhiteSpace(x.NomeBanco))
                    Dim banco = resolver(d.CodigoEmpresa)
                    If banco IsNot Nothing Then
                        d.NomeBanco = banco : associados += 1
                    Else
                        ambiguos += 1
                    End If
                Next
            End If

            If cfg.ConfiguracoesEmailPorEmpresa IsNot Nothing Then
                For Each c In cfg.ConfiguracoesEmailPorEmpresa.Where(Function(x) x.CodigoEmpresa <> 0 AndAlso String.IsNullOrWhiteSpace(x.NomeBanco))
                    Dim banco = resolver(c.CodigoEmpresa)
                    If banco IsNot Nothing Then
                        c.NomeBanco = banco : associados += 1
                    Else
                        ambiguos += 1
                    End If
                Next
            End If

            If associados > 0 Then Salvar(cfg)
            Return (associados, ambiguos)
        End SyncLock
    End Function

    ''' <summary>Trava de escrita/leitura dentro do mesmo processo (entre processos, a troca atômica do arquivo cobre o caso).</summary>
    Private Shared ReadOnly TravaArquivo As New Object()

    Private Shared ReadOnly CaminhoBackup As String = Caminho & ".bak"

    ''' <summary>
    ''' Preenchido quando <see cref="Carregar"/> precisou descartar um config.json
    ''' ilegível (corrompido). A tela principal mostra uma vez, em português, e
    ''' limpa com <see cref="ConsumirAvisoLeitura"/>.
    ''' </summary>
    Private Shared _avisoLeitura As String

    Public Shared Function ConsumirAvisoLeitura() As String
        Dim aviso = _avisoLeitura
        _avisoLeitura = Nothing
        Return aviso
    End Function

    ''' <summary>
    ''' Atualiza SÓ a competência executada, relendo o arquivo atual antes de
    ''' gravar. A execução mensal demora minutos; gravar o objeto
    ''' <c>cfg</c> carregado no início sobrescreveria qualquer coisa que o
    ''' usuário tenha mudado na tela nesse tempo.
    ''' </summary>
    Public Shared Sub SalvarCompetenciaExecutada(competencia As String)
        SyncLock TravaArquivo
            Dim atual = Carregar()
            atual.UltimaCompetenciaExecutada = competencia
            Salvar(atual)
        End SyncLock
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

        Dim config As Configuracoes = LerArquivoComFallback()
        If config Is Nothing Then config = New Configuracoes()
        MigrarConexaoUnica(config)

        ' Migração: se havia um destinatário legado em UltimoDestinatario, converte para DestinatariosLocais (Global) para não perder o valor após atualização
        If config.DestinatariosLocais Is Nothing Then
            config.DestinatariosLocais = New List(Of DestinatarioLocal)()
        End If

        If config.ConfiguracoesEmailPorEmpresa Is Nothing Then
            config.ConfiguracoesEmailPorEmpresa = New List(Of ConfiguracaoEmailEmpresa)()
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
    ''' Lê o config.json; se estiver ilegível (corrompido, vazio, truncado),
    ''' tenta o .bak; se esse também falhar, guarda uma cópia do arquivo ruim
    ''' (.corrompido-data) e devolve uma configuração em branco — o programa
    ''' abre normalmente e avisa o usuário em português, em vez de dar erro.
    ''' </summary>
    Private Shared Function LerArquivoComFallback() As Configuracoes
        Dim erroOriginal As Exception = Nothing

        ' Outro processo pode estar no meio da troca do arquivo: tenta algumas vezes antes de desistir.
        For tentativa = 1 To 3
            Try
                Return JsonSerializer.Deserialize(Of Configuracoes)(File.ReadAllText(Caminho))
            Catch ex As IOException
                erroOriginal = ex
                Threading.Thread.Sleep(150)
            Catch ex As JsonException
                erroOriginal = ex
                Exit For
            End Try
        Next

        LogService.Registrar(Path.Combine(LogService.PastaLogs, "Configuracao.log"),
                             $"config.json ilegível: {ErroAmigavel.Traduzir(erroOriginal)} | Técnico: {erroOriginal}")

        Try
            If File.Exists(CaminhoBackup) Then
                Dim doBackup = JsonSerializer.Deserialize(Of Configuracoes)(File.ReadAllText(CaminhoBackup))
                If doBackup IsNot Nothing Then
                    _avisoLeitura = "O arquivo de configuração estava danificado. Foi restaurada a cópia de segurança anterior; confira se as configurações estão como esperado."
                    Return doBackup
                End If
            End If
        Catch
        End Try

        Try
            File.Copy(Caminho, Caminho & $".corrompido-{DateTime.Now:yyyyMMdd-HHmmss}")
        Catch
        End Try

        _avisoLeitura = "O arquivo de configuração estava danificado e não pôde ser recuperado. As configurações (bancos, e-mail e agendamento) precisam ser feitas de novo. Uma cópia do arquivo antigo foi guardada na pasta ExportaXML do ProgramData."
        Return Nothing
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