namespace SantiagoConectaIA.Share.Objects.NoticiasModule
{
    public class NoticiaMetadato
    {
        public int iIdMetadato { get; set; }
        public int iIdNoticia { get; set; }
        public TipoDatoMetadato iIdTipoDato { get; set; }
        public string vchAlias { get; set; } = string.Empty;
        public string nvchValor { get; set; } = string.Empty;
        public int iOrden { get; set; }
    }
}
