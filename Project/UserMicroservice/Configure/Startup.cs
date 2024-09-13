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
            //_ = services.AddDbContext<UserDbContext>(options =>
            //{
            //    options.UseSqlServer(config.GetConnectionString("AzureSQLUserDBConnection"));
            //});

            //services.AddIdentity<User, IdentityRole<Guid>>()
            //    .AddEntityFrameworkStores<UserDbContext>()
            //    .AddDefaultTokenProviders();

            #region noSQL
            var a = "mongodb+srv://Admin:Admin@cluster0.esirf.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0"; 
            var client = new MongoClient(a);
            //var db = UserDbContext.Create(client.GetDatabase("UserDb"));
            var database = client.GetDatabase("UserDb");
            //var movie = db.Users.FirstOrDefault(m => m.UserName == "11");
            var collection = database.GetCollection<BsonDocument>("Users");
            var document = new User{ UserName = "", Email = "", Status = 0 }.ToBsonDocument();
            collection.InsertOne(document);


            //services.AddDbContext<UserDbContext>(options =>
            //{
            //    options.UseMongoDB(a, "UserDb");
            //});
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
