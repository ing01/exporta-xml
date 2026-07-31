Imports Microsoft.VisualBasic

Public Class Configuracoes
    Public Property Servidor As String
    Public Property Porta As Integer
    Public Property Banco As String
    Public Property Usuario As String
    Public Property Senha As String

    Public Property ServidorSMTP As String
    Public Property PortaSMTP As Integer
    Public Property UsuarioSMTP As String
    Public Property SenhaSMTP As String
    Public Property EmailRemetente As String
    Public Property UsarSSL As Boolean
    Public Sub New()
        Servidor = String.Empty
        Porta = 0
        Banco = String.Empty
        Usuario = String.Empty
        Senha = String.Empty

        ServidorSMTP = String.Empty
        PortaSMTP = 0
        UsuarioSMTP = String.Empty
        SenhaSMTP = String.Empty
        EmailRemetente = String.Empty
        UsarSSL = True
    End Sub

    Public Property UltimaEmpresa As Integer
    Public Property UltimoDestinatario As String
    Public Property UltimaPastaExportacao As String
    Public Property UltimoModelo As Integer
End Class
