using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Configure;
using ProductMicroservice.DBContexts;
using ProductMicroservice.Repostories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddAzureKeyVault(new Uri("https://duantotnghiep.vault.azure.net/"),
    new DefaultAzureCredential());
builder.Services.AddStartupService(builder.Configuration);
builder.Services.AddHttpClient<IRepoProduct, RepoProduct>();
builder.Services.AddScoped<IRepoProduct, RepoProduct>();


// Register the BackgroundService
builder.Services.AddHostedService<Background>();
builder.Services.AddSingleton<IMessage, Message>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
