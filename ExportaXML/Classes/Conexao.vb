Imports Npgsql

Public Class Conexao

    Public Shared Function Abrir(
        servidor As String,
        porta As Integer,
        banco As String,
        usuario As String,
        senha As String) As NpgsqlConnection

        ' Usa porta padrão do PostgreSQL se valor inválido for informado
        If porta <= 0 Then
            porta = 5432
        End If

        ' Substitui servidor vazio por localhost para evitar ArgumentNullException do Npgsql
        If String.IsNullOrWhiteSpace(servidor) Then
            servidor = "localhost"
        End If

        If String.IsNullOrWhiteSpace(banco) Then
            banco = "banco"
        End If

        If String.IsNullOrWhiteSpace(usuario) Then
            usuario = "postgres"
        End If

        If String.IsNullOrWhiteSpace(senha) Then
            senha = "ds_due339"
        End If

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