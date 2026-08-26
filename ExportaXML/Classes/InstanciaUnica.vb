Imports System.Runtime.InteropServices

''' <summary>
''' Suporte a instância única do aplicativo: garante que só exista uma janela
''' por vez, mesmo se o usuário tentar abrir o programa de novo enquanto ele já
''' está rodando (minimizado na bandeja). Sem isso, uma segunda tentativa
''' abriria uma segunda instância completa, duplicando os Timers de
''' Agendamento e Atualização automática — nada bom.
''' </summary>
Public Class InstanciaUnica

    ''' <summary>Nome do Mutex usado por <see cref="EntryPoint.Main"/> para detectar se já existe uma instância rodando.</summary>
    Public Const NomeMutex As String = "ExportaXML_InstanciaUnica_Mutex"

    Private Const HWND_BROADCAST As Integer = &HFFFF

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function RegisterWindowMessage(lpString As String) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function PostMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' Identificador de uma mensagem do Windows registrada especificamente
    ''' para este aplicativo, usada para "acordar" a instância já em execução.
    ''' Chamar RegisterWindowMessage duas vezes com o mesmo texto (uma vez
    ''' aqui, uma vez em cada processo que carregar esta classe) sempre
    ''' devolve o mesmo número — não precisa de nenhum outro mecanismo de
    ''' comunicação entre os dois processos.
    ''' </summary>
    Public Shared ReadOnly MensagemMostrarJanela As Integer =
        RegisterWindowMessage("ExportaXML_MostrarJanela")

    ''' <summary>
    ''' Avisa, por broadcast (para todas as janelas do sistema), que uma outra
    ''' instância do aplicativo já está rodando e deve se mostrar. Chamado pela
    ''' segunda instância, que sai logo em seguida sem abrir nenhuma tela — ver
    ''' <see cref="FrmPrincipal.WndProc"/>, que escuta essa mensagem do lado de quem já está aberto.
    ''' </summary>
    Public Shared Sub NotificarInstanciaExistente()
        PostMessage(CType(HWND_BROADCAST, IntPtr), MensagemMostrarJanela, IntPtr.Zero, IntPtr.Zero)
    End Sub

End Class
