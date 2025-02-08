using Microsoft.EntityFrameworkCore;
using PAS.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace PAS.DBContext
{
    public class AppDbContext : DbContext
    {
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteInventory> QuoteInventory { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Override OnModelCreating for additional configuration, like foreign keys
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the foreign key relationship between Inventory and Vendor
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Vendor)              // Inventory has one Vendor
                .WithMany(v => v.Inventories)       // Vendor can have many Inventories
                .HasForeignKey(i => i.VendorId)     // Foreign key in Inventory table
                .OnDelete(DeleteBehavior.Restrict); // Optional: prevent cascade deletes

            // Configure the relationship explicitly for PurchaseOrder and Inventory
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Inventory) // Navigation property
                .WithMany() // Inventory can be related to many PurchaseOrders
                .HasForeignKey(po => po.InventoryId) // Foreign key in PurchaseOrder
                .OnDelete(DeleteBehavior.Restrict); // Define delete behavior if necessary

            // Configure Customer-Field relationship
            modelBuilder.Entity<Field>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.Fields)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between Customer and Field
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Fields)
                .WithOne(f => f.Customer)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete fields when a customer is deleted

            // Configure Customer-Quote relationship
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotes)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between Quote and Inventory

            modelBuilder.Entity<QuoteInventory>()
                .HasKey(qi => new { qi.QuoteId, qi.InventoryId });

            modelBuilder.Entity<QuoteInventory>()
                .HasOne(qi => qi.Quote)
                .WithMany(q => q.QuoteInventories)
                .HasForeignKey(qi => qi.QuoteId);

            modelBuilder.Entity<QuoteInventory>()
                .HasOne(qi => qi.Inventory)
                .WithMany(i => i.QuoteInventories) // Update navigation property in Inventory
                .HasForeignKey(qi => qi.InventoryId);

            // Configure RowVersion as a concurrency token
            modelBuilder.Entity<QuoteInventory>()
                .Property(qi => qi.RowVersion)
                .IsRowVersion()
                .HasColumnType("bytea") // Set RowVersion to bytea type
                .IsRequired();


            base.OnModelCreating(modelBuilder);
        }
    }
}
