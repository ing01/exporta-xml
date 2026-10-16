''' <summary>
''' Regras de "a que empresa este cadastro pertence". Uma empresa NÃO é só o
''' código: com mais de um banco cadastrado, cada banco tem a sua "empresa 1",
''' "empresa 2"... Cadastros (destinatários e configuração de e-mail) que usavam
''' só o código valiam, por engano, para a empresa de mesmo código em TODOS os
''' bancos. Agora cada cadastro guarda também o nome do banco
''' (<c>NomeBanco</c>) e só vale para aquela combinação banco + código.
''' </summary>
Public Class EscopoEmpresa

    ''' <summary>
    ''' True se um cadastro (código + banco gravados) pertence à empresa informada.
    ''' - Código 0 ("Global") vale sempre.
    ''' - Cadastro com banco gravado: só vale para o banco de mesmo nome.
    ''' - Cadastro ANTIGO, sem banco gravado: só é seguro usar quando existe um
    '''   único banco (não há ambiguidade). Com vários bancos ele fica inativo até ser
    '''   associado a um banco (ver <see cref="ConfiguracaoService.MigrarEscopoLegado"/>)
    '''   ou recadastrado — melhor não enviar do que enviar o XML para a empresa errada.
    ''' </summary>
    Public Shared Function Corresponde(
        codigoRegistro As Integer,
        bancoRegistro As String,
        codigoEmpresa As Integer,
        nomeBanco As String,
        totalBancos As Integer) As Boolean

        If codigoRegistro <> codigoEmpresa Then Return False
        If codigoEmpresa = 0 Then Return True
        If String.IsNullOrWhiteSpace(bancoRegistro) Then Return totalBancos <= 1
        Return String.Equals(bancoRegistro.Trim(), If(nomeBanco, "").Trim(), StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Function DestinatariosDaEmpresa(cfg As Configuracoes, codigoEmpresa As Integer, nomeBanco As String) As List(Of DestinatarioLocal)
        If cfg Is Nothing OrElse cfg.DestinatariosLocais Is Nothing Then Return New List(Of DestinatarioLocal)
        Dim total As Integer = If(cfg.Conexoes Is Nothing, 0, cfg.Conexoes.Count)
        Return cfg.DestinatariosLocais.Where(Function(d) d.Ativo AndAlso Corresponde(d.CodigoEmpresa, d.NomeBanco, codigoEmpresa, nomeBanco, total)).ToList()
    End Function

    Public Shared Function ConfigEmailDaEmpresa(cfg As Configuracoes, codigoEmpresa As Integer, nomeBanco As String) As ConfiguracaoEmailEmpresa
        If cfg Is Nothing OrElse cfg.ConfiguracoesEmailPorEmpresa Is Nothing Then Return Nothing
        Dim total As Integer = If(cfg.Conexoes Is Nothing, 0, cfg.Conexoes.Count)
        Return cfg.ConfiguracoesEmailPorEmpresa.FirstOrDefault(Function(c) Corresponde(c.CodigoEmpresa, c.NomeBanco, codigoEmpresa, nomeBanco, total))
    End Function

    ''' <summary>True se há cadastro antigo (sem banco) de empresa específica e mais de um banco — situação que precisa de migração.</summary>
    Public Shared Function TemLegadosAmbiguos(cfg As Configuracoes) As Boolean
        If cfg Is Nothing OrElse cfg.Conexoes Is Nothing OrElse cfg.Conexoes.Count <= 1 Then Return False
        Dim temDest = cfg.DestinatariosLocais IsNot Nothing AndAlso cfg.DestinatariosLocais.Any(Function(d) d.CodigoEmpresa <> 0 AndAlso String.IsNullOrWhiteSpace(d.NomeBanco))
        Dim temCfg = cfg.ConfiguracoesEmailPorEmpresa IsNot Nothing AndAlso cfg.ConfiguracoesEmailPorEmpresa.Any(Function(c) c.CodigoEmpresa <> 0 AndAlso String.IsNullOrWhiteSpace(c.NomeBanco))
        Return temDest OrElse temCfg
    End Function

End Class
