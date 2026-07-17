using Microsoft.SemanticKernel;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EventosModule;
using SantiagoConectaIA.Share.Objects.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using EngramaCoreStandar.Extensions;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{
    public class EventosPlugin
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventosPlugin(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        [KernelFunction]
        [Description("Busca eventos, ferias, festivales, capacitaciones, talleres y actividades culturales o deportivas en el municipio de Santiago Papasquiaro. Utiliza esta función cuando el usuario pregunte qué hay de evento, qué actividades hay o qué ferias se realizan próximamente.")]
        public async Task<string> BuscarEventos(
            [Description("Palabra clave para filtrar eventos (ej. 'feria', 'capacitación', 'cultura', 'deporte'). Puede dejarse vacía para obtener los próximos eventos activos.")]
            string query = "",
            [Description("Número máximo de resultados. Por defecto es 5.")]
            int limit = 5)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventosDomain = scope.ServiceProvider.GetRequiredService<IEventosDomain>();

            var result = await eventosDomain.GetEventos(new PostGetEventos { bEstatus = true });

            if (!result.IsSuccess || result.Data == null)
            {
                return "No se pudieron obtener los eventos en este momento.";
            }

            var eventos = result.Data;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var cleanQuery = query.Trim().ToLower();
                eventos = eventos.Where(e =>
                    (e.vchNombre != null && e.vchNombre.ToLower().Contains(cleanQuery)) ||
                    (e.nvchDescripcion != null && e.nvchDescripcion.ToLower().Contains(cleanQuery))
                ).ToList();
            }

            var resultado = eventos.OrderByDescending(e => e.dtFechaInicio).Take(limit).Select(e => new
            {
                e.iIdEvento,
                e.vchNombre,
                FechaInicio = e.dtFechaInicio.ToString("dd/MM/yyyy"),
                FechaFin = e.dtFechaFin?.ToString("dd/MM/yyyy"),
                e.vchLugar,
                e.vchDireccion,
                Costo = e.vchCostoTexto,
                e.vchOrganizador,
                e.bDestacado
            }).ToList();

            if (resultado.Count == 0)
            {
                return $"No se encontraron eventos con la palabra clave '{query}'.";
            }

            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene el detalle completo de un evento específico incluyendo descripción, fecha, lugar, costo, organizador, contacto e imágenes. Requiere el ID del evento obtenido de BuscarEventos.")]
        public async Task<string> GetEventoDetalle(
            [Description("El ID del evento a consultar.")]
            int idEvento)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventosDomain = scope.ServiceProvider.GetRequiredService<IEventosDomain>();

            var result = await eventosDomain.GetEventoDetalle(new PostGetEventoDetalle { iIdEvento = idEvento });

            if (!result.IsSuccess || result.Data == null)
            {
                return $"No se encontró el detalle del evento con ID {idEvento}.";
            }

            return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene el catálogo de categorías disponibles para eventos (ej. 'Cultural', 'Deportivo', 'Capacitación', 'Feria').")]
        public async Task<string> GetCategoriasEventos()
        {
            using var scope = _scopeFactory.CreateScope();
            var eventosDomain = scope.ServiceProvider.GetRequiredService<IEventosDomain>();

            var result = await eventosDomain.GetCategoriaEventos(new PostGetCategoriaEventos());

            if (!result.IsSuccess || result.Data == null || !result.Data.Any())
            {
                return "No se encontraron categorías de eventos disponibles.";
            }

            return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Busca eventos filtrados por una categoría específica. Utiliza GetCategoriasEventos primero para obtener el ID de la categoría deseada.")]
        public async Task<string> GetEventosPorCategoria(
            [Description("El ID de la categoría de evento para filtrar (obtenido de GetCategoriasEventos).")]
            int idCategoria,
            [Description("Número máximo de resultados. Por defecto es 5.")]
            int limit = 5)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventosDomain = scope.ServiceProvider.GetRequiredService<IEventosDomain>();

            var result = await eventosDomain.GetEventos(new PostGetEventos { iIdCategoriaEvento = idCategoria, bEstatus = true });

            if (!result.IsSuccess || result.Data == null || !result.Data.Any())
            {
                return $"No se encontraron eventos para la categoría con ID {idCategoria}.";
            }

            var resultado = result.Data.OrderByDescending(e => e.dtFechaInicio).Take(limit).Select(e => new
            {
                e.iIdEvento,
                e.vchNombre,
                FechaInicio = e.dtFechaInicio.ToString("dd/MM/yyyy"),
                e.vchLugar,
                Costo = e.vchCostoTexto
            }).ToList();

            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene las sucursales, sedes o ubicaciones asociadas a un evento específico. Útil cuando el usuario pregunta dónde se llevará a cabo un evento en múltiples ubicaciones.")]
        public async Task<string> GetEventoSucursales(
            [Description("El ID del evento para consultar sus sucursales o sedes.")]
            int idEvento)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventosDomain = scope.ServiceProvider.GetRequiredService<IEventosDomain>();

            var result = await eventosDomain.GetEventosSucursales(new PostGetEventosSucursales { iIdEvento = idEvento });

            if (!result.IsSuccess || result.Data == null || !result.Data.Any())
            {
                return $"No se encontraron sucursales para el evento con ID {idEvento}.";
            }

            return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
