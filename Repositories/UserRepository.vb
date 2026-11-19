Imports Microsoft.EntityFrameworkCore

Public Class UserRepository
    Implements IUserRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function GetByUsername(username As String) As User Implements IUserRepository.GetByUsername
        Return _db.Users.AsNoTracking().FirstOrDefault(Function(u) u.Username = username)
    End Function

    Public Function AnyUserExists() As Boolean Implements IUserRepository.AnyUserExists
        Return _db.Users.Any()
    End Function

    Public Sub Add(user As User) Implements IUserRepository.Add
        _db.Users.Add(user)
        _db.SaveChanges()
    End Sub
End Class
