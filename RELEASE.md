# Como publicar uma nova versão do ExportaXML

Este guia descreve o passo a passo para lançar uma nova versão do aplicativo depois
de alterar o código. Todas as instalações já existentes (dos clientes) detectam e
aplicam essa nova versão sozinhas, automaticamente, dentro de até 30 minutos (ou
imediatamente, se o usuário abrir o programa depois da publicação) — não é preciso
visitar ou reinstalar nada manualmente em nenhum cliente.

> **Atenção ao testar**: se a instalação do cliente já estava aberta ANTES de você
> publicar a nova versão, ela só vai detectar a atualização no próximo ciclo
> automático (até 30 min) ou se o programa for fechado e reaberto — a checagem
> "de abertura" já rodou antes da versão nova existir. Isso não é um problema no
> release; é só uma questão de tempo. Para testar sem esperar, use o botão
> "Verificar Atualizações" na tela do cliente.

Esse processo foi validado de ponta a ponta (instalação real + duas atualizações
reais em sequência) durante o desenvolvimento desta funcionalidade.

## Visão geral do mecanismo

- O aplicativo usa a biblioteca **Velopack** para se atualizar sozinho.
- As versões publicadas ficam nos **Releases do repositório GitHub**
  (`https://github.com/ing01/exporta-xml`) — não existe servidor próprio de
  atualização, o GitHub (que já é público) serve esse papel de graça.
- Cada instalação verifica periodicamente (a cada 30 min, e também ao abrir o
  programa) se existe uma versão mais nova publicada; se existir, baixa e aplica
  sozinha, reiniciando o aplicativo automaticamente.
- O rodapé da tela principal ("Versão X.Y.Z") é a forma mais simples de confirmar
  visualmente que uma atualização foi aplicada.

## Pré-requisitos (só na primeira vez)

1. **Ferramenta `vpk`** (CLI do Velopack) instalada:
   ```bash
   dotnet tool install -g vpk
   ```
   Se já estiver instalada, `dotnet tool update -g vpk` traz a versão mais nova.

2. **Um Personal Access Token do GitHub**, só na hora de publicar (não precisa
   ficar salvo em lugar nenhum do projeto). Gere um em:
   `github.com → foto de perfil → Settings → Developer settings → Personal access
   tokens → Fine-grained tokens → Generate new token`
   - Repository access: **Only select repositories** → `ing01/exporta-xml`
   - Permissions → Repository permissions → **Contents: Read and write**
   - Expiração curta (7 dias, por exemplo) — o token só é usado no momento da
     publicação, pode revogar depois.

## Passo a passo de cada release (automatizado)

O script `release.ps1`, na raiz do repositório, faz os 4 passos abaixo de uma
vez só: sobe a versão no `.vbproj`, publica o build, empacota com o `vpk` e
publica no GitHub (perguntando o token na hora, sem precisar deixá-lo salvo
em lugar nenhum).

```powershell
.\release.ps1 -Version 1.0.9
```

