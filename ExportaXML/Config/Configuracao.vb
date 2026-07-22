Public Class Configuracao

    'Banco
    Public Property Servidor As String
    Public Property Porta As Integer
    Public Property Banco As String
    Public Property Usuario As String
    Public Property Senha As String

    'Email
    Public Property ServidorSMTP As String
    Public Property PortaSMTP As Integer
    Public Property UsuarioSMTP As String
    Public Property SenhaSMTP As String
    Public Property Remetente As String
    Public Property SSL As Boolean

End Class