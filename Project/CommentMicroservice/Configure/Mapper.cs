using AutoMapper;
using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.Models;

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

                cfg.CreateMap<Comment, CommentModel>().ReverseMap();
                cfg.CreateMap<UserDisLikes, UserDisLikesModel>().ReverseMap();
                cfg.CreateMap<UserLikes, UserLikesModel>().ReverseMap();
                //Register mapper here⬆️
            });
        }
    }
}
