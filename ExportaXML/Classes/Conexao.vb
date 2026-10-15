Imports Npgsql

Public Class Conexao

    Public Shared Function Abrir(
        servidor As String,
        porta As Integer,
        banco As String,
        usuario As String,
        senha As String,
        Optional ByRef codificacaoDetectada As String = Nothing
    ) As NpgsqlConnection

        If porta <= 0 Then porta = 5432
        If String.IsNullOrWhiteSpace(servidor) Then servidor = "localhost"
        If String.IsNullOrWhiteSpace(banco) Then banco = "banco"
        If String.IsNullOrWhiteSpace(usuario) Then usuario = "postgres"
        If String.IsNullOrWhiteSpace(senha) Then senha = "ds_due339"

        ' Builder em vez de concatenar texto: senha com ";" ou "=" não quebra a
        ' conexão nem injeta outros parâmetros. CommandTimeout alto porque a
        ' exportação de um mês inteiro em banco grande passa fácil dos 30s padrão;
        ' Timeout curto para falhar rápido (e com mensagem clara) se o servidor não existir.
        Dim construtor As New NpgsqlConnectionStringBuilder With {
            .Host = servidor.Trim(),
            .Port = porta,
            .Database = banco.Trim(),
            .Username = usuario.Trim(),
            .Password = senha,
            .Timeout = 15,
            .CommandTimeout = 900
        }

        Dim conn As New NpgsqlConnection(construtor.ConnectionString)

        Try
            conn.Open()

            Dim codificacaoServidor As String = ""

            Using cmd As New NpgsqlCommand("SHOW server_encoding;", conn)
                codificacaoServidor =
                    cmd.ExecuteScalar()?.ToString()?.ToUpperInvariant()
            End Using

            codificacaoDetectada = codificacaoServidor

            If codificacaoServidor = "SQL_ASCII" Then

                Using cmd As New NpgsqlCommand(
                    "SET client_encoding TO SQL_ASCII;",
                    conn
                )
                    cmd.ExecuteNonQuery()
                End Using

            End If
        Catch
            conn.Dispose()
            Throw
        End Try

        Return conn

    End Function

End Class