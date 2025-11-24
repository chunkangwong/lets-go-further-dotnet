using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Asp.Versioning;
using controller_api_test.src.Factories;
using controller_api_test.src.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder.AddRuntimeInstrumentation()
                      .AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation()
                      .AddPrometheusExporter();
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    var trustedOrigins = builder.Configuration["CORS:TrustedOrigins"]!;
    options.AddPolicy("TrustedOrigins", policy =>
    {
        policy.WithOrigins(trustedOrigins.Split(',')) // replace with your trusted origins
              .WithHeaders("Authorization", "Content-Type")
              .WithMethods("POST", "PUT", "PATCH", "DELETE")
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    // Read rate limiter settings from config
    var rps = builder.Configuration.GetValue<int>("RateLimiting:Rps", 10);        // default 10
    var burst = builder.Configuration.GetValue<int>("RateLimiting:Burst", 0);     // default 0
    var windowSeconds = builder.Configuration.GetValue<int>("RateLimiting:WindowSeconds", 60); // default 60s

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = rps,
                QueueLimit = burst,
                Window = TimeSpan.FromSeconds(windowSeconds)
            }));
});

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
    var jwtAudience = builder.Configuration["Jwt:Audience"]!;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // --- Add this section to handle custom responses ---
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Fired when an [Authorize] action is hit, but no valid token is provided (401 Unauthorized)
            context.HandleResponse(); // Prevent the default response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var response = new
            {
                status = 401,
                message = "Unauthorized"
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        },
        OnForbidden = context =>
        {
            // Fired when a valid token is provided, but the user does not meet the Authorization policy (403 Forbidden)
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            var response = new
            {
                status = 403,
                message = "Forbidden"
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    };
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, EmailConfirmedUserClaimsPrincipalFactory>();

// Add this configuration before builder.Build()
var authorizationBuilder = builder.Services.AddAuthorizationBuilder();

// 1. Set the DefaultPolicy to use the JWT Bearer scheme
authorizationBuilder.SetDefaultPolicy(new AuthorizationPolicyBuilder(
    JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser()
    .Build());

// 2. Define your existing named policies and explicitly add the JWT scheme
authorizationBuilder.AddPolicy("EmailConfirmed", policy =>
    policy.RequireClaim("email_confirmed", "true")
          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

authorizationBuilder.AddPolicy("MoviesRead", policy =>
    policy.RequireClaim("permission", "movies:read")
          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

authorizationBuilder.AddPolicy("MoviesWrite", policy =>
    policy.RequireClaim("permission", "movies:write")
          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPrometheusScrapingEndpoint();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCors("TrustedOrigins");
app.UseRateLimiter();
app.MapControllers();

app.Run();
