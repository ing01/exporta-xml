Imports System.IO

''' <summary>
''' Log em arquivo simples (texto), usado pelo Agendamento e pela Atualização
''' automática — as duas rotinas que rodam sozinhas, sem ninguém olhando a tela
''' na hora, então precisam deixar rastro em algum lugar.
''' </summary>
Public Class LogService

    ''' <summary>
    ''' Pasta base de todos os logs: %LocalAppData%\ExportaXML\Logs.
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
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ExportaXML", "Logs")

    ''' <summary>
    ''' Acrescenta uma linha com data/hora ao arquivo de log informado, criando a
    ''' pasta e o arquivo se ainda não existirem. Nunca sobrescreve — cada
    ''' chamada só adiciona uma linha no final.
    ''' </summary>
    ''' <param name="caminhoArquivo">Caminho completo do arquivo .log de destino.</param>
    ''' <param name="mensagem">Texto da linha (a data/hora é adicionada automaticamente).</param>
    Public Shared Sub Registrar(caminhoArquivo As String, mensagem As String)
        Dim pasta As String = Path.GetDirectoryName(caminhoArquivo)

        If Not String.IsNullOrEmpty(pasta) AndAlso Not Directory.Exists(pasta) Then
            Directory.CreateDirectory(pasta)
        End If

        Dim linha As String = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {mensagem}"
        File.AppendAllText(caminhoArquivo, linha & Environment.NewLine)
    End Sub

End Class
