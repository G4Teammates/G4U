using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductMicroservice.Configure;
using ProductMicroservice.DBContexts;
using ProductMicroservice.Repostories;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Sử dụng Managed Identity để cấu hình Azure Key Vault
/*builder.Configuration.AddAzureKeyVault(
    new Uri("https://duantotnghiep.vault.azure.net/"),
        new ManagedIdentityCredential()

);*/
// Sử dụng Managed Identity để cấu hình Azure Key Vault cục bộ
builder.Configuration.AddAzureKeyVault(
    new Uri("https://duantotnghiep.vault.azure.net/"),
        new VisualStudioCredential()

);
builder.Services.AddStartupService(builder.Configuration);
builder.Services.AddMemoryCache();  // Đăng ký IMemoryCache
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
// Đăng ký các wrapper types
builder.Services.AddSingleton<RabbitMQServer1ConnectionFactory>();
builder.Services.AddSingleton<RabbitMQServer2ConnectionFactory>();
builder.Services.AddSingleton<RabbitMQServer3ConnectionFactory>();
// Đăng ký IConnection từ mỗi Server
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<RabbitMQServer1ConnectionFactory>().Factory;
    return factory.CreateConnection();
});
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<RabbitMQServer2ConnectionFactory>().Factory;
    return factory.CreateConnection();
});

builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<RabbitMQServer3ConnectionFactory>().Factory;
    return factory.CreateConnection();
});

// Đăng ký IModel (Channel) nếu cần
builder.Services.AddSingleton(sp =>
{
    var connection = sp.GetRequiredService<IConnection>(); // Server1 Connection
    return connection.CreateModel();
});
builder.Services.AddSingleton(sp =>
{
    var connection = sp.GetRequiredService<IConnection>(); // Server2 Connection
    return connection.CreateModel();
});
builder.Services.AddSingleton(sp =>
{
    var connection = sp.GetRequiredService<IConnection>(); // Server3 Connection
    return connection.CreateModel();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
