Imports Npgsql

Public Class EmpresaService

    Public Shared Function Buscar(
        conn As NpgsqlConnection,
        codigo As Integer) As Empresa

        Dim sql As String =
            "SELECT codigo, cnpj, razao
             FROM empresas
             WHERE codigo = @codigo"

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@codigo", codigo)

            Using reader = cmd.ExecuteReader()

                If reader.Read() Then

                    Dim empresa As New Empresa()

                    empresa.Codigo = Convert.ToInt32(reader("codigo"))
                    empresa.CNPJ = reader("cnpj").ToString()
                    empresa.Razao = reader("razao").ToString()

                    Return empresa

                End If

            End Using

        End Using

        Return Nothing

    End Function

    Public Shared Function Listar(conn As NpgsqlConnection) As List(Of EmpresaItem)

        Dim lista As New List(Of EmpresaItem)

        lista.Add(New EmpresaItem With {
            .Codigo = 0,
            .Nome = "Todas as empresas"
        })

        Dim sql =
    "SELECT codigo, razao
FROM empresas
ORDER BY codigo"

        Using cmd As New NpgsqlCommand(sql, conn)

            Using rd = cmd.ExecuteReader()

                While rd.Read()

                    lista.Add(New EmpresaItem With {
                        .Codigo = rd.GetInt32(0),
                        .Nome = rd.GetString(1)
                    })

                End While

            End Using

        End Using

        Return lista

    End Function

End Class