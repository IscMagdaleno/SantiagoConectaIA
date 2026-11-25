# Handoff UI — Inicio y galería de emprendimientos

- **Fecha:** 2026-11-20 (America/Mexico_City)
- **Cliente:** Santiago Conecta (sitio público)
- **Pantallas:** Inicio, feed de emprendimientos, galería y detalle de un negocio
- **Rutas:** `@page "/"` · `@page "/emprendimientos"` · `@page "/emprendimientos/galeria"` · `@page "/emprendimientos/{iIdEmpresa:int}"`
- **Estados de UI:** lectura. El inicio no pide sesión. El buzón ciudadano escribe un reporte; no usarlo en estas pruebas.
- **Estado:** `ui-lista`

## Arranque

| Qué | Valor |
|-----|--------|
| App | `SantiagoConectaFront`: `http://localhost:5017` o `https://localhost:7186` |
| API en Development | `https://localhost:7196/` (`LOCAL_API_URL`) |
| API publicada | `https://santiagoconectaiaapi-c0eecydjbgfnepew.canadacentral-01.azurewebsites.net/` |
| Sesión | No hace falta. Sin sesión el header muestra **Únete**. Con sesión muestra el alias. |

Si la barra inferior está contraída, el control visible es el botón **Abrir menú**. Hay que abrirlo para ver **Inicio** y **Más**.

## Dato de control — negocio

Tomado del catálogo activo (`bEstatus = true`) el 2026-11-20. Si ese registro deja de estar activo, usar el primer negocio activo que se vea en la galería y anotar su nombre antes de entrar al detalle.

| Campo | Valor |
|-------|--------|
| `iIdEmpresa` | `1` |
| Ruta de detalle | `/emprendimientos/1` |
| `vchNombreComercial` | Lavado Ortega's |
| `vchSlogan` | Servicios Integrales de Limpieza |
| Categoría | Lavados (`iIdCatalogoEmpresa` `2`) |

Reserva, mismo catálogo: PayNut (`iIdEmpresa` `2`, slogan «Deleite en el primer bocado»).

La galería solo pinta empresas con `bEstatus = true`. El nombre comercial es el título de la tarjeta y el `h1` del detalle.

## Forma de datos — Inicio

No usar texto de relleno. Lo que se ve sale de estas llamadas. Un bloque sin filas simplemente no se pinta; no es error.

| Bloque | Origen | Qué debe verse |
|--------|--------|----------------|
| Título | Fijo | Un solo `h1`: «Trámites, noticias, eventos y negocios de Santiago en un solo lugar» |
| Buscador | Texto local; al enviar va a `/buscar?q=` | Placeholder «Busca un trámite, noticia o negocio» y botón **Buscar** |
| Atajos del hero | Fijos | **Pregúntale a la IA**, **WhatsApp**, **Únete a la comunidad** |
| Accesos rápidos | Fijos | Trámites `/tramites`, Noticias `/noticias`, Eventos `/eventos`, Emprendimientos `/emprendimientos`, Feed `/feed`, y el botón **Buzón ciudadano** |
| Noticias | `POST api/Noticias/PostGetNoticias` con `bActivo = true`. Hasta 3, la más nueva primero | `h3` **Noticias**, enlace **Ver todas** a `/noticias`. La primera lleva la insignia **Lo último** |
| Eventos | `POST api/Eventos/PostGetEventos` con `bDestacado = true` y `bEstatus = true`. Hasta 3 | `h3` **Próximos eventos**, enlace **Ver agenda** a `/eventos`. La primera lleva **Próximo** |
| Comunidad | `POST api/PublicacionesCiudadano/PostGetPublicacionesCiudadano` página 1, tamaño 3 | Hasta 3 tarjetas con alias y texto |
| Negocio | Fijo | «¿Tienes un negocio en Santiago?» y **Registrar mi negocio** hacia `/registro-emprendimiento` |
| Descubre | `POST api/InformacionLocal/PostGetInformacionLocal` con `bActivo = true`. Hasta 3 al azar | Si hay filas: «Descubre Santiago Papasquiaro». Si no hay, la sección no está |
| Buzón | Fijo hasta enviar | Nombre, correo, teléfono opcional, categoría (Queja, Sugerencia, Reporte, Solicitud, Otro) y mensaje. Correo `santiagoconecta01@gmail.com`, teléfonos `674 106 5612` y `332 834 0146` |

