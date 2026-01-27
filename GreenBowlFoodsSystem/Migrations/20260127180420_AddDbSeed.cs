using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenBowlFoodsSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDbSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FinishedProducts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RawMaterials",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "BillingAddress", "ContactName", "CustomerName", "Email", "Phone" },
                values: new object[] { 1, "Street London 55", "Emiliano Barrera", "Costco Wholesale", "costco@yopmail.com", "123-1234" });

            migrationBuilder.InsertData(
                table: "FinishedProducts",
                columns: new[] { "Id", "ProductName", "QuantityAvailable", "SKU", "UnitPrice" },
                values: new object[] { 1, "Quinoa Salad", 100, "QB-001", 12.50m });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "ContactPerson", "Email", "Phone", "SupplierName" },
                values: new object[] { 1, "Alice Johnson", "Raaz@yopmail.com", "555-1234", "Raaz Food Ingredients" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "123", "Admin", "Admin" },
                    { 2, "123", "Staff", "Emilio" }
                });

            migrationBuilder.InsertData(
                table: "RawMaterials",
                columns: new[] { "Id", "ExpiryDate", "LotNumber", "MaterialName", "QuantityInStock", "SupplierId", "Unit" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "L-8821", "Basmati Rice", 500m, 1, "kg" });
        }
    }
}
