namespace SantiagoConectaIA.Share.Objects.PublicacionesModule
{
	/// <summary>Una publicación (exitosa o fallida) de una noticia o un emprendimiento en redes sociales.</summary>
	public class PublicacionRegistro
	{
		public int iIdPublicacionRegistro { get; set; }

		/// <summary>NOTICIA o EMPRENDIMIENTO.</summary>
		public string vchTipo { get; set; } = string.Empty;

		/// <summary>iIdNoticia o iIdEmpresa, según el tipo.</summary>
		public int iIdReferencia { get; set; }

		public string vchTitulo { get; set; } = string.Empty;
		public string vchCanal { get; set; } = "FACEBOOK";

		/// <summary>AUTOMATICA, MANUAL o MIGRACION.</summary>
		public string vchOrigen { get; set; } = string.Empty;

		public bool bExitoso { get; set; }

		/// <summary>Motivo cuando la publicación falló.</summary>
		public string? vchResultado { get; set; }

		/// <summary>Texto que se publicó.</summary>
		public string? nvchMensaje { get; set; }

		public string? vchImagenUrl { get; set; }

		/// <summary>Fecha y hora en UTC.</summary>
		public DateTime dtPublicacion { get; set; }

		/// <summary>Día en hora de México.</summary>
		public DateTime dtFechaLocal { get; set; }
	}
}
