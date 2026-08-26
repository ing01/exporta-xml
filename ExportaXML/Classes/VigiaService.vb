Imports System.Diagnostics

''' <summary>
''' "Vigia" que mantém o ExportaXML sempre em execução: registra uma tarefa no
''' Agendador de Tarefas do Windows que tenta abrir o programa a cada poucos
''' minutos. Não é um serviço do Windows (Session 0 não permite ícone de
''' bandeja/janela) — é um atalho pra chegar num resultado parecido: se o
''' programa já estiver aberto, a tentativa não faz nada (ver
''' <see cref="InstanciaUnica"/>, que detecta a instância já em execução e
''' sai sem abrir nada); se tiver sido fechado por engano, travado, ou a
''' máquina reiniciado, essa mesma tentativa efetivamente reabre o programa.
''' </summary>
Public Class VigiaService

    ''' <summary>Nome da tarefa no Agendador — usado tanto para criar quanto pra checar/remover.</summary>
    Private Const NomeTarefa As String = "ExportaXML_ManterAtivo"

    ''' <summary>Intervalo, em minutos, entre cada tentativa do Agendador de reabrir o programa.</summary>
    Private Const IntervaloMinutos As Integer = 5

    ''' <summary>
    ''' Roda o schtasks.exe com os argumentos informados e devolve
    ''' (código de saída, saída padrão + erro combinadas). Não precisa de
    ''' privilégios de administrador — tarefas do usuário atual, sem
    ''' "executar com privilégios máximos", não exigem elevação.
    ''' </summary>
    Private Shared Function RodarSchtasks(argumentos As String) As (CodigoSaida As Integer, Saida As String)
        Dim psi As New ProcessStartInfo("schtasks.exe", argumentos) With {
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

    ''' <summary>True se a tarefa do Vigia já está cadastrada no Agendador de Tarefas.</summary>
    Public Shared Function EstaAtivo() As Boolean
        Dim resultado = RodarSchtasks($"/Query /TN ""{NomeTarefa}""")
        Return resultado.CodigoSaida = 0
    End Function

    ''' <summary>
    ''' Cadastra (ou substitui, se já existir com um caminho antigo) a tarefa
    ''' que tenta abrir o ExportaXML a cada <see cref="IntervaloMinutos"/> minutos,
    ''' pra sempre, sem data de término.
    ''' </summary>
    Public Shared Sub Ativar()
        Dim caminhoExe As String = Application.ExecutablePath
        Dim argumentos As String =
            $"/Create /TN ""{NomeTarefa}"" /TR ""\""{caminhoExe}\"""" /SC MINUTE /MO {IntervaloMinutos} /F"

        Dim resultado = RodarSchtasks(argumentos)

        If resultado.CodigoSaida <> 0 Then
            Throw New InvalidOperationException($"schtasks retornou erro ({resultado.CodigoSaida}): {resultado.Saida.Trim()}")
        End If
    End Sub

    ''' <summary>Remove a tarefa do Vigia. Não é erro chamar isso quando ela já não existe.</summary>
    Public Shared Sub Desativar()
        Dim resultado = RodarSchtasks($"/Delete /TN ""{NomeTarefa}"" /F")

        ' Código 1 com "not exist"/"não existe" na saída = já estava desativado, tudo bem.
        If resultado.CodigoSaida <> 0 AndAlso EstaAtivo() Then
            Throw New InvalidOperationException($"schtasks retornou erro ({resultado.CodigoSaida}): {resultado.Saida.Trim()}")
        End If
    End Sub

End Class
