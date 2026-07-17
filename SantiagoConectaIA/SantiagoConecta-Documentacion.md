# Santiago Conecta

**Propósito:** Plataforma digital integral del municipio de Santiago Papasquiaro, Durango, que centraliza la información sobre trámites gubernamentales, negocios locales, eventos culturales, noticias municipales y permite la comunicación bidireccional con la ciudadanía a través de un asistente con IA conversacional y WhatsApp. El sistema incluye un portal público para los ciudadanos, un panel administrativo PWA para la gestión de contenido, una aplicación móvil MAUI Hybrid y un chatbot inteligente con Google Gemini.

---

## Configuración General del Proyecto

### Herramientas Requeridas
- .NET 9.0 SDK / .NET 10 SDK
- Visual Studio 2022 / VS Code
- SQL Server (Azure Cloud o Local)
- Azure Blob Storage (para archivos multimedia)
- Cuenta de Google AI (API Key para Gemini)
- WhatsApp Cloud API (Meta Business Account)

### Metodología Engrama
Este proyecto sigue la **Metodología Engrama** para el desarrollo de software con una arquitectura en capas (Domain-Driven Design).

#### Estructura del Proyecto (8 proyectos en solución)

| Proyecto | Tipo | Propósito |
|----------|------|-----------|
| `SantiagoConectaIA.API` | ASP.NET Core Web API | Backend, controladores REST, lógica de dominio, Semantic Kernel |
| `SantiagoConectaIA.Share` | Class Library | Modelos compartidos (Objects), DTOs de petición (PostModels) |
| `SantiagoConectaIA.DAL` | Class Library | Entity Framework Core, DbContext, modelos de base de datos |
| `SantiagoConectaIA.PWA` | Blazor PWA | Panel administrativo con MudBlazor (CRUD de todos los módulos) |
| `SantiagoConecta.SharedUI` | Razor Class Library | Componentes Blazor reutilizables (página pública, widgets) |
| `SantiagoConectaFront` | Blazor WebAssembly | Portal público para ciudadanos (punto de entrada) |
| `SantiagoConecta.Mobile` | .NET MAUI Hybrid | Aplicación móvil multiplataforma (Android, iOS, Windows) |
| `SantiagoConectaIA.Test` | Test Project | Pruebas unitarias |

#### Convenciones del Proyecto
- **Tablas en BD:** Prefijos de tipo húngaro (`iId` = entero, `vch` = varchar, `nvch` = nvarchar, `dt` = datetime, `b` = bit, `fl` = float, `m` = money)
- **Stored Procedures:** `spGet*`, `spSave*`, `spSearch*`, `spDelete*`
- **Namespaces:** Por módulo (`EmpresasModule`, `EventosModule`, `NoticiasModule`, `TramitesModule`, `ConversationalModule`, etc.)
- **API Routes:** `api/[controller]/[Action]` con verbos POST para todas las operaciones
- **Dual Language:** Modelos con campos `vchNombre`/`vchNombreEn` para soporte español/inglés

#### Stack Tecnológico Principal

| Capa | Tecnología |
|------|-----------|
| **Runtime** | .NET 9.0 |
| **Base de Datos** | SQL Server + Entity Framework Core + Dapper (SPs) |
| **Autenticación** | JWT Bearer Token (panel administrativo) |
| **Inteligencia Artificial** | Google Gemini (`gemini-2.5-flash`) + Semantic Kernel 1.77 |
| **Mensajería** | WhatsApp Cloud API (Meta) |
| **Almacenamiento** | Azure Blob Storage (imágenes, documentos) |
| **Web Scraping** | HtmlAgilityPack + API REST externa |
| **Frontend** | Blazor Server (PWA), Blazor WebAssembly, MAUI Hybrid |
| **UI Framework** | MudBlazor (PWA admin), Bootstrap/Bulma (pública) |
| **Documentación API** | Swashbuckle (Swagger) |
| **Logging** | Serilog + Base de datos (ApiLoggingMiddleware) |

---

## Arquitectura del Sistema

### Capas de la API (SantiagoConectaIA.API)

```
┌─────────────────────────────────────────────┐
│              Controllers (REST API)          │
│  13 Controladores · 64 Endpoints            │
├─────────────────────────────────────────────┤
│           Domain Layer (EngramaLevels)       │
│  ┌────────────────────────────────────────┐ │
│  │  Interfaces  │  Core (Implementación)  │ │
│  │  15 Dominios  │  Lógica de negocio     │ │
│  └────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────┐ │
│  │     Servicios (Orquestación)           │ │
│  │  AgentOrchestrationService             │ │
│  │  NoticiasScraperService                │ │
│  └────────────────────────────────────────┘ │
├─────────────────────────────────────────────┤
│        Infrastructure Layer                 │
│  ┌────────────────────────────────────────┐ │
│  │  Entity (SP Results) │ Repository      │ │
│  │  Config              │ Interfaces      │ │
│  └────────────────────────────────────────┘ │
├─────────────────────────────────────────────┤
│              Semantic Kernel                │
│  ┌────────────────────────────────────────┐ │
│  │  KernelProvider · GeminiLoggingHandler  │ │
│  │  Agentes (TramitesAgentes)             │ │
│  │  Plugins (5 plugins · 22 funciones)    │ │
│  └────────────────────────────────────────┘ │
├─────────────────────────────────────────────┤
│          Background Services                │
│  WhatsAppWorker · DailyScraperService       │
├─────────────────────────────────────────────┤
│              Middleware                     │
│  ApiLoggingMiddleware                       │
└─────────────────────────────────────────────┘
```

---

## Módulos del Sistema

### 1. Módulo "Trámites"

**Propósito:** Centralizar y facilitar el acceso a la información sobre trámites gubernamentales municipales, proporcionando a los ciudadanos los requisitos, costos, ubicaciones de oficinas y pasos detallados para cada gestión.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Tramites/PostGetTramites` | Consultar lista de trámites |
| POST | `/api/Tramites/PostSearchTramites` | Buscar trámites por texto |
| POST | `/api/Tramites/PostGetTramitesCard` | Obtener trámites en formato tarjeta |
| POST | `/api/Tramites/PostGetTramiteDetalle` | Obtener detalle completo (requisitos, pasos, documentos) |
| POST | `/api/Tramites/PostGetRequisitosPorTramite` | Obtener requisitos por trámite |
| POST | `/api/Tramites/PostSaveTramite` | Guardar/actualizar trámite |
| POST | `/api/Tramites/PostSaveRequisito` | Guardar requisito |
| POST | `/api/Tramites/PostSaveDocumento` | Guardar documento asociado |

#### Modelo de Datos (Tramite)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdTramite` | int | Identificador único |
| `vchNombre` | string | Nombre del trámite |
| `nvchDescripcion` | string | Descripción detallada |
| `iIdCategoria` | int | Categoría del trámite |
| `bModalidadEnLinea` | bool | ¿Se puede realizar en línea? |
| `mCosto` | decimal | Costo del trámite |
| `bPrecioCalculado` | bool | Costo variable/calculado |

