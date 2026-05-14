using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using startup_project.Common;
using startup_project.Data;
using startup_project.Services;

namespace startup_project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Database ---
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // --- Distributed cache: Redis when credentials are set, otherwise in-memory ---
            // Prefer Redis:ConnectionString; fall back to ConnectionStrings:Redis for compatibility.
            var redisConnection =
                builder.Configuration["Redis:ConnectionString"]
                ?? builder.Configuration.GetConnectionString("Redis");

            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "QuickBite:";
                });
                Console.WriteLine("📦 Distributed cache: Redis (StackExchange.Redis).");
            }
            else
            {
                builder.Services.AddDistributedMemoryCache();
                Console.WriteLine("📦 Distributed cache: in-memory (set Redis:ConnectionString or ConnectionStrings:Redis to use Redis).");
            }

            // --- JWT Authentication ---
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Required configuration 'Jwt:Key' is missing.");
            _ = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Required configuration 'Jwt:Issuer' is missing.");
            _ = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Required configuration 'Jwt:Audience' is missing.");
            if (!double.TryParse(builder.Configuration["Jwt:ExpiryMinutes"], out _))
                throw new InvalidOperationException("Required configuration 'Jwt:ExpiryMinutes' is missing or not a valid number.");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            builder.Services.AddAuthorization();

            // --- Application Services ---
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<RestaurantService>();
            builder.Services.AddScoped<MenuItemService>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddScoped<OrderService>();

            // --- Global exception handler ---
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // --- Controllers ---
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // --- Swagger with JWT Bearer support ---
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuickBite API", Version = "v1" });

                // Allows pasting Bearer token in Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token. Example: eyJhbGci..."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            // --- Verify DB connection on startup ---
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    await db.Database.CanConnectAsync();
                    Console.WriteLine("✅ Database connection successful.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Database connection failed: {ex.Message}");
                    // App still starts — don't block dev startup if DB is temporarily unavailable
                }
            }

            // --- Middleware Pipeline ---
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickBite API v1"));
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
