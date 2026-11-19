Imports System.Security.Cryptography

Public Class AuthService
    Private Const Iterations As Integer = 100000
    Private Const HashSize As Integer = 32
    Private Const SaltSize As Integer = 16

    Private ReadOnly _userRepo As IUserRepository

    Public Sub New(userRepo As IUserRepository)
        _userRepo = userRepo
    End Sub

    Public Sub CreateUser(username As String, password As String)
        If String.IsNullOrWhiteSpace(username) Then Throw New ArgumentException("Username is required.")
        If String.IsNullOrWhiteSpace(password) OrElse password.Length < 6 Then
            Throw New ArgumentException("Password must be at least 6 characters.")
        End If
        If _userRepo.GetByUsername(username.Trim()) IsNot Nothing Then
            Throw New ArgumentException("Username already exists.")
        End If

        Dim salt = RandomNumberGenerator.GetBytes(SaltSize)
        Dim hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize)

        Dim user = New User With {
            .Username = username.Trim(),
            .PasswordSalt = Convert.ToBase64String(salt),
            .PasswordHash = Convert.ToBase64String(hash)
        }
        _userRepo.Add(user)
    End Sub

    Public Function Authenticate(username As String, password As String) As User
        If String.IsNullOrWhiteSpace(username) OrElse password Is Nothing Then Return Nothing

        Dim user = _userRepo.GetByUsername(username.Trim())
        If user Is Nothing Then Return Nothing

        Dim salt = Convert.FromBase64String(user.PasswordSalt)
        Dim expected = Convert.FromBase64String(user.PasswordHash)
        Dim actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize)

        If CryptographicOperations.FixedTimeEquals(actual, expected) Then
            Return user
        End If

        Return Nothing
    End Function
End Class
