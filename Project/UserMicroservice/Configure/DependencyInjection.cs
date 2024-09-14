namespace UserMicroService.Configure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            //services.AddScoped<IUserService, UserService>();
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IUserMapper, UserMapper>();
            return services;
        }
    }
}
