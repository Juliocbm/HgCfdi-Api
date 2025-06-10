# 🚛 CFDI.Data - Entity Framework Core Mapping Library

Este proyecto contiene el mapeo de todas las entidades de la base de datos **CFDisDB**, junto con el `DbContext` necesario para integrarse con Entity Framework Core. Está diseñado para ser reutilizado en múltiples proyectos mediante submódulos Git.

## 🧱 Estructura

```text
CFDI.Data/ # Submódulo Git con modelos CFDI compartidos           
├── CFDI.Data.csproj
├── Entities
    ├── CfdiDbContext.cs
    ├── Model.cs
├── README.md
└── ...
```

## 📦 Características

- Entidades completas del esquema de la base de datos `CFDisDB`
- `DbContext` preconfigurado para uso directo en proyectos EF Core
- Reutilizable como submódulo Git
- Separación limpia de la lógica de dominio y la infraestructura de datos


## 📚 Requisitos
- .NET 6.0 o superior
- Entity Framework Core


## 🆕 Mantenimiento: 
### 🛑Agregar nuevas entidades desde la base de datos (**entidad por entidad**)

Si se agregan nuevas tablas u objetos en la base de datos `SQL Server`, se deben actualizar las entidades en este proyecto utilizando el comando `Scaffold-DbContext` de Entity Framework Core.

**Usa el siguiente comando para generar automáticamente las clases correspondientes:**

```powershell
Scaffold-DbContext "Server=hg.servidor.com;User id=uAppCFDis;Password=********; Database=CFDisDB; TrustServerCertificate=true;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entities -Context CfdiDbContext -Tables nombre_nueva_tabla
```

**Notas importantes:**

- Sustituye nombre_nueva_tabla por el nombre real de la tabla que deseas agregar.
- El parámetro -OutputDir Entities asegura que las entidades se ubiquen correctamente en la carpeta correspondiente.
- Siempre revisa que los nuevos modelos se integren bien con el DbContext y la solución en general.

**Después de agregar las nuevas entidades, recuerda:**
- Confirmar que la tabla haya sido correctamente mapeada.
- Verificar relaciones y restricciones.
- Hacer commit de los cambios y actualizarlos en el repositorio.



### 🛑Agregar nuevas entidades desde la base de datos (**haciendo mapeo completo de la BD**)

Cuando se agregan nuevas tablas u objetos a la base de datos `SQL Server`, puedes regenerar los modelos usando el comando `Scaffold-DbContext` de Entity Framework Core:

```powershell
Scaffold-DbContext "Server=hg.servidor.com;User id=uAppCFDis;Password=********; Database=CFDisDB; TrustServerCertificate=true;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entities -Context TrucksDbContext -DataAnnotations -UseDatabaseNames -Force
```
⚠️ ¡Advertencia importante!

El uso del parámetro -Force sobrescribirá todos los archivos existentes en la carpeta de entidades. Esto incluye cualquier modificación manual que se haya hecho en los modelos generados.

🔧 ¿Cómo evitar perder cambios personalizados?

Si necesitas agregar lógica o propiedades personalizadas a las entidades generadas por Scaffold-DbContext, hazlo siempre utilizando partial class. De este modo, puedes mantener tu código separado del archivo generado, evitando que se pierda al regenerar.

```csharp
// Archivo generado automáticamente (Factura.cs)
// ¡NO editar aquí directamente!

public partial class Factura
{
    public int id { get; set; }
    public string Guia { get; set; }
}

// Tu archivo personalizado (Factura.Custom.cs)

public partial class Factura
{
    public string FullDescription => $"{id} - {Guia}";
}
```

➡️ Cuando utilices la clase Factura en tu código, vas a tener acceso tanto a id, Guia, como a FullDescription, todo en un solo objeto:

```csharp
var factura = new Factura { id = 1, Guia = "XYZ123" };
Console.WriteLine(factura.FullDescription); // Output: 1 - XYZ123
```
## 📤 Uso de repositorio Git
**Agregar como submódulo en proyectos GIT**

```bash
git submodule add https://juliocbm500@bitbucket.org/hgt_development/cfdidata-library.git /PrjoectConsumidor/lib
git submodule update --init --recursive
```

## 🚀 Implementación 
**Uso de DbContext en proyectos consumidores**
```bash
services.AddDbContext<CfdiDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("CfdiDatabase")));
```
## Author

- [desarrollohg@hgtransportaciones.com]

- **Repositorio:** https://juliocbm500@bitbucket.org/hgt_development/cfdidata-library.git
- **Versión:** 0.0.1 
- **Autor:** Julio Cesar Bautista M  
- **Licencia:** MIT 

