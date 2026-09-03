namespace SantiagoConectaIA.Share.PostModels.PublicacionCiudadanoModule
{
    public class PostGetPublicacionesCiudadano
    {
        public int iPage { get; set; } = 1;
        public int iPageSize { get; set; } = 10;
        public string? vchCategoria { get; set; }
        public string? nvchBusqueda { get; set; }
    }
}
