using EngramaCoreStandar.Dapper;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class NoticiasRepository : INoticiasRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public NoticiasRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetNoticias.Result>> spGetNoticias(spGetNoticias.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetNoticias.Result, spGetNoticias.Request>(daoModel, "");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetNoticias.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveNoticia.Result> spSaveNoticia(spSaveNoticia.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveNoticia.Result, spSaveNoticia.Request>(daoModel, "");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveNoticia.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveNoticiaImagen.Result> spSaveNoticiaImagen(spSaveNoticiaImagen.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveNoticiaImagen.Result, spSaveNoticiaImagen.Request>(daoModel, "");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveNoticiaImagen.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
