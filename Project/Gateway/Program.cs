using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Azure.Identity;
using Microsoft.OpenApi.Models;
using Gateway.Models;
using Ocelot.Values;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Configuration.AddAzureKeyVault(new Uri("https://duantotnghiep.vault.azure.net/"),
    new DefaultAzureCredential());

JwtOptions.Secret = builder.Configuration["9"]!;
JwtOptions.Issuer = builder.Configuration["10"]!;
JwtOptions.Audience = builder.Configuration["11"]!;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Để dev test, disable HTTPS
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtOptions.Issuer,
                    ValidAudience = JwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
            });
builder.Services.Configure<FormOptions>(options =>
{
	options.MultipartBodyLengthLimit = 104857600; // Giới hạn tối đa (ví dụ: 100 MB)
});
builder.WebHost.ConfigureKestrel(options =>
{
	options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    loggingBuilder.AddFile("Logs/ocelotlog.txt"); // Nếu bạn đang sử dụng logging file
});


// Thêm Ocelot

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("ocelot.json").Build();
builder.Services.AddOcelot(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();
await app.UseOcelot();
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
