Imports Npgsql
Imports System.IO
Imports System.IO.Compression
Public Class ExportadorXML
    Public Shared Sub ExportarNFCe(
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
        xml_gerado,
        cancelado,
        inutilizada
     FROM cupons
     WHERE dt_impressao >= @inicio
       AND dt_impressao < @fim"

        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

        If Not (incluirEmitidos And incluirCancelados And incluirInutilizados) Then

            Dim filtros As New List(Of String)

            If incluirEmitidos Then
                filtros.Add("(COALESCE(cancelado,'') <> 'S' AND COALESCE(inutilizada,'') <> 'S')")
            End If

            If incluirCancelados Then
                filtros.Add("cancelado='S'")
            End If

            If incluirInutilizados Then
                filtros.Add("TRIM(COALESCE(inutilizada,''))='S'")
            End If

            sql &= " AND (" & String.Join(" OR ", filtros) & ")"

        End If

        sql &= " ORDER BY dt_impressao"

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If

            Using reader = cmd.ExecuteReader()

                Using zip = AbrirZip(caminhoZip)

                    While reader.Read()

                        Dim chave = reader("chave_cfe").ToString()

                        Dim cancelado = reader("cancelado").ToString.Trim.ToUpper()

                        Dim inutilizada = reader("inutilizada").ToString.Trim.ToUpper()

                        If inutilizada = "S" Then

                            ExportarXml(
                            zip,
                            chave & "_inutilizacao.xml",
                            reader("xml_gerado"))

                        ElseIf cancelado = "S" Then

                            ExportarXml(
                            zip,
                            chave & "_cancelado.xml",
                            reader("xml_cancelado"))

                        Else

                            ExportarXml(
                            zip,
                            chave & ".xml",
                            reader("xml_autorizado"))

                        End If

                    End While

                End Using

            End Using

        End Using

    End Sub

    Public Shared Sub ExportarNFe(
    conn As NpgsqlConnection,
    cod_empresa As Integer,
    dataInicial As Date,
    dataFinal As Date,
    caminhoZip As String,
    incluirEmitidos As Boolean,
    incluirCancelados As Boolean,
    modelo As Integer)

        Dim sql As String =
    "SELECT
        num_nota,
        arq_xml,
        nfe_protocan,
        cod_empresa
     FROM vendas
     WHERE dt_emissao >= @inicio
       AND dt_emissao < @fim"

        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

        'Emitidas / Canceladas
        If incluirEmitidos Xor incluirCancelados Then

            If incluirEmitidos Then
                sql &= " AND COALESCE(nfe_protocan,'') = ''"
            Else
                sql &= " AND COALESCE(nfe_protocan,'') <> ''"
            End If

        End If

        sql &= " ORDER BY dt_emissao, num_nota"

        Using cmd As New NpgsqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If

            Using reader = cmd.ExecuteReader()

                Using zip = AbrirZip(caminhoZip)

                    While reader.Read()

                        If reader("arq_xml") Is DBNull.Value Then
                            Continue While
                        End If

                        Dim numero As String = reader("num_nota").ToString()

                        Dim cancelada As Boolean =
                        Not String.IsNullOrWhiteSpace(reader("nfe_protocan").ToString())

                        Dim nomeArquivo As String

                        If cancelada Then
                            nomeArquivo = numero & "_cancelada.xml"
                        Else
                            nomeArquivo = numero & ".xml"
                        End If

                        ExportarXml(
                        zip,
                        nomeArquivo,
                        reader("arq_xml"))

                    End While

                End Using

            End Using

        End Using

    End Sub

    Public Shared Sub Exportar(
    conn As NpgsqlConnection,
    cod_empresa As Integer,
    dataInicial As Date,
    dataFinal As Date,
    caminhoZip As String,
    incluirEmitidos As Boolean,
    incluirCancelados As Boolean,
    incluirInutilizados As Boolean,
    modelo As Integer)

        If modelo = 65 Then

            ExportarNFCe(
            conn,
            cod_empresa,
            dataInicial,
            dataFinal,
            caminhoZip,
            incluirEmitidos,
            incluirCancelados,
            incluirInutilizados)

        ElseIf modelo = 55 Then

            ExportarNFe(
            conn,
            cod_empresa,
            dataInicial,
            dataFinal,
            caminhoZip,
            incluirEmitidos,
            incluirCancelados,
            modelo)

        Else

            If File.Exists(caminhoZip) Then
                File.Delete(caminhoZip)
            End If

            ExportarNFCe(
            conn,
            cod_empresa,
            dataInicial,
            dataFinal,
            caminhoZip,
            incluirEmitidos,
            incluirCancelados,
            incluirInutilizados)

            ExportarNFe(
            conn,
            cod_empresa,
            dataInicial,
            dataFinal,
            caminhoZip,
            incluirEmitidos,
            incluirCancelados,
            modelo)

        End If

    End Sub

    Private Shared Function AbrirZip(caminho As String) As ZipArchive

        Dim modo As ZipArchiveMode

        If File.Exists(caminho) Then
            modo = ZipArchiveMode.Update
        Else
            modo = ZipArchiveMode.Create
        End If

        Return ZipFile.Open(caminho, modo)

    End Function

    Public Shared Sub ExportarXml(
    zip As ZipArchive,
    nomeArquivo As String,
    valor As Object)

        If valor Is DBNull.Value Then Return

        If valor Is Nothing Then Return

        Dim xml As String = valor.ToString()

        If String.IsNullOrWhiteSpace(xml) Then Return

        Dim entry = zip.CreateEntry(nomeArquivo)

        Using stream = entry.Open()

            Using sw As New StreamWriter(stream, System.Text.Encoding.UTF8)

                sw.Write(xml)

            End Using

        End Using

    End Sub

    Public Shared Function ContarXMLs(
    conn As NpgsqlConnection,
    cod_empresa As Integer,
    dataInicial As Date,
    dataFinal As Date,
    incluirEmitidos As Boolean,
    incluirCancelados As Boolean,
    incluirInutilizados As Boolean,
    modelo As Integer) As Integer

        '================ NFC-e ==================
        If modelo = 65 Then

            Dim sql As String =
"SELECT COUNT(*)
FROM cupons
WHERE dt_impressao >= @inicio
AND dt_impressao < @fim"

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
            End If

            If Not (incluirEmitidos And incluirCancelados And incluirInutilizados) Then

                Dim filtros As New List(Of String)

                If incluirEmitidos Then
                    filtros.Add("(COALESCE(inutilizada,'') <> 'S' AND COALESCE(cancelado,'') <> 'S')")
                End If

                If incluirCancelados Then
                    filtros.Add("cancelado='S'")
                End If

                If incluirInutilizados Then
                    filtros.Add("TRIM(COALESCE(inutilizada,''))='S'")
                End If

                sql &= " AND (" & String.Join(" OR ", filtros) & ")"

            End If

            Using cmd As New NpgsqlCommand(sql, conn)

                cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
                cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

                If cod_empresa <> 0 Then
                    cmd.Parameters.AddWithValue("@empresa", cod_empresa)
                End If

                Return Convert.ToInt32(cmd.ExecuteScalar())

            End Using

            '================ NFe ==================
        ElseIf modelo = 55 Then

            Dim sql As String =
"SELECT COUNT(*)
FROM vendas
WHERE dt_emissao >= @inicio
AND dt_emissao < @fim
AND COALESCE(arq_xml,'') <> ''"

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
            End If

            If Not (incluirEmitidos And incluirCancelados) Then

                Dim filtros As New List(Of String)

                If incluirEmitidos Then
                    filtros.Add("COALESCE(nfe_protocan,'') = ''")
                End If

                If incluirCancelados Then
                    filtros.Add("COALESCE(nfe_protocan,'') <> ''")
                End If

                sql &= " AND (" & String.Join(" OR ", filtros) & ")"

            End If

            Using cmd As New NpgsqlCommand(sql, conn)

                cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
                cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

                If cod_empresa <> 0 Then
                    cmd.Parameters.AddWithValue("@empresa", cod_empresa)
                End If

                Return Convert.ToInt32(cmd.ExecuteScalar())

            End Using

            '================ Ambos ==================
        Else

            Dim total As Integer = 0

            total += ContarXMLs(
            conn,
            cod_empresa,
            dataInicial,
            dataFinal,
            incluirEmitidos,
            incluirCancelados,
            incluirInutilizados,
            65)

            total += ContarXMLs(
            conn,
            cod_empresa,
            dataInicial,
            dataFinal,
            incluirEmitidos,
            incluirCancelados,
            incluirInutilizados,
            55)

            Return total

        End If

    End Function

    Public Shared Function BuscarCupons(
    conn As NpgsqlConnection,
    cod_empresa As Integer,
    dataInicial As Date,
    dataFinal As Date,
    incluirEmitidos As Boolean,
    incluirCancelados As Boolean,
    incluirInutilizados As Boolean,
    modelo As Integer) As DataTable

        Dim sql As String = ""

        '==========================
        ' NFC-e
        '==========================
        If modelo = 65 Then

            sql =
    "SELECT
    'NFC-e' AS Modelo,
    coo AS Documento,
    cod_empresa AS Empresa,
    serie_nfce AS Serie,
    chave_cfe AS Chave,
    CASE
        WHEN TRIM(COALESCE(inutilizada,''))='S' THEN 'INUTILIZADA'
        WHEN TRIM(COALESCE(cancelado,''))='S' THEN 'CANCELADA'
        ELSE 'EMITIDA'
    END AS Status,
    dt_impressao AS Data
FROM cupons
WHERE dt_impressao >= @inicio
AND dt_impressao < @fim"

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa=@empresa"
            End If

            If Not (incluirEmitidos And incluirCancelados And incluirInutilizados) Then

                Dim filtros As New List(Of String)

                If incluirEmitidos Then
                    filtros.Add("(TRIM(COALESCE(cancelado,''))<>'S' AND TRIM(COALESCE(inutilizada,''))<>'S')")
                End If

                If incluirCancelados Then
                    filtros.Add("TRIM(COALESCE(cancelado,''))='S'")
                End If

                If incluirInutilizados Then
                    filtros.Add("TRIM(COALESCE(inutilizada,''))='S'")
                End If

                sql &= " AND (" & String.Join(" OR ", filtros) & ")"

            End If

            sql &= " ORDER BY Data"

            '==========================
            ' NFe
            '==========================
        ElseIf modelo = 55 Then

            sql =
    "SELECT
    'NFe' AS Modelo,
    num_nota AS Documento,
    cod_empresa AS Empresa,
    '' AS Serie,
    num_nota AS Chave,
    CASE
        WHEN COALESCE(nfe_protocan,'')<>'' THEN 'CANCELADA'
        ELSE 'EMITIDA'
    END AS Status,
    dt_emissao AS Data
FROM vendas
WHERE dt_emissao >= @inicio
AND dt_emissao < @fim
AND COALESCE(arq_xml,'')<>''"

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa=@empresa"
            End If

            If incluirEmitidos Xor incluirCancelados Then

                If incluirEmitidos Then
                    sql &= " AND COALESCE(nfe_protocan,'')=''"
                Else
                    sql &= " AND COALESCE(nfe_protocan,'')<>''"
                End If

            End If

            sql &= " ORDER BY Data"

            '==========================
            ' AMBOS
            '==========================
        Else

            sql =
    "SELECT *
FROM (

SELECT
    'NFC-e' AS Modelo,
    coo AS Documento,
    cod_empresa AS Empresa,
    serie_nfce AS Serie,
    chave_cfe AS Chave,
    CASE
        WHEN TRIM(COALESCE(inutilizada,''))='S' THEN 'INUTILIZADA'
        WHEN TRIM(COALESCE(cancelado,''))='S' THEN 'CANCELADA'
        ELSE 'EMITIDA'
    END AS Status,
    dt_impressao AS Data
FROM cupons
WHERE dt_impressao >= @inicio
AND dt_impressao < @fim"

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa=@empresa"
            End If

            If Not (incluirEmitidos And incluirCancelados And incluirInutilizados) Then

                Dim filtros As New List(Of String)

                If incluirEmitidos Then
                    filtros.Add("(TRIM(COALESCE(cancelado,''))<>'S' AND TRIM(COALESCE(inutilizada,''))<>'S')")
                End If

                If incluirCancelados Then
                    filtros.Add("TRIM(COALESCE(cancelado,''))='S'")
                End If

                If incluirInutilizados Then
                    filtros.Add("TRIM(COALESCE(inutilizada,''))='S'")
                End If

                sql &= " AND (" & String.Join(" OR ", filtros) & ")"

            End If

            sql &= "

UNION ALL

SELECT
    'NFe' AS Modelo,
    num_nota AS Documento,
    cod_empresa AS Empresa,
    NULL::INTEGER AS Serie,
    CAST(num_nota AS VARCHAR) AS Chave,
    CASE
        WHEN COALESCE(nfe_protocan,'')<>'' THEN 'CANCELADA'
        ELSE 'EMITIDA'
    END AS Status,
    dt_emissao AS Data
FROM vendas
WHERE dt_emissao >= @inicio
AND dt_emissao < @fim
AND COALESCE(arq_xml,'')<>''"

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa=@empresa"
            End If

            If incluirEmitidos Xor incluirCancelados Then

                If incluirEmitidos Then
                    sql &= " AND COALESCE(nfe_protocan,'')=''"
                Else
                    sql &= " AND COALESCE(nfe_protocan,'')<>''"
                End If

            End If

            sql &= "

) consulta
ORDER BY Data"

        End If

        Dim tabela As New DataTable()

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