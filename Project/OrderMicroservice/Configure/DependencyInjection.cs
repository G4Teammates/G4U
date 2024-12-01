using AutoMapper;
using OrderMicroservice.Models.Message;
using OrderMicroservice.Repositories.Interfaces;
using OrderMicroservice.Repositories.Services;

namespace OrderMicroservice.Configure
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

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IHelperService, HelperService>();
            services.AddHostedService<Background>();
            services.AddSingleton<IMessage, Message>();
            services.AddScoped<CheckPurchaseReceive>();
            //Register DI here ⬆️

            return services;
        }
    }
}
