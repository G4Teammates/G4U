﻿using AutoMapper;

namespace Client.Configure
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

                //cfg.AllowNullCollections = true;
                //cfg.CreateMap<User, UserModel>().ReverseMap();
                //Register mapper here⬆️
            });
        }
    }
}