**Relaciones:**
- `Oficina` (N:M) — Un trámite puede realizarse en múltiples oficinas
- `Requisitos[]` — Documentos y condiciones necesarias
- `Pasos[]` — Guía paso a paso del proceso
- `Documentos[]` — Formatos y documentos descargables

#### Vista Pública (Galería de Tarjetas)
Cada trámite se muestra como una "card" con: nombre, descripción breve, modalidad (presencial/en línea/mixto), costo estimado, oficina principal, horario e icono representativo. Al hacer clic, se despliega una vista detallada con pestañas:

- **Requisitos:** Listado exhaustivo de documentos, especificando originales, copias o certificaciones. Incluye plantillas descargables si aplica.
- **Proceso:** Guía paso a paso numerada con tiempos estimados entre cada etapa.
- **Ubicación y Contacto:** Dirección completa, teléfonos, correos, horarios y mapa interactivo de Google Maps para cada oficina.

---

### 2. Módulo "Oficinas"

**Propósito:** Gestionar el catálogo de oficinas y dependencias municipales, incluyendo su ubicación, horarios, contacto y la relación N:M con los trámites que se realizan en cada una.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Oficinas/PostGetOficinas` | Consultar oficinas |
| POST | `/api/Oficinas/PostSearchOficinas` | Búsqueda paginada de oficinas |
| POST | `/api/Oficinas/PostSaveOficina` | Guardar/actualizar oficina |
| POST | `/api/Oficinas/PostLinkOficinaTramite` | Vincular oficina con trámite |
| POST | `/api/Oficinas/PostGetOficinasPorTramite` | Oficinas donde se realiza un trámite |

#### Modelo de Datos (Oficina)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdOficina` | int | Identificador único |
| `vchNombre` | string | Nombre de la oficina |
| `vchDireccion` | string | Dirección física |
| `vchTelefono` | string | Teléfono de contacto |
| `vchEmail` | string | Correo electrónico |
| `vchHorario` | string | Horario de atención |
| `flLatitud` | double? | Coordenada de latitud |
| `flLongitud` | double? | Coordenada de longitud |
| `vchNotas` | string | Notas adicionales |
| `vchUrlDireccion` | string | URL de Google Maps |

---

### 3. Módulo "Empresas" (Emprendimientos)

**Propósito:** Servir como vitrina digital para los negocios y emprendimientos locales, permitiendo a cada empresa gestionar su perfil completo, catálogo de productos/servicios, ubicaciones, redes sociales y configuración visual de su página.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Empresas/PostGetEmpresas` | Consultar empresas activas |
| POST | `/api/Empresas/PostSaveEmpresa` | Guardar/actualizar empresa |
| POST | `/api/Empresas/PostGetCatalogoEmpresas` | Catálogo de tipos de empresa |
| POST | `/api/Empresas/PostGetEmpresaUbicaciones` | Ubicaciones/sucursales |
| POST | `/api/Empresas/PostSaveEmpresaUbicacion` | Guardar ubicación |
| POST | `/api/Empresas/PostGetEmpresaRedesSociales` | Redes sociales |
| POST | `/api/Empresas/PostSaveEmpresaRedSocial` | Guardar red social |
| POST | `/api/Empresas/PostGetCategoriasPorEmpresa` | Categorías del catálogo |
| POST | `/api/Empresas/PostSaveCategoriaCatalogo` | Guardar categoría |
| POST | `/api/Empresas/PostGetProductosPorCategoria` | Productos/servicios por categoría |
| POST | `/api/Empresas/PostSaveProductoServicio` | Guardar producto/servicio |
| POST | `/api/Empresas/PostGetConfiguracionVisual` | Configuración visual |
| POST | `/api/Empresas/PostSaveConfiguracionVisual` | Guardar configuración visual |

#### Modelo de Datos (Empresa)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdEmpresa` | int | Identificador único |
| `vchNombreComercial` | string | Nombre comercial |
| `vchSlogan` | string? | Eslogan |
| `vchLogoUrl` | string? | URL del logo |
| `nvchDescripcion` | string? | Descripción del negocio |
| `nvchMision` | string? | Misión |
| `nvchVision` | string? | Visión |
| `nvchHistoria` | string? | Historia |
| `vchTelefono` | string? | Teléfono de contacto |
| `vchCorreo` | string? | Correo electrónico |

**Relaciones:**
- `Ubicaciones[]` — Direcciones y sucursales físicas
- `RedesSociales[]` — Facebook, Instagram, WhatsApp, sitio web
- `Categorias[]` → `Productos[]` — Catálogo de productos/servicios organizado por categorías
- `ConfiguracionVisual` — Personalización de colores y tipografía

#### Configuración Visual por Empresa
Cada empresa puede personalizar:
- `vchColorFondoBody`, `vchColorTextoPrincipal`
- `vchColorTitulos`, `vchColorBotones`, `vchColorMargenFotos`
- `vchTipografiaTitulos`, `vchTipografiaCuerpo`
- `vchEstiloBordes`, `iIdPlantillaBase`

---

### 4. Módulo "Eventos"

