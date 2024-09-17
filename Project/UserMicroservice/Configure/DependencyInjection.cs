using AutoMapper;
using UserMicroservice.Repositories.IRepositories;
using UserMicroservice.Repositories.RepositoryService;

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
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IUserMapper, UserMapper>();

            //Register DI here ⬆️

            return services;
        }
    }
}
