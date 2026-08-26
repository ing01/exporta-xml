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
    ''' <remarks>
    ''' Se a variável de ambiente EXPORTAXML_UPDATE_SOURCE_LOCAL estiver
    ''' definida (apontando pra uma pasta local com pacotes gerados por
    ''' "vpk pack"), usa essa pasta como origem em vez do GitHub — só pra
    ''' testar o ciclo real de atualização (baixar/aplicar/reiniciar) sem
    ''' precisar publicar nada de verdade. Não afeta instalações de clientes
    ''' (essa variável nunca existe fora de uma máquina de teste).
    ''' </remarks>
    Private Shared Function ObterGerenciador() As UpdateManager
        Dim pastaTeste = Environment.GetEnvironmentVariable("EXPORTAXML_UPDATE_SOURCE_LOCAL")
        If Not String.IsNullOrWhiteSpace(pastaTeste) Then
            Return New UpdateManager(New SimpleFileSource(New DirectoryInfo(pastaTeste)))
        End If

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
                LogService.Registrar(CaminhoLog, "Verificação pulada: aplicativo não foi instalado via atualizador (IsInstalled=False).")
                Return Nothing
            End If

            Dim novaVersao = mgr.CheckForUpdatesAsync().GetAwaiter().GetResult()

            If novaVersao IsNot Nothing Then
                LogService.Registrar(CaminhoLog, $"Verificação: versão instalada {mgr.CurrentVersion}, nova versão encontrada: {novaVersao.TargetFullRelease.Version}.")
            Else
                LogService.Registrar(CaminhoLog, $"Verificação: versão instalada {mgr.CurrentVersion}, nenhuma atualização disponível.")
            End If

            Return novaVersao
        Catch ex As Exception
            LogService.Registrar(CaminhoLog, "ERRO ao verificar atualização: " & ex.ToString())
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Baixa o pacote da versão informada e aplica a atualização.
    ''' </summary>
    ''' <param name="info">O resultado de uma chamada anterior a <see cref="VerificarAtualizacao"/>.</param>
    ''' <remarks>
    ''' Usa <c>WaitExitThenApplyUpdates</c> (não <c>ApplyUpdatesAndRestart</c>):
    ''' esse método lança um processo atualizador EXTERNO que fica esperando
    ''' este processo terminar de sair (até 60s) antes de aplicar a atualização
    ''' e reabrir — diferente de <c>ApplyUpdatesAndRestart</c>, que tenta fazer
    ''' tudo isso (aplicar + reabrir) de dentro do próprio processo que está
    ''' sendo substituído. Essa segunda forma tem um problema conhecido do
    ''' Velopack (github.com/velopack/velopack/issues/195): o app fecha mas às
    ''' vezes não reabre — foi exatamente o que aconteceu aqui. Depois de
    ''' avisar o atualizador, este método força o fechamento do processo atual
    ''' (<see cref="Application.Exit"/> + <see cref="Environment.Exit"/>) — sem
    ''' isso, o atualizador externo ficaria esperando os 60s à toa.
    ''' </remarks>
    Public Shared Sub BaixarEAplicar(info As UpdateInfo)
        Try
            Dim mgr = ObterGerenciador()

            LogService.Registrar(CaminhoLog, $"Baixando atualização {info.TargetFullRelease.Version}...")
            mgr.DownloadUpdatesAsync(info).GetAwaiter().GetResult()

            LogService.Registrar(CaminhoLog, "Download concluído. Avisando o atualizador e encerrando o aplicativo...")
            mgr.WaitExitThenApplyUpdates(info, silent:=True, restart:=True)

            Application.Exit()
            Environment.Exit(0)
        Catch ex As Exception
            LogService.Registrar(CaminhoLog, "ERRO ao baixar/aplicar atualização: " & ex.ToString())
        End Try
    End Sub

End Class
