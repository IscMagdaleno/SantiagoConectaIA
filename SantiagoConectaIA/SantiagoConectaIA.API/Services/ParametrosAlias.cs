namespace SantiagoConectaIA.API.Services
{
	public static class ParametrosAlias
	{
		/// <summary>Valor1 = modelo, Valor2 = API key.</summary>
		public const string Gemini = "key.gemini";

		/// <summary>Valor1 = modelo de Gemini para editar imágenes. La API key se toma de key.gemini.</summary>
		public const string GeminiImagen = "gemini.imagen";

		/// <summary>Valor1 = URL del webhook de Make que publica en Facebook.</summary>
		public const string MakeFacebookWebhook = "make.facebook.webhook";

		/// <summary>Valor1 = URL del sitio, Valor2 = URL base de noticias.</summary>
		public const string PublicacionesSitio = "publicaciones.sitio";

		/// <summary>Valor1 = hora diaria (0-23) de la publicación automática de noticias.</summary>
		public const string PublicacionesAutoNoticias = "publicaciones.auto.noticias";

		/// <summary>Valor1 = hora diaria (0-23), Valor2 = ID del emprendimiento inicial de la rotación.</summary>
		public const string PublicacionesAutoEmprendimientos = "publicaciones.auto.emprendimientos";

		/// <summary>Valor1 = URL base de la API de plantillas de memes (createa.meme), Valor2 = API key.</summary>
		public const string MemesPlantillas = "memes.plantillas";

		/// <summary>Valor1 = máximo de memes con IA por día, entre todos los administradores.</summary>
		public const string MemesLimiteDiario = "memes.limite.diario";

		/// <summary>Valor1 = cadena de conexión de Azure Blob Storage.</summary>
		public const string AzureBlobStorage = "azure.blob.storage";

		/// <summary>Valor1 = secreto para firmar los JWT.</summary>
		public const string JwtSecret = "jwt.secret";

		/// <summary>Valor1 = Issuer, Valor2 = Audience.</summary>
		public const string JwtConfig = "jwt.config";
	}
}
