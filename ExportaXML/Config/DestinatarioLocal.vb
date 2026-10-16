Public Class DestinatarioLocal
    Public Property CodigoEmpresa As Integer
    Public Property Email As String
    Public Property Descricao As String
    Public Property Ativo As Boolean = True

    ''' <summary>Nome do banco (ConexaoBanco.Nome) da empresa; vazio em cadastros antigos e no "Global" (código 0).</summary>
    Public Property NomeBanco As String = ""
End Class
