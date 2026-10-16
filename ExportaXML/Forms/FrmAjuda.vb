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
        NovoNo(configuracoes.Nodes, "destinatarios", "Destinatários por Empresa")
        NovoNo(configuracoes.Nodes, "agendamento", "Agendamento automático")
        NovoNo(configuracoes.Nodes, "restaurar", "Restaurar versão (GitHub)")

        NovoNo(trvTopicos.Nodes, "logs", "Problemas e Ver log")
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
            "Direção: Saída (vendas/cupons) ou Entrada (compras próprias e devoluções de venda)." & vbCrLf & vbCrLf &
            "Modelo: NFe, NFCe ou Ambos — só se aplica à Saída." & vbCrLf & vbCrLf &
            "Tipo de Nota (só aparece com Direção=Entrada): Compras Próprias, Devoluções de Venda, Fornecedores (compras de fornecedor externo) ou Todos (as três, padrão). IMPORTANTE — ""Fornecedores"" só funciona com notas lançadas a partir do DS Estoque 4.8.239: antes dessa versão o XML de compra de fornecedor externo não ficava salvo no banco, então notas antigas não têm como ser exportadas (aparecem na grade destacadas como ""SEM XML""). Compras Próprias e Devoluções de Venda sempre funcionaram normalmente, sem essa restrição." & vbCrLf & vbCrLf &
            "Número do documento (início/fim) e Série: restringem a busca a uma faixa específica (só Saída); deixe em branco para não filtrar." & vbCrLf & vbCrLf &
            "Status: Emitidos, Cancelados, Inutilizados ou Todos (só Saída — Entrada sempre inclui emitidas e canceladas)."

        conteudo("acoes") =
            "Pesquisar: só consulta e mostra o resultado na grade — não gera nenhum arquivo." & vbCrLf & vbCrLf &
            "Selecionar Pasta: escolhe a pasta onde o ZIP será salvo. O nome do arquivo é sempre o nome da empresa selecionada, não pode ser digitado." & vbCrLf & vbCrLf &
            "Exportar: gera o(s) ZIP(s) com os XMLs que batem com os filtros atuais e, ao terminar, pergunta se deseja enviar o resultado por e-mail. Funciona tanto para Saída quanto para Entrada (compras próprias/devoluções de venda). Enquanto uma exportação está em andamento, o botão não aceita um segundo clique." & vbCrLf & vbCrLf &
            "Ao responder ""Sim"" no envio por e-mail, abre a janela ""Enviar por e-mail"": nela você escolhe de qual conta o e-mail sai (Global ou a conta de uma das empresas exportadas — com ""Todas as empresas"" o padrão é a Global) e confere o servidor/usuário que serão usados, e informa o destinatário (a lista traz os endereços cadastrados; dá para digitar outro ou vários, separados por "";""). Se a conta estiver incompleta ou o endereço for inválido, o programa avisa antes de tentar enviar. Se o envio falhar, a mensagem mostra servidor, usuário, destinatário e o motivo em português; o ZIP já gerado continua salvo na pasta escolhida. Anexos acima de 20 MB são recusados com aviso (a maioria dos provedores não aceita): exporte um período menor ou uma empresa por vez." & vbCrLf & vbCrLf &
            "Exportar por série (só Saída): quando marcado, dentro do ZIP de cada empresa os XMLs deixam de ficar soltos e passam a vir separados em um ZIP para cada série (""NFCe_Serie_X.zip"", ""NFe_Serie_X.zip""). Útil quando a empresa usa séries diferentes para finalidades diferentes e cada uma precisa ser tratada separadamente depois."

        conteudo("resultado") =
            "A grade mostra os documentos encontrados pelo botão Pesquisar: modelo, número, código, fornecedor, série, chave, status e data." & vbCrLf & vbCrLf &
            "A contagem de XMLs encontrados aparece acima da grade." & vbCrLf & vbCrLf &
            "Documentos (NFC-e, NFe ou, na Entrada, Compras de Fornecedores) que aparecem destacados em laranja/amarelo, com o aviso abaixo da grade, batem com os filtros de data e status mas NÃO têm nenhum XML gerado no banco (nem autorizado, nem cancelado, nem de inutilização, dependendo do caso) — por isso não são incluídos no ZIP exportado, mesmo aparecendo na contagem. Em Compras de Fornecedores, isso é normal para qualquer nota lançada antes do DS Estoque 4.8.239."

        conteudo("conexao") =
            "Botão ""Configurar Bancos"" (aba Configurações): abre a lista de bancos PostgreSQL cadastrados, com opções para Adicionar, Editar e Remover." & vbCrLf & vbCrLf &
            "É possível cadastrar mais de um banco — útil quando as empresas do cliente estão espalhadas em bancos diferentes (mesmo servidor ou não). Nesse caso, o combo Empresa passa a mostrar ""(Nome do banco)"" ao lado de cada empresa, e ""Todas as empresas"" passa a exportar/pesquisar em todos os bancos cadastrados de uma vez." & vbCrLf & vbCrLf &
            "Cada banco tem seu próprio endereço, porta, nome do banco, usuário e senha, e pode ser testado antes de salvar. Com um único banco cadastrado, a tela funciona exatamente como antes (sem nenhum sufixo na combo)."

        conteudo("email") =
            "Botão ""Configurar E-mail"": abre uma lista com TODAS as empresas (+ ""Global"", a configuração padrão), mostrando como cada uma está configurada hoje (Padrão/Manual/Sincronizado, e quantos destinatários tem). Selecione uma linha e clique em ""Configurar"" (ou dê duplo clique) para abrir os detalhes SÓ daquela empresa — o nome dela aparece grande no topo da tela, pra não ter dúvida de qual configuração é qual." & vbCrLf & vbCrLf &
            "Modo de envio de cada empresa: ""Usar configuração padrão"" (usa a Global, sem nada configurado ali); ""Manual"" (servidor/porta/usuário/senha/SSL digitados só para essa empresa); ""Sincronizar do Banco"" (busca automaticamente, a cada envio, o e-mail já configurado no sistema do próprio cliente para aquela empresa — útil quando um mesmo servidor hospeda várias empresas, cada uma com sua própria conta de e-mail)." & vbCrLf & vbCrLf &
            "Destinatários: quem recebe o ZIP quando você confirma o envio depois de uma exportação — cada empresa tem os seus, editados na mesma tela de detalhe. Com mais de um banco cadastrado, a empresa é identificada pelo BANCO + código: o destinatário (e a conta de e-mail) cadastrado para a empresa 1 de um banco NÃO vale para a empresa 1 de outro banco. Cadastros feitos em versões antigas, que só guardavam o código, continuam funcionando e são convertidos automaticamente: se o código existe em um único banco, ficam presos a ele; se existe em mais de um banco, o cadastro passa a ter uma cópia para cada banco que tem aquele código (nada deixa de funcionar); depois disso você pode editar ou remover o destinatário de cada empresa separadamente. Antes da conversão o programa guarda uma cópia do arquivo de configuração (config.json.antes-da-migracao)." & vbCrLf & vbCrLf &
            "Botão ""Provedores Comuns"" (dentro do detalhe de cada empresa): mostra servidor, porta e requisitos de autenticação (senha de app, verificação em duas etapas etc.) dos provedores mais usados — Gmail, Outlook/Hotmail, Yahoo, iCloud, Zoho e hospedagem própria."

        conteudo("agendamento") =
            "Uma vez por mês, no dia e horário configurados, o programa exporta o mês anterior inteiro de cada empresa e envia um e-mail por empresa, com o ZIP dela anexado, sem precisar abrir a tela." & vbCrLf & vbCrLf &
            "Dia e horário: se o computador estava desligado ou o programa fechado nessa hora, o envio acontece assim que o programa voltar a rodar. Ao ligar o agendamento depois do dia configurado, o programa pergunta se quer enviar o mês anterior agora ou só a partir do próximo mês." & vbCrLf & vbCrLf &
            "Quem recebe: SOMENTE o destinatário cadastrado para a própria empresa. Se a empresa não tiver um, o e-mail NÃO é enviado (o destinatário Global e o último usado manualmente não são usados no envio automático, para não mandar XML de uma empresa para outra pessoa): ela conta como falha, aparece no aviso e no log, e é enviada na próxima tentativa assim que você cadastrar o destinatário. Empresas sem nenhum XML no mês não recebem e-mail (fica anotado no log)." & vbCrLf & vbCrLf &
            "Se alguma empresa falhar (sem destinatário, senha de e-mail errada, anexo grande demais...): o programa NÃO marca o mês como concluído. Ele tenta de novo sozinho, só as empresas que faltaram (esperas de 15 min, 1 h, 3 h, 6 h e 12 h; no máximo 8 tentativas), sem reenviar para quem já recebeu. Um balão avisa na bandeja e o motivo aparece em ""Ver log"". Corrija o problema (ex.: cadastre o destinatário) e a próxima tentativa já envia." & vbCrLf & vbCrLf &
            "Só uma execução por vez: se o programa e o serviço do Windows estiverem ativos ao mesmo tempo, ou se você estiver exportando manualmente, o agendamento espera — nunca envia em duplicidade. Se o programa ficar mais de um mês sem rodar, o log avisa quais meses ficaram sem envio (o agendamento só envia o mês anterior; os outros precisam ser exportados à mão)." & vbCrLf & vbCrLf &
            "E-mail de alerta: recebe um aviso na primeira falha e quando o programa desiste (uma vez, não a cada tentativa). Usa a mesma conta de e-mail Global configurada." & vbCrLf & vbCrLf &
            "Iniciar com o Windows: coloca o programa para abrir automaticamente junto com o computador (necessário para o agendamento funcionar mesmo sem abrir a tela manualmente)." & vbCrLf & vbCrLf &
            "Botão ""Testar Agendamento"": dispara a rotina imediatamente, sem esperar o horário, e envia os e-mails de verdade. Se terminar com falhas, mostra o motivo na hora. Se já houver outra exportação em andamento, avisa e não executa."

        conteudo("logs") =
            "Quando algo não funciona (e-mail não chegou, agendamento não rodou), clique em ""Ver log"" (aba Configurações, logo abaixo do botão Ajuda)." & vbCrLf & vbCrLf &
            "A tela mostra no topo, em vermelho, a última falha do agendamento (ou ""Nenhuma falha pendente""). Escolha na lista o arquivo desejado: ""Agendamento_AAAA-MM"" (o que aconteceu em cada envio mensal, empresa por empresa), ""Atividade_AAAA-MM"" (ações feitas na tela e erros de exportação/envio manual) e ""Atualizacao"" (verificação de versões). Os botões permitem Atualizar, Copiar o texto (para mandar ao suporte) e Abrir a pasta dos logs." & vbCrLf & vbCrLf &
            "As mensagens de erro que aparecem na tela agora vêm em português, dizendo o provável motivo e o que conferir (senha do banco ou do e-mail, servidor fora do ar, pasta sem permissão, arquivo aberto em outro programa, anexo grande demais etc.). O detalhe técnico original continua gravado no log para o suporte." & vbCrLf & vbCrLf &
            "Se o arquivo de configuração for danificado (queda de energia durante a gravação, por exemplo), o programa restaura sozinho a cópia de segurança anterior e avisa; se não houver cópia, abre em branco e avisa que é preciso reconfigurar (uma cópia do arquivo antigo é guardada)."

        conteudo("atualizacao") =
            "O programa verifica periodicamente (a cada 30 minutos) se existe uma versão mais nova disponível. Se houver uma exportação ou envio em andamento, a atualização espera terminar, para não interromper o trabalho no meio." & vbCrLf & vbCrLf &
            "O botão ""Verificar Atualizações"", no rodapé da tela, faz essa verificação a qualquer momento, sem esperar a checagem automática." & vbCrLf & vbCrLf &
            "A versão instalada aparece no canto inferior esquerdo da tela."

        conteudo("bandeja") =
            "Fechar a janela principal não encerra o programa — ele continua rodando em segundo plano, com um ícone na bandeja do sistema (perto do relógio do Windows)." & vbCrLf & vbCrLf &
            "Clicar no ícone da bandeja (ou usar o menu ""Abrir"") traz a janela de volta. ""Sair"" encerra o programa de verdade — se houver exportação ou envio em andamento, o programa pergunta antes de sair." & vbCrLf & vbCrLf &
            "Se você tentar abrir o programa de novo enquanto ele já está rodando em segundo plano, a janela existente é trazida para frente em vez de abrir uma segunda cópia."

        conteudo("dicas") =
            "- Selecionar ""Todas as empresas"" no combo Empresa exporta todas de uma vez, uma pasta por empresa." & vbCrLf & vbCrLf &
            "- Use Pesquisar antes de Exportar para confirmar quantos documentos batem com os filtros, sem gerar nenhum arquivo ainda." & vbCrLf & vbCrLf &
            "- O agendamento automático só roda se ""Iniciar com o Windows"" estiver marcado (ou se o programa já estiver aberto/na bandeja no horário configurado)." & vbCrLf & vbCrLf &
            "- Pressione F1 em qualquer momento na tela principal para reabrir este guia." & vbCrLf & vbCrLf &
            "- Toda ação importante (Pesquisar, Exportar, testar conexão/e-mail/agendamento, configurar banco/e-mail, verificar atualizações) fica registrada com data e hora em ""Atividade_AAAA-MM.log"", na mesma pasta dos outros logs (%LocalAppData%\ExportaXML\Logs). Para ler sem procurar arquivos, use o botão ""Ver log"" (aba Configurações, abaixo do Ajuda)." & vbCrLf & vbCrLf &
            "- Cliente reclamando que o e-mail não chegou? Abra ""Ver log"" e confira a primeira linha vermelha e o arquivo Agendamento do mês: ali aparece, empresa por empresa, quem recebeu, quem ficou sem destinatário e quem falhou (e por quê)."
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