## Forma de datos — Emprendimientos

| Pantalla | Qué confirma el caso |
|----------|----------------------|
| Feed `/emprendimientos` | `h1` **Emprendimientos Locales**, botón **Ver galería**, botón **Registrar negocio** |
| Galería `/emprendimientos/galeria` | `h1` **Galería de Emprendimientos**, buscador «Buscar emprendimiento…», combo **Filtrar por Categoría**, tarjetas con **Visitar** |
| Detalle `/emprendimientos/1` | `h1` **Lavado Ortega's**, botón **Volver a la galería**, bloque **Sobre Nosotros** |

**Volver a la galería** es un enlace a `/emprendimientos/galeria`. **Volver al Feed**, en la galería, regresa a `/emprendimientos`, no al inicio. El regreso al inicio es el logo **Santiago Conecta** o **Inicio** en la barra inferior.

Llamadas de la galería y del detalle:

- `POST api/Empresas/PostGetEmpresas` con `iIdEmpresa = -1` en la galería, y con `iIdEmpresa = 1` en el detalle
- `POST api/Empresas/PostGetEmpresaRedesSociales`, `PostGetEmpresaUbicaciones`, `PostGetCategoriasPorEmpresa` y `PostGetProductosPorCategoria` en el detalle

El combo de categoría de la galería llama `POST api/Empresas/PostGetCatalogoEmpresa`. El método publicado es `PostGetCatalogoEmpresas`. Si el combo solo muestra **Todas las categorías**, la búsqueda por nombre sigue siendo la prueba válida. No fallar el recorrido por ese combo.

## Archivos

- `SantiagoConectaIA/SantiagoConecta.SharedUI/Pages/Index.razor`
- `SantiagoConectaIA/SantiagoConecta.SharedUI/Pages/EmprendimientosFeedPage.razor`
- `SantiagoConectaIA/SantiagoConecta.SharedUI/Pages/EmprendimientosPage.razor`
- `SantiagoConectaIA/SantiagoConecta.SharedUI/Pages/EmprendimientoDetalle.razor`
- `SantiagoConectaIA/SantiagoConecta.SharedUI/Layout/MainLayout.razor`
- `SantiagoConectaIA/SantiagoConecta.SharedUI/Shared/FloatingNav.razor`

## Pendiente

- Documentación técnica del inicio y del recorrido a la galería
- Manual de usuario
- Automatizar los casos de abajo en `E2E/PruebasE2E/santiago/`

## E2E — Inicio

La ruta `/` no redirige a login. Durante la carga puede verse el esqueleto de «Lo más reciente». Al terminar no debe quedar un diálogo de error ni el texto «Error de conexión».

### HOME-01 — Carga

1. Abrir `/`.
2. El `h1` es «Trámites, noticias, eventos y negocios de Santiago en un solo lugar».
3. Están el buscador, el botón **Buscar** y los tres atajos del hero.
4. La sección **Accesos rápidos** lista Trámites, Noticias, Eventos, Emprendimientos, Feed y Buzón ciudadano.
5. El header muestra el logo **Santiago Conecta**.

### HOME-02 — Accesos rápidos

Pulsar cada acceso y comprobar la ruta. Volver con el logo o con **Inicio**.

| Acceso | Ruta |
|--------|------|
| Trámites | `/tramites` |
| Noticias | `/noticias` |
| Eventos | `/eventos` |
| Emprendimientos | `/emprendimientos` |
| Feed | `/feed` |

