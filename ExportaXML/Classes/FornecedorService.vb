Imports Npgsql

''' <summary>
''' Consultas relacionadas aos fornecedores (tabela "fornecedores") — usado
''' pelo filtro de Fornecedor da Direção "Entrada".
''' </summary>
Public Class FornecedorService

    ''' <summary>
    ''' Lista todos os fornecedores cadastrados, para popular o combo "Fornecedor".
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <returns>
    ''' Lista de <see cref="FornecedorItem"/> sempre começando com um item sentinela
    ''' Código=0 / Nome="Todos os fornecedores" (mesmo padrão do combo de Empresa).
    ''' </returns>
    Public Shared Function Listar(conn As NpgsqlConnection) As List(Of FornecedorItem)

        Dim lista As New List(Of FornecedorItem)

        lista.Add(New FornecedorItem With {
            .Codigo = 0,
            .Nome = "Todos os fornecedores"
        })

        Dim sql =
            "SELECT codigo, nome, cnpj_cpf
             FROM fornecedores
             ORDER BY nome"

        Using cmd As New NpgsqlCommand(sql, conn)
            Using rd = cmd.ExecuteReader()
                While rd.Read()
                    lista.Add(New FornecedorItem With {
                        .Codigo = rd.GetInt32(0),
                        .Nome = If(rd.IsDBNull(1), "", rd.GetString(1)),
                        .CNPJ = If(rd.IsDBNull(2), "", rd.GetString(2))
                    })
                End While
            End Using
        End Using

        Return lista

    End Function

End Class
