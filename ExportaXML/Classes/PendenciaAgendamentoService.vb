Imports System.IO
Imports System.Text.Json

''' <summary>
''' Estado persistido do agendamento da competência em andamento: quais
''' empresas já receberam o e-mail, quantas tentativas já houve e a última
''' falha. Ver <see cref="PendenciaAgendamentoService"/>.
''' </summary>
Public Class PendenciaAgendamento
    Public Property DataHora As DateTime
    Public Property Competencia As String
    Public Property Mensagem As String
    Public Property Notificada As Boolean

    ''' <summary>Quantas execuções com falha já houve nesta competência.</summary>
    Public Property Tentativas As Integer

    ''' <summary>Não tentar de novo antes deste horário (espera crescente entre tentativas).</summary>
    Public Property ProximaTentativa As DateTime

    ''' <summary>Empresas (chave "banco|código") que já receberam o e-mail desta competência — nunca reenviadas nas novas tentativas.</summary>
    Public Property EmpresasEnviadas As List(Of String) = New List(Of String)

    ''' <summary>True quando há uma falha de verdade para mostrar ao usuário (e não apenas o progresso de envios).</summary>
    <System.Text.Json.Serialization.JsonIgnore>
    Public ReadOnly Property HaFalha As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(Mensagem)
        End Get
    End Property
End Class

''' <summary>
''' Guarda, num arquivo compartilhado em %ProgramData%, o andamento do
''' agendamento mensal: (1) a última falha que ainda não foi avisada na tela —
''' pra alguém saber que o envio não aconteceu mesmo sem abrir o app (só
''' precisa estar com ele na bandeja; ver
''' <see cref="FrmPrincipal.VerificarPendenciaEAvisar"/>); (2) quais empresas
''' já receberam o e-mail e quando é a próxima tentativa — assim, se o
''' programa for fechado/atualizado no meio, ou uma empresa falhar, a nova
''' tentativa envia só o que faltou, sem duplicar e-mail.
''' </summary>
''' <remarks>
''' Fica em %ProgramData% (não %LocalAppData%) de propósito: é o único lugar
''' que tanto o app interativo (usuário normal) quanto o Windows Service
''' (LocalSystem) conseguem ler e escrever da mesma forma — ver o mesmo
''' raciocínio em <see cref="LogService.PastaLogs"/>.
''' </remarks>
Public Class PendenciaAgendamentoService

    ''' <summary>Depois de tantas execuções com falha na mesma competência, para de tentar sozinho (continua visível na tela/no log).</summary>
    Public Const MaximoTentativas As Integer = 8

    ''' <summary>Espera antes da tentativa seguinte, indexada pelo número de falhas já ocorridas (1ª falha → 15 min, ...).</summary>
    Private Shared ReadOnly Esperas As TimeSpan() = {
        TimeSpan.FromMinutes(15), TimeSpan.FromHours(1), TimeSpan.FromHours(3),
        TimeSpan.FromHours(6), TimeSpan.FromHours(12)
    }

    Private Shared ReadOnly Trava As New Object()

    Private Shared ReadOnly Caminho As String =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ExportaXML", "pendencia_agendamento.json")

    ''' <summary>Registra (sem repetir a mesma mensagem) que o estado do agendamento não pôde ser gravado — antes isso era engolido sem rastro.</summary>
    Private Shared Sub LogarFalhaEstado(ex As Exception)
        Dim msg As String = ex.GetType().Name & ": " & ex.Message
        If msg = _ultimaFalhaEstado Then Return
        _ultimaFalhaEstado = msg

        Dim motivo As String = If(TypeOf ex Is UnauthorizedAccessException,
            "sem permissão: o arquivo pode pertencer ao serviço do Windows; apague-o como administrador",
            ex.Message)
        Try
            LogService.Registrar(Path.Combine(LogService.PastaLogs, "Agendamento_Geral.log"),
                $"AVISO: não foi possível gravar o estado do agendamento em {Caminho} ({motivo}). Reenvios podem se repetir até isso ser corrigido.")
        Catch
        End Try
    End Sub

    Private Shared _ultimaFalhaEstado As String

    Private Shared Sub Gravar(p As PendenciaAgendamento)
        Dim pasta As String = Path.GetDirectoryName(Caminho)
        If Not Directory.Exists(pasta) Then Directory.CreateDirectory(pasta)
        Dim temporario As String = Caminho & ".tmp"
        File.WriteAllText(temporario, JsonSerializer.Serialize(p))
        File.Move(temporario, Caminho, overwrite:=True)
        PermissoesArquivo.LiberarParaUsuarios(Caminho)
    End Sub

    ''' <summary>Lê o estado atual, ou Nothing se não houver (arquivo inexistente ou ilegível).</summary>
    Public Shared Function Obter() As PendenciaAgendamento
        SyncLock Trava
            Try
                If Not File.Exists(Caminho) Then Return Nothing
                Dim p = JsonSerializer.Deserialize(Of PendenciaAgendamento)(File.ReadAllText(Caminho))
                If p IsNot Nothing AndAlso p.EmpresasEnviadas Is Nothing Then p.EmpresasEnviadas = New List(Of String)
                Return p
            Catch
                Return Nothing
            End Try
        End SyncLock
    End Function

    ''' <summary>Estado da competência informada; se o arquivo é de outra competência (ou não existe), devolve um novo, vazio.</summary>
    Public Shared Function ObterDaCompetencia(competencia As String) As PendenciaAgendamento
        Dim p = Obter()
        If p Is Nothing OrElse p.Competencia <> competencia Then
            Return New PendenciaAgendamento With {.Competencia = competencia, .DataHora = DateTime.Now}
        End If
        Return p
    End Function

    ''' <summary>True se esta empresa já recebeu o e-mail desta competência (não reenviar).</summary>
    Public Shared Function JaEnviada(competencia As String, chaveEmpresa As String) As Boolean
        Return ObterDaCompetencia(competencia).EmpresasEnviadas.Contains(chaveEmpresa)
    End Function

    ''' <summary>Anota que a empresa recebeu o e-mail — gravado na hora, pra sobreviver a um fechamento do programa no meio.</summary>
    Public Shared Sub MarcarEnviada(competencia As String, chaveEmpresa As String)
        SyncLock Trava
            Try
                Dim p = ObterDaCompetencia(competencia)
                If Not p.EmpresasEnviadas.Contains(chaveEmpresa) Then p.EmpresasEnviadas.Add(chaveEmpresa)
                Gravar(p)
            Catch ex As Exception
                LogarFalhaEstado(ex)
            End Try
        End SyncLock
    End Sub

    ''' <summary>
    ''' True se ainda é cedo para uma nova tentativa desta competência (espera
    ''' entre tentativas) ou se o limite de tentativas já foi atingido.
    ''' </summary>
    Public Shared Function DeveAguardar(competencia As String) As Boolean
        Dim p = Obter()
        If p Is Nothing OrElse p.Competencia <> competencia OrElse Not p.HaFalha Then Return False
        Return p.Tentativas >= MaximoTentativas OrElse DateTime.Now < p.ProximaTentativa
    End Function

    ''' <summary>
    ''' Registra uma falha (mantendo a lista de empresas já enviadas), soma uma
    ''' tentativa e agenda a próxima com espera crescente. Devolve o estado
    ''' gravado (o chamador usa <c>Tentativas</c> para decidir se manda e-mail de alerta).
    ''' </summary>
    Public Shared Function Registrar(competencia As String, mensagem As String) As PendenciaAgendamento
        SyncLock Trava
            Dim p = ObterDaCompetencia(competencia)
            Try
                p.DataHora = DateTime.Now
                p.Mensagem = mensagem
                p.Notificada = False
                p.Tentativas += 1
                p.ProximaTentativa = DateTime.Now + Esperas(Math.Min(p.Tentativas, Esperas.Length) - 1)
                Gravar(p)
            Catch ex As Exception
                LogarFalhaEstado(ex)
                ' Melhor esforço: se não conseguir gravar, a falha original já foi registrada no log.
            End Try
            Return p
        End SyncLock
    End Function

    ''' <summary>Marca a falha atual como já avisada, pra não repetir o balão de novo pela mesma falha.</summary>
    Public Shared Sub MarcarNotificada()
        SyncLock Trava
            Try
                Dim p = Obter()
                If p Is Nothing Then Return
                p.Notificada = True
                Gravar(p)
            Catch ex As Exception
                LogarFalhaEstado(ex)
            End Try
        End SyncLock
    End Sub

    ''' <summary>Remove o estado — chamado quando a competência termina com sucesso total.</summary>
    Public Shared Sub Limpar()
        SyncLock Trava
            Try
                If File.Exists(Caminho) Then File.Delete(Caminho)
            Catch ex As Exception
                LogarFalhaEstado(ex)
            End Try
        End SyncLock
    End Sub

End Class
