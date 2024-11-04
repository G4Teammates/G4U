using AutoMapper;
using CommentMicroservice.Repositories;

namespace CommentMicroservice.Configure
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

            services.AddScoped<IRepoComment, RepoComment>();

            //Register DI here ⬆️

            return services;
        }
    }
}
