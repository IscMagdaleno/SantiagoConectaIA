using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.Share.Objects.Common;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.PostModels.OficinasModule;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.OficinasArea.Utiles
{
    public class MainOficinas
    {
        private readonly string urlOficinas = "api/Oficinas";

        #region INJECTS
        private readonly IHttpService _httpService;
        private readonly MapperHelper _mapper;
        private readonly IValidaServicioService _validaServicioService;
        #endregion

        #region PROPIEDADES
        public List<Oficina> LstOficinas { get; set; } = new List<Oficina>();
        public Oficina OficinaSelected { get; set; } = new Oficina();
        #endregion

        public MainOficinas(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
        {
            _httpService = httpService;
            _mapper = mapper;
            _validaServicioService = validaServicioService;
        }

        public async Task<SeverityMessage> PostGetOficinas()
        {
            var APIUrl = urlOficinas + "/PostGetOficinas";
            var model = new PostGetOficinas();
            var response = await _httpService.Post<PostGetOficinas, Response<List<Oficina>>>(APIUrl, model);
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstOficinas = data.ToList());
            return validation;
        }

        public async Task<SeverityMessage> PostSaveOficina()
        {
            var APIUrl = urlOficinas + "/PostSaveOficina";
            var model = _mapper.Get<Oficina, PostSaveOficina>(OficinaSelected);
            var response = await _httpService.Post<PostSaveOficina, Response<Oficina>>(APIUrl, model);
            var validacion = _validaServicioService.ValidadionServicio(response,
                onSuccess: async data => 
                {
                    OficinaSelected.iIdOficina = data.iIdOficina;
                    await PostGetOficinas();
                });

            if (validacion.bResult && string.IsNullOrEmpty(validacion.vchMessage))
            {
                validacion.vchMessage = "Oficina guardada correctamente.";
            }

            return validacion;
        }
    }
}
