Imports Npgsql
Imports System.IO
Imports System.IO.Compression

''' <summary>
''' Núcleo de toda a exportação/consulta de XMLs de saída (NFC-e e NFe) e,
''' mais recentemente, também a consulta de notas de entrada (compra). O
''' conteúdo do XML de saída já vem gravado como texto direto no banco
''' (colunas tipo <c>arq_xml</c>/<c>xml_autorizado</c>); exportar é só ler essa
''' coluna e escrever num .zip — bem diferente da entrada, cujo XML só existe
''' como caminho de arquivo local (ver <see cref="BuscarCompras"/>).
''' </summary>
Public Class ExportadorXML

    ''' <summary>
    ''' Exporta para um .zip os XMLs de NFC-e (cupons fiscais) de uma empresa
    ''' num período, aplicando os filtros informados.
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <param name="cod_empresa">Código da empresa, ou 0 para não filtrar por empresa.</param>
    ''' <param name="dataInicial">Primeiro dia do período (inclusive).</param>
    ''' <param name="dataFinal">Último dia do período (inclusive — internamente vira "&lt; dataFinal+1 dia").</param>
    ''' <param name="caminhoZip">
    ''' Caminho do .zip de destino. Se já existir, os XMLs são adicionados a ele
    ''' (modo Update); se não existir, é criado.
    ''' </param>
    ''' <param name="incluirEmitidos">Inclui cupons emitidos normalmente (nem cancelados, nem inutilizados).</param>
    ''' <param name="incluirCancelados">Inclui cupons cancelados.</param>
    ''' <param name="incluirInutilizados">Inclui faixas de numeração inutilizadas.</param>
    ''' <param name="cupomInicial">Filtro opcional: número do COO inicial (inclusive). Nothing = sem limite inferior.</param>
    ''' <param name="cupomFinal">Filtro opcional: número do COO final (inclusive). Nothing = sem limite superior.</param>
    ''' <param name="serie">Filtro opcional de série do SAT/NFC-e. Vazio = não filtra por série.</param>
    ''' <param name="atualizarProgresso">
    ''' Callback opcional chamado a cada cupom processado, com (quantidadeProcessadaAteAgora, 0)
    ''' — o segundo parâmetro é sempre 0 aqui; quem quiser o total real deve calculá-lo antes (ver <see cref="ContarXMLs"/>).
    ''' </param>
    ''' <remarks>
    ''' Os parâmetros de cupom são passados como texto (via cast <c>::integer</c>
    ''' no SQL) porque o Postgres não consegue inferir sozinho o tipo de um
    ''' parâmetro usado só num "IS NULL OR ..." sem esse cast explícito — sem
    ''' ele, a consulta falha com erro 42P08 sempre que o filtro estiver vazio.
    ''' Cada linha do resultado vira um arquivo dentro do zip, nomeado pela
    ''' chave de acesso, com sufixo <c>_cancelado</c>/<c>_inutilizacao</c> quando aplicável.
    ''' Para inutilizados especificamente, o XML pode estar em uma de duas
    ''' colunas dependendo de quando o cupom foi inutilizado: cupons antigos
    ''' gravam em <c>xml_gerado</c> (comportamento anterior do PDV), cupons mais
    ''' novos gravam em <c>xml_inutilizacao_nfce</c> (comportamento corrigido). A
    ''' consulta usa <c>COALESCE(NULLIF(xml_inutilizacao_nfce, ''), xml_gerado)</c>
    ''' pra pegar o que estiver preenchido, sem precisar saber de antemão qual
    ''' coluna vale pra cada cupom. Isso só se aplica ao caso inutilizado —
    ''' emitidos e cancelados continuam vindo de <c>xml_autorizado</c>/<c>xml_cancelado</c>, inalterados.
    ''' </remarks>
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
                xml_autorizado,
                xml_cancelado,
                COALESCE(NULLIF(xml_inutilizacao_nfce, ''), xml_gerado) AS xml_inutilizado,
                cancelado,
                inutilizada
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

            Using reader = cmd.ExecuteReader()
                Using zip = AbrirZip(caminhoZip)
                    Dim processados As Integer = 0

                    While reader.Read()
                        processados += 1
                        Dim chave = reader("chave_cfe").ToString()
                        Dim cancelado = reader("cancelado").ToString.Trim.ToUpper()
                        Dim inutilizada = reader("inutilizada").ToString.Trim.ToUpper()

                        If inutilizada = "S" Then
                            ExportarXml(zip, chave & "_inutilizacao.xml", reader("xml_inutilizado"))
                        ElseIf cancelado = "S" Then
                            ExportarXml(zip, chave & "_cancelado.xml", reader("xml_cancelado"))
                        Else
                            ExportarXml(zip, chave & ".xml", reader("xml_autorizado"))
                        End If

                        atualizarProgresso?.Invoke(processados, 0)
                    End While
                End Using
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Exporta para um .zip os XMLs de NFe (nota fiscal eletrônica de venda)
    ''' de uma empresa num período, aplicando os filtros informados.
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <param name="cod_empresa">Código da empresa, ou 0 para não filtrar por empresa.</param>
    ''' <param name="dataInicial">Primeiro dia do período (inclusive).</param>
    ''' <param name="dataFinal">Último dia do período (inclusive).</param>
    ''' <param name="caminhoZip">Caminho do .zip de destino (criado ou atualizado).</param>
    ''' <param name="incluirEmitidos">Inclui notas emitidas (não canceladas).</param>
    ''' <param name="incluirCancelados">Inclui notas canceladas.</param>
    ''' <param name="modelo">
    ''' Recebido por simetria com <see cref="BuscarCupons"/>/<see cref="ContarXMLs"/>,
    ''' mas NÃO é usado dentro deste método — NFe aqui sempre significa modelo 55.
    ''' </param>
    ''' <param name="cupomInicial">Filtro opcional: número da nota inicial (inclusive).</param>
    ''' <param name="cupomFinal">Filtro opcional: número da nota final (inclusive).</param>
    ''' <param name="serie">Filtro opcional de série da NFe. Vazio = não filtra por série.</param>
    ''' <param name="atualizarProgresso">Callback opcional chamado a cada nota processada.</param>
    ''' <remarks>
    ''' Linhas sem <c>arq_xml</c> preenchido são silenciosamente puladas (o
    ''' registro de venda existe, mas nunca teve XML gerado/importado — não é
    ''' erro). Diferente de <c>incluirEmitidos</c>/<c>incluirCancelados</c> de
    ''' NFC-e (que usa uma lista de filtros OR), aqui é um simples Xor: se as
    ''' duas flags forem iguais (ambas true ou ambas false), nenhum filtro de
    ''' status é aplicado e tudo é incluído.
    ''' </remarks>
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
                arq_xml,
                nfe_protocan,
                cod_empresa
             FROM vendas
             WHERE dt_emissao >= @inicio
               AND dt_emissao < @fim
               AND (@cupomInicial::integer IS NULL OR num_nota >= @cupomInicial::integer)
               AND (@cupomFinal::integer IS NULL OR num_nota <= @cupomFinal::integer)"

        ' SÓ FILTRA POR SÉRIE SE FOR INFORMADA
        If Not String.IsNullOrWhiteSpace(serie) Then
            sql &= " AND serie = @serie"
        End If

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
                        Dim nomeArquivo As String

                        If cancelada Then
                            nomeArquivo = numero & "_cancelada.xml"
                        Else
                            nomeArquivo = numero & ".xml"
                        End If

                        ExportarXml(zip, nomeArquivo, reader("arq_xml"))
                        atualizarProgresso?.Invoke(processados, 0)
                    End While
                End Using
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Sanitiza um texto (normalmente o nome de uma empresa) para poder ser
    ''' usado como nome de arquivo no Windows, trocando cada caractere inválido
    ''' (barra, dois-pontos, etc.) por "_". Nothing vira string vazia.
    ''' </summary>
    ''' <param name="nome">Texto de entrada; pode ser Nothing.</param>
    ''' <returns>O texto sanitizado e sem espaços nas pontas (Trim).</returns>
    Public Shared Function NomeArquivoValido(nome As String) As String
        Dim resultado As String = If(nome, "")

        For Each caractere In Path.GetInvalidFileNameChars()
            resultado = resultado.Replace(caractere, "_"c)
        Next

        Return resultado.Trim()
    End Function

    ''' <summary>
    ''' Abre um .zip para escrita, criando-o se não existir ou reabrindo em modo
    ''' de atualização (adicionar entradas) se já existir.
    ''' </summary>
    Private Shared Function AbrirZip(caminho As String) As ZipArchive
        Dim modo As ZipArchiveMode

        If File.Exists(caminho) Then
            modo = ZipArchiveMode.Update
        Else
            modo = ZipArchiveMode.Create
        End If

        Return ZipFile.Open(caminho, modo)
    End Function

    ''' <summary>
    ''' Escreve um único XML (lido do banco) como uma entrada dentro do zip já aberto.
    ''' </summary>
    ''' <param name="zip">Zip já aberto (ver <see cref="AbrirZip"/>).</param>
    ''' <param name="nomeArquivo">Nome do arquivo dentro do zip (ex.: "{chave}.xml").</param>
    ''' <param name="valor">
    ''' Valor cru vindo do <c>NpgsqlDataReader</c> (tipicamente uma coluna text).
    ''' Se for <c>DBNull.Value</c>, Nothing, ou string vazia/só espaços, a
    ''' função simplesmente não escreve nada (não é tratado como erro — muitas
    ''' linhas legitimamente não têm XML gerado ainda).
    ''' </param>
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

    ''' <summary>
    ''' Conta quantos XMLs seriam exportados com os filtros informados, sem
    ''' exportar nada — usado para preencher a barra de progresso antes de uma
    ''' exportação de verdade começar.
    ''' </summary>
    ''' <param name="modelo">
    ''' 65 = só NFC-e (tabela cupons), 55 = só NFe (tabela vendas), qualquer
    ''' outro valor (normalmente 0) = soma os dois, chamando este mesmo método
    ''' recursivamente uma vez para cada modelo.
    ''' </param>
    ''' <returns>Quantidade total de linhas que batem com os filtros.</returns>
    ''' <remarks>
    ''' Os filtros e a lógica de cada bloco (65/55) espelham exatamente
    ''' <see cref="ExportarNFCe"/>/<see cref="ExportarNFe"/> — se mudar um
    ''' filtro lá, mude aqui também, senão a contagem prévia (mostrada antes de
    ''' exportar) fica diferente da quantidade realmente exportada.
    ''' </remarks>
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