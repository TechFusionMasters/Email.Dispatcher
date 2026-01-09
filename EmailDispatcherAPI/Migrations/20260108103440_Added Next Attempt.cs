using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailDispatcherAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedNextAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAt",
                table: "EmailLog",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextAttemptAt",
                table: "EmailLog");
        }
    }
}
