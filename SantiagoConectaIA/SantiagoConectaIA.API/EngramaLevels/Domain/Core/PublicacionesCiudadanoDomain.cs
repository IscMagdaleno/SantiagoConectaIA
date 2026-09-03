using EngramaCoreStandar.Results;
using Newtonsoft.Json;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.PublicacionCiudadanoModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class PublicacionesCiudadanoDomain : IPublicacionesCiudadanoDomain
    {
        private readonly IPublicacionesCiudadanoRepository _repository;
        private readonly IAzureBlobDomain _azureBlobDomain;

        public PublicacionesCiudadanoDomain(
            IPublicacionesCiudadanoRepository repository,
            IAzureBlobDomain azureBlobDomain)
        {
            _repository = repository;
            _azureBlobDomain = azureBlobDomain;
        }

        public async Task<Response<PublicacionCiudadano>> SavePublicacionCiudadano(PostSavePublicacionCiudadano postModel, int iIdCiudadano)
        {
            try
            {
                if (iIdCiudadano <= 0)
                {
                    return Response<PublicacionCiudadano>.BadResult("Sesión inválida o ciudadano no autenticado.", new PublicacionCiudadano());
                }

                if (string.IsNullOrWhiteSpace(postModel?.nvchContenidoTexto))
                {
                    return Response<PublicacionCiudadano>.BadResult("El contenido de la publicación es requerido.", new PublicacionCiudadano());
                }

                var finalUrls = new List<string>(postModel.ImagenesUrls ?? new List<string>());

                // Procesar imágenes base64 si existen y subirlas a Azure Blob Storage
                if (postModel.ImagenesBase64 != null && postModel.ImagenesBase64.Any())
                {
                    foreach (var base64Raw in postModel.ImagenesBase64)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(base64Raw)) continue;

                            string base64Data = base64Raw;
                            string extension = ".jpg";

                            if (base64Raw.Contains(","))
                            {
                                var parts = base64Raw.Split(',');
                                if (parts[0].Contains("png")) extension = ".png";
                                else if (parts[0].Contains("webp")) extension = ".webp";
                                base64Data = parts[1];
                            }

                            byte[] imageBytes = Convert.FromBase64String(base64Data);
                            using var stream = new MemoryStream(imageBytes);
                            string uniqueFileName = $"pub_{iIdCiudadano}_{Guid.NewGuid():N}{extension}";

                            var uploadResult = await _azureBlobDomain.UploadDocument(stream, uniqueFileName, "publicaciones-ciudadano");
                            if (uploadResult != null && uploadResult.IsSuccess && !string.IsNullOrWhiteSpace(uploadResult.Data?.URL))
                            {
                                finalUrls.Add(uploadResult.Data.URL);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al subir imagen de publicación a Azure Blob: {ex.Message}");
                        }
                    }
                }

                var req = new spSavePublicacionCiudadano.Request
                {
                    iIdPublicacion = postModel.iIdPublicacion,
                    iIdCiudadano = iIdCiudadano,
                    nvchTitulo = postModel.nvchTitulo?.Trim(),
                    nvchContenidoTexto = postModel.nvchContenidoTexto.Trim(),
                    vchCategoriaPublicacion = string.IsNullOrWhiteSpace(postModel.vchCategoriaPublicacion) ? "Comunidad" : postModel.vchCategoriaPublicacion.Trim()
                };

                var res = await _repository.spSavePublicacionCiudadano(req);
                if (!res.bResult || res.iIdPublicacion <= 0)
                {
                    return Response<PublicacionCiudadano>.BadResult(res.vchMessage ?? "Error al guardar la publicación.", new PublicacionCiudadano());
                }

                // Guardar cada imagen de forma individual usando spSaveImagenRegistro
                int orderIndex = 0;
                foreach (var url in finalUrls)
                {
                    if (string.IsNullOrWhiteSpace(url)) continue;

                    try
                    {
                        await _repository.spSaveImagenRegistro(new spSaveImagenRegistro.Request
                        {
                            vchTablaOrigen = "PublicacionCiudadano",
                            iIdRegistro = res.iIdPublicacion,
                            vchUrlImagen = url,
                            vchDescripcion = string.Empty,
                            iOrden = orderIndex++
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al registrar imagen en spSaveImagenRegistro: {ex.Message}");
                    }
                }

                var entity = new PublicacionCiudadano
                {
                    iIdPublicacion = res.iIdPublicacion,
                    iIdCiudadano = iIdCiudadano,
                    nvchTitulo = req.nvchTitulo,
                    nvchContenidoTexto = req.nvchContenidoTexto,
                    vchCategoriaPublicacion = req.vchCategoriaPublicacion,
                    dtFechaCreacion = DateTime.UtcNow,
                    bActiva = true,
                    vchPrimeraImagenUrl = finalUrls.FirstOrDefault(),
                    Imagenes = finalUrls.Select((url, idx) => new ImagenPublicacion
                    {
                        iIdPublicacion = res.iIdPublicacion,
                        nvchUrlImagen = url,
                        iOrdenVisualizacion = idx + 1
                    }).ToList()
                };

                return new Response<PublicacionCiudadano>
                {
                    Data = entity,
                    IsSuccess = true,
                    Message = res.vchMessage
                };
            }
            catch (Exception ex)
            {
                return Response<PublicacionCiudadano>.BadResult(ex.Message, new PublicacionCiudadano());
            }
        }

        public async Task<Response<IEnumerable<PublicacionCiudadano>>> GetPublicacionesCiudadano(PostGetPublicacionesCiudadano postModel)
        {
            try
            {
                var req = new spGetPublicacionesCiudadano.Request
                {
                    iPage = postModel?.iPage < 1 ? 1 : postModel!.iPage,
                    iPageSize = postModel?.iPageSize < 1 ? 10 : postModel!.iPageSize,
                    vchCategoria = postModel?.vchCategoria,
                    nvchBusqueda = postModel?.nvchBusqueda
                };

                var results = await _repository.spGetPublicacionesCiudadano(req);
                var list = new List<PublicacionCiudadano>();

                foreach (var r in results.Where(x => x.bResult && x.iIdPublicacion > 0))
                {
                    var imagenes = new List<ImagenPublicacion>();
                    if (!string.IsNullOrWhiteSpace(r.nvchImagenesJson))
                    {
                        try
                        {
                            var parsed = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(r.nvchImagenesJson);
                            if (parsed != null)
                            {
                                int idx = 1;
                                foreach (var item in parsed)
                                {
                                    if (item.TryGetValue("nvchUrlImagen", out var u) && !string.IsNullOrWhiteSpace(u))
                                    {
                                        imagenes.Add(new ImagenPublicacion
                                        {
                                            iIdPublicacion = r.iIdPublicacion,
                                            nvchUrlImagen = u,
                                            iOrdenVisualizacion = idx++
                                        });
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // fallback si era lista de strings
                            try
                            {
                                var strList = JsonConvert.DeserializeObject<List<string>>(r.nvchImagenesJson);
                                if (strList != null)
                                {
                                    int idx = 1;
                                    imagenes.AddRange(strList.Select(u => new ImagenPublicacion
                                    {
                                        iIdPublicacion = r.iIdPublicacion,
                                        nvchUrlImagen = u,
                                        iOrdenVisualizacion = idx++
                                    }));
                                }
                            }
                            catch { }
                        }
                    }

                    list.Add(new PublicacionCiudadano
                    {
                        iIdPublicacion = r.iIdPublicacion,
                        iIdCiudadano = r.iIdCiudadano,
                        vchAliasCiudadano = r.vchAliasCiudadano,
                        vchAvatarCiudadano = r.vchAvatarCiudadano,
                        bCiudadanoVerificado = r.bCiudadanoVerificado,
                        nvchTitulo = r.nvchTitulo,
                        nvchContenidoTexto = r.nvchContenidoTexto,
                        vchCategoriaPublicacion = r.vchCategoriaPublicacion,
                        dtFechaCreacion = r.dtFechaCreacion,
                        bActiva = r.bActiva,
                        iTotalComentarios = r.iTotalComentarios,
                        vchPrimeraImagenUrl = r.vchPrimeraImagenUrl,
                        Imagenes = imagenes,
                        iTotalRegistros = r.iTotalRegistros
                    });
                }

                return new Response<IEnumerable<PublicacionCiudadano>>
                {
                    Data = list,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<PublicacionCiudadano>>.BadResult(ex.Message, new List<PublicacionCiudadano>());
            }
        }

        public async Task<Response<IEnumerable<PublicacionCiudadano>>> GetMisPublicacionesCiudadano(PostGetMisPublicacionesCiudadano postModel, int iIdCiudadano)
        {
            try
            {
                if (iIdCiudadano <= 0)
                {
                    return Response<IEnumerable<PublicacionCiudadano>>.BadResult("Ciudadano inválido.", new List<PublicacionCiudadano>());
                }

                var req = new spGetMisPublicacionesCiudadano.Request
                {
                    iIdCiudadano = iIdCiudadano,
                    iPage = postModel?.iPage < 1 ? 1 : postModel!.iPage,
                    iPageSize = postModel?.iPageSize < 1 ? 20 : postModel!.iPageSize
                };

                var results = await _repository.spGetMisPublicacionesCiudadano(req);
                var list = new List<PublicacionCiudadano>();

                foreach (var r in results.Where(x => x.bResult && x.iIdPublicacion > 0))
                {
                    var imagenes = new List<ImagenPublicacion>();
                    if (!string.IsNullOrWhiteSpace(r.nvchImagenesJson))
                    {
                        try
                        {
                            var parsed = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(r.nvchImagenesJson);
                            if (parsed != null)
                            {
                                int idx = 1;
                                foreach (var item in parsed)
                                {
                                    if (item.TryGetValue("nvchUrlImagen", out var u) && !string.IsNullOrWhiteSpace(u))
                                    {
                                        imagenes.Add(new ImagenPublicacion
                                        {
                                            iIdPublicacion = r.iIdPublicacion,
                                            nvchUrlImagen = u,
                                            iOrdenVisualizacion = idx++
                                        });
                                    }
                                }
                            }
                        }
                        catch
                        {
                            try
                            {
                                var strList = JsonConvert.DeserializeObject<List<string>>(r.nvchImagenesJson);
                                if (strList != null)
                                {
                                    int idx = 1;
                                    imagenes.AddRange(strList.Select(u => new ImagenPublicacion
                                    {
                                        iIdPublicacion = r.iIdPublicacion,
                                        nvchUrlImagen = u,
                                        iOrdenVisualizacion = idx++
                                    }));
                                }
                            }
                            catch { }
                        }
                    }

                    list.Add(new PublicacionCiudadano
                    {
                        iIdPublicacion = r.iIdPublicacion,
                        iIdCiudadano = r.iIdCiudadano,
                        vchAliasCiudadano = r.vchAliasCiudadano,
                        vchAvatarCiudadano = r.vchAvatarCiudadano,
                        bCiudadanoVerificado = r.bCiudadanoVerificado,
                        nvchTitulo = r.nvchTitulo,
                        nvchContenidoTexto = r.nvchContenidoTexto,
                        vchCategoriaPublicacion = r.vchCategoriaPublicacion,
                        dtFechaCreacion = r.dtFechaCreacion,
                        bActiva = r.bActiva,
                        iTotalComentarios = r.iTotalComentarios,
                        vchPrimeraImagenUrl = r.vchPrimeraImagenUrl,
                        Imagenes = imagenes,
                        iTotalRegistros = r.iTotalRegistros
                    });
                }

                return new Response<IEnumerable<PublicacionCiudadano>>
                {
                    Data = list,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<PublicacionCiudadano>>.BadResult(ex.Message, new List<PublicacionCiudadano>());
            }
        }

        public async Task<Response<string>> DeletePublicacionCiudadano(PostDeletePublicacionCiudadano postModel, int iIdCiudadano)
        {
            try
            {
                if (iIdCiudadano <= 0 || postModel == null || postModel.iIdPublicacion <= 0)
                {
                    return Response<string>.BadResult("Parámetros inválidos para eliminar publicación.", string.Empty);
                }

                var req = new spDeletePublicacionCiudadano.Request
                {
                    iIdPublicacion = postModel.iIdPublicacion,
                    iIdCiudadano = iIdCiudadano
                };

                var res = await _repository.spDeletePublicacionCiudadano(req);
                if (!res.bResult)
                {
                    return Response<string>.BadResult(res.vchMessage ?? "Error al eliminar la publicación.", string.Empty);
                }

                return new Response<string>
                {
                    Data = "Ok",
                    IsSuccess = true,
                    Message = res.vchMessage
                };
            }
            catch (Exception ex)
            {
                return Response<string>.BadResult(ex.Message, string.Empty);
            }
        }
    }
}
