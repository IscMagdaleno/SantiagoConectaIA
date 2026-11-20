# SantiagoConectaIA — Reglas para agentes (Engrama)

Proyecto **independiente** de Conecta Control / Conecta Negocios. Misma metodología **Engrama** (Clean Architecture, SPs, PWA + API). **Sin** puente ni integración obligatoria con repos Conecta.

## Contexto

| Recurso | Ruta |
|---------|------|
| Solución principal | `SantiagoConectaIA/SantiagoConectaIA.sln` |
| Reglas compartidas | `../../Metodologia/TemplatePWA/AGENTS.md` |
| GEMINI (subordinado) | `../../Metodologia/TemplatePWA/GEMINI.md` |

**`AGENTS.md` (TemplatePWA y este archivo) gana** sobre GEMINI y prose antigua de metodología.

## Reglas Engrama (resumen)

- Capas Engrama estándar; sin SQL/Dapper fuera de Infrastructure.
- **HTTP:** `POST api/{Controlador}/{Método}`; método con prefijo **`Post`**; DTO en body.
- **EngramaCoreStandar 1.1.2** donde haya `PackageReference`.
- Al tocar back: encabezado de SP + XML en endpoints.
- **UI primero** en features nuevas: back después de handoff **`ui-lista`** y coordinación desde el Proyecto Conecta (si aplica al flujo del equipo) o aviso explícito del humano para Santiago.

## Entrega / handoff

### `docs/agentes/handoff-ui.md`

Misma estructura que en TemplatePWA (español, `America/Mexico_City`, rutas, estados, forma de datos, archivos, pendientes de docs/E2E, estado **`ui-lista`**).

Para otro módulo ya documentado: sección nueva o `handoff-ui-{modulo}.md` + `docs/agentes/README.md`.

### Aviso al humano

Chat de **este repo** para desarrollo. Al cerrar UI: si participan en el ecosistema Conecta, en el **Proyecto Conecta** pueden decir **«UI lista en SantiagoConectaIA»**; el coordinador usa el handoff. E2E: `../../E2E/PruebasE2E/santiago/`.

## Calidad

`dotnet build` en proyectos afectados. Patrones: módulos existentes en la solución Santiago.