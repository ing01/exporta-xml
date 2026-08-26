Imports Npgsql
Imports Microsoft.Win32

''' <summary>
''' Tela principal (e única janela "de verdade") do aplicativo — reúne
''' pesquisa/exportação de XMLs de saída e entrada, configuração de
''' conexão/e-mail/agendamento, e o ícone de bandeja que mantém o app rodando
''' em segundo plano mesmo com a janela fechada.
''' </summary>
Public Class FrmPrincipal

    ''' <summary>
    ''' True enquanto os campos da tela ainda estão sendo preenchidos a partir
    ''' de config.json no <see cref="Form1_Load"/>. Os manipuladores de
    ''' CheckedChanged/ValueChanged/Leave que gravam em config.json checam essa
    ''' flag primeiro — sem ela, cada linha que seta um valor inicial (ex.:
    ''' <c>rbAmbos.Checked = True</c>) dispararia uma gravação em disco
    ''' desnecessária durante a abertura da tela.
    ''' </summary>
    Private carregando As Boolean = True

    ''' <summary>
    ''' Inicializa a tela: datas padrão (mês atual) e carrega config.json nos
    ''' campos correspondentes. De propósito, NADA aqui faz I/O de rede/processo
    ''' (conectar no banco, checar o Agendador de Tarefas) — isso ficou pra
    ''' <see cref="FrmPrincipal_Shown"/>, que só dispara DEPOIS que a janela já
    ''' apareceu na tela. Antes dessa separação, um banco lento/inacessível (ou
    ''' vários bancos configurados) travava a JANELA de sequer aparecer até o
    ''' timeout de conexão — ver <see cref="CarregarEmpresasEFornecedoresAsync"/>.
    ''' </summary>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dtInicio.Value = New Date(Today.Year, Today.Month, 1)

        dtFim.Value = New Date(
            Today.Year,
            Today.Month,
            Date.DaysInMonth(Today.Year, Today.Month))
        dgvCupons.AutoGenerateColumns = False

        Dim cfg = ConfiguracaoService.Carregar()

        txtDestinatario.Text = cfg.UltimoDestinatario

        chkAgendamentoAtivo.Checked = cfg.AgendamentoAtivo
        dtpHoraAgendamento.Value = Date.Today.Add(New TimeSpan(cfg.HoraAgendamento, cfg.MinutoAgendamento, 0))
        nudDiaAgendamento.Value = cfg.DiaAgendamento
        chkDiaFixo.Checked = (cfg.DiaAgendamento = 1)
        nudDiaAgendamento.Enabled = Not chkDiaFixo.Checked
        txtEmailAlertaFalha.Text = cfg.EmailAlertaFalha
        chkIniciarComWindows.Checked = IniciarComWindowsEstaAtivo()

        Select Case cfg.UltimoModelo
            Case 55
                rbNFe.Checked = True
            Case 65
                rbNFCe.Checked = True
            Case Else
                rbAmbos.Checked = True
        End Select

        rbSaida.Checked = True
        AtualizarModoDirecao()

        lblQuantidade.Visible = False
        lblStatus.Visible = False
        lblQtd.Visible = False

        AtualizarConfiguracoes()
        chkTodos.Checked = True

        lblVersao.Text = $"Versão {My.Application.Info.Version.ToString(3)}"
    End Sub

    ''' <summary>
    ''' Dispara só depois que a janela já apareceu na tela: tenta conectar e
    ''' popular os combos de Empresa/Fornecedor, checa o estado do Vigia,
    ''' dispara a primeira verificação de Agendamento/Atualização automática.
    ''' Tudo isso pode envolver rede (banco) ou um processo externo (schtasks) —
    ''' rodar depois do Shown garante que o usuário vê a janela imediatamente,
    ''' mesmo que um banco esteja lento ou fora do ar.
    ''' </summary>
    ''' <remarks>
    ''' Se usuário/senha do banco estiverem vazios em config.json, a conexão
    ''' NEM É TENTADA (evita um erro certo logo na abertura em uma instalação
    ''' ainda não configurada) — só desabilita o combo de Empresa e mostra uma
    ''' mensagem. Qualquer outra falha de conexão é capturada e também vira só
    ''' uma mensagem de status, nunca uma exceção não tratada.
    ''' </remarks>
    Private Async Sub FrmPrincipal_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        chkManterSempreAtivo.Checked = VigiaService.EstaAtivo()

        Dim cfg = ConfiguracaoService.Carregar()
        Await CarregarEmpresasEFornecedoresAsync(cfg)
        carregando = False

        If Not String.IsNullOrWhiteSpace(cboEmpresa.Text) Then
            txtDestino.Text = ObterCaminhoDestinoPadrao()
        End If

        RegistrarAcessoTelaPrincipal()

        tmrAgendamento.Enabled = True
        VerificarEExecutarAgendamento()

        tmrAtualizacao.Enabled = True
        VerificarAtualizacaoDisponivelAsync()
    End Sub

    ''' <summary>
    ''' Registra na Duesoft (<see cref="TelemetriaService"/>) que a tela
    ''' principal foi acessada — uma vez por CNPJ configurado (todas as
    ''' empresas de todos os bancos, já carregadas em <c>cboEmpresa</c> por
    ''' <see cref="CarregarEmpresasEFornecedoresAsync"/>). Chamado uma única
    ''' vez por sessão, a partir de <see cref="FrmPrincipal_Shown"/> — esse
    ''' evento só dispara na primeira exibição real da janela, não repete ao
    ''' restaurar da bandeja, então não duplica o registro.
    ''' </summary>
    Private Sub RegistrarAcessoTelaPrincipal()
        Dim empresas = TryCast(cboEmpresa.DataSource, List(Of EmpresaItem))
        If empresas Is Nothing Then Return

        Dim cnpjs = empresas.
            Where(Function(emp) emp.Codigo <> 0).
            Select(Function(emp) emp.CNPJ)

        TelemetriaService.RegistrarAcessoTela("EXPORTACAOXML", cnpjs)
    End Sub

    ''' <summary>
    ''' Botão "Pesquisar": lê os filtros da tela e preenche a grade — via
    ''' <see cref="ExportadorXML.BuscarCompras"/> se a Direção for "Entrada", ou
    ''' <see cref="ExportadorXML.BuscarCupons"/> se for "Saída". Não exporta
    ''' nada, só consulta e mostra na tela.
    ''' </summary>
    Private Sub btnPesquisar_Click(sender As Object, e As EventArgs) Handles btnPesquisar.Click
        Try
            Dim cupomInicial As Integer?
            Dim cupomFinal As Integer?
            Dim tmpInt As Integer

            If Integer.TryParse(txbInicio.Text, tmpInt) Then
                cupomInicial = tmpInt
            End If

            If Integer.TryParse(txbFim.Text, tmpInt) Then
                cupomFinal = tmpInt
            End If

            Dim empresaSelecionada = DirectCast(cboEmpresa.SelectedItem, EmpresaItem)

            ' Obter filtros de status
            Dim incluirEmitidos = chkEmitidas.Checked
            Dim incluirCancelados = chkCancelados.Checked
            Dim incluirInutilizados = chkInutilizados.Checked

            ' Se chkTodos marcado OU nenhum filtro específico marcado, incluir todos
            If chkTodos.Checked Or Not (incluirEmitidos Or incluirCancelados Or incluirInutilizados) Then
                incluirEmitidos = True
                incluirCancelados = True
                incluirInutilizados = True
            End If

            Dim cfg = ConfiguracaoService.Carregar()

            ' Empresa específica: consulta só o banco dela. "Todas as empresas"
            ' (Codigo=0): consulta cada banco configurado e junta o resultado.
            Dim conexoesAConsultar =
                If(empresaSelecionada.Codigo <> 0,
                   New List(Of ConexaoBanco) From {empresaSelecionada.Conexao},
                   cfg.Conexoes)

            Dim resultado As DataTable = Nothing

            For Each banco As ConexaoBanco In conexoesAConsultar
                Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                    Dim parcial As DataTable

                    If rbEntrada.Checked Then
                        Dim cod_fornecedor As Integer = If(cboFornecedor.SelectedValue IsNot Nothing, CInt(cboFornecedor.SelectedValue), 0)

                        parcial = ExportadorXML.BuscarCompras(
                            conn,
                            empresaSelecionada.Codigo,
                            cod_fornecedor,
                            dtInicio.Value,
                            dtFim.Value,
                            incluirEmitidos,
                            incluirCancelados)
                    Else
                        Dim modelo As Integer

                        If rbNFCe.Checked Then
                            modelo = 65
                        ElseIf rbNFe.Checked Then
                            modelo = 55
                        Else
                            modelo = 0
                        End If

                        Dim serie As String = txbSerie.Text.Trim()

                        parcial = ExportadorXML.BuscarCupons(
                            conn,
                            empresaSelecionada.Codigo,
                            dtInicio.Value,
                            dtFim.Value,
                            incluirEmitidos,
                            incluirCancelados,
                            incluirInutilizados,
                            modelo,
                            cupomInicial,
                            cupomFinal,
                            serie)
                    End If

                    If resultado Is Nothing Then
                        resultado = parcial
                    Else
                        resultado.Merge(parcial)
                    End If
                End Using
            Next

            dgvCupons.DataSource = resultado
            lblQtd.Text = $"XMLs encontrados: {resultado.Rows.Count}"
            lblQtd.Visible = True

            LogService.RegistrarAtividade(
                $"Pesquisar: Empresa=""{empresaSelecionada.Nome}"", Direção={If(rbEntrada.Checked, "Entrada", "Saída")}, " &
                $"Período={dtInicio.Value:dd/MM/yyyy}-{dtFim.Value:dd/MM/yyyy} -> {resultado.Rows.Count} XML(s) encontrado(s)")

        Catch ex As Exception
            LogService.RegistrarAtividade($"Pesquisar -> ERRO: {ex.Message}")
            MessageBox.Show($"Erro ao pesquisar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Botão "Exportar": conta quantos XMLs batem com os filtros (pra
    ''' preencher a barra de progresso), exporta — uma empresa só ou todas de
    ''' uma vez, conforme o combo de Empresa — e, ao final, pergunta se quer
    ''' enviar o ZIP resultante por e-mail. Desabilitado quando a Direção é
    ''' "Entrada" (ver <see cref="AtualizarModoDirecao"/>).
    ''' </summary>
    ''' <remarks>
    ''' A barra de progresso é atualizada via <c>Application.DoEvents()</c>
    ''' chamado manualmente após cada etapa — o projeto não usa Task/Await
    ''' nesta parte (só na Atualização automática), então sem o DoEvents a
    ''' tela ficaria congelada até o fim de toda a exportação.
    ''' </remarks>
    Private Sub btnExportar_Click(sender As Object, e As EventArgs) Handles btnExportar.Click
        Try
            Dim cupomInicial As Integer?
            Dim cupomFinal As Integer?
            Dim tmpInt As Integer

            If Integer.TryParse(txbInicio.Text, tmpInt) Then
                cupomInicial = tmpInt
            End If

            If Integer.TryParse(txbFim.Text, tmpInt) Then
                cupomFinal = tmpInt
            End If

            ' Nome do arquivo é sempre o nome da empresa selecionada; se nenhuma pasta
            ' tiver sido escolhida, usa a Área de Trabalho por padrão.
            txtDestino.Text = ObterCaminhoDestinoPadrao()

            Dim empresaSelecionada = DirectCast(cboEmpresa.SelectedItem, EmpresaItem)

            lblStatus.Text = "Contando XMLs..."
            lblStatus.Visible = True
            Application.DoEvents()

            Dim cfg = ConfiguracaoService.Carregar()

            ' Obter filtros de status
            Dim incluirEmitidos = chkEmitidas.Checked
            Dim incluirCancelados = chkCancelados.Checked
            Dim incluirInutilizados = chkInutilizados.Checked

            If chkTodos.Checked Or Not (incluirEmitidos Or incluirCancelados Or incluirInutilizados) Then
                incluirEmitidos = True
                incluirCancelados = True
                incluirInutilizados = True
            End If

            Dim modelo As Integer

            If rbNFCe.Checked Then
                modelo = 65
            ElseIf rbNFe.Checked Then
                modelo = 55
            Else
                modelo = 0 'Ambos
            End If

            Dim serie As String = txbSerie.Text.Trim()

            If IO.File.Exists(txtDestino.Text) Then
                IO.File.Delete(txtDestino.Text)
            End If

            If empresaSelecionada.Codigo = 0 Then
                ' ==========================================
                ' TODAS AS EMPRESAS (de todos os bancos configurados)
                ' ==========================================
                Dim conexoesAbertas As New List(Of NpgsqlConnection)

                Try
                    Dim totalGeral As Integer = 0
                    Dim empresasPorConexao As New List(Of (Conn As NpgsqlConnection, Empresas As List(Of EmpresaItem)))

                    For Each banco As ConexaoBanco In cfg.Conexoes
                        Dim conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                        conexoesAbertas.Add(conn)

                        totalGeral += ExportadorXML.ContarXMLs(
                            conn, 0, dtInicio.Value, dtFim.Value,
                            incluirEmitidos, incluirCancelados, incluirInutilizados,
                            modelo, cupomInicial, cupomFinal, serie)

                        Dim empresasDaConexao = EmpresaService.Listar(conn).Where(Function(emp) emp.Codigo <> 0).ToList()
                        empresasPorConexao.Add((conn, empresasDaConexao))
                    Next

                    pbExportacao.Minimum = 0
                    pbExportacao.Maximum = totalGeral
                    pbExportacao.Value = 0
                    lblQuantidade.Text = $"0 / {totalGeral}"
                    lblQuantidade.Visible = True
                    lblStatus.Text = "Exportando..."
                    Application.DoEvents()

                    Dim totalProcessadoGeral As Integer = 0

                    For Each item In empresasPorConexao
                        ExportadorXML.ExportarTodasEmpresas(
                            item.Conn,
                            item.Empresas,
                            dtInicio.Value,
                            dtFim.Value,
                            txtDestino.Text,
                            incluirEmitidos,
                            incluirCancelados,
                            incluirInutilizados,
                            modelo,
                            cupomInicial,
                            cupomFinal,
                            serie,
                            Sub(status As String)
                                lblStatus.Text = status
                                Application.DoEvents()
                            End Sub,
                            Sub(processados As Integer, totalItem As Integer)
                                totalProcessadoGeral += 1
                                pbExportacao.Value = Math.Min(totalProcessadoGeral, pbExportacao.Maximum)
                                lblQuantidade.Text = $"{totalProcessadoGeral} / {totalGeral}"
                                lblQuantidade.Visible = True
                                Application.DoEvents()
                            End Sub)
                    Next
                Finally
                    For Each conn In conexoesAbertas
                        conn.Dispose()
                    Next
                End Try
            Else
                ' ==========================================
                ' UMA EMPRESA
                ' ==========================================
                Using conn = Conexao.Abrir(
                    empresaSelecionada.Conexao.Servidor,
                    empresaSelecionada.Conexao.Porta,
                    empresaSelecionada.Conexao.Banco,
                    empresaSelecionada.Conexao.Usuario,
                    empresaSelecionada.Conexao.Senha)

                    Dim total As Integer = ExportadorXML.ContarXMLs(
                        conn,
                        empresaSelecionada.Codigo,
                        dtInicio.Value,
                        dtFim.Value,
                        incluirEmitidos,
                        incluirCancelados,
                        incluirInutilizados,
                        modelo,
                        cupomInicial,
                        cupomFinal,
                        serie)

                    pbExportacao.Minimum = 0
                    pbExportacao.Maximum = total
                    pbExportacao.Value = 0

                    lblQuantidade.Text = $"0 / {total}"
                    lblQuantidade.Visible = True
                    lblStatus.Text = "Exportando..."
                    Application.DoEvents()

                    ' Determinar quais tipos exportar
                    Dim exportarNFCe As Boolean = (modelo = 0 Or modelo = 65)
                    Dim exportarNFe As Boolean = (modelo = 0 Or modelo = 55)

                    Dim processadosUnica As Integer = 0

                    If exportarNFCe Then
                        ExportadorXML.ExportarNFCe(
                            conn,
                            empresaSelecionada.Codigo,
                            dtInicio.Value,
                            dtFim.Value,
                            txtDestino.Text,
                            incluirEmitidos,
                            incluirCancelados,
                            incluirInutilizados,
                            cupomInicial,
                            cupomFinal,
                            serie,
                            Sub(processados As Integer, totalItem As Integer)
                                processadosUnica += 1
                                pbExportacao.Value = Math.Min(processadosUnica, pbExportacao.Maximum)
                                lblQuantidade.Text = $"{processadosUnica} / {total}"
                                Application.DoEvents()
                            End Sub)
                    End If

                    If exportarNFe Then
                        ExportadorXML.ExportarNFe(
                            conn,
                            empresaSelecionada.Codigo,
                            dtInicio.Value,
                            dtFim.Value,
                            txtDestino.Text,
                            incluirEmitidos,
                            incluirCancelados,
                            modelo,
                            cupomInicial,
                            cupomFinal,
                            serie,
                            Sub(processados As Integer, totalItem As Integer)
                                processadosUnica += 1
                                pbExportacao.Value = Math.Min(processadosUnica, pbExportacao.Maximum)
                                lblQuantidade.Text = $"{processadosUnica} / {total}"
                                Application.DoEvents()
                            End Sub)
                    End If
                End Using
            End If

            pbExportacao.Value = pbExportacao.Maximum
            lblQuantidade.Text = $"{pbExportacao.Maximum} / {pbExportacao.Maximum}"
            lblQuantidade.Visible = True
            lblStatus.Text = "Concluído!"
            lblStatus.Visible = True

            LogService.RegistrarAtividade(
                $"Exportar: Empresa=""{empresaSelecionada.Nome}"", Período={dtInicio.Value:dd/MM/yyyy}-{dtFim.Value:dd/MM/yyyy} " &
                $"-> {pbExportacao.Maximum} XML(s) exportado(s) em ""{txtDestino.Text}""")

            Dim resposta = MessageBox.Show(
                "Exportação concluída." & vbCrLf & vbCrLf &
                "Deseja enviar o arquivo por e-mail?",
                "Exportação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If resposta = DialogResult.Yes Then
                Dim cfgEmail = ConfiguracaoService.Carregar()
                Dim mensagem As String
                Dim competencia As String = dtInicio.Value.ToString("yyyyMM")

                Dim nomeEmpresaEmail As String
                If empresaSelecionada.Codigo = 0 Then
                    nomeEmpresaEmail = "Todas as empresas"
                Else
                    nomeEmpresaEmail = empresaSelecionada.Nome
                End If

                mensagem = $"Prezados,

                Segue em anexo a exportação dos arquivos XML referentes à competência {competencia}.

                Empresa(s): {nomeEmpresaEmail}

                O arquivo ZIP contém os arquivos XML separados por empresa.

                Em caso de dúvidas, permanecemos à disposição.

                Atenciosamente,
                {nomeEmpresaEmail}"

                EmailService.Enviar(
                    cfgEmail.ServidorSMTP,
                    cfgEmail.PortaSMTP,
                    cfgEmail.UsuarioSMTP,
                    cfgEmail.SenhaSMTP,
                    cfgEmail.EmailRemetente.Trim(),
                    txtDestinatario.Text.Trim(),
                    "XMLs Exportados - " & nomeEmpresaEmail & " - " & competencia,
                    mensagem,
                    txtDestino.Text,
                    cfgEmail.UsarSSL)

                LogService.RegistrarAtividade($"Exportar: e-mail enviado para ""{txtDestinatario.Text.Trim()}""")
                MessageBox.Show("E-mail enviado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            LogService.RegistrarAtividade($"Exportar -> ERRO: {ex.Message}")
            MessageBox.Show($"Erro na exportação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Monta o caminho padrão de exportação.
    ''' </summary>
    ''' <returns>
    ''' "{pasta}\{empresa}.zip" — a pasta é a que já está em <c>txtDestino</c>
    ''' (extraída do caminho atual), ou a Área de Trabalho se nada tiver sido
    ''' escolhido ainda; o nome do arquivo é SEMPRE recalculado a partir da
    ''' empresa atualmente selecionada (via <see cref="ExportadorXML.NomeArquivoValido"/>),
    ''' mesmo que o usuário tenha digitado outra coisa em <c>txtDestino</c> —
    ''' de propósito, pra garantir que o nome do zip sempre corresponda à
    ''' empresa que está sendo exportada.
    ''' </returns>
    ''' <summary>
    ''' Nome "puro" da empresa selecionada (sem o sufixo "(Banco)" que a combo
    ''' mostra quando há mais de uma conexão configurada) — usado pra nomear
    ''' o arquivo/pasta de exportação e o corpo do e-mail.
    ''' </summary>
    Private Function NomeEmpresaSelecionada() As String
        Return If(TryCast(cboEmpresa.SelectedItem, EmpresaItem)?.Nome, cboEmpresa.Text)
    End Function

    Private Function ObterCaminhoDestinoPadrao() As String
        Dim nomeArquivo As String = ExportadorXML.NomeArquivoValido(NomeEmpresaSelecionada()) & ".zip"

        Dim pasta As String = Nothing
        If Not String.IsNullOrWhiteSpace(txtDestino.Text) Then
            pasta = IO.Path.GetDirectoryName(txtDestino.Text)
        End If

        If String.IsNullOrWhiteSpace(pasta) Then
            pasta = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        End If

        Return IO.Path.Combine(pasta, nomeArquivo)
    End Function

    ''' <summary>
    ''' Botão "Selecionar Pasta": abre um seletor de PASTA (não de arquivo — o
    ''' nome do arquivo nunca é escolhido pelo usuário, só a pasta) e monta
    ''' <c>txtDestino</c> a partir da pasta escolhida + nome da empresa atual.
    ''' </summary>
    Private Sub btnDestino_Click(sender As Object, e As EventArgs) Handles btnDestino.Click
        Dim pastaInicial As String = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)

        If Not String.IsNullOrWhiteSpace(txtDestino.Text) Then
            Dim pastaAtual As String = IO.Path.GetDirectoryName(txtDestino.Text)
            If IO.Directory.Exists(pastaAtual) Then
                pastaInicial = pastaAtual
            End If
        End If

        FolderBrowserDialog1.Description = "Selecione a pasta onde a exportação será salva"
        FolderBrowserDialog1.UseDescriptionForTitle = True
        FolderBrowserDialog1.SelectedPath = pastaInicial

        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            txtDestino.Text = IO.Path.Combine(
                FolderBrowserDialog1.SelectedPath,
                ExportadorXML.NomeArquivoValido(NomeEmpresaSelecionada()) & ".zip")

            LogService.RegistrarAtividade($"Selecionar Pasta: ""{FolderBrowserDialog1.SelectedPath}""")
        End If
    End Sub

    ''' <summary>Atualiza o label que mostra o(s) banco(s) configurado(s) (aba Configurações → Conexão).</summary>
    Private Sub AtualizarConfiguracoes()
        Dim cfg = ConfiguracaoService.Carregar()

        Select Case cfg.Conexoes.Count
            Case 0
                lbServ.Text = "Nenhum banco configurado"
            Case 1
                lbServ.Text = cfg.Conexoes(0).Servidor
            Case Else
                lbServ.Text = $"{cfg.Conexoes.Count} bancos: " & String.Join(", ", cfg.Conexoes.Select(Function(c) c.Nome))
        End Select
    End Sub

    ''' <summary>
    ''' Tenta conectar com os dados atuais de config.json e (re)popula os
    ''' combos de Empresa e Fornecedor. Extraído do <see cref="Form1_Load"/>
    ''' para poder ser chamado de novo assim que o usuário salva uma nova
    ''' configuração de servidor — sem isso, a lista de empresas só aparecia
    ''' depois de fechar e reabrir o aplicativo.
    ''' </summary>
    ''' <remarks>
    ''' A parte que abre conexão com cada banco (potencialmente lenta — rede,
    ''' servidor fora do ar, vários bancos configurados) roda em
    ''' <see cref="Task.Run"/>, FORA da thread de interface — só o resultado
    ''' final é aplicado nos controles. Sem isso, abrir o app com um banco
    ''' lento/inacessível travava a JANELA de abrir até o timeout de conexão
    ''' (ver chamada em <see cref="FrmPrincipal_Shown"/>).
    ''' </remarks>
    ''' <param name="cfg">Configuração atual (servidor/usuário/senha do banco).</param>
    Private Async Function CarregarEmpresasEFornecedoresAsync(cfg As Configuracoes) As Task
        If cfg.Conexoes.Count = 0 Then
            lblStatus.Text = "Nenhum banco configurado. Configure em ""Configurar Servidor""."
            cboEmpresa.Enabled = False
            Return
        End If

        lblStatus.Text = "Conectando..."
        lblStatus.Visible = True

        Dim resultado = Await Task.Run(Function() ListarEmpresasDeTodosOsBancos(cfg))
        Dim todasEmpresas = resultado.Empresas
        Dim errosConexao = resultado.Erros

        cboEmpresa.DataSource = todasEmpresas
        cboEmpresa.DisplayMember = If(cfg.Conexoes.Count > 1, "NomeExibicao", "Nome")

        Dim itemParaSelecionar = todasEmpresas.FirstOrDefault(Function(it) it.Codigo = cfg.UltimaEmpresa)
        cboEmpresa.SelectedItem = If(itemParaSelecionar, todasEmpresas(0))

        cboEmpresa.Enabled = True
        Await CarregarFornecedoresDaConexaoAsync(TryCast(cboEmpresa.SelectedItem, EmpresaItem)?.Conexao)

        If errosConexao.Count > 0 Then
            lblStatus.Text = $"Conectado, mas com erro em: {String.Join("; ", errosConexao)}"
        Else
            lblStatus.Text = $"Conectado com sucesso. {todasEmpresas.Count - 1} empresa(s) carregada(s)."
        End If
    End Function

    ''' <summary>
    ''' Parte da carga de empresas que só faz I/O (abrir conexão, consultar) —
    ''' de propósito, NÃO toca em nenhum controle da tela, pra poder ser
    ''' chamada de dentro de <see cref="Task.Run"/> com segurança.
    ''' </summary>
    Private Function ListarEmpresasDeTodosOsBancos(cfg As Configuracoes) As (Empresas As List(Of EmpresaItem), Erros As List(Of String))
        Dim todasEmpresas As New List(Of EmpresaItem)
        Dim errosConexao As New List(Of String)

        For Each banco As ConexaoBanco In cfg.Conexoes
            Try
                Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                    For Each empresa As EmpresaItem In EmpresaService.Listar(conn).Where(Function(emp) emp.Codigo <> 0)
                        empresa.Conexao = banco
                        todasEmpresas.Add(empresa)
                    Next
                End Using
            Catch ex As Exception
                errosConexao.Add($"{banco.Nome} ({ex.Message})")
            End Try
        Next

        todasEmpresas.Insert(0, New EmpresaItem With {.Codigo = 0, .Nome = "Todas as empresas"})
        Return (todasEmpresas, errosConexao)
    End Function

    ''' <summary>
    ''' (Re)carrega a combo Fornecedor a partir de UM banco específico — sempre o
    ''' mesmo banco da empresa atualmente selecionada, nunca uma mistura de
    ''' bancos diferentes (os códigos de fornecedor não são comparáveis entre
    ''' bancos separados). Com <paramref name="banco"/> Nothing (item "Todas as
    ''' empresas" selecionado, ou nenhum banco configurado), a combo fica só com
    ''' o sentinela "Todos os fornecedores", desabilitada. A consulta em si roda
    ''' em <see cref="Task.Run"/> pelo mesmo motivo de <see cref="CarregarEmpresasEFornecedoresAsync"/>.
    ''' </summary>
    Private Async Function CarregarFornecedoresDaConexaoAsync(banco As ConexaoBanco) As Task
        If banco Is Nothing Then
            cboFornecedor.DataSource = New List(Of FornecedorItem) From {
                New FornecedorItem With {.Codigo = 0, .Nome = "Todos os fornecedores"}
            }
            cboFornecedor.DisplayMember = "Nome"
            cboFornecedor.ValueMember = "Codigo"
            cboFornecedor.Enabled = False
            Return
        End If

        Dim fornecedores = Await Task.Run(Function() ListarFornecedoresDeUmBanco(banco))

        If fornecedores IsNot Nothing Then
            cboFornecedor.DataSource = fornecedores
            cboFornecedor.DisplayMember = "Nome"
            cboFornecedor.ValueMember = "Codigo"
            cboFornecedor.Enabled = True
        Else
            cboFornecedor.DataSource = New List(Of FornecedorItem) From {
                New FornecedorItem With {.Codigo = 0, .Nome = "Todos os fornecedores"}
            }
            cboFornecedor.DisplayMember = "Nome"
            cboFornecedor.ValueMember = "Codigo"
            cboFornecedor.Enabled = False
        End If
    End Function

    ''' <summary>Só a parte de I/O de <see cref="CarregarFornecedoresDaConexaoAsync"/> — Nothing em caso de erro.</summary>
    Private Function ListarFornecedoresDeUmBanco(banco As ConexaoBanco) As List(Of FornecedorItem)
        Try
            Using conn = Conexao.Abrir(banco.Servidor, banco.Porta, banco.Banco, banco.Usuario, banco.Senha)
                Return FornecedorService.Listar(conn)
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Abre a tela modal de configuração do servidor; ao fechar, atualiza o
    ''' label de servidor e já tenta conectar/recarregar Empresa e Fornecedor
    ''' com os dados recém-salvos (não precisa fechar e abrir o app de novo).
    ''' </summary>
    Private Async Sub btnConfigurarServidor_Click(sender As Object, e As EventArgs) Handles btnConfigurarServidor.Click
        LogService.RegistrarAtividade("Abriu Configurar Bancos")

        Dim frm As New FrmBancos
        frm.ShowDialog()
        AtualizarConfiguracoes()

        Dim cfg = ConfiguracaoService.Carregar()
        Await CarregarEmpresasEFornecedoresAsync(cfg)
        lblStatus.Visible = True
    End Sub

    ''' <summary>Abre a tela modal de configuração de e-mail/SMTP.</summary>
    Private Sub btnConfigurarEmail_Click(sender As Object, e As EventArgs) Handles btnConfigurarEmail.Click
        LogService.RegistrarAtividade("Abriu Configurar E-mail")

        Dim frm As New FrmEmail
        frm.ShowDialog()
        AtualizarConfiguracoes()
    End Sub

    '=========================
    ' EVENTOS
    '=========================

    ''' <summary>
    ''' Ao trocar de empresa: lembra a escolha em config.json e já recalcula o
    ''' nome do arquivo de destino (mantendo a pasta atual), pra refletir a
    ''' nova empresa mesmo antes de clicar em Exportar.
    ''' </summary>
    Private Async Sub cboEmpresa_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboEmpresa.SelectedIndexChanged
        If carregando Then Exit Sub

        Dim empresaSelecionada = TryCast(cboEmpresa.SelectedItem, EmpresaItem)
        If empresaSelecionada Is Nothing Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.UltimaEmpresa = empresaSelecionada.Codigo
        ConfiguracaoService.Salvar(cfg)

        txtDestino.Text = ObterCaminhoDestinoPadrao()
        Await CarregarFornecedoresDaConexaoAsync(empresaSelecionada.Conexao)
    End Sub

    ''' <summary>Grava o destinatário de e-mail em config.json quando o campo perde o foco.</summary>
    Private Sub txtDestinatario_Leave(sender As Object, e As EventArgs) Handles txtDestinatario.Leave
        If carregando Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.UltimoDestinatario = txtDestinatario.Text.Trim()
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>Lembra "NFC-e" como último modelo escolhido. Um handler igual existe para cada radio do grupo Modelo.</summary>
    Private Sub rbNFCe_CheckedChanged(sender As Object, e As EventArgs) Handles rbNFCe.CheckedChanged
        If carregando Then Exit Sub
        If Not rbNFCe.Checked Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.UltimoModelo = 65
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>Lembra "NFe" como último modelo escolhido.</summary>
    Private Sub rbNFe_CheckedChanged(sender As Object, e As EventArgs) Handles rbNFe.CheckedChanged
        If carregando Then Exit Sub
        If Not rbNFe.Checked Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.UltimoModelo = 55
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>Lembra "Ambos" como último modelo escolhido.</summary>
    Private Sub rbAmbos_CheckedChanged(sender As Object, e As EventArgs) Handles rbAmbos.CheckedChanged
        If carregando Then Exit Sub
        If Not rbAmbos.Checked Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.UltimoModelo = 0
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>
    ''' Dispara <see cref="AtualizarModoDirecao"/> sempre que o radio Saída/Entrada muda.
    ''' Diferente dos radios de Modelo, a escolha de Direção NÃO é persistida em
    ''' config.json — sempre volta pra "Saída" ao reabrir o programa.
    ''' </summary>
    Private Sub rbDirecao_CheckedChanged(sender As Object, e As EventArgs) Handles rbSaida.CheckedChanged, rbEntrada.CheckedChanged
        AtualizarModoDirecao()
    End Sub

    ''' <summary>
    ''' Habilita/desabilita e mostra/esconde os controles da tela conforme a
    ''' Direção escolhida (Saída/Entrada). Chamado sempre que o radio de
    ''' Direção muda, e uma vez no <see cref="Form1_Load"/>.
    ''' </summary>
    ''' <remarks>
    ''' Entrada (compra) só tem listagem/pesquisa por enquanto: o XML de
    ''' entrada não fica salvo no banco, só um caminho de arquivo local de
    ''' quem importou a nota, então não há como exportar/zipar com
    ''' confiabilidade ainda — por isso Modelo, Status, Nº Doc./Série ficam
    ''' desabilitados (não se aplicam à entrada) e o botão Exportar também.
    ''' </remarks>
    Private Sub AtualizarModoDirecao()
        Dim entrada As Boolean = rbEntrada.Checked

        lblFornecedor.Visible = entrada
        cboFornecedor.Visible = entrada

        lblModelo.Enabled = Not entrada
        rbNFCe.Enabled = Not entrada
        rbNFe.Enabled = Not entrada
        rbAmbos.Enabled = Not entrada

        lblStatusFiltro.Enabled = Not entrada
        chkTodos.Enabled = Not entrada
        chkEmitidas.Enabled = Not entrada
        chkCancelados.Enabled = Not entrada
        chkInutilizados.Enabled = Not entrada

        lblNumDoc.Enabled = Not entrada
        txbInicio.Enabled = Not entrada
        txbFim.Enabled = Not entrada
        lblSerieFiltro.Enabled = Not entrada
        txbSerie.Enabled = Not entrada

        btnExportar.Enabled = Not entrada
        btnExportar.Text = If(entrada, "Indisponível", "Exportar")

        Fornecedor.Visible = entrada
    End Sub

    ''' <summary>
    ''' "Todos" e os checkboxes de status específicos são mutuamente exclusivos
    ''' na interface: marcar "Todos" desmarca os outros três.
    ''' </summary>
    Private Sub chkTodos_CheckedChanged(sender As Object, e As EventArgs) Handles chkTodos.CheckedChanged
        If chkTodos.Checked Then
            chkEmitidas.Checked = False
            chkCancelados.Checked = False
            chkInutilizados.Checked = False
        End If
    End Sub

    ''' <summary>
    ''' Marcar qualquer status específico desmarca "Todos"; desmarcar o último
    ''' status específico que restava marca "Todos" de novo automaticamente
    ''' (a tela nunca fica com os 4 checkboxes desmarcados ao mesmo tempo).
    ''' </summary>
    Private Sub chkStatus_CheckedChanged(sender As Object, e As EventArgs) Handles chkEmitidas.CheckedChanged, chkCancelados.CheckedChanged, chkInutilizados.CheckedChanged
        If chkEmitidas.Checked Or chkCancelados.Checked Or chkInutilizados.Checked Then
            chkTodos.Checked = False
        Else
            chkTodos.Checked = True
        End If
    End Sub

    '=========================
    ' AGENDAMENTO AUTOMÁTICO
    '=========================

    ''' <summary>Liga/desliga o agendamento automático mensal, gravando em config.json.</summary>
    Private Sub chkAgendamentoAtivo_CheckedChanged(sender As Object, e As EventArgs) Handles chkAgendamentoAtivo.CheckedChanged
        If carregando Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.AgendamentoAtivo = chkAgendamentoAtivo.Checked
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>Grava o horário configurado do agendamento (hora e minuto separados) em config.json.</summary>
    Private Sub dtpHoraAgendamento_ValueChanged(sender As Object, e As EventArgs) Handles dtpHoraAgendamento.ValueChanged
        If carregando Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.HoraAgendamento = dtpHoraAgendamento.Value.Hour
        cfg.MinutoAgendamento = dtpHoraAgendamento.Value.Minute
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>
    ''' Alterna entre "todo dia 01" (trava em 1) e dia personalizado (libera o
    ''' campo <see cref="nudDiaAgendamento"/> pra edição) — ambos gravam em
    ''' <c>DiaAgendamento</c> no config.json.
    ''' </summary>
    Private Sub chkDiaFixo_CheckedChanged(sender As Object, e As EventArgs) Handles chkDiaFixo.CheckedChanged
        nudDiaAgendamento.Enabled = Not chkDiaFixo.Checked

        If carregando Then Exit Sub

        If chkDiaFixo.Checked Then
            nudDiaAgendamento.Value = 1
        End If

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.DiaAgendamento = CInt(nudDiaAgendamento.Value)
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>Grava o dia personalizado do agendamento em config.json (só relevante quando "Todo dia 01" está desmarcado).</summary>
    Private Sub nudDiaAgendamento_ValueChanged(sender As Object, e As EventArgs) Handles nudDiaAgendamento.ValueChanged
        If carregando Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.DiaAgendamento = CInt(nudDiaAgendamento.Value)
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>Grava o e-mail de alerta de falha do agendamento quando o campo perde o foco.</summary>
    Private Sub txtEmailAlertaFalha_Leave(sender As Object, e As EventArgs) Handles txtEmailAlertaFalha.Leave
        If carregando Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.EmailAlertaFalha = txtEmailAlertaFalha.Text.Trim()
        ConfiguracaoService.Salvar(cfg)
    End Sub

    ''' <summary>
    ''' Adiciona ou remove a entrada do aplicativo na chave Run do registro do
    ''' Windows (HKCU), pra iniciar automaticamente junto com o login do
    ''' usuário. Não precisa de privilégio de administrador (é HKCU, não
    ''' HKLM). Falhas (ex.: chave bloqueada por política de grupo) só mostram
    ''' um aviso, não travam a tela.
    ''' </summary>
    Private Sub chkIniciarComWindows_CheckedChanged(sender As Object, e As EventArgs) Handles chkIniciarComWindows.CheckedChanged
        If carregando Then Exit Sub

        Try
            Using chave = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
                If chkIniciarComWindows.Checked Then
                    chave.SetValue("ExportaXML", $"""{Application.ExecutablePath}""")
                Else
                    chave.DeleteValue("ExportaXML", False)
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show(
                $"Não foi possível alterar a inicialização com o Windows: {ex.Message}",
                "Aviso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Liga/desliga o "Vigia" (<see cref="VigiaService"/>): uma tarefa no
    ''' Agendador de Tarefas do Windows que tenta abrir o programa a cada
    ''' poucos minutos — se já estiver aberto, não faz nada; se tiver caído ou
    ''' sido fechado por engano, reabre sozinho. Não precisa de privilégio de
    ''' administrador (tarefa do usuário atual, sem privilégios elevados).
    ''' </summary>
    Private Sub chkManterSempreAtivo_CheckedChanged(sender As Object, e As EventArgs) Handles chkManterSempreAtivo.CheckedChanged
        If carregando Then Exit Sub

        Try
            If chkManterSempreAtivo.Checked Then
                VigiaService.Ativar()
                LogService.RegistrarAtividade("Vigia (manter sempre em execução) ativado")
            Else
                VigiaService.Desativar()
                LogService.RegistrarAtividade("Vigia (manter sempre em execução) desativado")
            End If
        Catch ex As Exception
            LogService.RegistrarAtividade($"Vigia (manter sempre em execução) -> ERRO: {ex.Message}")
            MessageBox.Show(
                $"Não foi possível alterar o Vigia: {ex.Message}",
                "Aviso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            ' Ressincroniza a caixa com o estado real, sem disparar este mesmo
            ' handler de novo (carregando=True suprime CheckedChanged recursivo).
            carregando = True
            chkManterSempreAtivo.Checked = VigiaService.EstaAtivo()
            carregando = False
        End Try
    End Sub

    ''' <summary>
    ''' Lê diretamente do registro do Windows se a entrada de inicialização
    ''' automática já existe — é a fonte da verdade pro estado inicial do
    ''' checkbox (não guarda esse estado em config.json, pra não desincronizar
    ''' se alguém remover a entrada do registro por fora do programa).
    ''' </summary>
    Private Function IniciarComWindowsEstaAtivo() As Boolean
        Try
            Using chave = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", False)
                Return chave IsNot Nothing AndAlso chave.GetValue("ExportaXML") IsNot Nothing
            End Using
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Botão "Testar agora": executa o agendamento na hora, ignorando o
    ''' horário configurado e se a competência já rodou este mês — útil pra
    ''' validar toda a cadeia (exportação + e-mail + log) sem esperar o dia 01.
    ''' </summary>
    ''' <remarks>
    ''' ATENÇÃO: isso executa de verdade — gera o ZIP com dados reais do banco
    ''' e envia um e-mail real. Ao terminar, também grava
    ''' <c>UltimaCompetenciaExecutada</c>, então rodar isso "consome" a
    ''' competência do mês (a verificação automática não dispara de novo pra
    ''' esse mesmo mês, mas o próprio botão "Testar agora" ignora isso e roda
    ''' de novo se clicado outra vez).
    ''' </remarks>
    Private Sub btnTestarAgendamento_Click(sender As Object, e As EventArgs) Handles btnTestarAgendamento.Click
        LogService.RegistrarAtividade("Testar Agendamento (manual)")

        Try
            btnTestarAgendamento.Enabled = False
            ExecutarAgendamento(forcar:=True)
            LogService.RegistrarAtividade("Testar Agendamento (manual) -> concluído")
            MessageBox.Show(
                "Agendamento executado. Confira o log em ""Logs"" e o e-mail enviado.",
                "Agendamento",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
        Catch ex As Exception
            LogService.RegistrarAtividade($"Testar Agendamento (manual) -> ERRO: {ex.Message}")
            MessageBox.Show($"Falha ao testar o agendamento: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnTestarAgendamento.Enabled = True
        End Try
    End Sub

    ''' <summary>
    ''' Dispara a cada 1h (Interval configurado no Designer): reavalia se é
    ''' hora de rodar o agendamento mensal.
    ''' </summary>
    Private Sub tmrAgendamento_Tick(sender As Object, e As EventArgs) Handles tmrAgendamento.Tick
        VerificarEExecutarAgendamento()
    End Sub

    ''' <summary>
    ''' Verifica, sem interromper o uso normal do programa, se está na hora de
    ''' rodar o agendamento mensal (<see cref="AgendamentoService.DeveExecutar"/>);
    ''' se estiver, executa. Chamado também uma vez no <see cref="FrmPrincipal_Shown"/>,
    ''' pra pegar o caso de o programa ter sido aberto depois do horário
    ''' configurado (não precisa esperar o próximo tick do timer).
    ''' </summary>
    ''' <remarks>
    ''' A checagem em si (<see cref="AgendamentoService.DeveExecutar"/>) é só
    ''' data/config, rápida — mas se ela disser "sim, é agora", a execução de
    ''' verdade (<see cref="ExecutarAgendamento"/>) é jogada pra
    ''' <see cref="Task.Run"/>, porque ela exporta e envia e-mail de verdade e
    ''' não pode travar a tela (nem o timer de 1h que também chama isto).
    ''' </remarks>
    Private Sub VerificarEExecutarAgendamento()
        Try
            VerificarPendenciaEAvisar()

            Dim cfg = ConfiguracaoService.Carregar()
            If Not AgendamentoService.DeveExecutar(cfg) Then Exit Sub

            Task.Run(Sub() ExecutarAgendamento(forcar:=False))
        Catch
            ' Verificação silenciosa: qualquer falha aqui já é tratada e registrada
            ' dentro de AgendamentoService.ExecutarAgendamentoMensal.
        End Try
    End Sub

    ''' <summary>
    ''' Se houver uma falha de agendamento pendente (gravada em
    ''' <see cref="PendenciaAgendamentoService"/> por
    ''' <see cref="AgendamentoService.ExecutarAgendamentoMensal"/> — inclusive
    ''' quando quem executou foi o Windows Service, sem interface nenhuma) que
    ''' ainda não foi avisada, mostra um balão de aviso na bandeja. Aparece
    ''' mesmo com a janela minimizada/escondida — não precisa reabrir o app
    ''' pra ver. Chamado tanto ao abrir a tela quanto no timer horário (ver
    ''' <see cref="VerificarEExecutarAgendamento"/>), então o atraso máximo
    ''' pra alguém ver é de ~1h depois da falha, se o app já estava aberto.
    ''' </summary>
    Private Sub VerificarPendenciaEAvisar()
        Dim pendencia = PendenciaAgendamentoService.Obter()
        If pendencia Is Nothing OrElse pendencia.Notificada Then Exit Sub

        notifyIcon1.ShowBalloonTip(
            10000,
            "Falha no agendamento automático",
            $"O envio agendado de {pendencia.Competencia} não foi concluído: {pendencia.Mensagem}",
            ToolTipIcon.Warning)

        PendenciaAgendamentoService.MarcarNotificada()
    End Sub

    ''' <summary>
    ''' Abre uma conexão e efetivamente chama <see cref="AgendamentoService.ExecutarAgendamentoMensal"/>.
    ''' </summary>
    ''' <param name="forcar">
    ''' True quando chamado pelo botão "Testar agora" (roda direto na thread de
    ''' interface, de propósito — o usuário já espera uma pausa nesse caso, e o
    ''' botão fica desabilitado durante a execução); False quando chamado pela
    ''' verificação automática, que já joga a chamada pra uma thread de fundo
    ''' (ver <see cref="VerificarEExecutarAgendamento"/>).
    ''' </param>
    ''' <remarks>
    ''' O callback de status usa <see cref="Control.Invoke"/> porque este método
    ''' pode ser chamado tanto da thread de interface (botão "Testar agora")
    ''' quanto de uma thread de fundo (verificação automática) — <c>Invoke</c>
    ''' funciona corretamente nos dois casos.
    ''' </remarks>
    Private Sub ExecutarAgendamento(forcar As Boolean)
        Dim cfg = ConfiguracaoService.Carregar()

        If Not forcar AndAlso Not AgendamentoService.DeveExecutar(cfg) Then Exit Sub

        AgendamentoService.ExecutarAgendamentoMensal(
            cfg,
            Sub(status As String)
                Me.Invoke(Sub()
                              lblStatus.Text = status
                              lblStatus.Visible = True
                              Application.DoEvents()
                          End Sub)
            End Sub)
    End Sub

    '=========================
    ' ATUALIZAÇÃO AUTOMÁTICA
    '=========================

    ''' <summary>Dispara a cada 30 min (Interval configurado no Designer): reverifica se há atualização disponível.</summary>
    Private Sub tmrAtualizacao_Tick(sender As Object, e As EventArgs) Handles tmrAtualizacao.Tick
        VerificarAtualizacaoDisponivelAsync()
    End Sub

    ''' <summary>
    ''' Botão "Verificar Atualizações": checa na hora (com feedback visível —
    ''' MessageBox avisando se já está atualizado, ou perguntando antes de
    ''' baixar/aplicar), diferente da verificação silenciosa automática.
    ''' </summary>
    Private Async Sub btnVerificarAtualizacao_Click(sender As Object, e As EventArgs) Handles btnVerificarAtualizacao.Click
        LogService.RegistrarAtividade("Verificar Atualizações (manual)")
        btnVerificarAtualizacao.Enabled = False
        Try
            Dim info = Await Task.Run(Function() AtualizacaoService.VerificarAtualizacao())

            If info Is Nothing Then
                LogService.RegistrarAtividade("Verificar Atualizações (manual) -> já estava na versão mais recente")
                MessageBox.Show(
                    "Você já está usando a versão mais recente (ou o aplicativo não foi instalado via atualizador — isso é normal ao rodar pelo Visual Studio).",
                    "Verificar Atualizações",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
            Else
                Dim resposta = MessageBox.Show(
                    $"Versão {info.TargetFullRelease.Version} disponível. Baixar e atualizar agora? O aplicativo vai fechar e reabrir sozinho.",
                    "Verificar Atualizações",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)

                If resposta = DialogResult.Yes Then
                    LogService.RegistrarAtividade($"Verificar Atualizações (manual) -> baixando e aplicando versão {info.TargetFullRelease.Version}")
                    Await Task.Run(Sub() AtualizacaoService.BaixarEAplicar(info))
                Else
                    LogService.RegistrarAtividade($"Verificar Atualizações (manual) -> versão {info.TargetFullRelease.Version} disponível, atualização recusada pelo usuário")
                End If
            End If
        Finally
            btnVerificarAtualizacao.Enabled = True
        End Try
    End Sub

    ''' <summary>
    ''' Botão "Ajuda (F1)" e atalho F1 (ver <see cref="FrmPrincipal_KeyDown"/>):
    ''' abrem o guia de ajuda integrado (<see cref="FrmAjuda"/>). Não é modal,
    ''' então se já houver uma instância aberta ela só é trazida para frente em
    ''' vez de abrir uma segunda janela.
    ''' </summary>
    Private Sub btnAjuda_Click(sender As Object, e As EventArgs) Handles btnAjuda.Click
        AbrirGuiaDeAjuda()
    End Sub

    Private Sub FrmPrincipal_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F1 Then
            AbrirGuiaDeAjuda()
        End If
    End Sub

    Private Sub AbrirGuiaDeAjuda()
        Dim guiaAberto = Application.OpenForms.OfType(Of FrmAjuda)().FirstOrDefault()

        If guiaAberto IsNot Nothing Then
            guiaAberto.Activate()
        Else
            LogService.RegistrarAtividade("Abriu Guia de Ajuda")
            Dim frm As New FrmAjuda()
            frm.Show(Me)
        End If
    End Sub

    ''' <summary>
    ''' Verificação silenciosa de atualização (chamada no início e a cada
    ''' <c>tmrAtualizacao.Tick</c>): se achar uma versão nova, baixa e reinicia
    ''' o aplicativo sozinho, sem perguntar nada — pensada pra rodar sem
    ''' intervenção enquanto o app fica na bandeja.
    ''' </summary>
    ''' <remarks>
    ''' As chamadas a <see cref="AtualizacaoService"/> ficam dentro de
    ''' <c>Task.Run(...)</c> de propósito: os métodos daquela classe são
    ''' bloqueantes e chamá-los direto na thread de interface trava o
    ''' aplicativo (ver o aviso na própria <see cref="AtualizacaoService"/>).
    ''' Se a atualização for aplicada com sucesso, o aplicativo fecha e reabre
    ''' sozinho — esta chamada simplesmente não retorna nesse caso.
    ''' </remarks>
    Private Async Sub VerificarAtualizacaoDisponivelAsync()
        Try
            Dim info = Await Task.Run(Function() AtualizacaoService.VerificarAtualizacao())
            If info Is Nothing Then Return

            Await Task.Run(Sub() AtualizacaoService.BaixarEAplicar(info))
        Catch
            ' Qualquer falha aqui já fica registrada em Logs\Atualizacao.log;
            ' não interrompe o uso normal do aplicativo.
        End Try
    End Sub

    ''' <summary>
    ''' Ao clicar no X da janela, NÃO fecha o aplicativo de verdade — só
    ''' esconde a janela e mostra um balão de aviso, mantendo os Timers de
    ''' Agendamento/Atualização rodando em segundo plano. Fechar de verdade só
    ''' acontece via "Sair" no menu da bandeja (ver <see cref="SairToolStripMenuItem_Click"/>).
    ''' </summary>
    Private Sub FrmPrincipal_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
            notifyIcon1.ShowBalloonTip(
                2000,
                "Sistema em execução",
                "O sistema continua executando em segundo plano.",
                ToolTipIcon.Info)
        End If
    End Sub

    ''' <summary>Item "Sair" do menu da bandeja: aqui sim encerra o processo de verdade.</summary>
    ''' <summary>
    ''' Encerra o aplicativo de verdade (diferente do X da janela, que só
    ''' esconde). <c>Application.Exit()</c> por si só nem sempre garante que o
    ''' PROCESSO termine na hora — se algum código em segundo plano (ex.: uma
    ''' verificação de atualização em andamento) ainda estiver rodando, o
    ''' processo pode continuar vivo, invisível, sem ícone na bandeja, ainda
    ''' segurando o Mutex de instância única (ver <see cref="InstanciaUnica"/>)
    ''' — nesse caso, abrir o programa de novo não faz nada, porque ele acha
    ''' que já existe uma instância rodando (e de fato existe, só que sem
    ''' janela pra mostrar). Por isso, depois do Exit, força o encerramento
    ''' do processo com <see cref="Environment.Exit"/> como garantia.
    ''' </summary>
    Private Sub SairToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SairToolStripMenuItem.Click
        LogService.RegistrarAtividade("Sair (bandeja) - encerrando o aplicativo")

        notifyIcon1.Visible = False
        notifyIcon1.Dispose()
        Application.Exit()
        Environment.Exit(0)
    End Sub

    ''' <summary>Item "Abrir" do menu da bandeja: traz a janela de volta, restaurada e em foco.</summary>
    Private Sub AbrirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AbrirToolStripMenuItem.Click
        AbrirJanela()
    End Sub

    ''' <summary>
    ''' Duplo clique no ícone da bandeja também reabre a janela — mais
    ''' descobrível do que precisar clicar com o botão direito e escolher
    ''' "Abrir" no menu, principalmente pra quem está usando o programa pela
    ''' primeira vez.
    ''' </summary>
    Private Sub notifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles notifyIcon1.DoubleClick
        AbrirJanela()
    End Sub

    ''' <summary>Restaura e traz a janela principal para frente, a partir da bandeja.</summary>
    Private Sub AbrirJanela()
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        Me.Activate()
    End Sub

    ''' <summary>
    ''' Escuta a mensagem de broadcast que uma SEGUNDA tentativa de abrir o
    ''' aplicativo manda (ver <see cref="InstanciaUnica"/> e <c>EntryPoint.Main</c>)
    ''' quando já existe uma instância rodando — é assim que "abrir o programa
    ''' de novo" (pelo atalho, Menu Iniciar etc.) enquanto ele já está
    ''' minimizado na bandeja simplesmente restaura a janela existente, em vez
    ''' de abrir uma segunda janela por cima.
    ''' </summary>
    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = InstanciaUnica.MensagemMostrarJanela Then
            AbrirJanela()
        End If

        MyBase.WndProc(m)
    End Sub

    ''' <summary>Ao minimizar, some da barra de tarefas também (só fica visível o ícone da bandeja).</summary>
    Private Sub FrmPrincipal_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
        End If
    End Sub

    ''' <summary>
    ''' Stub vazio gerado pelo Designer do Visual Studio (duplo clique num
    ''' controle) — não tem "Handles", não está ligado a nenhum evento real, e
    ''' o controle "PictureBox1" nem existe mais na tela. Seguro remover numa
    ''' limpeza futura.
    ''' </summary>
    Private Sub PictureBox1_Click(sender As Object, e As EventArgs)

    End Sub

    ''' <summary>
    ''' Stub vazio gerado pelo Designer (duplo clique no label Fornecedor) —
    ''' não faz nada. Seguro remover numa limpeza futura.
    ''' </summary>
    Private Sub lblFornecedor_Click(sender As Object, e As EventArgs) Handles lblFornecedor.Click

    End Sub
End Class