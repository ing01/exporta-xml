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

    ''' <summary>
    ''' "Nome (Banco)", para uso como <c>DisplayMember</c> quando há mais de uma
    ''' conexão configurada — ver <see cref="FrmPrincipal.CarregarEmpresasEFornecedores"/>,
    ''' que só troca o combo para este campo nesse caso (com uma única conexão,
    ''' a combo usa <see cref="Nome"/> puro, idêntico ao comportamento anterior).
    ''' </summary>
    Public ReadOnly Property NomeExibicao As String
        Get
            If Conexao Is Nothing Then Return Nome
            Return $"{Nome} ({Conexao.Nome})"
        End Get
    End Property

End Class