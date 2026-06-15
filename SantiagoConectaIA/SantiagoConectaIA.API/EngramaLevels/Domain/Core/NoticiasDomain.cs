using AutoMapper;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NoticiaDto = SantiagoConectaIA.Share.Objects.NoticiasModule.Noticia;
using NoticiaMetadatoDto = SantiagoConectaIA.Share.Objects.NoticiasModule.NoticiaMetadato;
using NoticiaImagenDto = SantiagoConectaIA.Share.Objects.NoticiasModule.NoticiaImagen;
using CategoriaNoticiaDto = SantiagoConectaIA.Share.Objects.NoticiasModule.CategoriaNoticia;
using System.Text.Json;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class NoticiasDomain : INoticiasDomain
    {
        private readonly INoticiasRepository _noticiasRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public NoticiasDomain(INoticiasRepository noticiasRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
        {
            _noticiasRepository = noticiasRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<IEnumerable<NoticiaDto>>> GetNoticias(PostGetNoticias postModel)
        {
            try
            {
                var newsReq = _mapperHelper.Get<PostGetNoticias, spGetNoticias.Request>(postModel);
                var metaReq = new spGetNoticiaMetadatos.Request { bActivo = postModel.bActivo };

                var newsTask = _noticiasRepository.spGetNoticias(newsReq);
                var metaTask = _noticiasRepository.spGetNoticiaMetadatos(metaReq);

                await Task.WhenAll(newsTask, metaTask);

                var newsResults = newsTask.Result;
                var metaResults = metaTask.Result;

                // Validar error en resultados base
                var firstNews = newsResults.FirstOrDefault();
                if (firstNews != null && !firstNews.bResult)
                {
                    return Response<IEnumerable<NoticiaDto>>.BadResult(firstNews.vchMessage, new List<NoticiaDto>());
                }

                // Agrupar los metadatos obtenidos por Noticia y luego por Fila
                var metasByNews = metaResults
                    .GroupBy(m => m.iIdNoticia)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(m => m.iIdFila)
                              .Select(fg => new NoticiaFila
                              {
                                  iIdFila = fg.Key,
                                  iIdNoticia = g.Key,
                                  iOrden = fg.First().iFilaOrden,
                                  Metadatos = fg.Select(m => new NoticiaMetadatoDto
                                  {
                                      iIdMetadato = m.iIdMetadato,
                                      iIdNoticia = m.iIdNoticia,
                                      iIdFila = m.iIdFila,
                                      iIdTipoDato = (TipoDatoMetadato)m.iIdTipoDato,
                                      vchTitulo = m.vchTitulo,
                                      nvchValor = m.nvchValor,
                                      iOrden = m.iMetadatoOrden,
                                      iAncho = m.iAncho ?? 12,
                                      vchAlineacion = m.vchAlineacion ?? "none",
                                      vchAlto = m.vchAlto ?? "auto",
                                      nvchConfiguracion = m.nvchConfiguracion ?? "{}"
                                  }).OrderBy(m => m.iOrden).ToList()
                              })
                              .OrderBy(f => f.iOrden)
                              .ToList()
                    );

                // Agrupar la lista de noticias
                var noticiasList = newsResults
                    .GroupBy(n => n.iIdNoticia)
                    .Select(g => {
                        var first = g.First();

                        var categoryDto = first.iIdCategoria.HasValue ? new CategoriaNoticiaDto
                        {
                            iIdCategoria = first.iIdCategoria.Value,
                            vchNombre = first.vchCategoriaNombre,
                            vchColorHex = first.vchCategoriaColorHex
                        } : null;

                        var imagenes = g
                            .Where(x => x.iIdNoticiaImagen.HasValue && !string.IsNullOrEmpty(x.vchUrlImagen))
                            .Select(x => new NoticiaImagenDto
                            {
                                iIdNoticiaImagen = x.iIdNoticiaImagen.Value,
                                iIdNoticia = x.iIdNoticia,
                                vchUrlImagen = x.vchUrlImagen
                            })
                            .GroupBy(img => img.iIdNoticiaImagen)
                            .Select(imgGroup => imgGroup.First())
                            .ToList();

                        metasByNews.TryGetValue(first.iIdNoticia, out var filas);

                        return new NoticiaDto
                        {
                            iIdNoticia = first.iIdNoticia,
                            vchTitulo = first.vchTitulo,
                            vchTituloEn = first.vchTituloEn,
                            nvchContenido = first.nvchContenido,
                            nvchContenidoEn = first.nvchContenidoEn,
                            vchImagenPortada = first.vchImagenPortada,
                            dtFechaPublicacion = first.dtFechaPublicacion,
                            bActivo = first.bActivo,
                            iIdCategoria = first.iIdCategoria,
                            Categoria = categoryDto,
                            Imagenes = imagenes,
                            Filas = filas ?? new List<NoticiaFila>()
                        };
                    })
                    .ToList();

                return new Response<IEnumerable<NoticiaDto>>
                {
                    Data = noticiasList,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<NoticiaDto>>.BadResult(ex.Message, new List<NoticiaDto>());
            }
        }

        public async Task<Response<NoticiaDto>> SaveNoticia(PostSaveNoticia postModel)
        {
            try
            {
                // Mapear PostModel a Request
                var model = _mapperHelper.Get<PostSaveNoticia, spSaveNoticia.Request>(postModel);

                // Serializar listas hijas a JSON
                model.jsonImagenes = postModel.Imagenes != null && postModel.Imagenes.Any()
                    ? JsonSerializer.Serialize(postModel.Imagenes.Select(x => new { vchUrlImagen = x.vchUrlImagen }))
                    : null;

                model.jsonFilas = postModel.Filas != null && postModel.Filas.Any()
                    ? JsonSerializer.Serialize(postModel.Filas.Select(f => new {
                        iOrden = f.iOrden,
                        Metadatos = f.Metadatos.Select(m => new {
                            iIdTipoDato = (int)m.iIdTipoDato,
                            vchTitulo = m.vchTitulo,
                            nvchValor = m.nvchValor,
                            iOrden = m.iOrden,
                            iAncho = m.iAncho,
                            vchAlineacion = m.vchAlineacion,
                            vchAlto = m.vchAlto,
                            nvchConfiguracion = m.nvchConfiguracion
                        })
                    }))
                    : null;

                //Invocar al repositorio
                var result = await _noticiasRepository.spSaveNoticia(model);
                var validation = _responseHelper.Validacion<spSaveNoticia.Result, NoticiaDto>(result);

                if (validation.IsSuccess)
                {
                    var responseData = _mapperHelper.Get<PostSaveNoticia, NoticiaDto>(postModel);
                    responseData.iIdNoticia = validation.Data.iIdNoticia;
                    validation.Data = responseData;
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<NoticiaDto>.BadResult(ex.Message, new NoticiaDto());
            }
        }
    }
}
