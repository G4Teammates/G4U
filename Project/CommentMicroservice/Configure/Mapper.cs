using AutoMapper;
using CommentMicroservice.DBContexts.Entities;

namespace CommentMicroservice.Configure
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

                //cfg.CreateMap<Comment, CommentModel>().ReverseMap();


                //Register mapper here⬆️
            });
        }
    }
}
