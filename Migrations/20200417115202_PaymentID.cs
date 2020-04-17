using Microsoft.EntityFrameworkCore.Migrations;

namespace InoxicoHP.Migrations
{
    public partial class PaymentID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "paymentReceived",
                table: "Customer",
                newName: "PaymentReceived");

            migrationBuilder.AddColumn<string>(
                name: "PaymentID",
                table: "Customer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "PaymentReceived",
                table: "Customer",
                newName: "paymentReceived");
        }
    }
}
