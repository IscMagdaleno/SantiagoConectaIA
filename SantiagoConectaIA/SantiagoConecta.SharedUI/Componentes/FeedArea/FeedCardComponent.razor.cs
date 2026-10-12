using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;

using SantiagoConectaIA.Share.Objects.FeedModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConecta.SharedUI.Componentes.FeedArea
{
	public partial class FeedCardComponent
	{
		[Inject]
		public NavigationManager NavManager { get; set; } = default!;

		[Inject]
		public IDialogService DialogService { get; set; } = default!;

		[Inject]
		public IJSRuntime JS { get; set; } = default!;

		[Parameter, EditorRequired]
		public FeedCard Card { get; set; } = new();

		private IJSObjectReference? _shareModule;
		private bool _canNativeShare;
		private bool _lightboxOpen = false;
		private int _activeImageIndex = 0;
		private bool _isDescExpanded = false;
		private bool _showOpinions = false;

		private static readonly System.Globalization.CultureInfo EsMx = new("es-MX");

		// Tipos que aceptan comentarios (deben coincidir con OpinionDomain.TiposValidos).
		private static readonly HashSet<string> TiposConOpiniones = new(StringComparer.OrdinalIgnoreCase)
		{
			"NOTICIA", "TRAMITE", "EVENTO", "CAPSULA", "PUBLICACION"
		};

		private string Tipo => (Card.vchTipoEntidad ?? string.Empty).Trim().ToUpperInvariant();
		private bool IsPost => Tipo == "PUBLICACION";
		private bool IsCapsula => Tipo == "CAPSULA";
		private bool IsEvento => Tipo == "EVENTO";

		/// <summary>Prioridad 1 (post de hoy) o 2 (noticia/evento del día).</summary>
		private bool IsHoy => Card.iPrioridad is 1 or 2;
		private string HoyLabel => Card.iPrioridad == 1 ? "Nuevo hoy" : "Hoy";

		private string TypeClass => Tipo switch
		{
			"NOTICIA" => "noticia",
			"EVENTO" => "evento",
			"PUBLICACION" => "publicacion",
			"CAPSULA" => "capsula",
			_ => "otro"
		};

		private string AriaTitulo => string.IsNullOrWhiteSpace(Card.vchTitulo)
			? $"{GetTypeLabel()} de {AutorNombre}"
			: Card.vchTitulo;

		private bool CanComment => Card.iIdEntidad > 0 && TiposConOpiniones.Contains(Tipo);
		private bool HasCta => !IsPost && (IsCapsula || !string.IsNullOrWhiteSpace(Card.vchRutaDetalle));

		private string ComentariosTexto => Card.iTotalComentarios switch
		{
			<= 0 => "Comentar",
			1 => "1 comentario",
			_ => $"{Card.iTotalComentarios} comentarios"
		};

		private void ToggleOpinions() => _showOpinions = !_showOpinions;

		private string AutorNombre => string.IsNullOrWhiteSpace(Card.vchAutor) ? "Ciudadano de Santiago" : Card.vchAutor.Trim();
		private string AutorInicial => AutorNombre.Substring(0, 1).ToUpperInvariant();

		private string TiempoRelativo => Card.dtFecha.HasValue ? Card.dtFecha.Value.ToTiempoRelativo() : string.Empty;

		private (string Texto, string Icono) Categoria => (Card.vchEtiqueta ?? string.Empty).Trim().ToLowerInvariant() switch
		{
			"aviso" => ("Aviso", Icons.Material.Filled.Campaign),
			"pregunta" => ("Pregunta", Icons.Material.Filled.HelpOutline),
			"recomendacion" or "recomendación" => ("Recomendación", Icons.Material.Filled.Recommend),
			_ => ("Comunidad", Icons.Material.Filled.Forum)
		};
		private string CategoriaTexto => Categoria.Texto;
		private string CategoriaIcono => Categoria.Icono;

		private DateTime? FechaLocal => Card.dtFecha.HasValue ? Card.dtFecha.Value.ToUsuarioLocal() : null;

		/// <summary>Fecha visible bajo el contenido. Los posts muestran tiempo relativo en su cabecera y los datos curiosos no llevan fecha.</summary>
		private string FechaTexto
		{
			get
			{
				if (IsPost || IsCapsula || !FechaLocal.HasValue || FechaLocal.Value <= DateTime.MinValue)
				{
					return string.Empty;
				}

				var f = FechaLocal.Value;
				if (IsEvento)
				{
					var prefijo = f.Date == DateTime.Now.Date ? "Hoy" : f.ToString("dddd d 'de' MMMM", EsMx);
					var hora = f.TimeOfDay == TimeSpan.Zero ? string.Empty : $" · {f.ToString("h:mm tt", EsMx)}";
					return $"{prefijo}{hora}";
				}

				return f.ToString("dd MMM yyyy", EsMx);
			}
		}

		private string FullDescription
		{
			get
			{
				// Si la entidad trae el contenido detallado completo (ej. en PUBLICACION), usamos ese texto sin truncar.
				if (!string.IsNullOrWhiteSpace(Card.nvchContenidoDetallado))
				{
					return Card.nvchContenidoDetallado;
				}
				return Card.nvchDescripcion ?? string.Empty;
			}
		}

		private string CurrentDescriptionText
		{
			get
			{
				return _isDescExpanded ? FullDescription : (Card.nvchDescripcion ?? string.Empty);
			}
		}

		private void ToggleDescription()
		{
			_isDescExpanded = !_isDescExpanded;
		}

		private bool IsLongDescription()
		{
			var text = FullDescription;
			if (string.IsNullOrWhiteSpace(text)) return false;
			// Si el texto completo es más largo que la descripción previa, o tiene saltos de línea, o pasa de 140 caracteres
			return (!string.IsNullOrWhiteSpace(Card.nvchContenidoDetallado) && Card.nvchContenidoDetallado != Card.nvchDescripcion)
				   || text.Contains('\n')
				   || text.Length > 140;
		}

		private List<string> DisplayImages
		{
			get
			{
				if (Card.ImagenesUrls != null && Card.ImagenesUrls.Any(u => !string.IsNullOrWhiteSpace(u)))
				{
					return Card.ImagenesUrls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
				}
				if (!string.IsNullOrWhiteSpace(Card.vchImagenUrl))
				{
					return new List<string> { Card.vchImagenUrl };
				}
				return new List<string>();
			}
		}

		private void OpenLightbox(int index)
		{
			_activeImageIndex = Math.Clamp(index, 0, Math.Max(0, DisplayImages.Count - 1));
			_lightboxOpen = true;
		}

		private void CloseLightbox()
		{
			_lightboxOpen = false;
		}

		private void NextImage()
		{
			if (DisplayImages.Count <= 1) return;
			_activeImageIndex = (_activeImageIndex + 1) % DisplayImages.Count;
		}

		private void PrevImage()
		{
			if (DisplayImages.Count <= 1) return;
			_activeImageIndex = (_activeImageIndex - 1 + DisplayImages.Count) % DisplayImages.Count;
		}

		private void SetActiveImage(int index)
		{
			if (index >= 0 && index < DisplayImages.Count)
			{
				_activeImageIndex = index;
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!firstRender)
			{
				return;
			}

			try
			{
				_shareModule = await JS.InvokeAsync<IJSObjectReference>(
					"import", "./_content/SantiagoConecta.SharedUI/js/feedShare.js");
				_canNativeShare = await _shareModule.InvokeAsync<bool>("canShare");
				await InvokeAsync(StateHasChanged);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Feed share init error: {ex.Message}");
			}
		}

		private string GetTypeLabel() => Card.vchTipoEntidad?.ToUpperInvariant() switch
		{
			"TRAMITE" => "Trámite",
			"NOTICIA" => "Noticia",
			"EVENTO" => "Evento",
			"CAPSULA" => "Dato Curioso",
			"PUBLICACION" => "Comunidad",
			"EMPRENDIMIENTO" => "Emprendimiento",
			"PRODUCTO" => "Producto",
			_ => "Contenido"
		};

		private string GetCtaLabel() => Card.vchTipoEntidad?.ToUpperInvariant() switch
		{
			"TRAMITE" => "Ver Detalle",
			"NOTICIA" => "Leer Noticia",
			"EVENTO" => "Ir al Evento",
			"CAPSULA" => "Ver Dato Curioso",
			"PUBLICACION" => "Comunidad",
			"EMPRENDIMIENTO" => "Ver Negocio",
			"PRODUCTO" => "Ver Producto",
			_ => "Ver más"
		};

		private string GetTypeIcon() => Card.vchTipoEntidad?.ToUpperInvariant() switch
		{
			"TRAMITE" => Icons.Material.Filled.Assignment,
			"NOTICIA" => Icons.Material.Filled.Article,
			"EVENTO" => Icons.Material.Filled.Event,
			"CAPSULA" => Icons.Material.Filled.Lightbulb,
			"PUBLICACION" => Icons.Material.Filled.Forum,
			"EMPRENDIMIENTO" => Icons.Material.Filled.Storefront,
			"PRODUCTO" => Icons.Material.Filled.ShoppingBag,
			_ => Icons.Material.Filled.Info
		};

		private string GetShareUrl()
		{
			if (string.Equals(Card.vchTipoEntidad, "CAPSULA", StringComparison.OrdinalIgnoreCase)
				|| string.IsNullOrWhiteSpace(Card.vchRutaDetalle))
			{
				return NavManager.ToAbsoluteUri("/feed").AbsoluteUri;
			}

			return NavManager.ToAbsoluteUri(Card.vchRutaDetalle).AbsoluteUri;
		}

		private string GetShareText()
		{
			var title = string.IsNullOrWhiteSpace(Card.vchTitulo) ? "Santiago Conecta" : Card.vchTitulo.Trim();
			var url = GetShareUrl();

			if (string.Equals(Card.vchTipoEntidad, "CAPSULA", StringComparison.OrdinalIgnoreCase))
			{
				return $"{title} — mira este dato curioso en Santiago Conecta: {url}";
			}

			return $"{title} — {url}";
		}

		private async Task ShareNativeAsync()
		{
			if (_shareModule is null)
			{
				return;
			}

			await _shareModule.InvokeAsync<bool>("share", Card.vchTitulo, GetShareText(), GetShareUrl());
		}

		private async Task ShareWhatsAppAsync()
		{
			var text = Uri.EscapeDataString(GetShareText());
			var url = $"https://wa.me/?text={text}";
			await OpenShareUrlAsync(url);
		}

		private async Task ShareFacebookAsync()
		{
			var u = Uri.EscapeDataString(GetShareUrl());
			var url = $"https://www.facebook.com/sharer/sharer.php?u={u}";
			await OpenShareUrlAsync(url);
		}

		private async Task OpenShareUrlAsync(string url)
		{
			if (_shareModule is not null)
			{
				await _shareModule.InvokeVoidAsync("openUrl", url);
				return;
			}

			await JS.InvokeVoidAsync("open", url, "_blank");
		}

		private async Task HandleCta()
		{
			if (string.Equals(Card.vchTipoEntidad, "CAPSULA", StringComparison.OrdinalIgnoreCase))
			{
				var parameters = new DialogParameters
				{
					["Titulo"] = Card.vchTitulo,
					["Descripcion"] = Card.nvchDescripcion,
					["Contenido"] = Card.nvchContenidoDetallado,
					["IdEntidad"] = Card.iIdEntidad
				};
				var options = new DialogOptions
				{
					CloseButton = true,
					MaxWidth = MaxWidth.Medium,
					FullWidth = true
				};
				await DialogService.ShowAsync<FeedCapsulaDialog>("Dato Curioso", parameters, options);
				return;
			}

			if (!string.IsNullOrWhiteSpace(Card.vchRutaDetalle))
			{
				NavManager.NavigateTo(Card.vchRutaDetalle, forceLoad: true);
			}
		}
	}
}
