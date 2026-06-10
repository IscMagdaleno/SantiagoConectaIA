namespace SantiagoConectaIA.Share.Objects.NoticiasModule
{
    public class CategoriaNoticia
    {
        public int iIdCategoria { get; set; }
        public string vchNombre { get; set; } = string.Empty;
        public string vchColorHex { get; set; } = string.Empty;
        public bool bActivo { get; set; }
    }
}
