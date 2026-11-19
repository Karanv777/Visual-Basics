Public Interface IMaintenanceRepository
    Function Add(record As MaintenanceRecord) As MaintenanceRecord
    Function GetByAssetId(assetId As Integer) As List(Of MaintenanceRecord)
    Function GetAll() As List(Of MaintenanceRecord)
    Sub Update(record As MaintenanceRecord)
End Interface
