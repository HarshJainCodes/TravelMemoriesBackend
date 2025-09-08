using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using TravelMemories.Contracts.Storage;
using TravelMemories.Database;
using TravelMemories.Utilities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TravelMemories.Utilities.Request;
using TravelMemories.Controllers.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IImageCompressService, ImageCompressService>();
builder.Services.AddScoped<IRequestContextProvider, RequestContextProvider>();
builder.Services.AddScoped<LoginController, LoginController>();

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
        policy.WithOrigins("https://localhost:5173", "http://192.168.1.4:5173", "https://memories.harshjain17.com", "https://lemon-moss-0ef6b9200.4.azurestaticapps.net", "https://harshjain17.com", "https://travelmemories.azurewebsites.net").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie(x => x.Cookie.Name = "travelMemoriestoken")
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["IssuerSigningKeySecretText"])),
        ValidateIssuer = false,
        ValidateAudience = false,
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["travelMemoriestoken"];
            return Task.CompletedTask;
        }
    };
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
