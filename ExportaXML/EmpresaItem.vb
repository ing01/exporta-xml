''' <summary>
''' Item de exibição para o combo "Empresa" (<see cref="EmpresaService.Listar"/>).
''' Código=0 é o item sentinela "Todas as empresas" (nesse caso <see cref="Conexao"/> é Nothing).
''' </summary>
Public Class EmpresaItem

    Public Property Codigo As Integer
    Public Property Nome As String
    Public Property CNPJ As String

    ''' <summary>
    ''' Banco de onde esta empresa foi listada (ver <see cref="Configuracoes.Conexoes"/>)
    ''' — é nele que <see cref="FrmPrincipal"/> abre a conexão pra pesquisar/exportar
    ''' esta empresa. Sempre preenchido, exceto no item sentinela "Todas as empresas".
    ''' </summary>
    Public Property Conexao As ConexaoBanco
    Public Property NomeExibicao As String

    Public Overrides Function ToString() As String
        Return Nome
    End Function
End Class