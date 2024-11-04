using AutoMapper;
using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories;
using CategoryMicroservice.Repositories.Interfaces;
using CategoryMicroservice.Repositories.Services;

namespace CategoryMicroservice.Configure
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Register DI
        /// </summary>
        /// <param name="services"></param>
        /// <returns>service in IServiceCollection</returns>
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            //Register DI here ⬇️

            services.AddScoped<ICategoryService, CategoryService>();

            // Register the BackgroundService
            services.AddHostedService<Background>();
            services.AddSingleton<IMessage, Message>();
            services.AddScoped<CategoryDeleteResponse>();
            services.AddScoped<IHelperService, HelperService>();

            //Register DI here ⬆️

            return services;
        }
    }
}
