Public Class AssetService
    Private ReadOnly _repo As IAssetRepository

    Public Sub New(repo As IAssetRepository)
        _repo = repo
    End Sub

    Public Function RegisterAsset(name As String, category As String, manufacturer As String,
                                  purchaseDate As DateTime, purchaseCost As Decimal) As Asset
        If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Name is required.")
        If String.IsNullOrWhiteSpace(category) Then Throw New ArgumentException("Category is required.")
        If String.IsNullOrWhiteSpace(manufacturer) Then Throw New ArgumentException("Manufacturer is required.")
        If purchaseCost < 0 Then Throw New ArgumentException("Purchase cost cannot be negative.")

        Dim asset = New Asset With {
            .Name = name.Trim(),
            .Category = category.Trim(),
            .Manufacturer = manufacturer.Trim(),
            .PurchaseDate = purchaseDate,
            .PurchaseCost = purchaseCost,
            .Status = AssetStatus.Operational
        }
        Return _repo.Add(asset)
    End Function

    Public Function GetAllAssets() As List(Of Asset)
        Return _repo.GetAll()
    End Function

    Public Function GetAssetById(assetId As Integer) As Asset
        Return _repo.GetById(assetId)
    End Function

    Public Function SearchAssets(term As String) As List(Of Asset)
        If String.IsNullOrWhiteSpace(term) Then Return _repo.GetAll()
        Dim t = term.Trim().ToLowerInvariant()
        Return _repo.GetAll().Where(Function(a)
                                        Return a.Name.ToLowerInvariant().Contains(t) OrElse
                                               a.Category.ToLowerInvariant().Contains(t) OrElse
                                               a.Manufacturer.ToLowerInvariant().Contains(t) OrElse
                                               a.AssetCode.ToLowerInvariant().Contains(t)
                                    End Function).ToList()
    End Function

    Public Function FilterByStatus(status As AssetStatus) As List(Of Asset)
        Return _repo.GetAll().Where(Function(a) a.Status = status).ToList()
    End Function

    Public Sub UpdateAsset(asset As Asset)
        If String.IsNullOrWhiteSpace(asset.Name) OrElse String.IsNullOrWhiteSpace(asset.Category) OrElse
           String.IsNullOrWhiteSpace(asset.Manufacturer) Then
            Throw New ArgumentException("Name, category, and manufacturer are required.")
        End If
        If asset.PurchaseCost < 0 Then Throw New ArgumentException("Purchase cost cannot be negative.")
        _repo.Update(asset)
    End Sub

    Public Sub RetireAsset(assetId As Integer)
        Dim asset = _repo.GetById(assetId)
        If asset Is Nothing Then Throw New ArgumentException("Asset not found.")
        If asset.Status = AssetStatus.Retired Then Throw New InvalidOperationException("Asset already retired.")
        asset.Status = AssetStatus.Retired
        _repo.Update(asset)
    End Sub
End Class
