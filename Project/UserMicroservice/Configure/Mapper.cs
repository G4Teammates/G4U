using AutoMapper;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroService.Models;

namespace UserMicroService.Configure
{
    /// <summary>
    /// Class for Register mapper models and entities
    /// <br/>
    /// Lớp dùng để tạo ánh xạ giữa models và entities
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Register mapper
        /// <br/>
        /// Đăng kí mapper, ánh xạ models và entities
        /// </summary>
        /// <returns>MapperConfiguration</returns>
        public static MapperConfiguration RegisterMaps()
        {
            return new MapperConfiguration(cfg =>
            {
                //Register mapper here⬇️

                cfg.CreateMap<User, UserModel>().ReverseMap();
                cfg.CreateMap<UserModel, UserViewModel>().ReverseMap();

                //Register mapper here⬆️
            });
        }
    }
}
