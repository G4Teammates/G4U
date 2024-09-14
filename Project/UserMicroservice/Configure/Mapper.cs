using AutoMapper;
using UserMicroservice.DBContexts.Entities;
using UserMicroService.Models;

namespace UserMicroService.Configure
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

                cfg.CreateMap<User, UserModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
