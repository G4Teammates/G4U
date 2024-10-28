using AutoMapper;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.Models;
using OrderMicroservice.Models.OrderModel;

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

                cfg.CreateMap<Order, OrderModel>().ReverseMap();
                cfg.CreateMap<OrderItems, OrderItemModel>().ReverseMap();

                //Register mapper here⬆️
            });
        }
    }
}
