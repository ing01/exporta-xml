Imports Npgsql

''' <summary>
''' Ponto único de abertura de conexão com o PostgreSQL do cliente.
''' </summary>
Public Class Conexao

    ''' <summary>
    ''' Monta a connection string e abre (já conectada) uma conexão Npgsql.
    ''' </summary>
    ''' <param name="servidor">Host/IP do PostgreSQL. Vazio vira "localhost".</param>
    ''' <param name="porta">Porta do PostgreSQL. Valor &lt;= 0 vira a porta padrão 5432.</param>
    ''' <param name="banco">Nome do banco de dados.</param>
    ''' <param name="usuario">Usuário do PostgreSQL.</param>
    ''' <param name="senha">Senha do usuário do PostgreSQL.</param>
    ''' <returns>Uma <see cref="NpgsqlConnection"/> já aberta (chamador deve dar Dispose/Using).</returns>
    ''' <remarks>
    ''' ATENÇÃO (segurança): se banco/usuário/senha vierem vazios, o método cai em
    ''' valores padrão fixos no código (incluindo uma senha), em vez de falhar.
    ''' Isso existe para não quebrar em instalações mal configuradas, mas significa
    ''' que uma configuração vazia tenta conectar silenciosamente com credenciais
    ''' padrão em vez de avisar o usuário. Como o executável agora é distribuído
    ''' publicamente (atualização automática via GitHub), evite depender desses
    ''' valores padrão para qualquer ambiente real.
    ''' </remarks>
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