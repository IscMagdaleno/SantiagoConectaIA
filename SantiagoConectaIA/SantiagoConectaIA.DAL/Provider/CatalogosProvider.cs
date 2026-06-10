using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<Catalogo>> GetTipoDatosAsync(string groupAlias)
        {
            // Buscar el grupo por su alias
            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Alias == groupAlias);
            if (grupo == null) return new List<Catalogo>();

            // Obtener los catálogos vinculados a este grupo
            return await _context.Catalogos
                                 .Where(x => x.IdGrupo == grupo.IdGrupo)
                                 .ToListAsync();
        }

        public async Task<Parametro> GetParametroByAliasAsync(string alias)
        {
            return await _context.Parametros
                                 .FirstOrDefaultAsync(p => p.NvchAlias == alias && p.BHabilitado);
        }
    }
}
