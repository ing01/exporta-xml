''' <summary>
''' Uma conexão Postgres configurada (endereço + credenciais de UM banco).
''' <see cref="Configuracoes.Conexoes"/> guarda uma lista destas — clientes com
''' empresas espalhadas em bancos diferentes cadastram uma entrada por banco.
''' </summary>
Public Class ConexaoBanco

    ''' <summary>Rótulo livre escolhido pelo usuário (ex.: "Matriz", "Filial SP"), só para exibição.</summary>
    Public Property Nome As String

    Public Property Servidor As String
    Public Property Porta As Integer
    Public Property Banco As String
    Public Property Usuario As String
    Public Property Senha As String

    Public Sub New()
        Nome = String.Empty
        Servidor = String.Empty
        Porta = 0
        Banco = String.Empty
        Usuario = String.Empty
        Senha = String.Empty
    End Sub

End Class
