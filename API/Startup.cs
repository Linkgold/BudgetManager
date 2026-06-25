using API.Filters;
using API.Middleware;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Microsoft.OpenApi;

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
            if (services == null)                throw new ArgumentNullException(nameof(services));

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
        }

        /// <summary>
        /// Configura el pipeline de la aplicación
        /// </summary>
        /// <param name="app">Application Builder</param>
        public void Configure(IApplicationBuilder app)
        {
            if (app == null)                throw new ArgumentNullException(nameof(app));

            // ==================== MIDDLEWARE ====================

            // Manejo de excepciones
            app.UseMiddleware<ExceptionHandlingMiddleware>();

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

            // HTTPS
            app.UseHttpsRedirection();

            // Routing
            app.UseRouting();

            // CORS
            app.UseCors("AllowAll");

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

            // Registrar AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Registrar FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
        }

        /// <summary>
        /// Configura los controladores
        /// </summary>
        private void ConfigureControllers(IServiceCollection services)
        {
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
    }
}
