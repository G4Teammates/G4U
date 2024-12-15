using Azure.Identity;
using CommentMicroservice.Configure;
using CommentMicroservice.DBContexts;
using CommentMicroservice.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// S? d?ng Managed Identity ?? c?u hình Azure Key Vault
/*builder.Configuration.AddAzureKeyVault(
    new Uri("https://duantotnghiep.vault.azure.net/"),
        new ManagedIdentityCredential()

);*/
// S? d?ng Managed Identity ?? c?u hình Azure Key Vault c?c b?
builder.Configuration.AddAzureKeyVault(
    new Uri("https://duantotnghiep.vault.azure.net/"),
        new VisualStudioCredential()

);
builder.Services.AddStartupService(builder.Configuration);

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
builder.Services.AddSingleton<RabbitMQServer3ConnectionFactory>();
// Đăng ký IConnection từ mỗi Server
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
