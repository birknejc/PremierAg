using Microsoft.EntityFrameworkCore;
using PAS.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using PAS.Models;

namespace PAS.DBContext
{
    public class AppDbContext : DbContext
    {
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } 
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
        public DbSet<CustomerField> CustomerFields { get; set; }
        public DbSet<InvoiceHeader> InvoiceHeaders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPurchase> ProductPurchases { get; set; }
        public DbSet<ProductVendor> ProductVendors { get; set; }
        public DbSet<ApplicatorLicense> ApplicatorLicenses { get; set; }
        public DbSet<InventoryAudit> InventoryAudits { get; set; }




        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Override OnModelCreating for additional configuration, like foreign keys
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configure the foreign key relationship between PurchaseOrder and PurchaseOrderItem
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder) // Each item belongs to a PurchaseOrder
                .WithMany(po => po.Items) // A PurchaseOrder can have multiple items
                .HasForeignKey(poi => poi.PurchaseOrderId) // Foreign key in PurchaseOrderItem
                .OnDelete(DeleteBehavior.Cascade); // Delete items when a PurchaseOrder is deleted

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
                .HasKey(qi => qi.Id);

            modelBuilder.Entity<QuoteInventory>()
                .Property(qi => qi.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<QuoteInventory>()
                .HasOne(qi => qi.Quote)
                .WithMany(q => q.QuoteInventories)
                .HasForeignKey(qi => qi.QuoteId);

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

            // Configure LoadMix relationships and properties
            modelBuilder.Entity<LoadMix>()
                .Property(lm => lm.Id)
                .ValueGeneratedOnAdd(); // Keep Id as the primary key and auto-increment

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

            // Configure LoadFields-Customer relationship
            modelBuilder.Entity<LoadFields>()
                .HasOne(lf => lf.Customer)
                .WithMany() // Optional: Define this for navigation back to LoadFields in Customer
                .HasForeignKey(lf => lf.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Use Restrict to prevent cascading deletes

            // Configure LoadFields primary key to auto-increment
            modelBuilder.Entity<LoadFields>()
                .Property(lf => lf.Id)
                .ValueGeneratedOnAdd();

            // Configure LoadMix-LoadMixDetails relationship
            modelBuilder.Entity<LoadMixDetails>()
                .HasOne(lmd => lmd.LoadMix)
                .WithMany(lm => lm.LoadMixDetails)
                .HasForeignKey(lmd => lmd.LoadMixId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete LoadMixDetails when LoadMix is deleted

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
                .HasForeignKey(i => i.QuoteInventoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<NoQuoteInvoice>()
                .HasOne(nqi => nqi.Customer)
                .WithMany(c => c.NoQuoteInvoices)
                .HasForeignKey(nqi => nqi.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete or SetNull based on your needs

            modelBuilder.Entity<NoQuoteInvoice>()
                .Property(nqi => nqi.Id)
                .ValueGeneratedOnAdd(); // Ensure auto-increment for Id

            modelBuilder.Entity<CustomerField>().ToTable("CustomerField");

            modelBuilder.Entity<CustomerField>()
            .HasKey(cf => new { cf.CustomerId, cf.FieldId });

            modelBuilder.Entity<CustomerField>()
                .HasOne(cf => cf.Customer)
                .WithMany(c => c.CustomerFields)
                .HasForeignKey(cf => cf.CustomerId);

            modelBuilder.Entity<CustomerField>()
                .HasOne(cf => cf.Field)
                .WithMany(f => f.CustomerFields)
                .HasForeignKey(cf => cf.FieldId);

            modelBuilder.Entity<InvoiceHeader>()
            .HasMany(h => h.InvoiceLines)
            .WithOne(l => l.InvoiceHeader)
            .HasForeignKey(l => l.InvoiceGroupId);

            modelBuilder.Entity<Payment>()
            .HasOne(p => p.InvoiceHeader)
            .WithMany(h => h.Payments)
            .HasForeignKey(p => p.InvoiceGroupId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductVendor>()
            .HasKey(pv => new { pv.ProductId, pv.VendorId });

            modelBuilder.Entity<ProductVendor>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVendors)
                .HasForeignKey(pv => pv.ProductId);

            modelBuilder.Entity<ProductVendor>()
                .HasOne(pv => pv.Vendor)
                .WithMany(v => v.ProductVendors)
                .HasForeignKey(pv => pv.VendorId);

            modelBuilder.Entity<Quote>()
                .Property(q => q.Status)
                .HasConversion<string>();


            base.OnModelCreating(modelBuilder);
        }
    }
}
