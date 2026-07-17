using Microsoft.SemanticKernel;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using EngramaCoreStandar.Extensions;

namespace SantiagoConectaIA.API.SemanticKernel.Plugins
{
    public class EmpresasPlugin
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EmpresasPlugin(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        [KernelFunction]
        [Description("Busca empresas, comercios, negocios o servicios locales registrados en el municipio de Santiago Papasquiaro. Utiliza esta función cuando el usuario pregunte por restaurantes, hoteles, ferreterías, tiendas o cualquier negocio local.")]
        public async Task<string> BuscarEmpresas(
            [Description("Palabra clave para buscar empresas (ej. 'restaurante', 'hotel', 'ferretería', 'tienda'). Puede dejarse vacía para obtener todas las empresas activas.")]
            string query = "",
            [Description("Número máximo de resultados. Por defecto es 5.")]
            int limit = 5)
        {
            using var scope = _scopeFactory.CreateScope();
            var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

            var result = await empresasDomain.GetEmpresas(new PostGetEmpresas { bEstatus = true });

            if (!result.IsSuccess || result.Data == null)
            {
                return "No se pudieron obtener las empresas en este momento.";
            }

            var empresas = result.Data;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var cleanQuery = query.Trim().ToLower();
                empresas = empresas.Where(e =>
                    (e.vchNombreComercial != null && e.vchNombreComercial.ToLower().Contains(cleanQuery)) ||
                    (e.nvchDescripcion != null && e.nvchDescripcion.ToLower().Contains(cleanQuery))
                ).ToList();
            }

            var resultado = empresas.Take(limit).Select(e => new
            {
                e.iIdEmpresa,
                e.vchNombreComercial,
                e.vchSlogan,
                e.vchTelefono,
                e.vchCorreo,
                e.vchLogoUrl
            }).ToList();

            if (resultado.Count == 0)
            {
                return $"No se encontraron empresas con la palabra clave '{query}'.";
            }

            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene la información detallada de una empresa específica del municipio incluyendo descripción, misión, visión e historia. Requiere el ID de la empresa obtenido de BuscarEmpresas.")]
        public async Task<string> GetEmpresaDetalle(
            [Description("El ID de la empresa a consultar.")]
            int idEmpresa)
        {
            using var scope = _scopeFactory.CreateScope();
            var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

            var result = await empresasDomain.GetEmpresas(new PostGetEmpresas { iIdEmpresa = idEmpresa, bEstatus = true });

            if (!result.IsSuccess || result.Data == null || !result.Data.Any())
            {
                return $"No se encontró la empresa con ID {idEmpresa}.";
            }

            var empresa = result.Data.First();
            var resultado = new
            {
                empresa.iIdEmpresa,
                empresa.vchNombreComercial,
                empresa.vchSlogan,
                empresa.nvchDescripcion,
                empresa.nvchMision,
                empresa.nvchVision,
                empresa.nvchHistoria,
                empresa.vchTelefono,
                empresa.vchCorreo,
                empresa.vchLogoUrl
            };

            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene las ubicaciones físicas (direcciones, coordenadas) de una empresa del municipio. Utiliza esta función cuando el usuario pregunte por la dirección o la ubicación de un negocio.")]
        public async Task<string> GetEmpresaUbicaciones(
            [Description("El ID de la empresa para consultar sus ubicaciones.")]
            int idEmpresa)
        {
            using var scope = _scopeFactory.CreateScope();
            var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

            var result = await empresasDomain.GetEmpresaUbicaciones(new PostGetEmpresaUbicaciones { iIdEmpresa = idEmpresa });

            if (!result.IsSuccess || result.Data == null || !result.Data.Any())
            {
                return $"No se encontraron ubicaciones para la empresa con ID {idEmpresa}.";
            }

            return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene las redes sociales y enlaces web de una empresa del municipio (Facebook, Instagram, WhatsApp, sitio web, etc.).")]
        public async Task<string> GetEmpresaRedesSociales(
            [Description("El ID de la empresa para consultar sus redes sociales.")]
            int idEmpresa)
        {
            using var scope = _scopeFactory.CreateScope();
            var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

            var result = await empresasDomain.GetEmpresaRedesSociales(new PostGetEmpresaRedesSociales { iIdEmpresa = idEmpresa });

            if (!result.IsSuccess || result.Data == null || !result.Data.Any())
            {
                return $"No se encontraron redes sociales para la empresa con ID {idEmpresa}.";
            }

            return JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene los productos y servicios que ofrece una empresa del municipio, organizados por categoría. Utiliza esta función cuando el usuario pregunte qué vende o qué servicios ofrece un negocio.")]
        public async Task<string> GetEmpresaProductosServicios(
            [Description("El ID de la empresa para consultar sus productos y servicios.")]
            int idEmpresa)
        {
            using var scope = _scopeFactory.CreateScope();
            var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

            var categoriasRes = await empresasDomain.GetCategoriasPorEmpresa(new PostGetCategoriasPorEmpresa { iIdEmpresa = idEmpresa });

            if (!categoriasRes.IsSuccess || categoriasRes.Data == null || !categoriasRes.Data.Any())
            {
                return $"No se encontraron productos o servicios para la empresa con ID {idEmpresa}.";
            }

            var resultado = new List<object>();
            foreach (var cat in categoriasRes.Data)
            {
                var prodRes = await empresasDomain.GetProductosPorCategoria(new PostGetProductosPorCategoria { iIdCategoriaCat = cat.iIdCategoriaCat });
                if (prodRes.IsSuccess && prodRes.Data != null && prodRes.Data.Any())
                {
                    resultado.Add(new
                    {
                        Categoria = cat.vchNombre,
                            Productos = prodRes.Data.Select(p => new
                            {
                                p.iIdProducto,
                                p.vchNombre,
                                p.nvchDescripcionCorta,
                                p.mPrecio,
                                p.vchImagenUrl
                            })
                    });
                }
            }

            if (resultado.Count == 0)
            {
                return $"No se encontraron productos o servicios para la empresa con ID {idEmpresa}.";
            }

            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }

        [KernelFunction]
        [Description("Obtiene la información de contacto resumida de una empresa (teléfono, correo electrónico y dirección).")]
        public async Task<string> GetEmpresaContacto(
            [Description("El ID de la empresa para consultar su información de contacto.")]
            int idEmpresa)
        {
            using var scope = _scopeFactory.CreateScope();
            var empresasDomain = scope.ServiceProvider.GetRequiredService<IEmpresasDomain>();

            var empRes = await empresasDomain.GetEmpresas(new PostGetEmpresas { iIdEmpresa = idEmpresa, bEstatus = true });
            if (!empRes.IsSuccess || empRes.Data == null || !empRes.Data.Any())
            {
                return $"No se encontró la empresa con ID {idEmpresa}.";
            }

            var empresa = empRes.Data.First();
            var ubicacionesRes = await empresasDomain.GetEmpresaUbicaciones(new PostGetEmpresaUbicaciones { iIdEmpresa = idEmpresa });

            var resultado = new
            {
                empresa.vchNombreComercial,
                empresa.vchTelefono,
                empresa.vchCorreo,
                Ubicaciones = ubicacionesRes.IsSuccess && ubicacionesRes.Data != null
                    ? ubicacionesRes.Data.Select(u => new { u.vchDireccion, u.flLatitud, u.flLongitud })
                    : null
            };

            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
