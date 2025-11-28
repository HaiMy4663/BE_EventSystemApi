using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { 1, "admin@system.com", "System Administrator", "$2a$11$BFELCZ4EhsP/RdAIs6b8NuKje3quyYHrbRzWV3QbZiZxQC3Cooh8K", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
