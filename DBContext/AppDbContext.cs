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
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } // Added PurchaseOrderItem DbSet
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteInventory> QuoteInventories { get; set; }
        public DbSet<LoadMix> LoadMixes { get; set; }
        public DbSet<LoadFields> LoadFields { get; set; }
        public DbSet<LoadMixDetails> LoadMixDetails { get; set; }
        public DbSet<UOMConversion> UOMConversions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<NoQuoteInvoice> NoQuoteInvoices { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Override OnModelCreating for additional configuration, like foreign keys
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the foreign key relationship between Inventory and Vendor
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Vendor)
                .WithMany(v => v.Inventories)
                .HasForeignKey(i => i.VendorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade deletes for Vendors

            // Configure the foreign key relationship between PurchaseOrder and PurchaseOrderItem
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder) // Each item belongs to a PurchaseOrder
                .WithMany(po => po.Items) // A PurchaseOrder can have multiple items
                .HasForeignKey(poi => poi.PurchaseOrderId) // Foreign key in PurchaseOrderItem
                .OnDelete(DeleteBehavior.Cascade); // Delete items when a PurchaseOrder is deleted

            // Configure the relationship between PurchaseOrderItem and Inventory
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne<Inventory>() // Link to Inventory for additional details
                .WithMany()
                .HasForeignKey(poi => poi.InventoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade deletes for Inventory

            // Configure Customer-Field relationship
            modelBuilder.Entity<Field>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.Fields)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete Fields when a Customer is deleted

            // Configure the relationship between Customer and Field
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Fields)
                .WithOne(f => f.Customer)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Customer-Quote relationship
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotes)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Quote-Inventory relationship
            modelBuilder.Entity<QuoteInventory>()
                .HasKey(qi => new { qi.QuoteId, qi.InventoryId });

            modelBuilder.Entity<QuoteInventory>()
                .HasOne(qi => qi.Quote)
                .WithMany(q => q.QuoteInventories)
                .HasForeignKey(qi => qi.QuoteId);

            modelBuilder.Entity<QuoteInventory>()
                .HasOne(qi => qi.Inventory)
                .WithMany(i => i.QuoteInventories)
                .HasForeignKey(qi => qi.InventoryId);

            // Configure RowVersion as a concurrency token
            modelBuilder.Entity<QuoteInventory>()
                .Property(qi => qi.RowVersion)
                .IsRowVersion()
                .HasColumnType("bytea")
                .IsRequired();

            modelBuilder.Entity<QuoteInventory>()
                .Property(qi => qi.RowVersion)
                .HasDefaultValueSql("gen_random_bytes(8)");

            modelBuilder.Entity<Quote>()
                .HasMany(q => q.QuoteInventories)
                .WithOne(qi => qi.Quote)
                .HasForeignKey(qi => qi.QuoteId);

            // Configure Quote-LoadMix relationship
            modelBuilder.Entity<LoadMix>()
                .HasOne(lm => lm.Quote)
                .WithMany(q => q.LoadMixes)
                .HasForeignKey(lm => lm.QuoteId)
                .OnDelete(DeleteBehavior.SetNull); // Handle nullable foreign key

            // Configure LoadMix-LoadFields relationship
            modelBuilder.Entity<LoadFields>()
                .HasOne(lf => lf.LoadMix)
                .WithMany(lm => lm.LoadFields)
                .HasForeignKey(lf => lf.LoadMixId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure LoadFields primary key to auto-increment
            modelBuilder.Entity<LoadFields>()
                .Property(lf => lf.Id)
                .ValueGeneratedOnAdd();

            // Configure LoadMix-LoadMixDetails relationship
            modelBuilder.Entity<LoadMixDetails>()
                .HasOne(lmd => lmd.LoadMix)
                .WithMany(lm => lm.LoadMixDetails)
                .HasForeignKey(lmd => lmd.LoadMixId);

            modelBuilder.Entity<UOMConversion>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PUOM)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.SUOM)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.QUOM)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.ConversionFactor)
                      .IsRequired()
                      .HasColumnType("decimal(18, 6)");
            });

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.QuoteInventory)
                .WithMany(qi => qi.Invoices)
                .HasForeignKey(i => new { i.QuoteId, i.InventoryId }); // Correct composite key relationship

            modelBuilder.Entity<NoQuoteInvoice>()
                .HasOne(nqi => nqi.Customer)
                .WithMany(c => c.NoQuoteInvoices)
                .HasForeignKey(nqi => nqi.CustomerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
