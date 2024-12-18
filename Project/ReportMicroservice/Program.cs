using Azure.Identity;
using Microsoft.OpenApi.Models;
using ReportMicroservice.Configure;
using Azure.Identity;
using ReportMicroservice.Repostories;
using RabbitMQ.Client;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// S? d?ng Managed Identity ?? c?u h�nh Azure Key Vault
/*builder.Configuration.AddAzureKeyVault(
    new Uri("https://duantotnghiep.vault.azure.net/"),
        new ManagedIdentityCredential()

);*/
// S? d?ng Managed Identity ?? c?u h�nh Azure Key Vault c?c b?
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
// ??ng k� c�c wrapper types
builder.Services.AddSingleton<RabbitMQServer3ConnectionFactory>();
// ??ng k� IConnection t? m?i Server
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<RabbitMQServer3ConnectionFactory>().Factory;
    return factory.CreateConnection();
});

// ??ng k� IModel (Channel) n?u c?n
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
