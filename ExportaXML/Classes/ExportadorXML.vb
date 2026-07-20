Imports Npgsql
Imports System.IO
Imports System.IO.Compression
Public Class ExportadorXML

    Public Shared Sub Exportar(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        caminhoZip As String)

        Dim sql As String =
"SELECT
    chave_cfe,
    xml_autorizado,
    xml_cancelado,
    xml_inutilizacao_nfce
FROM cupons
WHERE cod_empresa = @empresa
AND dt_impressao >= @inicio
AND dt_impressao < @fim
ORDER BY dt_impressao"

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            Using reader = cmd.ExecuteReader()

                Using zip = ZipFile.Open(caminhoZip, ZipArchiveMode.Create)

                    While reader.Read()

                        Dim chave = reader("chave_cfe").ToString()

                        ExportarXml(zip, chave & ".xml", reader("xml_autorizado"))
                        ExportarXml(zip, chave & "_cancelado.xml", reader("xml_cancelado"))
                        ExportarXml(zip, chave & "_inutilizacao.xml", reader("xml_inutilizacao_nfce"))

                    End While

                End Using

            End Using

        End Using

    End Sub

    Private Shared Sub ExportarXml(zip As ZipArchive,
                                   nomeArquivo As String,
                                   valor As Object)

        If valor Is DBNull.Value Then Return

        Dim xml = valor.ToString()

        If String.IsNullOrWhiteSpace(xml) Then Return

        Dim entry = zip.CreateEntry(nomeArquivo)

        Using sw As New StreamWriter(entry.Open())

            sw.Write(xml)

        End Using

    End Sub

    Public Shared Function ContarXMLs(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date) As Integer

        Dim sql As String =
"SELECT COUNT(*)
FROM cupons
WHERE cod_empresa = @empresa
AND dt_impressao >= @inicio
AND dt_impressao < @fim"

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            Return Convert.ToInt32(cmd.ExecuteScalar())

        End Using

    End Function

End Class