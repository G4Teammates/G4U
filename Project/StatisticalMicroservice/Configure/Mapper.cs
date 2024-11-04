using AutoMapper;
using StatisticalMicroservice.DBContexts.Entities;
using StatisticalMicroservice.Model;


namespace StatisticalMicroservice.Configure
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

                cfg.CreateMap<Statistical, StatisticalModel>();
                cfg.CreateMap<TotalWebsiteInfo, TotalWebsiteInfoModel>();
                cfg.CreateMap<UserInfo, UserInfoModel>();
               
                //Register mapper here⬆️
            });
        }
    }
}
