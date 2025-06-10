using Hangfire;
using HG.CFDI.API.Jobs.TransferenciaCartaPorte;
//using HG.CFDI.API.Models;
using HG.CFDI.CORE.ContextFactory;
using HG.CFDI.CORE.Interfaces;
using HG.CFDI.DATA.LisApi;
using HG.CFDI.DATA.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog.Events;
using Serilog;
using HG.CFDI.SERVICE.Services;
using HG.CFDI.SERVICE;
using System.Text.Json;
using HG.CFDI.CORE.Models;
using System.Text.Json.Serialization;
using HG.CFDI.DATA;
using HG.CFDI.SERVICE.Services.Timbrado.ValidacionesSat;
using HG.CFDI.SERVICE.Services.Timbrado.Ryder;
using HG.CFDI.SERVICE.Services.ValidacionesSat;
using HG.CFDI.SERVICE.Services.Timbrado.Documentos;
using Interceptor.AOP.AspNetCore;
using CFDI.Data.Contexts;
using Ryder.Api.Client.DependencyInjection;
using Ryder.Api.Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Configuración avanzada de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "HG Transportaciones SA de CV - Timbrado unificado",
        Description = "Expone endpoints que timbran facturas carta porte en diversos PAC de facturacion (BuzonE, InvoiceOne)",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Julio Cesar Bautista",
            Email = "desarrollohg@hgtransportaciones.com",
            Url = new Uri("https://twitter.com/johndoe"),
        },
        License = new OpenApiLicense
        {
            Name = "Licencia MIT",
            Url = new Uri("https://example.com/license"),
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("timbradoIntegral");

//Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<CfdiDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configuración de Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;  
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30); // Intervalo de sondeo
});

builder.Services.AddMemoryCache();

// Configura AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Registra el cliente HTTP de Ryder (leyendo los defaults que tienes en la librería)
builder.Services.AddRyderApiClient();

//DbContextFactory
builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();//services
builder.Services.AddScoped<ICartaPorteService, CartaPorteService>();
builder.Services.AddScoped<IValidacionesSatService, ValidacionesSatService>();
builder.Services.AddScoped<IRyderService, RyderService>();
builder.Services.AddScoped<IUtilsService, UtilsService>();
builder.Services.AddScoped<ITrucksService, TrucksService>();
builder.Services.AddInterceptedTransient<IDocumentosService, DocumentosService>();
builder.Services.AddScoped<ICartaPorteRepository, CartaPorteRepository>();
builder.Services.AddScoped<ICartaPorteServiceApi, CartaPorteServiceApi>();
builder.Services.AddScoped<IRyderApiClient, RyderApiClient>();
builder.Services.AddScoped<ITipoCambioService, TipoCambioService>();
builder.Services.AddScoped<ITipoCambioRepository, TipoCambioRepository>();
builder.Services.AddTransient<ImportData>();
builder.Services.AddTransient<CartaPorteSender>();

//Appsettings
builder.Services.Configure<List<FirmaDigitalOptions>>(builder.Configuration.GetSection("FirmaDigital"));
builder.Services.Configure<InvoiceOneApiOptions>(builder.Configuration.GetSection("InvoiceOneApi"));
builder.Services.Configure<List<BuzonEApiCredential>>(builder.Configuration.GetSection("BuzonEApiCredentials"));
builder.Services.Configure<LisApiOptions>(builder.Configuration.GetSection("LisApi"));
builder.Services.Configure<RyderApiOptions>(builder.Configuration.GetSection("RyderApi"));
builder.Services.Configure<List<compania>>(builder.Configuration.GetSection("Companias"));


// Registro de HttpClient
builder.Services.AddHttpClient();

//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.WithOrigins("http://localhost:4200", "https://tools.apphgtransportaciones.com")
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});


builder.Services.AddControllers();

