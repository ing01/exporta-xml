Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports Npgsql

Public Class ExportadorXML

    Private Shared Function Base64ParaBytesXml(valor As Object) As Byte()

        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return Array.Empty(Of Byte)()
        End If

        Dim texto As String = valor.ToString()

        If String.IsNullOrWhiteSpace(texto) Then
            Return Array.Empty(Of Byte)()
        End If

        texto = texto.Trim()

        ' Primeiro tenta tratar como Base64
        Try
            Return Convert.FromBase64String(texto)
        Catch
            ' Se não for Base64, trata como XML puro
            Return Encoding.UTF8.GetBytes(texto)
        End Try

    End Function

    Private Shared Function XmlParaBytes(valor As Object) As Byte()

        If valor Is Nothing OrElse valor Is DBNull.Value Then
            Return Array.Empty(Of Byte)()
        End If

        Return DirectCast(valor, Byte())

    End Function
    Public Shared Sub ExportarNFCe(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        caminhoZip As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        Optional serie As String = "",
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim sql As String =
    "SELECT
        chave_cfe,
        encode(textsend(xml_autorizado), 'base64') AS xml_autorizado,
        encode(textsend(xml_cancelado), 'base64') AS xml_cancelado,
        encode(
            textsend(
                COALESCE(NULLIF(xml_inutilizacao_nfce, ''), xml_gerado)
            ),
            'base64'
        ) AS xml_inutilizado,
        cancelado,
        inutilizada
     FROM cupons
     WHERE dt_impressao >= @inicio
       AND dt_impressao < @fim
       AND (@cupomInicial::integer IS NULL OR coo >= @cupomInicial::integer)
       AND (@cupomFinal::integer IS NULL OR coo <= @cupomFinal::integer)"

        If Not String.IsNullOrWhiteSpace(serie) Then
            sql &= " AND CAST(serie_nfce AS VARCHAR) = @serie"
        End If

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
            cmd.Parameters.AddWithValue("@cupomInicial", If(cupomInicial.HasValue, CType(cupomInicial.Value, Object), DBNull.Value))
            cmd.Parameters.AddWithValue("@cupomFinal", If(cupomFinal.HasValue, CType(cupomFinal.Value, Object), DBNull.Value))
            If Not String.IsNullOrWhiteSpace(serie) Then
                cmd.Parameters.AddWithValue("@serie", serie.Trim())
            End If

            Using reader = cmd.ExecuteReader()
                Using zip = AbrirZip(caminhoZip)
                    Dim processados As Integer = 0

                    While reader.Read()
                        processados += 1
                        Dim chave = reader("chave_cfe").ToString()
                        Dim cancelado = reader("cancelado").ToString().Trim().ToUpper()
                        Dim inutilizada = reader("inutilizada").ToString().Trim().ToUpper()

                        If inutilizada = "S" Then
                            Dim bytes As Byte() = Base64ParaBytesXml(reader("xml_inutilizado"))
                            ExportarXml(zip, chave & "_inutilizacao.xml", bytes)
                        ElseIf cancelado = "S" Then
                            Dim bytes As Byte() = Base64ParaBytesXml(reader("xml_cancelado"))
                            ExportarXml(zip, chave & "_cancelado.xml", bytes)
                        Else
                            Dim bytes As Byte() = Base64ParaBytesXml(reader("xml_autorizado"))
                            ExportarXml(zip, chave & ".xml", bytes)
                        End If

                        atualizarProgresso?.Invoke(processados, 0)
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
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        Optional serie As String = "",
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim sql As String =
    "SELECT
        num_nota,
        textsend(arq_xml) AS arq_xml,
        nfe_protocan,
        cod_empresa
     FROM vendas
     WHERE dt_emissao >= @inicio
       AND dt_emissao < @fim
       AND (@cupomInicial::integer IS NULL OR num_nota >= @cupomInicial::integer)
       AND (@cupomFinal::integer IS NULL OR num_nota <= @cupomFinal::integer)"

        If Not String.IsNullOrWhiteSpace(serie) Then
            sql &= " AND serie = @serie"
        End If

        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

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
            cmd.Parameters.AddWithValue("@cupomInicial", If(cupomInicial.HasValue, CType(cupomInicial.Value, Object), DBNull.Value))
            cmd.Parameters.AddWithValue("@cupomFinal", If(cupomFinal.HasValue, CType(cupomFinal.Value, Object), DBNull.Value))
            If Not String.IsNullOrWhiteSpace(serie) Then
                cmd.Parameters.AddWithValue("@serie", serie.Trim())
            End If

            Using reader = cmd.ExecuteReader()
                Using zip = AbrirZip(caminhoZip)
                    Dim processados As Integer = 0

                    While reader.Read()
                        processados += 1

                        If reader("arq_xml") Is DBNull.Value Then
                            Continue While
                        End If

                        Dim numero As String = reader("num_nota").ToString()
                        Dim cancelada As Boolean = Not String.IsNullOrWhiteSpace(reader("nfe_protocan").ToString())
                        Dim nomeArquivo As String = If(cancelada, numero & "_cancelada.xml", numero & ".xml")

                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml"))
                        ExportarXml(zip, nomeArquivo, bytes)

                        atualizarProgresso?.Invoke(processados, 0)
                    End While
                End Using
            End Using
        End Using
    End Sub

    Public Shared Function NomeArquivoValido(nome As String) As String
        Dim resultado As String = If(nome, "")
        For Each caractere In Path.GetInvalidFileNameChars()
            resultado = resultado.Replace(caractere, "_"c)
        Next
        Return resultado.Trim()
    End Function

    Private Shared Function AbrirZip(caminho As String) As ZipArchive
        Dim modo As ZipArchiveMode = If(File.Exists(caminho), ZipArchiveMode.Update, ZipArchiveMode.Create)
        Return ZipFile.Open(caminho, modo)
    End Function

    ''' <summary>
    ''' Escreve um XML (array de bytes) diretamente no ZIP, sem conversão de codificação.
    ''' </summary>
    Private Shared Sub ExportarXml(zip As ZipArchive, nomeArquivo As String, bytesXml As Byte())
        If bytesXml Is Nothing OrElse bytesXml.Length = 0 Then Return
        Dim entry = zip.CreateEntry(nomeArquivo)
        Using stream = entry.Open()
            stream.Write(bytesXml, 0, bytesXml.Length)
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
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        serie As String) As Integer

        '================ NFC-e ==================
        If modelo = 65 Then
            Dim sql As String =
                "SELECT COUNT(*)
                 FROM cupons
                 WHERE dt_impressao >= @inicio
                 AND dt_impressao < @fim
                 AND (@cupomInicial::integer IS NULL OR coo >= @cupomInicial::integer)
                 AND (@cupomFinal::integer IS NULL OR coo <= @cupomFinal::integer)"

            ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                sql &= " AND CAST(serie_nfce AS VARCHAR) = @serie"
            End If

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

                cmd.Parameters.AddWithValue(
                    "@cupomInicial",
                    If(cupomInicial.HasValue, CType(cupomInicial.Value, Object), DBNull.Value))

                cmd.Parameters.AddWithValue(
                    "@cupomFinal",
                    If(cupomFinal.HasValue, CType(cupomFinal.Value, Object), DBNull.Value))

                ' SÓ ADICIONA O PARÂMETRO SÉRIE SE FOR INFORMADA
                If Not String.IsNullOrWhiteSpace(serie) Then
                    cmd.Parameters.AddWithValue("@serie", serie.Trim())
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
                 AND COALESCE(arq_xml,'') <> ''
                 AND (@cupomInicial::integer IS NULL OR num_nota >= @cupomInicial::integer)
                 AND (@cupomFinal::integer IS NULL OR num_nota <= @cupomFinal::integer)"

            ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                sql &= " AND serie = @serie"
            End If

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
            End If

            If incluirEmitidos Xor incluirCancelados Then
                If incluirEmitidos Then
                    sql &= " AND COALESCE(nfe_protocan,'') = ''"
                Else
                    sql &= " AND COALESCE(nfe_protocan,'') <> ''"
                End If
            End If

            Using cmd As New NpgsqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
                cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

                If cod_empresa <> 0 Then
                    cmd.Parameters.AddWithValue("@empresa", cod_empresa)
                End If

                cmd.Parameters.AddWithValue(
                    "@cupomInicial",
                    If(cupomInicial.HasValue, CType(cupomInicial.Value, Object), DBNull.Value))

                cmd.Parameters.AddWithValue(
                    "@cupomFinal",
                    If(cupomFinal.HasValue, CType(cupomFinal.Value, Object), DBNull.Value))

                ' SÓ ADICIONA O PARÂMETRO SÉRIE SE FOR INFORMADA
                If Not String.IsNullOrWhiteSpace(serie) Then
                    cmd.Parameters.AddWithValue("@serie", serie.Trim())
                End If

                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using

            '================ Ambos ==================
        Else
            Dim total As Integer = 0

            total += ContarXMLs(conn, cod_empresa, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, incluirInutilizados, 65, cupomInicial, cupomFinal, serie)
            total += ContarXMLs(conn, cod_empresa, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, incluirInutilizados, 55, cupomInicial, cupomFinal, serie)

            Return total
        End If
    End Function

    ''' <summary>
    ''' Consulta (sem exportar nada) os documentos de saída (NFC-e e/ou NFe)
    ''' que batem com os filtros — alimenta a grade da aba "Exportar / Pesquisar"
    ''' quando a Direção é "Saída". O nome do método é histórico; hoje cobre os
    ''' dois modelos, não só cupom fiscal.
    ''' </summary>
    ''' <param name="modelo">65 = só NFC-e, 55 = só NFe, outro valor = os dois juntos (UNION ALL).</param>
    ''' <returns>
    ''' Uma <see cref="DataTable"/> com as colunas Modelo, Documento, Empresa,
    ''' Serie, Chave, Status, Data — nomes que batem com o
    ''' <c>DataPropertyName</c> das colunas do DataGridView da tela principal.
    ''' </returns>
    ''' <remarks>
    ''' No modo "Ambos", as duas metades do UNION ALL precisam ter as mesmas
    ''' colunas com os MESMOS TIPOS — por isso <c>serie_nfce</c> (integer) é
    ''' convertido com <c>CAST(... AS VARCHAR)</c> pra bater com a série de NFe
    ''' (que já é texto). Já existiu um bug de produção aqui (erro do Postgres
    ''' "invalid input syntax for type integer") por causa desse mismatch —
    ''' não remova esse CAST.
    ''' </remarks>
    Public Shared Function BuscarCupons(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean,
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        serie As String) As DataTable

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
                AND dt_impressao < @fim
                AND (@cupomInicial::integer IS NULL OR coo >= @cupomInicial::integer)
                AND (@cupomFinal::integer IS NULL OR coo <= @cupomFinal::integer)"

            ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                sql &= " AND CAST(serie_nfce AS VARCHAR) = @serie"
            End If

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
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
                    CAST(num_nota AS VARCHAR) AS Chave,
                    CASE
                        WHEN COALESCE(nfe_protocan,'')<>'' THEN 'CANCELADA'
                        ELSE 'EMITIDA'
                    END AS Status,
                    dt_emissao AS Data
                FROM vendas
                WHERE dt_emissao >= @inicio
                AND dt_emissao < @fim
                AND COALESCE(arq_xml,'')<>''
                AND (@cupomInicial::integer IS NULL OR num_nota >= @cupomInicial::integer)
                AND (@cupomFinal::integer IS NULL OR num_nota <= @cupomFinal::integer)"

            ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                sql &= " AND serie = @serie"
            End If

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
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
                        CAST(serie_nfce AS VARCHAR) AS Serie,
                        chave_cfe AS Chave,
                        CASE
                            WHEN TRIM(COALESCE(inutilizada,''))='S' THEN 'INUTILIZADA'
                            WHEN TRIM(COALESCE(cancelado,''))='S' THEN 'CANCELADA'
                            ELSE 'EMITIDA'
                        END AS Status,
                        dt_impressao AS Data
                    FROM cupons
                    WHERE dt_impressao >= @inicio
                    AND dt_impressao < @fim
                    AND (@cupomInicial::integer IS NULL OR coo >= @cupomInicial::integer)
                    AND (@cupomFinal::integer IS NULL OR coo <= @cupomFinal::integer)"

            ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                sql &= " AND CAST(serie_nfce AS VARCHAR) = @serie"
            End If

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
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

            sql &=
                "
                UNION ALL

                SELECT
                    'NFe' AS Modelo,
                    num_nota AS Documento,
                    cod_empresa AS Empresa,
                    '' AS Serie,
                    CAST(num_nota AS VARCHAR) AS Chave,
                    CASE
                        WHEN COALESCE(nfe_protocan,'')<>'' THEN 'CANCELADA'
                        ELSE 'EMITIDA'
                    END AS Status,
                    dt_emissao AS Data
                FROM vendas
                WHERE dt_emissao >= @inicio
                AND dt_emissao < @fim
                AND COALESCE(arq_xml,'')<>''
                AND (@cupomInicial::integer IS NULL OR num_nota >= @cupomInicial::integer)
                AND (@cupomFinal::integer IS NULL OR num_nota <= @cupomFinal::integer)"

            ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                sql &= " AND serie = @serie"
            End If

            If cod_empresa <> 0 Then
                sql &= " AND cod_empresa = @empresa"
            End If

            If incluirEmitidos Xor incluirCancelados Then
                If incluirEmitidos Then
                    sql &= " AND COALESCE(nfe_protocan,'')=''"
                Else
                    sql &= " AND COALESCE(nfe_protocan,'')<>''"
                End If
            End If

            sql &=
                "
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

            If cupomInicial.HasValue Then
                cmd.Parameters.AddWithValue("@cupomInicial", cupomInicial.Value)
            Else
                cmd.Parameters.AddWithValue("@cupomInicial", DBNull.Value)
            End If

            If cupomFinal.HasValue Then
                cmd.Parameters.AddWithValue("@cupomFinal", cupomFinal.Value)
            Else
                cmd.Parameters.AddWithValue("@cupomFinal", DBNull.Value)
            End If

            ' SÓ ADICIONA O PARÂMETRO SÉRIE SE FOR INFORMADA
            If Not String.IsNullOrWhiteSpace(serie) Then
                cmd.Parameters.AddWithValue("@serie", serie.Trim())
            End If

            Using da As New NpgsqlDataAdapter(cmd)
                da.Fill(tabela)
            End Using
        End Using

        Return tabela
    End Function

    ''' <summary>
    ''' Consulta notas de entrada (compra) que batem com os filtros — alimenta a
    ''' grade da aba "Exportar / Pesquisar" quando a Direção é "Entrada".
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <param name="cod_empresa">Código da empresa (quem comprou), ou 0 para todas.</param>
    ''' <param name="cod_fornecedor">Código do fornecedor (quem vendeu), ou 0 para todos.</param>
    ''' <param name="dataInicial">Primeiro dia do período (por <c>dt_emissao</c>).</param>
    ''' <param name="dataFinal">Último dia do período (inclusive).</param>
    ''' <param name="incluirEmitidos">Inclui compras não canceladas.</param>
    ''' <param name="incluirCancelados">Inclui compras canceladas (<c>nfe_protocan</c> preenchido).</param>
    ''' <returns>
    ''' <see cref="DataTable"/> com as mesmas colunas de <see cref="BuscarCupons"/>
    ''' mais uma coluna extra "Fornecedor" (via JOIN com a tabela fornecedores).
    ''' </returns>
    ''' <remarks>
    ''' SOMENTE LISTAGEM/RELATÓRIO — não existe um "ExportarCompras": o XML de
    ''' entrada não fica salvo no banco, só um caminho de arquivo local (do
    ''' computador de quem importou a nota originalmente), então não dá pra
    ''' zipar isso de forma confiável a partir daqui. Notas com
    ''' <c>nf_propria = 'S'</c> (a própria empresa emitiu a entrada, sem XML de
    ''' terceiro) são excluídas de propósito, sempre — não é opcional.
    ''' </remarks>
    Public Shared Function BuscarCompras(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean) As DataTable

        Dim sql As String =
            "SELECT
                'Compra' AS Modelo,
                c.num_nota AS Documento,
                c.cod_empresa AS Empresa,
                COALESCE(f.nome, '') AS Fornecedor,
                COALESCE(c.serie, '') AS Serie,
                COALESCE(c.chave_nfe, '') AS Chave,
                CASE
                    WHEN COALESCE(c.nfe_protocan, '') <> '' THEN 'CANCELADA'
                    ELSE 'EMITIDA'
                END AS Status,
                c.dt_emissao AS Data
             FROM compra c
             LEFT JOIN fornecedores f ON f.codigo = c.cod_fornecedor
             WHERE c.dt_emissao >= @inicio
               AND c.dt_emissao < @fim
               AND COALESCE(c.nf_propria, 'N') <> 'S'"

        If cod_empresa <> 0 Then
            sql &= " AND c.cod_empresa = @empresa"
        End If

        If cod_fornecedor <> 0 Then
            sql &= " AND c.cod_fornecedor = @fornecedor"
        End If

        If incluirEmitidos Xor incluirCancelados Then
            If incluirEmitidos Then
                sql &= " AND COALESCE(c.nfe_protocan, '') = ''"
            Else
                sql &= " AND COALESCE(c.nfe_protocan, '') <> ''"
            End If
        End If

        sql &= " ORDER BY c.dt_emissao"

        Dim tabela As New DataTable()

        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))

            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If

            If cod_fornecedor <> 0 Then
                cmd.Parameters.AddWithValue("@fornecedor", cod_fornecedor)
            End If

            Using da As New NpgsqlDataAdapter(cmd)
                da.Fill(tabela)
            End Using
        End Using

        Return tabela
    End Function

    ''' <summary>
    ''' Exporta os XMLs de saída de várias empresas de uma vez, empacotando
    ''' tudo num único .zip final (um .zip por empresa dentro do .zip geral).
    ''' Usado tanto pela opção "Todas as empresas" da tela quanto pelo
    ''' Agendamento automático mensal.
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <param name="empresas">
    ''' Lista de empresas a exportar. Empresas com Codigo=0 (o item sentinela
    ''' "Todas as empresas") são ignoradas — filtradas internamente.
    ''' </param>
    ''' <param name="caminhoFinal">
    ''' Caminho do .zip consolidado final. Se já existir, as empresas desta
    ''' chamada são ADICIONADAS a ele (ver <see cref="AbrirZip"/>) — quem quiser
    ''' recomeçar do zero apaga o arquivo antes da primeira chamada (é assim que
    ''' <see cref="FrmPrincipal"/> chama uma vez por banco configurado).
    ''' </param>
    ''' <param name="atualizarStatus">
    ''' Callback opcional com mensagens tipo "Exportando empresa 2 de 5...".
    ''' </param>
    ''' <param name="atualizarProgresso">
    ''' Callback opcional com (totalProcessadoAteAgora, totalGeralEsperado),
    ''' já somando o progresso de todas as empresas juntas.
    ''' </param>
    ''' <remarks>
    ''' Passo a passo: 1) conta o total esperado (pra barra de progresso);
    ''' 2) pra cada empresa, exporta NFC-e e/ou NFe (conforme <paramref name="modelo"/>)
    ''' num .zip temporário próprio, nomeado "{codigo}_{empresa}.zip"; 3) no
    ''' final, junta todos esses .zips de empresa dentro do .zip final, como
    ''' entradas (não descompacta e recompacta os XMLs individualmente — é uma
    ''' cópia de arquivo pra arquivo, mais rápido); 4) sempre apaga a pasta
    ''' temporária no <c>Finally</c>, mesmo se algo falhar no meio do caminho
    ''' (menos se um arquivo estiver em uso — nesse caso só ignora, pra não
    ''' interromper uma exportação que já deu certo).
    ''' </remarks>
    Public Shared Sub ExportarTodasEmpresas(
        conn As NpgsqlConnection,
        empresas As List(Of EmpresaItem),
        dataInicial As Date,
        dataFinal As Date,
        caminhoFinal As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean,
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        serie As String,
        Optional atualizarStatus As Action(Of String) = Nothing,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim pastaTemporaria As String = Path.Combine(Path.GetTempPath(), "ExportadorXML_" & Guid.NewGuid().ToString())
        Directory.CreateDirectory(pastaTemporaria)

        Try
            Dim empresasValidas = empresas.Where(Function(emp) emp.Codigo <> 0).ToList()
            Dim totalEmpresas As Integer = empresasValidas.Count
            Dim contadorEmpresa As Integer = 0
            Dim totalProcessadoGeral As Integer = 0
            Dim arquivosGerados As New List(Of String)
            Dim totalGeral As Integer = 0

            For Each empresa In empresasValidas
                totalGeral += ContarXMLs(conn, empresa.Codigo, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, incluirInutilizados, modelo, cupomInicial, cupomFinal, serie)

                contadorEmpresa += 1

                atualizarStatus?.Invoke($"Exportando empresa {contadorEmpresa} de {totalEmpresas}...")
                atualizarStatus?.Invoke($"Empresa: {empresa.Nome}")

                Dim nomeEmpresa As String = NomeArquivoValido(empresa.Nome)

                Dim nomeZipEmpresa As String = $"{empresa.Codigo:000}_{nomeEmpresa}.zip"
                Dim caminhoZipEmpresa As String = Path.Combine(pastaTemporaria, nomeZipEmpresa)

                If File.Exists(caminhoZipEmpresa) Then
                    File.Delete(caminhoZipEmpresa)
                End If

                Select Case modelo
                    Case 65
                        ExportarNFCe(conn, empresa.Codigo, dataInicial, dataFinal, caminhoZipEmpresa, incluirEmitidos, incluirCancelados, incluirInutilizados, cupomInicial, cupomFinal, serie,
                            Sub(processados, total)
                                totalProcessadoGeral += 1
                                atualizarProgresso?.Invoke(totalProcessadoGeral, totalGeral)
                            End Sub)

                    Case 55
                        ExportarNFe(conn, empresa.Codigo, dataInicial, dataFinal, caminhoZipEmpresa, incluirEmitidos, incluirCancelados, modelo, cupomInicial, cupomFinal, serie,
                            Sub(processados, total)
                                totalProcessadoGeral += 1
                                atualizarProgresso?.Invoke(totalProcessadoGeral, totalGeral)
                            End Sub)

                    Case Else
                        ExportarNFCe(conn, empresa.Codigo, dataInicial, dataFinal, caminhoZipEmpresa, incluirEmitidos, incluirCancelados, incluirInutilizados, cupomInicial, cupomFinal, serie,
                            Sub(processados, total)
                                totalProcessadoGeral += 1
                                atualizarProgresso?.Invoke(totalProcessadoGeral, totalGeral)
                            End Sub)

                        ExportarNFe(conn, empresa.Codigo, dataInicial, dataFinal, caminhoZipEmpresa, incluirEmitidos, incluirCancelados, modelo, cupomInicial, cupomFinal, serie,
                            Sub(processados, total)
                                totalProcessadoGeral += 1
                                atualizarProgresso?.Invoke(totalProcessadoGeral, totalGeral)
                            End Sub)
                End Select

                If File.Exists(caminhoZipEmpresa) Then
                    arquivosGerados.Add(caminhoZipEmpresa)
                End If
            Next

            atualizarStatus?.Invoke("Montando ZIP final...")

            ' AbrirZip reabre em modo Update se caminhoFinal já existir — de propósito,
            ' pra permitir chamar este método uma vez por banco (empresas espalhadas em
            ' bancos diferentes) e acumular tudo no mesmo ZIP final. Quem quiser um ZIP
            ' do zero deve apagar caminhoFinal ANTES da primeira chamada.
            Using zipFinal As ZipArchive = AbrirZip(caminhoFinal)
                For Each caminhoZipEmpresa In arquivosGerados
                    Dim nomeArquivo As String = Path.GetFileName(caminhoZipEmpresa)
                    zipFinal.CreateEntryFromFile(caminhoZipEmpresa, nomeArquivo, CompressionLevel.Optimal)
                Next
            End Using

            atualizarStatus?.Invoke("ZIP finalizado.")

        Finally
            If Directory.Exists(pastaTemporaria) Then
                Try
                    Directory.Delete(pastaTemporaria, True)
                Catch
                    ' Se algum arquivo estiver em uso, não interrompe a exportação.
                End Try
            End If
        End Try
    End Sub

End Class