**Propósito:** Ofrecer un catálogo actualizado de eventos culturales, sociales, deportivos, capacitaciones y ferias del municipio, con información detallada de fechas, lugares, costos y organizadores.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Eventos/PostGetEventos` | Consultar eventos |
| POST | `/api/Eventos/PostSaveEvento` | Guardar/actualizar evento |
| POST | `/api/Eventos/PostGetEventoDetalle` | Detalle completo con imágenes |
| POST | `/api/Eventos/PostGetCategoriaEventos` | Catálogo de categorías |
| POST | `/api/Eventos/PostSaveCategoriaEvento` | Guardar categoría |
| POST | `/api/Eventos/PostGetImagenesRegistro` | Imágenes asociadas |
| POST | `/api/Eventos/PostSaveImagenRegistro` | Guardar imagen |
| POST | `/api/Eventos/PostDeleteImagenRegistro` | Eliminar imagen |
| POST | `/api/Eventos/PostGetEventosSucursales` | Sedes/sucursales del evento |
| POST | `/api/Eventos/PostSaveSucursalEvento` | Guardar sucursal |

#### Modelo de Datos (Evento)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdEvento` | int | Identificador único |
| `vchNombre` | string | Nombre del evento |
| `nvchDescripcion` | string | Descripción detallada |
| `iIdCategoriaEvento` | int? | Categoría del evento |
| `dtFechaInicio` | DateTime | Fecha de inicio |
| `dtFechaFin` | DateTime? | Fecha de fin |
| `vchLugar` | string | Lugar del evento |
| `vchDireccion` | string | Dirección |
| `flLatitud` | double | Coordenada de latitud |
| `flLongitud` | double | Coordenada de longitud |
| `vchImagenPortada` | string | URL de imagen de portada |
| `vchCostoTexto` | string | Descripción del costo |
| `vchOrganizador` | string | Nombre del organizador |
| `vchTelefono` | string | Teléfono de contacto |
| `vchCorreo` | string | Correo de contacto |
| `vchUrlOficial` | string | Sitio web oficial |
| `bDestacado` | bool | ¿Es evento destacado? |

**Relaciones:**
- `CategoriaEvento` — Clasificación del evento
- `Imagenes[]` — Galería de imágenes
- `Sucursales[]` — Múltiples sedes o ubicaciones

---

### 5. Módulo "Noticias"

**Propósito:** Mantener informada a la ciudadanía sobre acontecimientos y novedades diarias del municipio, con soporte para contenido multimedia enriquecido (imágenes, videos, galerías, contenido incrustado).

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Noticias/PostGetNoticias` | Consultar noticias activas |
| POST | `/api/Noticias/PostSaveNoticia` | Guardar/actualizar noticia |

#### Modelo de Datos (Noticia)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdNoticia` | int | Identificador único |
| `vchTitulo` | string | Título de la noticia |
| `nvchContenido` | string | Contenido HTML |
| `vchImagenPortada` | string | URL de imagen de portada |
| `dtFechaPublicacion` | DateTime | Fecha de publicación |
| `bActivo` | bool | ¿Está activa? |
| `iIdCategoria` | int? | Categoría de la noticia |

**Categorías de Noticias (enum):**
| Valor | Nombre |
|-------|--------|
| 1 | ComunicadoOficial |
| 2 | Cultura |
| 3 | Deportes |
| 4 | Policiaca |
| 5 | ObrasPublicas |

**Relaciones y Estructura de Contenido:**
- `Imagenes[]` — Galería de imágenes asociadas (`vchUrlImagen`)
- `Filas[]` → `Metadatos[]` — Bloques de contenido enriquecido:
  - `Video=1`, `GaleriaImagenes=2`, `Contenido=3`, `ImagenSola=4`
  - `RedesSociales=6`, `DatosCuriosos=7`
  - Cada metadato tiene: `vchTitulo`, `nvchValor`, `iOrden`, `iAncho`, `vchAlineacion`, `vchAlto`

#### Noticias Scraper (Background Service)
- Servicio `DailyScraperBackgroundService` que ejecuta `NoticiasScraperService`
- Obtiene noticias de API externa: `https://lisa-ia.com/api/feed/3/notes?categoria_id=12&geografia_id=6`
- Frecuencia configurable vía parámetro BD `noticias.service`
- Previene duplicados por título
- Convierte contenido HTML a estructura de metadatos

---

### 6. Módulo "Buzón Ciudadano"

**Propósito:** Permitir a los ciudadanos reportar fallas, quejas o sugerencias (baches, luminarias, fugas de agua, basura, seguridad, etc.) de manera directa a través del chat de IA o WhatsApp.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/BuzonCiudadano/PostSaveReporte` | Registrar reporte ciudadano |

#### Modelo de Datos (BuzonCiudadano)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdReporte` | int | Identificador único del reporte |
| `nvchNombreCiudadano` | string | Nombre del ciudadano |
| `nvchEmail` | string? | Correo electrónico de contacto |
| `nvchTelefono` | string? | Teléfono de contacto |
| `nvchCategoria` | string | Categoría del reporte |
| `nvchDescripcion` | string | Descripción detallada |
| `dtFechaReporte` | DateTime | Fecha del reporte |
| `nvchThreadId` | string? | ID del hilo de chat asociado |

**Categorías de Reporte:** Alumbrado, Baches, Fuga de agua, Basura, Seguridad, Otro

---

### 7. Módulo "Autenticación"

**Propósito:** Gestionar el acceso al panel administrativo mediante autenticación JWT.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Auth/PostLoginUsuario` | Inicio de sesión |
| POST | `/api/Auth/PostSaveUsuario` | Registro de nuevo usuario |

#### Modelo de Datos (UsuarioAuth)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdUsuario` | int | Identificador único |
| `vchNombre` | string | Nombre completo |
| `vchUserName` | string | Nombre de usuario |
| `vchEmail` | string | Correo electrónico |
| `vchRol` | string | Rol (Admin, Editor, etc.) |
| `Token` | string | Token JWT generado |

---

### 8. Módulo "Analíticas"

**Propósito:** Proporcionar estadísticas de uso del portal, incluyendo visitas por página, tráfico diario, visitantes únicos y recurrentes.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Analytics/PostSavePageVisit` | Registrar visita a página |
| POST | `/api/Analytics/PostGetPageVisitsSummary` | Resumen de visitas |
| POST | `/api/Analytics/PostGetPageVisitsByPage` | Visitas agrupadas por página |
| POST | `/api/Analytics/PostGetDailyTraffic` | Tráfico diario |
| POST | `/api/Analytics/PostGetRecentVisits` | Visitas recientes |

#### Modelo de Datos (PageVisit)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdPageVisit` | int | Identificador único |
| `vchPageUrl` | string | URL de la página visitada |
| `vchPageName` | string | Nombre de la página |
| `vchIpAddress` | string | Dirección IP del visitante |
| `vchUserAgent` | string | User-Agent del navegador |
| `vchReferrer` | string | URL de referencia |
| `vchBrowser` | string | Navegador detectado |
| `vchOperatingSystem` | string | Sistema operativo detectado |
| `vchDeviceType` | string | Tipo de dispositivo |
| `bIsUniqueVisitor` | bool | ¿Es visitante único? |
| `dtVisitDate` | DateTime | Fecha y hora de la visita |

