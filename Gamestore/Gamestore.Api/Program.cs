using System.Text;
using Gamestore.Api.Auth;
using Gamestore.Api.Configuration;
using Gamestore.Api.Logging;
using Gamestore.Api.Middleware;
using Gamestore.Api.Services;
using Gamestore.BLL.Services;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories;
using Gamestore.Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

DotEnvLoader.Load(
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env"));


var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFile();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GamestoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IGameDealsService, GameDealsService>();
builder.Services.AddScoped<IDiscountSimulationService, DiscountSimulationService>();
builder.Services.AddScoped<IDiscountNotificationService, LoggingDiscountNotificationService>();

builder.Services.AddScoped<IAuthManagementService, AuthManagementService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddSingleton(builder.Configuration.GetSection("OrderSettings").Get<OrderSettings>() ?? new OrderSettings());
builder.Services.AddSingleton(builder.Configuration.GetSection("PaymentGatewaySettings").Get<PaymentGatewaySettings>() ?? new PaymentGatewaySettings());
builder.Services.AddSingleton(builder.Configuration.GetSection("DiscountPolling").Get<DiscountPollingOptions>() ?? new DiscountPollingOptions());
builder.Services.AddHostedService<DiscountPollingBackgroundService>();
builder.Services.AddScoped<IPaymentGatewayClient, PaymentGatewayClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("GameStoreFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("x-total-numbers-of-games");
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthManagementService>();
    await authService.EnsureDefaultsAsync();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors("GameStoreFrontend");
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseMiddleware<GameCountMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
