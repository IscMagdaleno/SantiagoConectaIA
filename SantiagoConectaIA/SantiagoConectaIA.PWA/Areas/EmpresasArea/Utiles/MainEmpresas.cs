using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper.Results;
using EngramaCoreStandar.Mapper;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles
{
    /// <summary>
    /// Esta clase actúa como el ViewModel para el Área de Empresas. 
    /// </summary>
    public class MainEmpresas
    {
        private string url = @"api/Empresas";

        private readonly IHttpService _httpService;
        private readonly MapperHelper _mapper;
        private readonly IValidaServicioService _validaServicioService;

        // Propiedades de Estado
        public List<Empresa> LstRegistros { get; set; }
        public Empresa RegistroSeleccionado { get; set; }

        public MainEmpresas(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
        {
            _httpService = httpService;
            _mapper = mapper;
            _validaServicioService = validaServicioService;

            LstRegistros = new List<Empresa>();
            RegistroSeleccionado = new Empresa();
        }

        public async Task<SeverityMessage> PostGetRegistros()
        {
            var APIUrl = url + "/PostGetEmpresas";
            var request = new PostGetEmpresas { bEstatus = true };
            var response = await _httpService.Post<PostGetEmpresas, Response<List<Empresa>>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstRegistros = data);
            return validation;
        }

        public async Task<SeverityMessage> PostSaveRegistro()
        {
            var APIUrl = url + "/PostSaveEmpresa";
            var request = _mapper.Get<Empresa, PostSaveEmpresa>(RegistroSeleccionado);
            var response = await _httpService.Post<PostSaveEmpresa, Response<Empresa>>(APIUrl, request);
            var validation = _validaServicioService.ValidadionServicio(response, 
                onSuccess: data => 
                {
                    RegistroSeleccionado = data;
                    _ = PostGetRegistros();
                });
            return validation;
        }
    }
}
