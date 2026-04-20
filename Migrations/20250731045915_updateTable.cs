using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace voucherMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class updateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffLists",
                table: "StaffLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sellers",
                table: "Sellers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PayHistories",
                table: "PayHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Floatings",
                table: "Floatings");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "student");

            migrationBuilder.RenameTable(
                name: "StaffLists",
                newName: "staffList");

            migrationBuilder.RenameTable(
                name: "Sellers",
                newName: "seller");

            migrationBuilder.RenameTable(
                name: "PayHistories",
                newName: "payhistory");

            migrationBuilder.RenameTable(
                name: "Floatings",
                newName: "floating");

            migrationBuilder.AddPrimaryKey(
                name: "PK_student",
                table: "student",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_staffList",
                table: "staffList",
                column: "req_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_seller",
                table: "seller",
                column: "s_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payhistory",
                table: "payhistory",
                column: "h_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_floating",
                table: "floating",
                column: "h_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_student",
                table: "student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_staffList",
                table: "staffList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_seller",
                table: "seller");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payhistory",
                table: "payhistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_floating",
                table: "floating");

            migrationBuilder.RenameTable(
                name: "student",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "staffList",
                newName: "StaffLists");

            migrationBuilder.RenameTable(
                name: "seller",
                newName: "Sellers");

            migrationBuilder.RenameTable(
                name: "payhistory",
                newName: "PayHistories");

            migrationBuilder.RenameTable(
                name: "floating",
                newName: "Floatings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffLists",
                table: "StaffLists",
                column: "req_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sellers",
                table: "Sellers",
                column: "s_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayHistories",
                table: "PayHistories",
                column: "h_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Floatings",
                table: "Floatings",
                column: "h_id");
        }
    }
}
