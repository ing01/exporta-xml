Imports Npgsql

Public Class ConfiguracaoEmailService

    Public Shared Function BuscarConfiguracao(
        conn As NpgsqlConnection,
        codEmpresa As Integer,
        ByRef servidor As String,
        ByRef porta As Integer,
        ByRef usuario As String,
        ByRef senha As String,
        ByRef usarSSL As Boolean) As Boolean

        Dim sql As String
        If codEmpresa = 0 Then
            sql = "SELECT servidoremail, email, senhaemail, porta_smtp FROM conf_nfe WHERE codigo IS NULL OR codigo = 0 LIMIT 1"
        Else
            sql = "SELECT servidoremail, email, senhaemail, porta_smtp FROM conf_nfe WHERE codigo = @codigo LIMIT 1"
        End If

        Using cmd As New NpgsqlCommand(sql, conn)
            If codEmpresa <> 0 Then
                cmd.Parameters.AddWithValue("@codigo", codEmpresa)
            End If

            Using reader = cmd.ExecuteReader()
                If reader.Read() Then
                    servidor = If(IsDBNull(reader("servidoremail")), String.Empty, reader("servidoremail").ToString())
                    usuario = If(IsDBNull(reader("email")), String.Empty, reader("email").ToString())
                    Dim senhaCriptografada As String = If(IsDBNull(reader("senhaemail")), String.Empty, reader("senhaemail").ToString())
                    If String.IsNullOrEmpty(senhaCriptografada) Then
                        senha = String.Empty
                    Else
                        senha = CriptografiaHelper.Descriptografar(senhaCriptografada)
                    End If
                    If IsDBNull(reader("porta_smtp")) OrElse String.IsNullOrWhiteSpace(reader("porta_smtp").ToString()) Then
                        porta = 0
                    Else
                        Integer.TryParse(reader("porta_smtp").ToString(), porta)
                    End If
                    ' Habilita SSL/TLS para portas comuns (465 SSL, 587 STARTTLS). Se precisar de outro comportamento, ajustar aqui.
                    usarSSL = (porta = 465 OrElse porta = 587)
                    Return True

                End If
            End Using
        End Using

        Return False
    End Function
End Class