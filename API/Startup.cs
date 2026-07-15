using API.Filters;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Application.Validators;
using Domain.Exceptions;
using FluentValidation;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json;

namespace API
{
    /// <summary>
    /// Clase de configuración de la aplicación
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor de Startup
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="environment">Entorno de ejecución</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Configura los servicios de la aplicación (Dependency Injection)
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // ==================== INFRASTRUCTURE ====================
            ConfigureInfrastructure(services);

            // ==================== APPLICATION ====================
            ConfigureApplication(services);

            // ==================== CONTROLLERS ====================
            ConfigureControllers(services);

            // ==================== SWAGGER (Opcional) ====================
            ConfigureSwagger(services);

            // ==================== CORS (Opcional) ====================
            ConfigureCors(services);

            if (!_environment.IsEnvironment("Testing"))
            {
                // ==================== JWT AUTHENTICATION ====================
                ConfigureAuthentication(services);
            }
        }

        /// <summary>
        /// Configura el pipeline de la aplicación
        /// </summary>
        /// <param name="app">Application Builder</param>
        public void Configure(IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // Desarrollo
            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI
                (
                    options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Budget API v1");
                    }
                );
            }
            else
            {
                // Manejo de excepciones
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                        Exception? exception = exceptionFeature?.Error;

                        if (exception != null)
                        {
                            int statusCode = exception switch
                            {
                                KeyNotFoundException => StatusCodes.Status404NotFound,
                                ArgumentException => StatusCodes.Status400BadRequest,
                                InvalidOperationException => StatusCodes.Status400BadRequest,
                                ConflictException => StatusCodes.Status409Conflict,
                                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                                _ => StatusCodes.Status500InternalServerError
                            };

                            context.Response.StatusCode = statusCode;
                            context.Response.ContentType = "application/json";

                            ErrorResponse errorResponse = new ErrorResponse
                            {
                                StatusCode = statusCode,
                                Message = exception.Message,
                                Detail = exception.StackTrace,
                                Timestamp = DateTime.UtcNow
                            };

                            string jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });

                            await context.Response.WriteAsync(jsonResponse);
                        }
                    });
                });
            }

            // HTTPS
            app.UseHttpsRedirection();

            // Routing
            app.UseRouting();

            // CORS
            app.UseCors("AllowAll");

            // Asegurar que UseAuthentication está ANTES de UseAuthorization
            app.UseAuthentication();

            // Autorización (cuando se implemente)
            app.UseAuthorization();

            // ==================== ENDPOINTS ====================
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            // ==================== CREAR BASE DE DATOS ====================
            EnsureDatabaseCreated(app);
        }

        // ==================== MÉTODOS PRIVADOS DE CONFIGURACIÓN ====================

        /// <summary>
        /// Configura los servicios de Infrastructure
        /// </summary>
        private void ConfigureInfrastructure(IServiceCollection services)
        {
            // Obtener el tipo de base de datos desde configuración
            string? databaseTypeString = _configuration["DatabaseType"];

            if (string.IsNullOrEmpty(databaseTypeString)) throw new InvalidOperationException("DatabaseType not configured in appsettings.json");

            // Convertir a enum
            bool parseSuccess = Enum.TryParse<DatabaseType>(databaseTypeString, true, out DatabaseType databaseType);

            if (!parseSuccess) throw new InvalidOperationException($"DatabaseType '{databaseTypeString}' is not valid");

            // Registrar servicios de Infrastructure
            services.AddInfrastructureServices(_configuration, databaseType);
        }

        /// <summary>
        /// Configura los servicios de Application
        /// </summary>
        private void ConfigureApplication(IServiceCollection services)
        {
            // Registrar servicios de aplicación
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IFixedExpenseService, FixedExpenseService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IUserService, UserService>();

            // 🔥 Registrar ICurrentUserService (siempre, independientemente del entorno)
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Registrar AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Registrar FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateBudgetRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateFixedExpenseRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateBudgetRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateCategoryRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateFixedExpenseRequestValidator>();
        }

        /// <summary>
        /// Configura los controladores
        /// </summary>
        private void ConfigureControllers(IServiceCollection services)
        {
            // Registrar el filtro de validación
            services.AddScoped<ValidationFilter>();

            services.AddControllers
            (
                options =>
                {
                        // Agregar filtro de validación global
                        options.Filters.Add<ValidationFilter>();
                }
            ).AddJsonOptions
            (
                options =>
                {
                    // Configurar serialización JSON
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = true;
                }
            );
        }

        /// <summary>
        /// Configura Swagger (Opcional)
        /// </summary>
        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc
                (
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Budget API",
                        Version = "v1",
                        Description = "API para control de gastos",
                        Contact = new OpenApiContact
                        {
                            Name = "Mario Cuevas",
                            Email = "linkgold@gmail.com"
                        }
                    }
                );

                // Opcional: incluir comentarios XML para mejor documentación
                // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // options.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// Configura CORS
        /// </summary>
        private void ConfigureCors(IServiceCollection services)
        {
            services.AddCors
            (
                options =>
                {
                    options.AddPolicy
                    (
                        "AllowAll",
                        policy =>
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                        }
                    );
                }
            );
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            IConfigurationSection jwtSection = _configuration.GetSection("Jwt");
            string? key = jwtSection["Key"];
            string? issuer = jwtSection["Issuer"];
            string? audience = jwtSection["Audience"];

            if
            (
            string.IsNullOrEmpty(key) ||
            string.IsNullOrEmpty(issuer) ||
            string.IsNullOrEmpty(audience)
            ) throw new InvalidOperationException("JWT configuration is incomplete");

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            services.AddAuthentication
            (
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }
            )
            .AddJwtBearer
            (
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = securityKey,
                        ClockSkew = TimeSpan.Zero // Elimina el margen de 5 minutos por defecto
                    };
                }
            );

            // ==================== AUTORIZACIÓN ====================

            services.AddAuthorization();

            // ==================== HTTP CONTEXT ACCESSOR ====================

            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// Asegura que la base de datos está creada
        /// </summary>
        private void EnsureDatabaseCreated(IApplicationBuilder app)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                IDbContextFactory factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory>();
                ApplicationDbContext dbContext = factory.CreateDbContext();

                // Crear la base de datos si no existe (SQLite)
                dbContext.EnsureDatabaseCreated();
            }
        }

        /// <summary>
        /// DTO para respuesta de error
        /// </summary>
        private class ErrorResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public string Detail { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}