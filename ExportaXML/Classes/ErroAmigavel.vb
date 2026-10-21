Imports System.IO
Imports System.Net.Http
Imports System.Net.Sockets
Imports System.Text.Json
Imports MailKit.Net.Smtp
Imports MailKit.Security
Imports Npgsql

''' <summary>
''' Converte exceções técnicas (quase sempre em inglês, vindas do Npgsql,
''' MailKit, System.Text.Json, etc.) numa frase em português que o usuário
''' final entende, dizendo o que provavelmente está errado e o que fazer.
''' </summary>
''' <remarks>
''' Só para MOSTRAR ao usuário. O log continua gravando a mensagem técnica
''' original (<c>ex.ToString()</c>) para o suporte — ver
''' <see cref="Registrar"/>, que grava as duas.
''' </remarks>
Public Class ErroAmigavel

    ''' <summary>Frase amigável (sem detalhe técnico) para a exceção informada.</summary>
    Public Shared Function Traduzir(ex As Exception) As String
        If ex Is Nothing Then Return "Ocorreu um erro desconhecido."

        ' Procura, da exceção mais externa para a mais interna, a primeira que sabemos explicar.
        Dim atual As Exception = ex
        Do While atual IsNot Nothing
            Dim texto As String = TraduzirUma(atual)
            If texto IsNot Nothing Then Return texto
            atual = atual.InnerException
        Loop

        ' Mensagens que o próprio programa escreveu em português passam direto.
        If TemAcentoOuPortugues(ex.Message) Then Return ex.Message

        Return "Ocorreu um erro inesperado. Se continuar, abra ""Ver log"" e envie o conteúdo ao suporte."
    End Function

    ''' <summary>Frase amigável + detalhe técnico entre parênteses (para telas de diagnóstico, onde o suporte precisa do original).</summary>
    Public Shared Function TraduzirComDetalhe(ex As Exception) As String
        Dim amigavel As String = Traduzir(ex)
        Dim tecnico As String = If(ex?.GetBaseException()?.Message, "").Trim()
        If tecnico = "" OrElse amigavel.Contains(tecnico) Then Return amigavel
        Return amigavel & vbCrLf & vbCrLf & "Detalhe técnico: " & tecnico
    End Function

    ''' <summary>Grava a mensagem amigável e a técnica completa no arquivo de log informado.</summary>
    Public Shared Sub Registrar(caminhoLog As String, contexto As String, ex As Exception)
        LogService.Registrar(caminhoLog, $"{contexto}: {Traduzir(ex)} | Técnico: {ex}")
    End Sub

    Private Shared Function TraduzirUma(ex As Exception) As String
        ' ---------- Banco de dados ----------
        Dim pg = TryCast(ex, PostgresException)
        If pg IsNot Nothing Then
            Select Case pg.SqlState
                Case "28P01", "28000"
                    Return "Usuário ou senha do banco de dados incorretos. Confira em Configurar Bancos."
                Case "3D000"
                    Return "O banco de dados informado não existe no servidor. Confira o nome em Configurar Bancos."
                Case "42P01", "42703"
                    Return "O banco de dados não tem uma tabela/coluna que o programa precisa (provavelmente o sistema da Duesoft está desatualizado nesse banco). Contate o suporte."
                Case "57014"
                    Return "O banco de dados demorou demais para responder e a consulta foi cancelada. Tente um período menor ou tente novamente mais tarde."
                Case "22021", "22P05"
                    Return "O banco tem um texto com caracteres inválidos (acentuação gravada na codificação errada) em algum cadastro ou XML da empresa, e o PostgreSQL não conseguiu ler. Anote o período e a empresa e contate o suporte."
                Case "53300"
                    Return "O servidor de banco de dados está com excesso de conexões. Tente novamente em alguns minutos."
                Case "42501"
                    Return "O usuário do banco não tem permissão para esta consulta. Contate o suporte."
            End Select
            Return "O banco de dados recusou a consulta. Contate o suporte."
        End If

        If TypeOf ex Is NpgsqlException Then
            ' Endereço e porta tentados (o Npgsql informa "Failed to connect to IP:PORTA").
            Dim alvo As String = ExtrairAlvo(ex)
            Dim onde As String = If(alvo <> "", $"em {alvo}", "no servidor de banco de dados")
            Dim socket = AcharSocket(ex)

            If socket IsNot Nothing Then
                Select Case socket.SocketErrorCode
                    Case SocketError.ConnectionRefused
                        Return $"O computador respondeu, mas recusou a conexão {onde}. Isso indica PORTA errada ou PostgreSQL parado (a porta padrão é 5432). Se o IP está certo, confira a porta e se o serviço do PostgreSQL está em execução."
                    Case SocketError.HostNotFound, SocketError.NoData, SocketError.TryAgain
                        Return $"Não foi possível encontrar o servidor {onde}. O nome/IP está escrito errado ou não existe nessa rede."
                    Case SocketError.TimedOut, SocketError.HostUnreachable, SocketError.NetworkUnreachable
                        Return $"Ninguém respondeu {onde}. Isso indica IP errado, computador desligado/fora da rede ou firewall bloqueando a porta."
                End Select
            End If

            If TemTimeout(ex) Then
                Return $"Ninguém respondeu {onde} a tempo. Isso indica IP errado, computador desligado/fora da rede ou firewall bloqueando a porta."
            End If

            Return $"Não foi possível conectar {onde}. Confira o IP/nome do servidor (se estiver errado, ninguém responde), a porta (se estiver errada, a conexão é recusada) e se o PostgreSQL está em execução."
        End If

        ' ---------- E-mail ----------
        ' O Gmail/Outlook, em vez de responder "senha inválida", muitas vezes FECHAM a conexão com um texto do tipo
        ' "5.7.8 Username and Password not accepted" / "BadCredentials" / "535" — o MailKit reporta isso como "servidor desconectou".
        If TemTextoDeCredencial(ex) Then
            Return "O servidor de e-mail recusou o usuário ou a senha. Confira em Configurar E-mail (em Gmail/Outlook é preciso usar uma ""senha de aplicativo"" de 16 letras, não a senha normal da conta, e a verificação em duas etapas precisa estar ligada)."
        End If

        If TypeOf ex Is AuthenticationException Then
            Return "O servidor de e-mail recusou o usuário ou a senha. Confira em Configurar E-mail (em Gmail/Outlook é preciso usar uma ""senha de aplicativo"", não a senha normal)."
        End If

        Dim smtpCmd = TryCast(ex, SmtpCommandException)
        If smtpCmd IsNot Nothing Then
            Select Case smtpCmd.ErrorCode
                Case SmtpErrorCode.RecipientNotAccepted
                    Return "O servidor de e-mail não aceitou o endereço do destinatário. Confira se o e-mail está escrito certo."
                Case SmtpErrorCode.SenderNotAccepted
                    Return "O servidor de e-mail não aceitou o remetente. O remetente precisa ser o mesmo e-mail da conta usada para enviar."
                Case SmtpErrorCode.MessageNotAccepted
                    Return "O servidor de e-mail recusou a mensagem (provavelmente o anexo é grande demais ou o envio foi bloqueado como spam)."
            End Select
            Return "O servidor de e-mail recusou o envio. Confira a conta em Configurar E-mail."
        End If

        If TypeOf ex Is SslHandshakeException Then
            Return "Não foi possível estabelecer conexão segura com o servidor de e-mail. Confira a porta (465 ou 587) e a opção SSL em Configurar E-mail."
        End If

        If TypeOf ex Is SmtpProtocolException OrElse TypeOf ex Is MailKit.ServiceNotConnectedException OrElse TypeOf ex Is MailKit.ServiceNotAuthenticatedException Then
            Return "A comunicação com o servidor de e-mail foi interrompida. Confira servidor, porta e SSL em Configurar E-mail."
        End If

        ' ---------- Rede ----------
        Dim sock = TryCast(ex, SocketException)
        If sock IsNot Nothing Then Return TraduzirSocket(sock, "o servidor")

        If TypeOf ex Is HttpRequestException Then
            Return "Sem acesso à internet ou o servidor não respondeu."
        End If

        If TypeOf ex Is TimeoutException OrElse TypeOf ex Is TaskCanceledException OrElse TypeOf ex Is OperationCanceledException Then
            Return "A operação demorou demais e foi cancelada. Verifique a conexão e tente novamente."
        End If

        ' ---------- Arquivos / configuração ----------
        If TypeOf ex Is JsonException Then
            Return "O arquivo de configuração está corrompido ou ilegível. O programa vai usar uma cópia de segurança, se existir; caso contrário será preciso reconfigurar."
        End If

        If TypeOf ex Is UnauthorizedAccessException Then
            Return "O Windows não permitiu acessar uma pasta ou arquivo. Execute o programa como administrador ou escolha outra pasta."
        End If

        If TypeOf ex Is DirectoryNotFoundException Then
            Return "A pasta informada não existe. Escolha outra pasta de destino."
        End If

        If TypeOf ex Is FileNotFoundException Then
            Return "Um arquivo necessário não foi encontrado."
        End If

        If TypeOf ex Is IOException Then
            If ex.Message.Contains("being used", StringComparison.OrdinalIgnoreCase) OrElse ex.Message.Contains("em uso", StringComparison.OrdinalIgnoreCase) Then
                Return "O arquivo está aberto em outro programa. Feche-o (ou o ZIP anterior) e tente novamente."
            End If
            If ex.Message.Contains("not enough space", StringComparison.OrdinalIgnoreCase) OrElse ex.Message.Contains("espaço", StringComparison.OrdinalIgnoreCase) Then
                Return "Não há espaço suficiente no disco para gerar o arquivo."
            End If
            Return "Não foi possível ler ou gravar um arquivo. Confira se a pasta existe e se há espaço no disco."
        End If

        If TypeOf ex Is OutOfMemoryException Then
            Return "O computador ficou sem memória para este período. Tente exportar um período menor."
        End If

        If TypeOf ex Is InvalidCastException OrElse TypeOf ex Is NullReferenceException OrElse TypeOf ex Is IndexOutOfRangeException Then
            Return "O programa encontrou um dado inesperado no banco (campo vazio ou em formato diferente do esperado). Envie o log ao suporte."
        End If

        Return Nothing
    End Function

    ''' <summary>Procura um SocketException em toda a cadeia de exceções internas (inclusive dentro de AggregateException).</summary>
    Private Shared Function TemTextoDeCredencial(ex As Exception) As Boolean
        Dim atual = ex
        Do While atual IsNot Nothing
            Dim m As String = If(atual.Message, "")
            If m.Contains("Username and Password not accepted", StringComparison.OrdinalIgnoreCase) OrElse
               m.Contains("BadCredentials", StringComparison.OrdinalIgnoreCase) OrElse
               m.Contains("5.7.8", StringComparison.Ordinal) OrElse
               m.Contains("5.7.139", StringComparison.Ordinal) OrElse
               m.Contains("authentication failed", StringComparison.OrdinalIgnoreCase) OrElse
               m.Contains("535 ", StringComparison.Ordinal) Then
                Return True
            End If
            atual = atual.InnerException
        Loop
        Return False
    End Function

    Private Shared Function AcharSocket(ex As Exception) As SocketException
        If ex Is Nothing Then Return Nothing
        Dim s = TryCast(ex, SocketException)
        If s IsNot Nothing Then Return s

        Dim agg = TryCast(ex, AggregateException)
        If agg IsNot Nothing Then
            For Each interna In agg.InnerExceptions
                Dim achou = AcharSocket(interna)
                If achou IsNot Nothing Then Return achou
            Next
        End If

        Return AcharSocket(ex.InnerException)
    End Function

    Private Shared Function TemTimeout(ex As Exception) As Boolean
        Dim atual = ex
        Do While atual IsNot Nothing
            If TypeOf atual Is TimeoutException OrElse TypeOf atual Is OperationCanceledException Then Return True
            If atual.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) OrElse atual.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase) Then Return True
            atual = atual.InnerException
        Loop
        Return False
    End Function

    ''' <summary>Extrai "IP:PORTA" de mensagens do tipo "Failed to connect to 192.168.0.4:5434"; "" se não houver.</summary>
    Private Shared Function ExtrairAlvo(ex As Exception) As String
        Dim atual = ex
        Do While atual IsNot Nothing
            Dim m = System.Text.RegularExpressions.Regex.Match(If(atual.Message, ""), "Failed to connect to\s+(\S+)")
            If m.Success Then Return m.Groups(1).Value.TrimEnd("."c, ","c)
            atual = atual.InnerException
        Loop
        Return ""
    End Function

    Private Shared Function TraduzirSocket(ex As SocketException, alvo As String) As String
        Select Case ex.SocketErrorCode
            Case SocketError.ConnectionRefused
                Return $"Conexão recusada por {alvo}. Confira o endereço, a porta e se o serviço está em execução."
            Case SocketError.HostNotFound, SocketError.NoData, SocketError.TryAgain
                Return $"Não foi possível encontrar {alvo}. Confira o nome/endereço informado e a conexão com a internet/rede."
            Case SocketError.TimedOut
                Return $"{Char.ToUpper(alvo(0))}{alvo.Substring(1)} não respondeu a tempo. Confira a rede, o firewall e o endereço informado."
            Case SocketError.NetworkUnreachable, SocketError.HostUnreachable
                Return "A rede está inacessível. Confira a conexão com a internet/rede."
        End Select
        Return $"Falha de rede ao falar com {alvo}. Confira a conexão e o endereço informado."
    End Function

    ''' <summary>Heurística simples: mensagens com acento ou palavras comuns em português foram escritas pelo próprio programa.</summary>
    Private Shared Function TemAcentoOuPortugues(texto As String) As Boolean
        If String.IsNullOrWhiteSpace(texto) Then Return False
        If texto.IndexOfAny("áàâãéêíóôõúçÁÀÂÃÉÊÍÓÔÕÚÇ".ToCharArray()) >= 0 Then Return True
        For Each palavra In {"inválido", "não ", "Selecione", "Informe", "Falha", "Erro ", "configur"}
            If texto.Contains(palavra, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

End Class