---

### 9. Módulo "Catálogos"

**Propósito:** Gestionar parámetros de configuración del sistema y catálogos generales.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/Catalogos/PostGetTipoDatos` | Tipos de dato para metadatos |
| POST | `/api/Catalogos/PostGetCatalogos` | Catálogos por grupo |
| POST | `/api/Catalogos/PostGetParametro` | Parámetro por alias |

#### Parámetros del Sistema (Tabla Parametro)

| Alias | Propósito |
|-------|-----------|
| `key.gemini` | Modelo y API Key de Google Gemini |
| `noticias.service` | Intervalo de scraping de noticias |
| `WHATSAPP_CONFIG_1` | Configuración de WhatsApp (URL base) |
| `WHATSAPP_CONFIG_2` | Configuración de WhatsApp (tokens) |
| `WHATSAPP_CONFIG_3` | Configuración de WhatsApp (números) |
| `tipos.datos.noticias` | Tipos de dato para metadatos de noticias |

---

### 10. Módulo "Azure Blob Storage"

**Propósito:** Almacenar y gestionar archivos multimedia (imágenes y documentos) en la nube de Azure.

#### Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/AzureBlob/UploadDocument` | Subir PDF a contenedor `tramitedocs` |
| POST | `/api/AzureBlob/UploadImage-empresas` | Subir imagen a contenedor `empresas` |
| POST | `/api/AzureBlob/UploadImage-Eventos` | Subir imagen a contenedor `eventos` |

---

## Chat de Inteligencia Artificial (Agente Conversacional)

### Arquitectura del Asistente Virtual

El sistema integra un asistente virtual impulsado por **Google Gemini 2.5 Flash** a través de **Microsoft Semantic Kernel**, capaz de entender consultas en lenguaje natural y ejecutar funciones de plugins para responder con información precisa del municipio.

### Componentes del Chatbot

```
Usuario (Web/WhatsApp)
         │
         ▼
┌──────────────────────────────┐
│  ChatController              │  POST /api/Chat/PostSearchForChat
│  (API Endpoint)              │
└──────┬───────────────────────┘
       │ userQuery + userId
       ▼
┌──────────────────────────────────────────────┐
│  AgentOrchestrationService                   │
│  ┌────────────────────────────────────────┐  │
│  │ 1. Carga historial desde BD           │  │
│  │ 2. Reconstruye ChatHistory             │  │
│  │ 3. Envía a Gemini con plugins          │  │
│  │ 4. Guarda mensajes en BD               │  │
│  └────────────────────────────────────────┘  │
└──────┬───────────────────────────────────────┘
       │ ChatHistory + userMessage
       ▼
┌──────────────────────────────────────────────┐
│  TramitesAgentes (Agente)                    │
│  ┌────────────────────────────────────────┐  │
│  │ ChatAsync(userMessage, chatHistory)    │  │
│  │ → Gemini + AutoInvokeKernelFunctions   │  │
│  └────────────────────────────────────────┘  │
└──────┬───────────────────────────────────────┘
       │ Plugin auto-invocations
       ▼
┌──────────────────────────────────────────────┐
│  Kernel (Semantic Kernel)                    │
│                                              │
│  ┌──────────┐  ┌────────┐  ┌──────────────┐ │
│  │Tramites  │  │Noticias│  │BuzonCiudadano│ │
│  │Oficinas  │  │        │  │              │ │
│  │ (8 func) │  │(2 func)│  │  (1 func)    │ │
│  ├──────────┤  ├────────┤  ├──────────────┤ │
│  │Empresas  │  │Eventos │  │              │ │
│  │ (6 func) │  │(5 func)│  │              │ │
│  └──────────┘  └────────┘  └──────────────┘ │
└──────────────────────────────────────────────┘
```

### KernelProvider

El `KernelProvider` es el componente encargado de construir y proporcionar una instancia única del Kernel de Semantic Kernel:

1. Lee la configuración de Gemini desde la base de datos (parámetro `key.gemini`)
2. Obtiene el nombre del modelo (default: `gemini-2.5-flash`) y la API Key
3. Configura logging de peticiones HTTP a Gemini mediante `GeminiLoggingHandler`
4. Registra el conector `GoogleAIGeminiChatCompletion`
5. Construye el Kernel y registra los 5 plugins

### GeminiLoggingHandler

`DelegatingHandler` personalizado que intercepta todas las llamadas HTTP a la API de Gemini:
- Asigna un ID de correlación único a cada petición
- Logea la URL y longitud del cuerpo de la petición
- Muestra los primeros y últimos 500 caracteres del body
- Logea el código de estado y cuerpo de la respuesta
- Facilita la depuración de errores de la API de Gemini

### Plugins del Kernel

#### Plugin "TramitesOficinas" (8 funciones)

| Función | Descripción | Parámetros |
|---------|-------------|------------|
| `SearchTramites` | Busca trámites por palabra clave | `query`, `limit=5` |
| `SearchOficinas` | Busca oficinas por texto | `query`, `limit=5` |
| `SearchRequisitos` | Obtiene requisitos de un trámite | `idTramite` |
| `SearchCosto` | Obtiene costo y modalidad (en línea/presencial) | `idTramite` |
| `SearchOficinasByTramite` | Oficinas donde se realiza un trámite | `idTramite` |
| `SearchTramitesCard` | Trámites en formato resumen | `query`, `limit=5` |
| `GetTramiteDetalle` | Detalle completo (requisitos, pasos, documentos) | `idTramite` |

**Descripción:** Plugin principal para consultas sobre trámites gubernamentales. Cuando el usuario pregunta por un trámite específico, el agente usa `SearchTramites` para obtener el ID y luego funciones más específicas como `SearchRequisitos`, `SearchCosto` u `SearchOficinasByTramite` para profundizar.

#### Plugin "Noticias" (2 funciones)

| Función | Descripción | Parámetros |
|---------|-------------|------------|
| `BuscarNoticias` | Noticias y novedades del municipio | `query=""` |
| `GetNoticiaDetalle` | Detalle completo con imágenes y galería | `idNoticia` |

