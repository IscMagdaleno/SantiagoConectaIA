using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmprendimientosFeedModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.EmprendimientosFeedModule;
using SantiagoConectaIA.Share.PostModels.EmprendimientosFeedModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class EmprendimientosFeedDomain : IEmprendimientosFeedDomain
    {
        private readonly IEmprendimientosFeedRepository _repository;

        public EmprendimientosFeedDomain(IEmprendimientosFeedRepository repository)
        {
            _repository = repository;
        }

        public async Task<Response<IEnumerable<EmprendimientosFeedCard>>> GetFeed(PostGetEmprendimientosFeed postModel)
        {
            try
            {
                var request = new spGetEmprendimientosFeed.Request
                {
                    iPage = postModel.iPage < 1 ? 1 : postModel.iPage,
                    iPageSize = postModel.iPageSize < 1 ? 10 : postModel.iPageSize,
                    vchSessionSeed = postModel.vchSessionSeed
                };

                var results = (await _repository.spGetEmprendimientosFeed(request)).ToList();
                var first = results.FirstOrDefault();

                if (first != null && !first.bResult && first.iIdEntidad <= 0)
                {
                    return new Response<IEnumerable<EmprendimientosFeedCard>>
                    {
                        Data = new List<EmprendimientosFeedCard>(),
                        IsSuccess = true,
                        Message = first.vchMessage
                    };
                }

                var cards = results
                    .Where(r => r.iIdEntidad > 0 && !string.IsNullOrWhiteSpace(r.vchTipoEntidad))
                    .Select(MapGetResult)
                    .ToList();

                return new Response<IEnumerable<EmprendimientosFeedCard>>
                {
                    Data = cards,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<EmprendimientosFeedCard>>.BadResult(ex.Message, new List<EmprendimientosFeedCard>());
            }
        }

        public async Task<Response<EmprendimientosFeedSearchResult>> SearchFeed(PostSearchEmprendimientosFeed postModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(postModel.vchTexto))
                {
                    return Response<EmprendimientosFeedSearchResult>.BadResult("Debe indicar un texto de búsqueda.", new EmprendimientosFeedSearchResult());
                }

                var request = new spSearchEmprendimientosFeed.Request
                {
                    vchTexto = postModel.vchTexto.Trim(),
                    iPage = postModel.iPage < 1 ? 1 : postModel.iPage,
                    iPageSize = postModel.iPageSize < 1 ? 50 : postModel.iPageSize
                };

                var results = (await _repository.spSearchEmprendimientosFeed(request)).ToList();
                var first = results.FirstOrDefault();

                if (first != null && !first.bResult && first.iIdEntidad <= 0)
                {
                    return new Response<EmprendimientosFeedSearchResult>
                    {
                        Data = new EmprendimientosFeedSearchResult(),
                        IsSuccess = true,
                        Message = first.vchMessage
                    };
                }

                var cards = results
                    .Where(r => r.iIdEntidad > 0 && !string.IsNullOrWhiteSpace(r.vchTipoEntidad))
                    .Select(MapSearchResult)
                    .ToList();

                var searchResult = new EmprendimientosFeedSearchResult
                {
                    Emprendimientos = cards.Where(c => c.vchTipoEntidad == "EMPRENDIMIENTO").ToList(),
                    Productos = cards.Where(c => c.vchTipoEntidad == "PRODUCTO").ToList(),
                    iTotalRegistros = cards.FirstOrDefault()?.iTotalRegistros ?? 0
                };

                return new Response<EmprendimientosFeedSearchResult>
                {
                    Data = searchResult,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<EmprendimientosFeedSearchResult>.BadResult(ex.Message, new EmprendimientosFeedSearchResult());
            }
        }

        private static EmprendimientosFeedCard MapGetResult(spGetEmprendimientosFeed.Result r) => MapCard(
            r.vchTipoEntidad, r.iIdEntidad, r.iIdEmpresa, r.vchTitulo, r.nvchDescripcion,
            r.vchImagenUrl, r.vchNombreEmpresa, r.mPrecio, r.bAplicaDescuento, r.mPrecioDescuento, r.iTotalRegistros);

        private static EmprendimientosFeedCard MapSearchResult(spSearchEmprendimientosFeed.Result r) => MapCard(
            r.vchTipoEntidad, r.iIdEntidad, r.iIdEmpresa, r.vchTitulo, r.nvchDescripcion,
            r.vchImagenUrl, r.vchNombreEmpresa, r.mPrecio, r.bAplicaDescuento, r.mPrecioDescuento, r.iTotalRegistros);

        private static EmprendimientosFeedCard MapCard(
            string tipo,
            int id,
            int idEmpresa,
            string titulo,
            string descripcion,
            string imagenUrl,
            string nombreEmpresa,
            decimal precio,
            bool aplicaDescuento,
            decimal precioDescuento,
            int total)
        {
            var tipoNorm = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return new EmprendimientosFeedCard
            {
                vchTipoEntidad = tipoNorm,
                iIdEntidad = id,
                iIdEmpresa = idEmpresa,
                vchTitulo = titulo ?? string.Empty,
                nvchDescripcion = descripcion ?? string.Empty,
                vchImagenUrl = imagenUrl ?? string.Empty,
                vchNombreEmpresa = nombreEmpresa ?? string.Empty,
                mPrecio = precio,
                bAplicaDescuento = aplicaDescuento,
                mPrecioDescuento = precioDescuento,
                iTotalRegistros = total,
                vchRutaDetalle = idEmpresa > 0 ? $"/emprendimientos/{idEmpresa}" : string.Empty
            };
        }
    }
}
