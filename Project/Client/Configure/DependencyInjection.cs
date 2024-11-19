using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Order;
using Client.Repositories.Services;
using Client.Repositories.Services.Authentication;
using Client.Repositories.Services.Order;
using Client.Repositories.Services.Stastistical;


namespace Client.Configure
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
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<IHelperService, HelperService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();




            //Register DI here ⬆️

            return services;
        }
    }
}
