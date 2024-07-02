using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogApi.Migrations
{
    /// <inheritdoc />
    public partial class PopulateCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Insert into Categories(Name, ImageURL) Values('Drinks', 'drinks.jpg')");
            migrationBuilder.Sql("Insert into Categories(Name, ImageURL) Values('Snacks', 'snacks.jpg')");
            migrationBuilder.Sql("Insert into Categories(Name, ImageURL) Values('Desserts', 'desserts.jpg')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from Categorias");
        }
    }
}
