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

        public async Task<Response<IEnumerable<Noticia>>> GetNoticias(PostGetNoticias postModel)
        {
            try
            {
                var model = _mapperHelper.Get<PostGetNoticias, spGetNoticias.Request>(postModel);
                var result = await _noticiasRepository.spGetNoticias(model);

                // Group by Noticia
                var groupedNews = result
                    .GroupBy(x => x.iIdNoticia)
                    .Select(g => 
                    {
                        var first = g.First();
                        var noticia = _mapperHelper.Get<spGetNoticias.Result, Noticia>(first);
                        
                        // Populate Images
                        noticia.Imagenes = g
                            .Where(x => x.iIdNoticiaImagen.HasValue && !string.IsNullOrEmpty(x.vchUrlImagen))
                            .Select(x => new NoticiaImagen 
                            { 
                                iIdNoticiaImagen = x.iIdNoticiaImagen.Value,
                                iIdNoticia = x.iIdNoticia,
                                vchUrlImagen = x.vchUrlImagen 
                            })
                            .ToList();
                            
                        return noticia;
                    })
                    .ToList();

                return new Response<IEnumerable<Noticia>>
                {
                    Data = groupedNews,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Noticia>>.BadResult(ex.Message, new List<Noticia>());
            }
        }

        public async Task<Response<Noticia>> SaveNoticia(PostSaveNoticia postModel)
        {
            try
            {
                // 1. Save the Noticia itself
                var model = _mapperHelper.Get<PostSaveNoticia, spSaveNoticia.Request>(postModel);
                var result = await _noticiasRepository.spSaveNoticia(model);
                var validation = _responseHelper.Validacion<spSaveNoticia.Result, Noticia>(result);

                if (validation.IsSuccess)
                {
                    // 2. If Noticia save was successful, save the images
                    int noticiaId = validation.Data.iIdNoticia;

                    foreach (var img in postModel.Imagenes)
                    {
                        var imgRequest = new spSaveNoticiaImagen.Request
                        {
                            iIdNoticia = noticiaId,
                            vchUrlImagen = img.vchUrlImagen
                        };
                        await _noticiasRepository.spSaveNoticiaImagen(imgRequest);
                    }
                    
                    // Update the returned data with the ID (and potentially images if we re-queried, but for now just ID is critical)
                     postModel.iIdNoticia = noticiaId;
                     validation.Data = _mapperHelper.Get<PostSaveNoticia, Noticia>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<Noticia>.BadResult(ex.Message, new Noticia());
            }
        }
    }
}
