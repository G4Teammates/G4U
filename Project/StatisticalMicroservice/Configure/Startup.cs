using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using MongoDB.Driver;
using MongoDB.Bson;
using StatisticalMicroservice.DBContexts;
using StatisticalMicroservice.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace StatisticalMicroservice.Configure
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
            //_ = services.AddDbContext<ProductDbContext>(options =>
            //{
            //    options.UseSqlServer(config.GetConnectionString("AzureSQLProductDBConnection"));
            //});

            //services.AddIdentity<Product, IdentityRole<Guid>>()
            //    .AddEntityFrameworkStores<ProductDbContext>()
            //    .AddDefaultTokenProviders();
            #endregion

            #region noSQL
            //Connect MongoDb by connection string
            var client = new MongoClient(config["1"]!);
            //Create or get if database exists
            var database = client.GetDatabase("StatisticalDb");
            services.AddDbContext<StatisticalDbcontext>(option => option
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

            #region Register Authentication
            JwtOptionModel.Secret = config["9"]!;
            JwtOptionModel.Issuer = config["10"]!;
            JwtOptionModel.Audience = config["11"]!;

           /* GoogleOptionModel.ClientId = config["12"]!;
            GoogleOptionModel.ClientSecret = config["13"]!;*/

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                /*options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;*/
            })
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/User/Login"; // Đường dẫn tới trang đăng nhập
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Để dev test, disable HTTPS
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtOptionModel.Issuer,
                    ValidAudience = JwtOptionModel.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptionModel.Secret)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
            });


            #endregion

            return services;
        }
    }
}
