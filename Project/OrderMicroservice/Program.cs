using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderMicroservice.Configure;
using OrderMicroservice.DBContexts;
using OrderMicroservice.Models.UserModel;
using OrderMicroservice.Repositories.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.   

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 
builder.Services.AddHostedService<TempFileCleaner>();
builder.Services.AddHttpContextAccessor();

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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
