using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Infraestructure.DataBase;
using MicroServiceProduct.Infraestructure.Repository;
using MicroServiceProduct.Domain.Services;
using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Infraestructure.Messaging;


namespace MicroServiceProduct
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MicroServiceProduct API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Autenticación JWT usando el esquema Bearer. Ejemplo: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
            });
        }

        public static IServiceCollection AddJwtConfig(this IServiceCollection services, IConfiguration config, bool isDevelopment)
        {
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            var key = config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Falta configuración JWT en appsettings.json.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = !isDevelopment;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("MicroServiceProduct") ?? config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(conn)) throw new InvalidOperationException("Falta ConnectionString.");

            services.AddSingleton<IDataBase>(_ => DataBaseConnection.GetInstance(conn));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddSingleton<IEventPublisher, RabbitPublisher>();
            services.AddHostedService<RabbitConsumerForProduct>();

            return services;
        }
    }
}
