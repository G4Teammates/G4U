using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.DBContexts;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserMicroservice.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;

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
            #region Register Authentication
            JwtOptionModel.Secret = config["9"]!;
            JwtOptionModel.Issuer = config["10"]!;
            JwtOptionModel.Audience = config["11"]!;

            GoogleOptionModel.ClientId = config["12"]!;
            GoogleOptionModel.ClientSecret = config["13"]!;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/User/Login"; // Đường dẫn tới trang đăng nhập
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = GoogleOptionModel.ClientId;
                googleOptions.ClientSecret = GoogleOptionModel.ClientSecret;
                googleOptions.SaveTokens = true;
                // Yêu cầu thêm các thông tin từ Google
                //googleOptions.Scope.Add("profile");
                googleOptions.ClaimActions.MapJsonKey("urn:google:picture", "picture");
                googleOptions.ClaimActions.MapJsonKey("urn:google:email_verified", "email_verified");
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
                    RoleClaimType = ClaimTypes.Role,
                };
            });

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option => {
            //    option.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidateLifetime = true,
            //        ValidIssuer = config["ApiSettings:JwtOptions:Issuer"],
            //        ValidAudience = config["ApiSettings:JwtOptions:Audience"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["ApiSettings:JwtOptions:Secret"]!)),
            //        LifetimeValidator = (before, expires, token, param) =>
            //        {
            //            return expires > DateTime.UtcNow;
            //        },
            //        RoleClaimType = ClaimTypes.Role
            //    };

            //});

            #endregion

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
            var client = new MongoClient(config["1"]! + "?connect=replicaSet");
            //Create or get if database exists
            //client.DropDatabase("UserDb");
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
