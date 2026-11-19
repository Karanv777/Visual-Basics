Public Class MaintenanceService
    Private ReadOnly _maintenanceRepo As IMaintenanceRepository
    Private ReadOnly _assetRepo As IAssetRepository

    Public Sub New(maintenanceRepo As IMaintenanceRepository, assetRepo As IAssetRepository)
        _maintenanceRepo = maintenanceRepo
        _assetRepo = assetRepo
    End Sub

    Public Function ScheduleMaintenance(assetId As Integer, description As String,
                                        scheduledDate As DateTime, technician As String) As MaintenanceRecord
        Dim asset = GetActiveAsset(assetId)
        If String.IsNullOrWhiteSpace(description) Then Throw New ArgumentException("Description is required.")
        If String.IsNullOrWhiteSpace(technician) Then Throw New ArgumentException("Technician is required.")

        Dim record = New MaintenanceRecord With {
            .AssetId = asset.AssetId,
            .Description = description.Trim(),
            .MaintenanceDate = scheduledDate,
            .Technician = technician.Trim(),
            .Cost = 0D,
            .IsCompleted = False
        }

        Dim created = _maintenanceRepo.Add(record)
        asset.Status = AssetStatus.UnderMaintenance
        _assetRepo.Update(asset)
        Return created
    End Function

    Public Function RecordCompletedMaintenance(assetId As Integer, description As String,
                                               completionDate As DateTime, cost As Decimal,
                                               technician As String) As MaintenanceRecord
        Dim asset = GetActiveAsset(assetId)
        If String.IsNullOrWhiteSpace(description) Then Throw New ArgumentException("Description is required.")
        If String.IsNullOrWhiteSpace(technician) Then Throw New ArgumentException("Technician is required.")
        If cost < 0 Then Throw New ArgumentException("Cost cannot be negative.")

        Dim record = New MaintenanceRecord With {
            .AssetId = asset.AssetId,
            .Description = description.Trim(),
            .MaintenanceDate = completionDate,
            .Cost = cost,
            .Technician = technician.Trim(),
            .IsCompleted = True
        }

        Dim created = _maintenanceRepo.Add(record)
        Dim hasPending = _maintenanceRepo.GetByAssetId(assetId).Any(Function(r) Not r.IsCompleted)
        asset.Status = If(hasPending, AssetStatus.UnderMaintenance, AssetStatus.Operational)
        _assetRepo.Update(asset)
        Return created
    End Function

    Public Sub CompleteScheduledMaintenance(maintenanceId As Integer, cost As Decimal, completionDate As DateTime)
        If cost < 0 Then Throw New ArgumentException("Cost cannot be negative.")

        Dim record = _maintenanceRepo.GetAll().FirstOrDefault(Function(r) r.MaintenanceId = maintenanceId)
        If record Is Nothing Then Throw New ArgumentException("Maintenance record not found.")
        If record.IsCompleted Then Throw New InvalidOperationException("Already completed.")

        record.Cost = cost
        record.MaintenanceDate = completionDate
        record.IsCompleted = True
        _maintenanceRepo.Update(record)

        Dim asset = _assetRepo.GetById(record.AssetId)
        If asset IsNot Nothing Then
            Dim stillPending = _maintenanceRepo.GetByAssetId(asset.AssetId).Any(Function(r) Not r.IsCompleted)
            asset.Status = If(stillPending, AssetStatus.UnderMaintenance, AssetStatus.Operational)
            _assetRepo.Update(asset)
        End If
    End Sub

    Public Function GetHistory(assetId As Integer) As List(Of MaintenanceRecord)
        Return _maintenanceRepo.GetByAssetId(assetId)
    End Function

    Public Function GetTotalMaintenanceCost() As Decimal
        Return _maintenanceRepo.GetAll().Where(Function(r) r.IsCompleted).Sum(Function(r) r.Cost)
    End Function

    Private Function GetActiveAsset(assetId As Integer) As Asset
        Dim asset = _assetRepo.GetById(assetId)
        If asset Is Nothing Then Throw New ArgumentException("Asset not found.")
        If asset.Status = AssetStatus.Retired Then Throw New InvalidOperationException("Cannot maintain a retired asset.")
        Return asset
    End Function
End Class
