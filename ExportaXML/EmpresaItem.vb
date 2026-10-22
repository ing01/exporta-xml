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

    ''' <summary>CNPJ no formato 00.000.000/0000-00 (se tiver 14 dígitos); caso contrário devolve o texto como veio, ou "" se vazio.</summary>
    Public Shared Function FormatarCnpj(cnpj As String) As String
        If String.IsNullOrWhiteSpace(cnpj) Then Return ""
        Dim d As String = New String(cnpj.Where(Function(c) Char.IsDigit(c)).ToArray())
        If d.Length <> 14 Then Return cnpj.Trim()
        Return $"{d.Substring(0, 2)}.{d.Substring(2, 3)}.{d.Substring(5, 3)}/{d.Substring(8, 4)}-{d.Substring(12, 2)}"
    End Function

    ''' <summary>
    ''' Texto mostrado nas listas de empresa: "CNPJ - NOME" (e "(Banco)" no final quando há mais de um
    ''' banco). Começar pelo CNPJ ajuda a identificar a empresa certa quando há muitas no banco, e
    ''' permite digitar o CNPJ na lista para pular até ela.
    ''' </summary>
    Public Shared Function MontarNomeExibicao(nome As String, cnpj As String, nomeBanco As String, variosBancos As Boolean) As String
        Dim cnpjFmt As String = FormatarCnpj(cnpj)
        Dim texto As String = If(cnpjFmt = "", nome, $"{cnpjFmt} - {nome}")
        If variosBancos AndAlso Not String.IsNullOrWhiteSpace(nomeBanco) Then texto &= $" ({nomeBanco})"
        Return texto
    End Function

    Public Overrides Function ToString() As String
        Return Nome
    End Function
End Class