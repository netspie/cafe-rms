using System.Text;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Infrastructure;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Persistence.Interceptors;
using CafeRMS.Api.Persistence.Seeding;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditableSaveChangesInterceptor>();
builder.Services.AddScoped<SoftDeletableSaveChangesInterceptor>();
builder.Services.AddScoped<StartupSeeder>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention()
           .AddInterceptors(
               serviceProvider.GetRequiredService<AuditableSaveChangesInterceptor>(),
               serviceProvider.GetRequiredService<SoftDeletableSaveChangesInterceptor>()));

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>();

var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var authBuilder = builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(Policies.RequireGuest, p =>
        p.RequireClaim(ClaimsPrincipalExtensions.AccountTypeClaim, nameof(AccountType.Guest)))
    .AddPolicy(Policies.RequireStaff, p =>
        p.RequireClaim(ClaimsPrincipalExtensions.AccountTypeClaim, nameof(AccountType.Staff)))
    .AddPolicy(Policies.RequireSuperAdmin, p =>
        p.RequireClaim(ClaimsPrincipalExtensions.AccountTypeClaim, nameof(AccountType.SuperAdmin)))
    .AddPolicy(Policies.RequireStaffOrSuperAdmin, p =>
        p.RequireClaim(ClaimsPrincipalExtensions.AccountTypeClaim,
            nameof(AccountType.Staff), nameof(AccountType.SuperAdmin)));

foreach (var permission in Permissions.All)
    authBuilder.AddPolicy(permission, p => p.AddRequirements(new PermissionRequirement(permission)));

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddHealthChecks();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

const string corsPolicy = "ClientApps";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.EnvironmentName != "Testing")
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    if (app.Environment.IsDevelopment())
        await sp.GetRequiredService<AppDbContext>().Database.MigrateAsync();

    await sp.GetRequiredService<StartupSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
