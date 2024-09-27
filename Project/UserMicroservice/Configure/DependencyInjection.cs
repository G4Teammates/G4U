using AutoMapper;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.Repositories.IRepositories;
using UserMicroservice.Repositories.RepositoryService;
using UserMicroservice.Repositories.Services;

namespace UserMicroService.Configure
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
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IHelperService, HelperService>();

            //Register DI here ⬆️

            return services;
        }
    }
}
