using CatalogApi.DataTransferObjects;
using CatalogApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CatalogApi.Extensions;

public static class CategoryExtensions
{
    public static CategoryDTO ToDTO(this Category category)
    {
        return new CategoryDTO()
        { CategoryId = category.CategoryId, Name = category.Name, ImageURL = category.ImageURL };        
    }

    public static Category ToCategory (this CategoryDTO categoryDTO)
    {
        return new Category()
        { CategoryId = categoryDTO.CategoryId, Name = categoryDTO.Name, ImageURL = categoryDTO.ImageURL };
    }

    public static IEnumerable<CategoryDTO> ToDTOList(this IEnumerable<Category> categoryList)
    {
        return categoryList.Select(category => new CategoryDTO()
        { CategoryId = category.CategoryId, Name = category.Name, ImageURL= category.ImageURL }
        );
    }
}
