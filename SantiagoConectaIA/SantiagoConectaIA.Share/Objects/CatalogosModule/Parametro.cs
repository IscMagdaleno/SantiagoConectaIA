using System;

namespace SantiagoConectaIA.Share.Objects.CatalogosModule
{
    public class Parametro
    {
        public int IIdParametro { get; set; }
        public int? IIdParametroPadre { get; set; }
        public string NvchAlias { get; set; }
        public string NvchNombre { get; set; }
        public string NvchNombreEn { get; set; }
        public string NvchDescripcion { get; set; }
        public string NvchDescripcionEn { get; set; }
        public int ISecuencia { get; set; }
        public bool BTieneValores { get; set; }
        public string NvchValor1 { get; set; }
        public string NvchValor2 { get; set; }
        public bool BHabilitado { get; set; }
    }
}
