Public Interface IUserRepository
    Function GetByUsername(username As String) As User
    Function AnyUserExists() As Boolean
    Sub Add(user As User)
End Interface
