using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BackEnd.Models;
using BackEnd.Data;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();

// AutoMapper-ის რეგისტრაცია
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<BackEnd.Mappings.MappingProfile>();
});

// Repositories-ის რეგისტრაცია
builder.Services.AddScoped(typeof(BackEnd.Interfaces.IGenericRepository<>), typeof(BackEnd.Repositories.GenericRepository<>));
builder.Services.AddScoped<BackEnd.Interfaces.IHotelRepository, BackEnd.Repositories.HotelRepository>();
builder.Services.AddScoped<BackEnd.Interfaces.IRoomRepository, BackEnd.Repositories.RoomRepository>();
builder.Services.AddScoped<BackEnd.Interfaces.IGuestRepository, BackEnd.Repositories.GuestRepository>();
builder.Services.AddScoped<BackEnd.Interfaces.IReservationRepository, BackEnd.Repositories.ReservationRepository>();
builder.Services.AddScoped<BackEnd.Interfaces.IManagerRepository, BackEnd.Repositories.ManagerRepository>();

// Services-ის რეგისტრაცია
builder.Services.AddScoped<BackEnd.Interfaces.IHotelService, BackEnd.Services.HotelService>();
builder.Services.AddScoped<BackEnd.Interfaces.IRoomService, BackEnd.Services.RoomService>();
builder.Services.AddScoped<BackEnd.Interfaces.IGuestService, BackEnd.Services.GuestService>();
builder.Services.AddScoped<BackEnd.Interfaces.IReservationService, BackEnd.Services.ReservationService>();
builder.Services.AddScoped<BackEnd.Interfaces.IManagerService, BackEnd.Services.ManagerService>();
builder.Services.AddScoped<BackEnd.Interfaces.IAuthService, BackEnd.Services.AuthService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Swagger-ს ვეუბნებით, რომ ვიყენებთ Bearer ტოკენს
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "აქ ჩაწერეთ: Bearer {თქვენი_ტოკენი}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// DbContext-ის რეგისტრაცია SQL Server-ით
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Identity-ს რეგისტრაცია
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

var app = builder.Build();

// Seed roles, demo logins and sample data on startup (only inserts what's missing).
// Wrapped so a transient DB hiccup at boot doesn't crash the whole app - seeding can
// always be re-run later via POST /api/Seed.
using (var scope = app.Services.CreateScope())
{
    try
    {
        await BackEnd.Data.DataSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Data seeding failed at startup; continuing. Use POST /api/Seed to retry.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ერორების დამჭერი
app.UseMiddleware<BackEnd.Middlewares.ExceptionMiddleware>();

// NOTE: No HTTPS redirect here on purpose.
// This API is consumed by the WinForms desktop client. If we redirect HTTP -> HTTPS,
// HttpClient follows the redirect but strips the "Authorization" header on the
// cross-scheme hop, so the bearer token is lost and authenticated calls get 401.
// The client already talks to both http://localhost:5126 and https://localhost:7003,
// so we serve both schemes directly and let the token through untouched.

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();