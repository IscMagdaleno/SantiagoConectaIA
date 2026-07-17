using EngramaCoreStandar.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.InformacionLocalModule;
using SantiagoConectaIA.Share.PostModels.InformacionLocalModule;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{
    public class InformacionLocalPlugin
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InformacionLocalPlugin(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        [KernelFunction]
        [Description("Busca informacion general del municipio de Santiago Papasquiaro. Categorias disponibles: Personaje Historico, Famosos, Gastronomia, Turismo, Naturaleza, Tradicion, Cultura, Leyenda, Comercio, Servicio, Economia, Geografia, Clima, Educacion. Usa esta funcion cuando el usuario pregunte sobre cualquier tema del municipio:人物, comida tipica, lugares turisticos, costumbres, economia, clima, geografia, educacion. Si el usuario pide 'toda la informacion' o 'todo sobre' un tema sin una cantidad especifica, usa limit=10.")]
        public async Task<string> BuscarInformacion(
            [Description("Palabra clave o frase de busqueda (ej. 'costo agua', 'donde queda', 'transporte', 'turismo', 'comida tipica', 'clima', 'escuelas', 'personajes'). Puede dejarse vacia para obtener toda la informacion disponible.")]
            string query = "",
            [Description("Categoria para filtrar. Valores disponibles: Personaje Historico, Famosos, Gastronomia, Turismo, Naturaleza, Tradicion, Cultura, Leyenda, Comercio, Servicio, Economia, Geografia, Clima, Educacion. Opcional.")]
            string categoria = "",
            [Description("Numero maximo de resultados. Por defecto 10. Si el usuario pide un numero especifico de resultados, usa ese valor aqui.")]
            int limit = 10)
        {
            using var scope = _scopeFactory.CreateScope();
            var domain = scope.ServiceProvider.GetRequiredService<IInformacionLocalDomain>();

            var result = await domain.PostGetformacionLocalByText(new PostGetInformacionLocal
            {
                nvchCategoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria,
                nvchTexto = string.IsNullOrWhiteSpace(query) ? null : query,
                bActivo = true
            });

            if (result.IsSuccess.False() || result.Data == null || !result.Data.Any())
            {
                return $"No se encontro informacion relacionada con '{query}'.";
            }

            var ranked = RankByKeywordRelevance(result.Data.ToList(), query);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var data = ranked.Take(limit).Select(i => new
            {
                i.iIdInformacionLocal,
                i.nvchCategoria,
                i.nvchTitulo,
                i.nvchDescripcionCorta,
                i.nvchContenidoDetallado,
                i.nvchUbicacion_LatLong,
                i.nvchPalabrasClave
            }).ToList();

            return JsonSerializer.Serialize(data, options);
        }

        [KernelFunction]
        [Description("Obtiene la informacion detallada y completa de un registro especifico usando su ID. Usa esta funcion cuando el usuario necesite el contenido completo (descripcion detallada, ubicacion exacta) de un resultado obtenido de BuscarInformacion.")]
        public async Task<string> GetInformacionDetalle(
            [Description("El ID del registro de informacion local a consultar.")]
            int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var domain = scope.ServiceProvider.GetRequiredService<IInformacionLocalDomain>();

            var result = await domain.GetInformacionLocalById(id);

            if (result.IsSuccess.False() || result.Data == null)
            {
                return $"No se encontro informacion con ID {id}.";
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var data = result.Data;

            return JsonSerializer.Serialize(new
            {
                data.iIdInformacionLocal,
                data.nvchCategoria,
                data.nvchTitulo,
                data.nvchDescripcionCorta,
                data.nvchContenidoDetallado,
                data.nvchUbicacion_LatLong,
                data.nvchPalabrasClave,
                Fecha = data.dtFechaCreacion.ToString("dd/MM/yyyy")
            }, options);
        }

        [KernelFunction]
        [Description("Obtiene toda la informacion local de una categoria especifica. Categorias disponibles: Personaje Historico, Famosos, Gastronomia, Turismo, Naturaleza, Tradicion, Cultura, Leyenda, Comercio, Servicio, Economia, Geografia, Clima, Educacion. Usa esta funcion cuando el usuario pregunte por 'todo lo relacionado a' un tema o quiera explorar una categoria completa.")]
        public async Task<string> GetInformacionPorCategoria(
            [Description("La categoria a consultar. Valores: Personaje Historico, Famosos, Gastronomia, Turismo, Naturaleza, Tradicion, Cultura, Leyenda, Comercio, Servicio, Economia, Geografia, Clima, Educacion.")]
            string categoria,
            [Description("Numero maximo de resultados. Por defecto 10. Si el usuario pide un numero especifico, usa ese valor.")]
            int limit = 10)
        {
            using var scope = _scopeFactory.CreateScope();
            var domain = scope.ServiceProvider.GetRequiredService<IInformacionLocalDomain>();

            var result = await domain.PostGetformacionLocalByText(new PostGetInformacionLocal
            {
                nvchCategoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria,
                nvchTexto = null,
                bActivo = true
            });

            if (result.IsSuccess.False() || result.Data == null || !result.Data.Any())
            {
                return $"No se encontro informacion en la categoria '{categoria}'.";
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var data = result.Data.Take(limit).Select(i => new
            {
                i.iIdInformacionLocal,
                i.nvchTitulo,
                i.nvchDescripcionCorta,
                i.nvchUbicacion_LatLong,
                i.nvchPalabrasClave
            }).ToList();

            return JsonSerializer.Serialize(data, options);
        }

        [KernelFunction]
        [Description("Obtiene las categorias disponibles en la base de datos de informacion local. Usa esta funcion cuando el usuario pregunte 'que categorias hay', 'que temas existen', 'sobre que puedo preguntar' o cuando quieras mostrar al usuario todas las opciones de consulta disponibles.")]
        public async Task<string> ObtenerCategoriasDisponibles()
        {
            using var scope = _scopeFactory.CreateScope();
            var domain = scope.ServiceProvider.GetRequiredService<IInformacionLocalDomain>();

            var result = await domain.GetInformacionLocal();

            if (result.IsSuccess.False() || result.Data == null || !result.Data.Any())
            {
                return "No se encontraron categorias disponibles.";
            }

            var categorias = result.Data
                .Where(i => !string.IsNullOrWhiteSpace(i.nvchCategoria))
                .Select(i => i.nvchCategoria)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return JsonSerializer.Serialize(new
            {
                TotalCategorias = categorias.Count,
                Categorias = categorias
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Busqueda usando el campo de palabras clave. Divide la consulta del usuario en terminos individuales y busca coincidencias en nvchPalabrasClave. Ideal para encontrar informacion muy especifica o cuando el usuario usa terminos exactos. Usa esta funcion cuando el usuario busque por palabras clave concretas (ej. 'clima seco', 'rio', 'sierra', 'queso') y BuscarInformacion no haya dado buenos resultados.")]
        public async Task<string> BuscarPorPalabraClave(
            [Description("Palabra clave para buscar en el campo de palabras clave de los registros (ej. 'rio', 'montana', 'clima', 'comida', 'musica', 'bailes'). Se dividira automaticamente en terminos individuales.")]
            string keywords,
            [Description("Numero maximo de resultados. Por defecto 10.")]
            int limit = 10)
        {
            using var scope = _scopeFactory.CreateScope();
            var domain = scope.ServiceProvider.GetRequiredService<IInformacionLocalDomain>();

            var result = await domain.PostGetformacionLocalByText(new PostGetInformacionLocal
            {
                nvchTexto = keywords,
                bActivo = true
            });

            if (result.IsSuccess.False() || result.Data == null || !result.Data.Any())
            {
                return $"No se encontro informacion con la palabra clave '{keywords}'.";
            }

            var searchTerms = keywords
                .Split(new[] { ' ', ',', ';', '.', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var data = result.Data
                .Select(i => new
                {
                    Item = i,
                    MatchCount = i.nvchPalabrasClave != null
                        ? searchTerms.Count(term =>
                            i.nvchPalabrasClave.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                        : 0
                })
                .Where(x => x.MatchCount > 0)
                .OrderByDescending(x => x.MatchCount)
                .ThenBy(x => x.Item.nvchTitulo)
                .Take(limit)
                .Select(x => new
                {
                    x.Item.iIdInformacionLocal,
                    x.Item.nvchCategoria,
                    x.Item.nvchTitulo,
                    x.Item.nvchDescripcionCorta,
                    x.Item.nvchPalabrasClave,
                    Relevancia = x.MatchCount
                }).ToList();

            if (!data.Any())
            {
                return $"No se encontro coincidencia en palabras clave para '{keywords}'.";
            }

            return JsonSerializer.Serialize(data, options);
        }

        private static List<InformacionLocal> RankByKeywordRelevance(List<InformacionLocal> items, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return items;

            var searchTerms = query
                .Split(new[] { ' ', ',', ';', '.', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 1)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!searchTerms.Any())
                return items;

            return items
                .Select(i => new
                {
                    Item = i,
                    KeywordScore = i.nvchPalabrasClave != null
                        ? searchTerms.Count(term =>
                            i.nvchPalabrasClave.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                        : 0,
                    TitleScore = searchTerms.Count(term =>
                        i.nvchTitulo.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                })
                .OrderByDescending(x => x.KeywordScore)
                .ThenByDescending(x => x.TitleScore)
                .ThenBy(x => x.Item.nvchTitulo)
                .Select(x => x.Item)
                .ToList();
        }
    }
}
