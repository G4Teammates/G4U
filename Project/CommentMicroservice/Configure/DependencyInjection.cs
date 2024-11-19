using AutoMapper;
using CommentMicroservice.Models.Message;
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
            services.AddHostedService<Background>();
            services.AddSingleton<IMessage, Message>();
            services.AddScoped<CheckPurchasedResponse>();
            //Register DI here ⬆️

            return services;
        }
    }
}
