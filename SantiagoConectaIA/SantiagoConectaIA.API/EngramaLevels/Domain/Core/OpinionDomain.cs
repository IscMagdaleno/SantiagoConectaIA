using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OpinionModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.OpinionModule;
using SantiagoConectaIA.Share.PostModels.OpinionModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class OpinionDomain : IOpinionDomain
    {
        private static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "NOTICIA", "TRAMITE", "EVENTO", "CAPSULA"
        };

        private readonly IOpinionRepository _repository;

        public OpinionDomain(IOpinionRepository repository)
        {
            _repository = repository;
        }

        public async Task<Response<IEnumerable<Opinion>>> GetOpiniones(PostGetOpiniones postModel)
        {
            try
            {
                var tipo = NormalizeTipo(postModel.vchTipoEntidad);
                if (!TiposValidos.Contains(tipo) || postModel.iIdEntidad <= 0)
                {
                    return Response<IEnumerable<Opinion>>.BadResult("Contenido inválido para opiniones.", new List<Opinion>());
                }

                var results = (await _repository.spGetOpiniones(new spGetOpiniones.Request
                {
                    vchTipoEntidad = tipo,
                    iIdEntidad = postModel.iIdEntidad
                })).ToList();

                var first = results.FirstOrDefault();
                if (first != null && !first.bResult && first.iIdOpinion <= 0)
                {
                    return Response<IEnumerable<Opinion>>.BadResult(first.vchMessage ?? "No se pudieron cargar las opiniones.", new List<Opinion>());
                }

                var mapped = results
                    .Where(r => r.iIdOpinion > 0)
                    .Select(Map)
                    .ToList();

                var roots = mapped.Where(o => !o.iIdOpinionPadre.HasValue || o.iIdOpinionPadre <= 0).ToList();
                foreach (var root in roots)
                {
                    root.Respuestas = mapped
                        .Where(o => o.iIdOpinionPadre == root.iIdOpinion)
                        .OrderBy(o => o.dtFechaCreacion)
                        .ToList();
                }

                return new Response<IEnumerable<Opinion>>
                {
                    Data = roots.OrderBy(o => o.dtFechaCreacion),
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Opinion>>.BadResult(ex.Message, new List<Opinion>());
            }
        }

        public async Task<Response<Opinion>> SaveOpinion(PostSaveOpinion postModel, int iIdCiudadano)
        {
            try
            {
                if (iIdCiudadano <= 0)
                {
                    return Response<Opinion>.BadResult("Debes iniciar sesión para opinar.", new Opinion());
                }

                var tipo = NormalizeTipo(postModel.vchTipoEntidad);
                var texto = (postModel.nvchTexto ?? string.Empty).Trim();

                if (!TiposValidos.Contains(tipo) || postModel.iIdEntidad <= 0)
                {
                    return Response<Opinion>.BadResult("Contenido inválido para opiniones.", new Opinion());
                }

                if (texto.Length < 1 || texto.Length > 1000)
                {
                    return Response<Opinion>.BadResult("La opinión debe tener entre 1 y 1000 caracteres.", new Opinion());
                }

                var result = await _repository.spSaveOpinion(new spSaveOpinion.Request
                {
                    vchTipoEntidad = tipo,
                    iIdEntidad = postModel.iIdEntidad,
                    iIdCiudadano = iIdCiudadano,
                    iIdOpinionPadre = postModel.iIdOpinionPadre > 0 ? postModel.iIdOpinionPadre : null,
                    nvchTexto = texto
                });

                if (result == null || !result.bResult || result.iIdOpinion <= 0)
                {
                    return Response<Opinion>.BadResult(result?.vchMessage ?? "No se pudo publicar la opinión.", new Opinion());
                }

                return new Response<Opinion>
                {
                    Data = MapSave(result),
                    IsSuccess = true,
                    Message = result.vchMessage
                };
            }
            catch (Exception ex)
            {
                return Response<Opinion>.BadResult(ex.Message, new Opinion());
            }
        }

        private static string NormalizeTipo(string? tipo)
            => (tipo ?? string.Empty).Trim().ToUpperInvariant();

        private static Opinion Map(spGetOpiniones.Result r) => new()
        {
            iIdOpinion = r.iIdOpinion,
            vchTipoEntidad = r.vchTipoEntidad ?? string.Empty,
            iIdEntidad = r.iIdEntidad,
            iIdCiudadano = r.iIdCiudadano,
            vchAlias = r.vchAlias ?? string.Empty,
            iIdOpinionPadre = r.iIdOpinionPadre,
            nvchTexto = r.nvchTexto ?? string.Empty,
            dtFechaCreacion = r.dtFechaCreacion ?? DateTime.MinValue
        };

        private static Opinion MapSave(spSaveOpinion.Result r) => new()
        {
            iIdOpinion = r.iIdOpinion,
            vchTipoEntidad = r.vchTipoEntidad ?? string.Empty,
            iIdEntidad = r.iIdEntidad,
            iIdCiudadano = r.iIdCiudadano,
            vchAlias = r.vchAlias ?? string.Empty,
            iIdOpinionPadre = r.iIdOpinionPadre,
            nvchTexto = r.nvchTexto ?? string.Empty,
            dtFechaCreacion = r.dtFechaCreacion ?? DateTime.UtcNow
        };
    }
}
