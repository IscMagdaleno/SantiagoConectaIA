using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;

namespace SantiagoConectaIA.Share.Utilities
{
    public static class EmprendimientoTextoBuilder
    {
        public static string ArmarInformacion(PostSaveEmprendimientoFull detalle)
        {
            var empresa = detalle.Empresa ?? new Empresa();
            var sb = new StringBuilder();

            AgregarCampo(sb, "Nombre comercial", empresa.vchNombreComercial);
            AgregarCampo(sb, "Slogan", empresa.vchSlogan);
            AgregarCampo(sb, "Descripción", TextoPublicacion.ATextoPlano(empresa.nvchDescripcion));
            AgregarCampo(sb, "Misión", TextoPublicacion.ATextoPlano(empresa.nvchMision));
            AgregarCampo(sb, "Visión", TextoPublicacion.ATextoPlano(empresa.nvchVision));
            AgregarCampo(sb, "Historia", TextoPublicacion.ATextoPlano(empresa.nvchHistoria));
            AgregarCampo(sb, "Teléfono", empresa.vchTelefono);
            AgregarCampo(sb, "Correo", empresa.vchCorreo);

            AgregarContacto(sb, detalle);
            AgregarUbicaciones(sb, detalle);

            var categorias = (detalle.Categorias ?? new List<CategoriaCatalogoConProductos>())
                .OrderBy(c => c.iOrdenAparicion)
                .Select(c => new
                {
                    Categoria = c,
                    Productos = (c.Productos ?? new List<ProductoServicio>())
                        .Where(p => p.bEstatus && !string.IsNullOrWhiteSpace(p.vchNombre))
                        .ToList()
                })
                .Where(c => c.Productos.Any())
                .ToList();

            if (categorias.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Productos y servicios:");
                foreach (var grupo in categorias)
                {
                    if (!string.IsNullOrWhiteSpace(grupo.Categoria.vchNombre))
                    {
                        sb.AppendLine($"{grupo.Categoria.vchNombre.Trim()}:");
                    }

                    foreach (var producto in grupo.Productos)
                    {
                        sb.AppendLine($"- {DescribirProducto(producto)}");
                    }
                }
            }

            return sb.ToString().Trim();
        }

        public static List<ProductoPublicable> ProductosPublicables(PostSaveEmprendimientoFull detalle)
        {
            return (detalle.Categorias ?? new List<CategoriaCatalogoConProductos>())
                .OrderBy(c => c.iOrdenAparicion)
                .SelectMany(c => (c.Productos ?? new List<ProductoServicio>())
                    .Where(p => p.bEstatus && !string.IsNullOrWhiteSpace(p.vchNombre))
                    .OrderBy(p => p.vchNombre)
                    .Select(p => new ProductoPublicable(p, string.IsNullOrWhiteSpace(c.vchNombre) ? null : c.vchNombre.Trim())))
                .ToList();
        }

        public static string ArmarInformacionProducto(PostSaveEmprendimientoFull detalle, ProductoPublicable publicable)
        {
            var empresa = detalle.Empresa ?? new Empresa();
            var producto = publicable.Producto;
            var sb = new StringBuilder();

            sb.AppendLine("Producto:");
            AgregarCampo(sb, "Nombre", producto.vchNombre);
            AgregarCampo(sb, "Categoría", publicable.Categoria);
            AgregarCampo(sb, "Descripción", TextoPublicacion.ATextoPlano(producto.nvchDescripcionCorta));
            if (producto.mPrecio > 0)
            {
                AgregarCampo(sb, "Precio", Precio(producto.mPrecio));
                if (TieneDescuento(producto))
                {
                    AgregarCampo(sb, "Precio con descuento", Precio(producto.mPrecioDescuento));
                }
            }

            sb.AppendLine();
            sb.AppendLine("Emprendimiento:");
            AgregarCampo(sb, "Nombre comercial", empresa.vchNombreComercial);
            AgregarCampo(sb, "Slogan", empresa.vchSlogan);
            AgregarCampo(sb, "Teléfono", empresa.vchTelefono);
            AgregarCampo(sb, "Correo", empresa.vchCorreo);

            AgregarContacto(sb, detalle);
            AgregarUbicaciones(sb, detalle);

            return sb.ToString().Trim();
        }

