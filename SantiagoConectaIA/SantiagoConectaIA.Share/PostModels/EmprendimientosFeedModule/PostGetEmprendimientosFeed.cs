namespace SantiagoConectaIA.Share.PostModels.EmprendimientosFeedModule
{
    public class PostGetEmprendimientosFeed
    {
        public int iPage { get; set; } = 1;
        public int iPageSize { get; set; } = 10;
        public string? vchSessionSeed { get; set; }
    }
}
