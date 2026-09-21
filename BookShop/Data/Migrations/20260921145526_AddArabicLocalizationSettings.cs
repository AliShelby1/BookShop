using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArabicLocalizationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CartAnnouncementBannerAr",
                table: "StoreSettings",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultLanguage",
                table: "StoreSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EnableLanguageSwitcher",
                table: "StoreSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "StoreSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CartAnnouncementBannerAr", "DefaultLanguage", "EnableLanguageSwitcher" },
                values: new object[] { "تغليف إهدائي فاخر وفاصل كتب مجاني مع كل طلب يتجاوز 50 دولاراً.", "en", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartAnnouncementBannerAr",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EnableLanguageSwitcher",
                table: "StoreSettings");
        }
    }
}
