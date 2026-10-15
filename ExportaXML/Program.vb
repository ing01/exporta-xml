Imports Velopack
Imports System.Diagnostics
Imports System.Linq
Imports System.Threading
Imports System.Security.Principal
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Hosting.WindowsServices
Imports System.Text
Imports System
Imports System.Windows.Forms

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
                Try
                    souDono = mutex.WaitOne(TimeSpan.FromSeconds(1))
                Catch
                    souDono = True
                End Try
            End If

            If Not souDono Then
                LogService.RegistrarAtividade("Segunda tentativa de abrir o programa detectada - sinal enviado pra instância já em execução")
                InstanciaUnica.NotificarInstanciaExistente()
                Return
            End If

            ' ✅ CORREÇÃO DO DPI – ANTES de qualquer UI
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

    Private Sub ExecutarComandoServico(argumentos As String(), acao As Action)
        If Not EstaElevado() Then
            RelancarElevado(argumentos)
            Return
        End If

        Try
            acao()
            Console.WriteLine("Operação concluída com sucesso.")
        Catch ex As Exception
            Console.WriteLine($"Falha: {ErroAmigavel.Traduzir(ex)}")
        End Try
    End Sub

    Private Function EstaElevado() As Boolean
        Using identidade = WindowsIdentity.GetCurrent()
            Return New WindowsPrincipal(identidade).IsInRole(WindowsBuiltInRole.Administrator)
        End Using
    End Function

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

    Private Sub RodarComoServico()
        Dim builder = Host.CreateApplicationBuilder()
        builder.Services.AddWindowsService(Sub(opcoes) opcoes.ServiceName = ServicoWindowsService.NomeServico)
        builder.Services.AddHostedService(Of AgendamentoWorker)()

        Using host = builder.Build()
            host.Run()
        End Using
    End Sub
End Module