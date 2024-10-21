using Azure.Identity;
using Client.Configure;
using Client.Repositories.Interfaces;

using Client.Repositories.Interfaces.Product;
using Client.Repositories.Interfaces.User;
using Client.Repositories.Services;
using Client.Repositories.Services.Product;

using Client.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureKeyVault(new Uri("https://duantotnghiep.vault.azure.net/"),
    new DefaultAzureCredential());

builder.Services.AddStartupService(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IRepoProduct, RepoProduct>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Đăng ký thư viện
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
StaticTypeApi.APIGateWay = builder.Configuration["ServiceUrls:APIGateWay"];
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRepoProduct, RepoProduct>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Địa chỉ để người dùng được chuyển hướng khi không được xác thực
        options.LogoutPath = "/logout"; // Địa chỉ để người dùng được chuyển hướng khi đăng xuất
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.UseCors("AllowAllOrigins");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
