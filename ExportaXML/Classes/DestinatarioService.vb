Imports Npgsql

''' <summary>
''' Destinatários cadastrados NO BANCO do cliente (tabela conf_nfe_destinatarios).
''' Essa tabela só existe em versões mais novas do sistema da Duesoft: quando ela
''' não existe (erro 42P01 do PostgreSQL), isso NÃO é falha — simplesmente não há
''' destinatário no banco, e os cadastrados no próprio ExportaXML (config.json) são
''' usados. Antes, cada abertura da tela de e-mail gravava um "erro" em inglês no log.
''' </summary>
Public Class DestinatarioService

    Private Const TabelaInexistente As String = "42P01"

    Public Shared Function ObterPadrao(conn As NpgsqlConnection, codigoEmpresa As Integer) As String
        If conn Is Nothing Then Return String.Empty
        Dim sql As String = "SELECT destinatario_email FROM conf_nfe_destinatarios WHERE codigo_empresa = @codigo AND ativo = true ORDER BY id LIMIT 1"
        Try
            Using cmd As New NpgsqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@codigo", codigoEmpresa)
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Return reader("destinatario_email").ToString()
                    End If
                End Using
            End Using
        Catch ex As PostgresException When ex.SqlState = TabelaInexistente
            ' Banco sem a tabela: sem destinatários no banco (normal).
        End Try
        Return String.Empty
    End Function

    Public Shared Function ListarPorEmpresa(conn As NpgsqlConnection, codigoEmpresa As Integer) As List(Of String)
        Dim lista As New List(Of String)()
        If conn Is Nothing Then Return lista
        Dim sql As String = "SELECT destinatario_email FROM conf_nfe_destinatarios WHERE codigo_empresa = @codigo AND ativo = true ORDER BY id"
        Try
            Using cmd As New NpgsqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@codigo", codigoEmpresa)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        lista.Add(reader("destinatario_email").ToString())
                    End While
                End Using
            End Using
        Catch ex As PostgresException When ex.SqlState = TabelaInexistente
            ' Banco sem a tabela: lista vazia (normal).
        End Try
        Return lista
    End Function

    Public Shared Sub Inserir(conn As NpgsqlConnection, codigoEmpresa As Integer, destinatarioEmail As String, descricao As String)
        If conn Is Nothing Then Exit Sub
        Dim sql As String = "INSERT INTO conf_nfe_destinatarios (codigo_empresa, destinatario_email, descricao, ativo) VALUES (@codigo, @email, @descricao, true)"
        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@codigo", codigoEmpresa)
            cmd.Parameters.AddWithValue("@email", destinatarioEmail)
            cmd.Parameters.AddWithValue("@descricao", If(descricao, String.Empty))
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Shared Sub Remover(conn As NpgsqlConnection, codigoEmpresa As Integer, destinatarioEmail As String)
        If conn Is Nothing Then Exit Sub
        Dim sql As String = "DELETE FROM conf_nfe_destinatarios WHERE codigo_empresa = @codigo AND destinatario_email = @email"
        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@codigo", codigoEmpresa)
            cmd.Parameters.AddWithValue("@email", destinatarioEmail)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Shared Sub Desativar(conn As NpgsqlConnection, id As Integer)
        If conn Is Nothing Then Exit Sub
        Dim sql As String = "UPDATE conf_nfe_destinatarios SET ativo = false WHERE id = @id"
        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@id", id)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
