using EngramaCoreStandar.Results;
using Newtonsoft.Json.Linq;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.FeedModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.FeedModule;
using SantiagoConectaIA.Share.PostModels.FeedModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class FeedDomain : IFeedDomain
    {
        private readonly IFeedRepository _feedRepository;

        public FeedDomain(IFeedRepository feedRepository)
        {
            _feedRepository = feedRepository;
        }

        public async Task<Response<IEnumerable<FeedCard>>> GetFeed(PostGetFeed postModel)
        {
            try
            {
                var page = postModel?.iPage ?? 1;
                var pageSize = postModel?.iPageSize ?? 10;
                var filtro = NormalizeTipoFiltro(postModel?.vchTipoFiltro);

                var request = new spGetFeed.Request
                {
                    iPage = page,
                    iPageSize = pageSize,
                    vchSessionSeed = postModel?.vchSessionSeed,
                    vchTipoFiltro = filtro
                };

                var results = (await _feedRepository.spGetFeed(request)).ToList();
                var cards = results
                    .Where(r => r.iIdEntidad > 0)
                    .Select(MapGetResult)
                    .ToList();

                return new Response<IEnumerable<FeedCard>>
                {
                    Data = cards,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<FeedCard>>.BadResult(
                    $"Error al obtener feed: {ex.Message}",
                    Enumerable.Empty<FeedCard>());
            }
        }

        public async Task<Response<FeedSearchResult>> SearchFeed(PostSearchFeed postModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(postModel?.vchTexto))
                {
                    return Response<FeedSearchResult>.BadResult("Debe indicar un texto de búsqueda.", new FeedSearchResult());
                }

                var page = postModel?.iPage ?? 1;
                var pageSize = postModel?.iPageSize ?? 50;

                var request = new spSearchFeed.Request
                {
                    vchTexto = postModel.vchTexto.Trim(),
                    iPage = page < 1 ? 1 : page,
                    iPageSize = pageSize < 1 ? 50 : pageSize
                };

                var results = (await _feedRepository.spSearchFeed(request)).ToList();
                var first = results.FirstOrDefault();

                if (first != null && !first.bResult && first.iIdEntidad <= 0)
                {
                    return new Response<FeedSearchResult>
                    {
                        Data = new FeedSearchResult(),
                        IsSuccess = true,
                        Message = first.vchMessage
                    };
                }

                var cards = results
                    .Where(r => r.iIdEntidad > 0 && !string.IsNullOrWhiteSpace(r.vchTipoEntidad))
                    .Select(MapSearchResult)
                    .ToList();

                var searchResult = new FeedSearchResult
                {
                    Tramites = cards.Where(c => c.vchTipoEntidad == "TRAMITE").ToList(),
                    Noticias = cards.Where(c => c.vchTipoEntidad == "NOTICIA").ToList(),
                    Eventos = cards.Where(c => c.vchTipoEntidad == "EVENTO").ToList(),
                    Capsulas = cards.Where(c => c.vchTipoEntidad == "CAPSULA").ToList(),
                    Publicaciones = cards.Where(c => c.vchTipoEntidad == "PUBLICACION").ToList(),
                    iTotalRegistros = cards.FirstOrDefault()?.iTotalRegistros ?? 0
                };

                return new Response<FeedSearchResult>
                {
                    Data = searchResult,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<FeedSearchResult>.BadResult(ex.Message, new FeedSearchResult());
            }
        }

        private static FeedCard MapGetResult(spGetFeed.Result r) => MapCard(
            r.vchTipoEntidad, r.iIdEntidad, r.vchTitulo, r.nvchDescripcion,
            r.nvchContenidoDetallado, r.vchImagenUrl, r.dtFecha, r.iTotalRegistros, r.nvchImagenesJson);

        private static FeedCard MapSearchResult(spSearchFeed.Result r) => MapCard(
            r.vchTipoEntidad, r.iIdEntidad, r.vchTitulo, r.nvchDescripcion,
            r.nvchContenidoDetallado, r.vchImagenUrl, r.dtFecha, r.iTotalRegistros, r.nvchImagenesJson);

        private static FeedCard MapCard(
            string tipo,
            int id,
            string titulo,
            string descripcion,
            string? contenidoDetallado,
            string imagenUrl,
            DateTime? fecha,
            int total,
            string? imagenesJson = null)
        {
            var tipoNorm = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            var urls = ParseImagenesJson(imagenesJson, imagenUrl);
            return new FeedCard
            {
                vchTipoEntidad = tipoNorm,
                iIdEntidad = id,
                vchTitulo = titulo ?? string.Empty,
                nvchDescripcion = descripcion ?? string.Empty,
                nvchContenidoDetallado = contenidoDetallado,
                vchImagenUrl = urls.FirstOrDefault() ?? imagenUrl ?? string.Empty,
                dtFecha = fecha,
                iTotalRegistros = total,
                vchRutaDetalle = ResolveRoute(tipoNorm, id),
                nvchImagenesJson = imagenesJson,
                ImagenesUrls = urls
            };
        }

        private static List<string> ParseImagenesJson(string? json, string singleFallback)
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var jToken = JToken.Parse(json);
                    if (jToken is JArray arr)
                    {
                        foreach (var item in arr)
                        {
                            if (item.Type == JTokenType.String)
                            {
                                var s = item.ToString();
                                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                            }
                            else if (item is JObject obj)
                            {
                                var url = obj["vchUrlImagen"]?.ToString() ?? obj["nvchUrlImagen"]?.ToString() ?? obj["url"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(url)) list.Add(url);
                            }
                        }
                    }
                }
                catch { }
            }

            if (!list.Any() && !string.IsNullOrWhiteSpace(singleFallback))
            {
                list.Add(singleFallback);
            }
            return list;
        }

        private static string ResolveRoute(string tipo, int id) => tipo switch
        {
            "TRAMITE" => $"/tramites/{id}",
            "NOTICIA" => $"/noticias/{id}",
            "EVENTO" => $"/eventos/{id}",
            "CAPSULA" => string.Empty,
            "PUBLICACION" => string.Empty,
            _ => string.Empty
        };

        private static string NormalizeTipoFiltro(string? filtro)
        {
            var value = (filtro ?? string.Empty).Trim().ToUpperInvariant();
            return value switch
            {
                "TRAMITE" or "NOTICIA" or "EVENTO" or "CAPSULA" or "PUBLICACION" => value,
                _ => "TODO"
            };
        }
    }
}
