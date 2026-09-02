Imports Velopack
Imports System.Diagnostics
Imports System.Linq
Imports System.Threading
Imports System.Security.Principal
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Hosting.WindowsServices
Imports System.Text

''' <summary>
''' Ponto de entrada real do aplicativo. Existe como Sub Main explícito (em vez
''' do "Application Framework" padrão do VB) só por causa do Velopack — ver
''' MyType=WindowsFormsWithCustomSubMain no .vbproj. Antes desta mudança, o
''' projeto usava My.Application/Application.myapp; foram removidos porque não
''' são compatíveis com um Sub Main customizado.
''' </summary>
Module EntryPoint

    Private Const ArgInstalarServico As String = "--instalar-servico"
    Private Const ArgDesinstalarServico As String = "--desinstalar-servico"

    ''' <summary>
    ''' Inicializa o Velopack e então segue por um de três caminhos: instalar/
    ''' remover o Windows Service (linha de comando), rodar como Windows
    ''' Service (sem UI, só o AgendamentoWorker em segundo plano), ou o fluxo
    ''' interativo de sempre (instância única + tela principal).
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
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        VelopackApp.Build().Run()

        Dim argumentos As String() = Environment.GetCommandLineArgs().Skip(1).ToArray()

        If argumentos.Contains(ArgInstalarServico, StringComparer.OrdinalIgnoreCase) Then
            ExecutarComandoServico(argumentos, Sub() ServicoWindowsService.Instalar())
            Return
        End If

        If argumentos.Contains(ArgDesinstalarServico, StringComparer.OrdinalIgnoreCase) Then
            ExecutarComandoServico(argumentos, Sub() ServicoWindowsService.Desinstalar())
            Return
        End If

        If WindowsServiceHelpers.IsWindowsService() Then
            RodarComoServico()
            Return
        End If

        Dim mutex As Mutex = Nothing
        Dim souDono As Boolean = False

        Try
            Dim criouNova As Boolean
            mutex = New Mutex(True, InstanciaUnica.NomeMutex, criouNova)
            souDono = criouNova

            If Not souDono Then
                ' O Mutex já existe — mas isso pode ser uma instância "de
                ' verdade" já aberta, OU o processo ANTERIOR ainda estar
                ' terminando de sair (ex.: durante o restart automático do
                ' atualizador — Application.Exit()/ApplyUpdatesAndRestart não
                ' garantem que o processo antigo já tenha liberado o Mutex no
                ' exato instante em que o novo processo já começou a rodar).
                ' Espera até 1s pra ver se o Mutex se libera antes de desistir
                ' e avisar a tal instância "existente" — sem essa espera, uma
                ' atualização podia fechar o app e o processo novo, iniciado
                ' em seguida pelo próprio atualizador, achava (erradamente)
                ' que já havia outra instância e saía sem abrir nada.
                Try
                    souDono = mutex.WaitOne(TimeSpan.FromSeconds(1))
                Catch
                    ' AbandonedMutexException: o dono anterior terminou sem
                    ' liberar (ex.: processo anterior encerrado abruptamente) —
                    ' a espera ainda assim nos torna donos do Mutex.
                    souDono = True
                End Try
            End If

            If Not souDono Then
                ' Registrado no log de Atividade de propósito: se isso disparar
                ' repetidamente sem NENHUMA janela/ícone de bandeja aparecer, é
                ' sinal de que a instância "existente" é, na verdade, um processo
                ' fantasma que não terminou de verdade (ver SairToolStripMenuItem_Click).
                LogService.RegistrarAtividade("Segunda tentativa de abrir o programa detectada - sinal enviado pra instância já em execução")
                InstanciaUnica.NotificarInstanciaExistente()
                Return
            End If

            Application.SetHighDpiMode(HighDpiMode.SystemAware)
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New FrmPrincipal())
        Finally
            If souDono AndAlso mutex IsNot Nothing Then
                Try
                    mutex.ReleaseMutex()
                Catch
                End Try
            End If

            mutex?.Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' Roda a ação de instalar/desinstalar o serviço, garantindo elevação
    ''' primeiro: se o processo atual não estiver rodando como administrador,
    ''' se relança com o verbo "runas" (dispara o UAC) passando os mesmos
    ''' argumentos, espera terminar, e sai — quem executa de fato é o
    ''' processo elevado, não este.
    ''' </summary>
    Private Sub ExecutarComandoServico(argumentos As String(), acao As Action)
        If Not EstaElevado() Then
            RelancarElevado(argumentos)
            Return
        End If

        Try
            acao()
            Console.WriteLine("Operação concluída com sucesso.")
        Catch ex As Exception
            Console.WriteLine($"Falha: {ex.Message}")
        End Try
    End Sub

    Private Function EstaElevado() As Boolean
        Using identidade = WindowsIdentity.GetCurrent()
            Return New WindowsPrincipal(identidade).IsInRole(WindowsBuiltInRole.Administrator)
        End Using
    End Function

    ''' <summary>
    ''' Relança o próprio executável elevado (via UAC), repassando os mesmos
    ''' argumentos, e espera terminar. Se o usuário cancelar o UAC, informa e
    ''' não faz mais nada — a operação simplesmente não é feita.
    ''' </summary>
    Private Sub RelancarElevado(argumentos As String())
        Dim caminhoExe As String = Process.GetCurrentProcess().MainModule.FileName

        Dim psi As New ProcessStartInfo(caminhoExe) With {
            .Arguments = String.Join(" ", argumentos),
            .UseShellExecute = True,
            .Verb = "runas"
        }

        Try
            Using processo = Process.Start(psi)
                processo.WaitForExit()
            End Using
        Catch ex As System.ComponentModel.Win32Exception
            Console.WriteLine("Operação cancelada: privilégios de administrador são necessários.")
        End Try
    End Sub

    ''' <summary>
    ''' Sobe o Generic Host em modo Windows Service, registrando o
    ''' AgendamentoWorker como hosted service. Bloqueia até o SCM parar o
    ''' serviço (Host.Run).
    ''' </summary>
    Private Sub RodarComoServico()
        Dim builder = Host.CreateApplicationBuilder()
        builder.Services.AddWindowsService(Sub(opcoes) opcoes.ServiceName = ServicoWindowsService.NomeServico)
        builder.Services.AddHostedService(Of AgendamentoWorker)()

        Using host = builder.Build()
            host.Run()
        End Using
    End Sub

End Module
