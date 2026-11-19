Imports System
Imports System.IO
Imports System.Text

Module Program
    Private _db As AppDbContext
    Private _assetRepo As IAssetRepository
    Private _maintenanceRepo As IMaintenanceRepository
    Private _userRepo As IUserRepository
    Private _assetService As AssetService
    Private _maintenanceService As MaintenanceService
    Private _authService As AuthService
    Private _currentUser As User

    Sub Main()
        Try
            If Not InitializeDatabase() Then Return

            If Not AuthenticateUser() Then Return

            RunMainMenu()

        Catch ex As EndOfStreamException
            Console.WriteLine()
            Console.WriteLine("Input stream ended. Exiting AssetCare.")
        Catch ex As Exception
            Console.WriteLine()
            Console.WriteLine("Fatal error: " & ex.Message)
            Console.WriteLine("Check your PostgreSQL connection and try again.")
        End Try
    End Sub

    Private Function InitializeDatabase() As Boolean
        Dim connectionString = Environment.GetEnvironmentVariable("ASSETCARE_DB_CONNECTION")
        If String.IsNullOrWhiteSpace(connectionString) Then
            connectionString = "Host=localhost;Port=5432;Database=assetcare;Username=postgres;Password=karan"
        End If

        Console.WriteLine("Connecting to PostgreSQL...")
        _db = New AppDbContext(connectionString)

        Try
            _db.Database.EnsureCreated()
        Catch ex As Exception
            Console.WriteLine("Could not connect to or initialize the database.")
            Console.WriteLine("Reason: " & ex.Message)
            Console.WriteLine()
            Console.WriteLine("Set ASSETCARE_DB_CONNECTION to a valid PostgreSQL connection string, for example:")
            Console.WriteLine("  Host=localhost;Port=5432;Database=assetcare;Username=postgres;Password=postgres")
            Return False
        End Try

        _assetRepo = New AssetRepository(_db)
        _maintenanceRepo = New MaintenanceRepository(_db)
        _userRepo = New UserRepository(_db)

        _assetService = New AssetService(_assetRepo)
        _maintenanceService = New MaintenanceService(_maintenanceRepo, _assetRepo)
        _authService = New AuthService(_userRepo)

        Return True
    End Function

    Private Function AuthenticateUser() As Boolean
        ClearScreen()
        Console.WriteLine("================================")
        Console.WriteLine("       ASSETCARE LOGIN")
        Console.WriteLine("================================")
        Console.WriteLine()

        If Not _userRepo.AnyUserExists() Then
            Console.WriteLine("No user accounts found. Create the initial administrator account.")
            Console.WriteLine()
            CreateInitialUser()
            Console.WriteLine()
        End If

        While True
            Dim username = ReadNonEmptyString("Username: ")
            Dim password = ReadPassword("Password: ")

            Dim user = _authService.Authenticate(username, password)
            If user IsNot Nothing Then
                _currentUser = user
                Console.WriteLine()
                Console.WriteLine($"Welcome, {_currentUser.Username}.")
                Pause()
                Return True
            End If

            Console.WriteLine()
            Console.WriteLine("Invalid username or password. Try again.")
            Console.WriteLine()
        End While

        Return False
    End Function

    Private Sub CreateInitialUser()
        While True
            Try
                Dim username = ReadNonEmptyString("Choose admin username: ")
                Dim password = ReadPassword("Choose admin password (min 6 chars): ")
                Dim confirm = ReadPassword("Confirm password: ")

                If password <> confirm Then
                    Console.WriteLine("Passwords do not match. Try again.")
                    Console.WriteLine()
                    Continue While
                End If

                _authService.CreateUser(username, password)
                Console.WriteLine("Administrator account created.")
                Return
            Catch ex As EndOfStreamException
                Throw
            Catch ex As Exception
                Console.WriteLine("Error: " & ex.Message)
                Console.WriteLine()
            End Try
        End While
    End Sub

    Private Sub RunMainMenu()
        Dim running As Boolean = True

        While running
            ClearScreen()
            Console.WriteLine("================================")
            Console.WriteLine("       ASSETCARE MANAGEMENT")
            Console.WriteLine("================================")
            Console.WriteLine($"Logged in as: {_currentUser.Username}")
            Console.WriteLine("--------------------------------")
            Console.WriteLine("1. Register New Asset")
            Console.WriteLine("2. View All Assets")
            Console.WriteLine("3. Manage Maintenance")
            Console.WriteLine("4. Generate Reports")
            Console.WriteLine("5. Logout / Exit")
            Console.WriteLine("================================")
            Console.Write("Select an option: ")

            Select Case Ask().Trim()
                Case "1"
                    RegisterAsset()
                Case "2"
                    AssetMenu()
                Case "3"
                    MaintenanceMenu()
                Case "4"
                    ReportsMenu()
                Case "5"
                    running = False
                Case Else
                    Console.WriteLine("Invalid option. Press any key to try again.")
                    Ask()
            End Select
        End While
    End Sub

    Private Sub AssetMenu()
        Dim running As Boolean = True

        While running
            ClearScreen()
            Console.WriteLine("======== ASSET MANAGEMENT ========")
            Console.WriteLine("1. View All Assets")
            Console.WriteLine("2. View Asset Details")
            Console.WriteLine("3. Search Assets")
            Console.WriteLine("4. Filter by Status")
            Console.WriteLine("5. Update Asset")
            Console.WriteLine("6. Retire Asset")
            Console.WriteLine("7. Back to Main Menu")
            Console.Write("Select an option: ")

            Select Case Ask().Trim()
                Case "1"
                    PrintAssets(_assetService.GetAllAssets())
                    Pause()
                Case "2"
                    ShowAssetDetails()
                Case "3"
                    SearchAssets()
                Case "4"
                    FilterByStatus()
                Case "5"
                    UpdateAsset()
                Case "6"
                    RetireAsset()
                Case "7"
                    running = False
                Case Else
                    Console.WriteLine("Invalid option. Press any key to try again.")
                    Ask()
            End Select
        End While
    End Sub

    Private Sub MaintenanceMenu()
        Dim running As Boolean = True

        While running
            ClearScreen()
            Console.WriteLine("======= MAINTENANCE =======")
            Console.WriteLine("1. Schedule Maintenance")
            Console.WriteLine("2. Complete Scheduled Maintenance")
            Console.WriteLine("3. Record Completed Maintenance")
            Console.WriteLine("4. View Maintenance History for Asset")
            Console.WriteLine("5. Back to Main Menu")
            Console.Write("Select an option: ")

            Select Case Ask().Trim()
                Case "1"
                    ScheduleMaintenance()
                Case "2"
                    CompleteScheduledMaintenance()
                Case "3"
                    RecordCompletedMaintenance()
                Case "4"
                    ShowMaintenanceHistory()
                Case "5"
                    running = False
                Case Else
                    Console.WriteLine("Invalid option. Press any key to try again.")
                    Ask()
            End Select
        End While
    End Sub

    Private Sub ReportsMenu()
        Dim running As Boolean = True

        While running
            ClearScreen()
            Console.WriteLine("======== REPORTS ========")
            Console.WriteLine("1. Asset Summary")
            Console.WriteLine("2. Total Maintenance Expenditure")
            Console.WriteLine("3. Maintenance History by Asset")
            Console.WriteLine("4. Back to Main Menu")
            Console.Write("Select an option: ")

            Select Case Ask().Trim()
                Case "1"
                    ShowAssetSummary()
                Case "2"
                    Console.WriteLine()
                    Console.WriteLine($"Total maintenance expenditure: {_maintenanceService.GetTotalMaintenanceCost():C}")
                    Pause()
                Case "3"
                    ShowMaintenanceHistory()
                Case "4"
                    running = False
                Case Else
                    Console.WriteLine("Invalid option. Press any key to try again.")
                    Ask()
            End Select
        End While
    End Sub

    Private Sub RegisterAsset()
        ClearScreen()
        Console.WriteLine("=== Register New Asset ===")

        Try
            Dim name = ReadNonEmptyString("Name: ")
            Dim category = ReadNonEmptyString("Category: ")
            Dim manufacturer = ReadNonEmptyString("Manufacturer: ")
            Dim purchaseDate = ReadDate("Purchase date (yyyy-mm-dd): ")
            Dim purchaseCost = ReadDecimal("Purchase cost: ")

            Dim asset = _assetService.RegisterAsset(name, category, manufacturer, purchaseDate, purchaseCost)
            Console.WriteLine($"Asset registered: {asset.AssetCode} (ID: {asset.AssetId})")
        Catch ex As EndOfStreamException
            Throw
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try

        Pause()
    End Sub

    Private Sub PrintAssets(assets As List(Of Asset))
        ClearScreen()

        If assets.Count = 0 Then
            Console.WriteLine("No assets found.")
            Return
        End If

        Console.WriteLine($"{"ID",-4} {"Code",-9} {"Name",-25} {"Category",-15} {"Status",-16} {"Cost",10}")
        Console.WriteLine(New String("-"c, 80))

        For Each asset In assets
            Console.WriteLine($"{asset.AssetId,-4} {asset.AssetCode,-9} {Truncate(asset.Name, 25),-25} {Truncate(asset.Category, 15),-15} {asset.Status,-16} {asset.PurchaseCost,10:C}")
        Next
    End Sub

    Private Sub ShowAssetDetails()
        Dim id = ReadInt("Enter asset ID: ")
        Dim asset = _assetService.GetAssetById(id)

        If asset Is Nothing Then
            Console.WriteLine("Asset not found.")
        Else
            Console.WriteLine($"ID:            {asset.AssetId}")
            Console.WriteLine($"Code:          {asset.AssetCode}")
            Console.WriteLine($"Name:          {asset.Name}")
            Console.WriteLine($"Category:      {asset.Category}")
            Console.WriteLine($"Manufacturer:  {asset.Manufacturer}")
            Console.WriteLine($"Purchase date: {asset.PurchaseDate:yyyy-MM-dd}")
            Console.WriteLine($"Purchase cost: {asset.PurchaseCost:C}")
            Console.WriteLine($"Status:        {asset.Status}")
        End If

        Pause()
    End Sub

    Private Sub SearchAssets()
        Dim term = ReadNonEmptyString("Search (name, code, category, manufacturer): ")
        PrintAssets(_assetService.SearchAssets(term))
        Pause()
    End Sub

    Private Sub FilterByStatus()
        Console.WriteLine("1. Operational  2. UnderMaintenance  3. Retired")
        Dim choice = Ask().Trim()
        Dim status As AssetStatus

        Select Case choice
            Case "1"
                status = AssetStatus.Operational
            Case "2"
                status = AssetStatus.UnderMaintenance
            Case "3"
                status = AssetStatus.Retired
            Case Else
                Console.WriteLine("Invalid choice.")
                Pause()
                Return
        End Select

        PrintAssets(_assetService.FilterByStatus(status))
        Pause()
    End Sub

    Private Sub UpdateAsset()
        Dim id = ReadInt("Enter asset ID to update: ")
        Dim asset = _assetService.GetAssetById(id)

        If asset Is Nothing Then
            Console.WriteLine("Asset not found.")
            Pause()
            Return
        End If

        Console.WriteLine("Leave blank to keep the current value.")

        Dim name = ReadOptional($"Name ({asset.Name}): ", asset.Name)
        Dim category = ReadOptional($"Category ({asset.Category}): ", asset.Category)
        Dim manufacturer = ReadOptional($"Manufacturer ({asset.Manufacturer}): ", asset.Manufacturer)

        asset.Name = name
        asset.Category = category
        asset.Manufacturer = manufacturer

        Try
            _assetService.UpdateAsset(asset)
            Console.WriteLine("Asset updated.")
        Catch ex As EndOfStreamException
            Throw
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try

        Pause()
    End Sub

    Private Sub RetireAsset()
        Dim id = ReadInt("Enter asset ID to retire: ")

        Try
            _assetService.RetireAsset(id)
            Console.WriteLine("Asset retired.")
        Catch ex As EndOfStreamException
            Throw
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try

        Pause()
    End Sub

    Private Sub ScheduleMaintenance()
        Try
            Dim assetId = ReadInt("Asset ID: ")
            Dim description = ReadNonEmptyString("Description: ")
            Dim date_ = ReadDate("Scheduled date (yyyy-mm-dd): ")
            Dim technician = ReadNonEmptyString("Technician: ")

            Dim record = _maintenanceService.ScheduleMaintenance(assetId, description, date_, technician)
            Console.WriteLine($"Maintenance scheduled (ID: {record.MaintenanceId}).")
        Catch ex As EndOfStreamException
            Throw
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try

        Pause()
    End Sub

    Private Sub CompleteScheduledMaintenance()
        Try
            Dim maintenanceId = ReadInt("Maintenance record ID: ")
            Dim cost = ReadDecimal("Final cost: ")
            Dim completionDate = ReadDate("Completion date (yyyy-mm-dd): ")

            _maintenanceService.CompleteScheduledMaintenance(maintenanceId, cost, completionDate)
            Console.WriteLine("Maintenance marked as completed.")
        Catch ex As EndOfStreamException
            Throw
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try

        Pause()
    End Sub

    Private Sub RecordCompletedMaintenance()
        Try
            Dim assetId = ReadInt("Asset ID: ")
            Dim description = ReadNonEmptyString("Description: ")
            Dim completionDate = ReadDate("Completion date (yyyy-mm-dd): ")
            Dim cost = ReadDecimal("Cost: ")
            Dim technician = ReadNonEmptyString("Technician: ")

            Dim record = _maintenanceService.RecordCompletedMaintenance(assetId, description, completionDate, cost, technician)
            Console.WriteLine($"Maintenance recorded (ID: {record.MaintenanceId}).")
        Catch ex As EndOfStreamException
            Throw
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try

        Pause()
    End Sub

    Private Sub ShowMaintenanceHistory()
        Dim assetId = ReadInt("Asset ID: ")
        Dim records = _maintenanceService.GetHistory(assetId)

        ClearScreen()

        If records.Count = 0 Then
            Console.WriteLine("No maintenance records for this asset.")
            Pause()
            Return
        End If

        Console.WriteLine($"{"ID",-4} {"Date",-12} {"Cost",10} {"Technician",-15} {"Status",-10} Description")
        Console.WriteLine(New String("-"c, 78))

        For Each record In records
            Dim status = If(record.IsCompleted, "Completed", "Scheduled")
            Console.WriteLine($"{record.MaintenanceId,-4} {record.MaintenanceDate:yyyy-MM-dd} {record.Cost,10:C} {Truncate(record.Technician, 15),-15} {status,-10} {record.Description}")
        Next

        Console.WriteLine()
        Console.WriteLine($"Total cost: {records.Where(Function(r) r.IsCompleted).Sum(Function(r) r.Cost):C}")
        Pause()
    End Sub

    Private Sub ShowAssetSummary()
        Dim assets = _assetService.GetAllAssets()

        ClearScreen()
        Console.WriteLine("=== Asset Summary ===")
        Console.WriteLine($"Total registered assets: {assets.Count}")
        Console.WriteLine($"Operational:             {assets.Where(Function(a) a.Status = AssetStatus.Operational).Count()}")
        Console.WriteLine($"Under maintenance:       {assets.Where(Function(a) a.Status = AssetStatus.UnderMaintenance).Count()}")
        Console.WriteLine($"Retired:                 {assets.Where(Function(a) a.Status = AssetStatus.Retired).Count()}")
        Pause()
    End Sub

    Private Function ReadNonEmptyString(prompt As String) As String
        While True
            Console.Write(prompt)
            Dim input = Ask().Trim()
            If Not String.IsNullOrWhiteSpace(input) Then Return input
            Console.WriteLine("A value is required.")
        End While

        Return String.Empty
    End Function

    Private Function ReadOptional(prompt As String, fallback As String) As String
        Console.Write(prompt)
        Dim input = Ask().Trim()
        Return If(String.IsNullOrWhiteSpace(input), fallback, input)
    End Function

    Private Function ReadInt(prompt As String) As Integer
        While True
            Console.Write(prompt)
            Dim value As Integer
            If Integer.TryParse(Ask().Trim(), value) AndAlso value > 0 Then Return value
            Console.WriteLine("Enter a positive whole number.")
        End While

        Return 0
    End Function

    Private Function ReadDecimal(prompt As String) As Decimal
        While True
            Console.Write(prompt)
            Dim value As Decimal
            If Decimal.TryParse(Ask().Trim(), value) AndAlso value >= 0D Then Return value
            Console.WriteLine("Enter a valid non-negative amount.")
        End While

        Return 0D
    End Function

    Private Function ReadDate(prompt As String) As DateTime
        While True
            Console.Write(prompt)
            Dim value As DateTime
            If DateTime.TryParse(Ask().Trim(), value) Then Return value
            Console.WriteLine("Enter a valid date (yyyy-mm-dd).")
        End While

        Return DateTime.MinValue
    End Function

    Private Function ReadPassword(prompt As String) As String
        Console.Write(prompt)

        If Console.IsInputRedirected Then
            Return Ask().Trim()
        End If

        Dim password As New StringBuilder()
        While True
            Dim key = Console.ReadKey(True)

            If key.Key = ConsoleKey.Enter Then
                Console.WriteLine()
                Return password.ToString()
            End If

            If key.Key = ConsoleKey.Backspace Then
                If password.Length > 0 Then password.Length -= 1
                Continue While
            End If

            If Not Char.IsControl(key.KeyChar) Then
                password.Append(key.KeyChar)
            End If
        End While

        Return String.Empty
    End Function

    Private Function Ask() As String
        Dim line = Console.ReadLine()
        If line Is Nothing Then Throw New EndOfStreamException("End of input stream.")
        Return line
    End Function

    Private Sub ClearScreen()
        If Not Console.IsOutputRedirected Then Console.Clear()
    End Sub

    Private Function Truncate(value As String, length As Integer) As String
        If String.IsNullOrEmpty(value) Then Return String.Empty
        Return If(value.Length <= length, value, value.Substring(0, length))
    End Function

    Private Sub Pause()
        Console.WriteLine()
        Console.Write("Press any key to continue...")
        Ask()
    End Sub
End Module
