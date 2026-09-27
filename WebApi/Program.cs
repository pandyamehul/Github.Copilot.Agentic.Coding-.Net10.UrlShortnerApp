using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UrlTrimmer.WebApi.Contracts;
using UrlTrimmer.WebApi.Data;
using UrlTrimmer.WebApi.Models;
using UrlTrimmer.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5044")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var clerkAuthority = builder.Configuration["Clerk:Authority"];
if (string.IsNullOrWhiteSpace(clerkAuthority))
{
    throw new InvalidOperationException("Clerk:Authority must be configured for server-side token validation.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = clerkAuthority;
        options.Audience = builder.Configuration["Clerk:Audience"];
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token))
                {
                    context.Token = context.Request.Headers["X-Clerk-Session-Token"].FirstOrDefault();
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ClerkAuthentication");
                logger.LogInformation("Bearer token received: {HasToken}.", !string.IsNullOrWhiteSpace(context.Token));
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ClerkAuthentication");
                logger.LogWarning(context.Exception, "Clerk token validation failed.");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ClerkAuthentication");
                logger.LogInformation("Clerk token validated. Authenticated: {Authenticated}; Subject: {Subject}.",
                    context.Principal?.Identity?.IsAuthenticated,
                    context.Principal?.FindFirst("sub")?.Value);
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("UrlShortenerDb"));
});

builder.Services.AddScoped<UrlCodeGenerator>();

var app = builder.Build();

app.UseCors("WebApp");
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization") &&
        context.Request.Headers.TryGetValue("X-Clerk-Session-Token", out var sessionToken) &&
        !string.IsNullOrWhiteSpace(sessionToken))
    {
        context.Request.Headers.Authorization = $"Bearer {sessionToken}";
    }

    await next();
});
app.UseAuthentication();
app.UseAuthorization();

await DatabaseInitializer.InitializeAsync(app.Services);

UrlTrimmer.WebApi.EndpointMappings.Map(app);

await app.RunAsync();