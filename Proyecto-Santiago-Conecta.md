# Santiago Conecta

**Propósito:** Portal ciudadano de Santiago Papasquiaro para trámites, noticias, eventos, cápsulas culturales, emprendimientos locales, feed cívico y participación (opiniones), con asistencia por IA y WhatsApp.

---

## Configuracion General

## Herramientas Requeridas
- .NET 9 SDK
- Visual Studio 2022 / VS Code / Cursor
- SQL Server (local o cloud)
- Meta WhatsApp Cloud API (chat IA + códigos de verificación)

## Metodologia Engrama
Este proyecto sigue la Metodologia Engrama para el desarrollo de software.

### Estructura del Proyecto
- **Share**: Modelos y DTOs compartidos
- **Infrastructure**: Repositorios y acceso a datos (Dapper + SPs, schema `SCIA`)
- **Domain**: Logica de negocio
- **API**: Controllers REST (`PostGet*`, `PostSave*`)
- **SharedUI**: UI Blazor WASM compartida (portal público + Mobile)
- **PWA Admin**: aplicación administrativa aparte (usuarios `SCIA.Usuario`)

### Convenciones
- Tablas: prefijos de tipo (`iId`, `vch`, `nvch`, `dt`, `b`)
- SPs: `spGet*`, `spSave*`
- Namespaces por modulo
- Ciudadanos públicos ≠ administradores (tabla `SCIA.Ciudadano` vs `SCIA.Usuario`)

## Stack relevante
- Portal: Blazor WebAssembly (`SantiagoConectaFront` + `SantiagoConecta.SharedUI`)
- API: ASP.NET Core + JWT + Engrama/Dapper
- Auth ciudadana: teléfono 10 dígitos + PIN + verificación WhatsApp (texto)
- Auth admin: email/usuario + JWT rol administrativo

---

## Flujo del Usuario

Este documento describe la interacción del ciudadano con el portal "Santiago Conecta".

## 1. Acceso y Página de Inicio (Home)

Al acceder, el usuario llega al Home.

### 1.1 Elementos de la Página de Inicio
- **Hero:** marca Santiago Conecta, mensaje de bienvenida y CTA de registro.
- **Burbuja “Tu voz cuenta”:** invita a unirse para opinar en noticias, trámites y eventos.
- **Botón Registrarse:** navega a `/unete` (ya no abre el chat de IA desde el hero).
- **Carrusel informativo:** WhatsApp IA / última noticia.
- **Barra de búsqueda cívica:** apunta a resultados en `/buscar`.
- **Navegación:** Inicio, Feed, Trámites, Noticias, Eventos, Buzón Ciudadano, Emprendimientos, Únete (o alias + Salir si hay sesión).

## 2. Módulos Principales del Portal

### 2.1 Trámites
Información detallada de trámites municipales (requisitos, proceso, ubicación).

### 2.2 Eventos
Catálogo de eventos culturales, sociales y recreativos.

### 2.3 Noticias
Noticias y avisos del municipio (galería + detalle interno).

### 2.4 Emprendimientos
Vitrina de negocios locales, con feed mixto (negocios + productos) y galería.

### 2.5 Feed cívico (`/feed`)
Scroll infinito con mezcla de trámites, noticias, eventos y cápsulas; opiniones ligadas al contenido.

### 2.6 Únete (`/unete`)
Registro e inicio de sesión de ciudadanos por teléfono + PIN, con verificación por WhatsApp.

### 2.7 Opiniones
Comentarios y respuestas sobre noticias, trámites, eventos y cápsulas (solo ciudadanos logueados).

## 3. Funcionalidades de Asistencia y Contacto

### 3.1 Chat de Inteligencia Artificial (IA)
Widget flotante en el portal; responde sobre trámites, noticias, eventos y conocimiento local.

### 3.2 WhatsApp
- Asistente por WhatsApp Cloud API (mismo número de negocio).
- Envío de códigos de verificación para registro ciudadano (mensaje de texto tras abrir ventana de chat).
- Buzón ciudadano / contacto según flujos existentes.

---

## Únete (Ciudadanos)

# Módulo: Únete / Autenticación ciudadana

## 1. Objetivo
Permitir que un ciudadano se registre e inicie sesión en el portal público con celular y PIN, sin usar la cuenta de administrador.

## 2. Identidad
- Tabla `SCIA.Ciudadano`: alias, teléfono único (10 dígitos), PIN hasheado (SHA2-256), activo.
- JWT con rol `Ciudadano`.
- Sesión en navegador (`localStorage`): token, alias, id.

