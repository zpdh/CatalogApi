using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogApi.Migrations
{
    /// <inheritdoc />
    public partial class PopulateProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Insert into Products(Name, Description, Price, Stock, ImageURL, RegistrationDate, CategoryId)" +
            " Values('Coca Cola', '350ml cola soda', '2.50', 50, 'cocacola.jpg', now(), 1)");
            migrationBuilder.Sql("Insert into Products(Name, Description, Price, Stock, ImageURL, RegistrationDate, CategoryId)" +
            " Values('Doritos', 'Corn tortilla chip snack', '1.25', 10, 'doritos.jpg', now(), 2)");
            migrationBuilder.Sql("Insert into Products(Name, Description, Price, Stock, ImageURL, RegistrationDate, CategoryId)" +
            " Values('Pudding', 'Condensed milk pudding', '5.00', 20, 'pudding.jpg', now(), 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from Products");
        }
    }
}
