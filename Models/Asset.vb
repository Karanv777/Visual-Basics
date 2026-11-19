Public Class Asset
    Public Property AssetId As Integer
    Public Property AssetCode As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Category As String = String.Empty
    Public Property Manufacturer As String = String.Empty
    Public Property PurchaseDate As DateTime
    Public Property PurchaseCost As Decimal
    Public Property Status As AssetStatus
    Public Overridable Property MaintenanceRecords As ICollection(Of MaintenanceRecord) = New List(Of MaintenanceRecord)()
End Class
