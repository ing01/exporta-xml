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

End Class