# Carpeta Provider - Capa de Acceso a Datos (DAL)

Esta carpeta está destinada a contener los **Proveedores de Datos (Providers)** de `SantiagoConectaIA.DAL`. 

## ¿Qué es un Provider?

Un **Provider** es una clase que encapsula consultas y operaciones personalizadas en la base de datos usando Entity Framework Core. Mientras que los modelos y el `DbContext` son autogenerados (y no deben modificarse manualmente para no perder los cambios al actualizar), los *Providers* te permiten escribir tu lógica de negocio de datos personalizada (consultas complejas, llamadas a procedimientos almacenados, filtros dinámicos) con código limpio y mantenible.

### Ventajas de este enfoque:
1. **Separación de responsabilidades:** La API no realiza consultas directas con EF Core; en su lugar, delega en los Providers de la DAL.
2. **Cero pérdida de código:** Si regeneras tus modelos de base de datos con EF Core Power Tools, tus Providers no se verán afectados porque están en archivos separados.
3. **Reutilización:** Diferentes controladores o servicios de la API pueden reutilizar el mismo Provider para obtener datos de una tabla o vista.

---

## Cómo usar EF Core Power Tools en este proyecto

Para generar o actualizar tus modelos de base de datos cada vez que agregues una tabla, sigue estos pasos en **Visual Studio**:

1. Asegúrate de tener instalada la extensión **EF Core Power Tools** en Visual Studio.
2. Haz clic derecho sobre el proyecto **`SantiagoConectaIA.DAL`** en el Explorador de Soluciones.
3. Selecciona **EF Core Power Tools** $\rightarrow$ **Reverse Engineer** (Ingeniería Inversa).
4. Configura tu conexión a la base de datos (puedes usar la cadena de conexión de SQL Server `EngramaCloudConnection`).
5. Elige la versión de Entity Framework Core (**EF Core 9**).
6. Selecciona las tablas, vistas y procedimientos almacenados que deseas mapear.
7. En la configuración de generación, te sugerimos los siguientes nombres de carpetas:
   - **Context name:** `SantiagoConectaContext` (o el nombre que prefieras).
   - **Models path:** `Models` (se creará una carpeta llamada `Models` con tus entidades).
8. Haz clic en **OK**. Se generarán automáticamente el `DbContext` y todas tus clases entidad.

---

## Ejemplo de Estructura de un Provider

Imagina que tienes una tabla `Noticias`. Una vez generados los modelos, puedes crear un proveedor para consultas personalizadas:

### 1. Definir la interfaz (en tu capa Share, o directamente en DAL/Provider si es interna):
```csharp
using SantiagoConectaIA.Share.Objects; // Si tienes algún DTO aquí

namespace SantiagoConectaIA.DAL.Provider
{
    public interface INoticiaProvider
    {
        Task<List<Noticia>> ObtenerNoticiasDestacadasAsync();
    }
}
```

### 2. Implementar el Provider en esta carpeta:
```csharp
using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL.Models; // Aquí estarán tus modelos autogenerados

namespace SantiagoConectaIA.DAL.Provider
{
    public class NoticiaProvider : INoticiaProvider
    {
        private readonly SantiagoConectaContext _context;

        public NoticiaProvider(SantiagoConectaContext context)
        {
            _context = context;
        }

        public async Task<List<Noticia>> ObtenerNoticiasDestacadasAsync()
        {
            // Consultas avanzadas usando LINQ y Entity Framework Core
            return await _context.Noticias
                .Where(n => n.Activo == true)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }
    }
}
```

### 3. Registrar el Provider y el DbContext en tu API (`Program.cs`):
```csharp
using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL;
using SantiagoConectaIA.DAL.Provider;

// Registrar DbContext
builder.Services.AddDbContext<SantiagoConectaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EngramaCloudConnection")));

// Registrar tus Providers
builder.Services.AddScoped<INoticiaProvider, NoticiaProvider>();
```
