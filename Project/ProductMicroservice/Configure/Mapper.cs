using AutoMapper;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models;

namespace ProductMicroservice.Configure
{
    public static class Mapper
    {
        /// <summary>
        /// Register mapper
        /// </summary>
        /// <returns>MapperConfiguration</returns>
        public static MapperConfiguration RegisterMaps()
        {
            return new MapperConfiguration(cfg =>
            {
                //Register mapper here⬇️

                cfg.CreateMap<Product, ProductModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
