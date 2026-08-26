''' <summary>
''' Item de exibição para o combo "Fornecedor" (<see cref="FornecedorService.Listar"/>).
''' Código=0 é o item sentinela "Todos os fornecedores".
''' </summary>
Public Class FornecedorItem

    Public Property Codigo As Integer
    Public Property Nome As String
    Public Property CNPJ As String

    ''' <summary>Banco de onde este fornecedor foi listado — sempre o mesmo da empresa selecionada na tela.</summary>
    Public Property Conexao As ConexaoBanco

End Class
