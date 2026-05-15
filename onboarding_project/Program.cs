using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using startup_project.Common;
using startup_project.Data;
using startup_project.Models;
using startup_project.Models.Enums;
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
            // Simple approach: sign the token with the key, put userId inside.
            // On each request the middleware decrypts the token using the same key and extracts userId.
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Required configuration 'Jwt:Key' is missing.");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,          // verify the key is correct
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateLifetime = true,                  // reject expired tokens
                        ValidateIssuer = false,                   // no issuer needed
                        ValidateAudience = false                  // no audience needed
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
                c.UseAllOfToExtendReferenceSchemas();
                c.UseInlineDefinitionsForEnums();

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

                // Include entity/table models in Swagger schemas
                c.DocumentFilter<EntityModelsDocumentFilter>();
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

    /// <summary>
    /// Document filter that ensures all request, response, and entity schemas are present in Swagger.
    /// Previously this filter deleted non-entity schemas, which caused request bodies to appear blank.
    /// Now it only adds the extra entity types that aren't auto-discovered via controller signatures.
    /// </summary>
    public class EntityModelsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Force-register entity/table models that are not directly referenced by any
            // controller action signature (so Swagger wouldn't auto-discover them otherwise).
            context.SchemaGenerator.GenerateSchema(typeof(User), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(Restaurant), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(MenuItem), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(Order), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(OrderItem), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(Cart), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(CartItem), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(OrderStatus), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(UserRole), context.SchemaRepository);

            // Do NOT remove any schemas — all request/response ViewModels must stay so that
            // Swagger can render the correct "Request body" and "Responses" sections.
        }
    }
}
