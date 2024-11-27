using AutoMapper;
using ReportMicroservice.DBContexts.Entities;
using ReportMicroservice.Models;

namespace ReportMicroservice.Configure
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
                cfg.CreateMap<Reports, ReportsModel>().ReverseMap();
            });
        }
    }
}
