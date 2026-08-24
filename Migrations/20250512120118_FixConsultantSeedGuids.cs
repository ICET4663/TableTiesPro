using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TableTies.Migrations
{
    /// <inheritdoc />
    public partial class FixConsultantSeedGuids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Consultants",
                keyColumn: "Id",
                keyValue: new Guid("0b5b13de-9dff-4032-9a10-d8340c296e7c"));

            migrationBuilder.DeleteData(
                table: "Consultants",
                keyColumn: "Id",
                keyValue: new Guid("0cb16f22-31dd-43c5-9ed5-2213ce50f474"));

            migrationBuilder.DeleteData(
                table: "Consultants",
                keyColumn: "Id",
                keyValue: new Guid("41e870e7-9953-42b3-bf8b-6070023b7214"));

            migrationBuilder.InsertData(
                table: "Consultants",
                columns: new[] { "Id", "Name", "Specialty" },
                values: new object[,]
                {
                    { new Guid("cccccccc-0000-0000-0000-000000000001"), "Alice Smith", "Business Strategy" },
                    { new Guid("cccccccc-0000-0000-0000-000000000002"), "Bob Johnson", "Technical Consulting" },
                    { new Guid("cccccccc-0000-0000-0000-000000000003"), "Charlie Brown", "Marketing" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Consultants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Consultants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Consultants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000003"));

            migrationBuilder.InsertData(
                table: "Consultants",
                columns: new[] { "Id", "Name", "Specialty" },
                values: new object[,]
                {
                    { new Guid("0b5b13de-9dff-4032-9a10-d8340c296e7c"), "Charlie Brown", "Marketing" },
                    { new Guid("0cb16f22-31dd-43c5-9ed5-2213ce50f474"), "Alice Smith", "Business Strategy" },
                    { new Guid("41e870e7-9953-42b3-bf8b-6070023b7214"), "Bob Johnson", "Technical Consulting" }
                });
        }
    }
}
