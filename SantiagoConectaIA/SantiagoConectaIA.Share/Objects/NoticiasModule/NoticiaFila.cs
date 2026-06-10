using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.NoticiasModule
{
    public class NoticiaFila
    {
        public int iIdFila { get; set; }
        public int iIdNoticia { get; set; }
        public int iOrden { get; set; }

        public List<NoticiaMetadato> Metadatos { get; set; } = new List<NoticiaMetadato>();

        [System.Text.Json.Serialization.JsonIgnore]
        public string UI_Id { get; set; } = Guid.NewGuid().ToString();
    }
}
