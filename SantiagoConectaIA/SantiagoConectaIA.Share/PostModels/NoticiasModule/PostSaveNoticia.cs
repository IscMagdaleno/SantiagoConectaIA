using System;
using System.Collections.Generic;
using SantiagoConectaIA.Share.Objects.NoticiasModule;

namespace SantiagoConectaIA.Share.PostModels.NoticiasModule
{
    public class PostSaveNoticia
    {
        public int iIdNoticia { get; set; }
        public string vchTitulo { get; set; } = string.Empty;
        public string vchTituloEn { get; set; } = string.Empty;
        public string nvchContenido { get; set; } = string.Empty;
        public string nvchContenidoEn { get; set; } = string.Empty;
        public string vchImagenPortada { get; set; } = string.Empty;
        public DateTime dtFechaPublicacion { get; set; }
        public bool bActivo { get; set; }

        public int? iIdCategoria { get; set; }
        public List<NoticiaImagen> Imagenes { get; set; } = new List<NoticiaImagen>();
        public List<NoticiaFila> Filas { get; set; } = new List<NoticiaFila>();
    }
}