**Descripción:** Consulta las noticias activas del municipio. Si el usuario proporciona una palabra clave, filtra localmente por título o contenido. `BuscarNoticias` retorna las últimas 5 noticias; `GetNoticiaDetalle` da acceso al contenido completo con imágenes y metadatos.

#### Plugin "Eventos" (5 funciones)

| Función | Descripción | Parámetros |
|---------|-------------|------------|
| `BuscarEventos` | Eventos, ferias y actividades | `query=""`, `limit=5` |
| `GetEventoDetalle` | Detalle completo del evento | `idEvento` |
| `GetCategoriasEventos` | Catálogo de categorías de eventos | *(ninguno)* |
| `GetEventosPorCategoria` | Filtrar eventos por categoría | `idCategoria`, `limit=5` |
| `GetEventoSucursales` | Sedes y ubicaciones del evento | `idEvento` |

**Descripción:** Permite al agente consultar eventos próximos, filtrarlos por categoría y obtener información detallada. Ordena los resultados por fecha de inicio descendente.

#### Plugin "Empresas" (6 funciones)

| Función | Descripción | Parámetros |
|---------|-------------|------------|
| `BuscarEmpresas` | Negocios y comercios locales | `query=""`, `limit=5` |
| `GetEmpresaDetalle` | Información detallada (misión, visión, historia) | `idEmpresa` |
| `GetEmpresaUbicaciones` | Direcciones y sucursales | `idEmpresa` |
| `GetEmpresaRedesSociales` | Enlaces a redes sociales | `idEmpresa` |
| `GetEmpresaProductosServicios` | Catálogo de productos/servicios | `idEmpresa` |
| `GetEmpresaContacto` | Resumen de contacto | `idEmpresa` |

**Descripción:** Consulta el directorio de negocios locales registrados en el municipio. `BuscarEmpresas` filtra por nombre o descripción. `GetEmpresaProductosServicios` itera las categorías para construir el catálogo completo.

#### Plugin "BuzonCiudadano" (1 función)

| Función | Descripción | Parámetros |
|---------|-------------|------------|
| `RegistrarReporte` | Registra reporte/queja/sugerencia ciudadana | `nombreCiudadano`, `categoria`, `descripcion`, `email=""`, `telefono=""` |

**Descripción:** El agente solicita al ciudadano los datos requeridos (nombre, categoría, descripción y opcionalmente teléfono/email) antes de invocar esta función. Devuelve un mensaje de confirmación con el ID de reporte generado.

### TramitesAgentes (System Prompt)

El agente "Santiago Conecta IA" sigue un prompt de sistema detallado con las siguientes instrucciones:

- **Identidad:** Es un asistente virtual oficial del municipio de Santiago Papasquiaro, Durango
- **Tono:** Profesional, amigable y muy claro
- **Principio:** Siempre debe basar sus respuestas en la información obtenida de sus herramientas
- **Nunca mostrar JSON crudo** al usuario; solo información procesada en lenguaje natural
- **Instrucciones específicas por plugin** (12 puntos numerados):
  1. `TramitesOficinas.SearchTramites` para buscar trámites
  2. `TramitesOficinas.SearchOficinasByTramite` para ubicaciones
  3. `TramitesOficinas.SearchRequisitos` para documentos
  4. `TramitesOficinas.SearchCosto` para precios
  5. `Noticias.BuscarNoticias` para novedades
  6. `BuzonCiudadano.RegistrarReporte` para reportes
  7. `Empresas.*` para negocios locales
  8. `Eventos.*` para eventos y actividades
  9. `TramitesOficinas.GetTramiteDetalle` y `SearchTramitesCard` para detalle de trámites
  10. `Noticias.GetNoticiaDetalle` para contenido completo de noticias
  11. Consultas ambiguas → pedir aclaración
  12. Respuesta final formateada en lenguaje natural

### Flujo del Chat

1. El usuario envía una consulta desde la web o WhatsApp
2. `AgentOrchestrationService` recupera el historial del chat usando `userId` como `nvchThreadId`
3. Si no existe historial, crea un nuevo chat en BD (proyecto ID 1 "Santiago Conecta")
4. Reconstruye el `ChatHistory` de Semantic Kernel con los mensajes previos ordenados
5. Envía a Gemini con `GeminiToolCallBehavior.AutoInvokeKernelFunctions`
6. Gemini decide qué funciones invocar según la consulta del usuario
7. La respuesta del asistente se persiste en BD junto con el mensaje del usuario
8. Se retorna `ChatResponseIA` con la propiedad `nvchAgenteResponse`

### Modelo del Chat (ConversationalModule)

**Chat:**
| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdChat` | int | Identificador único del chat |
| `iIdProyecto` | int | Proyecto al que pertenece (1 = Santiago Conecta) |
| `dtFechaCreacion` | DateTime | Fecha de creación |
| `nvchNombre` | string | Nombre descriptivo del chat |
| `bActivo` | bool | ¿Está activo? |
| `nvchThreadId` | string | ID del hilo (userId del ciudadano) |

**Mensaje:**
| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `iIdMensaje` | int | Identificador único |
| `iIdChat` | int | Chat al que pertenece |
| `iOrden` | int | Orden secuencial en la conversación |
| `nvchRol` | string | Rol ("user" o "assistant") |
| `nvchContenido` | string | Contenido del mensaje |
| `dtFecha` | DateTime | Fecha y hora del mensaje |

**ChatResponseIA:**
| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `nvchAgenteResponse` | string | Respuesta generada por el asistente |

---

## Integración con WhatsApp

### Arquitectura de WhatsApp

```
Meta Cloud API (WhatsApp)
         │
         ▼ (Webhook POST)
┌────────────────────────────────┐
│ WhatsAppController             │  GET/POST /api/WhatsApp/webhook
│ (Verificación + Recepción)     │
└──────┬─────────────────────────┘
       │ Mensaje entrante
       ▼
┌────────────────────────────────┐
│ WhatsAppMessageQueue           │  ConcurrentQueue<WhatsAppQueuedMessage>
│ (Singleton)                    │
└──────┬─────────────────────────┘
       │ (desencolado cada 1s)
       ▼
┌────────────────────────────────┐
│ WhatsAppWorker                 │  BackgroundService (Hosted Service)
└──────┬─────────────────────────┘
       │ Obtiene IAgentOrchestrationService
       ▼
