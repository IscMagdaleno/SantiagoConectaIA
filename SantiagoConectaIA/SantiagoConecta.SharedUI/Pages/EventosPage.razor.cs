using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SantiagoConecta.SharedUI.Data;
using SantiagoConectaIA.Share.Objetos.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
namespace SantiagoConecta.SharedUI.Pages
{
    public partial class EventosPage : ComponentBase
    {
        [Inject]
        public NavigationManager NavManager { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        [Inject]
        public Data_Eventos dataEventos { get; set; } = default!;

        // Datos Mockeados
        private List<CategoriaEvento> categorias = new();
        private List<Evento> todosEventos = new();
        protected List<Evento> filteredEventos = new();

        // Estado UI
        protected int? selectedCategoryId;
        private string _searchQuery = string.Empty;

        protected string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                ApplyFilters();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosReales();
            ApplyFilters();
        }

        private async Task CargarDatosReales()
        {
            try
            {
                var catResp = await dataEventos.PostGetCategoriaEventos();
                if (catResp != null && catResp.IsSuccess && catResp.Data != null)
                {
                    categorias = catResp.Data;
                }

                var evResp = await dataEventos.PostGetEventos(new PostGetEventos { bEstatus = true });
                if (evResp != null && evResp.IsSuccess && evResp.Data != null)
                {
                    todosEventos = evResp.Data.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos reales: {ex.Message}");
            }
        }

        protected void FilterByCategory(int? categoryId)
        {
            selectedCategoryId = categoryId;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var query = todosEventos.AsQueryable();

            if (selectedCategoryId.HasValue)
            {
                query = query.Where(e => e.iIdCategoriaEvento == selectedCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                var lowerQuery = _searchQuery.ToLower();
                query = query.Where(e => e.vchNombre.ToLower().Contains(lowerQuery) || 
                                         (e.nvchDescripcion != null && e.nvchDescripcion.ToLower().Contains(lowerQuery)));
            }

            filteredEventos = query.OrderByDescending(e => e.bDestacado).ThenBy(e => e.dtFechaInicio).ToList();
            StateHasChanged();
        }

        protected void VerDetalle(Evento evento)
        {
            NavManager.NavigateTo($"/eventos/{evento.iIdEvento}");
        }

        protected string GetCategoryName(int? id)
        {
            if (!id.HasValue) return "Sin Categoría";
            var cat = categorias.FirstOrDefault(c => c.iIdCategoriaEvento == id.Value);
            return cat?.vchNombre ?? "Desconocido";
        }
    }
}
