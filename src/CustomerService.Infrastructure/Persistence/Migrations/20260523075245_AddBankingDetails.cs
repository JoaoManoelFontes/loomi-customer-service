using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banking_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    checking_account_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banking_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_banking_details_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_banking_details_customer_id",
                table: "banking_details",
                column: "customer_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banking_details");
        }
    }
}
