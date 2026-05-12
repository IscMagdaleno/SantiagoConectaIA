using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.NoticiasModule
{
    public class Noticia
    {
        public int iIdNoticia { get; set; }
        public string vchTitulo { get; set; } = string.Empty;
        public string vchTituloEn { get; set; } = string.Empty;
        public string nvchContenido { get; set; } = string.Empty;
        public string nvchContenidoEn { get; set; } = string.Empty;
        public string vchImagenPortada { get; set; } = string.Empty;
        public DateTime dtFechaPublicacion { get; set; } = DateTime.Now;
        public bool bActivo { get; set; }

        public int? iIdCategoria { get; set; }
        public CategoriaNoticia? Categoria { get; set; }

        public List<NoticiaImagen> Imagenes { get; set; } = new List<NoticiaImagen>();
        public List<NoticiaMetadato> Metadatos { get; set; } = new List<NoticiaMetadato>();
    }
}
