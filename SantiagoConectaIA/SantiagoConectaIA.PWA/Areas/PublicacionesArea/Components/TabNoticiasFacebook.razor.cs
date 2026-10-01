using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
    public partial class TabNoticiasFacebook : ComponentBase
    {
        [Inject] public MainNoticias Noticias { get; set; } = default!;
        [Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        public string FiltroTexto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Cargando { get; set; } = true;
        public bool Mejorando { get; set; }
        public bool Publicando { get; set; }

        public PublicacionAutomatica? Automatica { get; set; }
        public bool GuardandoAutomatica { get; set; }

        public Noticia? NoticiaSeleccionada { get; set; }

        public bool TieneImagen => !string.IsNullOrWhiteSpace(NoticiaSeleccionada?.vchImagenPortada);
        public bool PuedePublicar => NoticiaSeleccionada != null && TieneImagen && !string.IsNullOrWhiteSpace(Mensaje) && !Publicando && !Mejorando;
        public bool PuedeMejorar => NoticiaSeleccionada != null && !Publicando && !Mejorando;

        public IEnumerable<Noticia> NoticiasFiltradas =>
            (string.IsNullOrWhiteSpace(FiltroTexto)
                ? Noticias.LstNoticias
                : Noticias.LstNoticias.Where(n =>
                    (n.vchTitulo?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (n.nvchContenido?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false)))
            .OrderByDescending(n => n.dtFechaPublicacion);

        protected override async Task OnInitializedAsync()
        {
            var noticiasTask = Noticias.PostGetNoticias();
            var automaticaTask = Publicaciones.GetPublicacionAutomatica();
            await Task.WhenAll(noticiasTask, automaticaTask);

            var automatica = automaticaTask.Result;
            if (automatica.IsSuccess)
            {
                Automatica = automatica.Data;
            }

            Cargando = false;
        }

        public async Task CambiarAutomatica(bool activo)
        {
            GuardandoAutomatica = true;
            var result = await Publicaciones.SavePublicacionAutomatica(activo);
            GuardandoAutomatica = false;

            if (result.IsSuccess && result.Data != null)
            {
                Automatica = result.Data;
                Snackbar.Add(result.Message, Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo cambiar la publicación automática." : result.Message, Severity.Error);
            }
        }

        public void Seleccionar(Noticia noticia)
        {
            NoticiaSeleccionada = noticia;
            Mensaje = ArmarMensaje(noticia);
        }

        public async Task MejorarConIa()
        {
            if (NoticiaSeleccionada == null)
            {
                Snackbar.Add("Selecciona una noticia para mejorar el texto.", Severity.Warning);
                return;
            }

            Mejorando = true;
            var result = await Publicaciones.MejorarConIa(
                NoticiaSeleccionada.iIdNoticia,
                NoticiaSeleccionada.vchTitulo ?? string.Empty,
                NoticiaSeleccionada.nvchContenido ?? string.Empty);
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
            if (NoticiaSeleccionada == null || !TieneImagen || string.IsNullOrWhiteSpace(Mensaje))
            {
                Snackbar.Add("Selecciona una noticia con imagen y escribe el texto de la publicación.", Severity.Warning);
                return;
            }

            Publicando = true;
            var result = await Publicaciones.PublicarFacebook(Mensaje.Trim(), NoticiaSeleccionada.vchImagenPortada.Trim());
            Publicando = false;

            if (result.IsSuccess)
            {
                Snackbar.Add("La noticia se envió a Facebook.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar la noticia." : result.Message, Severity.Error);
            }
        }

        private static string ArmarMensaje(Noticia noticia)
        {
            var titulo = (noticia.vchTitulo ?? string.Empty).Trim();
            var contenido = TextoPublicacion.ATextoPlano(noticia.nvchContenido);
            if (string.IsNullOrWhiteSpace(titulo))
            {
                return contenido;
            }

            if (string.IsNullOrWhiteSpace(contenido))
            {
                return titulo;
            }

            return $"{titulo}\n\n{contenido}";
        }
    }
}
