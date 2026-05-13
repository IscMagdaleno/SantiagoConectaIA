namespace SantiagoConectaIA.Share.Objects.NoticiasModule
{
    public class NoticiaMetadato
    {
        public int iIdMetadato { get; set; }
        public int iIdNoticia { get; set; }
        public TipoDatoMetadato iIdTipoDato { get; set; }
        public string vchTitulo { get; set; } = string.Empty;
        public string nvchValor { get; set; } = string.Empty;
        public int iOrden { get; set; }
        public int? iAncho { get; set; } = 12;
        public string vchAlineacion { get; set; } = "none";
        public string vchAlto { get; set; } = "auto";
        public int? iIdFila { get; set; }
        public string nvchConfiguracion { get; set; } = string.Empty;
    }
}
