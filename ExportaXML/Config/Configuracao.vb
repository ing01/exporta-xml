''' <summary>
''' OBSOLETO / não usado em nenhum lugar do código: foi substituído por
''' <see cref="Configuracoes"/> (em Models\Configuracao.vb, plural), que é a
''' classe realmente lida/gravada por <see cref="ConfiguracaoService"/>. Este
''' arquivo ficou pra trás de uma versão antiga e é seguro remover — mantido
''' aqui só até uma limpeza dedicada, pra não misturar com as mudanças desta vez.
''' </summary>
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