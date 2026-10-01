using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
    public partial class PostProductoEmprendimiento : ComponentBase
    {
        [Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        [Parameter] public PostSaveEmprendimientoFull? Detalle { get; set; }

        private PostSaveEmprendimientoFull? _detalleCargado;

        public List<ProductoPublicable> Productos { get; set; } = new();
        public ProductoPublicable? ProductoSeleccionado { get; set; }
        public string Informacion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public List<string> ImagenesPost { get; set; } = new();
        public string? ImagenSeleccionada { get; set; }
        public bool Mejorando { get; set; }
        public bool Publicando { get; set; }
        public bool EditandoImagen { get; set; }

        public bool Ocupado => Mejorando || Publicando || EditandoImagen;
        public bool PuedeMejorar => ProductoSeleccionado != null && !string.IsNullOrWhiteSpace(Informacion) && !Ocupado;
        public bool PuedeEditarImagen => !string.IsNullOrWhiteSpace(ImagenSeleccionada) && !Ocupado;
        public bool PuedePublicar => ProductoSeleccionado != null && !string.IsNullOrWhiteSpace(ImagenSeleccionada) && !string.IsNullOrWhiteSpace(Mensaje) && !Ocupado;

        protected override void OnParametersSet()
        {
            if (ReferenceEquals(Detalle, _detalleCargado))
            {
                return;
            }

            _detalleCargado = Detalle;
            Productos = Detalle == null ? new List<ProductoPublicable>() : EmprendimientoTextoBuilder.ProductosPublicables(Detalle);
            ProductoSeleccionado = null;
            Informacion = string.Empty;
            Mensaje = string.Empty;
            ImagenesPost = new List<string>();
            ImagenSeleccionada = null;
        }

        public static bool TieneImagen(ProductoPublicable producto) => TextoPublicacion.EsUrlHttp(producto.Producto.vchImagenUrl);

        public static string PrecioTexto(ProductoPublicable publicable)
        {
            var producto = publicable.Producto;
            if (producto.mPrecio <= 0)
            {
                return string.Empty;
            }

            return EmprendimientoTextoBuilder.TieneDescuento(producto)
                ? $"{EmprendimientoTextoBuilder.Precio(producto.mPrecioDescuento)} (antes {EmprendimientoTextoBuilder.Precio(producto.mPrecio)})"
                : EmprendimientoTextoBuilder.Precio(producto.mPrecio);
        }

        public void Seleccionar(ProductoPublicable producto)
        {
            if (Ocupado || Detalle == null)
            {
                return;
            }

            ProductoSeleccionado = producto;
            Informacion = EmprendimientoTextoBuilder.ArmarInformacionProducto(Detalle, producto);
            Mensaje = string.Empty;
            ImagenesPost = TieneImagen(producto) ? new List<string> { producto.Producto.vchImagenUrl!.Trim() } : new List<string>();
            ImagenSeleccionada = ImagenesPost.FirstOrDefault();
        }

        public void ElegirImagen(string url) => ImagenSeleccionada = url;

        public async Task EditarImagen()
        {
            if (!PuedeEditarImagen)
            {
                return;
            }

            EditandoImagen = true;
            var editada = await EditorImagenIaDialog.AbrirAsync(DialogService, ImagenSeleccionada!);
            EditandoImagen = false;

            if (!string.IsNullOrWhiteSpace(editada))
            {
                ImagenesPost.Add(editada);
                ImagenSeleccionada = editada;
            }
        }

        public async Task MejorarConIa()
        {
            if (!PuedeMejorar || ProductoSeleccionado == null)
            {
                return;
            }

            var empresa = Detalle?.Empresa;
            Mejorando = true;
            var result = await Publicaciones.MejorarProductoConIa(
                empresa?.iIdEmpresa ?? 0,
                empresa?.vchNombreComercial ?? string.Empty,
                ProductoSeleccionado.Producto,
                Informacion);
            Mejorando = false;

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data))
            {
                Mensaje = result.Data;
                Snackbar.Add("Texto listo. Revísalo y publícalo cuando quede bien.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo mejorar el texto." : result.Message, Severity.Error);
            }
        }

        public async Task Publicar()
        {
            if (!PuedePublicar)
            {
                Snackbar.Add("Elige un producto con imagen y genera el texto de la publicación.", Severity.Warning);
                return;
            }

            Publicando = true;
            var result = await Publicaciones.PublicarFacebook(Mensaje.Trim(), ImagenSeleccionada!.Trim());
            Publicando = false;

            if (result.IsSuccess)
            {
                Snackbar.Add("El producto se envió a Facebook.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar el producto." : result.Message, Severity.Error);
            }
        }
    }
}
