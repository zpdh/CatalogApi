using AutoMapper;
using CatalogApi.Models;

namespace CatalogApi.DataTransferObjects;

public class ProductDTOMappingProfile : Profile
{
    public ProductDTOMappingProfile()
    {
        CreateMap<Product, ProductDTO>().ReverseMap();
        CreateMap<Category, CategoryDTO>().ReverseMap();
        CreateMap<Product, ProductUpdateRequestDTO>().ReverseMap();
        CreateMap<Product, ProductUpdateResponseDTO>().ReverseMap();
    }
}
