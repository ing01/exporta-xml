<#
.SYNOPSIS
    Automatiza a publicacao de uma nova versao do ExportaXML (ver RELEASE.md):
    atualiza a versao no .vbproj, publica o build self-contained, empacota
    com o Velopack (vpk) e, por padrao, ja publica o Release no GitHub.

.PARAMETER Version
    Novo numero de versao (MAJOR.MINOR.PATCH, ex.: 1.0.9). Precisa ser maior
    que qualquer versao ja publicada em Releases\RELEASES.

.PARAMETER Token
    Personal Access Token do GitHub (permissao "Contents: Read and write"),
    usado so na hora de publicar. Ordem de resolucao se omitido: 1) arquivo
    ".release-token" na raiz do repo (uma linha, so o token - ver NOTES);
    2) pergunta interativa (digitacao mascarada).

.NOTES
    Para publicar sem digitar o token toda vez, crie o arquivo
    ".release-token" na raiz do repo com o token numa unica linha. Esse
    arquivo esta no .gitignore - NUNCA sera commitado - mas ainda fica em
    texto puro no disco, entao qualquer pessoa/programa com acesso a essa
    pasta consegue ler o token. So use essa opcao numa maquina que so voce
    usa. Revogue e gere um token novo se algum dia suspeitar que ele foi
    exposto (ex.: colado em algum chat, enviado por e-mail, etc.).

.PARAMETER SkipUpload
    Gera os pacotes em Releases\ mas NAO publica no GitHub - util para
    revisar os arquivos antes de decidir publicar, ou para publicar depois
    manualmente com "vpk upload github ...".

.EXAMPLE
    .\release.ps1 -Version 1.0.9
    Pede o token interativamente e publica direto no GitHub.

.EXAMPLE
    .\release.ps1 -Version 1.0.9 -SkipUpload
    So gera o instalador e os pacotes de atualizacao em Releases\, sem publicar.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$Token,

    [switch]$SkipUpload
)

$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$vbproj = Join-Path $repoRoot "ExportaXML\ExportaXML.vbproj"
$publishDir = Join-Path $repoRoot "publish"
$releasesDir = Join-Path $repoRoot "Releases"
$releasesManifest = Join-Path $releasesDir "RELEASES"

function Etapa($texto) {
    Write-Host ""
    Write-Host "==> $texto" -ForegroundColor Cyan
}

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Versao invalida: '$Version'. Use o formato MAJOR.MINOR.PATCH, ex.: 1.0.9"
}

if (Test-Path $releasesManifest) {
    $jaPublicada = Select-String -Path $releasesManifest -Pattern "ExportaXML-$Version-full\.nupkg" -Quiet
    if ($jaPublicada) {
        throw "A versao $Version ja aparece em $releasesManifest. Nunca reutilize um numero de versao ja publicado - escolha outra."
    }
}

Etapa "Atualizando a versao para $Version em $vbproj"
$tagAntiga = [regex]::Match((Get-Content $vbproj -Raw), '<Version>[\d\.]+</Version>').Value
if (-not $tagAntiga) {
    throw "Nao encontrei a tag de versao em $vbproj para atualizar."
}
$conteudoOriginal = Get-Content $vbproj -Raw
$tagNova = "<Version>$Version</Version>"
$conteudoNovo = $conteudoOriginal.Replace($tagAntiga, $tagNova)
Set-Content -Path $vbproj -Value $conteudoNovo -NoNewline -Encoding utf8

if (Test-Path $publishDir) {
    Etapa "Limpando pasta publish antiga"
    Remove-Item $publishDir -Recurse -Force
}

$objDir = Join-Path (Split-Path $vbproj -Parent) "obj"
if (Test-Path $objDir) {
    Etapa "Limpando pasta obj antiga (evita erro NETSDK1047 de restauracao sem win-x64)"
    Remove-Item $objDir -Recurse -Force
}

