Imports System.Text
Imports System.Security.Cryptography
Imports System.Linq

''' <summary>
''' Helper de criptografia compatível com o sistema legado (método antigo: soma/subtrai 22)
''' e com valores cifrados no formato OpenSSL (Salted__... base64).
''' </summary>
Public Class CriptografiaHelper

    ''' <summary>
    ''' Descriptografa uma string. Tenta, nesta ordem:
    ''' 1) método antigo (subtrai 22),
    ''' 2) OpenSSL salted (se for Base64 começando com 'Salted__') usando a chave definida em config,
    ''' 3) Base64 UTF8 simples,
    ''' 4) Hex.
    ''' Se nenhuma técnica produzir texto legível, retorna a tentativa antiga (fallback).
    ''' </summary>
    Public Shared Function Descriptografar(ByVal textoCriptografado As String) As String
        If String.IsNullOrEmpty(textoCriptografado) Then
            Return String.Empty
        End If

        ' 1) Se for Base64 e tiver prefixo OpenSSL (Salted__), tenta descriptografar primeiro (prioridade sobre o método antigo)
        If IsLikelyBase64(textoCriptografado) Then
            Try
                Dim cipherBytes = Convert.FromBase64String(textoCriptografado)
                If cipherBytes.Length > 16 Then
                    Dim header = Encoding.ASCII.GetString(cipherBytes, 0, 8)
                    If header = "Salted__" Then
                        Dim cfg = ConfiguracaoService.Carregar()
                        Dim pass As String = If(cfg IsNot Nothing, cfg.ChaveCriptografia, String.Empty)
                        ' Se não houver chave no config, tentar a passphrase conhecida usada pelo ERP (fallback)
                        Dim triedPasses As New List(Of String)()
                        If Not String.IsNullOrEmpty(pass) Then triedPasses.Add(pass)
                        triedPasses.Add("duesoft339")
                        For Each p In triedPasses
                            If String.IsNullOrEmpty(p) Then Continue For
                            Try
                                Dim dec = OpenSslDecrypt(textoCriptografado, p)
                                If Not String.IsNullOrEmpty(dec) AndAlso IsMostlyPrintable(dec) Then
                                    Return dec
                                End If
                            Catch
                            End Try
                        Next
                    End If
                End If
            Catch
            End Try

            ' se não for Salted__ ou descriptografia falhar, também testar Base64 UTF8 simples
            Try
                Dim bytes = Convert.FromBase64String(textoCriptografado)
                Dim s = Encoding.UTF8.GetString(bytes)
                If IsMostlyPrintable(s) Then
                    Return s
                End If
            Catch
            End Try
        End If

        ' 2) Tenta método antigo (subtrai 22)
        Dim tentativaAntiga As String = DescriptaAntigo(textoCriptografado)
        If IsMostlyPrintable(tentativaAntiga) Then
            Return tentativaAntiga
        End If

        ' 3) Se parecer hex, tenta decodificar
        If IsLikelyHex(textoCriptografado) Then
            Try
                Dim bytes = HexToBytes(textoCriptografado)
                Dim s = Encoding.UTF8.GetString(bytes)
                If IsMostlyPrintable(s) Then
                    Return s
                End If
            Catch
            End Try
        End If

        ' 4) Fallback: retorna a tentativa antiga (mesmo que contenha caracteres estranhos)
        Return tentativaAntiga
    End Function

    ''' <summary>
    ''' Descriptografa dados gerados pelo OpenSSL com passphrase (formato Salted__... base64).
    ''' Implementa o EVP_BytesToKey (MD5) usado pelo OpenSSL para derivar key+iv.
    ''' </summary>
    Private Shared Function OpenSslDecrypt(ByVal base64Text As String, ByVal passphrase As String) As String
        Dim cipherBytes = Convert.FromBase64String(base64Text)
        If cipherBytes.Length < 16 Then Return Nothing
        Dim header = Encoding.ASCII.GetString(cipherBytes, 0, 8)
        If header <> "Salted__" Then Return Nothing
        Dim salt(7) As Byte
        Array.Copy(cipherBytes, 8, salt, 0, 8)
        Dim cipherText = cipherBytes.Skip(16).ToArray()

        ' Deriva key+iv com PBKDF2-HMACSHA512 (10000 iterações) – compatível com o módulo do ERP
        Dim derived As Byte()
        Using pbkdf As New Rfc2898DeriveBytes(passphrase, salt, 10000, HashAlgorithmName.SHA512)
            derived = pbkdf.GetBytes(32 + 16)
        End Using
        Dim key = derived.Take(32).ToArray()
        Dim iv = derived.Skip(32).Take(16).ToArray()

        ' Decrypt usando AES-CTR (implementado via AES-ECB + XOR), compatível com a implementação do ERP
        Using aesAlg = Aes.Create()
            aesAlg.Mode = CipherMode.ECB
            aesAlg.Padding = PaddingMode.None
            aesAlg.Key = key
            Using encryptor = aesAlg.CreateEncryptor()
                Dim plain(cipherText.Length - 1) As Byte
                Dim counter(15) As Byte
                Array.Copy(iv, counter, Math.Min(16, iv.Length))
                Dim blocks = CInt(Math.Ceiling(cipherText.Length / 16.0))
                For i As Integer = 0 To blocks - 1
                    Dim keystream(15) As Byte
                    encryptor.TransformBlock(counter, 0, 16, keystream, 0)
                    Dim offset = i * 16
                    Dim chunkLen = Math.Min(16, cipherText.Length - offset)
                    For j As Integer = 0 To chunkLen - 1
                        plain(offset + j) = CByte((cipherText(offset + j) Xor keystream(j)) And &HFF)
                    Next
                    ' incrementa counter (big-endian)
                    For k As Integer = 15 To 0 Step -1
                        counter(k) = CByte((counter(k) + 1) And &HFF)
                        If counter(k) <> 0 Then Exit For
                    Next
                Next
                Return Encoding.UTF8.GetString(plain)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Criptografa uma string usando o método antigo (soma 22 a cada caractere).
    ''' </summary>
    Public Shared Function Criptografar(ByVal textoLimpo As String) As String
        Return CriptaAntigo(textoLimpo)
    End Function

    Private Shared Function CriptaAntigo(ByVal vSenha As String) As String
        Dim Y As String = ""
        For n As Integer = 1 To Len(vSenha)
            Y &= Chr(Asc(Mid(vSenha, n, 1)) + 22)
        Next
        Return Y
    End Function

    Private Shared Function DescriptaAntigo(ByVal vSenha As String) As String
        Dim Y As String = ""
        For n As Integer = 1 To Len(vSenha)
            Y &= Chr(Asc(Mid(vSenha, n, 1)) - 22)
        Next
        Return Y
    End Function

    Private Shared Function IsMostlyPrintable(ByVal s As String) As Boolean
        If String.IsNullOrEmpty(s) Then Return False
        Dim total = s.Length
        Dim printable = 0
        For Each ch As Char In s
            If AscW(ch) >= 32 AndAlso AscW(ch) <= 126 Then
                printable += 1
            End If
        Next
        Return (printable / total) >= 0.7
    End Function

    Private Shared Function IsLikelyBase64(ByVal s As String) As Boolean
        If String.IsNullOrEmpty(s) Then Return False
        ' Base64 básico: caracteres A-Z a-z 0-9 + / = e tamanho múltiplo de 4 (ou com padding)
        If s.Length Mod 4 <> 0 Then Return False
        For Each c As Char In s
            If Not (Char.IsLetterOrDigit(c) OrElse c = "+"c OrElse c = "/"c OrElse c = "="c) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Function IsLikelyHex(ByVal s As String) As Boolean
        If String.IsNullOrEmpty(s) Then Return False
        If s.Length Mod 2 <> 0 Then Return False
        For Each c As Char In s
            If Not ((c >= "0"c AndAlso c <= "9"c) OrElse (c >= "a"c AndAlso c <= "f"c) OrElse (c >= "A"c AndAlso c <= "F"c)) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Function HexToBytes(ByVal hex As String) As Byte()
        Dim bytes((hex.Length \ 2) - 1) As Byte
        For i As Integer = 0 To hex.Length - 1 Step 2
            bytes(i \ 2) = Convert.ToByte(hex.Substring(i, 2), 16)
        Next
        Return bytes
    End Function

End Class
