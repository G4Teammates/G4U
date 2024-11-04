using StatisticalMicroservice.Repostories;

namespace StatisticalMicroservice.Configure
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
            services.AddScoped<IRepo, Repo>();

            //Register DI here ⬆️

            return services;
        }
    }
}
