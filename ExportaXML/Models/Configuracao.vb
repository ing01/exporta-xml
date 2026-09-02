Imports Microsoft.VisualBasic

''' <summary>
''' Modelo persistido inteiro em "config.json" via <see cref="ConfiguracaoService"/>.
''' Não tem validação própria — cada tela grava só os campos que edita, então o
''' construtor abaixo é a única fonte de valores padrão para campos que um
''' config.json antigo (de uma versão anterior do app) ainda não tem.
''' </summary>
Public Class Configuracoes

    ''' <summary>
    ''' Bancos Postgres configurados. Pode ter mais de um quando as empresas do
    ''' cliente estão espalhadas em bancos diferentes (mesmo servidor ou não).
    ''' Populado a partir de <see cref="Servidor"/>/<see cref="Porta"/>/etc. na
    ''' migração de um config.json antigo — ver <see cref="ConfiguracaoService.Carregar"/>.
    ''' </summary>
    ''' 
    Public Property OrigemConfiguracao As String = "Manual"  ' "Manual" ou "Banco"
    Public Property CodEmpresaSincronizada As Integer = 0
    Public Property Conexoes As List(Of ConexaoBanco)

    ''' <summary>
    ''' OBSOLETO: conexão única de versões anteriores ao suporte a múltiplos
    ''' bancos. Mantidos só para migrar um config.json antigo para
    ''' <see cref="Conexoes"/> — não usar em código novo.
    ''' </summary>
    Public Property Servidor As String
    Public Property Porta As Integer
    Public Property Banco As String
    Public Property Usuario As String
    Public Property Senha As String

    ' --- SMTP usado tanto no envio manual quanto no Agendamento/alerta de falha ---
    Public Property ServidorSMTP As String
    Public Property PortaSMTP As Integer
    Public Property UsuarioSMTP As String
    Public Property SenhaSMTP As String
    Public Property EmailRemetente As String
    Public Property UsarSSL As Boolean
    ' Chave usada para descriptografar valores no banco que foram cifrados com OpenSSL (Salted__...).
    Public Property ChaveCriptografia As String

    ''' <summary>
    ''' Valores padrão usados quando o config.json ainda não existe (primeira
    ''' execução) ou quando ele existe mas é de uma versão anterior que não tinha
    ''' um destes campos ainda (o desserializador de JSON preserva esses padrões
    ''' para qualquer chave ausente no arquivo).
    ''' </summary>
    Public Sub New()
        Conexoes = New List(Of ConexaoBanco)

        Servidor = String.Empty
        Porta = 0
        Banco = String.Empty
        Usuario = String.Empty
        Senha = String.Empty

        ServidorSMTP = String.Empty
        PortaSMTP = 0
        UsuarioSMTP = String.Empty
        SenhaSMTP = String.Empty
        EmailRemetente = String.Empty
        UsarSSL = True
        ChaveCriptografia = String.Empty
        DestinatariosLocais = New List(Of DestinatarioLocal)()

        AgendamentoAtivo = False
        HoraAgendamento = 8
        MinutoAgendamento = 0
        DiaAgendamento = 1
        EmailAlertaFalha = String.Empty
        UltimaCompetenciaExecutada = String.Empty
    End Sub

    ' --- Últimas escolhas do usuário na tela principal, para reabrir do jeito que deixou ---
    Public Property UltimaEmpresa As Integer
    Public Property UltimoDestinatario As String
    Public Property DestinatariosLocais As List(Of DestinatarioLocal)

    ''' <summary>
    ''' OBSOLETO / não usado mais: o destino de exportação hoje é sempre calculado
    ''' na hora (Área de Trabalho ou última pasta escolhida no diálogo daquela
    ''' sessão) — ver <c>ObterCaminhoDestinoPadrao</c> em FrmPrincipal. Mantido
    ''' aqui só para não quebrar a leitura de um config.json antigo.
    ''' </summary>
    Public Property UltimaPastaExportacao As String

    Public Property UltimoModelo As Integer

    ' --- Agendamento automático (exportação + envio por e-mail do mês anterior, todo mês) ---
    Public Property AgendamentoAtivo As Boolean
    Public Property HoraAgendamento As Integer
    Public Property MinutoAgendamento As Integer

    ''' <summary>
    ''' Dia do mês (1 a 31) em que o agendamento roda. Padrão 1 ("todo dia
    ''' 01"). Se o mês não tiver esse dia (ex.: 31 em abril, 30/31 em
    ''' fevereiro), <see cref="AgendamentoService.DiaEfetivo"/> ajusta pro
    ''' último dia válido daquele mês.
    ''' </summary>
    Public Property DiaAgendamento As Integer
    Public Property EmailAlertaFalha As String

    ''' <summary>
    ''' Competência (formato "yyyy-MM") do último agendamento executado com
    ''' sucesso — é o que impede o agendamento de rodar duas vezes no mesmo mês.
    ''' Ver <see cref="AgendamentoService.DeveExecutar"/>.
    ''' </summary>
    Public Property UltimaCompetenciaExecutada As String
End Class
