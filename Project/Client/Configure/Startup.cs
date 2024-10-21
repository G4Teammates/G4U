using AutoMapper;
using Client.Models;
using System.Security.Claims;

namespace Client.Configure
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
            #region Initialize Cloudinary
            ConfigKeyModel.CloudinaryKey = config["14"]!;
            ConfigKeyModel.CloudinarySecret = config["15"]!;
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
