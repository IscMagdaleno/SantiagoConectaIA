using EngramaCoreStandar.Results;
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
                var request = new spGetFeed.Request
                {
                    iPage = postModel.iPage < 1 ? 1 : postModel.iPage,
                    iPageSize = postModel.iPageSize < 1 ? 10 : postModel.iPageSize,
                    vchSessionSeed = postModel.vchSessionSeed
                };

                var results = (await _feedRepository.spGetFeed(request)).ToList();
                var first = results.FirstOrDefault();

                if (first != null && !first.bResult && first.iIdEntidad <= 0)
                {
                    // Empty feed is a valid state for the UI (not a hard failure).
                    return new Response<IEnumerable<FeedCard>>
                    {
                        Data = new List<FeedCard>(),
                        IsSuccess = true,
                        Message = first.vchMessage
                    };
                }

                var cards = results
                    .Where(r => r.iIdEntidad > 0 && !string.IsNullOrWhiteSpace(r.vchTipoEntidad))
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
                return Response<IEnumerable<FeedCard>>.BadResult(ex.Message, new List<FeedCard>());
            }
        }

        public async Task<Response<FeedSearchResult>> SearchFeed(PostSearchFeed postModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(postModel.vchTexto))
                {
                    return Response<FeedSearchResult>.BadResult("Debe indicar un texto de búsqueda.", new FeedSearchResult());
                }

                var request = new spSearchFeed.Request
                {
                    vchTexto = postModel.vchTexto.Trim(),
                    iPage = postModel.iPage < 1 ? 1 : postModel.iPage,
                    iPageSize = postModel.iPageSize < 1 ? 50 : postModel.iPageSize
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
            r.nvchContenidoDetallado, r.vchImagenUrl, r.dtFecha, r.iTotalRegistros);

        private static FeedCard MapSearchResult(spSearchFeed.Result r) => MapCard(
            r.vchTipoEntidad, r.iIdEntidad, r.vchTitulo, r.nvchDescripcion,
            r.nvchContenidoDetallado, r.vchImagenUrl, r.dtFecha, r.iTotalRegistros);

        private static FeedCard MapCard(
            string tipo,
            int id,
            string titulo,
            string descripcion,
            string? contenidoDetallado,
            string imagenUrl,
            DateTime? fecha,
            int total)
        {
            var tipoNorm = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return new FeedCard
            {
                vchTipoEntidad = tipoNorm,
                iIdEntidad = id,
                vchTitulo = titulo ?? string.Empty,
                nvchDescripcion = descripcion ?? string.Empty,
                nvchContenidoDetallado = contenidoDetallado,
                vchImagenUrl = imagenUrl ?? string.Empty,
                dtFecha = fecha,
                iTotalRegistros = total,
                vchRutaDetalle = ResolveRoute(tipoNorm, id)
            };
        }

        private static string ResolveRoute(string tipo, int id) => tipo switch
        {
            "TRAMITE" => $"/tramites/{id}",
            "NOTICIA" => $"/noticias/{id}",
            "EVENTO" => $"/eventos/{id}",
            "CAPSULA" => string.Empty,
            _ => string.Empty
        };
    }
}
