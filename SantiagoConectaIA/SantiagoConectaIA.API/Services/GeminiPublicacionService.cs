using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SantiagoConectaIA.API.Services
{
	public class GeminiPublicacionService : IGeminiPublicacionService
	{
		private const string HashtagsNoticias = "#SantiagoConecta #SantiagoPapasquiaro #NoticiasSantiago";
		private const string HashtagsEmprendimientos = "#SantiagoConecta #SantiagoPapasquiaro #EmprendimientosLocales #HechoEnSantiago";
		private const string ModeloDefault = "gemini-2.5-flash";
		private const string SitioDefault = "https://www.santiagopapasquiaro.com.mx";
		private const int MaxCaracteresEntrada = 12000;

		private const string InstruccionEditor =
			"Eres el editor de redes de Santiago Conecta, el portal del municipio de Santiago Papasquiaro, Durango. " +
			"Redactas publicaciones de Facebook en español de México, claras y cercanas. " +
			"No inventas datos, nombres, precios, direcciones ni enlaces que no estén en la información recibida.";

		private const string InstruccionEditorHtml =
			"Eres el editor digital y diseñador editorial de Santiago Conecta, el portal comunitario y municipal de Santiago Papasquiaro, Durango. " +
			"Transformas noticias en artículos HTML enriquecidos con estilos en línea (inline CSS) elegantes, modernos y con la identidad visual de Santiago Conecta. " +
			"No inventas datos, nombres, cifras, cargos ni hechos que no aparezcan en el texto original.";

		private static readonly TimeSpan TiempoMaximo = TimeSpan.FromSeconds(60);

		private readonly IGeminiClient _gemini;
		private readonly IParametrosService _parametros;
		private readonly ILogger<GeminiPublicacionService> _logger;

		public GeminiPublicacionService(
			IGeminiClient gemini,
			IParametrosService parametros,
			ILogger<GeminiPublicacionService> logger)
		{
			_gemini = gemini;
			_parametros = parametros;
			_logger = logger;
		}

		public async Task<Response<string>> MejorarAsync(PostMejorarPublicacion post, CancellationToken cancellationToken = default)
		{
			if (post == null || post.iIdNoticia <= 0 || string.IsNullOrWhiteSpace(post.vchTitulo))
			{
				return Response<string>.BadResult("Selecciona una noticia con título para mejorar el texto.", string.Empty);
			}

			var (_, noticiasUrl) = await ObtenerSitioAsync(cancellationToken);
			var enlace = $"{noticiasUrl}/{post.iIdNoticia}";
			var contenido = Recortar(TextoPublicacion.ATextoPlano(post.nvchContenido));
			var prompt = PromptNoticia(post.vchTitulo.Trim(), contenido, enlace);

			var generado = await GenerarAsync(prompt, cancellationToken);
			if (!generado.IsSuccess)
			{
				return generado;
			}

			var texto = GeminiTexto.QuitarBloquesDeCodigo(generado.Data);
			texto = AsegurarLinea(texto, enlace, $"👉 Conoce más y lee la nota completa en Santiago Conecta:\n{enlace}");
			texto = AsegurarLinea(texto, "#SantiagoConecta", HashtagsNoticias);
			return Ok(texto);
		}

		public async Task<Response<string>> MejorarNoticiaHtmlAsync(PostMejorarNoticiaHtml post, CancellationToken cancellationToken = default)
		{
			if (post == null || (string.IsNullOrWhiteSpace(post.vchTitulo) && string.IsNullOrWhiteSpace(post.nvchContenido)))
			{
				return Response<string>.BadResult("Proporciona el título o contenido de la noticia para generar el HTML.", string.Empty);
			}

			var titulo = post.vchTitulo?.Trim() ?? string.Empty;
			var contenido = Recortar(TextoPublicacion.ATextoPlano(post.nvchContenido));
			var prompt = PromptNoticiaHtml(titulo, contenido);

			var generado = await GenerarAsync(prompt, cancellationToken, InstruccionEditorHtml);
			if (!generado.IsSuccess)
			{
				return generado;
			}

			var html = GeminiTexto.QuitarBloquesDeCodigo(generado.Data);
			var inicioArticle = html.IndexOf("<article", StringComparison.OrdinalIgnoreCase);
			var finArticle = html.LastIndexOf("</article>", StringComparison.OrdinalIgnoreCase);
			if (inicioArticle >= 0 && finArticle > inicioArticle)
			{
				html = html.Substring(inicioArticle, (finArticle + "</article>".Length) - inicioArticle);
			}

			return Ok(html.Trim());
		}

		public async Task<Response<string>> MejorarEmprendimientoAsync(PostMejorarEmprendimiento post, CancellationToken cancellationToken = default)
		{
			if (post == null || post.iIdEmpresa <= 0 || string.IsNullOrWhiteSpace(post.nvchInformacion))
			{
				return Response<string>.BadResult("Selecciona un emprendimiento con información para mejorar el texto.", string.Empty);
			}

			var (sitio, _) = await ObtenerSitioAsync(cancellationToken);
			var enlace = $"{sitio}/emprendimientos/{post.iIdEmpresa}";
			var nombre = string.IsNullOrWhiteSpace(post.vchNombreComercial) ? "este emprendimiento" : post.vchNombreComercial.Trim();
			var prompt = PromptEmprendimiento(nombre, Recortar(post.nvchInformacion.Trim()), enlace, sitio);

			var generado = await GenerarAsync(prompt, cancellationToken);
			if (!generado.IsSuccess)
			{
				return generado;
			}

			var texto = GeminiTexto.QuitarBloquesDeCodigo(generado.Data);
			texto = AsegurarLinea(texto, enlace, $"👉 Conoce a {nombre} en Santiago Conecta:\n{enlace}");
			texto = AsegurarLinea(texto, "#SantiagoConecta", HashtagsEmprendimientos);
			return Ok(texto);
		}

		public async Task<Response<string>> MejorarProductoAsync(PostMejorarProducto post, CancellationToken cancellationToken = default)
		{
			if (post == null || post.iIdEmpresa <= 0 || string.IsNullOrWhiteSpace(post.nvchInformacion))
			{
				return Response<string>.BadResult("Selecciona un producto con información para mejorar el texto.", string.Empty);
			}

			var (sitio, _) = await ObtenerSitioAsync(cancellationToken);
			var enlace = $"{sitio}/emprendimientos/{post.iIdEmpresa}";
			var negocio = string.IsNullOrWhiteSpace(post.vchNombreComercial) ? "este emprendimiento" : post.vchNombreComercial.Trim();
			var producto = string.IsNullOrWhiteSpace(post.vchNombreProducto) ? "este producto" : post.vchNombreProducto.Trim();
			var prompt = PromptProducto(producto, negocio, Recortar(post.nvchInformacion.Trim()), enlace, sitio);

			var generado = await GenerarAsync(prompt, cancellationToken);
			if (!generado.IsSuccess)
			{
				return generado;
			}

			var texto = GeminiTexto.QuitarBloquesDeCodigo(generado.Data);
			texto = AsegurarLinea(texto, enlace, $"👉 Conoce más de {negocio} en Santiago Conecta:\n{enlace}");
			texto = AsegurarLinea(texto, "#SantiagoConecta", HashtagsEmprendimientos);
			return Ok(texto);
		}

		public async Task<Response<string>> MejorarEventoAsync(PostMejorarEvento post, CancellationToken cancellationToken = default)
		{
			if (post == null || post.iIdEvento <= 0 || string.IsNullOrWhiteSpace(post.vchNombre))
			{
				return Response<string>.BadResult("Selecciona un evento para mejorar el texto.", string.Empty);
			}

			var (sitio, _) = await ObtenerSitioAsync(cancellationToken);
			var enlace = $"{sitio}/eventos/{post.iIdEvento}";
			var nombre = post.vchNombre.Trim();
			var prompt = PromptEvento(nombre, post, enlace);

			var generado = await GenerarAsync(prompt, cancellationToken);
			if (!generado.IsSuccess)
			{
				return generado;
			}

			var texto = GeminiTexto.QuitarBloquesDeCodigo(generado.Data);
			texto = AsegurarLinea(texto, enlace, $"👉 {enlace}");
			return Ok(texto);
		}

		private static string PromptEvento(string nombre, PostMejorarEvento post, string enlace)
		{
			return
				$"""
                Redacta UNA publicación de Facebook/Instagram para promocionar este evento en Santiago Papasquiaro. Devuelve solo el texto listo para copiar, sin comillas, sin bloques de código y sin asteriscos de Markdown (* o **).

                Sigue EXACTAMENTE esta estructura y estilo que la audiencia ama:
                1. Título inicial impactante con emojis temáticos según el tipo de evento (ej. 🎸 ¡Noche de rock en español en Santiago Papasquiaro! 🤘✨ o 🎨 ¡Llamado a todos los artistas de nuestra región! ✨).
                2. Un párrafo atractivo y cálido que invite a la comunidad a asistir o participar, destacando quién presenta/organiza, la esencia del evento y por qué no se lo pueden perder.
                3. Bloque de datos clave con emojis:
                🗓️ Fecha: (Día y mes del evento o fecha límite)
                ⏰ Hora: (Horario si aplica)
                📍 Lugar o Registro: (Ubicación, recinto o dirección)
                🎟️ Boletos o Acceso: (Precios de preventa/taquilla o Entrada libre si aplica)
                4. Llamado a la acción entusiasta (ej. ¡Asegura tus entradas con tiempo y no te quedes fuera de esta gran velada! 🔥).
                5. Línea de enlace oficial:
                🔗 Consulta todos los detalles y bases en:
                👉 {enlace}
                6. Pregunta de IA:
                💬 ¿Dudas? ¡Pregúntale a nuestra IA para que te dé todos los detalles del evento al instante! 🤖✨
                .
                .
                7. Hashtags finales relevantes: #SantiagoPapasquiaro #{Regex.Replace(nombre, @"\s+", "")} #SantiagoConecta #EventosSantiago

                Datos del evento:
                - Nombre: {nombre}
                - Descripción: {post.nvchDescripcion}
                - Fecha/Hora: {post.dtFechaInicio}
                - Lugar: {post.vchLugar}
                - Dirección: {post.vchDireccion}
                - Costo/Boletos: {post.vchCostoTexto}
                - Organizador: {post.vchOrganizador}
                """;
		}

		private async Task<Response<string>> GenerarAsync(string prompt, CancellationToken cancellationToken, string? instruccion = null)
		{
			var configuracion = await _gemini.ObtenerConfiguracionAsync(ParametrosAlias.Gemini, ModeloDefault, cancellationToken);
			if (!configuracion.Configurada)
			{
				return Response<string>.BadResult($"No está configurada la clave de Gemini (parámetro {ParametrosAlias.Gemini}).", string.Empty);
			}

			var solicitud = GeminiSolicitud.DeTexto(prompt, instruccion ?? InstruccionEditor, new GeminiGenerationConfig { Temperature = 0.7 });

			try
			{
				var respuesta = await _gemini.GenerarContenidoAsync(configuracion, solicitud, TiempoMaximo, cancellationToken);
				if (!respuesta.Exitosa)
				{
					var motivo = respuesta.CodigoEstado is 401 or 403
						? $"Gemini rechazó la clave ({respuesta.CodigoEstado}). Revisa el parámetro {ParametrosAlias.Gemini} (Valor2)."
						: $"Gemini no pudo redactar la publicación ({respuesta.CodigoEstado}).";
					return Response<string>.BadResult(motivo, string.Empty);
				}

				var generado = respuesta.PrimerTexto;
				if (string.IsNullOrWhiteSpace(generado))
				{
					return Response<string>.BadResult("Gemini no devolvió texto para la publicación.", string.Empty);
				}

				return Ok(generado.Trim());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al consultar Gemini para la publicación.");
				return Response<string>.BadResult("No se pudo consultar Gemini.", string.Empty);
			}
		}

		private async Task<(string Sitio, string Noticias)> ObtenerSitioAsync(CancellationToken cancellationToken)
		{
			var sitio = (await _parametros.GetValor1Async(ParametrosAlias.PublicacionesSitio, cancellationToken) ?? SitioDefault).TrimEnd('/');
			var noticias = await _parametros.GetValor2Async(ParametrosAlias.PublicacionesSitio, cancellationToken);
			return (sitio, noticias?.TrimEnd('/') ?? $"{sitio}/noticias");
		}

		private static string PromptNoticiaHtml(string titulo, string contenido)
		{
			var cuerpo = string.IsNullOrWhiteSpace(contenido)
				? "(La noticia solo cuenta con el título. Desarrolla un artículo breve y fiel únicamente al título, sin inventar cifras ni nombres.)"
				: contenido;

			return
				$$"""
                Transforma y mejora la siguiente noticia en un artículo HTML enriquecido y visualmente atractivo con estilos inline (CSS en línea), diseñado con la identidad y paleta de colores de Santiago Conecta (#4a154b, #880e4f, #d53f8c, #d81b60, #dd6b20, #7b1fa2).

                REGLAS ESTRICTAS:
                1. Devuelve ÚNICAMENTE el código HTML que inicia con <article ...> y termina con </article>. No incluyas bloques ```html, ni explicaciones, ni texto fuera de <article>.
                2. Conserva toda la información, nombres, cargos, cifras, fechas y lugares reales del texto original. NO inventes datos que no estén en la nota.
                3. Mejora la redacción en español de México para que sea fluida, periodística, cálida y profesional, resaltando con <strong> y <em> las palabras, nombres, cifras o lugares clave.
                4. Elige el degradado del encabezado según el tono de la noticia:
                   - General, cultural, festivo o educativo: linear-gradient(135deg, #4a154b, #d53f8c, #dd6b20) o linear-gradient(135deg, #880e4f, #d81b60, #dd6b20)
                   - Institucional, crónica, solemne o comunitario: linear-gradient(135deg, #2d3748, #4a154b, #7b1fa2)

                ESTRUCTURA HTML OBLIGATORIA (sigue este diseño y estilos inline):
                <article style="width: 100%; max-width: 100%; margin: 0 auto; font-family: 'Segoe UI', system-ui, -apple-system, BlinkMacSystemFont, sans-serif; background: #ffffff; border-radius: 16px; box-shadow: 0 10px 25px rgba(0,0,0,0.07); overflow: hidden; border: 1px solid #f0f0f0; color: #2d3748; line-height: 1.7;">

                  <!-- 1. Encabezado con degradado Santiago Conecta -->
                  <div style="background: linear-gradient(135deg, #4a154b, #d53f8c, #dd6b20); padding: 26px 28px; color: #ffffff !important;">
                    <span style="display: inline-block; background: rgba(255, 255, 255, 0.22); color: #ffffff !important; padding: 4px 14px; border-radius: 20px; font-size: 13px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;">
                      [Emoji temático] [Categoría corta de la nota] • Santiago Conecta
                    </span>
                    <h1 style="margin: 14px 0 0; font-size: 26px; font-weight: 800; line-height: 1.3; color: #ffffff !important;">
                      [Título atractivo y periodístico basado en el título original]
                    </h1>
                  </div>

                  <div style="padding: 28px;">
                    <!-- 2. Resumen principal / Bajada -->
                    <p style="font-size: 17px; color: #4a5568; margin-top: 0; font-weight: 500;">
                      [Párrafo inicial o entrada atractiva que enganche al lector resumiendo el hecho principal con <strong> en datos clave]
                    </p>

                    <!-- 3. Caja de Resumen / Puntos Destacados (adapta si son puntos clave con <ul><li> y emojis, o un destacado central si es crónica) -->
                    <div style="background: #fffaf0; border: 1px solid #feebc8; border-left: 5px solid #dd6b20; border-radius: 10px; padding: 20px; margin: 22px 0;">
                      <h3 style="margin-top: 0; margin-bottom: 12px; color: #c05621 !important; font-size: 18px; display: flex; align-items: center; gap: 8px;">
                        ✨ Lo más destacado de la nota:
                      </h3>
                      <ul style="margin: 0; padding-left: 20px; color: #2d3748; font-size: 15px; line-height: 1.8;">
                        <li>[Emoji] <strong>[Concepto clave 1]:</strong> [Explicación concisa basada en la nota].</li>
                        <li>[Emoji] <strong>[Concepto clave 2]:</strong> [Explicación concisa basada en la nota].</li>
                        <li>[Emoji] <strong>[Concepto clave 3]:</strong> [Explicación concisa basada en la nota].</li>
                      </ul>
                    </div>

                    <!-- 4. Desarrollo de la nota (2 a 4 párrafos bien redactados con <p style="color: #4a5568; font-size: 15px;">, y opcionalmente una cita o recuadro intermedio si la nota es extensa) -->
                    <p style="color: #4a5568; font-size: 15px;">
                      [Desarrollo completo de los detalles de la noticia...]
                    </p>

                    <!-- 5. Caja comunitaria de Santiago Conecta -->
                    <div style="background: #faf5ff; border: 1px solid #e9d8fd; padding: 16px 20px; border-radius: 8px; margin-top: 24px;">
                      <p style="margin: 0; font-size: 14px; color: #553c9e; text-align: center;">
                        🤝 <strong>Santiago Conecta está con nuestra gente.</strong><br>
                        Somos una plataforma de Santiago y para Santiago. Si tienes una iniciativa, evento, noticia o reporte que quieras dar a conocer, contáctanos y con gusto le damos difusión.
                      </p>
                    </div>

                    <!-- 6. Cierre / Hashtags -->
                    <div style="border-top: 1px solid #edf2f7; margin-top: 24px; padding-top: 16px; color: #718096; font-size: 12px; display: flex; flex-wrap: wrap; gap: 8px;">
                      <span>#SantiagoPapasquiaro</span>
                      <span>#[HashtagRelacionado1]</span>
                      <span>#[HashtagRelacionado2]</span>
                      <span>#[HashtagRelacionado3]</span>
                      <span>#Durango</span>
                      <span>#SantiagoConecta</span>
                    </div>
                  </div>

                </article>

                DATOS DE LA NOTICIA ORIGINAL A TRANSFORMAR:
                Título: {{titulo}}

                Contenido:
                {{cuerpo}}
                """;
		}

		private static string PromptNoticia(string titulo, string contenido, string enlace)
		{
			var cuerpo = string.IsNullOrWhiteSpace(contenido)
				? "(La noticia no tiene cuerpo. Redacta solo con el título, sin inventar detalles.)"
				: contenido;

			return
				$"""
                Redacta UNA publicación de Facebook para esta noticia. Devuelve solo el texto listo para copiar, sin título de sección, sin comillas y sin bloques de código.

                Estructura obligatoria:
                1. Primera línea: empieza exactamente con "Santiago Conecta presenta esta noticia:" y después un título corto y llamativo, con 1 o 2 emojis relacionados. El título debe destacar lo principal de la nota.
                2. Un resumen breve, de 2 a 4 párrafos cortos, en tono cercano del municipio. Deja claro que la información está publicada en Santiago Conecta y que ahí se lee completa.
                3. Si la nota menciona personas o instituciones, nómbralas solo con los datos que aparezcan. No agregues cargos ni nombres que no estén en el texto.
                4. Cierre invitando a leer la nota completa y a comentar. Incluye esta línea tal cual:
                👉 Conoce más y lee la nota completa en Santiago Conecta:
                {enlace}
                5. Última línea, exactamente estos hashtags:
                {HashtagsNoticias}

                Título: {titulo}

                Contenido:
                {cuerpo}
                """;
		}

		private static string PromptEmprendimiento(string nombre, string informacion, string enlace, string sitio)
		{
			return
				$"""
                Redacta UNA publicación de Facebook para presentar este emprendimiento local. Devuelve solo el texto listo para copiar, sin comillas, sin bloques de código y sin formato Markdown (nada de asteriscos ni almohadillas para títulos).

                Estructura obligatoria, en este orden:
                1. Primera línea: una frase llamativa relacionada con el giro del negocio, con 1 o 2 emojis.
                2. Un párrafo que empiece con "En Santiago Conecta nos encanta apoyar a los talentos locales" y presente a {nombre}: qué hace, qué lo distingue y, si existen, una idea breve de su misión o visión. Usa 1 o 2 emojis.
                3. Una línea con 📍 que invite a visitarlo y, debajo, la dirección o direcciones tal como aparecen. Omite esta sección si no hay dirección.
                4. Una sección "Productos y Servicios Destacados:" con un renglón por producto o servicio: nombre, descripción corta y precio si existe (formato $123 MXN; si hay precio con descuento, menciónalo). Máximo 10 renglones; si hay más, elige los más representativos.
                5. Una sección "📲 Contáctalo:" con WhatsApp, teléfono, Facebook, Instagram u otras redes, copiando los enlaces exactamente como vienen. Omite los que no existan.
                6. Una invitación a cotizar o pedir sin compromiso, seguida de esta línea tal cual:
                👉 Conoce a {nombre} en Santiago Conecta:
                {enlace}
                7. Cierre: "¿Tienes un negocio y quieres formar parte de Santiago Conecta? Regístrate en {sitio} y haz que más gente conozca tus productos y servicios."
                8. Última línea, exactamente estos hashtags:
                {HashtagsEmprendimientos}

                Información del emprendimiento:
                {informacion}
                """;
		}

		private static string PromptProducto(string producto, string negocio, string informacion, string enlace, string sitio)
		{
			return
				$"""
                Redacta UNA publicación de Facebook para promocionar un producto o servicio de un emprendimiento local. Devuelve solo el texto listo para copiar, sin comillas, sin bloques de código y sin formato Markdown (nada de asteriscos ni almohadillas para títulos).

                Estructura obligatoria, en este orden:
                1. Primera línea: una frase llamativa sobre {producto}, con 1 o 2 emojis relacionados.
                2. Un párrafo que empiece con "En Santiago Conecta te presentamos" y presente {producto} de {negocio}: qué es y por qué vale la pena. Usa 1 o 2 emojis.
                3. Una descripción breve del producto con los datos recibidos, sin inventar características.
                4. Una línea con 💲 y el precio en formato $123 MXN. Si hay precio con descuento, muestra ambos y destaca el descuento. Omite esta línea si no hay precio.
                5. Una línea con 📍 que diga dónde conseguirlo y, debajo, la dirección o direcciones tal como aparecen. Omite esta sección si no hay dirección.
                6. Una sección "📲 Pide el tuyo:" con WhatsApp, teléfono, Facebook, Instagram u otras redes, copiando los enlaces exactamente como vienen. Omite los que no existan.
                7. Esta línea tal cual:
                👉 Conoce más de {negocio} en Santiago Conecta:
                {enlace}
                8. Cierre: "¿Tienes un negocio? Regístrate en {sitio} y da a conocer tus productos."
                9. Última línea, exactamente estos hashtags:
                {HashtagsEmprendimientos}

                Información:
                {informacion}
                """;
		}

		private static string AsegurarLinea(string texto, string marcador, string linea)
		{
			return texto.Contains(marcador, StringComparison.OrdinalIgnoreCase)
				? texto
				: $"{texto}\n\n{linea}".Trim();
		}

		private static string Recortar(string texto)
		{
			return texto.Length > MaxCaracteresEntrada ? texto[..MaxCaracteresEntrada] : texto;
		}

		private static Response<string> Ok(string texto)
		{
			return new Response<string>
			{
				IsSuccess = true,
				Data = texto,
				Message = "Texto listo para revisar."
			};
		}

	}
}
