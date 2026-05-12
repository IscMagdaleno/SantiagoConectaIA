using System.Collections.Generic;
using System.Threading.Tasks;
using SantiagoConectaIA.DAL.Models;

namespace SantiagoConectaIA.DAL.Provider
{
    public interface ICatalogosProvider
    {
        Task<List<TipoDato>> GetTipoDatosAsync();
    }
}
