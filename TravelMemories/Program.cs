using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using TravelMemories.Contracts.Storage;
using TravelMemories.Database;
using TravelMemories.Utilities.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IImageCompressService, ImageCompressService>();

builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"],
});

builder.Services.AddDbContext<ImageMetadataDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ImageSqlServer"]);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("TMFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://192.168.1.4:5173", "https://memories.harshjain17.com/", "https://lemon-moss-0ef6b9200.4.azurestaticapps.net\r\n").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
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

app.UseCors("TMFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
