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

                cfg.CreateMap<Products, ProductModel>().ReverseMap();

                cfg.CreateMap<Interactions, InteractionModel>().ReverseMap();

                cfg.CreateMap<Categories, CategoryModel>().ReverseMap();

                cfg.CreateMap<Link, LinkModel>().ReverseMap();

                cfg.CreateMap<Censorship, CensorshipModel>().ReverseMap();

                cfg.CreateMap<UserLikes, UserLikesModel>().ReverseMap();

                cfg.CreateMap<UserDisLikes, UserDisLikesModel>().ReverseMap();
                //Register mapper here⬆️
            });
        }
    }
}
