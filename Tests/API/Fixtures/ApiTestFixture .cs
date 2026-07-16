using API;
using API.Filters;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Crypt = BCrypt.Net.BCrypt;

namespace Tests.API.Fixtures
{
    /// <summary>
    /// Fixture para pruebas de integración de API
    /// </summary>
    public class ApiTestFixture : WebApplicationFactory<Startup>, IDisposable
    {
        public HttpClient Client { get; private set; }
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _databaseName;
        public ApplicationDbContext DbContext { get; private set; }
        public IServiceScope ServiceScope { get; private set; }
        public int TestUserId { get; private set; }

        public ApiTestFixture()
        {
            // Generar un nombre único para CADA instancia del fixture
            _databaseName = $"TestDb_{Guid.NewGuid()}";

            // Crear el cliente HTTP
            Client = CreateClient();

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Obtener el DbContext para las pruebas
            ServiceScope = Services.CreateScope();
            DbContext = ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Eliminar y recrear la base de datos para cada ejecución
            ClearDatabase();

            Console.WriteLine("🚀 Creando ApiTestFixture...");
            Console.WriteLine($"✅ Client creado. BaseAddress: {Client.BaseAddress}");
            Console.WriteLine($"✅ API levantada en memoria.");
            Console.WriteLine($"✅ TestUserId: {TestUserId}");
        }

        private void SeedTestUser()
        {
            UserInfo userInfo = new UserInfo("TestUser", "test@email.com");
            User user = new User(userInfo, Crypt.HashPassword("Password123!"));

            DbContext.Users.Add(user);
            DbContext.SaveChanges();

            // Guardar el Id real que EF Core asignó
            TestUserId = user.Id;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices
            (
                services =>
                {
                    ConfigureDatabase(services);

                    ConfigureAutomapper(services);

                    ConfigureFilters(services);

                    ConfigureCurrentUserService(services);

                    ConfigureAuthentication(services);

                    ConfigureJwt(services);
                }
            );

            // Forzar que el entorno sea "Testing" o "Production"
            builder.UseEnvironment("Testing");
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            // Eliminar la configuración de DbContext existente
            ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Usar InMemory Database para pruebas
            services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase(_databaseName); });
        }

        private void ConfigureAutomapper(IServiceCollection services)
        {
            // 🔥 Eliminar cualquier configuración existente de AutoMapper
            ServiceDescriptor? autoMapperDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));

            if (autoMapperDescriptor != null)
            {
                services.Remove(autoMapperDescriptor);
            }

            // 🔥 Registrar IMapper manualmente
            services.AddSingleton<IMapper>
            (
                sp =>
                {
                    MapperConfiguration config = new MapperConfiguration
                    (
                        cfg =>
                        {
                            cfg.AddProfile<AutoMapperProfile>();
                        },
                        new LoggerFactory()
                    );
                    return config.CreateMapper();
                }
            );
        }

        private void ConfigureCurrentUserService(IServiceCollection services)
        {
            // Reemplazar ICurrentUserService por la implementación falsa
            ServiceDescriptor? currentUserDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserService));

            if (currentUserDescriptor != null)
            {
                services.Remove(currentUserDescriptor);
            }

            // Registra FakeCurrentUserService como un Singleton para poder pasarle el fixture
            services.AddSingleton<ICurrentUserService>(sp => { return new FakeCurrentUserService(this); });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            // Registrar autenticación falsa para el entorno Testing
            services.AddAuthentication("FakeAuthentication").AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>("FakeAuthentication", null);

            // Forzar que el esquema falso sea el predeterminado
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "FakeAuthentication";
                options.DefaultChallengeScheme = "FakeAuthentication";
            });
        }

        private void ConfigureJwt(IServiceCollection services)
        {
            services.AddScoped<IJwtSettings>
            (
                provider =>
                {
                    return new JwtSettings(
                        key: "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6",
                        issuer: "https://fake-issuer.com",
                        audience: "https://fake-audience.com",
                        expirationInMinutes: 60
                    );
                }
            );
        }

        private void ConfigureFilters(IServiceCollection services)
        {
            // 🔥 Registrar el filtro de validación en el entorno de pruebas
            services.AddScoped<ValidationFilter>();

            // Asegurar que el filtro de validación está registrado en el pipeline
            services.Configure<MvcOptions>(options =>
            {
                // Eliminar filtros existentes del mismo tipo
                ValidationFilter? existingFilter = options.Filters
                    .OfType<ValidationFilter>()
                    .FirstOrDefault();

                if (existingFilter != null)
                {
                    options.Filters.Remove(existingFilter);
                }

                // Agregar el filtro de validación
                options.Filters.Add<ValidationFilter>();
            });
        }


        // 🔥 Método para serializar requests
        public StringContent SerializeRequest<T>(T request) => new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");

        // 🔥 Método para deserializar responses
        public T? DeserializeResponse<T>(string json) => JsonSerializer.Deserialize<T>(json, _jsonOptions);
        
        // 🔥 Método para limpiar la base de datos
        public void ClearDatabase()
        {
            DbContext.ChangeTracker.Clear();

            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

            SeedTestUser();
        }

        public void Dispose()
        {
            Client?.Dispose();
            ServiceScope?.Dispose();
            DbContext?.Dispose();
        }
    }
}