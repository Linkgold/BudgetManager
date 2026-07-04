using API;
using API.Filters;
using Application.Mappings;
using Application.Validators;
using AutoMapper;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

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

        public ApiTestFixture()
        {
            // 🔥 Generar un nombre único para CADA instancia del fixture
            _databaseName = $"TestDb_{Guid.NewGuid()}";

            // Crear el cliente HTTP
            Client = CreateClient();

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Obtener el DbContext para las pruebas
            ServiceScope = Services.CreateScope();
            DbContext = ServiceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 🔥 Eliminar y recrear la base de datos para cada ejecución
            ClearDatabase();

            Console.WriteLine("🚀 Creando ApiTestFixture...");

            Client = CreateClient();

            Console.WriteLine($"✅ Client creado. BaseAddress: {Client.BaseAddress}");
            Console.WriteLine($"✅ API levantada en memoria.");
        }

        // 🔥 Método para serializar requests
        public StringContent SerializeRequest<T>(T request) => new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");

        // 🔥 Método para deserializar responses
        public T? DeserializeResponse<T>(string json) => JsonSerializer.Deserialize<T>(json, _jsonOptions);
        
        // 🔥 Método para limpiar la base de datos
        public void ClearDatabase()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices
            (
                services =>
                {
                    // Eliminar la configuración de DbContext existente
                    ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Usar InMemory Database para pruebas
                    services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase(_databaseName); });

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

                    // ==================== FILTROS ====================

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
            );

            // 🔥 Forzar que el entorno sea "Testing" o "Production"
            builder.UseEnvironment("Testing");
        }

        public void Dispose()
        {
            Client?.Dispose();
            ServiceScope?.Dispose();
            DbContext?.Dispose();
        }
    }
}