**Buzón ciudadano** no cambia de ruta: baja a la sección `#contacto` de la misma página.

### HOME-03 — Búsqueda

1. En el buscador del inicio escribir `Lavado Ortega's` y pulsar **Buscar**.
2. La ruta queda `/buscar?q=Lavado%20Ortega%27s`.
3. Enviar el buscador vacío no navega.

### HOME-04 — Lo más reciente

Si la API devolvió filas:

1. Hay un `h2` **Lo más reciente**.
2. Con noticias: `h3` **Noticias**, enlace **Ver todas** y como máximo 3 tarjetas. La primera dice **Lo último**.
3. Con eventos: `h3` **Próximos eventos** y enlace **Ver agenda**.
4. Abrir una noticia o un evento abre su diálogo. Cerrarlo deja al usuario en `/`.

Si no hay filas, la sección no aparece. Eso no falla el caso.

### HOME-05 — Invitación a negocios y Descubre

1. Se ve «¿Tienes un negocio en Santiago?» y **Registrar mi negocio**.
2. Ese botón abre `/registro-emprendimiento`. Volver al inicio.
3. Si hay datos locales, se ve «Descubre Santiago Papasquiaro» con hasta 3 tarjetas y el botón **Pregúntale a la IA** en cada una.

### HOME-06 — Buzón sin enviar

1. En `#contacto` pulsar **Enviar mensaje** con el formulario vacío.
2. Aparecen las validaciones de nombre, correo y mensaje. No hay aviso de éxito.
3. No completar el envío: `POST api/BuzonCiudadano/PostSaveReporte` guarda un reporte real.

### HOME-07 — Menú Más e Inicio

1. En la barra inferior pulsar **Más**.
2. El diálogo **Módulos** incluye Feed, Trámites, Noticias, Eventos, Emprendimientos, Registrar negocio, Buzón ciudadano, Asistente IA, Buscar, Únete o Mi cuenta, y Privacidad.
3. Elegir **Noticias**. La ruta es `/noticias` y el panel se cierra.
4. Pulsar **Inicio**. La ruta vuelve a `/` y el ítem Inicio queda marcado.

## E2E — Galería de emprendimientos

Recorrido: inicio → feed → galería → Lavado Ortega's → galería → inicio.

### EMP-01 — Llegar a la galería

1. En `/`, dentro de **Accesos rápidos**, pulsar **Emprendimientos**.
2. La ruta es `/emprendimientos` y el `h1` es **Emprendimientos Locales**.
3. Pulsar **Ver galería**.
4. La ruta es `/emprendimientos/galeria` y el `h1` es **Galería de Emprendimientos**.
5. Hay al menos una tarjeta con botón **Visitar**. Entre ellas está **Lavado Ortega's**.

### EMP-02 — Abrir un negocio

1. En la galería, buscar `Lavado`.
2. La tarjeta **Lavado Ortega's** sigue visible. Su slogan es «Servicios Integrales de Limpieza».
3. Pulsar **Visitar** en esa tarjeta.
4. La ruta es `/emprendimientos/1`.
5. El `h1` es **Lavado Ortega's** y se ve **Sobre Nosotros**.
6. El botón **Volver a la galería** está en el encabezado, arriba del logo del negocio.

### EMP-03 — Volver a la galería

1. En `/emprendimientos/1` pulsar **Volver a la galería**.
2. La ruta es `/emprendimientos/galeria`.
3. El `h1` vuelve a ser **Galería de Emprendimientos**.
4. La tarjeta **Lavado Ortega's** sigue en el listado (limpiar el buscador si quedó el texto `Lavado`).

### EMP-04 — Volver al inicio

1. Desde la galería, pulsar el logo **Santiago Conecta**.
2. La ruta es `/` y se ve el `h1` del inicio.
3. Repetir el camino hasta la galería y, esta vez, volver con **Inicio** de la barra inferior. El resultado es el mismo.

**Volver al Feed** no sustituye este caso: deja al usuario en `/emprendimientos`.
