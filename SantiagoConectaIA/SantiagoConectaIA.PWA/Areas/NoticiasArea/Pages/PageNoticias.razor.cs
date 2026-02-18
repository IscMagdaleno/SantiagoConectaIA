using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Servicios;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Pages
{
    public partial class PageNoticias : EngramaPage
    {
        public MainNoticias Data { get; set; }
        
        // UI State
        public bool bShowForm { get; set; } = false;
        public string filtroTexto { get; set; } = "";

        public IEnumerable<Noticia> NoticiasFiltradas => 
            string.IsNullOrWhiteSpace(filtroTexto) 
            ? Data.LstNoticias 
            : Data.LstNoticias.Where(n => (n.vchTitulo?.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase) ?? false) || 
                                          (n.nvchContenido?.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase) ?? false));

        protected override async Task OnInitializedAsync()
        {
            Data = new MainNoticias(httpService, mapperHelper, validaServicioService);
            await Data.PostGetNoticias();
        }

        // Show Create Form
        private void OnClickShowForm()
        {
            Data.NoticiaSelected = new Noticia();
            bShowForm = true;
        }

        // Show List
        private void OnClickShowList()
        {
            bShowForm = false;
        }

        // Edit Item
        private void EditNoticia(Noticia noticia)
        {
            Data.NoticiaSelected = noticia;
            bShowForm = true;
        }

        // Callback after successful save
        private async Task OnSaveSuccess()
        {
            bShowForm = false;
            await Data.PostGetNoticias();
        }
    }
}
