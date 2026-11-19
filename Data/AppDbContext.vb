Imports Microsoft.EntityFrameworkCore

Public Class AppDbContext
    Inherits DbContext

    Private ReadOnly _connectionString As String

    Public Sub New(connectionString As String)
        _connectionString = connectionString
    End Sub

    Public Property Assets As DbSet(Of Asset)
    Public Property MaintenanceRecords As DbSet(Of MaintenanceRecord)
    Public Property Users As DbSet(Of User)

    Protected Overrides Sub OnConfiguring(optionsBuilder As DbContextOptionsBuilder)
        If Not optionsBuilder.IsConfigured Then
            optionsBuilder.UseNpgsql(_connectionString)
        End If
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
        modelBuilder.Entity(Of Asset)(Sub(entity)
                                          entity.ToTable("assets")
                                          entity.HasKey(Function(a) a.AssetId)
                                          entity.Property(Function(a) a.AssetCode).HasMaxLength(20)
                                          entity.HasIndex(Function(a) a.AssetCode).IsUnique()
                                          entity.Property(Function(a) a.Name).IsRequired().HasMaxLength(200)
                                          entity.Property(Function(a) a.Category).IsRequired().HasMaxLength(100)
                                          entity.Property(Function(a) a.Manufacturer).IsRequired().HasMaxLength(100)
                                          entity.Property(Function(a) a.PurchaseDate).HasColumnType("timestamp without time zone")
                                          entity.Property(Function(a) a.PurchaseCost).HasPrecision(18, 2)
                                          entity.Property(Function(a) a.Status).HasConversion(Of String)().HasMaxLength(30)
                                      End Sub)

        modelBuilder.Entity(Of MaintenanceRecord)(Sub(entity)
                                                     entity.ToTable("maintenance_records")
                                                     entity.HasKey(Function(m) m.MaintenanceId)
                                                     entity.Property(Function(m) m.Description).IsRequired().HasMaxLength(500)
                                                     entity.Property(Function(m) m.Technician).IsRequired().HasMaxLength(100)
                                                     entity.Property(Function(m) m.MaintenanceDate).HasColumnType("timestamp without time zone")
                                                     entity.Property(Function(m) m.Cost).HasPrecision(18, 2)
                                                     entity.HasOne(Function(m) m.Asset).
                                                         WithMany(Function(a) a.MaintenanceRecords).
                                                         HasForeignKey(Function(m) m.AssetId).
                                                         OnDelete(DeleteBehavior.Cascade)
                                                 End Sub)

        modelBuilder.Entity(Of User)(Sub(entity)
                                         entity.ToTable("users")
                                         entity.HasKey(Function(u) u.UserId)
                                         entity.Property(Function(u) u.Username).IsRequired().HasMaxLength(100)
                                         entity.HasIndex(Function(u) u.Username).IsUnique()
                                         entity.Property(Function(u) u.PasswordHash).IsRequired()
                                         entity.Property(Function(u) u.PasswordSalt).IsRequired()
                                     End Sub)
    End Sub
End Class
