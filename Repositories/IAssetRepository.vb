Public Interface IAssetRepository
    Function Add(asset As Asset) As Asset
    Function GetById(assetId As Integer) As Asset
    Function GetByCode(assetCode As String) As Asset
    Function GetAll() As List(Of Asset)
    Sub Update(asset As Asset)
    Sub Delete(assetId As Integer)
End Interface
