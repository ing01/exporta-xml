Imports System.Diagnostics

''' <summary>
''' "Vigia" — recurso DESCONTINUADO ("Manter sempre em execução") que registrava
''' uma tarefa no Agendador de Tarefas do Windows para reabrir o programa a
''' cada poucos minutos. Removido porque trazia a janela pra frente com
''' frequência incômoda para quem preferia deixar o programa minimizado na
''' bandeja. Esta classe continua existindo só para <see cref="Desativar"/> a
''' tarefa em instalações antigas que já tinham o recurso ativado (ver
''' <see cref="FrmPrincipal.FrmPrincipal_Shown"/>).
''' </summary>
Public Class VigiaService

    ''' <summary>Nome da tarefa no Agendador — usado tanto para checar quanto pra remover.</summary>
    Private Const NomeTarefa As String = "ExportaXML_ManterAtivo"

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

    ''' <summary>Remove a tarefa do Vigia. Não é erro chamar isso quando ela já não existe.</summary>
    Public Shared Sub Desativar()
        Dim resultado = RodarSchtasks($"/Delete /TN ""{NomeTarefa}"" /F")

        ' Código 1 com "not exist"/"não existe" na saída = já estava desativado, tudo bem.
        If resultado.CodigoSaida <> 0 AndAlso EstaAtivo() Then
            Throw New InvalidOperationException($"schtasks retornou erro ({resultado.CodigoSaida}): {resultado.Saida.Trim()}")
        End If
    End Sub

End Class
