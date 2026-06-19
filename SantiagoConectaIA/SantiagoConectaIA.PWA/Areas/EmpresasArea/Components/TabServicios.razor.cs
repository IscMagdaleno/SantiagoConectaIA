using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using System.Threading.Tasks;
using MudBlazor;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class TabServicios : ComponentBase
    {
        [Inject] public MainEmpresas Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }

        private CategoriaCatalogo categoriaSeleccionada;
        
        // Modal Categoría
        private bool dialogCategoriaVisible = false;
        private DialogOptions dialogOptionsSmall = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        private CategoriaCatalogo categoriaActual = new();

        // Modal Producto
        private bool dialogProductoVisible = false;
        private DialogOptions dialogOptionsMedium = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
        private ProductoServicio productoActual = new();

        private async Task SeleccionarCategoria(CategoriaCatalogo cat)
        {
            categoriaSeleccionada = cat;
            Data.LstProductos.Clear();
            await Data.PostGetProductos(cat.iIdCategoriaCat);
        }

        // ---------- CATEGORÍA ----------
        private void AbrirDialogoNuevaCategoria()
        {
            categoriaActual = new CategoriaCatalogo
            {
                iIdEmpresa = Data.RegistroSeleccionado.iIdEmpresa,
                iOrdenAparicion = (Data.LstCategorias.Count + 1) * 10
            };
            dialogCategoriaVisible = true;
        }

        private void EditarCategoria(CategoriaCatalogo cat)
        {
            categoriaActual = new CategoriaCatalogo
            {
                iIdCategoriaCat = cat.iIdCategoriaCat,
                iIdEmpresa = cat.iIdEmpresa,
                vchNombre = cat.vchNombre,
                iOrdenAparicion = cat.iOrdenAparicion
            };
            dialogCategoriaVisible = true;
        }

        private async Task GuardarCategoria()
        {
            if (string.IsNullOrWhiteSpace(categoriaActual.vchNombre))
            {
                Snackbar.Add("El nombre de la categoría es requerido.", Severity.Warning);
                return;
            }

            var result = await Data.PostSaveCategoria(categoriaActual);
            if (result.bResult)
            {
                await Data.PostGetCategorias();
                Snackbar.Add("Categoría guardada exitosamente.", Severity.Success);
                dialogCategoriaVisible = false;
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }

        // ---------- PRODUCTO / SERVICIO ----------
        private void AbrirDialogoNuevoProducto()
        {
            if (categoriaSeleccionada == null || categoriaSeleccionada.iIdCategoriaCat <= 0)
            {
                Snackbar.Add("Debes seleccionar una categoría primero.", Severity.Warning);
                return;
            }

            productoActual = new ProductoServicio
            {
                iIdCategoriaCat = categoriaSeleccionada.iIdCategoriaCat,
                bEstatus = true,
                mPrecio = 0
            };
            dialogProductoVisible = true;
        }

        private void EditarProducto(ProductoServicio prod)
        {
            productoActual = new ProductoServicio
            {
                iIdProducto = prod.iIdProducto,
                iIdCategoriaCat = prod.iIdCategoriaCat,
                vchNombre = prod.vchNombre,
                nvchDescripcionCorta = prod.nvchDescripcionCorta,
                mPrecio = prod.mPrecio,
                vchImagenUrl = prod.vchImagenUrl,
                bAplicaDescuento = prod.bAplicaDescuento,
                mPrecioDescuento = prod.mPrecioDescuento,
                bEstatus = prod.bEstatus
            };
            dialogProductoVisible = true;
        }

        private async Task GuardarProducto()
        {
            if (string.IsNullOrWhiteSpace(productoActual.vchNombre))
            {
                Snackbar.Add("El nombre del servicio es requerido.", Severity.Warning);
                return;
            }

            var result = await Data.PostSaveProducto(productoActual, categoriaSeleccionada.iIdCategoriaCat);
            if (result.bResult)
            {
                await Data.PostGetProductos(categoriaSeleccionada.iIdCategoriaCat);
                Snackbar.Add("Servicio guardado exitosamente.", Severity.Success);
                dialogProductoVisible = false;
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }
    }
}
