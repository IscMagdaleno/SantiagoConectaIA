# SantiagoConectaIA.API — Resumen de controladores

API REST en .NET 9 de Santiago Conecta. Todas las rutas siguen el patrón `api/{Controlador}/{Accion}` (por ejemplo `POST api/Noticias/PostGetNoticias`).

## Convenciones

- **Verbo:** casi todas las acciones son `POST` con cuerpo JSON (`[FromBody]`). Los modelos de entrada (`Post*`) viven en `SantiagoConectaIA.Share`. Las excepciones se indican en cada tabla.
- **Respuesta:** `Response<T>` (`EngramaCoreStandar.Results`) con `IsSuccess`, `Data` y `Message`. Cuando falla, la mayoría de las acciones responden `400 BadRequest` con el mismo cuerpo.
- **Arquitectura:** controlador → dominio (`I*Domain`, metodología Engrama) → DAL (`EngramaContext`, base `Engrama`, esquema `SCIA`).
- **Autenticación:** JWT Bearer. Las acciones de administración usan la política `Administracion` (`[Authorize(Policy = AuthPolicies.Administracion)]`: token válido y rol distinto de `Ciudadano`). `Opinion` y `PublicacionesCiudadano` usan `[Authorize(Roles = "Ciudadano")]` en sus acciones de escritura. Las acciones de lectura pública y los flujos de registro siguen siendo anónimos (ver [Seguridad](#seguridad)).
- **CORS:** abierto a cualquier origen (`SetIsOriginAllowed(_ => true)` con credenciales).
- **Swagger:** disponible solo en `Development`. Los comentarios XML se generan con `GenerateDocumentationFile`.
- **Bitácora:** `ApiLoggingMiddleware` registra las peticiones.

## Índice

| Área | Controladores |
|---|---|
| Sitio público | [Feed](#feed), [EmprendimientosFeed](#emprendimientosfeed), [Noticias](#noticias), [Eventos](#eventos), [Tramites](#tramites), [Oficinas](#oficinas), [InformacionLocal](#informacionlocal), [BuzonCiudadano](#buzonciudadano) |
| Comunidad | [Ciudadano](#ciudadano), [PublicacionesCiudadano](#publicacionesciudadano), [Opinion](#opinion) |
| Emprendimientos | [Empresas](#empresas) |
| IA y mensajería | [Chat](#chat), [WhatsApp](#whatsapp) |
| Administración | [Auth](#auth), [Catalogos](#catalogos), [AzureBlob](#azureblob), [Publicaciones](#publicaciones), [Mantenimiento](#mantenimiento) |
| Analítica | [Analytics](#analytics), [PageVisits](#pagevisits), [WhatsAppAnalytics](#whatsappanalytics) |

---

## Sitio público

### Feed
`api/Feed` — Feed mixto de contenido público.

| Acción | Descripción |
|---|---|
| `PostGetFeed` | Página del feed mixto (trámites, noticias, eventos, cápsulas). |
| `PostSearchFeed` | Busca contenido en trámites, noticias, eventos y cápsulas. |

### EmprendimientosFeed
`api/EmprendimientosFeed` — Feed de emprendimientos para el sitio público.

| Acción | Descripción |
|---|---|
| `PostGetEmprendimientosFeed` | Lista paginada de emprendimientos. |
| `PostSearchEmprendimientosFeed` | Búsqueda de emprendimientos. |

### Noticias
`api/Noticias`

| Acción | Descripción |
|---|---|
| `PostGetNoticias` | Listado de noticias. |
| `PostSaveNoticia` | Crea o actualiza una noticia. |

### Eventos
`api/Eventos` — Eventos, categorías, imágenes y sucursales.

| Acción | Descripción |
|---|---|
| `PostEscanearEventoConIA` | Sube un flyer (`IFormFile`) a Azure Blob y extrae los datos del evento con Gemini Vision. |
| `PostGetEventos` | Lista de eventos filtrada por ID, categoría, estatus o destacado. |
| `PostSaveEvento` | Crea o actualiza un evento. |
| `PostGetCategoriaEventos` / `PostSaveCategoriaEvento` | Consulta y guarda el catálogo de categorías. |
| `PostGetImagenesRegistro` / `PostSaveImagenRegistro` / `PostDeleteImagenRegistro` | Imágenes asociadas a un registro (genérico); la baja es lógica. |
| `PostGetEventoDetalle` | Detalle completo del evento con sus imágenes. |
| `PostGetEventosSucursales` / `PostSaveSucursalEvento` | Sucursales o locales de un evento. |

### Tramites
`api/Tramites` — Trámites municipales.

| Acción | Descripción |
|---|---|
| `PostGetTramites` | Lista de trámites. |
| `PostSearchTramites` | Búsqueda de trámites. |
| `PostGetTramitesCard` | Trámites en formato de tarjeta (resumen). |
| `PostGetTramiteDetalle` | Detalle completo: requisitos, pasos y documentos. |
| `PostSaveTramite` | Crea o actualiza un trámite. |
| `PostGetRequisitosPorTramite` / `PostSaveRequisito` | Requisitos de un trámite. |
| `PostSaveDocumento` | Documentos relacionados con un trámite. |

### Oficinas
`api/Oficinas`

| Acción | Descripción |
|---|---|
| `PostGetOficinas` / `PostSearchOficinas` | Lista y búsqueda de oficinas. |
| `PostSaveOficina` | Crea o actualiza una oficina. |
| `PostLinkOficinaTramite` | Vincula una oficina con un trámite. |
| `PostGetOficinasPorTramite` | Oficinas donde se realiza un trámite. |

### InformacionLocal
`api/InformacionLocal` — Base de conocimiento local que consume el agente de IA.

| Acción | Descripción |
|---|---|
| `PostGetAllInformacionLocal` | Todos los registros. |
| `PostGetInformacionLocalByID` | Un registro por ID. |
| `PostGetformacionLocalByText` | Búsqueda por texto (el nombre de la acción conserva el typo original). |
| `PostGetInformacionLocal` | Lista filtrada por parámetros. |
| `PostSaveInformacionLocal` | Crea o actualiza un registro. |

### BuzonCiudadano
`api/BuzonCiudadano`

| Acción | Descripción |
|---|---|
| `PostSaveReporte` | Guarda un reporte o sugerencia del buzón ciudadano. |

---

## Comunidad

### Ciudadano
`api/Ciudadano` — Registro e inicio de sesión de ciudadanos.

| Acción | Descripción |
|---|---|
| `PostEnviarCodigoWhatsApp` | Envía un código de verificación por WhatsApp. |
| `PostSaveCiudadano` | Registra un ciudadano con alias, teléfono de 10 dígitos y PIN. |
| `PostLoginCiudadano` | Inicio de sesión con teléfono y PIN (devuelve JWT). |
| `PostExternalLogin` | Inicio de sesión o registro con Google o Facebook. |
| `GetSocialAuthId` (`GET`) | ClientId o AppId del proveedor social (`?proveedor=`). |

### PublicacionesCiudadano
`api/PublicacionesCiudadano` — Publicaciones de la comunidad.

| Acción | Auth | Descripción |
|---|---|---|
| `PostGetPublicacionesCiudadano` | Anónimo | Feed público con paginación. |
| `PostSavePublicacionCiudadano` | `Ciudadano` | Crea o actualiza una publicación con fotos. |
| `PostGetMisPublicacionesCiudadano` | `Ciudadano` | Publicaciones propias (panel `/unete`). |
| `PostDeletePublicacionCiudadano` | `Ciudadano` | Baja lógica de una publicación propia. |

### Opinion
`api/Opinion` — Opiniones y respuestas sobre noticias, trámites, eventos o cápsulas.

| Acción | Auth | Descripción |
|---|---|---|
| `PostGetOpiniones` | Anónimo | Lista opiniones y respuestas de un contenido. |
| `PostSaveOpinion` | `Ciudadano` | Publica una opinión o respuesta. |

---

## Emprendimientos

### Empresas
`api/Empresas` — Módulo de emprendimientos.

| Acción | Descripción |
|---|---|
| `PostGetEmpresas` / `PostSaveEmpresa` | Lista y guarda empresas. |
| `PostGetCatalogoEmpresas` | Catálogo de empresas. |
| `PostGetEmpresaUbicaciones` / `PostSaveEmpresaUbicacion` | Ubicaciones. |
| `PostGetEmpresaRedesSociales` / `PostSaveEmpresaRedSocial` | Redes sociales. |
| `PostGetCategoriasPorEmpresa` / `PostSaveCategoriaCatalogo` | Categorías del catálogo de la empresa. |
| `PostGetProductosPorCategoria` / `PostSaveProductoServicio` | Productos y servicios. |
| `PostGetConfiguracionVisual` / `PostSaveConfiguracionVisual` | Configuración visual de la página. |
| `PostGetPropietario` / `PostSavePropietario` | Propietario del emprendimiento. |
| `PostGetEmpresasPropietarioByCorreo` | Empresas de un propietario por correo. |
| `PostGetEmprendimientoFullById` | Emprendimiento completo (empresa, categorías, productos, ubicaciones, redes). |
| `PostSaveEmprendimientoFull` | Guarda el emprendimiento completo en una sola operación. |

---

## IA y mensajería

### Chat
`api/Chat` — Conversaciones con el agente de IA.

| Acción | Descripción |
|---|---|
| `PostSearchForChat` | Procesa una consulta del usuario con el orquestador de agentes (`IAgentOrchestrationService`). |
| `PostGetChat` / `PostSaveChat` | Chats de un usuario. |
| `PostGetMensaje` / `PostSaveMensaje` | Mensajes de un chat. |
| `PostSeedMockData` | Inserta datos de prueba (asegura el proyecto 1). **Solo para desarrollo.** |

### WhatsApp
`api/WhatsApp` — Integración con WhatsApp Business (Meta).

| Acción | Descripción |
|---|---|
| `GET webhook` | Verificación del webhook por Meta. |
| `POST webhook` | Recibe mensajes; responde `200` de inmediato para evitar reintentos de Meta y procesa en segundo plano (`WhatsAppWorker`). |
| `POST test-message` | Simula el flujo completo (usuario, conversación, agente) sin enviar mensaje real. |

---

## Administración

### Auth
`api/Auth` — Usuarios administradores del PWA.

| Acción | Descripción |
|---|---|
| `PostLoginUsuario` | Inicio de sesión (devuelve JWT). |
| `PostSaveUsuario` | Crea o actualiza un usuario. |

### Catalogos
`api/Catalogos`

| Acción | Descripción |
|---|---|
| `PostGetTipoDatos` | Tipos de datos para metadatos. |
| `PostGetCatalogos` | Catálogos de un grupo por alias. |
| `PostGetParametro` | Parámetro por alias (tabla `SCIA.Parametros`). |

### AzureBlob
`api/AzureBlob` — Subida de archivos a Azure Blob Storage (reciben `IFormFile` y devuelven la URL).

| Acción | Descripción |
|---|---|
| `UploadDocument` | Sube un PDF. |
| `UploadImage-empresas` | Logo de empresa. |
| `UploadImage-Eventos` | Imagen de evento. |
| `UploadImage-noticias` | Portada de noticia. |

### Publicaciones
`api/Publicaciones` — Publicación en Facebook con IA y publicación automática.

**Texto e imagen con IA**

| Acción | Descripción |
|---|---|
| `PostMejorarPublicacion` | Redacta el texto de una noticia con Gemini. |
| `PostMejorarEmprendimiento` | Redacta el texto de un emprendimiento. |
| `PostMejorarProducto` | Redacta el texto de un producto. |
| `PostMejorarEvento` | Redacta el texto de un evento. |
| `PostEditarImagenIa` | Edita una imagen con IA. |
| `PostGuardarImagenBase64` | Sube una imagen base64 (por ejemplo, el post capturado en el navegador) a Blob y devuelve la URL. |
| `PostProxyImageBase64` / `PostProxyBatchImagesBase64` | Descargan imágenes remotas y las devuelven en base64 para poder capturarlas con html2canvas sin problemas de CORS. |

**Facebook**

| Acción | Descripción |
|---|---|
| `PostPublicarFacebook` | Publica texto e imagen enviándolos al webhook de Make (`make.facebook.webhook`). |

**Publicación automática y "Publicar siguiente"**

| Acción | Descripción |
|---|---|
| `PostGetPublicacionAutomatica` / `PostSavePublicacionAutomatica` | Estado e interruptor de la publicación diaria de noticias. |
| `PostGetPublicacionAutomaticaEmprendimientos` / `PostSavePublicacionAutomaticaEmprendimientos` | Estado e interruptor de la de emprendimientos (incluye cuál sigue). |
| `PostGetSiguienteNoticia` | Noticia que toca publicar (la más reciente activa que no se haya publicado). |
| `PostPublicarNoticiaGenerada` | Publica esa noticia con el texto y la imagen generados en el navegador; registra y valida que siga siendo la que toca. |
| `PostGetSiguienteEmprendimiento` | Emprendimiento que toca según la rotación (respeta una espera de 2 minutos y exige logo). |
| `PostPublicarEmprendimientoGenerado` | Publica ese emprendimiento con el texto y la imagen capturada, y avanza la rotación. |
| `GET Diagnostico` | Revisa, sin publicar, si los parámetros están configurados y cuál fue el último intento. Nunca devuelve valores secretos. |

Los servicios en segundo plano publican a diario, hora de México: `DailyFacebookPublicacionBackgroundService` (noticias, hora por defecto 11) y `DailyEmprendimientoPublicacionBackgroundService` (emprendimientos, hora por defecto 13). Usan la imagen de portada o el logo, no el post capturado, porque la captura requiere navegador. Requieren **Always On** en Azure App Service. Los parámetros están en `SCIA.Parametros`: `key.gemini`, `make.facebook.webhook`, `publicaciones.sitio`, `publicaciones.auto.noticias`, `publicaciones.auto.emprendimientos`, `azure.blob.storage`.

### Mantenimiento
`api/Mantenimiento` — Tareas de mantenimiento de datos. Todo el controlador exige la política `Administracion`.

| Acción | Descripción |
|---|---|
| `PostRepararImagenesPdf` | Corrige logos (`SCIA.Empresa.vchLogoUrl`) e imágenes de producto (`SCIA.ProductoServicio.vchImagenUrl`) cuyo archivo termina en `.pdf` pero en realidad es una imagen. Detecta el formato real por los primeros bytes, sube una copia con la extensión correcta a `empresas` y actualiza el registro (con guarda por URL anterior). **Por defecto simula**: envía `{"bSimular": false}` para aplicar los cambios. No borra los blobs anteriores. |

---

## Analítica

### Analytics
`api/Analytics` — Visitas al sitio público.

| Acción | Descripción |
|---|---|
| `PostSavePageVisit` | Registra una visita (indica si es única). |
| `PostGetPageVisitsSummary` | Totales, únicas, nuevas y recurrentes (rango de fechas opcional). |
| `PostGetPageVisitsByPage` | Visitas agrupadas por página. |
| `PostGetDailyTraffic` | Tráfico diario para gráficas. |
| `PostGetRecentVisits` | Visitas más recientes. |

### PageVisits
`api/PageVisits`

| Acción | Descripción |
|---|---|
| `PostGetPageVisitsStats` | Estadísticas de visitas para el dashboard. |

### WhatsAppAnalytics
`api/WhatsAppAnalytics` — Todas las acciones son `GET`.

| Acción | Descripción |
|---|---|
| `stats` | Estadísticas generales del módulo de WhatsApp. |
| `daily?days=30` | Estadísticas diarias para gráficas. |
| `users?top=100` | Usuarios ordenados por primer contacto. |

---

## Servicios en segundo plano

| Servicio | Función |
|---|---|
| `WhatsAppWorker` | Procesa en cola los mensajes recibidos por el webhook de WhatsApp. |
| `DailyScraperBackgroundService` | Tarea diaria que ejecuta el scraper de noticias (`INoticiasScraperService`). |
| `DailyFacebookPublicacionBackgroundService` | Publica a diario la noticia más reciente en Facebook. |
| `DailyEmprendimientoPublicacionBackgroundService` | Publica a diario el siguiente emprendimiento de la rotación. |

## Seguridad

**Política `Administracion`** (`Services/AuthPolicies.cs`): requiere usuario autenticado y que **no** tenga el rol `Ciudadano`. Hace falta porque los tokens de administrador y de ciudadano comparten emisor y ambos son válidos; un `[Authorize]` simple dejaría entrar a un ciudadano a las acciones de administración. Los tokens duran 24 h. El PWA envía el JWT en cada llamada y, si expiró, cierra la sesión y manda al login.

**Protegido con `Administracion`**

- Controladores completos: `Publicaciones`, `Mantenimiento`, `WhatsAppAnalytics`, `PageVisits`.
- `Auth/PostSaveUsuario`, `Catalogos/PostGetParametro`, `Chat/*` (excepto `PostSearchForChat`) y `WhatsApp/test-message`.
- `Analytics`: todo excepto `PostSavePageVisit`.
- `AzureBlob`: `UploadDocument`, `UploadImage-Eventos` y `UploadImage-noticias`.
- Las acciones de escritura de `Noticias`, `Eventos`, `Tramites`, `Oficinas`, `InformacionLocal` y `Empresas` (excepto el registro público de emprendimientos), además de `Empresas/PostGetPropietario`.

**Protegido con rol `Ciudadano`:** `Opinion/PostSaveOpinion` y las acciones de escritura y "mis publicaciones" de `PublicacionesCiudadano`.

**Anónimo a propósito** (lo usan el sitio público o terceros)

- Lecturas del sitio: `Feed`, `EmprendimientosFeed`, `Noticias`, `Eventos`, `Tramites`, `Oficinas`, `InformacionLocal`, `Opinion/PostGetOpiniones`, `PublicacionesCiudadano/PostGetPublicacionesCiudadano`, `Catalogos/PostGetTipoDatos` y `PostGetCatalogos`, y las lecturas de `Empresas`.
- `Auth/PostLoginUsuario` y todo `Ciudadano` (registro e inicio de sesión).
- `BuzonCiudadano/PostSaveReporte`, `Analytics/PostSavePageVisit`, `Chat/PostSearchForChat` (chatbot público) y `WhatsApp/webhook` (Meta).
- Registro de emprendimientos: `Empresas/PostSaveEmprendimientoFull`, `PostGetEmpresasPropietarioByCorreo`, `PostGetEmprendimientoFullById` y `AzureBlob/UploadImage-empresas`.

**Riesgos que quedan**

- Los endpoints anónimos de escritura (`PostSaveEmprendimientoFull`, `UploadImage-empresas`, `PostSaveReporte`, `PostSearchForChat`) permiten abusar del almacenamiento, la base y la cuota de Gemini. Conviene agregar límite de peticiones (rate limiting) y validar tamaño y formato.
- CORS permite cualquier origen con credenciales; conviene limitarlo a los dominios del PWA y del sitio público.
- `Chat/PostSeedMockData` y `WhatsApp/test-message` son endpoints de prueba; conviene deshabilitarlos fuera de `Development`.
- Las credenciales de `appsettings*.json` no deben vivir en el repositorio: usar variables de entorno o Azure Key Vault y rotar las que ya estuvieron publicadas.

--Prueba 1 de publicacion