        private static void AgregarContacto(StringBuilder sb, PostSaveEmprendimientoFull detalle)
        {
            var empresa = detalle.Empresa ?? new Empresa();
            var redes = (detalle.RedesSociales ?? new List<EmpresaRedSocial>())
                .Where(r => r.bActivo && !string.IsNullOrWhiteSpace(r.vchUrl))
                .ToList();

            if (!redes.Any(r => EsWhatsApp(r.vchPlataforma)))
            {
                var whatsapp = EnlaceWhatsApp(empresa.vchTelefono);
                if (whatsapp != null)
                {
                    redes.Insert(0, new EmpresaRedSocial { vchPlataforma = "WhatsApp", vchUrl = whatsapp, bActivo = true });
                }
            }

            if (redes.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Contacto y redes sociales:");
                foreach (var red in redes)
                {
                    var plataforma = string.IsNullOrWhiteSpace(red.vchPlataforma) ? "Enlace" : red.vchPlataforma.Trim();
                    sb.AppendLine($"- {plataforma}: {red.vchUrl!.Trim()}");
                }
            }
        }

        private static void AgregarUbicaciones(StringBuilder sb, PostSaveEmprendimientoFull detalle)
        {
            var ubicaciones = (detalle.Ubicaciones ?? new List<EmpresaUbicacion>())
                .Where(u => u.bActivo && (!string.IsNullOrWhiteSpace(u.vchDireccion) || TieneCoordenadas(u)))
                .ToList();

            if (ubicaciones.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Ubicaciones:");
                foreach (var ubicacion in ubicaciones)
                {
                    var linea = new StringBuilder("- ");
                    if (!string.IsNullOrWhiteSpace(ubicacion.vchAlias))
                    {
                        linea.Append($"{ubicacion.vchAlias.Trim()}: ");
                    }

                    linea.Append(string.IsNullOrWhiteSpace(ubicacion.vchDireccion) ? "Sin dirección escrita" : ubicacion.vchDireccion.Trim());
                    if (TieneCoordenadas(ubicacion))
                    {
                        var lat = ubicacion.flLatitud.ToString(CultureInfo.InvariantCulture);
                        var lng = ubicacion.flLongitud.ToString(CultureInfo.InvariantCulture);
                        linea.Append($" (Google Maps: https://www.google.com/maps?q={lat},{lng})");
                    }

                    sb.AppendLine(linea.ToString());
                }
            }
        }

        public static List<string> ArmarImagenes(PostSaveEmprendimientoFull detalle)
        {
            var imagenes = new List<string?> { detalle.Empresa?.vchLogoUrl };
            imagenes.AddRange((detalle.Categorias ?? new List<CategoriaCatalogoConProductos>())
                .SelectMany(c => c.Productos ?? new List<ProductoServicio>())
                .Where(p => p.bEstatus)
                .Select(p => p.vchImagenUrl));

            return imagenes
                .Where(TextoPublicacion.EsUrlHttp)
                .Select(url => url!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string DescribirProducto(ProductoServicio producto)
        {
            var partes = new List<string> { producto.vchNombre!.Trim() };

            var descripcion = TextoPublicacion.ATextoPlano(producto.nvchDescripcionCorta);
            if (!string.IsNullOrWhiteSpace(descripcion))
            {
                partes.Add(descripcion);
            }

            if (producto.mPrecio > 0)
            {
                var precio = $"Precio: {Precio(producto.mPrecio)}";
                if (TieneDescuento(producto))
                {
                    precio += $" (con descuento: {Precio(producto.mPrecioDescuento)})";
                }

                partes.Add(precio);
            }

            return string.Join(" — ", partes);
        }

        private static void AgregarCampo(StringBuilder sb, string etiqueta, string? valor)
        {
            if (!string.IsNullOrWhiteSpace(valor))
            {
                sb.AppendLine($"{etiqueta}: {valor.Trim()}");
            }
        }

        private static bool EsWhatsApp(string? plataforma)
        {
            return plataforma?.Contains("whatsapp", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        private static string? EnlaceWhatsApp(string? telefono)
        {
            var digitos = Regex.Replace(telefono ?? string.Empty, "\\D", string.Empty);
            if (digitos.Length == 10)
            {
                return $"https://wa.me/52{digitos}";
            }

            if (digitos.Length == 12 && digitos.StartsWith("52"))
            {
                return $"https://wa.me/{digitos}";
            }

            return null;
        }

        private static bool TieneCoordenadas(EmpresaUbicacion ubicacion)
        {
            return ubicacion.flLatitud != 0 && ubicacion.flLongitud != 0;
        }

        public static bool TieneDescuento(ProductoServicio producto)
        {
            return producto.bAplicaDescuento && producto.mPrecioDescuento > 0 && producto.mPrecioDescuento < producto.mPrecio;
        }

        public static string Precio(decimal valor)
        {
            return $"${valor.ToString("#,0.##", CultureInfo.InvariantCulture)} MXN";
        }
    }

    public sealed record ProductoPublicable(ProductoServicio Producto, string? Categoria);
}