- Confirma a versão antes de publicar de verdade no GitHub (pode responder
  "n" nessa hora pra só gerar os arquivos em `Releases\` sem publicar ainda).
- Recusa rodar se a versão informada já tiver sido publicada antes (evita o
  erro mais comum: reusar um número de versão).
- Use `-SkipUpload` pra só gerar o instalador/pacotes localmente, sem tentar
  publicar (útil pra revisar os arquivos antes, ou publicar depois manualmente
  com o comando `vpk upload github ...` que o próprio script mostra no final):
  ```powershell
  .\release.ps1 -Version 1.0.9 -SkipUpload
  ```
- Se preferir não digitar o token interativamente (ex.: rodando de um script
  maior), passe `-Token SEU_TOKEN_AQUI`.
- Pra publicar sem digitar o token TODA VEZ, crie o arquivo `.release-token`
  na raiz do repositório com o token numa única linha. Esse arquivo já está
  no `.gitignore` — nunca vai ser commitado — mas continua em texto puro no
  disco, então só faça isso numa máquina que só você usa. Se algum dia
  suspeitar que um token foi exposto (colado num chat, por exemplo), revogue
  na hora em GitHub → Settings → Developer settings → Personal access tokens
  e gere um novo.

O passo a passo manual abaixo continua válido (é o que o script automatiza) —
use-o se quiser rodar cada etapa na mão, ou se precisar depurar algum problema.

## Passo a passo de cada release (manual)

### 1. Suba a versão do projeto

Edite `ExportaXML/ExportaXML.vbproj` e aumente o número:

```xml
<Version>1.0.1</Version>
```

Use [versionamento semântico](https://semver.org/lang/pt-BR/) simples:
`MAJOR.MINOR.PATCH` (ex.: `1.0.0` → `1.0.1` para uma correção pequena, `1.1.0`
para uma funcionalidade nova, `2.0.0` para uma mudança que quebra compatibilidade).

**Nunca reutilize um número de versão já publicado** — o Velopack não sabe lidar
bem com isso.

### 2. Publique o build (self-contained)

```bash
dotnet publish ExportaXML/ExportaXML.vbproj -c Release -r win-x64 --self-contained -o publish
```

- `--self-contained`: inclui o runtime do .NET dentro do próprio pacote, então o
  cliente não precisa ter o .NET instalado separadamente.
- `-o publish`: pasta de saída (pode ser qualquer pasta temporária; não precisa
  versionar essa pasta no Git).

### 3. Empacote com o Velopack

```bash
vpk pack --packId ExportaXML --packVersion 1.0.1 --packDir publish --mainExe ExportaXML.exe --packAuthors "ing01" --packTitle "Exportador XML" -o Releases
```

- **`--packId ExportaXML`**: precisa ser **sempre exatamente esse texto**, em
  toda versão, para sempre — é como o Velopack identifica que essa versão
  pertence ao mesmo aplicativo instalado no cliente. Nunca mude isso.
- **`--packVersion`**: precisa bater com o `<Version>` do passo 1.
- **`-o Releases`**: pasta de saída com os arquivos prontos para publicar (não
  precisa versionar no Git).

Isso gera, entre outros arquivos, dentro da pasta `Releases`:
- `ExportaXML-win-Setup.exe` — instalador completo (usado só na primeira
  instalação de um cliente novo).
- `ExportaXML-{versão}-full.nupkg` — o pacote que os clientes já instalados
  baixam sozinhos para se atualizar.

### 4. Publique no GitHub

```bash
vpk upload github --repoUrl https://github.com/ing01/exporta-xml --outputDir Releases --token SEU_TOKEN_AQUI --publish --releaseName "v1.0.1" --tag "v1.0.1"
```

- `--publish`: publica o Release direto (sem isso, fica como rascunho/draft,
  invisível para as instalações existentes).
- `--tag`: use sempre o formato `vX.Y.Z` igual ao `<Version>`.

Pronto — a partir daqui, toda instalação existente do ExportaXML vai detectar essa
versão sozinha na próxima verificação.

## Instalação em um cliente novo

Para uma máquina que **nunca teve o ExportaXML instalado antes**:

1. **Instale o app**: envie o arquivo `ExportaXML-win-Setup.exe` (gerado no
   passo 3) e peça para executar uma vez (não precisa ser como administrador).
   Isso instala em `%LocalAppData%\ExportaXML\current\ExportaXML.exe` e já
   abre o app pela primeira vez. A partir daqui, essa instalação também passa
   a se atualizar sozinha em toda publicação futura.

   > O instalador ainda não é assinado digitalmente, então o Windows/SmartScreen
   > pode exibir um aviso de "Editor desconhecido" nessa primeira instalação —
   > isso não afeta as atualizações automáticas depois, só essa primeira execução.

2. **Configure pela tela** antes de instalar o serviço: conexão(ões) do
   banco, SMTP/e-mail, e o agendamento automático (horário e destinatário).
   Tudo fica salvo em `config.json`, ao lado do executável — o mesmo arquivo
   que o Windows Service (próximo passo) vai ler.

3. **Instale o Windows Service**, pra o agendamento mensal rodar mesmo sem
   ninguém logado. Num terminal na máquina do cliente:
   ```bash
   "%LocalAppData%\ExportaXML\current\ExportaXML.exe" --instalar-servico
   ```
   Aceite o prompt do UAC. Confirme em `services.msc` que "ExportaXML -
   Exportação Automática" aparece com Status "Em execução".

   > **Como isso convive com o auto-update**: o serviço NÃO roda direto de
   > dentro de `current\` (a pasta que o Velopack sobrescreve inteira em toda
   > atualização) — `--instalar-servico` copia os binários pra uma pasta
   > própria (`%ProgramData%\ExportaXML\Servico\`) e o serviço roda a partir
   > dali. Isso existe justamente para o auto-update do app interativo nunca
   > tentar sobrescrever um executável que o serviço está com o processo
   > aberto (o Windows não permite, e isso quebraria o auto-update). O efeito
   > colateral é que o serviço fica com uma "foto" congelada do código de
   > quando foi instalado — se uma versão nova mudar algo em
   > `AgendamentoService`/`ExportadorXML`/`EmailService` (a lógica que o
   > serviço executa), rode `--instalar-servico` de novo (depois que o app
   > interativo já tiver atualizado para a versão nova) pra recopiar os
   > binários atuais e recriar o serviço com o código novo.

## Ativando o Windows Service em clientes que já tinham o ExportaXML instalado

O Windows Service é uma funcionalidade nova — instalações existentes que
atualizam sozinhas via Velopack ganham o app interativo atualizado (com o
código do serviço já embutido no `.exe`), mas o serviço em si **não se
instala sozinho**. É um passo manual, único, por máquina:

```bash
"%LocalAppData%\ExportaXML\current\ExportaXML.exe" --instalar-servico
```

(mesmo comando do passo 3 de "Instalação em um cliente novo", acima — aceita
o UAC, cria e já inicia o serviço "ExportaXML - Exportação Automática",
copiando os binários pra `%ProgramData%\ExportaXML\Servico\`).

Rodar esse comando é seguro mesmo se o serviço já estiver instalado — ele
para, remove e recria do zero com os binários atuais. É assim, inclusive,
que se "atualiza" o serviço depois de uma versão nova que mexa em
`AgendamentoService`/`ExportadorXML`/`EmailService` (ver aviso no passo 3
acima sobre por que o serviço não se atualiza sozinho via Velopack).

```bash
"%LocalAppData%\ExportaXML\current\ExportaXML.exe" --desinstalar-servico
```
remove o serviço por completo (para, apaga o registro no SCM e a cópia em
`%ProgramData%\ExportaXML\Servico\`).

## Aviso de falha no agendamento (balão da bandeja + Log de Eventos)

Quando o envio agendado mensal falha — banco fora do ar, SMTP com problema,
etc. — o cliente agora fica sabendo mesmo sem precisar abrir o app:

- **Balão de aviso na bandeja** (`notifyIcon1.ShowBalloonTip`, ícone de
  alerta): aparece na próxima vez que o app interativo abrir OU no timer
  horário, mesmo com a janela minimizada/escondida — não precisa clicar em
  nada pra ver. Só aparece uma vez por falha (não repete a cada hora) e some
  sozinho quando o próximo agendamento rodar com sucesso.
- **Log de Eventos do Windows** (Visualizador de Eventos → Application,
  origem "ExportaXML"): registro adicional, útil se algum suporte técnico
  for investigar depois — funciona mesmo se ninguém estiver com o app aberto
  na bandeja (cenário em que o balão não tem como aparecer).
- O e-mail de alerta de falha (`EmailService.EnviarAlertaFalha`, campo
  "E-mail para alerta de falha" na tela) continua existindo como antes — os
  dois avisos acima são um reforço pro caso desse e-mail também não sair
  (ex.: o problema ser justamente no SMTP).

Não exige nenhuma configuração nova nem ação manual — passa a funcionar
sozinho assim que o cliente atualizar pra essa versão. A pendência fica
gravada em `%ProgramData%\ExportaXML\pendencia_agendamento.json` (lida tanto
pelo app interativo quanto, indiretamente, gravada pelo Windows Service —
ver `PendenciaAgendamentoService`).

## Como confirmar que uma atualização realmente chegou aos clientes

1. **Rodapé da tela principal** — o texto "Versão X.Y.Z" muda sozinho depois que
   o aplicativo detecta, baixa e aplica a atualização (o programa fecha e reabre
   sozinho nesse momento — é esperado, não é um erro).
2. **Log de atualização**, em `%LocalAppData%\ExportaXML\Logs\Atualizacao.log` na
   máquina do cliente — registra cada verificação, download e eventual erro.
3. **Página de Releases do GitHub**
   (`github.com/ing01/exporta-xml/releases`) — mostra qual é a versão mais
   recente publicada.
4. Botão **"Verificar Atualizações"**, ao lado do rodapé — força a checagem na
   hora, sem esperar os 30 min do timer automático (útil para testar).

## Recomendação antes de liberar para todos os clientes

Antes de confiar uma versão nova a todos os clientes de uma vez, é uma boa
prática instalar essa mesma versão numa máquina de teste (ou pedir para um
cliente "amigável" testar primeiro) e confirmar que tudo funciona normalmente
antes de considerar o release definitivo — principalmente porque o aplicativo já
lida com dados reais de produção e envia e-mails reais.

## Erros comuns

| Sintoma | Causa provável |
|---|---|
| Instalação não detecta a atualização | `--packId` diferente do usado nas versões anteriores, ou release ainda em rascunho (esqueceu `--publish`) |
| `vpk upload github` falha com erro de permissão | Token sem escopo "Contents: Read and write", ou expirado |
| Cliente recebe aviso de "Editor desconhecido" mesmo já tendo atualizado antes | Normal só na primeira instalação (Setup.exe); atualizações automáticas não mostram esse aviso |
| Log de atualização não aparece / não persiste entre versões | Verifique se algum código voltou a gravar logs dentro de `Application.StartupPath` em vez de `LogService.PastaLogs` — essa pasta é substituída a cada atualização |
| `--instalar-servico`/`--desinstalar-servico` não faz nada / fecha sem avisar | Provavelmente o UAC foi recusado/cancelado — o comando se relança elevado e só executa a ação de verdade depois de aceitar o prompt |
| Serviço "ExportaXML" continua rodando um comportamento antigo mesmo depois do app interativo já ter atualizado | Esperado — o serviço roda de uma cópia congelada em `%ProgramData%\ExportaXML\Servico\`, não da pasta `current\` que o Velopack atualiza. Rode `--instalar-servico` de novo (ver seção acima) |
