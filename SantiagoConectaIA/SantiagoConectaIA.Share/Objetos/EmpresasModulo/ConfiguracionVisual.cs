namespace SantiagoConectaIA.Share.Objetos.EmpresasModulo
{
    public class ConfiguracionVisual
    {
        public int iIdConfiguracion { get; set; } = -1;
        public int iIdEmpresa { get; set; } = -1;
        public int? iIdPlantillaBase { get; set; } = null;
        public string vchColorFondoBody { get; set; } = string.Empty;
        public string vchColorTextoPrincipal { get; set; } = string.Empty;
        public string vchColorTitulos { get; set; } = string.Empty;
        public string vchColorBotones { get; set; } = string.Empty;
        public string vchColorMargenFotos { get; set; } = string.Empty;
        public string vchTipografiaTitulos { get; set; } = string.Empty;
        public string vchTipografiaCuerpo { get; set; } = string.Empty;
        public string vchEstiloBordes { get; set; } = string.Empty;
    }
}