Etapa "Restaurando pacotes para win-x64"
dotnet restore $vbproj -r win-x64
if ($LASTEXITCODE -ne 0) { throw "dotnet restore falhou (exit code $LASTEXITCODE)." }

Etapa "Publicando build self-contained (dotnet publish)"
dotnet publish $vbproj -c Release -r win-x64 --self-contained --no-restore -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish falhou (exit code $LASTEXITCODE)." }

Etapa "Empacotando com o Velopack (vpk pack)"
# Uma pequena pausa aqui evita um erro transiente comum (antivirus ainda
# escaneando os arquivos que o dotnet publish acabou de escrever, fazendo o
# vpk falhar com "arquivo nao encontrado" numa copia interna dele mesmo).
Start-Sleep -Seconds 3

$tentativasRestantes = 2
do {
    vpk pack --packId ExportaXML --packVersion $Version --packDir $publishDir --mainExe ExportaXML.exe --packAuthors "ing01" --packTitle "Exportador XML" -o $releasesDir
    $tentativasRestantes--

    if ($LASTEXITCODE -ne 0 -and $tentativasRestantes -gt 0) {
        Write-Host "vpk pack falhou, tentando novamente em 5s (pode ser antivirus escaneando os arquivos)..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
} while ($LASTEXITCODE -ne 0 -and $tentativasRestantes -gt 0)

if ($LASTEXITCODE -ne 0) { throw "vpk pack falhou (exit code $LASTEXITCODE)." }

Write-Host ""
Write-Host "==> Pacotes gerados em $releasesDir (Setup.exe, .nupkg, delta, etc.)" -ForegroundColor Green

if ($SkipUpload) {
    Write-Host ""
    Write-Host "-SkipUpload informado: NADA foi publicado no GitHub." -ForegroundColor Yellow
    Write-Host "Quando quiser publicar, rode:"
    Write-Host "  vpk upload github --repoUrl https://github.com/ing01/exporta-xml --outputDir Releases --token SEU_TOKEN --publish --releaseName ""v$Version"" --tag ""v$Version"""
    exit 0
}

$arquivoToken = Join-Path $repoRoot ".release-token"
if (-not (Test-Path $arquivoToken)) {
    $arquivoToken = Join-Path $repoRoot ".release-token.txt"
}

if (-not $Token -and (Test-Path $arquivoToken)) {
    $Token = (Get-Content $arquivoToken -Raw).Trim()
    if ($Token) {
        Write-Host "Token lido de $arquivoToken" -ForegroundColor DarkGray
    }
}

if (-not $Token) {
    $tokenSeguro = Read-Host "Personal Access Token do GitHub (Contents: Read and write)" -AsSecureString
    $Token = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [System.Runtime.InteropServices.Marshal]::SecureStringToGlobalAllocUnicode($tokenSeguro))
}

if ([string]::IsNullOrWhiteSpace($Token)) {
    throw "Nenhum token informado - publicacao cancelada. Use -SkipUpload pra so gerar os arquivos sem publicar."
}

$confirmacao = Read-Host "Confirma publicar a versao $Version no GitHub Releases agora? (s/N)"
if ($confirmacao -ne "s" -and $confirmacao -ne "S") {
    Write-Host "Publicacao cancelada pelo usuario. Os arquivos continuam disponiveis em $releasesDir." -ForegroundColor Yellow
    exit 0
}

Etapa "Publicando no GitHub Releases (v$Version)"
vpk upload github --repoUrl https://github.com/ing01/exporta-xml --outputDir $releasesDir --token $Token --publish --releaseName "v$Version" --tag "v$Version"
if ($LASTEXITCODE -ne 0) { throw "vpk upload falhou (exit code $LASTEXITCODE)." }

Write-Host ""
Write-Host "==> Release v$Version publicado com sucesso!" -ForegroundColor Green
Write-Host "    https://github.com/ing01/exporta-xml/releases/tag/v$Version"
Write-Host "    Instalacoes existentes detectam essa versao sozinhas em ate 30 min."