┌────────────────────────────────┐
│ AgentOrchestrationService      │  Procesa consulta con Gemini
└──────┬─────────────────────────┘
       │ Respuesta del asistente
       ▼
┌────────────────────────────────┐
│ WhatsAppService                │  POST a Meta Cloud API
│ SendTextMessageAsync           │  /{version}/{phone-id}/messages
└────────────────────────────────┘
```

### WhatsAppController (`/api/WhatsApp`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/WhatsApp/webhook` | Verificación del webhook de Meta (challenge) |
| POST | `/api/WhatsApp/webhook` | Recibir mensajes entrantes de WhatsApp |

**GET Webhook:** Recibe `hub.mode`, `hub.verify_token` y `hub.challenge`. Compara el token contra el configurado en BD y responde con el challenge si es válido.

**POST Webhook:** Recibe payload `WhatsAppInboundMessage` de Meta Cloud API:
- Solo procesa mensajes de tipo texto
- Aplica parche para números mexicanos (remueve `1` después de `521`)
- Encola el mensaje en `WhatsAppMessageQueue`
- **Siempre retorna 200 OK** para evitar reintentos de Meta

### WhatsAppMessageQueue
- `ConcurrentQueue<WhatsAppQueuedMessage>` (thread-safe)
- `WhatsAppQueuedMessage` contiene: `PhoneNumber`, `UserMessage`, `UserName`, `ReceivedAt`
- Patrón productor-consumidor

### WhatsAppWorker (BackgroundService)
- Ejecuta en bucle infinito con intervalo de 1 segundo
- Desencola mensajes de la cola
- Obtiene `IAgentOrchestrationService` del scope
- Llama a `ProcessUserQueryAsync()` con el mensaje del ciudadano
- Envía la respuesta vía `IWhatsAppService.SendTextMessageAsync()`
- Manejo de errores con logging

### WhatsAppService
- Configuración cargada desde BD (parámetros `WHATSAPP_CONFIG_1/2/3`)
- `SendTextMessageAsync(to, message)` → POST a la API de Meta:
  ```
  POST {BaseUrl}/{ApiVersion}/{PhoneNumberId}/messages
  Body: { "messaging_product": "whatsapp", "to": "{to}", "type": "text", "text": { "body": "{message}" } }
  ```
- `ValidateSignature(body, signatureHeader)` → Validación HMAC-SHA256
- `VerifyWebhookToken(token)` → Compara contra Verify Token almacenado

### Modelo de Datos WhatsApp

**WhatsAppInboundMessage (payload entrante):**
```
{
  "object": "whatsapp_business_account",
  "entry": [{
    "id": "entry-id",
    "changes": [{
      "field": "messages",
      "value": {
        "messaging_product": "whatsapp",
        "metadata": { "display_phone_number": "521...", "phone_number_id": "..." },
        "contacts": [{ "profile": { "name": "Ciudadano" }, "wa_id": "521..." }],
        "messages": [{ "from": "521...", "id": "msg-id", "timestamp": "...", "type": "text", "text": { "body": "Hola" } }]
      }
    }]
  }]
}
```

---

## Portal Administrativo (PWA)

### SantiagoConectaIA.PWA (Blazor PWA)

Panel administrativo progresivo construido con **MudBlazor** que permite la gestión completa de todos los módulos del sistema.

#### Autenticación
- Pantalla de login con usuario y contraseña
- Generación de token JWT
- Protección de rutas administrativas
- Expiración de sesión

#### Módulo Trámites (Área Administrativa)
| Componente | Descripción |
|-----------|-------------|
| `GridTramites.razor` | Tabla con filtros y búsqueda |
| `WizardTramites.razor` | Wizard de creación/edición multi-paso |
| `FormTramites.razor` | Formulario de información general |
| `FormRequisito.razor` | Formulario de requisitos |
| `FormPaso.razor` | Formulario de pasos del proceso |
| `FormDocumento.razor` | Formulario de documentos |
| `FormOficina.razor` | Asignación de oficinas al trámite |
| `CardTramites.razor` | Vista de tarjeta para previsualización |
| `CardOficina.razor` | Tarjeta de oficina vinculada |
| `FiltroTramites.razor` | Panel de filtros avanzados |
| `TableRequisitos.razor` | Tabla editable de requisitos |
| `TablaDocumentos.razor` | Tabla de documentos asociados |
| `GridOficinas.razor` | Grid de oficinas disponibles |

#### Módulo Empresas
| Componente | Descripción |
|-----------|-------------|
| `GridEmpresas.razor` | Grid con filtros |
| `FormEmpresa.razor` | Formulario con tabs |
| `TabUbicaciones.razor` | Gestión de ubicaciones/sucursales |
| `TabServicios.razor` | Gestión de productos y servicios |
| `TabRedesSociales.razor` | Configuración de redes sociales |
| `TabConfiguracionVisual.razor` | Personalización visual (colores, tipografía) |

#### Módulo Eventos
| Componente | Descripción |
|-----------|-------------|
| `GridEventos.razor` | Grid con filtros (categoría, estatus, destacado) |
| `FormEvento.razor` | Formulario de evento |
| `TabImagenesEvento.razor` | Gestión de imágenes |
| `TabSucursales.razor` | Gestión de sucursales/sedes |

#### Módulo Noticias
| Componente | Descripción |
|-----------|-------------|
| `GridNoticias.razor` | Grid de noticias |
| `FormNoticias.razor` | Editor de contenido enriquecido |
| `TabMetadatosNoticias.razor` | Gestión de metadatos por fila (video, galería, etc.) |
| `NoticiaCardComponent.razor` | Previsualización en formato card |

#### Módulo Oficinas
| Componente | Descripción |
|-----------|-------------|
| `GridOficinas.razor` | Grid con búsqueda y paginación |
| `FormOficina.razor` | Formulario con mapa de ubicación |
| `CardOficina.razor` | Tarjeta de oficina |

#### Módulo Chat
| Componente | Descripción |
|-----------|-------------|
| `GridChatbot.razor` | Historial de conversaciones del chatbot |

#### Analíticas
| Componente | Descripción |
|-----------|-------------|
| `DashboardVisits.razor` | Dashboard de visitas por página, tráfico diario, visitantes |

#### Componentes Compartidos
| Componente | Descripción |
|-----------|-------------|
| `ConfirmationDialog.razor` | Diálogo de confirmación genérico |
| `Loading.razor` | Indicador de carga |
| `TextAlert.razor` | Alertas de texto |
| `MudTextFieldDisable.razor` | Campo de texto deshabilitado |
| `ToolbarSuperior.razor` | Barra de herramientas superior |

