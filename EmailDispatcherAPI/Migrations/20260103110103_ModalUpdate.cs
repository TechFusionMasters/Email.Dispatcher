using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailDispatcherAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModalUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageKey",
                table: "EmailLog",
                newName: "ToAddress");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "EmailLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmailIdempotencyId",
                table: "EmailLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "EmailLog",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_EmailIdempotencyId",
                table: "EmailLog",
                column: "EmailIdempotencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLog_EmailIdempotency_EmailIdempotencyId",
                table: "EmailLog",
                column: "EmailIdempotencyId",
                principalTable: "EmailIdempotency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailLog_EmailIdempotency_EmailIdempotencyId",
                table: "EmailLog");

            migrationBuilder.DropIndex(
                name: "IX_EmailLog_EmailIdempotencyId",
                table: "EmailLog");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "EmailLog");

            migrationBuilder.DropColumn(
                name: "EmailIdempotencyId",
                table: "EmailLog");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "EmailLog");

            migrationBuilder.RenameColumn(
                name: "ToAddress",
                table: "EmailLog",
                newName: "MessageKey");
        }
    }
}
