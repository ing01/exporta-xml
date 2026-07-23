Imports Npgsql
Imports System.IO
Imports System.IO.Compression
Public Class ExportadorXML

    Public Shared Sub Exportar(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        caminhoZip As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean)

        Dim sql As String =
"SELECT
    chave_cfe,
    xml_autorizado,
    xml_cancelado,
    xml_inutilizacao_nfce,
    CASE
        WHEN TRIM(COALESCE(inutilizada, '')) = 'S' THEN 'INUTILIZADA'
        WHEN cancelado = 'S' THEN 'CANCELADA'
        ELSE 'EMITIDA'
    END AS status_cupom
FROM cupons
WHERE cod_empresa = @empresa
AND modelo = '65'
AND dt_impressao >= @inicio
AND dt_impressao < @fim"

        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

        ' Aplicar filtro por tipos de XML quando selecionar (Emitidos/Cancelados/Inutilizados)
        If Not (incluirEmitidos AndAlso incluirCancelados AndAlso incluirInutilizados) Then
            Dim filtros As New List(Of String)
            If incluirEmitidos Then
                filtros.Add("(COALESCE(inutilizada,'') <> 'S' AND COALESCE(cancelado,'') <> 'S')")
            End If

            If incluirCancelados Then
                filtros.Add("cancelado = 'S'")
            End If

            If incluirInutilizados Then
                filtros.Add("TRIM(COALESCE(inutilizada,'')) = 'S'")
            End If

            If filtros.Count > 0 Then
                sql &= " AND (" & String.Join(" OR ", filtros) & ")"
            End If
        End If

        sql &= vbCrLf & "ORDER BY dt_impressao"

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            Using reader = cmd.ExecuteReader()

                If File.Exists(caminhoZip) Then
                    File.Delete(caminhoZip)
                End If

                Using zip = ZipFile.Open(caminhoZip, ZipArchiveMode.Create)

                    While reader.Read()

                        Dim chave = reader("chave_cfe").ToString()
                        Dim status = reader("status_cupom").ToString()

                        Select Case status

                            Case "EMITIDA"

                                If incluirEmitidos Then
                                    ExportarXml(zip, chave & ".xml", reader("xml_autorizado"))
                                End If


                            Case "CANCELADA"

                                If incluirCancelados Then
                                    ExportarXml(zip, chave & "_cancelado.xml", reader("xml_cancelado"))
                                End If


                            Case "INUTILIZADA"

                                If incluirInutilizados Then
                                    ExportarXml(zip, chave & "_inutilizacao.xml", reader("xml_inutilizacao_nfce"))
                                End If

                        End Select

                    End While

                End Using

            End Using

        End Using

    End Sub

    Public Shared Sub ExportarXml(
    zip As ZipArchive,
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
        dataFinal As Date,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean) As Integer

        Dim sql As String

        If cod_empresa = 0 Then

            sql =
    "SELECT COUNT(*)
     FROM cupons
     WHERE dt_impressao >= @inicio
     AND dt_impressao < @fim"

        Else

            sql =
    "SELECT COUNT(*)
     FROM cupons
     WHERE cod_empresa = @empresa
     AND dt_impressao >= @inicio
     AND dt_impressao < @fim"

        End If

        ' Aplicar filtro por tipos de XML (quando não for selecionar "todos")
        If Not (incluirEmitidos AndAlso incluirCancelados AndAlso incluirInutilizados) Then
            Dim filtros As New List(Of String)
            If incluirEmitidos Then
                filtros.Add("(COALESCE(inutilizada,'') <> 'S' AND COALESCE(cancelado,'') <> 'S')")
            End If

            If incluirCancelados Then
                filtros.Add("cancelado = 'S'")
            End If

            If incluirInutilizados Then
                filtros.Add("TRIM(COALESCE(inutilizada,'')) = 'S'")
            End If

            If filtros.Count > 0 Then
                sql &= " AND (" & String.Join(" OR ", filtros) & ")"
            End If
        End If

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If

            Return Convert.ToInt32(cmd.ExecuteScalar())

        End Using

    End Function

    Public Shared Function BuscarCupons(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean) As DataTable

        Dim sql As String =
    "SELECT
    coo AS Cupom,
    cod_empresa AS Empresa,
    serie_nfce AS Serie,
    chave_cfe AS Chave,
    CASE
        WHEN TRIM(COALESCE(inutilizada,'')) = 'S' THEN 'INUTILIZADA'
        WHEN cancelado = 'S' THEN 'CANCELADA'
        ELSE 'EMITIDA'
    END AS Status,
    dt_impressao AS Data
FROM cupons
WHERE modelo = '65'
AND dt_impressao >= @inicio
AND dt_impressao < @fim"


        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

        If Not (incluirEmitidos AndAlso incluirCancelados AndAlso incluirInutilizados) Then

            Dim filtros As New List(Of String)

            If incluirEmitidos Then
                filtros.Add("(COALESCE(inutilizada,'') <> 'S' AND COALESCE(cancelado,'') <> 'S')")
            End If

            If incluirCancelados Then
                filtros.Add("cancelado = 'S'")
            End If

            If incluirInutilizados Then
                filtros.Add("TRIM(COALESCE(inutilizada,'')) = 'S'")
            End If

            If filtros.Count > 0 Then
                sql &= " AND (" & String.Join(" OR ", filtros) & ")"
            End If

        End If


        Dim tabela As New DataTable


        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))


            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If


            Using da As New NpgsqlDataAdapter(cmd)
                da.Fill(tabela)
            End Using

        End Using


        Return tabela

    End Function

End Class