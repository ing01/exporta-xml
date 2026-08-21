Imports Npgsql

''' <summary>
''' Consultas relacionadas às empresas cadastradas no banco (tabela "empresas").
''' </summary>
Public Class EmpresaService

    ''' <summary>
    ''' Busca uma empresa específica pelo código.
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <param name="codigo">Código da empresa (coluna "codigo").</param>
    ''' <returns>O objeto <see cref="Empresa"/> encontrado, ou Nothing se não existir.</returns>
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

    ''' <summary>
    ''' Lista todas as empresas cadastradas, para popular o combo "Empresa" da tela.
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <returns>
    ''' Lista de <see cref="EmpresaItem"/> sempre começando com um item sentinela
    ''' Código=0 / Nome="Todas as empresas" — é assim que a tela representa
    ''' "sem filtro de empresa" / "exportar todas de uma vez".
    ''' </returns>
    Public Shared Function Listar(conn As NpgsqlConnection) As List(Of EmpresaItem)

        Dim lista As New List(Of EmpresaItem)

        lista.Add(New EmpresaItem With {
            .Codigo = 0,
            .Nome = "Todas as empresas"
        })

        Dim sql =
    "SELECT codigo, razao, cnpj
FROM empresas
ORDER BY codigo"

        Using cmd As New NpgsqlCommand(sql, conn)

            Using rd = cmd.ExecuteReader()

                While rd.Read()

                    lista.Add(New EmpresaItem With {
                        .Codigo = rd.GetInt32(0),
                        .Nome = rd.GetString(1),
                        .CNPJ = rd.GetString(2)
                    })

                End While

            End Using

        End Using

        Return lista

    End Function

End Class