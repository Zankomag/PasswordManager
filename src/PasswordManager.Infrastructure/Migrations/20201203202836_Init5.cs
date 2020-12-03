using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordManager.Infrastructure.Migrations
{
    public partial class Init5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OutdatedTime",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(180, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OutdatedTime",
                table: "Users",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldDefaultValue: new TimeSpan(180, 0, 0, 0, 0));
        }
    }
}