---

## Portal Público (Blazor WebAssembly + SharedUI)

### Interfaz de Ciudadano

#### Página de Inicio (Home)
- Título principal "Santiago Conecta"
- Navegación directa a los módulos principales
- Widget del chatbot flotante
- Selector de idioma (español/inglés)

#### Módulo Trámites (Vista Pública)
**Componentes (SharedUI):**
| Componente | Descripción |
|-----------|-------------|
| `TramiteCardComponent.razor` | Tarjeta con resumen del trámite |
| `TramiteDetalle.razor` | Vista detallada con pestañas |
| `TramiteListItemComponent.razor` | Elemento de lista |

**Vista Detallada (pestañas):**
1. **Requisitos:** Lista de documentos necesarios
2. **Proceso:** Pasos numerados con tiempos estimados
3. **Ubicación y Contacto:** Mapa interactivo + datos de contacto

#### Módulo Eventos (Vista Pública)
**Componentes (SharedUI):**
| Componente | Descripción |
|-----------|-------------|
| `EventosPage.razor` | Listado de eventos del mes |
| `EventosDetalle.razor` | Vista detallada del evento |

#### Módulo Noticias (Vista Pública)
**Componentes (SharedUI):**
| Componente | Descripción |
|-----------|-------------|
| `NoticiasPage.razor` | Listado de noticias |
| `PlantillaNoticia.razor` | Renderizador de contenido de noticia |
| `NoticiaRenderer.razor` | Renderizador con soporte para: videos, galerías, contenido HTML, redes sociales, datos curiosos |

#### Módulo Emprendimientos (Vista Pública)
**Componentes (SharedUI):**
| Componente | Descripción |
|-----------|-------------|
| `EmprendimientosPage.razor` | Directorio de empresas |
| `EmprendimientoDetalle.razor` | Perfil completo de empresa |

#### Widget del Chatbot
- Botón flotante en todas las páginas
- Interfaz de conversación en lenguaje natural
- Integración con `ChatController.PostSearchForChat`
- Persistencia del historial por sesión

#### Componentes Compartidos (SharedUI)
| Componente | Descripción |
|-----------|-------------|
| `HeaderComponent.razor` | Encabezado del portal |
| `ChatbotWidget.razor` | Widget de chat flotante |
| `LanguageSwitcher.razor` | Selector de idioma |
| `MapaDependencias.razor` | Mapa interactivo de oficinas y dependencias |

**Data Services (SharedUI):**
- `Data_Tramites.cs` — Consumo de API de trámites
- `Data_Noticias.cs` — Consumo de API de noticias
- `Data_Eventos.cs` — Consumo de API de eventos
- `Data_Emprendimientos.cs` — Consumo de API de empresas
- `Data_BuzonCiudadano.cs` — Consumo de API de buzón ciudadano
- `Data_Analytics.cs` — Consumo de API de analíticas

---

## Aplicación Móvil (MAUI Hybrid)

**SantiagoConecta.Mobile** es una aplicación .NET MAUI Hybrid que envuelve la experiencia web en un contenedor nativo.

| Plataforma | Target |
|-----------|--------|
| Android | `net9.0-android` |
| iOS | `net9.0-ios` |
| Mac Catalyst | `net9.0-maccatalyst` |
| Windows | `net9.0-windows10.0.19041.0` |
| Tizen | `net9.0-tizen` |

**Componentes MAUI:**
- `Components/Routes.razor` — Definición de rutas de navegación
- `Components/_Imports.ps1` — Importaciones globales

La aplicación utiliza `BlazorWebView` para renderizar los componentes Blazor del proyecto `SharedUI`, ofreciendo una experiencia nativa con el mismo código base del portal web.

---

## Background Services

### DailyScraperBackgroundService
| Propiedad | Valor |
|-----------|-------|
| **Tipo** | `BackgroundService` (IHostedService) |
| **Intervalo** | 24 horas (configurable vía BD `noticias.service`) |
| **Función** | Ejecuta `INoticiasScraperService.RunScrapingAsync()` |
| **Fuente** | API REST `https://lisa-ia.com/api/feed/3/notes` |
| **Prevención** | Evita duplicados por título |
| **Almacenamiento** | Guarda como entidades `Noticia` con metadatos HTML |

### WhatsAppWorker
| Propiedad | Valor |
|-----------|-------|
| **Tipo** | `BackgroundService` (IHostedService) |
| **Intervalo** | 1 segundo |
| **Función** | Desencola mensajes de `WhatsAppMessageQueue` |
| **Procesamiento** | Llama a `IAgentOrchestrationService.ProcessUserQueryAsync()` |
| **Respuesta** | Envía vía `IWhatsAppService.SendTextMessageAsync()` |

---

## Seguridad y Middleware

### Autenticación JWT
- Emisión de tokens en `/api/Auth/PostLoginUsuario`
- Validación de `IssuerSigningKey`, `Issuer`, `Audience`, `Lifetime`
- Esquema: `JwtBearerDefaults.AuthenticationScheme`

### ApiLoggingMiddleware
Middleware global que intercepta todas las peticiones HTTP entrantes:

**Datos capturados por cada llamada:**
| Campo | Descripción |
|-------|-------------|
| `vchEndpoint` | Ruta del endpoint |
| `vchRequestMethod` | Método HTTP (GET, POST, etc.) |
| `dtRequestTimestamp` | Timestamp de la solicitud |
| `nvchRequestBody` | Cuerpo de la petición |
| `dtResponseTimestamp` | Timestamp de la respuesta |
| `nvchResponseBody` | Cuerpo de la respuesta |
| `bIsSuccess` | ¿Fue exitosa? (código 2xx) |
| `iDurationMs` | Duración en milisegundos |
| `vchHost` | Host de origen |

### Validación de Webhook WhatsApp
- Firma HMAC-SHA256 en cada petición entrante de Meta
- Verify Token almacenado en BD para verificación inicial

---

## Base de Datos

### Esquema General
| Elemento | Descripción |
|----------|-------------|
| **Motor** | SQL Server |
| **Ubicación** | Azure Cloud |
| **Conexión** | `EngramaCloudConnection` (appsettings.json) |
| **ORM** | Entity Framework Core + Dapper (SPs) |
| **Patrón** | Stored Procedures para todas las operaciones |
| **Nomenclatura** | `spGet*`, `spSave*`, `spSearch*`, `spDelete*` |

