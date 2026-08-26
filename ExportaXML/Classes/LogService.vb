Imports System.IO
Imports Microsoft.Extensions.Hosting.WindowsServices

''' <summary>
''' Log em arquivo simples (texto). Usado pelo Agendamento e pela Atualização
''' automática — as duas rotinas que rodam sozinhas, sem ninguém olhando a tela
''' na hora — e também pelo log de Atividade (<see cref="RegistrarAtividade"/>),
''' que registra as principais ações do usuário na tela (nada a ver com envio
''' de e-mail especificamente — e-mail é só uma das ações registradas).
''' </summary>
Public Class LogService

    ''' <summary>
    ''' Protege <see cref="Registrar"/> contra escrita concorrente no mesmo
    ''' arquivo — necessário desde que <see cref="TelemetriaService"/> passou
    ''' a chamar o log de várias threads ao mesmo tempo (um Task.Run por
    ''' empresa). Sem isso, duas chamadas simultâneas podem colidir no
    ''' File.AppendAllText (IOException "arquivo em uso"), abortando a
    ''' chamada de quem perdeu a corrida.
    ''' </summary>
    Private Shared ReadOnly Trava As New Object()

    ''' <summary>
    ''' Pasta base de todos os logs: %LocalAppData%\ExportaXML\Logs no app
    ''' interativo; %ProgramData%\ExportaXML\Logs quando rodando como Windows
    ''' Service (LocalSystem não tem um %LocalAppData% de usuário de verdade).
    ''' </summary>
    ''' <remarks>
    ''' Fica FORA da pasta de instalação de propósito: com o atualizador
    ''' automático (Velopack), a pasta do executável (Application.StartupPath) é
    ''' substituída a cada atualização — um log guardado lá seria apagado
    ''' exatamente quando mais importa (durante uma atualização). Isso já foi um
    ''' bug real neste projeto antes de mover pra cá; não volte a usar
    ''' Application.StartupPath para logs.
    ''' </remarks>
    Public Shared ReadOnly PastaLogs As String =
        Path.Combine(
            Environment.GetFolderPath(
                If(WindowsServiceHelpers.IsWindowsService(),
                   Environment.SpecialFolder.CommonApplicationData,
                   Environment.SpecialFolder.LocalApplicationData)),
            "ExportaXML", "Logs")

    ''' <summary>
    ''' Acrescenta uma linha com data/hora ao arquivo de log informado, criando a
    ''' pasta e o arquivo se ainda não existirem. Nunca sobrescreve — cada
    ''' chamada só adiciona uma linha no final.
    ''' </summary>
    ''' <param name="caminhoArquivo">Caminho completo do arquivo .log de destino.</param>
    ''' <param name="mensagem">Texto da linha (a data/hora é adicionada automaticamente).</param>
    Public Shared Sub Registrar(caminhoArquivo As String, mensagem As String)
        SyncLock Trava
            Dim pasta As String = Path.GetDirectoryName(caminhoArquivo)

            If Not String.IsNullOrEmpty(pasta) AndAlso Not Directory.Exists(pasta) Then
                Directory.CreateDirectory(pasta)
            End If

            Dim linha As String = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {mensagem}"
            File.AppendAllText(caminhoArquivo, linha & Environment.NewLine)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Arquivo de log de atividade do mês atual — um arquivo por mês (mesmo
    ''' padrão do log de Agendamento), com as principais ações do usuário na
    ''' tela: pesquisar, exportar, testar conexão/envio/agendamento, configurar
    ''' banco/e-mail, verificar atualização, etc. Não registra cada clique em
    ''' checkbox/radio/campo de texto — só ações que de fato fazem algo.
    ''' </summary>
    Public Shared ReadOnly Property CaminhoLogAtividade As String
        Get
            Return Path.Combine(PastaLogs, $"Atividade_{DateTime.Now:yyyy-MM}.log")
        End Get
    End Property

    ''' <summary>Atalho para <see cref="Registrar"/> já apontando pro log de atividade do mês atual.</summary>
    Public Shared Sub RegistrarAtividade(mensagem As String)
        Registrar(CaminhoLogAtividade, mensagem)
    End Sub

End Class
