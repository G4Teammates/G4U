using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.DBContexts;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using MongoDB.Driver;
using MongoDB.Bson;

namespace UserMicroService.Configure
{
    /// <summary>
    /// Setup config about database, mapper, DI, Authen
    /// <br/>
    /// Cài đặt các cấu hình về cơ sở dữ liệu, ánh xạ, DI, xác thực
    /// </summary>
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
            //_ = services.AddDbContext<UserDbContext>(options =>
            //{
            //    options.UseSqlServer(config.GetConnectionString("AzureSQLUserDBConnection"));
            //});

            //services.AddIdentity<User, IdentityRole<Guid>>()
            //    .AddEntityFrameworkStores<UserDbContext>()
            //    .AddDefaultTokenProviders();
            #endregion

            #region noSQL
            //Connect MongoDb by connection string
            var client = new MongoClient(config["1"]!+ "?connect=replicaSet");
            //Create or get if database exists
            var database = client.GetDatabase("UserDb");
            services.AddDbContext<UserDbContext>(option => option
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

            _ = DependencyInjection.AddDependencyInjection(services);

            #endregion

            return services;
        }
    }
}
