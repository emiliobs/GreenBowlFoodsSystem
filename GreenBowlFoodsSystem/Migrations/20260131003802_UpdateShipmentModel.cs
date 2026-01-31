using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenBowlFoodsSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_DeliveryForms_DeliveryFormId",
                table: "Shipments");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryFormId",
                table: "Shipments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FinishedProductId",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityShipped",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Shipments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalValue",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_FinishedProductId",
                table: "Shipments",
                column: "FinishedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_DeliveryForms_DeliveryFormId",
                table: "Shipments",
                column: "DeliveryFormId",
                principalTable: "DeliveryForms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_FinishedProducts_FinishedProductId",
                table: "Shipments",
                column: "FinishedProductId",
                principalTable: "FinishedProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_DeliveryForms_DeliveryFormId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_FinishedProducts_FinishedProductId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_FinishedProductId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "FinishedProductId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "QuantityShipped",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalValue",
                table: "Shipments");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryFormId",
                table: "Shipments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_DeliveryForms_DeliveryFormId",
                table: "Shipments",
                column: "DeliveryFormId",
                principalTable: "DeliveryForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
