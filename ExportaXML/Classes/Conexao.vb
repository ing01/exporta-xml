Imports Npgsql

Public Class Conexao

    Public Shared Function Abrir(
        servidor As String,
        porta As Integer,
        banco As String,
        usuario As String,
        senha As String) As NpgsqlConnection

        Dim connectionString As String =
            $"Host={servidor};" &
            $"Port={porta};" &
            $"Database={banco};" &
            $"Username={usuario};" &
            $"Password={senha};"

        Dim conn As New NpgsqlConnection(connectionString)

        conn.Open()

        Return conn

    End Function

End Class