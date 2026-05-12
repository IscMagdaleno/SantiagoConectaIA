using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL.Models;

namespace SantiagoConectaIA.DAL.Provider
{
    public class CatalogosProvider : ICatalogosProvider
    {
        private readonly EngramaContext _context;

        public CatalogosProvider(EngramaContext context)
        {
            _context = context;
        }

        public async Task<List<TipoDato>> GetTipoDatosAsync()
        {
            // Consulta LINQ para obtener los tipos de datos activos
            return await _context.TipoDatos.ToListAsync();
        }
    }
}
