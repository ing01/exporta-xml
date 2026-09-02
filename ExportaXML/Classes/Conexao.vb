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

        Dim connString As String =
            $"Host={servidor};" &
            $"Port={porta};" &
            $"Database={banco};" &
            $"Username={usuario};" &
            $"Password={senha};"

        Dim conn As New NpgsqlConnection(connString)

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

        Return conn

    End Function

End Class