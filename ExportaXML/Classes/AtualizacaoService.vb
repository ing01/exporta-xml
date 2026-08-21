Imports System.IO
Imports Velopack
Imports Velopack.Sources

''' <summary>
''' Serviço de atualização automática do aplicativo via Velopack, usando os
''' Releases do GitHub como origem dos pacotes.
''' </summary>
''' <remarks>
''' IMPORTANTE: todos os métodos públicos aqui são bloqueantes de propósito
''' (usam .GetAwaiter().GetResult() internamente) e SEMPRE devem ser chamados
''' de dentro de um Task.Run(...) a partir da tela (nunca diretamente na
''' thread de interface) — chamar direto na UI trava o aplicativo, porque o
''' Velopack tenta retomar na mesma thread que está bloqueada esperando por
''' ele (deadlock clássico de UI + código async mal isolado).
''' </remarks>
Public Class AtualizacaoService

    Public Const RepositorioGitHub As String = "https://github.com/ing01/exporta-xml"

    Private Shared ReadOnly CaminhoLog As String =
        Path.Combine(LogService.PastaLogs, "Atualizacao.log")

    ''' <summary>
    ''' Cria o gerenciador do Velopack apontado para os Releases públicos do
    ''' repositório no GitHub (nenhuma autenticação é usada aqui — o repositório
    ''' é público, então qualquer instalação consegue consultar sozinha).
    ''' </summary>
    Private Shared Function ObterGerenciador() As UpdateManager
        Return New UpdateManager(New GithubSource(RepositorioGitHub, Nothing, False))
    End Function

    ''' <summary>
    ''' Consulta o GitHub Releases e verifica se existe uma versão mais nova que
    ''' a instalada.
    ''' </summary>
    ''' <returns>
    ''' O <see cref="UpdateInfo"/> da versão nova disponível, ou Nothing se:
    ''' não houver atualização nova; o app não tiver sido instalado via Velopack
    ''' (ex.: rodando direto pelo Visual Studio, onde <c>IsInstalled</c> é sempre
    ''' False); ou a verificação falhar (sem internet, GitHub fora do ar etc. —
    ''' nesses casos o erro fica registrado no log, mas não é lançado pra cima).
    ''' </returns>
    Public Shared Function VerificarAtualizacao() As UpdateInfo
        Try
            Dim mgr = ObterGerenciador()

            If Not mgr.IsInstalled Then
                Return Nothing
            End If

            Dim novaVersao = mgr.CheckForUpdatesAsync().GetAwaiter().GetResult()

            If novaVersao IsNot Nothing Then
                LogService.Registrar(CaminhoLog, $"Nova versão encontrada: {novaVersao.TargetFullRelease.Version}")
            End If

            Return novaVersao
        Catch ex As Exception
            LogService.Registrar(CaminhoLog, "ERRO ao verificar atualização: " & ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Baixa o pacote da versão informada e aplica a atualização.
    ''' </summary>
    ''' <param name="info">O resultado de uma chamada anterior a <see cref="VerificarAtualizacao"/>.</param>
    ''' <remarks>
    ''' Se der tudo certo, <c>ApplyUpdatesAndRestart</c> NÃO retorna: o processo
    ''' atual é encerrado ali mesmo e a versão nova é iniciada no lugar (é assim
    ''' que qualquer atualizador troca um .exe que está em uso — não tem como
    ''' evitar o fechar/reabrir). Qualquer falha no meio do caminho (sem
    ''' internet durante o download, por exemplo) só é registrada no log; o
    ''' aplicativo continua rodando na versão antiga normalmente.
    ''' </remarks>
    Public Shared Sub BaixarEAplicar(info As UpdateInfo)
        Try
            Dim mgr = ObterGerenciador()

            LogService.Registrar(CaminhoLog, $"Baixando atualização {info.TargetFullRelease.Version}...")
            mgr.DownloadUpdatesAsync(info).GetAwaiter().GetResult()

            LogService.Registrar(CaminhoLog, "Download concluído. Aplicando e reiniciando o aplicativo...")
            mgr.ApplyUpdatesAndRestart(info)
        Catch ex As Exception
            LogService.Registrar(CaminhoLog, "ERRO ao baixar/aplicar atualização: " & ex.ToString())
        End Try
    End Sub

End Class
