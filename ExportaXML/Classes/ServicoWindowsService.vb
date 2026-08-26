Imports System.Diagnostics
Imports System.IO
Imports System.Threading

''' <summary>
''' Instala/remove o ExportaXML como um Windows Service de verdade (visível em
''' services.msc), rodando o AgendamentoWorker em segundo plano mesmo sem
''' usuário logado. Espelha o estilo de <see cref="VigiaService"/> (roda uma
''' ferramenta de linha de comando do Windows via Process.Start), mas usando
''' sc.exe em vez de schtasks.exe — porque sc create/delete exigem elevação,
''' diferente de uma tarefa agendada do usuário atual (ver
''' <c>ExecutarComandoServico</c> em Program.vb, que garante essa elevação
''' antes de chamar <see cref="Instalar"/>/<see cref="Desinstalar"/>).
''' </summary>
''' <remarks>
''' O serviço NÃO roda direto de dentro de "current\" (a pasta que o Velopack
''' sobrescreve inteira a cada atualização automática) — ele roda a partir de
''' uma cópia própria em <see cref="PastaServico"/>. Sem isso, o serviço
''' ficaria com o executável sempre aberto, e o Velopack não conseguiria
''' sobrescrevê-lo numa atualização (Windows não deixa substituir um arquivo
''' em uso), quebrando o auto-update do app interativo. Rodar
''' "--instalar-servico" de novo depois de publicar uma versão nova (com o
''' app já atualizado) refaz essa cópia e recria o serviço com o código novo
''' — é assim que o serviço "atualiza".
''' </remarks>
Public Class ServicoWindowsService

    ''' <summary>Nome do serviço no SCM — usado tanto para criar quanto para consultar/remover.</summary>
    Public Const NomeServico As String = "ExportaXML"

    Private Const NomeExibicao As String = "ExportaXML - Exportação Automática"
    Private Const Descricao As String = "Executa a exportação e envio mensal automático de XMLs mesmo sem usuário logado."

    ''' <summary>Código de saída do sc.exe quando o serviço consultado não existe (ERROR_SERVICE_DOES_NOT_EXIST).</summary>
    Private Const CodigoServicoInexistente As Integer = 1060

    ''' <summary>
    ''' Pasta própria do serviço — uma cópia independente dos binários do app,
    ''' fora de "current\" (ver remarks da classe). %ProgramData% porque o
    ''' serviço roda como LocalSystem, sem %LocalAppData% de usuário de verdade.
    ''' </summary>
    Private Shared ReadOnly PastaServico As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ExportaXML", "Servico")

    Private Shared ReadOnly CaminhoExeServico As String =
        Path.Combine(PastaServico, "ExportaXML.exe")

    ''' <summary>
    ''' Roda o sc.exe com os argumentos informados e devolve (código de saída,
    ''' saída padrão + erro combinadas).
    ''' </summary>
    Private Shared Function RodarSc(argumentos As String) As (CodigoSaida As Integer, Saida As String)
        Dim psi As New ProcessStartInfo("sc.exe", argumentos) With {
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }

        Using processo = Process.Start(psi)
            Dim saida As String = processo.StandardOutput.ReadToEnd() & processo.StandardError.ReadToEnd()
            processo.WaitForExit()
            Return (processo.ExitCode, saida)
        End Using
    End Function

    ''' <summary>True se o serviço já está registrado no SCM (rodando ou parado).</summary>
    Public Shared Function EstaInstalado() As Boolean
        Dim resultado = RodarSc($"query {NomeServico}")
        Return resultado.CodigoSaida <> CodigoServicoInexistente
    End Function

    ''' <summary>
    ''' (Re)instala o serviço: copia os binários da instalação atual (a pasta
    ''' de onde ESTE processo está rodando — normalmente "current\", se
    ''' chamado a partir do app instalado) para <see cref="PastaServico"/>,
    ''' cria o serviço apontando pra essa cópia (LocalSystem, início
    ''' automático), define a descrição, e já inicia.
    ''' </summary>
    ''' <remarks>
    ''' Se o serviço já existir, para e remove antes de recriar — é assim que
    ''' rodar "--instalar-servico" de novo, depois de uma atualização do app,
    ''' também atualiza o código que o serviço executa (ver remarks da classe).
    ''' </remarks>
    Public Shared Sub Instalar()
        If EstaInstalado() Then
            Desinstalar()
        End If

        Dim pastaOrigem As String = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
        CopiarPastaServico(pastaOrigem, PastaServico)

        Dim argumentosCreate As String =
            $"create {NomeServico} binPath= ""{CaminhoExeServico}"" start= auto obj= LocalSystem DisplayName= ""{NomeExibicao}"""

        Dim resultadoCreate = RodarSc(argumentosCreate)
        If resultadoCreate.CodigoSaida <> 0 Then
            Throw New InvalidOperationException($"sc create retornou erro ({resultadoCreate.CodigoSaida}): {resultadoCreate.Saida.Trim()}")
        End If

        RodarSc($"description {NomeServico} ""{Descricao}""")

        Dim resultadoStart = RodarSc($"start {NomeServico}")
        If resultadoStart.CodigoSaida <> 0 Then
            Throw New InvalidOperationException($"sc start retornou erro ({resultadoStart.CodigoSaida}): {resultadoStart.Saida.Trim()}")
        End If
    End Sub

    ''' <summary>
    ''' Para (esperando o processo liberar os arquivos), remove o serviço, e
    ''' apaga a cópia em <see cref="PastaServico"/>. Não é erro chamar isso
    ''' quando ele já não existe.
    ''' </summary>
    Public Shared Sub Desinstalar()
        If Not EstaInstalado() Then Return

        PararEEsperar()

        Dim resultadoDelete = RodarSc($"delete {NomeServico}")
        If resultadoDelete.CodigoSaida <> 0 AndAlso EstaInstalado() Then
            Throw New InvalidOperationException($"sc delete retornou erro ({resultadoDelete.CodigoSaida}): {resultadoDelete.Saida.Trim()}")
        End If

        If Directory.Exists(PastaServico) Then
            Try
                Directory.Delete(PastaServico, recursive:=True)
            Catch
                ' Best-effort: se algum arquivo ainda estiver bloqueado, a
                ' próxima instalação limpa de novo antes de copiar.
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Pede pro SCM parar o serviço e espera até ele sair do ar (até ~10s),
    ''' pra garantir que os arquivos em <see cref="PastaServico"/> já estão
    ''' livres antes de apagar/recopiar. Best-effort: se o tempo esgotar,
    ''' segue mesmo assim (a cópia recursiva abaixo vai falhar com uma
    ''' mensagem clara se algum arquivo ainda estiver em uso).
    ''' </summary>
    Private Shared Sub PararEEsperar()
        RodarSc($"stop {NomeServico}")

        For tentativa = 1 To 20
            Dim status = RodarSc($"query {NomeServico}")
            If status.CodigoSaida = CodigoServicoInexistente Then Exit Sub

            Dim correspondencia = Text.RegularExpressions.Regex.Match(status.Saida, "STATE\s*:\s*(\d+)")
            If correspondencia.Success AndAlso correspondencia.Groups(1).Value = "1" Then Exit Sub ' 1 = SERVICE_STOPPED

            Thread.Sleep(500)
        Next
    End Sub

    ''' <summary>Copia recursivamente todos os arquivos/subpastas de <paramref name="origem"/> para <paramref name="destino"/>, substituindo se já existir.</summary>
    Private Shared Sub CopiarPastaServico(origem As String, destino As String)
        Directory.CreateDirectory(destino)

        For Each arquivo In Directory.GetFiles(origem)
            File.Copy(arquivo, Path.Combine(destino, Path.GetFileName(arquivo)), overwrite:=True)
        Next

        For Each subPasta In Directory.GetDirectories(origem)
            CopiarPastaServico(subPasta, Path.Combine(destino, Path.GetFileName(subPasta)))
        Next
    End Sub

End Class
