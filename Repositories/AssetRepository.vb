Imports Microsoft.EntityFrameworkCore

Public Class AssetRepository
    Implements IAssetRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function Add(asset As Asset) As Asset Implements IAssetRepository.Add
        _db.Assets.Add(asset)
        _db.SaveChanges()

        asset.AssetCode = $"AST-{asset.AssetId:D4}"
        _db.SaveChanges()
        Return asset
    End Function

    Public Function GetById(assetId As Integer) As Asset Implements IAssetRepository.GetById
        Return _db.Assets.FirstOrDefault(Function(a) a.AssetId = assetId)
    End Function

    Public Function GetByCode(assetCode As String) As Asset Implements IAssetRepository.GetByCode
        Return _db.Assets.FirstOrDefault(Function(a) a.AssetCode = assetCode)
    End Function

    Public Function GetAll() As List(Of Asset) Implements IAssetRepository.GetAll
        Return _db.Assets.AsNoTracking().OrderBy(Function(a) a.AssetId).ToList()
    End Function

    Public Sub Update(asset As Asset) Implements IAssetRepository.Update
        Dim tracked = _db.Assets.Local.FirstOrDefault(Function(a) a.AssetId = asset.AssetId)
        If tracked IsNot Nothing AndAlso Not ReferenceEquals(tracked, asset) Then
            _db.Entry(tracked).State = EntityState.Detached
        End If
        _db.Assets.Update(asset)
        _db.SaveChanges()
    End Sub

    Public Sub Delete(assetId As Integer) Implements IAssetRepository.Delete
        Dim asset = _db.Assets.Find(assetId)
        If asset IsNot Nothing Then
            _db.Assets.Remove(asset)
            _db.SaveChanges()
        End If
    End Sub
End Class
