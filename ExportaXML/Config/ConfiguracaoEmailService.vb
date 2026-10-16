Imports System.IO
Imports Npgsql

Public Class ConfiguracaoEmailService

    ''' <summary>
    ''' Arquivo de texto simples com a última senha decifrada de cada empresa
    ''' no modo "Sincronizar do banco" — esse modo, por padrão, nunca persiste
    ''' a senha em lugar nenhum (busca e decifra de novo a cada envio/teste);
    ''' este arquivo existe só porque foi pedido explicitamente pra sempre ter
    ''' a senha em texto puro disponível em disco, mesmo nesse modo. Fica ao
    ''' lado do config.json (mesma pasta, ver <see cref="ConfiguracaoService"/>).
    ''' ATENÇÃO: fica em texto puro, sem criptografia — mesmo nível de proteção
    ''' que o config.json já tem hoje pro modo Manual.
    ''' </summary>
    Private Shared ReadOnly CaminhoCacheSenhas As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ExportaXML", "SenhasEmailSincronizado.txt")

    ''' <summary>
    ''' Grava (substituindo a entrada anterior da mesma empresa, se houver) a
    ''' configuração de e-mail que <see cref="BuscarConfiguracao"/> acabou de
    ''' decifrar. Best-effort: qualquer falha aqui é engolida — nunca deve
    ''' interromper quem chamou (nem o envio de e-mail, nem o teste).
    ''' </summary>
    Private Shared Sub SalvarCacheSenhaLocal(codEmpresa As Integer, servidor As String, porta As Integer, usuario As String, senha As String)
        Try
            Dim pasta = Path.GetDirectoryName(CaminhoCacheSenhas)
            If Not Directory.Exists(pasta) Then
                Directory.CreateDirectory(pasta)
            End If

            Dim linhas As New List(Of String)
            If File.Exists(CaminhoCacheSenhas) Then
                linhas.AddRange(File.ReadAllLines(CaminhoCacheSenhas))
            End If

            Dim prefixo As String = $"Empresa {codEmpresa}: "
            linhas.RemoveAll(Function(l) l.StartsWith(prefixo))

            linhas.Add($"{prefixo}servidor={servidor}; porta={porta}; usuario={usuario}; senha={senha}; atualizado_em={DateTime.Now:yyyy-MM-dd HH:mm:ss}")

            File.WriteAllLines(CaminhoCacheSenhas, linhas)
        Catch
            ' Nunca deixa isso quebrar o envio/teste de e-mail.
        End Try
    End Sub

    ''' <summary>
    ''' Resolve qual configuração de SMTP usar pra enviar o e-mail de UMA
    ''' empresa específica — não confundir com <see cref="BuscarConfiguracao"/>,
    ''' que só lê o que está gravado na tabela <c>conf_nfe</c> do banco do
    ''' cliente. Ordem de prioridade:
    ''' 1) Override da empresa em <see cref="Configuracoes.ConfiguracoesEmailPorEmpresa"/>
    '''    com <c>Origem="Manual"</c> — usa os campos salvos ali.
    ''' 2) Mesmo override, mas com <c>Origem="Banco"</c> — busca de
    '''    <see cref="BuscarConfiguracao"/> pra ESSA empresa, na conexão informada.
    ''' 3) Sem override (ou <c>Origem="Padrao"</c>, ou não achou nada no passo 2):
    '''    cai pra configuração Global (<paramref name="cfg"/>.ServidorSMTP etc.),
    '''    que por sua vez já pode ser Manual ou sincronizada do banco
    '''    (<paramref name="cfg"/>.OrigemConfiguracao) — mesmo comportamento de
    '''    sempre pra quem nunca configurou nada por empresa.
    ''' </summary>
    ''' <param name="conn">
    ''' Conexão já aberta no banco AO QUAL A EMPRESA PERTENCE (não necessariamente
    ''' o primeiro banco cadastrado) — necessária só pros passos que consultam
    ''' <c>conf_nfe</c>. Pode ser Nothing; nesse caso esses passos são pulados.
    ''' </param>
    Public Shared Function ResolverConfiguracaoParaEmpresa(
        cfg As Configuracoes,
        conn As NpgsqlConnection,
        codigoEmpresa As Integer,
        Optional nomeBanco As String = Nothing) As (Servidor As String, Porta As Integer, Usuario As String, Senha As String, Remetente As String, UsarSSL As Boolean)

        Dim override = EscopoEmpresa.ConfigEmailDaEmpresa(cfg, codigoEmpresa, nomeBanco)

        If override IsNot Nothing Then
            If override.Origem = "Manual" Then
                Return (override.ServidorSMTP, override.PortaSMTP, override.UsuarioSMTP, override.SenhaSMTP,
                        If(String.IsNullOrWhiteSpace(override.EmailRemetente), override.UsuarioSMTP, override.EmailRemetente),
                        override.UsarSSL)

            ElseIf override.Origem = "Banco" Then
                Try
                    If conn IsNot Nothing Then
                        Dim servidor As String = "", usuario As String = "", senha As String = ""
                        Dim porta As Integer = 0
                        Dim ssl As Boolean = False
                        If BuscarConfiguracao(conn, codigoEmpresa, servidor, porta, usuario, senha, ssl) Then
                            Return (servidor, porta, usuario, senha, usuario, ssl)
                        End If
                    End If
                Catch
                    ' Falha ao consultar o banco: cai pro Global abaixo.
                End Try
            End If
        End If

        ' Sem override específico (ou "Banco" sem sucesso) — Global, com o mesmo
        ' fallback banco->manual que a configuração Global sempre teve.
        If cfg.OrigemConfiguracao = "Banco" AndAlso conn IsNot Nothing Then
            Try
                Dim servidorG As String = "", usuarioG As String = "", senhaG As String = ""
                Dim portaG As Integer = 0
                Dim sslG As Boolean = False
                If BuscarConfiguracao(conn, 0, servidorG, portaG, usuarioG, senhaG, sslG) Then
                    Return (servidorG, portaG, usuarioG, senhaG, usuarioG, sslG)
                End If
            Catch
            End Try
        End If

        Return (cfg.ServidorSMTP, cfg.PortaSMTP, cfg.UsuarioSMTP, cfg.SenhaSMTP, cfg.EmailRemetente, cfg.UsarSSL)
    End Function

    Public Shared Function BuscarConfiguracao(
        conn As NpgsqlConnection,
        codEmpresa As Integer,
        ByRef servidor As String,
        ByRef porta As Integer,
        ByRef usuario As String,
        ByRef senha As String,
        ByRef usarSSL As Boolean) As Boolean

        Dim sql As String
        If codEmpresa = 0 Then
            sql = "SELECT servidoremail, email, senhaemail, porta_smtp FROM conf_nfe WHERE codigo IS NULL OR codigo = 0 LIMIT 1"
        Else
            sql = "SELECT servidoremail, email, senhaemail, porta_smtp FROM conf_nfe WHERE codigo = @codEmpresa LIMIT 1"
        End If

        Using cmd As New NpgsqlCommand(sql, conn)
            If codEmpresa <> 0 Then
                cmd.Parameters.AddWithValue("@codEmpresa", codEmpresa)
            End If

            Using reader = cmd.ExecuteReader()
                If reader.Read() Then
                    servidor = reader("servidoremail").ToString().Trim()
                    usuario = reader("email").ToString().Trim()
                    Dim senhaCriptografada As String = reader("senhaemail").ToString().Trim()

                    ' Lê a porta tratando DBNull
                    If reader("porta_smtp") Is DBNull.Value Then
                        porta = 0
                    Else
                        porta = Convert.ToInt32(reader("porta_smtp"))
                    End If

                    ' Valida se tem dados mínimos
                    If String.IsNullOrEmpty(servidor) OrElse String.IsNullOrEmpty(usuario) Then
                        Return False
                    End If

                    senha = CriptografiaHelper.Descriptografar(senhaCriptografada)

                    ' Define SSL com base na porta (se porta for 465 => SSL, 587 => STARTTLS, 0 => false)
                    usarSSL = (porta = 465)

                    SalvarCacheSenhaLocal(codEmpresa, servidor, porta, usuario, senha)

                    Return True
                End If
            End Using
        End Using

        Return False
    End Function
End Class