using ProductMicroservice.Repostories;
using ProductMicroservice.Repostories.Helper;
using ProductMicroservice.Repostories.Messages;

namespace ProductMicroservice.Configure
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

            services.AddHttpClient<IRepoProduct, RepoProduct>();
            services.AddScoped<IRepoProduct, RepoProduct>();
            services.AddScoped<IHelper, Helper>();

            // Register the BackgroundService
            services.AddHostedService<Background>();
            services.AddSingleton<IMessage, Message>();

            //Register DI here ⬆️

            return services;
        }
    }
}
