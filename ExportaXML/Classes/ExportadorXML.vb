Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports Npgsql

Public Class ExportadorXML

    ''' <summary>Nomes já gravados em cada ZipArchive aberto (ZipArchive em modo Create não permite listar as entradas).</summary>
    Private NotInheritable Class NomesDoZip
        Public ReadOnly Nomes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    End Class

    Private Shared ReadOnly NomesPorZip As New System.Runtime.CompilerServices.ConditionalWeakTable(Of ZipArchive, NomesDoZip)()

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
    ''' <summary>
    ''' Exporta NFC-e. Recebe o <see cref="ZipArchive"/> de destino JÁ ABERTO
    ''' (não um caminho) — quem chama decide quando abrir/fechar, pra permitir
    ''' reaproveitar o MESMO archive entre NFC-e e NFe (modelo "Ambos") sem
    ''' nunca precisar reabrir um zip já escrito em modo Update (ver
    ''' <see cref="ExportarSaida"/>, que é quem deveria ser chamado na maioria
    ''' dos casos — abrir/fechar na mão só se precisar controlar isso de fora).
    ''' </summary>
    Public Shared Sub ExportarNFCe(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        zip As ZipArchive,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        Optional serie As String = "",
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing,
        Optional agruparPorSerie As Boolean = False)

        Dim sql As String =
    "SELECT
        chave_cfe,
        serie_nfce,
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

        Dim pastaTemporariaSerie As String = Nothing
        Dim zipsPorSerie As New Dictionary(Of String, ZipArchive)
        Dim arquivosPorSerie As New Dictionary(Of String, String)

        Try
            If agruparPorSerie Then
                pastaTemporariaSerie = Path.Combine(Path.GetTempPath(), "ExportadorXML_Serie_" & Guid.NewGuid().ToString())
                Directory.CreateDirectory(pastaTemporariaSerie)
            End If

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
                    Dim processados As Integer = 0

                    While reader.Read()
                        processados += 1
                        Dim chave = reader("chave_cfe").ToString()
                        Dim cancelado = reader("cancelado").ToString().Trim().ToUpper()
                        Dim inutilizada = reader("inutilizada").ToString().Trim().ToUpper()

                        Dim zipDestino As ZipArchive = zip
                        If agruparPorSerie Then
                            zipDestino = ObterOuCriarZipSerie(zipsPorSerie, arquivosPorSerie, pastaTemporariaSerie, reader("serie_nfce"))
                        End If

                        If inutilizada = "S" Then
                            Dim bytes As Byte() = Base64ParaBytesXml(reader("xml_inutilizado"))
                            ExportarXml(zipDestino, chave & "_inutilizacao.xml", bytes)
                        ElseIf cancelado = "S" Then
                            Dim bytes As Byte() = Base64ParaBytesXml(reader("xml_cancelado"))
                            ExportarXml(zipDestino, chave & "_cancelado.xml", bytes)
                        Else
                            Dim bytes As Byte() = Base64ParaBytesXml(reader("xml_autorizado"))
                            ExportarXml(zipDestino, chave & ".xml", bytes)
                        End If

                        atualizarProgresso?.Invoke(processados, 0)
                    End While

                    If agruparPorSerie Then
                        MesclarZipsPorSerie(zip, zipsPorSerie, arquivosPorSerie, "NFCe_Serie")
                    End If
                End Using
            End Using
        Finally
            If pastaTemporariaSerie IsNot Nothing AndAlso Directory.Exists(pastaTemporariaSerie) Then
                Try
                    Directory.Delete(pastaTemporariaSerie, True)
                Catch
                    ' Se algum arquivo estiver em uso, não interrompe a exportação.
                End Try
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Exporta NFe. Mesma ideia de <see cref="ExportarNFCe"/>: recebe o
    ''' <see cref="ZipArchive"/> já aberto — ver <see cref="ExportarSaida"/>.
    ''' </summary>
    Public Shared Sub ExportarNFe(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        zip As ZipArchive,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        Optional serie As String = "",
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing,
        Optional agruparPorSerie As Boolean = False)

        Dim sql As String =
    "SELECT
        num_nota,
        textsend(arq_xml) AS arq_xml,
        nfe_protocan,
        cod_empresa,
        COALESCE(serie, '') AS serie
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

        Dim pastaTemporariaSerie As String = Nothing
        Dim zipsPorSerie As New Dictionary(Of String, ZipArchive)
        Dim arquivosPorSerie As New Dictionary(Of String, String)

        Try
            If agruparPorSerie Then
                pastaTemporariaSerie = Path.Combine(Path.GetTempPath(), "ExportadorXML_Serie_" & Guid.NewGuid().ToString())
                Directory.CreateDirectory(pastaTemporariaSerie)
            End If

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
                    Dim processados As Integer = 0

                    While reader.Read()
                        processados += 1

                        If reader("arq_xml") Is DBNull.Value Then
                            Continue While
                        End If

                        Dim numero As String = reader("num_nota").ToString()
                        Dim cancelada As Boolean = Not String.IsNullOrWhiteSpace(reader("nfe_protocan").ToString())
                        Dim nomeArquivo As String = If(cancelada, numero & "_cancelada.xml", numero & ".xml")

                        Dim zipDestino As ZipArchive = zip
                        If agruparPorSerie Then
                            zipDestino = ObterOuCriarZipSerie(zipsPorSerie, arquivosPorSerie, pastaTemporariaSerie, reader("serie"))
                        End If

                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml"))
                        ExportarXml(zipDestino, nomeArquivo, bytes)

                        atualizarProgresso?.Invoke(processados, 0)
                    End While

                    If agruparPorSerie Then
                        MesclarZipsPorSerie(zip, zipsPorSerie, arquivosPorSerie, "NFe_Serie")
                    End If
                End Using
            End Using
        Finally
            If pastaTemporariaSerie IsNot Nothing AndAlso Directory.Exists(pastaTemporariaSerie) Then
                Try
                    Directory.Delete(pastaTemporariaSerie, True)
                Catch
                    ' Se algum arquivo estiver em uso, não interrompe a exportação.
                End Try
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Exporta Saída (NFC-e e/ou NFe, conforme <paramref name="modelo"/>) pra
    ''' UM ÚNICO arquivo <paramref name="caminhoZip"/>, abrindo-o UMA VEZ SÓ e
    ''' reaproveitando o mesmo <see cref="ZipArchive"/> pros dois modelos
    ''' quando "Ambos" (modelo=0).
    ''' </summary>
    ''' <remarks>
    ''' Antes, "Ambos" chamava <see cref="ExportarNFCe"/> (cria o zip) e depois
    ''' <see cref="ExportarNFe"/> (reabre o MESMO caminho em modo Update) —
    ''' reabrir em modo Update é uma operação bem mais frágil no
    ''' <see cref="ZipArchive"/> do .NET (ele reescreve o arquivo inteiro ao
    ''' fechar), e piorou de vez quando as entradas passaram a ser zips
    ''' aninhados (<paramref name="agruparPorSerie"/>): um teste real em
    ''' produção perdeu quase todo o conteúdo do zip final depois desse
    ''' reaproveitamento. Abrir uma única vez elimina esse reaproveitamento —
    ''' use este método em vez de chamar <see cref="ExportarNFCe"/>/
    ''' <see cref="ExportarNFe"/> diretamente sempre que for possível.
    ''' </remarks>
    Public Shared Sub ExportarSaida(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        caminhoZip As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean,
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        serie As String,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing,
        Optional agruparPorSerie As Boolean = False)

        ' Nomes em minúsculas evitados de propósito: VB.NET não diferencia
        ' maiúsculas/minúsculas, então "exportarNFCe" colidiria com o método
        ' ExportarNFCe (erro BC30454, "Expressão não é um método").
        Dim deveExportarNFCe As Boolean = (modelo = 0 Or modelo = 65)
        Dim deveExportarNFe As Boolean = (modelo = 0 Or modelo = 55)

        Using zip = AbrirZip(caminhoZip)
            If deveExportarNFCe Then
                ExportarNFCe(conn, cod_empresa, dataInicial, dataFinal, zip, incluirEmitidos, incluirCancelados, incluirInutilizados, cupomInicial, cupomFinal, serie, atualizarProgresso, agruparPorSerie)
            End If

            If deveExportarNFe Then
                ExportarNFe(conn, cod_empresa, dataInicial, dataFinal, zip, incluirEmitidos, incluirCancelados, modelo, cupomInicial, cupomFinal, serie, atualizarProgresso, agruparPorSerie)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Exporta os XMLs de "compras próprias" — notas de entrada emitidas pelo
    ''' próprio sistema (<c>compra.nf_propria = 'S'</c>). Recebe o
    ''' <see cref="ZipArchive"/> de destino JÁ ABERTO — ver
    ''' <see cref="ExportarEntrada"/>, que decide quando abrir/fechar (evita
    ''' reabrir o mesmo zip em modo Update entre os três tipos de Entrada).
    ''' </summary>
    ''' <remarks>
    ''' Mesmo padrão de <see cref="ExportarNFCe"/>: escolhe entre
    ''' <c>arq_xml</c> (normal) e <c>arq_xml_cancelado</c> conforme
    ''' <c>nfe_protocan</c> estar preenchido ou não.
    ''' </remarks>
    Public Shared Sub ExportarComprasProprias(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        zip As ZipArchive,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim sql As String =
    "SELECT
        chave_nfe,
        textsend(arq_xml) AS arq_xml,
        textsend(arq_xml_cancelado) AS arq_xml_cancelado,
        nfe_protocan
     FROM compra
     WHERE nf_propria = 'S'
       AND dt_emissao >= @inicio
       AND dt_emissao < @fim"

        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

        If cod_fornecedor <> 0 Then
            sql &= " AND cod_fornecedor = @fornecedor"
        End If

        If incluirEmitidos Xor incluirCancelados Then
            If incluirEmitidos Then
                sql &= " AND COALESCE(nfe_protocan,'') = ''"
            Else
                sql &= " AND COALESCE(nfe_protocan,'') <> ''"
            End If
        End If

        sql &= " ORDER BY dt_emissao"

        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))
            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If
            If cod_fornecedor <> 0 Then
                cmd.Parameters.AddWithValue("@fornecedor", cod_fornecedor)
            End If

            Using reader = cmd.ExecuteReader()
                Dim processados As Integer = 0

                While reader.Read()
                    processados += 1

                    Dim chave As String = reader("chave_nfe").ToString()
                    If String.IsNullOrWhiteSpace(chave) Then Continue While

                    Dim cancelada As Boolean = Not String.IsNullOrWhiteSpace(reader("nfe_protocan").ToString())

                    If cancelada Then
                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml_cancelado"))
                        ExportarXml(zip, chave & "_cancelada.xml", bytes)
                    Else
                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml"))
                        ExportarXml(zip, chave & ".xml", bytes)
                    End If

                    atualizarProgresso?.Invoke(processados, 0)
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Exporta os XMLs de "compras de fornecedores" — compras normais de
    ''' fornecedor externo (<c>compra.nf_propria &lt;&gt; 'S'</c>).
    ''' </summary>
    ''' <remarks>
    ''' SÓ FUNCIONA a partir do DS Estoque 4.8.239: antes dessa versão, o DS
    ''' Estoque não gravava o XML de compra de fornecedor em <c>arq_xml</c> —
    ''' só um caminho de arquivo local (do computador de quem importou a
    ''' nota), sem confiabilidade pra exportar. Notas lançadas ANTES da
    ''' atualização continuam sem XML salvo mesmo depois do cliente atualizar
    ''' (a coluna não é preenchida retroativamente) — aparecem na grade
    ''' destacadas como "SEM XML" (ver <see cref="BuscarCompras"/>), do mesmo
    ''' jeito que cupons sem XML gerado.
    ''' </remarks>
    Public Shared Sub ExportarComprasFornecedores(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        zip As ZipArchive,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim sql As String =
    "SELECT
        chave_nfe,
        textsend(arq_xml) AS arq_xml,
        textsend(arq_xml_cancelado) AS arq_xml_cancelado,
        nfe_protocan
     FROM compra
     WHERE COALESCE(nf_propria,'') <> 'S'
       AND COALESCE(arq_xml,'') <> ''
       AND dt_emissao >= @inicio
       AND dt_emissao < @fim"

        If cod_empresa <> 0 Then
            sql &= " AND cod_empresa = @empresa"
        End If

        If cod_fornecedor <> 0 Then
            sql &= " AND cod_fornecedor = @fornecedor"
        End If

        If incluirEmitidos Xor incluirCancelados Then
            If incluirEmitidos Then
                sql &= " AND COALESCE(nfe_protocan,'') = ''"
            Else
                sql &= " AND COALESCE(nfe_protocan,'') <> ''"
            End If
        End If

        sql &= " ORDER BY dt_emissao"

        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))
            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If
            If cod_fornecedor <> 0 Then
                cmd.Parameters.AddWithValue("@fornecedor", cod_fornecedor)
            End If

            Using reader = cmd.ExecuteReader()
                Dim processados As Integer = 0

                While reader.Read()
                    processados += 1

                    Dim chave As String = reader("chave_nfe").ToString()
                    If String.IsNullOrWhiteSpace(chave) Then Continue While

                    Dim cancelada As Boolean = Not String.IsNullOrWhiteSpace(reader("nfe_protocan").ToString())

                    If cancelada Then
                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml_cancelado"))
                        ExportarXml(zip, chave & "_cancelada.xml", bytes)
                    Else
                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml"))
                        ExportarXml(zip, chave & ".xml", bytes)
                    End If

                    atualizarProgresso?.Invoke(processados, 0)
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Exporta os XMLs de devolução de venda (<c>devolucao_venda</c>) — o
    ''' cliente devolveu um produto vendido e isso gerou uma nota fiscal, que
    ''' conta como entrada. Não tem fornecedor (tem <c>cod_cliente</c>, sem
    ''' filtro de combo próprio ainda), então <paramref name="cod_empresa"/> é
    ''' o único filtro além do período.
    ''' </summary>
    Public Shared Sub ExportarDevolucoesVenda(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        dataInicial As Date,
        dataFinal As Date,
        zip As ZipArchive,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim sql As String =
    "SELECT
        chave_nfe,
        textsend(arq_xml) AS arq_xml,
        textsend(arq_xml_cancelado) AS arq_xml_cancelado,
        nfe_protocan
     FROM devolucao_venda
     WHERE dt_emissao >= @inicio
       AND dt_emissao < @fim"

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

        sql &= " ORDER BY dt_emissao"

        Using cmd As New NpgsqlCommand(sql, conn)
            cmd.Parameters.AddWithValue("@inicio", dataInicial.Date)
            cmd.Parameters.AddWithValue("@fim", dataFinal.Date.AddDays(1))
            If cod_empresa <> 0 Then
                cmd.Parameters.AddWithValue("@empresa", cod_empresa)
            End If

            Using reader = cmd.ExecuteReader()
                Dim processados As Integer = 0

                While reader.Read()
                    processados += 1

                    Dim chave As String = reader("chave_nfe").ToString()
                    If String.IsNullOrWhiteSpace(chave) Then Continue While

                    Dim cancelada As Boolean = Not String.IsNullOrWhiteSpace(reader("nfe_protocan").ToString())

                    If cancelada Then
                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml_cancelado"))
                        ExportarXml(zip, chave & "_cancelada.xml", bytes)
                    Else
                        Dim bytes As Byte() = XmlParaBytes(reader("arq_xml"))
                        ExportarXml(zip, chave & ".xml", bytes)
                    End If

                    atualizarProgresso?.Invoke(processados, 0)
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Exporta Entrada (Compras Próprias / Devoluções de Venda / Compras de
    ''' Fornecedores, conforme <paramref name="tipoEntrada"/>) pra UM ÚNICO
    ''' arquivo, abrindo-o UMA VEZ SÓ — mesma ideia de <see cref="ExportarSaida"/>
    ''' (ver o remarks lá sobre por que reabrir em modo Update é arriscado,
    ''' ainda mais agora com três tipos em vez de dois).
    ''' </summary>
    ''' <param name="tipoEntrada">
    ''' 1 = Compras Próprias, 2 = Devoluções de Venda, 3 = Compras de
    ''' Fornecedores, 0 = Ambas (Próprias + Devoluções, sentido histórico),
    ''' 4 = Todos (as três). Qualquer outro valor cai em "Todos" por segurança.
    ''' </param>
    Public Shared Sub ExportarEntrada(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        caminhoZip As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        tipoEntrada As Integer,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim exportarProprias As Boolean
        Dim exportarDevolucoes As Boolean
        Dim exportarFornecedores As Boolean

        Select Case tipoEntrada
            Case 1
                exportarProprias = True : exportarDevolucoes = False : exportarFornecedores = False
            Case 2
                exportarProprias = False : exportarDevolucoes = True : exportarFornecedores = False
            Case 3
                exportarProprias = False : exportarDevolucoes = False : exportarFornecedores = True
            Case 0
                exportarProprias = True : exportarDevolucoes = True : exportarFornecedores = False
            Case Else ' 4 = "Todos", e qualquer valor não previsto cai aqui por segurança (exporta tudo em vez de nada)
                exportarProprias = True : exportarDevolucoes = True : exportarFornecedores = True
        End Select

        Using zip = AbrirZip(caminhoZip)
            If exportarProprias Then
                ExportarComprasProprias(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, zip, incluirEmitidos, incluirCancelados, atualizarProgresso)
            End If

            If exportarDevolucoes Then
                ExportarDevolucoesVenda(conn, cod_empresa, dataInicial, dataFinal, zip, incluirEmitidos, incluirCancelados, atualizarProgresso)
            End If

            If exportarFornecedores Then
                ExportarComprasFornecedores(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, zip, incluirEmitidos, incluirCancelados, atualizarProgresso)
            End If
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

        ' Dois documentos com o mesmo nome (ex.: NF-e de mesmo número em séries
        ' diferentes) virariam duas entradas iguais no ZIP, e ao extrair um
        ' sobrescreveria o outro. Mantém todos, acrescentando _2, _3...
        Dim usados As HashSet(Of String)
        SyncLock NomesPorZip
            usados = NomesPorZip.GetOrCreateValue(zip).Nomes
        End SyncLock
        Dim nomeFinal As String = nomeArquivo
        Dim sufixo As Integer = 1
        Do While Not usados.Add(nomeFinal)
            sufixo += 1
            nomeFinal = Path.GetFileNameWithoutExtension(nomeArquivo) & "_" & sufixo & Path.GetExtension(nomeArquivo)
        Loop

        Dim entry = zip.CreateEntry(nomeFinal)
        Using stream = entry.Open()
            stream.Write(bytesXml, 0, bytesXml.Length)
        End Using
    End Sub

    ''' <summary>
    ''' Usado pela exportação "por série" (<see cref="ExportarNFCe"/>/<see cref="ExportarNFe"/>
    ''' com <c>agruparPorSerie=True</c>): devolve o zip temporário (criando-o se
    ''' for a primeira vez) correspondente à série do documento atual, pra
    ''' escrever nele em vez de escrever direto no zip principal.
    ''' </summary>
    Private Shared Function ObterOuCriarZipSerie(
        zipsPorSerie As Dictionary(Of String, ZipArchive),
        arquivosPorSerie As Dictionary(Of String, String),
        pastaTemporaria As String,
        valorSerie As Object) As ZipArchive

        Dim serieTexto As String = If(valorSerie Is Nothing OrElse valorSerie Is DBNull.Value, "", valorSerie.ToString().Trim())
        Dim chave As String = NomeArquivoValido(If(String.IsNullOrWhiteSpace(serieTexto), "SemSerie", serieTexto))

        If Not zipsPorSerie.ContainsKey(chave) Then
            Dim caminho As String = Path.Combine(pastaTemporaria, $"{chave}.zip")
            arquivosPorSerie(chave) = caminho
            zipsPorSerie(chave) = AbrirZip(caminho)
        End If

        Return zipsPorSerie(chave)
    End Function

    ''' <summary>
    ''' Fecha os zips por série já preenchidos por <see cref="ObterOuCriarZipSerie"/>
    ''' e junta cada um, como um arquivo à parte, dentro do zip principal —
    ''' mesma ideia de <see cref="ExportarTodasEmpresas"/> (que agrupa por
    ''' empresa), só que aninhando por série. <paramref name="prefixo"/> evita
    ''' colisão de nome quando o zip principal recebe tanto NFC-e quanto NFe
    ''' (modelo "Ambos" chama <see cref="ExportarNFCe"/> e <see cref="ExportarNFe"/>
    ''' no mesmo arquivo de destino).
    ''' </summary>
    Private Shared Sub MesclarZipsPorSerie(
        zip As ZipArchive,
        zipsPorSerie As Dictionary(Of String, ZipArchive),
        arquivosPorSerie As Dictionary(Of String, String),
        prefixo As String)

        For Each par In zipsPorSerie
            par.Value.Dispose()
        Next

        For Each par In arquivosPorSerie
            zip.CreateEntryFromFile(par.Value, $"{prefixo}_{par.Key}.zip", CompressionLevel.Optimal)
        Next
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
    ''' Igual a <see cref="ContarXMLs"/>, mas para as fontes de Entrada — usado
    ''' pela barra de progresso de <see cref="ExportarEntrada"/>.
    ''' </summary>
    ''' <param name="tipoEntrada">
    ''' 1 = Compras Próprias, 2 = Devoluções de Venda, 3 = Compras de
    ''' Fornecedores, 0 = Ambas (Próprias + Devoluções), 4 = Todos (as três).
    ''' Qualquer outro valor cai em "Todos" por segurança.
    ''' </param>
    Public Shared Function ContarXMLsEntrada(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        tipoEntrada As Integer) As Integer

        Select Case tipoEntrada
            '================ Compras Próprias ==================
            Case 1
                Dim sql As String =
                    "SELECT COUNT(*)
                     FROM compra
                     WHERE nf_propria = 'S'
                     AND dt_emissao >= @inicio
                     AND dt_emissao < @fim"

                If cod_empresa <> 0 Then
                    sql &= " AND cod_empresa = @empresa"
                End If

                If cod_fornecedor <> 0 Then
                    sql &= " AND cod_fornecedor = @fornecedor"
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

                    If cod_fornecedor <> 0 Then
                        cmd.Parameters.AddWithValue("@fornecedor", cod_fornecedor)
                    End If

                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using

            '================ Devoluções de Venda ==================
            Case 2
                Dim sql As String =
                    "SELECT COUNT(*)
                     FROM devolucao_venda
                     WHERE dt_emissao >= @inicio
                     AND dt_emissao < @fim"

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

                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using

            '================ Compras de Fornecedores ==================
            ' Só conta quem já tem arq_xml preenchido (notas do DS Estoque
            ' 4.8.239+) — o que não tiver aparece na grade "SEM XML", mas não
            ' entra nessa contagem (não vai ser exportado de qualquer jeito).
            Case 3
                Dim sql As String =
                    "SELECT COUNT(*)
                     FROM compra
                     WHERE COALESCE(nf_propria,'') <> 'S'
                     AND COALESCE(arq_xml,'') <> ''
                     AND dt_emissao >= @inicio
                     AND dt_emissao < @fim"

                If cod_empresa <> 0 Then
                    sql &= " AND cod_empresa = @empresa"
                End If

                If cod_fornecedor <> 0 Then
                    sql &= " AND cod_fornecedor = @fornecedor"
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

                    If cod_fornecedor <> 0 Then
                        cmd.Parameters.AddWithValue("@fornecedor", cod_fornecedor)
                    End If

                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using

            '================ Ambas (Próprias + Devoluções) ==================
            Case 0
                Return ContarXMLsEntrada(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, 1) +
                       ContarXMLsEntrada(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, 2)

            '================ Todos (as três) ==================
            Case Else
                Return ContarXMLsEntrada(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, 1) +
                       ContarXMLsEntrada(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, 2) +
                       ContarXMLsEntrada(conn, cod_empresa, cod_fornecedor, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, 3)
        End Select
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
                        WHEN TRIM(COALESCE(inutilizada,''))='S' THEN
                            CASE WHEN COALESCE(NULLIF(xml_inutilizacao_nfce,''), xml_gerado, '')='' THEN 'INUTILIZADA (SEM XML)' ELSE 'INUTILIZADA' END
                        WHEN TRIM(COALESCE(cancelado,''))='S' THEN
                            CASE WHEN COALESCE(xml_cancelado,'')='' THEN 'CANCELADA (SEM XML)' ELSE 'CANCELADA' END
                        ELSE
                            CASE WHEN COALESCE(xml_autorizado,'')='' THEN 'EMITIDA (SEM XML)' ELSE 'EMITIDA' END
                    END AS Status,
                    CASE
                        WHEN TRIM(COALESCE(inutilizada,''))='S' THEN COALESCE(NULLIF(xml_inutilizacao_nfce,''), xml_gerado, '')=''
                        WHEN TRIM(COALESCE(cancelado,''))='S' THEN COALESCE(xml_cancelado,'')=''
                        ELSE COALESCE(xml_autorizado,'')=''
                    END AS SemXml,
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
                        WHEN COALESCE(nfe_protocan,'')<>'' AND COALESCE(arq_xml,'')='' THEN 'CANCELADA (SEM XML)'
                        WHEN COALESCE(nfe_protocan,'')<>'' THEN 'CANCELADA'
                        WHEN COALESCE(arq_xml,'')='' THEN 'EMITIDA (SEM XML)'
                        ELSE 'EMITIDA'
                    END AS Status,
                    COALESCE(arq_xml,'')='' AS SemXml,
                    dt_emissao AS Data
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
                            WHEN TRIM(COALESCE(inutilizada,''))='S' THEN
                                CASE WHEN COALESCE(NULLIF(xml_inutilizacao_nfce,''), xml_gerado, '')='' THEN 'INUTILIZADA (SEM XML)' ELSE 'INUTILIZADA' END
                            WHEN TRIM(COALESCE(cancelado,''))='S' THEN
                                CASE WHEN COALESCE(xml_cancelado,'')='' THEN 'CANCELADA (SEM XML)' ELSE 'CANCELADA' END
                            ELSE
                                CASE WHEN COALESCE(xml_autorizado,'')='' THEN 'EMITIDA (SEM XML)' ELSE 'EMITIDA' END
                        END AS Status,
                        CASE
                            WHEN TRIM(COALESCE(inutilizada,''))='S' THEN COALESCE(NULLIF(xml_inutilizacao_nfce,''), xml_gerado, '')=''
                            WHEN TRIM(COALESCE(cancelado,''))='S' THEN COALESCE(xml_cancelado,'')=''
                            ELSE COALESCE(xml_autorizado,'')=''
                        END AS SemXml,
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
                        WHEN COALESCE(nfe_protocan,'')<>'' AND COALESCE(arq_xml,'')='' THEN 'CANCELADA (SEM XML)'
                        WHEN COALESCE(nfe_protocan,'')<>'' THEN 'CANCELADA'
                        WHEN COALESCE(arq_xml,'')='' THEN 'EMITIDA (SEM XML)'
                        ELSE 'EMITIDA'
                    END AS Status,
                    COALESCE(arq_xml,'')='' AS SemXml,
                    dt_emissao AS Data
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
    ''' Consulta notas de entrada que batem com os filtros — alimenta a
    ''' grade da aba "Exportar / Pesquisar" quando a Direção é "Entrada".
    ''' </summary>
    ''' <param name="conn">Conexão já aberta.</param>
    ''' <param name="cod_empresa">Código da empresa, ou 0 para todas.</param>
    ''' <param name="cod_fornecedor">
    ''' Código do fornecedor, ou 0 para todos. Só filtra "Compras Próprias" —
    ''' devolução de venda não tem fornecedor (tem cliente), então esse filtro
    ''' é ignorado pras linhas de devolução, que aparecem sempre.
    ''' </param>
    ''' <param name="dataInicial">Primeiro dia do período (por <c>dt_emissao</c>).</param>
    ''' <param name="dataFinal">Último dia do período (inclusive).</param>
    ''' <param name="incluirEmitidos">Inclui notas não canceladas.</param>
    ''' <param name="incluirCancelados">Inclui notas canceladas (<c>nfe_protocan</c> preenchido).</param>
    ''' <param name="tipoEntrada">
    ''' 1 = Compras Próprias, 2 = Devoluções de Venda, 3 = Compras de
    ''' Fornecedores, 0 = Ambas (Próprias + Devoluções), 4 = Todos (as três).
    ''' Qualquer outro valor cai em "Todos" por segurança.
    ''' </param>
    ''' <returns>
    ''' <see cref="DataTable"/> com as mesmas colunas de <see cref="BuscarCupons"/>
    ''' (incluindo "SemXml") mais "Fornecedor" — a coluna "Modelo" identifica a
    ''' origem de cada linha ("Compra Própria", "Devolução" ou "Compra Fornecedor").
    ''' </returns>
    ''' <remarks>
    ''' Compras de fornecedor externo (<c>nf_propria &lt;&gt; 'S'</c>) só têm
    ''' XML salvo em <c>arq_xml</c> a partir do DS Estoque 4.8.239 — notas
    ''' lançadas antes disso aparecem aqui (batem com o filtro de data/status)
    ''' mas com "SemXml"=True, destacadas na grade como as demais (ver
    ''' <see cref="ExportarComprasFornecedores"/>).
    ''' </remarks>
    Public Shared Function BuscarCompras(
        conn As NpgsqlConnection,
        cod_empresa As Integer,
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        tipoEntrada As Integer) As DataTable

        Dim sqlComprasPropias As String =
            "SELECT
                'Compra Própria' AS Modelo,
                c.num_nota AS Documento,
                c.cod_empresa AS Empresa,
                COALESCE(f.nome, '') AS Fornecedor,
                COALESCE(c.serie, '') AS Serie,
                COALESCE(c.chave_nfe, '') AS Chave,
                CASE
                    WHEN COALESCE(c.nfe_protocan, '') <> '' THEN 'CANCELADA'
                    ELSE 'EMITIDA'
                END AS Status,
                FALSE AS SemXml,
                c.dt_emissao AS Data
             FROM compra c
             LEFT JOIN fornecedores f ON f.codigo = c.cod_fornecedor
             WHERE c.nf_propria = 'S'
               AND c.dt_emissao >= @inicio
               AND c.dt_emissao < @fim"

        If cod_empresa <> 0 Then
            sqlComprasPropias &= " AND c.cod_empresa = @empresa"
        End If

        If cod_fornecedor <> 0 Then
            sqlComprasPropias &= " AND c.cod_fornecedor = @fornecedor"
        End If

        If incluirEmitidos Xor incluirCancelados Then
            If incluirEmitidos Then
                sqlComprasPropias &= " AND COALESCE(c.nfe_protocan, '') = ''"
            Else
                sqlComprasPropias &= " AND COALESCE(c.nfe_protocan, '') <> ''"
            End If
        End If

        Dim sqlDevolucoes As String =
            "SELECT
                'Devolução' AS Modelo,
                d.num_nota AS Documento,
                d.cod_empresa AS Empresa,
                ''::character varying AS Fornecedor,
                ''::character varying AS Serie,
                COALESCE(d.chave_nfe, '') AS Chave,
                CASE
                    WHEN COALESCE(d.nfe_protocan, '') <> '' THEN 'CANCELADA'
                    ELSE 'EMITIDA'
                END AS Status,
                FALSE AS SemXml,
                d.dt_emissao AS Data
             FROM devolucao_venda d
             WHERE d.dt_emissao >= @inicio
               AND d.dt_emissao < @fim"

        If cod_empresa <> 0 Then
            sqlDevolucoes &= " AND d.cod_empresa = @empresa"
        End If

        If incluirEmitidos Xor incluirCancelados Then
            If incluirEmitidos Then
                sqlDevolucoes &= " AND COALESCE(d.nfe_protocan, '') = ''"
            Else
                sqlDevolucoes &= " AND COALESCE(d.nfe_protocan, '') <> ''"
            End If
        End If

        Dim sqlComprasFornecedores As String =
            "SELECT
                'Compra Fornecedor' AS Modelo,
                c.num_nota AS Documento,
                c.cod_empresa AS Empresa,
                COALESCE(f.nome, '') AS Fornecedor,
                COALESCE(c.serie, '') AS Serie,
                COALESCE(c.chave_nfe, '') AS Chave,
                CASE
                    WHEN COALESCE(c.nfe_protocan, '') <> '' AND COALESCE(c.arq_xml,'') = '' THEN 'CANCELADA (SEM XML)'
                    WHEN COALESCE(c.nfe_protocan, '') <> '' THEN 'CANCELADA'
                    WHEN COALESCE(c.arq_xml,'') = '' THEN 'EMITIDA (SEM XML)'
                    ELSE 'EMITIDA'
                END AS Status,
                COALESCE(c.arq_xml,'') = '' AS SemXml,
                c.dt_emissao AS Data
             FROM compra c
             LEFT JOIN fornecedores f ON f.codigo = c.cod_fornecedor
             WHERE COALESCE(c.nf_propria,'') <> 'S'
               AND c.dt_emissao >= @inicio
               AND c.dt_emissao < @fim"

        If cod_empresa <> 0 Then
            sqlComprasFornecedores &= " AND c.cod_empresa = @empresa"
        End If

        If cod_fornecedor <> 0 Then
            sqlComprasFornecedores &= " AND c.cod_fornecedor = @fornecedor"
        End If

        If incluirEmitidos Xor incluirCancelados Then
            If incluirEmitidos Then
                sqlComprasFornecedores &= " AND COALESCE(c.nfe_protocan, '') = ''"
            Else
                sqlComprasFornecedores &= " AND COALESCE(c.nfe_protocan, '') <> ''"
            End If
        End If

        Dim sql As String

        Select Case tipoEntrada
            Case 1
                sql = sqlComprasPropias & " ORDER BY c.dt_emissao"
            Case 2
                sql = sqlDevolucoes & " ORDER BY d.dt_emissao"
            Case 3
                sql = sqlComprasFornecedores & " ORDER BY c.dt_emissao"
            Case 0
                sql = "SELECT * FROM (" & sqlComprasPropias & " UNION ALL " & sqlDevolucoes & ") consulta ORDER BY Data"
            Case Else ' 4 = "Todos", e qualquer valor não previsto
                sql = "SELECT * FROM (" & sqlComprasPropias & " UNION ALL " & sqlDevolucoes & " UNION ALL " & sqlComprasFornecedores & ") consulta ORDER BY Data"
        End Select

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
    ''' <summary>
    ''' Gera um ZIP por empresa de Entrada (usando <see cref="ExportarEntrada"/>,
    ''' que abre cada zip de empresa uma única vez) e devolve os caminhos
    ''' gerados — SEM tocar no zip final. Mesma ideia de
    ''' <see cref="GerarZipsPorEmpresa"/> (Saída): quem chama (<see cref="ExportarTodasEmpresasEntrada"/>
    ''' ou o loop multi-banco de <see cref="FrmPrincipal"/>) decide quando
    ''' juntar tudo com <see cref="MesclarZipsDeEmpresas"/>.
    ''' </summary>
    ''' <param name="tipoEntrada">
    ''' 1 = Compras Próprias, 2 = Devoluções de Venda, 3 = Compras de
    ''' Fornecedores, 0 = Ambas (Próprias + Devoluções), 4 = Todos (as três).
    ''' </param>
    Public Shared Function GerarZipsPorEmpresaEntrada(
        conn As NpgsqlConnection,
        empresas As List(Of EmpresaItem),
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        pastaTemporaria As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        tipoEntrada As Integer,
        Optional atualizarStatus As Action(Of String) = Nothing,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing) As List(Of String)

        Dim empresasValidas = empresas.Where(Function(emp) emp.Codigo <> 0).ToList()
        Dim totalEmpresas As Integer = empresasValidas.Count
        Dim contadorEmpresa As Integer = 0
        Dim totalProcessadoGeral As Integer = 0
        Dim arquivosGerados As New List(Of String)
        Dim totalGeral As Integer = 0

        For Each empresa In empresasValidas
            totalGeral += ContarXMLsEntrada(conn, empresa.Codigo, cod_fornecedor, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, tipoEntrada)
        Next

        For Each empresa In empresasValidas
            contadorEmpresa += 1

            atualizarStatus?.Invoke($"Exportando empresa {contadorEmpresa} de {totalEmpresas}...")
            atualizarStatus?.Invoke($"Empresa: {empresa.Nome}")

            Dim nomeEmpresa As String = NomeArquivoValido(empresa.Nome)

            Dim nomeZipEmpresa As String = $"{empresa.Codigo:000}_{nomeEmpresa}.zip"
            Dim caminhoZipEmpresa As String = Path.Combine(pastaTemporaria, nomeZipEmpresa)

            If File.Exists(caminhoZipEmpresa) Then
                File.Delete(caminhoZipEmpresa)
            End If

            ExportarEntrada(conn, empresa.Codigo, cod_fornecedor, dataInicial, dataFinal, caminhoZipEmpresa, incluirEmitidos, incluirCancelados, tipoEntrada,
                Sub(processados, total)
                    totalProcessadoGeral += 1
                    atualizarProgresso?.Invoke(totalProcessadoGeral, totalGeral)
                End Sub)

            If File.Exists(caminhoZipEmpresa) Then
                arquivosGerados.Add(caminhoZipEmpresa)
            End If
        Next

        Return arquivosGerados
    End Function

    ''' <summary>
    ''' Conveniência pra quando só existe UM banco: gera os zips de empresa de
    ''' Entrada e já junta tudo no zip final, numa passada só. Usado pelo
    ''' agendamento automático, que chama isto uma vez por empresa/zip.
    ''' </summary>
    ''' <param name="tipoEntrada">1 = só Compras Próprias, 2 = só Devoluções, 3 = só Compras de Fornecedores, 0 = Próprias+Devoluções, 4 = as três.</param>
    Public Shared Sub ExportarTodasEmpresasEntrada(
        conn As NpgsqlConnection,
        empresas As List(Of EmpresaItem),
        cod_fornecedor As Integer,
        dataInicial As Date,
        dataFinal As Date,
        caminhoFinal As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        tipoEntrada As Integer,
        Optional atualizarStatus As Action(Of String) = Nothing,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing)

        Dim pastaTemporaria As String = Path.Combine(Path.GetTempPath(), "ExportadorXML_" & Guid.NewGuid().ToString())
        Directory.CreateDirectory(pastaTemporaria)

        Try
            Dim arquivosGerados = GerarZipsPorEmpresaEntrada(conn, empresas, cod_fornecedor, dataInicial, dataFinal, pastaTemporaria,
                incluirEmitidos, incluirCancelados, tipoEntrada, atualizarStatus, atualizarProgresso)

            atualizarStatus?.Invoke("Montando ZIP final...")
            MesclarZipsDeEmpresas(caminhoFinal, arquivosGerados)
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

    ''' <summary>
    ''' Gera um ZIP por empresa (usando <see cref="ExportarSaida"/>, que abre
    ''' cada zip de empresa uma única vez) e devolve os caminhos gerados —
    ''' SEM tocar no zip final. Quem chama (<see cref="ExportarTodasEmpresas"/>
    ''' ou o loop multi-banco de <see cref="FrmPrincipal"/>) decide quando
    ''' juntar tudo com <see cref="MesclarZipsDeEmpresas"/>.
    ''' </summary>
    Public Shared Function GerarZipsPorEmpresa(
        conn As NpgsqlConnection,
        empresas As List(Of EmpresaItem),
        dataInicial As Date,
        dataFinal As Date,
        pastaTemporaria As String,
        incluirEmitidos As Boolean,
        incluirCancelados As Boolean,
        incluirInutilizados As Boolean,
        modelo As Integer,
        cupomInicial As Integer?,
        cupomFinal As Integer?,
        serie As String,
        Optional atualizarStatus As Action(Of String) = Nothing,
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing,
        Optional agruparPorSerie As Boolean = False) As List(Of String)

        Dim empresasValidas = empresas.Where(Function(emp) emp.Codigo <> 0).ToList()
        Dim totalEmpresas As Integer = empresasValidas.Count
        Dim contadorEmpresa As Integer = 0
        Dim totalProcessadoGeral As Integer = 0
        Dim arquivosGerados As New List(Of String)
        Dim totalGeral As Integer = 0

        For Each empresa In empresasValidas
            totalGeral += ContarXMLs(conn, empresa.Codigo, dataInicial, dataFinal, incluirEmitidos, incluirCancelados, incluirInutilizados, modelo, cupomInicial, cupomFinal, serie)
        Next

        For Each empresa In empresasValidas
            contadorEmpresa += 1

            atualizarStatus?.Invoke($"Exportando empresa {contadorEmpresa} de {totalEmpresas}...")
            atualizarStatus?.Invoke($"Empresa: {empresa.Nome}")

            Dim nomeEmpresa As String = NomeArquivoValido(empresa.Nome)

            Dim nomeZipEmpresa As String = $"{empresa.Codigo:000}_{nomeEmpresa}.zip"
            Dim caminhoZipEmpresa As String = Path.Combine(pastaTemporaria, nomeZipEmpresa)

            If File.Exists(caminhoZipEmpresa) Then
                File.Delete(caminhoZipEmpresa)
            End If

            ExportarSaida(conn, empresa.Codigo, dataInicial, dataFinal, caminhoZipEmpresa, incluirEmitidos, incluirCancelados, incluirInutilizados, modelo, cupomInicial, cupomFinal, serie,
                Sub(processados, total)
                    totalProcessadoGeral += 1
                    atualizarProgresso?.Invoke(totalProcessadoGeral, totalGeral)
                End Sub, agruparPorSerie)

            If File.Exists(caminhoZipEmpresa) Then
                arquivosGerados.Add(caminhoZipEmpresa)
            End If
        Next

        Return arquivosGerados
    End Function

    ''' <summary>
    ''' Junta os zips de empresa (de <see cref="GerarZipsPorEmpresa"/>, de um
    ''' ou mais bancos) dentro do zip final — chame no MÁXIMO uma vez por
    ''' exportação (ver <see cref="ExportarSaida"/> sobre por que reabrir um
    ''' zip em modo Update é arriscado): se tiver vários bancos, acumule TODOS
    ''' os caminhos de todos eles numa lista só e chame este método UMA VEZ no
    ''' final, em vez de uma vez por banco.
    ''' </summary>
    Public Shared Sub MesclarZipsDeEmpresas(caminhoFinal As String, arquivosGerados As List(Of String))
        Using zipFinal As ZipArchive = AbrirZip(caminhoFinal)
            For Each caminhoZipEmpresa In arquivosGerados
                Dim nomeArquivo As String = Path.GetFileName(caminhoZipEmpresa)
                zipFinal.CreateEntryFromFile(caminhoZipEmpresa, nomeArquivo, CompressionLevel.Optimal)
            Next
        End Using
    End Sub

    ''' <summary>
    ''' Conveniência pra quando só existe UM banco (ou já se sabe que
    ''' <see cref="MesclarZipsDeEmpresas"/> só vai ser chamado esta vez):
    ''' gera os zips de empresa e já junta tudo no zip final, numa passada só.
    ''' Usado pelo agendamento automático (<see cref="AgendamentoService"/>),
    ''' que chama isto uma vez por empresa/zip, então o risco de reabrir o
    ''' mesmo caminho em modo Update não se aplica a esse uso.
    ''' </summary>
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
        Optional atualizarProgresso As Action(Of Integer, Integer) = Nothing,
        Optional agruparPorSerie As Boolean = False)

        Dim pastaTemporaria As String = Path.Combine(Path.GetTempPath(), "ExportadorXML_" & Guid.NewGuid().ToString())
        Directory.CreateDirectory(pastaTemporaria)

        Try
            Dim arquivosGerados = GerarZipsPorEmpresa(conn, empresas, dataInicial, dataFinal, pastaTemporaria,
                incluirEmitidos, incluirCancelados, incluirInutilizados, modelo, cupomInicial, cupomFinal, serie,
                atualizarStatus, atualizarProgresso, agruparPorSerie)

            atualizarStatus?.Invoke("Montando ZIP final...")
            MesclarZipsDeEmpresas(caminhoFinal, arquivosGerados)
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