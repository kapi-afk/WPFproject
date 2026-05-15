using Microsoft.EntityFrameworkCore;
using ServiceCenter.Contex;
using ServiceCenter.Models;
using ServiceCenter.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceCenter.Repositories
{
   
   
    public static class RepositoryManager
    {
        private static AppDbContext _context;
        public static UserRepository Users { get; private set; }
        public static CommentRepository Comments { get; private set; }
        public static OrderRepository Orders { get; private set; }
        public static WarehouseRepository Warehouse { get; private set; }
        public static WarehouseRequestRepository WarehouseRequests { get; private set; }

        public static void Initialize()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated();
            EnsureOrderSchema();
            EnsureUserSchema();
            EnsureWarehouseSchema();
            EnsureWarehouseRequestSchema();

            Users = new UserRepository(_context);
            Comments = new CommentRepository(_context);
            Orders = new OrderRepository(_context);
            Warehouse = new WarehouseRepository(_context);
            WarehouseRequests = new WarehouseRequestRepository(_context);
            Orders.EnsurePublicNumbers();

            var admin = Users.GetByLogin("admin");
            if (admin == null)
            {
                var adminUser = new User
                {
                    Login = "admin",
                    HashPassword = HashHelper.HashPassword("admin"),
                    Name = "Admin",
                    Role = UserRole.Admin
                };
                Users.Add(adminUser);
            }

            var master = Users.GetByLogin("master");
            if (master == null)
            {
                var masterUser = new User
                {
                    Login = "master",
                    HashPassword = HashHelper.HashPassword("master"),
                    Name = "Мастер",
                    Role = UserRole.Master,
                    MasterSpecializations = "Ноутбуки;ПК;Оргтехника"
                };
                Users.Add(masterUser);
            }
            else if (string.IsNullOrWhiteSpace(master.MasterSpecializations))
            {
                master.MasterSpecializations = "Ноутбуки;ПК;Оргтехника";
                Users.Update(master);
            }

            if (!Warehouse.GetAll().Any())
            {
                SeedWarehouseItems();
            }

            AssignOpenOrdersToMasters();
        }

        private static void EnsureOrderSchema()
        {
            _context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Orders', 'ProblemPhoto') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [ProblemPhoto] VARBINARY(MAX) NULL;
    END

    IF COL_LENGTH('Orders', 'DeliveryMethod') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [DeliveryMethod] NVARCHAR(50) NULL;
    END

    IF COL_LENGTH('Orders', 'DeliveryAddress') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [DeliveryAddress] NVARCHAR(250) NULL;
    END

    IF COL_LENGTH('Orders', 'PublicNumber') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [PublicNumber] NVARCHAR(20) NULL;
    END

    IF COL_LENGTH('Orders', 'PaymentMethod') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [PaymentMethod] NVARCHAR(40) NULL;
    END

    IF COL_LENGTH('Orders', 'IsOnlinePaymentCompleted') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [IsOnlinePaymentCompleted] BIT NOT NULL CONSTRAINT [DF_Orders_IsOnlinePaymentCompleted] DEFAULT(0);
    END

    IF COL_LENGTH('Orders', 'OnlinePaymentPaidAt') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [OnlinePaymentPaidAt] DATETIME2 NULL;
    END

    IF COL_LENGTH('Orders', 'EstimatedRepairCost') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [EstimatedRepairCost] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Orders_EstimatedRepairCost] DEFAULT(0);
    END

    IF COL_LENGTH('Orders', 'EstimatedPartsCost') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [EstimatedPartsCost] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Orders_EstimatedPartsCost] DEFAULT(0);
    END

    IF COL_LENGTH('Orders', 'MasterWorkCost') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [MasterWorkCost] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Orders_MasterWorkCost] DEFAULT(0);
    END

    IF COL_LENGTH('Orders', 'CompletedAt') IS NULL
    BEGIN
        ALTER TABLE [Orders] ADD [CompletedAt] DATETIME2 NULL;
    END
END");

            _context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH('Orders', 'PaymentMethod') IS NOT NULL
BEGIN
    UPDATE [Orders]
    SET [PaymentMethod] = N'Оплата на месте'
    WHERE [PaymentMethod] IS NULL
       OR LTRIM(RTRIM([PaymentMethod])) = N'';
END");

            _context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Orders]', N'U') IS NOT NULL
   AND COL_LENGTH('Orders', 'EstimatedPartsCost') IS NOT NULL
   AND COL_LENGTH('Orders', 'MasterWorkCost') IS NOT NULL
   AND COL_LENGTH('Orders', 'EstimatedRepairCost') IS NOT NULL
BEGIN
    UPDATE [Orders]
    SET [EstimatedPartsCost] = [EstimatedRepairCost]
    WHERE ISNULL([EstimatedPartsCost], 0) = 0
      AND ISNULL([MasterWorkCost], 0) = 0
      AND ISNULL([EstimatedRepairCost], 0) > 0;