### Parámetros del Sistema (Tabla Parametro)
Configuración centralizada vía BD para características modificables sin redeploy:

| Alias | Propósito | Valores típicos |
|-------|-----------|-----------------|
| `key.gemini` | Modelo y API Key de Gemini | `gemini-2.5-flash`, `API_KEY` |
| `noticias.service` | Intervalo del scraper | `24` (horas) |
| `WHATSAPP_CONFIG_1` | URL base de WhatsApp API | `https://graph.facebook.com` |
| `WHATSAPP_CONFIG_2` | Token de acceso WhatsApp | `EAA...` |
| `WHATSAPP_CONFIG_3` | Phone Number ID y Verify Token | `123456789`, `verify_token` |
| `tipos.datos.noticias` | Tipos de metadatos | JSON con lista de tipos |

---

## Resumen de Integraciones

| Integración | Propósito | Tipo | Dirección |
|------------|-----------|------|-----------|
| **Google Gemini IA** | Procesar consultas ciudadanas en lenguaje natural | API REST | API → Gemini |
| **WhatsApp Cloud API** | Recibir y enviar mensajes de ciudadanos | Webhook + API | Meta → API → Meta |
| **Azure Blob Storage** | Almacenar imágenes y documentos | SDK Azure | API → Azure |
| **Fuente Externa Noticias** | Scraping automático de noticias | API REST | Externa → API |
| **Google Maps** | Mostrar ubicaciones de oficinas y eventos | Embed/API | Frontend → Google |

---

## Diagrama de Flujo de Usuario

```
CIUDADANO
    │
    ├── ACCEDE AL PORTAL WEB (Blazor WASM / MAUI Hybrid)
    │       │
    │       ├── HOME → Navegación a Módulos
    │       │   │
    │       │   ├── TRÁMITES
    │       │   │   ├── Galería de Tarjetas (cards)
    │       │   │   └── Detalle del Trámite
    │       │   │       ├── Pestaña: Requisitos
    │       │   │       ├── Pestaña: Proceso (pasos)
    │       │   │       └── Pestaña: Ubicación (mapa + contacto)
    │       │   │
    │       │   ├── EVENTOS
    │       │   │   ├── Listado del mes
    │       │   │   └── Detalle (fechas, lugar, costo, organizador)
    │       │   │
    │       │   ├── NOTICIAS
    │       │   │   ├── Listado con imágenes
    │       │   │   └── Detalle (contenido multimedia, galerías)
    │       │   │
    │       │   └── EMPRENDIMIENTOS
    │       │       ├── Directorio de empresas
    │       │       └── Perfil (descripción, catálogo, contacto, mapa)
    │       │
    │       └── WIDGET CHATBOT (flotante)
    │               │
    │               └── Consulta en lenguaje natural
    │                       │
    │                       ├── ChatController.PostSearchForChat
    │                       ├── AgentOrchestrationService
    │                       │   ├── Carga historial BD
    │                       │   └── Reconstruye ChatHistory
    │                       ├── TramitesAgentes → Gemini IA
    │                       │   ├── ¿Busca trámite? → Plugin TramitesOficinas
    │                       │   ├── ¿Busca noticia? → Plugin Noticias
    │                       │   ├── ¿Busca negocio? → Plugin Empresas
    │                       │   ├── ¿Busca evento?  → Plugin Eventos
    │                       │   └── ¿Reporta falla? → Plugin BuzonCiudadano
    │                       └── Respuesta formateada ← Gemini
    │
    └── ENVÍA WHATSAPP AL NÚMERO MUNICIPAL
            │
            ├── Meta Cloud API → Webhook POST
            ├── WhatsAppController → Encola mensaje
            ├── WhatsAppWorker → Desencola
            ├── AgentOrchestrationService → Gemini IA
            │   └── (mismos plugins que web)
            └── Respuesta ← WhatsApp Cloud API
```

---

## Resumen de Endpoints (64 total)

| Controlador | Rutas | Propósito |
|------------|-------|-----------|
| `AuthController` | 2 | Login y registro de usuarios |
| `AnalyticsController` | 5 | Estadísticas de visitas |
| `AzureBlobController` | 3 | Subida de archivos a Azure |
| `BuzonCiudadanoController` | 1 | Registro de reportes ciudadanos |
| `CatalogosController` | 3 | Catálogos y parámetros del sistema |
| `ChatController` | 6 | Chat conversacional con IA |
| `EmpresasController` | 13 | Gestión de empresas y emprendimientos |
| `EventosController` | 10 | Gestión de eventos municipales |
| `NoticiasController` | 2 | Gestión de noticias |
| `OficinasController` | 5 | Gestión de oficinas municipales |
| `PageVisitsController` | 1 | Estadísticas del dashboard |
| `TramitesController` | 8 | Gestión de trámites |
| `WhatsAppController` | 2 | Webhook de WhatsApp (GET + POST) |

---

## Especificaciones Técnicas Adicionales

### Arquitectura de Plugins (Semantic Kernel)
- Cada plugin usa `IServiceScopeFactory` para resolver servicios scoped
- Todos los métodos son `async Task<string>` decorados con `[KernelFunction]`
- Las funciones retornan JSON serializado para que Gemini lo procese
- Las descripciones están en español para que el LLM entienda cuándo invocar cada función

### Manejo de Errores en el Agente
- Si un plugin falla (BD no disponible, etc.), retorna un mensaje descriptivo
- El agente recibe el mensaje de error y puede informar al usuario
- `GeminiLoggingHandler` captura errores HTTP de la API de Gemini
- Excepciones en `ChatAsync` se relanzan con detalles del error

### Seed de Datos de Prueba
El `ChatController.PostSeedMockData` genera 3 chats de ejemplo:
1. **Juan Pérez** — Consulta sobre Acta de Nacimiento (costo, requisitos, pago)
2. **María Rodríguez** — Consulta sobre descuentos de Predial
3. **Anónimo WhatsApp** — Consulta sobre renovación de Licencia de Conducir

### Configuración de la Solución
- Archivo `.sln` en la raíz del proyecto
- Proyectos organizados por capa (API, DAL, Share) y frontend (PWA, SharedUI, Front, Mobile)
- Test project con pruebas unitarias para el módulo de Oficinas
