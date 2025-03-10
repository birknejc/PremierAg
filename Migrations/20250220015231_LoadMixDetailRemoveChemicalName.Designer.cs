﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PAS.DBContext;

#nullable disable

namespace PAS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250220015231_LoadMixDetailRemoveChemicalName")]
    partial class LoadMixDetailRemoveChemicalName
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LoadFields", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("FieldAcres")
                        .HasColumnType("numeric");

                    b.Property<decimal>("FieldAverageRate")
                        .HasColumnType("numeric");

                    b.Property<string>("FieldName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("FieldTotalGallons")
                        .HasColumnType("numeric");

                    b.Property<int>("LoadMixId")
                        .HasColumnType("integer");

                    b.Property<int>("SelectedFieldId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LoadMixId");

                    b.ToTable("LoadFields");
                });

            modelBuilder.Entity("PAS.Models.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CustomerBusinessName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerCell")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerCity")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerEmail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerFName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerFax")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerLName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerPhone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerState")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerStreet")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerZipCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("PAS.Models.Field", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Acres")
                        .HasColumnType("double precision");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<string>("FieldName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("PAS.Models.Inventory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ChemicalName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EPA")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("VendorId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("VendorId");

                    b.ToTable("Inventories");
                });

            modelBuilder.Entity("PAS.Models.LoadMix", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Crop")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("LMRatePerAcre")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LoadDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("LoadTime")
                        .HasColumnType("interval");

                    b.Property<int?>("QuoteId")
                        .HasColumnType("integer");

                    b.Property<int>("TotalAcres")
                        .HasColumnType("integer");

                    b.Property<int>("TotalGallons")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("QuoteId");

                    b.ToTable("LoadMixes");
                });

            modelBuilder.Entity("PAS.Models.LoadMixDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("EPA")
                        .HasColumnType("text");

                    b.Property<int>("LoadMixId")
                        .HasColumnType("integer");

                    b.Property<decimal?>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("Product")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("QuotePrice")
                        .HasColumnType("numeric");

                    b.Property<string>("QuoteUnitOfMeasure")
                        .HasColumnType("text");

                    b.Property<string>("RatePerAcre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Total")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("LoadMixId");

                    b.ToTable("LoadMixDetails");
                });

            modelBuilder.Entity("PAS.Models.PurchaseOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BusinessName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ChemicalName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("DeliveryPickUpDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EPANumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("InventoryId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PONumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("PaymentDueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PickUpLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int>("QuantityOrdered")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReceivedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("InventoryId");

                    b.ToTable("PurchaseOrders");
                });

            modelBuilder.Entity("PAS.Models.Quote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CustomerBusinessName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("CustomerId")
                        .HasColumnType("integer");

                    b.Property<string>("QuoteCity")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("QuoteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("QuotePhone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("QuoteState")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("QuoteStreet")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("QuoteTotal")
                        .HasColumnType("numeric");

                    b.Property<string>("QuoteZipcode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("PAS.Models.QuoteInventory", b =>
                {
                    b.Property<int>("QuoteId")
                        .HasColumnType("integer");

                    b.Property<int>("InventoryId")
                        .HasColumnType("integer");

                    b.Property<string>("ChemicalName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EPA")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<decimal>("QuantityPerAcre")
                        .HasColumnType("numeric");

                    b.Property<decimal>("QuotePrice")
                        .HasColumnType("numeric");

                    b.Property<string>("QuoteUnitOfMeasure")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasDefaultValueSql("gen_random_bytes(8)");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("QuoteId", "InventoryId");

                    b.HasIndex("InventoryId");

                    b.ToTable("QuoteInventory");
                });

            modelBuilder.Entity("PAS.Models.Vendor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BusinessName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Fax")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SalesRepEmail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SalesRepName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SalesRepPhone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StreetAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Vendors");
                });

            modelBuilder.Entity("LoadFields", b =>
                {
                    b.HasOne("PAS.Models.LoadMix", "LoadMix")
                        .WithMany("LoadFields")
                        .HasForeignKey("LoadMixId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LoadMix");
                });

            modelBuilder.Entity("PAS.Models.Field", b =>
                {
                    b.HasOne("PAS.Models.Customer", "Customer")
                        .WithMany("Fields")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("PAS.Models.Inventory", b =>
                {
                    b.HasOne("PAS.Models.Vendor", "Vendor")
                        .WithMany("Inventories")
                        .HasForeignKey("VendorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Vendor");
                });

            modelBuilder.Entity("PAS.Models.LoadMix", b =>
                {
                    b.HasOne("PAS.Models.Quote", "Quote")
                        .WithMany("LoadMixes")
                        .HasForeignKey("QuoteId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Quote");
                });

            modelBuilder.Entity("PAS.Models.LoadMixDetails", b =>
                {
                    b.HasOne("PAS.Models.LoadMix", "LoadMix")
                        .WithMany("LoadMixDetails")
                        .HasForeignKey("LoadMixId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LoadMix");
                });

            modelBuilder.Entity("PAS.Models.PurchaseOrder", b =>
                {
                    b.HasOne("PAS.Models.Inventory", "Inventory")
                        .WithMany()
                        .HasForeignKey("InventoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Inventory");
                });

            modelBuilder.Entity("PAS.Models.Quote", b =>
                {
                    b.HasOne("PAS.Models.Customer", "Customer")
                        .WithMany("Quotes")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("PAS.Models.QuoteInventory", b =>
                {
                    b.HasOne("PAS.Models.Inventory", "Inventory")
                        .WithMany("QuoteInventories")
                        .HasForeignKey("InventoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PAS.Models.Quote", "Quote")
                        .WithMany("QuoteInventories")
                        .HasForeignKey("QuoteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Inventory");

                    b.Navigation("Quote");
                });

            modelBuilder.Entity("PAS.Models.Customer", b =>
                {
                    b.Navigation("Fields");

                    b.Navigation("Quotes");
                });

            modelBuilder.Entity("PAS.Models.Inventory", b =>
                {
                    b.Navigation("QuoteInventories");
                });

            modelBuilder.Entity("PAS.Models.LoadMix", b =>
                {
                    b.Navigation("LoadFields");

                    b.Navigation("LoadMixDetails");
                });

            modelBuilder.Entity("PAS.Models.Quote", b =>
                {
                    b.Navigation("LoadMixes");

                    b.Navigation("QuoteInventories");
                });

            modelBuilder.Entity("PAS.Models.Vendor", b =>
                {
                    b.Navigation("Inventories");
                });
#pragma warning restore 612, 618
        }
    }
}
