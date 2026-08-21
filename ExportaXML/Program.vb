Imports Velopack

''' <summary>
''' Ponto de entrada real do aplicativo. Existe como Sub Main explícito (em vez
''' do "Application Framework" padrão do VB) só por causa do Velopack — ver
''' MyType=WindowsFormsWithCustomSubMain no .vbproj. Antes desta mudança, o
''' projeto usava My.Application/Application.myapp; foram removidos porque não
''' são compatíveis com um Sub Main customizado.
''' </summary>
Module EntryPoint

    ''' <summary>
    ''' Inicializa o Velopack e abre a tela principal.
    ''' </summary>
    ''' <remarks>
    ''' <c>VelopackApp.Build().Run()</c> PRECISA ser a primeiríssima linha do
    ''' programa, antes de qualquer outra coisa (inclusive antes de
    ''' EnableVisualStyles) — é assim que o instalador/atualizador do Velopack
    ''' identifica os argumentos de linha de comando especiais que ele mesmo
    ''' invoca durante instalação, atualização e desinstalação (ex.:
    ''' --veloapp-install, --veloapp-updated). Se essa chamada for movida pra
    ''' depois de outra coisa, os hooks de instalação/atualização param de
    ''' funcionar silenciosamente.
    ''' </remarks>
    <STAThread()>
    Sub Main()
        VelopackApp.Build().Run()

        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New FrmPrincipal())
    End Sub

End Module
