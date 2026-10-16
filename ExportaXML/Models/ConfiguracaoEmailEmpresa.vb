''' <summary>
''' Configuração de SMTP (envio) específica de UMA empresa — permite que cada
''' empresa tenha seu próprio remetente/servidor, em vez de todo o programa
''' compartilhar uma única configuração Global. Ver <see cref="Configuracoes.ConfiguracoesEmailPorEmpresa"/>
''' e <see cref="ConfiguracaoEmailService.ResolverConfiguracaoParaEmpresa"/>.
''' </summary>
Public Class ConfiguracaoEmailEmpresa

    Public Property CodigoEmpresa As Integer

    ''' <summary>Nome do banco (ConexaoBanco.Nome) ao qual a empresa pertence; vazio em cadastros antigos.</summary>
    Public Property NomeBanco As String = ""

    ''' <summary>
    ''' "Padrao" = não tem override, usa a configuração Global (cfg.ServidorSMTP
    ''' etc.); "Manual" = usa os campos abaixo, digitados nesta tela; "Banco" =
    ''' busca da tabela conf_nfe do PRÓPRIO banco dessa empresa a cada envio
    ''' (ver <see cref="ConfiguracaoEmailService.BuscarConfiguracao"/>).
    ''' </summary>
    Public Property Origem As String = "Padrao"

    Public Property ServidorSMTP As String
    Public Property PortaSMTP As Integer
    Public Property UsuarioSMTP As String
    Public Property SenhaSMTP As String
    Public Property EmailRemetente As String
    Public Property UsarSSL As Boolean

    Public Sub New()
        ServidorSMTP = String.Empty
        UsuarioSMTP = String.Empty
        SenhaSMTP = String.Empty
        EmailRemetente = String.Empty
    End Sub

End Class
