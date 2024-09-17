using AutoMapper;
using LinkMicroservice.DBContexts.Entities;

namespace LinkMicroservice.Configure
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

                //cfg.CreateMap<Link, LinkModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
