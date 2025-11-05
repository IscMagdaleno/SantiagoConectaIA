namespace SantiagoConectaIA.Share.PostModels.OficinasModule
{
	public class PostGetOficinas { public int iIdOficina { get; set; } = 0; public bool bActivo { get; set; } = false; public bool bIncluirContacto { get; set; } = false; }
	public class PostSearchOficinas { public string vchTexto { get; set; } public int iIdDependencia { get; set; } = 0; public int iPage { get; set; } = 1; public int iPageSize { get; set; } = 20; public bool bIncluirContacto { get; set; } = false; }
	public class PostSaveOficina { public int iIdOficina { get; set; } = 0; public int? iIdDependencia { get; set; } public string vchNombre { get; set; } public string vchDireccion { get; set; } public string vchTelefono { get; set; } public string vchEmail { get; set; } public string vchHorario { get; set; } public double? flLatitud { get; set; } public double? flLongitud { get; set; } public string vchNotas { get; set; } public bool bActivo { get; set; } = true; public int? iIdUsuario { get; set; } }
	public class PostGetDependencias { public int iIdDependencia { get; set; } = 0; public bool bActivo { get; set; } = false; }
	public class PostSaveDependencia { public int iIdDependencia { get; set; } = 0; public string vchNombre { get; set; } public string nvchDescripcion { get; set; } public string vchUrlOficial { get; set; } public bool bActivo { get; set; } = true; }
	public class PostLinkOficinaTramite { public int iIdOficina { get; set; } public int iIdTramite { get; set; } public string vchObservacion { get; set; } public bool bActivo { get; set; } = true; }
	public class PostGetOficinasPorTramite { public int iIdTramite { get; set; } public string vchTexto { get; set; } public bool bIncluirContacto { get; set; } = false; }


}