Log.Logger = new LoggerConfiguration()
    // Establecer el nivel mínimo de logging global
    .MinimumLevel.Information()

    // Sobrescribir niveles para categorías específicas
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Ignorar la mayoría de los logs de Microsoft
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Server.IIS", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Server.IISIntegration", LogEventLevel.Warning)
    .MinimumLevel.Override("Hangfire", LogEventLevel.Warning) // Filtrar logs de Hangfire

    // Enriquecer los logs
    .Enrich.FromLogContext()

    // Especificar los sinks
    .WriteTo.Console() // Para depuración
    .WriteTo.File("logs/TimbradoUnificado.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


#region esto se hace una vez, en el sistema se puede indicar la carga del certificado una vez y ya, no se hará en cada factura que se timbre
try
{
    // Ubicación del archivo appsettings.json
    string jsonFilePath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
    string jsonString = File.ReadAllText(jsonFilePath);

    // Deserializar el JSON a un objeto Dictionary para manipulación fácil
    var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

    // Acceder y modificar la sección 'FirmaDigital'
    if (settings != null && settings.TryGetValue("FirmaDigital", out var firmaDigitalArray))
    {
        var firmasDigitales = JsonSerializer.Deserialize<List<JsonElement>>(firmaDigitalArray.ToString());

        if (firmasDigitales != null)
        {
            for (int i = 0; i < firmasDigitales.Count; i++)
            {
                var firmaDigital = firmasDigitales[i];

                string empresa = firmaDigital.GetProperty("Empresa").GetString();
                string nombreCarpeta = firmaDigital.GetProperty("NombreCarpeta").GetString();
                string passPrivateKey = firmaDigital.GetProperty("PassPrivateKey").GetString();

                string nombreArchivoPrivateKey = firmaDigital.GetProperty("NombreArchivoPrivateKey").GetString();
                string nombreArchivoCadenaOriginal = firmaDigital.GetProperty("NombreArchivoCadenaOriginal").GetString();
                string nombreArchivoCertificado = firmaDigital.GetProperty("NombreArchivoCertificado").GetString();

                var patPrivateKey = Path.Combine(Directory.GetCurrentDirectory(), "archivosSat", nombreCarpeta, nombreArchivoPrivateKey);
                var pathCertificate = Path.Combine(Directory.GetCurrentDirectory(), "archivosSat", nombreCarpeta, nombreArchivoCertificado);
                var pathCadenaOriginalXslt = Path.Combine(Directory.GetCurrentDirectory(), "archivosSat", nombreCarpeta, nombreArchivoCadenaOriginal);

                CFDIHandler handler = new CFDIHandler();

                string numeroDeCertificado = handler.ObtenerNumeroDeCertificado(pathCertificate, passPrivateKey);

                var modifiedFirmaDigital = new
                {
                    Empresa = empresa,
                    NombreCarpeta = nombreCarpeta,
                    PassPrivateKey = passPrivateKey,
                    NumeroDeCertificado = numeroDeCertificado,
                    NombreArchivoPrivateKey = nombreArchivoPrivateKey,
                    NombreArchivoCadenaOriginal = nombreArchivoCadenaOriginal,
                    NombreArchivoCertificado = nombreArchivoCertificado
                };

                firmasDigitales[i] = JsonDocument.Parse(JsonSerializer.Serialize(modifiedFirmaDigital)).RootElement;
            }

            // Actualizar la sección 'FirmaDigital' en el diccionario principal
            settings["FirmaDigital"] = JsonDocument.Parse(JsonSerializer.Serialize(firmasDigitales)).RootElement;
        }
    }

    // Serializar el diccionario actualizado de nuevo a JSON
    string modifiedJsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

    // Guardar el archivo modificado
    File.WriteAllText(jsonFilePath, modifiedJsonString);

    Log.Information("El archivo appsettings.json se ha modificado correctamente.");
}
catch (Exception ex)
{
    Log.Error($"Error al modificar el archivo appsettings.json: {ex.Message}");
}
#endregion

builder.Host.UseSerilog();

var app = builder.Build();

app.UseCors();

//app.Use(async (context, next) =>
//{
//    var origin = context.Request.Headers["Origin"].ToString();
//    var userAgent = context.Request.Headers["User-Agent"].ToString();

//    var allowedOrigins = new[]
//    {
//        "http://localhost:4200",
//        "https://tools.apphgtransportaciones.com"
//    };

//    // Si tiene Origin, validamos contra los permitidos (navegador)
//    if (!string.IsNullOrEmpty(origin) && !allowedOrigins.Contains(origin))
//    {
//        context.Response.StatusCode = StatusCodes.Status403Forbidden;
//        await context.Response.WriteAsync("Forbidden: Origin not allowed");
//        return;
//    }

//    // Opcional: Bloquear por User-Agent patrones sospechosos (cliente .NET, curl, etc.)
//    if (string.IsNullOrEmpty(origin) && userAgent.Contains("HttpClient"))
//    {
//        context.Response.StatusCode = StatusCodes.Status403Forbidden;
//        await context.Response.WriteAsync("Forbidden: Non-browser clients are not allowed");
//        return;
//    }

//    await next();
//});

//app.Use(async (context, next) =>
//{
//    var origin = context.Request.Headers["Origin"].ToString();
//    var userAgent = context.Request.Headers["User-Agent"].ToString();

//    var allowedOrigins = new[]
//    {
//        "http://localhost:4200",
//        "https://tools.apphgtransportaciones.com"
//    };

//    var isBrowserRequest = !string.IsNullOrEmpty(origin);

//    // Si la solicitud es desde navegador, validamos el Origin
//    if (isBrowserRequest && !allowedOrigins.Contains(origin))
//    {
//        context.Response.StatusCode = StatusCodes.Status403Forbidden;
//        await context.Response.WriteAsync("Forbidden: Origin not allowed.");
//        return;
//    }

//    // Si la solicitud no tiene Origin (como apps .NET WinForms), las bloqueamos
//    if (!isBrowserRequest)
//    {
//        context.Response.StatusCode = StatusCodes.Status403Forbidden;
//        await context.Response.WriteAsync("Forbidden: Non-browser clients are not allowed.");
//        return;
//    }

//    await next();
//});


// Inyectar y utilizar IRecurringJobManager
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

// Después de construir la aplicación
var serviceProvider = app.Services;
//using (var scope = app.Services.CreateScope())
//{
//    var scopedServices = scope.ServiceProvider;
//    var jobEnviaCartasPorte = scopedServices.GetRequiredService<CartaPorteSender>();

//    if (jobEnviaCartasPorte != null)
//    {
//        //var jobId = BackgroundJob.Enqueue(() => jobEnviaCartasPorte.EnviarCartaPorte());
//        recurringJobManager.AddOrUpdate("Envio de cartas porte", () => jobEnviaCartasPorte.EnviarCartasPorte(), Cron.MinuteInterval(3));
//    }
//}


//using (var scope = app.Services.CreateScope())
//{
//    var scopedServices = scope.ServiceProvider;
//    var importaCartasPorte2008 = scopedServices.GetRequiredService<ImportData>();

//    if (importaCartasPorte2008 != null)
//    {
//        var jobId = BackgroundJob.Enqueue(() => importaCartasPorte2008.ImportarCartasPorte());
//        recurringJobManager.AddOrUpdate("Traslado de cartas porte a server 2019", () => importaCartasPorte2008.ImportarCartasPorte(), Cron.MinuteInterval(7));
//    }
//}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
