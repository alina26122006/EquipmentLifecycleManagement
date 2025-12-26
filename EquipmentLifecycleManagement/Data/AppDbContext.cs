using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EquipmentLifecycleManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Maintenance> Maintenance { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-QOG2EQH;Database=EquipmentLifecycleDB;Trusted_Connection=true;TrustServerCertificate=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настраиваем связь между оборудованием и отделениями
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Equipment)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Указываем имена таблиц
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Equipment>().ToTable("Equipment");
            modelBuilder.Entity<Maintenance>().ToTable("Maintenance");
            modelBuilder.Entity<Department>().ToTable("Departments");
        }
    }

    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InventoryNumber { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public DateTime CommissionDate { get; set; }

        // Добавляем связь с отделением
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public List<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
    }

    public class Maintenance
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Status { get; set; }

        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
    }

    // Новая модель для отделений
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; } // Короткий код отдела (например, "ИТ", "БУХ" и т.д.)
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Equipment> Equipment { get; set; } = new List<Equipment>();
    }
}