using AutoMapper;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.Models;

namespace OrderMicroservice.Configure
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

                //cfg.CreateMap<User, UserModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
