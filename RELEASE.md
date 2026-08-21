# Como publicar uma nova versão do ExportaXML

Este guia descreve o passo a passo para lançar uma nova versão do aplicativo depois
de alterar o código. Todas as instalações já existentes (dos clientes) detectam e
aplicam essa nova versão sozinhas, automaticamente, dentro de até 4 horas (ou
imediatamente, se o usuário abrir o programa depois da publicação) — não é preciso
visitar ou reinstalar nada manualmente em nenhum cliente.

Esse processo foi validado de ponta a ponta (instalação real + duas atualizações
reais em sequência) durante o desenvolvimento desta funcionalidade.

## Visão geral do mecanismo

- O aplicativo usa a biblioteca **Velopack** para se atualizar sozinho.
- As versões publicadas ficam nos **Releases do repositório GitHub**
  (`https://github.com/ing01/exporta-xml`) — não existe servidor próprio de
  atualização, o GitHub (que já é público) serve esse papel de graça.
- Cada instalação verifica periodicamente (a cada 4h, e também ao abrir o
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

## Passo a passo de cada release

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

Para uma máquina que **nunca teve o ExportaXML instalado antes**, envie o arquivo
`ExportaXML-win-Setup.exe` (gerado no passo 3) e peça para executar uma vez. A
partir daí, essa instalação também passa a se atualizar sozinha em toda
publicação futura.

> O instalador ainda não é assinado digitalmente, então o Windows/SmartScreen
> pode exibir um aviso de "Editor desconhecido" nessa primeira instalação — isso
> não afeta as atualizações automáticas depois, só essa primeira execução.

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
   hora, sem esperar as 4h do timer automático (útil para testar).

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
