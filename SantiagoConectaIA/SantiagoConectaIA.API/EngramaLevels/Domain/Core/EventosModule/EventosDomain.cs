using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EventosModule;
using SantiagoConectaIA.Share.Objetos.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core.EventosModule
{
    public class EventosDomain : IEventosDomain
    {
        private readonly IEventosRepository _eventosRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public EventosDomain(IEventosRepository eventosRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
        {
            _eventosRepository = eventosRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<IEnumerable<Evento>>> GetEventos(PostGetEventos postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEventos, spGetEventos.Request>(postModel);
                var result = await _eventosRepository.spGetEventos(request);
                return _responseHelper.Validacion<spGetEventos.Result, Evento>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<Evento>>.BadResult(ex.Message, new List<Evento>());
            }
        }

        public async Task<Response<Evento>> SaveEvento(PostSaveEvento postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveEvento, spSaveEvento.Request>(postModel);
                var result = await _eventosRepository.spSaveEvento(request);
                var validation = _responseHelper.Validacion<spSaveEvento.Result, Evento>(result);

                if (validation.IsSuccess)
                {
                    postModel.iIdEvento = validation.Data.iIdEvento;
                    validation.Data = _mapperHelper.Get<PostSaveEvento, Evento>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<Evento>.BadResult(ex.Message, new Evento());
            }
        }

        public async Task<Response<IEnumerable<CategoriaEvento>>> GetCategoriaEventos(PostGetCategoriaEventos postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetCategoriaEventos, spGetCategoriaEventos.Request>(postModel);
                var result = await _eventosRepository.spGetCategoriaEventos(request);
                return _responseHelper.Validacion<spGetCategoriaEventos.Result, CategoriaEvento>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<CategoriaEvento>>.BadResult(ex.Message, new List<CategoriaEvento>());
            }
        }

        public async Task<Response<CategoriaEvento>> SaveCategoriaEvento(PostSaveCategoriaEvento postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveCategoriaEvento, spSaveCategoriaEvento.Request>(postModel);
                var result = await _eventosRepository.spSaveCategoriaEvento(request);
                var validation = _responseHelper.Validacion<spSaveCategoriaEvento.Result, CategoriaEvento>(result);

                if (validation.IsSuccess)
                {
                    postModel.iIdCategoriaEvento = validation.Data.iIdCategoriaEvento;
                    validation.Data = _mapperHelper.Get<PostSaveCategoriaEvento, CategoriaEvento>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<CategoriaEvento>.BadResult(ex.Message, new CategoriaEvento());
            }
        }

        public async Task<Response<IEnumerable<ImagenRegistro>>> GetImagenesRegistro(PostGetImagenesRegistro postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetImagenesRegistro, spGetImagenesRegistro.Request>(postModel);
                var result = await _eventosRepository.spGetImagenesRegistro(request);
                return _responseHelper.Validacion<spGetImagenesRegistro.Result, ImagenRegistro>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<ImagenRegistro>>.BadResult(ex.Message, new List<ImagenRegistro>());
            }
        }

        public async Task<Response<ImagenRegistro>> SaveImagenRegistro(PostSaveImagenRegistro postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveImagenRegistro, spSaveImagenRegistro.Request>(postModel);
                var result = await _eventosRepository.spSaveImagenRegistro(request);
                var validation = _responseHelper.Validacion<spSaveImagenRegistro.Result, ImagenRegistro>(result);

                if (validation.IsSuccess)
                {
                    postModel.iIdImagen = validation.Data.iIdImagen;
                    validation.Data = _mapperHelper.Get<PostSaveImagenRegistro, ImagenRegistro>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<ImagenRegistro>.BadResult(ex.Message, new ImagenRegistro());
            }
        }

        public async Task<Response<ImagenRegistro>> DeleteImagenRegistro(PostDeleteImagenRegistro postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostDeleteImagenRegistro, spDeleteImagenRegistro.Request>(postModel);
                var result = await _eventosRepository.spDeleteImagenRegistro(request);
                return _responseHelper.Validacion<spDeleteImagenRegistro.Result, ImagenRegistro>(result);
            }
            catch (Exception ex)
            {
                return Response<ImagenRegistro>.BadResult(ex.Message, new ImagenRegistro());
            }
        }

        public async Task<Response<EventoDetalle>> GetEventoDetalle(PostGetEventoDetalle postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEventoDetalle, spGetEventoDetalle.Request>(postModel);
                var result = await _eventosRepository.spGetEventoDetalle(request);
                var resultList = result.ToList();

                if (!resultList.Any() || !resultList.First().bResult)
                {
                    var msg = resultList.FirstOrDefault()?.vchMessage ?? "No se encontró el evento.";
                    return Response<EventoDetalle>.BadResult(msg, new EventoDetalle());
                }

                var first = resultList.First();
                var detalle = new EventoDetalle
                {
                    iIdEvento = first.iIdEvento,
                    iIdCategoriaEvento = first.iIdCategoriaEvento,
                    vchCategoriaNombre = first.vchCategoriaNombre,
                    vchNombre = first.vchNombre,
                    nvchDescripcion = first.nvchDescripcion,
                    dtFechaInicio = first.dtFechaInicio,
                    dtFechaFin = first.dtFechaFin,
                    vchLugar = first.vchLugar,
                    vchDireccion = first.vchDireccion,
                    flLatitud = first.flLatitud,
                    flLongitud = first.flLongitud,
                    vchImagenPortada = first.vchImagenPortada,
                    vchCostoTexto = first.vchCostoTexto,
                    vchOrganizador = first.vchOrganizador,
                    vchTelefono = first.vchTelefono,
                    vchCorreo = first.vchCorreo,
                    vchUrlOficial = first.vchUrlOficial,
                    bDestacado = first.bDestacado,
                    bEstatus = first.bEstatus,
                    dtFechaRegistro = first.dtFechaRegistro,
                    imagenes = resultList
                        .Where(r => r.iIdImagen > 0)
                        .Select(r => new ImagenRegistro
                        {
                            iIdImagen = r.iIdImagen,
                            vchTablaOrigen = "Eventos",
                            iIdRegistro = r.iIdEvento,
                            vchUrlImagen = r.vchUrlImagen,
                            vchDescripcion = r.vchDescripcionImagen,
                            iOrden = r.iOrden
                        })
                        .ToList()
                };

                return new Response<EventoDetalle>
                {
                    IsSuccess = true,
                    Data = detalle
                };
            }
            catch (Exception ex)
            {
                return Response<EventoDetalle>.BadResult(ex.Message, new EventoDetalle());
            }
        }

        public async Task<Response<IEnumerable<SucursalEvento>>> GetEventosSucursales(PostGetEventosSucursales postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostGetEventosSucursales, spGetEventosSucursales.Request>(postModel);
                var result = await _eventosRepository.spGetEventosSucursales(request);
                return _responseHelper.Validacion<spGetEventosSucursales.Result, SucursalEvento>(result);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<SucursalEvento>>.BadResult(ex.Message, new List<SucursalEvento>());
            }
        }

        public async Task<Response<SucursalEvento>> SaveSucursalEvento(PostSaveSucursalEvento postModel)
        {
            try
            {
                var request = _mapperHelper.Get<PostSaveSucursalEvento, spSaveSucursalEvento.Request>(postModel);
                var result = await _eventosRepository.spSaveSucursalEvento(request);
                var validation = _responseHelper.Validacion<spSaveSucursalEvento.Result, SucursalEvento>(result);

                if (validation.IsSuccess)
                {
                    postModel.iIdSucursalEvento = validation.Data.iIdSucursalEvento;
                    validation.Data = _mapperHelper.Get<PostSaveSucursalEvento, SucursalEvento>(postModel);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<SucursalEvento>.BadResult(ex.Message, new SucursalEvento());
            }
        }
    }
}