## 3. Registro (`/unete` — pestaña Únete)
1. Captura alias, teléfono (10 dígitos MX) y PIN (mín. 4).
2. **Abrir WhatsApp y saludar** al número de atención (fuerza ventana de 24 h de Meta).
3. **Enviar código por WhatsApp** (texto plano vía Cloud API).
4. Captura código de 6 dígitos y completa registro.
5. Si no llega el código: ayuda “No recibí ningún código” (debe iniciar/reiniciar chat si pasaron +24 h).

## 4. Login (`/unete` — pestaña Ya soy parte)
- Teléfono + PIN.
- No requiere código WhatsApp en cada login (el teléfono ya se validó al unirse).

## 5. API
- `POST /api/Ciudadano/PostEnviarCodigoWhatsApp`
- `POST /api/Ciudadano/PostSaveCiudadano` (incluye `vchCodigo`)
- `POST /api/Ciudadano/PostLoginCiudadano`

## 6. Base de datos
- `SCIA.Ciudadano`
- `SCIA.CiudadanoVerificacion` (OTP hasheado, expiración, intentos)
- SPs: `spSaveCiudadano`, `spGetCiudadanoAuth`, `spSaveCiudadanoCodigo`, `spValidarCiudadanoCodigo`

## 7. Notas Meta / WhatsApp
- El envío OTP usa `type: "text"` con la config existente (`WHATSAPP_CONFIG_1/2/3`).
- Meta solo entrega texto libre si el ciudadano escribió primero (o renueva la ventana de 24 h).
- Plantilla de autenticación queda como mejora futura para producción sin “saludo previo”.

---

## Opiniones

# Módulo: Opiniones (Tu opinión cuenta)

## 1. Objetivo
Que los ciudadanos logueados comenten y respondan sobre el contenido cívico. La opinión se liga al **contenido**, no a la card del feed: se ve igual en feed y en detalle.

## 2. Alcance
Tipos: `NOTICIA`, `TRAMITE`, `EVENTO`, `CAPSULA`.
- Publicar: solo ciudadanos autenticados (JWT `Ciudadano`).
- Leer: público.
- Respuestas: 1 nivel (responder a opinión raíz).
- Calificaciones (estrellas): **no implementadas** en esta fase.

## 3. Modelo
Tabla `SCIA.Opinion`:
- `vchTipoEntidad` + `iIdEntidad`
- `iIdCiudadano`
- `iIdOpinionPadre` (NULL = raíz)
- `nvchTexto` (1–1000)
- fechas / activo

SPs: `spGetOpiniones`, `spSaveOpinion`.

## 4. API
- `POST /api/Opinion/PostGetOpiniones` (público)
- `POST /api/Opinion/PostSaveOpinion` (Authorize Roles=`Ciudadano`; id desde token)

## 5. UI
Componente reutilizable `OpinionThread`:
- Feed (`FeedCardComponent`)
- Diálogo de cápsula
- Detalle noticia (`/noticias/{id}`)
- Detalle trámite (`/tramites/{id}`)
- Detalle evento (`/eventos/{id}`)

Si no hay sesión: CTA “Únete para opinar”.

---

## Tramites

# Módulo "Trámites"

## 1. Objetivo Principal
Información completa de trámites: requisitos, papelería, costos, oficinas, pasos.

## 2. Vista General (Galería)
Cards con nombre, descripción, modalidad, costo, oficina, horario, icono.

## 3. Vista Detallada
Pestañas:
- **Requisitos**
- **Proceso**
- **Ubicación y Contacto** (mapa)
- **Opiniones** (hilo compartido con el feed)

---

## Noticias

# Módulo de Noticias

## 1. Objetivo
Galería y lectura de noticias del municipio; scraper/sincronización desde fuentes oficiales cuando aplica.

## 2. Funcionalidades
- Galería de tarjetas (imagen, título, resumen, fecha, categoría).
- Detalle interno en `/noticias/{id}` (plantillas de contenido).
- Filtros por categoría.
- **Opiniones** en el detalle (mismas que en el feed para esa noticia).

## 3. Flujo típico
1. Usuario entra a Noticias.
2. Explora galería / filtra.
3. Abre detalle.
4. Si está logueado, puede opinar o responder.

---

## Eventos

# Módulo de Eventos

## 1. Objetivo
Difundir eventos del municipio con detalle, mapa y puntos de venta.

## 2. Público
- Galería de cards (portada, título, fechas, lugar).
- Detalle: descripción, mapa, sucursales/puntos de venta, contacto.
- **Opiniones** en el detalle (mismas que en el feed).

