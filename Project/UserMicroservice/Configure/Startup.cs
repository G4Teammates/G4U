using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.DBContexts;
using Microsoft.EntityFrameworkCore;

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

            services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
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
