using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.OpinionModule
{
    public class Opinion
    {
        public int iIdOpinion { get; set; }
        public string vchTipoEntidad { get; set; } = string.Empty;
        public int iIdEntidad { get; set; }
        public int iIdCiudadano { get; set; }
        public string vchAlias { get; set; } = string.Empty;
        public int? iIdOpinionPadre { get; set; }
        public string nvchTexto { get; set; } = string.Empty;
        public DateTime dtFechaCreacion { get; set; }
        public List<Opinion> Respuestas { get; set; } = new();
    }
}
