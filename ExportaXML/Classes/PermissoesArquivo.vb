Imports System.IO
Imports System.Security.AccessControl
Imports System.Security.Principal

''' <summary>
''' Arquivos em %ProgramData%\ExportaXML são lidos E gravados por dois "donos"
''' diferentes: o app do usuário e o Windows Service (SYSTEM). No Windows, quem
''' CRIA o arquivo vira dono, e a pasta só dá leitura aos demais usuários — então
''' um arquivo criado pelo serviço não podia mais ser atualizado pelo app (e
''' vice-versa), e o programa falhava em silêncio. Depois de gravar, este helper
''' libera modificação para o grupo Usuários.
''' </summary>
Public Class PermissoesArquivo

    ''' <summary>Dá ao grupo Usuários permissão de modificar o arquivo. Melhor esforço: nunca lança exceção.</summary>
    Public Shared Sub LiberarParaUsuarios(caminho As String)
        Try
            Dim info As New FileInfo(caminho)
            If Not info.Exists Then Return

            Dim usuarios As New SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, Nothing)
            Dim seguranca As FileSecurity = info.GetAccessControl()
            seguranca.AddAccessRule(New FileSystemAccessRule(usuarios, FileSystemRights.Modify, AccessControlType.Allow))
            info.SetAccessControl(seguranca)
        Catch
            ' Sem permissão para alterar a ACL (arquivo de outro dono): segue sem ela.
        End Try
    End Sub

End Class