## 3. Administración
Alta/edición de eventos, imágenes, coordenadas y puntos de venta (PWA admin).

---

## Emprendimientos

# Módulo de Emprendimientos

## 1. Objetivo
Vitrina digital de negocios y productos/servicios locales.

## 2. Experiencia pública
- **`/emprendimientos`:** feed infinito mixto (emprendimientos + productos), orden por seed de sesión.
- **`/emprendimientos/galeria`:** galería clásica en grid.
- **`/emprendimientos/buscar`:** búsqueda por secciones.
- Detalle de empresa: `/emprendimientos/{id}`.
- Registro público de emprendimiento (flujo existente).

## 3. API feed
- `POST /api/EmprendimientosFeed/PostGetEmprendimientosFeed`
- `POST /api/EmprendimientosFeed/PostSearchEmprendimientosFeed`

---

## Feed cívico (Scroll infinito)

# Portal Público — Feed de Contenido Principal (`/feed`)

## 1. Descripción
Feed dinámico con cards de **Trámites, Noticias, Eventos y Cápsulas** (`InformacionLocal`), scroll infinito y búsqueda hacia `/buscar`.

## 2. Mezcla y paginación
- SP `SCIA.spGetFeed` / `spSearchFeed`.
- Patrón de mezcla (filtro `TODO`): trámite → noticias → cápsula → evento (repetible).
- Filtro rápido `vchTipoFiltro`: `TODO` | `TRAMITE` | `NOTICIA` | `EVENTO` | `CAPSULA` (tipo único ordenado por fecha).
- Seed de sesión para orden estable por visita.
- API: `POST /api/Feed/PostGetFeed`, `PostSearchFeed`.
- Migración: `Migration_FeedFiltro.sql`.

## 3. Card
- Badge de tipo, título, descripción, imagen, fecha.
- CTA a detalle (o diálogo para cápsula).
- **Compartir:** WhatsApp, Facebook y Web Share (si el navegador lo soporta).
- **Sección Opiniones** (`OpinionThread`) ligada a `(vchTipoEntidad, iIdEntidad)`.

## 4. Búsqueda
- Home / feed: barra de búsqueda.
- Resultados en `/buscar` por secciones colapsables (trámites, noticias, eventos, cápsulas).

## 5. Participación
- **Implementado:** opiniones + respuestas (ciudadanos logueados).
- **Pendiente:** calificaciones por estrellas / moderación admin.

---

## Informacion local (Cápsulas)

# Módulo: Cápsulas de Santiago

## 1. Objetivo
Datos históricos, curiosidades y hechos culturales (`SCIA.InformacionLocal`), útiles para el portal y la IA.

## 2. Público
- Aparecen en el feed como tipo `CAPSULA`.
- Detalle en diálogo del feed (sin ruta propia).
- **Opiniones** también disponibles en el diálogo.

## 3. Administración
Alta/edición de cápsulas (título, contenido, categoría, imagen, estado) desde panel admin.

## 4. IA
La IA puede usar este contenido como conocimiento local.

---

## Mapa de rutas públicas (resumen)

| Ruta | Función |
|------|---------|
| `/` | Home + búsqueda + destacados |
| `/feed` | Feed cívico infinito |
| `/buscar` | Resultados cívicos |
| `/tramites`, `/tramites/{id}` | Galería / detalle + opiniones |
| `/noticias`, `/noticias/{id}` | Galería / detalle + opiniones |
| `/eventos`, `/eventos/{id}` | Galería / detalle + opiniones |
| `/emprendimientos` | Feed emprendimientos/productos |
| `/emprendimientos/galeria` | Galería grid |
| `/emprendimientos/buscar` | Búsqueda emprendimientos |
| `/emprendimientos/{id}` | Detalle empresa |
| `/unete` | Registro / login ciudadano |

---

## Estado de implementación (resumen)

| Módulo | Estado |
|--------|--------|
| Trámites | Implementado |
| Noticias | Implementado |
| Eventos | Implementado |
| Cápsulas / InformacionLocal | Implementado (feed + diálogo) |
| Feed cívico + búsqueda | Implementado |
| Emprendimientos feed/galería | Implementado |
| Únete + OTP WhatsApp | Implementado (texto; requiere saludo previo) |
| Opiniones + respuestas | Implementado (4 tipos) |
| Calificaciones (estrellas) | Pendiente |
| Plantilla WhatsApp Authentication OTP | Mejora futura |
