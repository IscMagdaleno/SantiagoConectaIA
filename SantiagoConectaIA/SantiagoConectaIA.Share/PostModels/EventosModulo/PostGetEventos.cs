namespace SantiagoConectaIA.Share.PostClass.EventosModulo
{
    public class PostGetEventos
    {
        public int iIdEvento { get; set; } = -1;
        public int? iIdCategoriaEvento { get; set; } = null;
        public bool? bEstatus { get; set; } = null;
        public bool? bDestacado { get; set; } = null;
    }
}
