using AutoMapper;
using BackEnd.DTO.Category;
using BackEnd.DTO.Product;
using BackEnd.Models;

namespace BackEnd.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            #region MapsForCategory
            CreateMap<AddCategory, Category>();
            CreateMap<EditCategory, Category>();
            CreateMap<Category, GetCategory>();
            #endregion

            #region MapsForProducts
            CreateMap<AddProduct, Product>();
            CreateMap<EditProduct, Product>();
            CreateMap<Product, GetProduct>();
            #endregion
        }
    }
}
