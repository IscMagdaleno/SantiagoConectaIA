using System.Collections.Generic;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;

namespace SantiagoConectaIA.Share.PostModels.EmpresasModulo
{
    public class CategoriaCatalogoConProductos : CategoriaCatalogo
    {
        public string TempId { get; set; } = System.Guid.NewGuid().ToString();
        public List<ProductoServicio> Productos { get; set; } = new List<ProductoServicio>();
    }

    public class PostSaveEmprendimientoFull
    {
        public Propietario Propietario { get; set; } = new Propietario();
        public Empresa Empresa { get; set; } = new Empresa();
        public List<EmpresaRedSocial> RedesSociales { get; set; } = new List<EmpresaRedSocial>();
        public List<EmpresaUbicacion> Ubicaciones { get; set; } = new List<EmpresaUbicacion>();
        public List<CategoriaCatalogoConProductos> Categorias { get; set; } = new List<CategoriaCatalogoConProductos>();
    }
}
