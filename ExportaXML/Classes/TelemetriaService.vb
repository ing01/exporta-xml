Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Text.RegularExpressions

''' <summary>
''' Corpo de POST /api/DSMON/RegistrarTela (API de telemetria da Duesoft).
''' Nomes de propriedade em PascalCase por convenção .NET — o
''' <see cref="JsonPropertyNameAttribute"/> em cada uma garante o JSON exato
''' que a API espera (minúsculo), verificado no Swagger.
''' </summary>
Public Class TelemetriaRegistro
    <JsonPropertyName("cnpj")> Public Property Cnpj As String
    <JsonPropertyName("computador")> Public Property Computador As String
    <JsonPropertyName("sistema")> Public Property Sistema As String
    <JsonPropertyName("tela")> Public Property Tela As String
    <JsonPropertyName("acesso")> Public Property Acesso As String
    <JsonPropertyName("usuario")> Public Property Usuario As String
End Class

''' <summary>
''' Envia telemetria de acesso a telas para a API da Duesoft
''' (POST https://dswebapi.duesoft.com.br/api/DSMON/RegistrarTela).
''' </summary>
''' <remarks>
''' Uso "melhor esforço" de propósito: qualquer falha (sem internet, API fora
''' do ar, timeout, erro HTTP) é só registrada no log de Atividade — nunca
''' lançada pra quem chamou. Telemetria não pode, em hipótese nenhuma,
''' impedir o uso normal do ExportaXML.
'''
''' Endpoint verificado via Swagger
''' (https://dswebapi.duesoft.com.br/swagger/v2026817.172/swagger.json) em
''' 25/08/2026: o spec declara Bearer JWT global, mas um teste real confirmou
''' que a chamada funciona sem token (200 OK). Se isso mudar no futuro, as
''' chamadas passam a falhar com 401 — e, pelo design acima, isso só aparece
''' no log, sem quebrar o app.
''' </remarks>
Public Class TelemetriaService

    Private Const UrlRegistrarTela As String = "https://dswebapi.duesoft.com.br/api/DSMON/RegistrarTela"
    Private Const NomeSistema As String = "ExportaXML"

    ''' <summary>
    ''' HttpClient único e estático (reaproveitado entre chamadas) — criar um
    ''' novo a cada requisição pode esgotar sockets sob uso intenso; timeout
    ''' curto de propósito, pra nunca segurar a aplicação esperando a API.
    ''' </summary>
    Private Shared ReadOnly Cliente As New HttpClient With {
        .Timeout = TimeSpan.FromSeconds(5)
    }

    ''' <summary>
    ''' Dispara (sem esperar) o registro de acesso à tela informada, um
    ''' registro por CNPJ em <paramref name="cnpjs"/>. Fire-and-forget: volta
    ''' na hora pra quem chamou; qualquer erro fica só no log.
    ''' </summary>
    ''' <param name="tela">Identificador da tela (ex.: "EXPORTACAOXML").</param>
    ''' <param name="cnpjs">
    ''' CNPJs das empresas associadas a este acesso (normalmente todas as
    ''' empresas configuradas). CNPJs inválidos (diferentes de 14 dígitos
    ''' depois de remover pontuação) são ignorados e registrados no log — não
    ''' são "consertados" nem inventados.
    ''' </param>
    Public Shared Sub RegistrarAcessoTela(tela As String, cnpjs As IEnumerable(Of String))
        For Each cnpjOriginal In cnpjs
            Dim cnpjLimpo = LimparCnpj(cnpjOriginal)

            If cnpjLimpo.Length <> 14 Then
                LogService.RegistrarAtividade($"Telemetria: CNPJ inválido ignorado ao registrar tela {tela} (""{cnpjOriginal}"")")
                Continue For
            End If

            Task.Run(Function() EnviarAsync(tela, cnpjLimpo))
        Next
    End Sub

    ''' <summary>Remove tudo que não for dígito (pontos, barra, hífen) do CNPJ vindo do banco.</summary>
    Private Shared Function LimparCnpj(cnpj As String) As String
        If cnpj Is Nothing Then Return String.Empty
        Return Regex.Replace(cnpj, "[^0-9]", "")
    End Function

    Private Shared Async Function EnviarAsync(tela As String, cnpj As String) As Task
        Try
            Dim registro As New TelemetriaRegistro With {
                .Cnpj = cnpj,
                .Computador = Environment.MachineName,
                .Sistema = NomeSistema,
                .Tela = tela,
                .Acesso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                .Usuario = Environment.UserName
            }

            LogService.RegistrarAtividade($"Telemetria: enviando registro da tela {tela} (CNPJ {cnpj})")

            Dim json As String = JsonSerializer.Serialize(registro)

            Using conteudo As New StringContent(json, Encoding.UTF8, "application/json")
                Using resposta = Await Cliente.PostAsync(UrlRegistrarTela, conteudo)
                    If resposta.IsSuccessStatusCode Then
                        LogService.RegistrarAtividade($"Telemetria: registro da tela {tela} enviado com sucesso (CNPJ {cnpj})")
                    Else
                        LogService.RegistrarAtividade($"Telemetria: falha ao registrar tela {tela} (CNPJ {cnpj}) - HTTP {CInt(resposta.StatusCode)}")
                    End If
                End Using
            End Using

        Catch ex As Exception
            ' Sem internet, timeout, DNS, certificado, etc. — nunca deixa
            ' subir pra quem chamou (ver remarks da classe).
            LogService.RegistrarAtividade($"Telemetria: falha ao registrar tela {tela} (CNPJ {cnpj}) - {ex.Message}")
        End Try
    End Function

End Class
