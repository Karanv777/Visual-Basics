Imports Microsoft.EntityFrameworkCore

Public Class MaintenanceRepository
    Implements IMaintenanceRepository

    Private ReadOnly _db As AppDbContext

    Public Sub New(db As AppDbContext)
        _db = db
    End Sub

    Public Function Add(record As MaintenanceRecord) As MaintenanceRecord Implements IMaintenanceRepository.Add
        _db.MaintenanceRecords.Add(record)
        _db.SaveChanges()
        Return record
    End Function

    Public Function GetByAssetId(assetId As Integer) As List(Of MaintenanceRecord) Implements IMaintenanceRepository.GetByAssetId
        Return _db.MaintenanceRecords.AsNoTracking().
            Where(Function(r) r.AssetId = assetId).
            OrderBy(Function(r) r.MaintenanceDate).ThenBy(Function(r) r.MaintenanceId).
            ToList()
    End Function

    Public Function GetAll() As List(Of MaintenanceRecord) Implements IMaintenanceRepository.GetAll
        Return _db.MaintenanceRecords.AsNoTracking().OrderBy(Function(r) r.MaintenanceId).ToList()
    End Function

    Public Sub Update(record As MaintenanceRecord) Implements IMaintenanceRepository.Update
        Dim tracked = _db.MaintenanceRecords.Local.FirstOrDefault(Function(r) r.MaintenanceId = record.MaintenanceId)
        If tracked IsNot Nothing AndAlso Not ReferenceEquals(tracked, record) Then
            _db.Entry(tracked).State = EntityState.Detached
        End If
        _db.MaintenanceRecords.Update(record)
        _db.SaveChanges()
    End Sub
End Class
