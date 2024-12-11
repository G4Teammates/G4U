using Client.Repositories.Interfaces;
using Client.Repositories.Interfaces.Authentication;
using Client.Repositories.Interfaces.Categories;
using Client.Repositories.Interfaces.Comment;
using Client.Repositories.Interfaces.Order;
using Client.Repositories.Interfaces.Reports;
using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.Stastistical;
using Client.Repositories.Interfaces.User;
using Client.Repositories.Services;
using Client.Repositories.Services.Authentication;
using Client.Repositories.Services.Categories;
using Client.Repositories.Services.Comment;
using Client.Repositories.Services.Order;
using Client.Repositories.Services.Reports;
using Client.Repositories.Services.Product;
using Client.Repositories.Services.Stastistical;
using Client.Repositories.Services.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;


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
            services.AddScoped<IExportService, ExportService>();

            services.AddScoped<IReportsService, ReportsService>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRepoProduct, RepoProduct>();
            services.AddScoped<ICategoriesService, CategoriesService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IRepoStastistical, RepoStastistical>();

            services.AddHttpClient<IRepoProduct, RepoProduct>();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddRazorComponents();
            services.AddControllersWithViews();


            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout as needed
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // Giới hạn tối đa (ví dụ: 100 MB)
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login"; // Địa chỉ để người dùng được chuyển hướng khi không được xác thực
                options.LogoutPath = "/logout"; // Địa chỉ để người dùng được chuyển hướng khi đăng xuất
            });
            //Register DI here ⬆️

            return services;
        }
    }
}
