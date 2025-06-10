# Ryder.Api.Client

Librería **.NET 6** para consumir de forma limpia y mantenible la API de Ryder (CFDI / Carta Porte). Incluye:

- DTOs específicos por endpoint (heredando de un `BaseRequest` común).
- Inyección automática de `Email` y `AccessKey` en el body.
- Registro sencillo con Dependency Injection y `HttpClientFactory`.
- Excepciones enriquecidas (`RyderApiException`) para manejar errores HTTP.
- Configuración centralizada dentro de la librería, lista para usar “out-of-the-box”.

---

## 📁 Estructura del proyecto
```text
Ryder.Api.Client/                ← proyecto de tipo “Class Library”
│
├─ Configuration/                ← opciones de configuración (POCOs)
│   └─ RyderApiOptions.cs
│
├─ Models/                       ← DTOs para requests y responses
│   ├─ Requests/
│   │   ├─ GetDatosCartaPorteRequest.cs
│   │   ├─ GetCartaPorteRequest.cs
│   │   └─ …
│   └─ Responses/
│       ├─ DatosCartaPorteResponse.cs
│       ├─ CartaPorteResponse.cs
│       └─ …
│
├─ Services/                     ← interfaz e implementación del cliente HTTP
│   ├─ IRyderApiClient.cs
│   └─ RyderApiClient.cs
│
├─ Exceptions/                   ← excepciones específicas
│   └─ RyderApiException.cs
│
└─ DependencyInjection/          ← extensión para registrar todo en DI
    └─ ServiceCollectionExtensions.cs
```

## 🚀 Primeros pasos
En tu proyecto host (Web API, consola, worker, etc.):
```csharp
using Ryder.Api.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1) Registrar el cliente con valores por defecto embebidos
builder.Services.AddRyderApiClient();

var app = builder.Build();
```

## 🛠 Uso
Inyecta IRyderApiClient en tus servicios o controladores:

```csharp
public class MiServicio
{
    private readonly IRyderApiClient _ryder;

    public MiServicio(IRyderApiClient ryderApiClient)
    {
        _ryder = ryderApiClient;
    }

    public async Task ProcesarAsync()
    {
        // 1) Listar viajes
        var viajes = await _ryder.GetViajesAsync(new GetViajesRequest());
        
        // 2) Obtener datos de Carta Porte
        var datos = await _ryder.GetDatosCartaPorteAsync(new GetDatosCartaPorteRequest {
            OperacionID = 8,
            ViajeID     = 324315
        });

        // 3) Descargar PDF/Base64
        var pdf = await _ryder.GetCartaPorteAsync(new GetCartaPorteRequest {
            OperacionID = 8,
            ViajeID     = 324315
        });

        // 4) Enviar comprobante de ingreso
        await _ryder.UploadIngresoAsync(new UploadIngresoRequest {
            OperacionID = 8,
            ViajeID     = 324315,
            PdfBase64   = "...",
            XmlBase64   = "..."
        });
    }
}
```

## 🔧 Configuración
La librería ya incluye valores por defecto embebidos:

```csharp
BaseUrl: https://apiqa.ryder.com

AccessKey: clave de prueba/prod según versión de paquete

Email: correo configurado

SubscriptionKey: cabecera Ocp-Apim-Subscription-Key

Si necesitas sobrescribir alguno de estos valores, puedes hacerlo antes o después de AddRyderApiClient():

builder.Services.Configure<RyderApiOptions>(opts => {
  opts.BaseUrl         = "https://api.ryder.com";
  opts.AccessKey       = "TU_NUEVA_ACCESSKEY";
  opts.Email           = "usuario@tuempresa.com";
  opts.SubscriptionKey = "TU_SUBSCRIPTION_KEY";
});
builder.Services.AddRyderApiClient();
```

## 💡 Excepciones
Todos los errores HTTP se lanzan como RyderApiException, que expone:

- StatusCode (HttpStatusCode)

- Content (cuerpo de la respuesta)

- Mensaje amigable describiendo el endpoint y el estado HTTP

Puedes capturarlas así:
```csharp
try
{
    var datos = await _ryder.GetDatosCartaPorteAsync(req);
}
catch (RyderApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    // credenciales inválidas…
}
catch (RyderApiException ex)
{
    // otros errores…
    Console.WriteLine(ex.Content);
}
```

## 📑 Modelos incluidos
- BaseRequest: Email, AccessKey
- GetViajesRequest
- GetDatosCartaPorteRequest (OperacionID, ViajeID)
- GetCartaPorteRequest (OperacionID, ViajeID)
- UploadIngresoRequest (OperacionID, ViajeID, PdfBase64, XmlBase64)
- UploadIngresoCruceRequest (OperacionID, ViajeID, PdfBase64, XmlBase64)
- BaseResponse (Estatus, Error, CSVFileBase64, Datos)

## Author
- **Repositorio GIT** https://juliocbm500@bitbucket.org/hgt_development/ryderapiclient-library.git
- **Versión:** 0.0.1 
- **Autor:** [Julio Cesar Bautista M](https://github.com/Juliocbm/)
- **Contacto:** desarrollohg@hgtransportaciones.com
- **Licencia:** MIT 