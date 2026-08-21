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
    ''' Inicializa a tela: datas padrão (mês atual), carrega config.json nos
    ''' campos correspondentes, tenta conectar e popular os combos de Empresa e
    ''' Fornecedor, e dispara as primeiras verificações de Agendamento e
    ''' Atualização automática.
    ''' </summary>
    ''' <remarks>
    ''' Se usuário/senha do banco estiverem vazios em config.json, a conexão
    ''' NEM É TENTADA (evita um erro certo logo na abertura em uma instalação
    ''' ainda não configurada) — só desabilita o combo de Empresa e mostra uma
    ''' mensagem. Qualquer outra falha de conexão é capturada e também vira só
    ''' uma mensagem de status, nunca uma exceção não tratada.
    ''' </remarks>
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

        ' Não tenta conectar na inicialização se usuário/senha estiverem vazios
        If String.IsNullOrWhiteSpace(cfg.Usuario) OrElse String.IsNullOrWhiteSpace(cfg.Senha) Then
            lblStatus.Text = "Conexão não configurada. Configure servidor, usuário e senha."
            cboEmpresa.Enabled = False
        Else
            Try
                Using conn = Conexao.Abrir(
                    cfg.Servidor,
                    cfg.Porta,
                    cfg.Banco,
                    cfg.Usuario,
                    cfg.Senha)

                    cboEmpresa.DataSource = EmpresaService.Listar(conn)
                    cboEmpresa.DisplayMember = "Nome"
                    cboEmpresa.ValueMember = "Codigo"

                    If cboEmpresa.Items.Count > 0 Then
                        cboEmpresa.SelectedValue = cfg.UltimaEmpresa
                    End If

                    cboFornecedor.DataSource = FornecedorService.Listar(conn)
                    cboFornecedor.DisplayMember = "Nome"
                    cboFornecedor.ValueMember = "Codigo"
                End Using

                cboEmpresa.Enabled = True
            Catch ex As Exception
                lblStatus.Text = $"Erro ao conectar: {ex.Message}"
                cboEmpresa.Enabled = False
            End Try
        End If
        carregando = False

        rbSaida.Checked = True
        AtualizarModoDirecao()

        lblQuantidade.Visible = False
        lblStatus.Visible = False
        lblQtd.Visible = False

        lbServ.Text = cfg.Servidor

        AtualizarConfiguracoes()
        chkTodos.Checked = True

        If Not String.IsNullOrWhiteSpace(cboEmpresa.Text) Then
            txtDestino.Text = ObterCaminhoDestinoPadrao()
        End If

        tmrAgendamento.Enabled = True
        VerificarEExecutarAgendamento()

        lblVersao.Text = $"Versão {My.Application.Info.Version.ToString(3)}"
        tmrAtualizacao.Enabled = True
        VerificarAtualizacaoDisponivelAsync()
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

            Dim cod_empresa As Integer = CInt(cboEmpresa.SelectedValue)

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

            Using conn = Conexao.Abrir(
                cfg.Servidor,
                cfg.Porta,
                cfg.Banco,
                cfg.Usuario,
                cfg.Senha)

                If rbEntrada.Checked Then
                    Dim cod_fornecedor As Integer = If(cboFornecedor.SelectedValue IsNot Nothing, CInt(cboFornecedor.SelectedValue), 0)

                    dgvCupons.DataSource = ExportadorXML.BuscarCompras(
                        conn,
                        cod_empresa,
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

                    dgvCupons.DataSource = ExportadorXML.BuscarCupons(
                        conn,
                        cod_empresa,
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
            End Using

            Dim dt As DataTable = CType(dgvCupons.DataSource, DataTable)
            lblQtd.Text = $"XMLs encontrados: {dt.Rows.Count}"
            lblQtd.Visible = True

        Catch ex As Exception
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

            Dim codigoEmpresa As Integer = CInt(cboEmpresa.SelectedValue)

            lblStatus.Text = "Contando XMLs..."
            lblStatus.Visible = True
            Application.DoEvents()

            Dim cfg = ConfiguracaoService.Carregar()

            Using conn = Conexao.Abrir(
                cfg.Servidor,
                cfg.Porta,
                cfg.Banco,
                cfg.Usuario,
                cfg.Senha)

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

                Dim total As Integer = ExportadorXML.ContarXMLs(
                    conn,
                    codigoEmpresa,
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

                If codigoEmpresa = 0 Then
                    ' ==========================================
                    ' TODAS AS EMPRESAS
                    ' ==========================================
                    Dim empresas As List(Of EmpresaItem) = EmpresaService.Listar(conn)
                    empresas = empresas.Where(Function(emp) emp.Codigo <> 0).ToList()

                    lblStatus.Text = "Preparando exportação..."
                    Application.DoEvents()

                    ExportadorXML.ExportarTodasEmpresas(
                        conn,
                        empresas,
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
                        Sub(processados As Integer, totalGeral As Integer)
                            If totalGeral > 0 Then
                                pbExportacao.Maximum = totalGeral
                                pbExportacao.Value = Math.Min(processados, totalGeral)
                                lblQuantidade.Text = $"{processados} / {totalGeral}"
                                lblQuantidade.Visible = True
                            End If
                            Application.DoEvents()
                        End Sub)
                Else
                    ' ==========================================
                    ' UMA EMPRESA
                    ' ==========================================
                    If IO.File.Exists(txtDestino.Text) Then
                        IO.File.Delete(txtDestino.Text)
                    End If

                    lblStatus.Text = "Exportando..."
                    Application.DoEvents()

                    ' Determinar quais tipos exportar
                    Dim exportarNFCe As Boolean = (modelo = 0 Or modelo = 65)
                    Dim exportarNFe As Boolean = (modelo = 0 Or modelo = 55)

                    Dim processadosUnica As Integer = 0

                    If exportarNFCe Then
                        ExportadorXML.ExportarNFCe(
                            conn,
                            codigoEmpresa,
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
                            codigoEmpresa,
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

                End If
            End Using

            pbExportacao.Value = pbExportacao.Maximum
            lblQuantidade.Text = $"{pbExportacao.Maximum} / {pbExportacao.Maximum}"
            lblQuantidade.Visible = True
            lblStatus.Text = "Concluído!"
            lblStatus.Visible = True

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
                If codigoEmpresa = 0 Then
                    nomeEmpresaEmail = "Todas as empresas"
                Else
                    nomeEmpresaEmail = cboEmpresa.Text
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

                MessageBox.Show("E-mail enviado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
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
    Private Function ObterCaminhoDestinoPadrao() As String
        Dim nomeArquivo As String = ExportadorXML.NomeArquivoValido(cboEmpresa.Text) & ".zip"

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
                ExportadorXML.NomeArquivoValido(cboEmpresa.Text) & ".zip")
        End If
    End Sub

    ''' <summary>Atualiza o label que mostra o servidor configurado (aba Configurações → Conexão).</summary>
    Private Sub AtualizarConfiguracoes()
        Dim cfg = ConfiguracaoService.Carregar()
        lbServ.Text = cfg.Servidor
    End Sub

    ''' <summary>Abre a tela modal de configuração do servidor e atualiza o label ao fechar.</summary>
    Private Sub btnConfigurarServidor_Click(sender As Object, e As EventArgs) Handles btnConfigurarServidor.Click
        Dim frm As New FrmServidor
        frm.ShowDialog()
        AtualizarConfiguracoes()
    End Sub

    ''' <summary>Abre a tela modal de configuração de e-mail/SMTP.</summary>
    Private Sub btnConfigurarEmail_Click(sender As Object, e As EventArgs) Handles btnConfigurarEmail.Click
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
    Private Sub cboEmpresa_SelectedValueChanged(sender As Object, e As EventArgs) Handles cboEmpresa.SelectedValueChanged
        If carregando Then Exit Sub
        If cboEmpresa.SelectedValue Is Nothing Then Exit Sub

        Dim tmpEmpresa As Integer
        If Not Integer.TryParse(If(cboEmpresa.SelectedValue?.ToString(), String.Empty), tmpEmpresa) Then Exit Sub

        Dim cfg = ConfiguracaoService.Carregar()
        cfg.UltimaEmpresa = Convert.ToInt32(cboEmpresa.SelectedValue)
        ConfiguracaoService.Salvar(cfg)

        txtDestino.Text = ObterCaminhoDestinoPadrao()
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
        Try
            btnTestarAgendamento.Enabled = False
            ExecutarAgendamento(forcar:=True)
            MessageBox.Show(
                "Agendamento executado. Confira o log em ""Logs"" e o e-mail enviado.",
                "Agendamento",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
        Catch ex As Exception
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
    ''' se estiver, executa. Chamado também uma vez no <see cref="Form1_Load"/>,
    ''' pra pegar o caso de o programa ter sido aberto depois do horário
    ''' configurado (não precisa esperar o próximo tick do timer).
    ''' </summary>
    Private Sub VerificarEExecutarAgendamento()
        Try
            Dim cfg = ConfiguracaoService.Carregar()
            If Not AgendamentoService.DeveExecutar(cfg) Then Exit Sub

            ExecutarAgendamento(forcar:=False)
        Catch
            ' Verificação silenciosa: qualquer falha aqui já é tratada e registrada
            ' dentro de AgendamentoService.ExecutarAgendamentoMensal.
        End Try
    End Sub

    ''' <summary>
    ''' Abre uma conexão e efetivamente chama <see cref="AgendamentoService.ExecutarAgendamentoMensal"/>.
    ''' </summary>
    ''' <param name="forcar">
    ''' True quando chamado pelo botão "Testar agora" (pula a checagem de
    ''' <see cref="AgendamentoService.DeveExecutar"/>); False quando chamado
    ''' pela verificação automática (que já checou antes de chegar aqui, mas
    ''' checa de novo por segurança).
    ''' </param>
    Private Sub ExecutarAgendamento(forcar As Boolean)
        Dim cfg = ConfiguracaoService.Carregar()

        If Not forcar AndAlso Not AgendamentoService.DeveExecutar(cfg) Then Exit Sub

        Using conn = Conexao.Abrir(cfg.Servidor, cfg.Porta, cfg.Banco, cfg.Usuario, cfg.Senha)
            AgendamentoService.ExecutarAgendamentoMensal(
                conn,
                cfg,
                Sub(status As String)
                    lblStatus.Text = status
                    lblStatus.Visible = True
                    Application.DoEvents()
                End Sub)
        End Using
    End Sub

    '=========================
    ' ATUALIZAÇÃO AUTOMÁTICA
    '=========================

    ''' <summary>Dispara a cada 4h (Interval configurado no Designer): reverifica se há atualização disponível.</summary>
    Private Sub tmrAtualizacao_Tick(sender As Object, e As EventArgs) Handles tmrAtualizacao.Tick
        VerificarAtualizacaoDisponivelAsync()
    End Sub

    ''' <summary>
    ''' Botão "Verificar Atualizações": checa na hora (com feedback visível —
    ''' MessageBox avisando se já está atualizado, ou perguntando antes de
    ''' baixar/aplicar), diferente da verificação silenciosa automática.
    ''' </summary>
    Private Async Sub btnVerificarAtualizacao_Click(sender As Object, e As EventArgs) Handles btnVerificarAtualizacao.Click
        btnVerificarAtualizacao.Enabled = False
        Try
            Dim info = Await Task.Run(Function() AtualizacaoService.VerificarAtualizacao())

            If info Is Nothing Then
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
                    Await Task.Run(Sub() AtualizacaoService.BaixarEAplicar(info))
                End If
            End If
        Finally
            btnVerificarAtualizacao.Enabled = True
        End Try
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
    Private Sub SairToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SairToolStripMenuItem.Click
        notifyIcon1.Visible = False
        Application.Exit()
    End Sub

    ''' <summary>Item "Abrir" do menu da bandeja: traz a janela de volta, restaurada e em foco.</summary>
    Private Sub AbrirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AbrirToolStripMenuItem.Click
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        Me.Activate()
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