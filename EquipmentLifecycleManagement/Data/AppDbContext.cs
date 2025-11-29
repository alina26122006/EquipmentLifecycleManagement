using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EquipmentLifecycleManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Maintenance> Maintenance { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-QOG2EQH;Database=EquipmentLifecycleDB;Trusted_Connection=true;TrustServerCertificate=true;");
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
}