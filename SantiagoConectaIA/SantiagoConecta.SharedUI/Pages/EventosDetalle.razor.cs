using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SantiagoConecta.SharedUI.Pages
{
    public partial class EventosDetalle : ComponentBase
    {
        [Inject]
        public NavigationManager NavManager { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        // Modelos
        public class CategoriaEvento
        {
            public int iIdCategoriaEvento { get; set; }
            public string vchNombre { get; set; } = string.Empty;
            public string? vchIconoUrl { get; set; }
        }

        public class Evento
        {
            public int iIdEvento { get; set; }
            public int? iIdCategoriaEvento { get; set; }
            public string vchNombre { get; set; } = string.Empty;
            public string? nvchDescripcion { get; set; }
            public DateTime dtFechaInicio { get; set; }
            public DateTime? dtFechaFin { get; set; }
            public string? vchLugar { get; set; }
            public string? vchDireccion { get; set; }
            public double? flLatitud { get; set; }
            public double? flLongitud { get; set; }
            public string? vchImagenPortada { get; set; }
            public decimal? mCosto { get; set; }
            public string? vchCostoTexto { get; set; }
            public string? vchOrganizador { get; set; }
            public string? vchTelefono { get; set; }
            public string? vchCorreo { get; set; }
            public string? vchUrlOficial { get; set; }
            public bool bDestacado { get; set; }
            public bool bEstatus { get; set; }
            public DateTime dtFechaRegistro { get; set; }
        }

        // Nuevo Modelo para Sucursales/Puntos de Venta
        public class EventosSucursal
        {
            public int iIdSucursalEvento { get; set; }
            public int iIdEvento { get; set; }
            public string vchNombre { get; set; } = string.Empty;
            public string? vchDireccion { get; set; }
            public double? flLatitud { get; set; }
            public double? flLongitud { get; set; }
            public string? vchTelefono { get; set; }
            public string? vchContacto { get; set; }
            public string? vchHorario { get; set; }
            public string? vchNotas { get; set; }
            public bool bActivo { get; set; }
            public DateTime dtFechaRegistro { get; set; }
        }

        protected List<CategoriaEvento> categorias = new();
        protected List<Evento> todosEventos = new();
        protected List<EventosSucursal> todasSucursales = new();
        
        protected Evento? selectedEvento;
        protected List<EventosSucursal> sucursalesEvento = new();

        protected override void OnInitialized()
        {
            CargarMockups();
        }

        protected override void OnParametersSet()
        {
            selectedEvento = todosEventos.FirstOrDefault(e => e.iIdEvento == Id);
            if (selectedEvento != null)
            {
                sucursalesEvento = todasSucursales.Where(s => s.iIdEvento == Id && s.bActivo).ToList();
            }
        }

        protected void VolverAlListado()
        {
            NavManager.NavigateTo("/eventos");
        }

        protected string GetCategoryName(int? id)
        {
            if (!id.HasValue) return "Sin Categoría";
            var cat = categorias.FirstOrDefault(c => c.iIdCategoriaEvento == id.Value);
            return cat?.vchNombre ?? "Desconocido";
        }

        private void CargarMockups()
        {
            categorias = new List<CategoriaEvento>
            {
                new CategoriaEvento { iIdCategoriaEvento = 1, vchNombre = "Concierto" },
                new CategoriaEvento { iIdCategoriaEvento = 2, vchNombre = "Festival" },
                new CategoriaEvento { iIdCategoriaEvento = 3, vchNombre = "Deporte" },
                new CategoriaEvento { iIdCategoriaEvento = 4, vchNombre = "Cultural" },
                new CategoriaEvento { iIdCategoriaEvento = 5, vchNombre = "Gastronómico" },
                new CategoriaEvento { iIdCategoriaEvento = 6, vchNombre = "Feria" },
                new CategoriaEvento { iIdCategoriaEvento = 7, vchNombre = "Teatro" },
                new CategoriaEvento { iIdCategoriaEvento = 8, vchNombre = "Otro" }
            };

            todosEventos = new List<Evento>
            {
                new Evento
                {
                    iIdEvento = 1,
                    iIdCategoriaEvento = 6, // Feria
                    vchNombre = "Feria Santiago Papasquiaro 2026",
                    nvchDescripcion = "La fiesta más grande de Santiago Papasquiaro. Ven a disfrutar de la tradicional feria con juegos mecánicos, exposiciones, conciertos en el palenque, antojitos mexicanos y mucha diversión para toda la familia.\n\nContaremos con la presencia de artistas reconocidos a nivel nacional e internacional.",
                    dtFechaInicio = new DateTime(2026, 7, 17, 18, 0, 0),
                    dtFechaFin = new DateTime(2026, 7, 26, 23, 59, 0),
                    vchLugar = "Recinto Ferial",
                    vchDireccion = "Carretera Santiago - Tepehuanes Km 1.5",
                    vchImagenPortada = "https://scontent-qro3-1.xx.fbcdn.net/v/t39.99422-6/720334046_3503580916462973_7986243803335080159_n.png?stp=dst-jpg_tt6&cstp=mx1280x1600&ctp=s1280x1600&_nc_cat=106&ccb=1-7&_nc_sid=127cfc&_nc_eui2=AeE0R5923CfupQHoZNBUGfX1_mnY_XHWODf-adj9cdY4N4exNPOg4xHPX5Cg3Gx24fOsZCIYdnWQh7Ew2YYJxkVV&_nc_ohc=dR-Ex_X4Dj8Q7kNvwF7Tgge&_nc_oc=AdpsxkGSReNnwuRw41Yx8uIZQcYcWvmvfMcEkW2HvTkMzST2d2vFLDqOdrxJlLC81JNjNY6q-PTPpdLDyBkVKb_k&_nc_zt=14&_nc_ht=scontent-qro3-1.xx&_nc_gid=hZfS3In2oCinuuyXVl0Nkw&_nc_ss=7b2a8&oh=00_AQAxVtCSzdZDduadnODs9AUgWJzjE75n3dDfa9I7aEoWfQ&oe=6A4C5473",
                    vchCostoTexto = "Entrada General $50",
                    vchOrganizador = "Comité de la Feria",
                    bDestacado = true,
                    bEstatus = true
                },
                new Evento
                {
                    iIdEvento = 2,
                    iIdCategoriaEvento = 1, // Concierto
                    vchNombre = "Gran Concierto Sinfónico de Verano",
                    nvchDescripcion = "Disfruta de una velada mágica con la Orquesta Sinfónica Juvenil del Estado de Durango. Interpretarán obras clásicas y adaptaciones de música regional mexicana.",
                    dtFechaInicio = new DateTime(2026, 8, 5, 20, 0, 0),
                    vchLugar = "Auditorio Municipal",
                    vchDireccion = "Centro, Santiago Papasquiaro",
                    vchImagenPortada = "https://images.unsplash.com/photo-1514320291840-2e0a9bf2a9ae?q=80&w=1470&auto=format&fit=crop",
                    mCosto = 0,
                    vchOrganizador = "Instituto Municipal del Arte y la Cultura",
                    bDestacado = false,
                    bEstatus = true
                },
                new Evento
                {
                    iIdEvento = 3,
                    iIdCategoriaEvento = 5, // Gastronómico
                    vchNombre = "Muestra Gastronómica 'Sabor a Santiago'",
                    nvchDescripcion = "Los mejores restaurantes y cocineras tradicionales se reúnen para ofrecer los platillos típicos de la región. Pinole, asado, gorditas y más.",
                    dtFechaInicio = new DateTime(2026, 8, 15, 10, 0, 0),
                    dtFechaFin = new DateTime(2026, 8, 15, 18, 0, 0),
                    vchLugar = "Plaza de Armas",
                    vchImagenPortada = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?q=80&w=1374&auto=format&fit=crop",
                    vchCostoTexto = "Entrada Libre",
                    bDestacado = true,
                    bEstatus = true
                },
                new Evento
                {
                    iIdEvento = 4,
                    iIdCategoriaEvento = 3, // Deporte
                    vchNombre = "Carrera Atlética 10K Santiago",
                    nvchDescripcion = "Participa en nuestra carrera anual. Recorrido por las principales calles de la ciudad con premios para los primeros lugares de cada categoría.",
                    dtFechaInicio = new DateTime(2026, 9, 2, 7, 0, 0),
                    vchLugar = "Presidencia Municipal",
                    vchImagenPortada = "https://images.unsplash.com/photo-1552674605-15c9ef04392c?q=80&w=1470&auto=format&fit=crop",
                    mCosto = 250,
                    vchOrganizador = "Dirección de Deportes",
                    vchTelefono = "6741112233",
                    vchUrlOficial = "https://santiagopapasquiaro.gob.mx",
                    bDestacado = false,
                    bEstatus = true
                }
            };

            // Nuevos Mockups para Puntos de Venta (Vinculados a la Feria iIdEvento = 1)
            todasSucursales = new List<EventosSucursal>
            {
                new EventosSucursal
                {
                    iIdSucursalEvento = 1,
                    iIdEvento = 1, // Feria
                    vchNombre = "Taquilla Oficial - Presidencia",
                    vchDireccion = "Centro Histórico, Santiago Papasquiaro",
                    vchHorario = "Lunes a Viernes de 9:00 AM a 3:00 PM",
                    vchTelefono = "6741112233",
                    vchNotas = "Solo pago en efectivo.",
                    bActivo = true
                },
                new EventosSucursal
                {
                    iIdSucursalEvento = 2,
                    iIdEvento = 1, // Feria
                    vchNombre = "Módulo Plaza Cristal",
                    vchDireccion = "Interior Plaza Cristal Local 4",
                    vchHorario = "Lunes a Domingo de 10:00 AM a 8:00 PM",
                    vchTelefono = "6745558899",
                    vchNotas = "Aceptan tarjeta de crédito y débito.",
                    bActivo = true
                },
                new EventosSucursal
                {
                    iIdSucursalEvento = 3,
                    iIdEvento = 4, // Carrera
                    vchNombre = "Dirección de Deportes",
                    vchDireccion = "Unidad Deportiva Municipal",
                    vchHorario = "Lunes a Viernes de 8:00 AM a 4:00 PM",
                    vchNotas = "Presentar identificación oficial para inscripción.",
                    bActivo = true
                }
            };
        }
    }
}
