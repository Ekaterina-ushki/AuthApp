using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AuthApp.Data.Migrations
{
    public partial class AddTestUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers", 
                columns: new [] {"Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "AccessFailedCount", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled"},
                values: new object[] {"f2bc0937-52d6-4012-bc65-91535266799d", "test@test.com", "TEST@TEST.COM", "test@test.com", "TEST@TEST.COM", true, "AQAAAAEAACcQAAAAEIQXb6ZLlLw48E6c7BIo6G9QBSmT3GhT5BJk/Gt4mmSoa1YE0Z5Zeb4FHGTLSamOVg==", "UVTAIDKN6J4SOF5ME7V72A6LCLBKMKMZ", "c55c0af2-2514-4c33-8aff-144102e67c45", 0, false, false, true});
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f2bc0937-52d6-4012-bc65-91535266799d");
        }
    }
}
