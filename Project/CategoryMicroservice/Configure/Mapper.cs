using AutoMapper;
using CategoryMicroservice.DBContexts.Entities;

namespace CategoryMicroservice.Configure
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

                //cfg.CreateMap<Category, CategoryModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
