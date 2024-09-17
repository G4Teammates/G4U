using AutoMapper;
using CollectionMicroservice.DBContexts.Entities;

namespace CollectionMicroservice.Configure
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

                //cfg.CreateMap<Collection, CollectionModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
