# Exportador XML

Aplicativo Windows (WinForms / VB.NET) que centraliza a exportação, pesquisa e
envio automático de XMLs fiscais (NFC-e e NFe) a partir de um banco PostgreSQL
(Duesoft/Utilar), com agendamento mensal automático, consulta de notas de
entrada (compras) e atualização automática de versão.

O aplicativo roda em segundo plano, na bandeja do sistema, e é pensado para ser
instalado uma vez em cada cliente e depois se manter atualizado sozinho.

## Funcionalidades

- **Exportação de XMLs de saída** (NFC-e e/ou NFe) por empresa, com filtros de
  período, modelo, status (emitidos/cancelados/inutilizados), número de
  documento e série — uma empresa por vez ou todas de uma vez, num único
  arquivo `.zip` consolidado.
- **Pesquisa** dos documentos antes de exportar, numa grade na própria tela.
- **Consulta de notas de entrada** (compras de fornecedores) — somente
  listagem, sem exportação de arquivo (ver [docs/Manual_Funcionalidades.pdf](docs/Manual_Funcionalidades.pdf)
  para o motivo).
- **Envio por e-mail** do arquivo exportado, sob demanda.
- **Agendamento automático mensal**: todo dia 01, exporta sozinho os XMLs de
  todas as empresas referentes ao mês anterior e envia por e-mail, com log e
  alerta de falha por e-mail.
- **Atualização automática** via [Velopack](https://velopack.io), usando os
  Releases deste repositório como origem — instalações existentes se
  atualizam sozinhas, sem reinstalação manual.

Documentação completa das funcionalidades, do ponto de vista de quem usa o
aplicativo: **[docs/Manual_Funcionalidades.pdf](docs/Manual_Funcionalidades.pdf)**.

## Tecnologia

- VB.NET / WinForms, .NET 10 (`net10.0-windows`)
- [Npgsql](https://www.npgsql.org/) — acesso ao PostgreSQL
- [MailKit](https://github.com/jstedfast/MailKit) — envio de e-mail (SMTP)
- [Velopack](https://velopack.io) — empacotamento e atualização automática

## Estrutura do projeto

```
ExportaXML/
  Classes/        Regras de negócio (exportação, agendamento, atualização, e-mail, log, conexão)
  Config/         Modelo de configuração persistida (config.json) e serviço de leitura/gravação
  Forms/          Telas (principal, configurar servidor, configurar e-mail)
  Models/         Modelo de configuração (versão atual, usada de fato)
  Program.vb      Ponto de entrada (Sub Main) — inicializa o Velopack antes de tudo
docs/             Manual de funcionalidades (PDF) e demais documentação de apoio
RELEASE.md        Passo a passo para publicar uma nova versão
```

## Configuração

Na primeira execução, o aplicativo cria um `config.json` ao lado do executável
com valores em branco. Configure pela própria interface:

1. **Configurações → Conexão → Configurar Servidor**: dados do PostgreSQL
   (servidor, porta, banco, usuário, senha). Use o botão "Testar" para validar.
2. **Configurações → E-mail → Configurar E-mail**: servidor SMTP para envio dos
   relatórios e alertas. Use o botão de teste de envio para validar.
3. **Configurações → Agendamento Automático**: opcional — habilite se quiser a
   exportação e envio mensal automáticos.

## Desenvolvimento

Requer o SDK do .NET 10 com o workload de Windows Desktop.

```bash
dotnet build ExportaXML/ExportaXML.vbproj
```

Para rodar localmente (sem passar pelo instalador/atualizador), basta compilar
e executar o `.exe` gerado normalmente — a checagem de atualização é
automaticamente ignorada quando o aplicativo não foi instalado via Velopack.

## Publicando uma nova versão

Veja o passo a passo completo em **[RELEASE.md](RELEASE.md)**: como
versionar, empacotar com `vpk` e publicar no GitHub Releases para que todas as
instalações existentes se atualizem sozinhas.
