''' <summary>
''' Guia de ajuda integrado: árvore de tópicos à esquerda, texto explicativo à
''' direita. Não é modal — pode ficar aberto junto com o <see cref="FrmPrincipal"/>
''' enquanto o usuário navega pela tela principal (ver <see cref="FrmPrincipal.AbrirGuiaDeAjuda"/>).
''' </summary>
Public Class FrmAjuda

    ''' <summary>Texto de cada tópico, indexado pela <c>Tag</c> do nó correspondente na árvore.</summary>
    Private ReadOnly conteudo As New Dictionary(Of String, String)

    Private Sub FrmAjuda_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MontarConteudo()
        MontarArvore()
        trvTopicos.SelectedNode = trvTopicos.Nodes(0)
        trvTopicos.Nodes(0).Expand()
    End Sub

    ''' <summary>
    ''' Monta a árvore de tópicos, na mesma ordem em que as seções aparecem na
    ''' tela principal (aba Exportar/Pesquisar, depois aba Configurações, etc).
    ''' A Tag de cada nó é a chave usada em <see cref="conteudo"/>.
    ''' </summary>
    Private Sub MontarArvore()
        Dim visaoGeral = NovoNo(trvTopicos.Nodes, "visaoGeral", "Visão geral")

        Dim exportarPesquisar = NovoNo(trvTopicos.Nodes, "exportarPesquisar", "Exportar / Pesquisar")
        NovoNo(exportarPesquisar.Nodes, "filtros", "Filtros")
        NovoNo(exportarPesquisar.Nodes, "acoes", "Ações (Pesquisar / Exportar)")
        NovoNo(exportarPesquisar.Nodes, "resultado", "Resultado (grade)")

        Dim configuracoes = NovoNo(trvTopicos.Nodes, "configuracoes", "Configurações")
        NovoNo(configuracoes.Nodes, "conexao", "Conexão com o banco")
        NovoNo(configuracoes.Nodes, "email", "E-mail")
        NovoNo(configuracoes.Nodes, "agendamento", "Agendamento automático")

        NovoNo(trvTopicos.Nodes, "atualizacao", "Atualização automática")
        NovoNo(trvTopicos.Nodes, "bandeja", "Execução em segundo plano")
        NovoNo(trvTopicos.Nodes, "dicas", "Dicas rápidas")
    End Sub

    ''' <summary>Cria um nó com a Tag já preenchida (chave usada em <see cref="conteudo"/>).</summary>
    Private Function NovoNo(colecao As TreeNodeCollection, chave As String, texto As String) As TreeNode
        Dim no = colecao.Add(chave, texto)
        no.Tag = chave
        Return no
    End Function

    Private Sub MontarConteudo()
        conteudo("visaoGeral") =
            "O ExportaXML exporta os XMLs fiscais (NFe e NFCe) emitidos ou recebidos por uma ou mais empresas, gerando um arquivo ZIP por empresa." & vbCrLf & vbCrLf &
            "A tela principal tem duas abas:" & vbCrLf &
            "- Exportar / Pesquisar: filtra, consulta e exporta os documentos." & vbCrLf &
            "- Configurações: conexão com o banco, e-mail e agendamento automático." & vbCrLf & vbCrLf &
            "O programa também fica disponível na bandeja do sistema, mesmo depois de a janela ser fechada — veja o tópico ""Execução em segundo plano""."

        conteudo("exportarPesquisar") =
            "Aba onde os documentos são filtrados, consultados e exportados. Veja os tópicos ""Filtros"", ""Ações"" e ""Resultado"" para os detalhes de cada parte da tela."

        conteudo("configuracoes") =
            "Aba com a conexão do banco de dados, os dados de e-mail e o agendamento automático de exportação. Veja os tópicos ""Conexão com o banco"", ""E-mail"" e ""Agendamento automático""."

        conteudo("filtros") =
            "Empresa: qual empresa consultar. Selecionar ""Todas as empresas"" exporta todas de uma vez, uma pasta por empresa." & vbCrLf & vbCrLf &
            "Fornecedor: só se aplica quando a Direção é ""Entrada"" (compras). Com mais de um banco cadastrado, a lista de fornecedores é sempre a do banco da empresa selecionada — com ""Todas as empresas"" selecionado, o filtro de fornecedor fica desabilitado." & vbCrLf & vbCrLf &
            "Período: intervalo de datas dos documentos." & vbCrLf & vbCrLf &
            "Direção: Saída (vendas/cupons) ou Entrada (compras)." & vbCrLf & vbCrLf &
            "Modelo: NFe, NFCe ou Ambos — só se aplica à Saída." & vbCrLf & vbCrLf &
            "Número do documento (início/fim) e Série: restringem a busca a uma faixa específica; deixe em branco para não filtrar." & vbCrLf & vbCrLf &
            "Status: Emitidos, Cancelados, Inutilizados ou Todos."

        conteudo("acoes") =
            "Pesquisar: só consulta e mostra o resultado na grade — não gera nenhum arquivo." & vbCrLf & vbCrLf &
            "Selecionar Pasta: escolhe a pasta onde o ZIP será salvo. O nome do arquivo é sempre o nome da empresa selecionada, não pode ser digitado." & vbCrLf & vbCrLf &
            "Exportar: gera o(s) ZIP(s) com os XMLs que batem com os filtros atuais e, ao terminar, pergunta se deseja enviar o resultado por e-mail (usa a configuração de E-mail e o destinatário informado na aba Configurações). Fica desabilitado quando a Direção é ""Entrada"" — a exportação é só para os documentos de Saída."

        conteudo("resultado") =
            "A grade mostra os documentos encontrados pelo botão Pesquisar: modelo, número, código, fornecedor, série, chave, status e data." & vbCrLf & vbCrLf &
            "A contagem de XMLs encontrados aparece acima da grade."

        conteudo("conexao") =
            "Botão ""Configurar Bancos"" (aba Configurações): abre a lista de bancos PostgreSQL cadastrados, com opções para Adicionar, Editar e Remover." & vbCrLf & vbCrLf &
            "É possível cadastrar mais de um banco — útil quando as empresas do cliente estão espalhadas em bancos diferentes (mesmo servidor ou não). Nesse caso, o combo Empresa passa a mostrar ""(Nome do banco)"" ao lado de cada empresa, e ""Todas as empresas"" passa a exportar/pesquisar em todos os bancos cadastrados de uma vez." & vbCrLf & vbCrLf &
            "Cada banco tem seu próprio endereço, porta, nome do banco, usuário e senha, e pode ser testado antes de salvar. Com um único banco cadastrado, a tela funciona exatamente como antes (sem nenhum sufixo na combo)."

        conteudo("email") =
            "Botão ""Configurar E-mail"": dados do servidor SMTP (endereço, porta, usuário, senha, SSL) usados para enviar o ZIP exportado." & vbCrLf & vbCrLf &
            "O campo ""Destinatário"" na aba Configurações é o e-mail que recebe o ZIP quando você confirma o envio depois de uma exportação."

        conteudo("agendamento") =
            "Permite disparar a exportação automaticamente, todo dia, num horário fixo, sem precisar abrir a tela e clicar em Exportar." & vbCrLf & vbCrLf &
            "Horário do agendamento: hora em que a exportação automática roda." & vbCrLf & vbCrLf &
            "E-mail de alerta: recebe um aviso se a exportação agendada falhar." & vbCrLf & vbCrLf &
            "Iniciar com o Windows: coloca o programa para abrir automaticamente junto com o computador (necessário para o agendamento funcionar mesmo sem abrir a tela manualmente)." & vbCrLf & vbCrLf &
            "Manter sempre em execução: registra uma tarefa no Agendador de Tarefas do Windows que tenta reabrir o programa a cada 5 minutos — se ele já estiver aberto, não faz nada; se tiver sido fechado por engano ou travado, reabre sozinho. Não é um serviço do Windows (isso não é possível — serviços não têm acesso à área de trabalho para mostrar o ícone na bandeja), é uma forma de garantir que o programa nunca fique parado por muito tempo sem alguém notar." & vbCrLf & vbCrLf &
            "Botão ""Testar Agendamento"": dispara a rotina de agendamento imediatamente, sem esperar o horário configurado, útil para confirmar que está tudo certo."

        conteudo("atualizacao") =
            "O programa verifica periodicamente (a cada 30 minutos) se existe uma versão mais nova disponível." & vbCrLf & vbCrLf &
            "O botão ""Verificar Atualizações"", no rodapé da tela, faz essa verificação a qualquer momento, sem esperar a checagem automática." & vbCrLf & vbCrLf &
            "A versão instalada aparece no canto inferior esquerdo da tela."

        conteudo("bandeja") =
            "Fechar a janela principal não encerra o programa — ele continua rodando em segundo plano, com um ícone na bandeja do sistema (perto do relógio do Windows)." & vbCrLf & vbCrLf &
            "Clicar no ícone da bandeja (ou usar o menu ""Abrir"") traz a janela de volta. ""Sair"" encerra o programa de verdade." & vbCrLf & vbCrLf &
            "Se você tentar abrir o programa de novo enquanto ele já está rodando em segundo plano, a janela existente é trazida para frente em vez de abrir uma segunda cópia."

        conteudo("dicas") =
            "- Selecionar ""Todas as empresas"" no combo Empresa exporta todas de uma vez, uma pasta por empresa." & vbCrLf & vbCrLf &
            "- Use Pesquisar antes de Exportar para confirmar quantos documentos batem com os filtros, sem gerar nenhum arquivo ainda." & vbCrLf & vbCrLf &
            "- O agendamento automático só roda se ""Iniciar com o Windows"" estiver marcado (ou se o programa já estiver aberto/na bandeja no horário configurado)." & vbCrLf & vbCrLf &
            "- Pressione F1 em qualquer momento na tela principal para reabrir este guia." & vbCrLf & vbCrLf &
            "- Toda ação importante (Pesquisar, Exportar, testar conexão/e-mail/agendamento, configurar banco/e-mail, verificar atualizações) fica registrada com data e hora em ""Atividade_AAAA-MM.log"", na mesma pasta dos outros logs (%LocalAppData%\ExportaXML\Logs)."
    End Sub

    Private Sub trvTopicos_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trvTopicos.AfterSelect
        Dim chave = TryCast(e.Node.Tag, String)

        If chave IsNot Nothing AndAlso conteudo.ContainsKey(chave) Then
            rtbConteudo.Text = conteudo(chave)
        Else
            rtbConteudo.Text = ""
        End If
    End Sub

    Private Sub btnFechar_Click(sender As Object, e As EventArgs) Handles btnFechar.Click
        Me.Close()
    End Sub
End Class
