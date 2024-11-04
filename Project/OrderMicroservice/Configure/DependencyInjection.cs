using AutoMapper;
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
            services.AddScoped<IMessage, Message>();
            //Register DI here ⬆️

            return services;
        }
    }
}
