using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailDispatcherAPI.Migrations
{
    /// <inheritdoc />
    public partial class EmailIdempotencypublishstateinclusion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "EmailIdempotency",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "EmailIdempotency");
        }
    }
}
