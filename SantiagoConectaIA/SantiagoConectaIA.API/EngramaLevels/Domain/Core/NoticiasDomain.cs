using AutoMapper;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using SantiagoConectaIA.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NoticiaDto = SantiagoConectaIA.Share.Objects.NoticiasModule.Noticia;
using NoticiaMetadatoDto = SantiagoConectaIA.Share.Objects.NoticiasModule.NoticiaMetadato;
using NoticiaImagenDto = SantiagoConectaIA.Share.Objects.NoticiasModule.NoticiaImagen;
using CategoriaNoticiaDto = SantiagoConectaIA.Share.Objects.NoticiasModule.CategoriaNoticia;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class NoticiasDomain : INoticiasDomain
    {
        private readonly INoticiasRepository _noticiasRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;
        private readonly EngramaContext _context;

        public NoticiasDomain(INoticiasRepository noticiasRepository, MapperHelper mapperHelper, IResponseHelper responseHelper, EngramaContext context)
        {
            _noticiasRepository = noticiasRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
            _context = context;
        }

        public async Task<Response<IEnumerable<NoticiaDto>>> GetNoticias(PostGetNoticias postModel)
        {
            try
            {
                var query = _context.Noticias
                    .Include(n => n.IIdCategoriaNavigation)
                    .Include(n => n.NoticiaFilas)
                        .ThenInclude(f => f.NoticiaMetadatos)
                    .Include(n => n.NoticiasImagenes)
                    .AsQueryable();

                if (postModel.bActivo.HasValue)
                {
                    query = query.Where(n => n.BActivo == postModel.bActivo.Value);
                }

                var noticiasDb = await query.OrderByDescending(n => n.DtFechaPublicacion).ToListAsync();

                var list = noticiasDb.Select(n => new NoticiaDto
                {
                    iIdNoticia = n.IIdNoticia,
                    vchTitulo = n.VchTitulo,
                    vchTituloEn = n.VchTituloEn,
                    nvchContenido = n.NvchContenido,
                    nvchContenidoEn = n.NvchContenidoEn,
                    vchImagenPortada = n.VchImagenPortada,
                    dtFechaPublicacion = n.DtFechaPublicacion,
                    bActivo = n.BActivo,
                    iIdCategoria = n.IIdCategoria,
                    Categoria = n.IIdCategoriaNavigation != null ? new CategoriaNoticiaDto
                    {
                        iIdCategoria = n.IIdCategoriaNavigation.IIdCategoria,
                        vchNombre = n.IIdCategoriaNavigation.VchNombre,
                        vchColorHex = n.IIdCategoriaNavigation.VchColorHex
                    } : null,
                    Imagenes = n.NoticiasImagenes.Select(img => new NoticiaImagenDto
                    {
                        iIdNoticiaImagen = img.IIdNoticiaImagen,
                        iIdNoticia = img.IIdNoticia,
                        vchUrlImagen = img.VchUrlImagen
                    }).ToList(),
                    Filas = n.NoticiaFilas.OrderBy(f => f.IOrden).Select(f => new SantiagoConectaIA.Share.Objects.NoticiasModule.NoticiaFila
                    {
                        iIdFila = f.IIdFila,
                        iIdNoticia = f.IIdNoticia,
                        iOrden = f.IOrden,
                        Metadatos = f.NoticiaMetadatos.OrderBy(m => m.IOrden).Select(m => new NoticiaMetadatoDto
                        {
                            iIdMetadato = m.IIdMetadato,
                            iIdNoticia = m.IIdNoticia,
                            iIdFila = m.IIdFila,
                            iIdTipoDato = (TipoDatoMetadato)m.IIdTipoDato,
                            vchTitulo = m.VchTitulo,
                            nvchValor = m.NvchValor,
                            iOrden = m.IOrden,
                            iAncho = m.IAncho ?? 12,
                            vchAlineacion = m.VchAlineacion ?? "none",
                            vchAlto = m.VchAlto ?? "auto",
                            nvchConfiguracion = m.NvchConfiguracion ?? "{}"
                        }).ToList()
                    }).ToList()
                }).ToList();

                return new Response<IEnumerable<NoticiaDto>>
                {
                    Data = list,
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
                // 1. Guardar la noticia base usando el SP existente
                var model = _mapperHelper.Get<PostSaveNoticia, spSaveNoticia.Request>(postModel);
                var result = await _noticiasRepository.spSaveNoticia(model);
                var validation = _responseHelper.Validacion<spSaveNoticia.Result, NoticiaDto>(result);

                if (validation.IsSuccess)
                {
                    int noticiaId = validation.Data.iIdNoticia;

                    // 2. Actualizar iIdCategoria y las listas hijas (Imágenes y Metadatos) usando EF Core
                    var noticiaEntity = await _context.Noticias
                        .FirstOrDefaultAsync(n => n.IIdNoticia == noticiaId);

                    if (noticiaEntity != null)
                    {
                        // Guardar Categoría
                        noticiaEntity.IIdCategoria = postModel.iIdCategoria;

                        // Guardar Imágenes (Eliminar y recrear)
                        var existingImages = _context.NoticiasImagenes.Where(x => x.IIdNoticia == noticiaId);
                        _context.NoticiasImagenes.RemoveRange(existingImages);

                        foreach (var img in postModel.Imagenes)
                        {
                            _context.NoticiasImagenes.Add(new DAL.Models.NoticiasImagene
                            {
                                IIdNoticia = noticiaId,
                                VchUrlImagen = img.vchUrlImagen
                            });
                        }

                        // Guardar Filas y Metadatos (Eliminar y recrear)
                        var existingMetadatos = _context.NoticiaMetadatos.Where(x => x.IIdNoticia == noticiaId);
                        var existingFilas = _context.NoticiaFilas.Where(x => x.IIdNoticia == noticiaId);
                        
                        _context.NoticiaMetadatos.RemoveRange(existingMetadatos);
                        _context.NoticiaFilas.RemoveRange(existingFilas);

                        if (postModel.Filas != null)
                        {
                            foreach (var fila in postModel.Filas)
                            {
                                var nuevaFila = new DAL.Models.NoticiaFila
                                {
                                    IIdNoticia = noticiaId,
                                    IOrden = fila.iOrden
                                };
                                _context.NoticiaFilas.Add(nuevaFila);
                                await _context.SaveChangesAsync(); // Para obtener el IIdFila

                                if (fila.Metadatos != null)
                                {
                                    foreach (var meta in fila.Metadatos)
                                    {
                                        _context.NoticiaMetadatos.Add(new DAL.Models.NoticiaMetadato
                                        {
                                            IIdNoticia = noticiaId,
                                            IIdFila = nuevaFila.IIdFila,
                                            IIdTipoDato = (int)meta.iIdTipoDato,
                                            VchTitulo = meta.vchTitulo,
                                            NvchValor = meta.nvchValor,
                                            IOrden = meta.iOrden,
                                            IAncho = meta.iAncho ?? 12,
                                            VchAlineacion = meta.vchAlineacion ?? "none",
                                            VchAlto = meta.vchAlto ?? "auto",
                                            NvchConfiguracion = meta.nvchConfiguracion
                                        });
                                    }
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    // Actualizar el modelo devuelto para que contenga las listas y categorías nuevas
                    postModel.iIdNoticia = noticiaId;
                    
                    var responseData = _mapperHelper.Get<PostSaveNoticia, NoticiaDto>(postModel);
                    if (postModel.iIdCategoria.HasValue)
                    {
                        var cat = await _context.CategoriaNoticia.FirstOrDefaultAsync(c => c.IIdCategoria == postModel.iIdCategoria.Value);
                        if (cat != null)
                        {
                            responseData.Categoria = new CategoriaNoticiaDto
                            {
                                iIdCategoria = cat.IIdCategoria,
                                vchNombre = cat.VchNombre,
                                vchColorHex = cat.VchColorHex
                            };
                        }
                    }
                    responseData.Filas = postModel.Filas;
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
