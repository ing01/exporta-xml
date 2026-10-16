''' <summary>
''' Regras de "a que empresa este cadastro pertence". Uma empresa NÃO é só o
''' código: com mais de um banco cadastrado, cada banco tem a sua "empresa 1",
''' "empresa 2"... Cadastros novos (destinatários e configuração de e-mail)
''' guardam também o nome do banco (<c>NomeBanco</c>) e só valem para aquela
''' combinação banco + código.
''' </summary>
''' <remarks>
''' REGRA DE OURO: nenhum cadastro existente pode deixar de funcionar numa
''' atualização. Cadastro ANTIGO (sem banco gravado) continua valendo para o
''' código em qualquer banco, exatamente como sempre foi, até ser convertido por
''' <see cref="ConfiguracaoService.MigrarEscopoLegado"/> (que preserva o
''' comportamento: um cadastro por banco que tem aquele código). Um cadastro
''' específico (com banco) sempre tem prioridade sobre um antigo.
''' </remarks>
Public Class EscopoEmpresa

    ''' <summary>
    ''' True se um cadastro (código + banco gravados) pertence à empresa informada.
    ''' - Código 0 ("Global") vale sempre.
    ''' - Cadastro com banco gravado: só vale para o banco de mesmo nome.
    ''' - Cadastro ANTIGO, sem banco gravado: vale para o código em qualquer banco (comportamento histórico).
    ''' </summary>
    Public Shared Function Corresponde(
        codigoRegistro As Integer,
        bancoRegistro As String,
        codigoEmpresa As Integer,
        nomeBanco As String,
        totalBancos As Integer) As Boolean

        If codigoRegistro <> codigoEmpresa Then Return False
        If codigoEmpresa = 0 Then Return True
        If String.IsNullOrWhiteSpace(bancoRegistro) Then Return True
        Return String.Equals(bancoRegistro.Trim(), If(nomeBanco, "").Trim(), StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>Destinatários ativos da empresa; os cadastrados especificamente para o banco dela vêm primeiro, os antigos (sem banco) depois.</summary>
    Public Shared Function DestinatariosDaEmpresa(cfg As Configuracoes, codigoEmpresa As Integer, nomeBanco As String) As List(Of DestinatarioLocal)
        If cfg Is Nothing OrElse cfg.DestinatariosLocais Is Nothing Then Return New List(Of DestinatarioLocal)
        Dim total As Integer = If(cfg.Conexoes Is Nothing, 0, cfg.Conexoes.Count)
        Return cfg.DestinatariosLocais.
            Where(Function(d) d.Ativo AndAlso Corresponde(d.CodigoEmpresa, d.NomeBanco, codigoEmpresa, nomeBanco, total)).
            OrderBy(Function(d) If(String.IsNullOrWhiteSpace(d.NomeBanco), 1, 0)).
            ToList()
    End Function

    ''' <summary>Configuração de e-mail própria da empresa; a específica do banco dela tem prioridade sobre a antiga (sem banco).</summary>
    Public Shared Function ConfigEmailDaEmpresa(cfg As Configuracoes, codigoEmpresa As Integer, nomeBanco As String) As ConfiguracaoEmailEmpresa
        If cfg Is Nothing OrElse cfg.ConfiguracoesEmailPorEmpresa Is Nothing Then Return Nothing
        Dim total As Integer = If(cfg.Conexoes Is Nothing, 0, cfg.Conexoes.Count)
        Return cfg.ConfiguracoesEmailPorEmpresa.
            Where(Function(c) Corresponde(c.CodigoEmpresa, c.NomeBanco, codigoEmpresa, nomeBanco, total)).
            OrderBy(Function(c) If(String.IsNullOrWhiteSpace(c.NomeBanco), 1, 0)).
            FirstOrDefault()
    End Function

    ''' <summary>True se há cadastro antigo (sem banco) de empresa específica e mais de um banco — vale a pena converter.</summary>
    Public Shared Function TemLegados(cfg As Configuracoes) As Boolean
        If cfg Is Nothing OrElse cfg.Conexoes Is Nothing OrElse cfg.Conexoes.Count <= 1 Then Return False
        Dim temDest = cfg.DestinatariosLocais IsNot Nothing AndAlso cfg.DestinatariosLocais.Any(Function(d) d.CodigoEmpresa <> 0 AndAlso String.IsNullOrWhiteSpace(d.NomeBanco))
        Dim temCfg = cfg.ConfiguracoesEmailPorEmpresa IsNot Nothing AndAlso cfg.ConfiguracoesEmailPorEmpresa.Any(Function(c) c.CodigoEmpresa <> 0 AndAlso String.IsNullOrWhiteSpace(c.NomeBanco))
        Return temDest OrElse temCfg
    End Function

End Class
