using EngramaCoreStandar.Results;
using EngramaCoreStandar.Servicios;
using EngramaCoreStandar.Mapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.PWA.Areas.MockupArea.Utiles
{
    /// <summary>
    /// Esta clase actúa como el ViewModel para el Área. 
    /// Recuerda registrarla en Program.cs: builder.Services.AddScoped<MainMockup>();
    /// </summary>
    public class MainMockup
    {
        private string url = @"api/TuControladorMockup";

        private readonly IHttpService _httpService;
        private readonly MapperHelper _mapper;
        private readonly IValidaServicioService _validaServicioService;

        // Propiedades de Estado
        // TODO: Reemplazar 'object' por tu modelo real (Ej. 'Noticia', 'Tramite')
        public List<object> LstRegistros { get; set; }
        public object RegistroSeleccionado { get; set; }

        public MainMockup(IHttpService httpService, MapperHelper mapper, IValidaServicioService validaServicioService)
        {
            _httpService = httpService;
            _mapper = mapper;
            _validaServicioService = validaServicioService;

            LstRegistros = new List<object>();
            RegistroSeleccionado = new object();
        }

        public async Task<SeverityMessage> PostGetRegistros()
        {
            var APIUrl = url + "/PostGetRegistros";
            var response = await _httpService.Post<object, Response<List<object>>>(APIUrl, new { });
            var validation = _validaServicioService.ValidadionServicio(response, onSuccess: data => LstRegistros = data);
            return validation;
        }

        public async Task<SeverityMessage> PostSaveRegistro()
        {
            var APIUrl = url + "/PostSaveRegistro";
            var response = await _httpService.Post<object, Response<object>>(APIUrl, RegistroSeleccionado);
            var validation = _validaServicioService.ValidadionServicio(response, 
                onSuccess: data => 
                {
                    RegistroSeleccionado = data;
                    PostGetRegistros(); // Refrescar lista
                });
            return validation;
        }
    }
}