END");
        }

        private static void EnsureUserSchema()
        {
            _context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Users', 'Email') IS NULL
    BEGIN
        ALTER TABLE [Users] ADD [Email] NVARCHAR(150) NULL;
    END

    IF COL_LENGTH('Users', 'MasterSpecializations') IS NULL
    BEGIN
        ALTER TABLE [Users] ADD [MasterSpecializations] NVARCHAR(250) NULL;
    END
END");
        }

        private static void EnsureWarehouseSchema()
        {
            _context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[WarehouseItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [WarehouseItems]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(120) NOT NULL,
        [Category] NVARCHAR(80) NULL,
        [Quantity] INT NOT NULL CONSTRAINT [DF_WarehouseItems_Quantity] DEFAULT(0),
        [Unit] NVARCHAR(30) NULL,
        [MinimumQuantity] INT NOT NULL CONSTRAINT [DF_WarehouseItems_MinimumQuantity] DEFAULT(0),
        [Notes] NVARCHAR(250) NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_WarehouseItems_UnitPrice] DEFAULT(0)
    );
END
ELSE
BEGIN
    IF COL_LENGTH('WarehouseItems', 'Category') IS NULL
    BEGIN
        ALTER TABLE [WarehouseItems] ADD [Category] NVARCHAR(80) NULL;
    END

    IF COL_LENGTH('WarehouseItems', 'UnitPrice') IS NULL
    BEGIN
        ALTER TABLE [WarehouseItems] ADD [UnitPrice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_WarehouseItems_UnitPrice_2] DEFAULT(0);
    END

    IF COL_LENGTH('WarehouseItems', 'Unit') IS NULL
    BEGIN
        ALTER TABLE [WarehouseItems] ADD [Unit] NVARCHAR(30) NULL;
    END

    IF COL_LENGTH('WarehouseItems', 'MinimumQuantity') IS NULL
    BEGIN
        ALTER TABLE [WarehouseItems] ADD [MinimumQuantity] INT NOT NULL CONSTRAINT [DF_WarehouseItems_MinimumQuantity_2] DEFAULT(0);
    END

    IF COL_LENGTH('WarehouseItems', 'Notes') IS NULL
    BEGIN
        ALTER TABLE [WarehouseItems] ADD [Notes] NVARCHAR(250) NULL;
    END
END");
        }

        private static void EnsureWarehouseRequestSchema()
        {
            _context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[WarehouseRequests]', N'U') IS NULL
BEGIN
    CREATE TABLE [WarehouseRequests]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [MasterId] INT NULL,
        [WarehouseItemId] INT NULL,
        [RequestedItemName] NVARCHAR(120) NOT NULL,
        [RequestedCategory] NVARCHAR(80) NULL,
        [RequestedQuantity] INT NOT NULL CONSTRAINT [DF_WarehouseRequests_RequestedQuantity] DEFAULT(1),
        [Status] NVARCHAR(40) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_WarehouseRequests_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WarehouseRequests_Users] FOREIGN KEY ([MasterId]) REFERENCES [Users]([Id]),
        CONSTRAINT [FK_WarehouseRequests_WarehouseItems] FOREIGN KEY ([WarehouseItemId]) REFERENCES [WarehouseItems]([Id]) ON DELETE SET NULL
    );
END");
        }

        private static void SeedWarehouseItems()
        {
            var items = new List<WarehouseItem>
            {
                new WarehouseItem { Name = "Термопаста Arctic MX-4", Category = "Охлаждение", Quantity = 18, UnitPrice = 12.50m },
                new WarehouseItem { Name = "Кулер DeepCool GAMMAXX", Category = "Охлаждение", Quantity = 6, UnitPrice = 55.00m },
                new WarehouseItem { Name = "SSD Kingston 480GB", Category = "Накопители", Quantity = 9, UnitPrice = 95.00m },
                new WarehouseItem { Name = "HDD Toshiba 1TB", Category = "Накопители", Quantity = 5, UnitPrice = 82.00m },
                new WarehouseItem { Name = "Планка RAM DDR4 8GB", Category = "Оперативная память", Quantity = 14, UnitPrice = 68.00m },
                new WarehouseItem { Name = "Планка RAM DDR4 16GB", Category = "Оперативная память", Quantity = 8, UnitPrice = 123.00m },
                new WarehouseItem { Name = "Блок питания 500W", Category = "Питание", Quantity = 7, UnitPrice = 110.00m },
                new WarehouseItem { Name = "Разъем питания ноутбука", Category = "Разъемы и шлейфы", Quantity = 22, UnitPrice = 16.00m },
                new WarehouseItem { Name = "Шлейф матрицы 30 pin", Category = "Разъемы и шлейфы", Quantity = 11, UnitPrice = 24.00m },
                new WarehouseItem { Name = "Матрица 15.6 Full HD", Category = "Экраны", Quantity = 4, UnitPrice = 185.00m },
                new WarehouseItem { Name = "Клавиатура для Lenovo IdeaPad", Category = "Периферия ноутбука", Quantity = 6, UnitPrice = 49.00m },
                new WarehouseItem { Name = "Аккумулятор для ASUS X541", Category = "Питание", Quantity = 5, UnitPrice = 98.00m }
            };

            foreach (var item in items)
            {
                Warehouse.Add(item);
            }
        }

        private static void AssignOpenOrdersToMasters()
        {
            foreach (var order in Orders.GetAll()
                .Where(order => order.Status == OrderStatus.Created &&
                                !order.AssignedMasterId.HasValue &&
                                !string.IsNullOrWhiteSpace(order.DeviceType)))
            {
                var master = MasterAssignmentService.FindBestMaster(
                    order.DeviceType,
                    Users.GetByRole(UserRole.Master),
                    Orders.GetAll());

                if (master == null)
                {
                    continue;
                }

                order.AssignedMasterId = master.Id;
                order.AssignedMaster = master;
                order.Status = OrderStatus.Assigned;
                Orders.Update(order);
            }
        }
    }
}
