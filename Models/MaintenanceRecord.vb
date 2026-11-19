Public Class MaintenanceRecord
    Public Property MaintenanceId As Integer
    Public Property AssetId As Integer
    Public Overridable Property Asset As Asset
    Public Property Description As String = String.Empty
    Public Property MaintenanceDate As DateTime
    Public Property Cost As Decimal
    Public Property Technician As String = String.Empty
    Public Property IsCompleted As Boolean
End Class
