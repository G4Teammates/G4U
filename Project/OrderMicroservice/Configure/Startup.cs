using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.DBContexts;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using MongoDB.Driver;
using MongoDB.Bson;
using OrderMicroservice.Models;

namespace OrderMicroservice.Configure
{
    public static class Startup
    {
        /// <summary>
        /// Add Startup Service includes Database, Mapper, DI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns>service in IServiceCollection</returns>
        public static IServiceCollection AddStartupService(this IServiceCollection services, IConfiguration config)
        {
            #region Register Database
            #region SQL
            //_ = services.AddDbContext<OrderDbContext>(options =>
            //{
            //    options.UseSqlServer(config.GetConnectionString("AzureSQLOrderDBConnection"));
            //});

            //services.AddIdentity<Order, IdentityRole<Guid>>()
            //    .AddEntityFrameworkStores<OrderDbContext>()
            //    .AddDefaultTokenProviders();
            #endregion

            #region noSQL

            MoMoOptionModel.AccessKey = config["16"];
            MoMoOptionModel.SecretKey = config["17"];

            //Connect MongoDb by connection string
            var client = new MongoClient(config["1"]!);
            //Create or get if database exists
            var database = client.GetDatabase("OrderDb");
            services.AddDbContext<OrderDbContext>(option => option
                .UseMongoDB(client, database.DatabaseNamespace.DatabaseName)
            );

            #endregion

            #endregion

            #region Register Mapper
            IMapper mapper = Mapper.RegisterMaps().CreateMapper();
            services.AddSingleton(mapper);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            #endregion

            #region Register DI

            _ = services.AddDependencyInjection();

            #endregion

            return services;
        }
    }
}
