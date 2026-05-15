using Microsoft.EntityFrameworkCore;
using ServiceCenter.Models;

namespace ServiceCenter.Contex
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<WarehouseItem> WarehouseItems { get; set; }
        public DbSet<WarehouseRequest> WarehouseRequests { get; set; }
        public DbSet<AdminActionLog> AdminActionLogs { get; set; }

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Properties.Settings.Default.DbConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<User>()
                .Property(u => u.Login)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired(false);
            modelBuilder.Entity<User>()
                .Property(u => u.MasterSpecializations)
                .HasMaxLength(250)
                .IsRequired(false);
            modelBuilder.Entity<User>()
                .Property(u => u.HashPassword)
                .IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.Photo)
                .HasColumnType("VARBINARY(MAX)"); // Исправлено с BLOB на VARBINARY(MAX)

            // Конфигурация Comment
            modelBuilder.Entity<Comment>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>()
                .Property(c => c.Text)
                .IsRequired();

            // Конфигурация Order
            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.AssignedMaster)
                .WithMany(u => u.AssignedOrders)
                .HasForeignKey(o => o.AssignedMasterId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.DeviceType)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.DeviceBrand)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.DeviceModel)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.ContactPhone)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.ProblemDescription)
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryMethod)
                .HasMaxLength(50)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryAddress)
                .HasMaxLength(250)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.PublicNumber)
                .HasMaxLength(20)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentMethod)
                .HasMaxLength(40)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.IsOnlinePaymentCompleted)
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.OnlinePaymentPaidAt)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.EstimatedPartsCost)
                .HasColumnType("DECIMAL(18,2)")
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.MasterWorkCost)
                .HasColumnType("DECIMAL(18,2)")
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.EstimatedRepairCost)
                .HasColumnType("DECIMAL(18,2)")
                .IsRequired();
            modelBuilder.Entity<Order>()
                .Property(o => o.CompletedAt)
                .IsRequired(false);
            modelBuilder.Entity<Order>()
                .Property(o => o.ProblemPhoto)
                .HasColumnType("VARBINARY(MAX)")
                .IsRequired(false);

            modelBuilder.Entity<WarehouseItem>()
                .HasKey(w => w.Id);
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.Name)
                .HasMaxLength(120)
                .IsRequired();
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.Category)
                .HasMaxLength(80)
                .IsRequired(false);
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.Quantity)
                .IsRequired();
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.Unit)
                .HasMaxLength(30)
                .IsRequired(false);
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.MinimumQuantity)
                .IsRequired();
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.Notes)
                .HasMaxLength(250)
                .IsRequired(false);
            modelBuilder.Entity<WarehouseItem>()
                .Property(w => w.UnitPrice)
                .HasColumnType("DECIMAL(18,2)")
                .IsRequired();

            modelBuilder.Entity<WarehouseRequest>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<WarehouseRequest>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WarehouseRequest>()
                .HasOne(r => r.Master)
                .WithMany()
                .HasForeignKey(r => r.MasterId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            modelBuilder.Entity<WarehouseRequest>()
                .HasOne(r => r.WarehouseItem)
                .WithMany()
                .HasForeignKey(r => r.WarehouseItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            modelBuilder.Entity<WarehouseRequest>()
                .Property(r => r.RequestedItemName)
                .HasMaxLength(120)
                .IsRequired();
            modelBuilder.Entity<WarehouseRequest>()
                .Property(r => r.RequestedCategory)
                .HasMaxLength(80)
                .IsRequired(false);
            modelBuilder.Entity<WarehouseRequest>()
                .Property(r => r.RequestedQuantity)
                .IsRequired();
            modelBuilder.Entity<WarehouseRequest>()
                .Property(r => r.Status)
                .HasMaxLength(40)
                .IsRequired();

            modelBuilder.Entity<AdminActionLog>()
                .HasKey(log => log.Id);
            modelBuilder.Entity<AdminActionLog>()
                .Property(log => log.AdminLogin)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<AdminActionLog>()
                .Property(log => log.ActionType)
                .HasMaxLength(40)
                .IsRequired();
            modelBuilder.Entity<AdminActionLog>()
                .Property(log => log.EntityType)
                .HasMaxLength(40)
                .IsRequired();
            modelBuilder.Entity<AdminActionLog>()
                .Property(log => log.Description)
                .HasMaxLength(500)
                .IsRequired();
            modelBuilder.Entity<AdminActionLog>()
                .Property(log => log.CreatedAt)
                .IsRequired();
        }
    }
}
