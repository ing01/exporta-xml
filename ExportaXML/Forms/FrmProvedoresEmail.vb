''' <summary>
''' Referência rápida, só de leitura, com servidor/porta/autenticação dos
''' provedores de e-mail mais comuns — aberta pelo botão "Provedores Comuns"
''' na tela "Configurar E-mail" (<see cref="FrmEmail"/>), pra quem estiver
''' configurando não precisar procurar isso em outro lugar.
''' </summary>
''' <remarks>
''' Conteúdo estático (estas configurações podem mudar de tempos em tempos
''' pelo próprio provedor) — se algum provedor mudar a política de segurança
''' e algo aqui parar de funcionar, atualize o texto em <see cref="MontarConteudo"/>.
''' </remarks>
Public Class FrmProvedoresEmail

    Private Class Provedor
        Public Property Nome As String
        Public Property Linhas As String()
    End Class

    Private Sub FrmProvedoresEmail_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MontarConteudo()
    End Sub

    Private Sub MontarConteudo()
        Dim provedores As New List(Of Provedor) From {
            New Provedor With {
                .Nome = "Gmail / Google Workspace",
                .Linhas = {
                    "Servidor: smtp.gmail.com",
                    "Porta: 587 (TLS/STARTTLS) ou 465 (SSL) — 587 é a mais comum.",
                    "Precisa de verificação em duas etapas (2FA) ativada na conta Google.",
                    "Precisa de uma ""Senha de app"" (não é a senha normal da conta) — gerada em:",
                    "myaccount.google.com > Segurança > Verificação em duas etapas > Senhas de app."
                }
            },
            New Provedor With {
                .Nome = "Outlook.com / Hotmail / Live — GERALMENTE NÃO FUNCIONA MAIS",
                .Linhas = {
                    "A Microsoft desativou por completo o login SMTP por usuário/senha (""autenticação básica"") —",
                    "cronograma concluído em abril/2026. Nem ""Senha de app"" resolve mais, porque ela também",
                    "dependia dessa autenticação básica que foi desligada. Contas Hotmail/Outlook.com pessoais",
                    "criadas recentemente já vêm assim de fábrica, sem nenhuma opção do usuário pra reativar.",
                    "Esse programa só faz login SMTP simples (usuário+senha) — não dá pra contornar isso mudando",
                    "configuração aqui. Se aparecer o erro ""535 5.7.139... basic authentication is disabled"",",
                    "é exatamente isso.",
                    "",
                    "Alternativas: usar Gmail (funciona normalmente, veja acima) ou um serviço de envio como",
                    "SendGrid/Brevo/Mailgun/Amazon SES (todos aceitam usuário+senha via SMTP, com plano grátis",
                    "suficiente pra um volume de exportação mensal)."
                }
            },
            New Provedor With {
                .Nome = "Yahoo Mail",
                .Linhas = {
                    "Servidor: smtp.mail.yahoo.com",
                    "Porta: 587 (TLS/STARTTLS) ou 465 (SSL).",
                    "Quase sempre exige uma ""Senha de app"" (independente de ter 2FA ativado) — gerada em:",
                    "Conta Yahoo > Segurança da conta > Gerar senha de app."
                }
            },
            New Provedor With {
                .Nome = "iCloud Mail (Apple)",
                .Linhas = {
                    "Servidor: smtp.mail.me.com",
                    "Porta: 587 (TLS/STARTTLS).",
                    "Precisa de verificação em duas etapas ativada no ID Apple.",
                    "Precisa de uma ""Senha específica de app"", gerada em: appleid.apple.com > Segurança."
                }
            },
            New Provedor With {
                .Nome = "Zoho Mail",
                .Linhas = {
                    "Servidor: smtp.zoho.com",
                    "Porta: 587 (TLS/STARTTLS) ou 465 (SSL).",
                    "Se a conta tiver 2FA ativado, precisa de uma ""Senha de aplicativo"" (gerada nas configurações",
                    "de segurança da conta Zoho); sem 2FA, geralmente a senha normal funciona."
                }
            },
            New Provedor With {
                .Nome = "Hospedagem própria / webmail personalizado (domínio próprio da empresa)",
                .Linhas = {
                    "Não existe um servidor único — normalmente é algo como ""smtp.nomedaempresa.com.br"" ou um",
                    "endereço fornecido pela empresa de hospedagem (ex.: Locaweb, HostGator, KingHost, cPanel).",
                    "Porta: geralmente 587 (TLS) ou 465 (SSL), mas varia por hospedagem.",
                    "O usuário/senha costuma ser a própria caixa de e-mail completa (ex.: contato@empresa.com.br).",
                    "Consulte o painel de controle da hospedagem (ou o suporte dela) para confirmar os dados exatos."
                }
            }
        }

        rtbConteudo.Clear()

        rtbConteudo.SelectionFont = New Font(rtbConteudo.Font, FontStyle.Italic)
        rtbConteudo.AppendText("Configurações abaixo válidas na data em que este guia foi escrito — provedores mudam a política de segurança de vez em quando. Se algo não funcionar, confira o suporte do próprio provedor." & vbCrLf & vbCrLf)

        For Each provedor In provedores
            Dim inicioTitulo = rtbConteudo.TextLength
            rtbConteudo.AppendText(provedor.Nome & vbCrLf)
            rtbConteudo.Select(inicioTitulo, provedor.Nome.Length)
            rtbConteudo.SelectionFont = New Font(rtbConteudo.Font, FontStyle.Bold)
            rtbConteudo.SelectionColor = Color.FromArgb(30, 60, 120)
            rtbConteudo.Select(rtbConteudo.TextLength, 0)
            rtbConteudo.SelectionFont = New Font(rtbConteudo.Font, FontStyle.Regular)
            rtbConteudo.SelectionColor = rtbConteudo.ForeColor

            For Each linha In provedor.Linhas
                rtbConteudo.AppendText("   " & linha & vbCrLf)
            Next

            rtbConteudo.AppendText(vbCrLf)
        Next

        rtbConteudo.Select(0, 0)
    End Sub

    Private Sub btnFechar_Click(sender As Object, e As EventArgs) Handles btnFechar.Click
        Me.Close()
    End Sub
End Class
