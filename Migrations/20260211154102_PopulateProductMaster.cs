using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    public partial class PopulateProductMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //
            // 1. Insert distinct Products from Inventories
            //
            migrationBuilder.Sql(@"
                INSERT INTO ""Products"" (""Name"", ""EPA"", ""DefaultUnitOfMeasure"", ""Category"", ""Description"")
                SELECT DISTINCT
                    i.""ChemicalName"",
                    i.""EPA"",
                    i.""UnitOfMeasure"",
                    'Chemical',                -- default category
                    ''                         -- empty description
                FROM ""Inventories"" i
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Products"" p
                    WHERE p.""Name"" = i.""ChemicalName""
                      AND p.""EPA"" = i.""EPA""
                );
            ");

            //
            // 2. Update Inventories.ProductId to match the new Products
            //
            migrationBuilder.Sql(@"
                UPDATE ""Inventories"" inv
                SET ""ProductId"" = p.""Id""
                FROM ""Products"" p
                WHERE p.""Name"" = inv.""ChemicalName""
                  AND p.""EPA"" = inv.""EPA"";
            ");

            //
            // 3. Insert ProductPurchases for each Inventory row
            //
            migrationBuilder.Sql(@"
                INSERT INTO ""ProductPurchases""
                (""ProductId"", ""VendorId"", ""QuantityReceived"", ""QuantityRemaining"",
                 ""PricePerUnit"", ""ReceivedDate"", ""PurchaseOrderId"")
                SELECT
                    inv.""ProductId"",
                    inv.""VendorId"",
                    inv.""QuantityOnHand"",
                    inv.""QuantityOnHand"",
                    inv.""Price"",
                    NOW(),                    -- default received date
                    NULL                      -- no PO link
                FROM ""Inventories"" inv
                WHERE inv.""ProductId"" IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM ""ProductPurchases"" pp
                      WHERE pp.""ProductId"" = inv.""ProductId""
                        AND pp.""VendorId"" = inv.""VendorId""
                  );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional cleanup if rollback is needed
            migrationBuilder.Sql(@"DELETE FROM ""ProductPurchases"";");
            migrationBuilder.Sql(@"UPDATE ""Inventories"" SET ""ProductId"" = NULL;");
            migrationBuilder.Sql(@"DELETE FROM ""Products"";");
        }
    }
}
