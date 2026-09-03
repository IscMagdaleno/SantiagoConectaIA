using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule
{
    public class PublicacionCiudadano
    {
        public int iIdPublicacion { get; set; }
        public int iIdCiudadano { get; set; }
        public string vchAliasCiudadano { get; set; } = string.Empty;
        public string? vchAvatarCiudadano { get; set; }
        public bool bCiudadanoVerificado { get; set; }
        public string? nvchTitulo { get; set; }
        public string nvchContenidoTexto { get; set; } = string.Empty;
        public string vchCategoriaPublicacion { get; set; } = "Comunidad";
        public DateTime dtFechaCreacion { get; set; } = DateTime.UtcNow;
        public bool bActiva { get; set; } = true;
        public int iTotalComentarios { get; set; }
        public string? vchPrimeraImagenUrl { get; set; }
        public List<ImagenPublicacion> Imagenes { get; set; } = new();
        public int iTotalRegistros { get; set; }
    }
}
