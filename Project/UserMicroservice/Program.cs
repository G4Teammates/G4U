using Azure.Identity;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Text.Json.Serialization;
using UserMicroservice.Repositories;
using UserMicroService.Configure;

var builder = WebApplication.CreateBuilder(args);

//Get connect string
//builder.Configuration.AddAzureKeyVault(new Uri("https://duantotnghiep.vault.azure.net/"),
//    new DefaultAzureCredential());

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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
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
builder.Services.AddHttpContextAccessor(); // Đăng ký IHttpContextAccessor

// Đăng ký các wrapper types
builder.Services.AddSingleton<RabbitMQServer2ConnectionFactory>();
builder.Services.AddSingleton<RabbitMQServer3ConnectionFactory>();
// Đăng ký IConnection từ mỗi Server
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
    var connection = sp.GetRequiredService<IConnection>(); // Server1 Connection
    return connection.CreateModel();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
