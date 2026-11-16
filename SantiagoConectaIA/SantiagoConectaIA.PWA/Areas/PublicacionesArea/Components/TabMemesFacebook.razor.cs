using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;

using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.Objects.MemesModule;
using SantiagoConectaIA.Share.PostModels.MemesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
	public partial class TabMemesFacebook : ComponentBase
	{
		private const int TamanoPagina = 24;
		private const int MaxTendencias = 100;
		private const int MinCaracteresIdea = 3;

		[Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
		[Inject] public ISnackbar Snackbar { get; set; } = default!;
		[Inject] public IJSRuntime JSRuntime { get; set; } = default!;

		public static readonly (string Valor, string Etiqueta)[] Periodos =
		{
			("24h", "24 horas"),
			("7d", "7 días"),
			("30d", "30 días")
		};

		// Cada consulta de plantillas lleva un número: si llega una respuesta vieja (el usuario ya cambió el filtro) se descarta.
		private int _consulta;
		private int _limiteTendencias = TamanoPagina;
		private int _totalPlantillas;

		public bool Cargando { get; set; } = true;
		public bool CargandoPlantillas { get; set; }
		public bool Creando { get; set; }
		public bool Publicando { get; set; }
		public bool Descargando { get; set; }

		public string Modo { get; set; } = ModosMeme.Tendencias;
		public string Periodo { get; set; } = "7d";
		public string Busqueda { get; set; } = string.Empty;
		public string Categoria { get; set; } = string.Empty;
		public string? ErrorPlantillas { get; set; }
		public List<PlantillaMeme> Plantillas { get; set; } = new();
		public List<string> Categorias { get; set; } = new();

		public PlantillaMeme? Seleccion { get; set; }
		public string Idea { get; set; } = string.Empty;
		public MemeCreado? Resultado { get; set; }
		public string Mensaje { get; set; } = string.Empty;

		public ResumenMemes Resumen { get; set; } = new();
		public List<MemeCreado> Recientes { get; set; } = new();

		public bool EsTendencias => Modo == ModosMeme.Tendencias;
		public bool Ocupado => Creando || Publicando;
		public bool LimiteAlcanzado => Resumen.iLimiteDiario > 0 && Resumen.iUsoHoy >= Resumen.iLimiteDiario;
		public bool PuedeCrear => !Creando && !LimiteAlcanzado && Seleccion != null && Idea.Trim().Length >= MinCaracteresIdea;
		public bool PuedePublicar => Resultado != null && !string.IsNullOrWhiteSpace(Mensaje) && !Ocupado;

		public bool HayMasPlantillas => string.IsNullOrWhiteSpace(ErrorPlantillas) && Plantillas.Count > 0 && (EsTendencias
			? Plantillas.Count >= _limiteTendencias && _limiteTendencias < MaxTendencias
			: Plantillas.Count < _totalPlantillas);

		protected override async Task OnInitializedAsync()
		{
			await Task.WhenAll(CargarPlantillasAsync(), CargarResumenAsync(), CargarRecientesAsync());
			Cargando = false;
		}

		// ---------------------------------------------------------------- Plantillas

		public async Task CargarPlantillasAsync(bool masResultados = false)
		{
			var consulta = ++_consulta;
			CargandoPlantillas = true;
			ErrorPlantillas = null;
			StateHasChanged();

			if (!masResultados)
			{
				_limiteTendencias = TamanoPagina;
			}
			else if (EsTendencias)
			{
				_limiteTendencias = Math.Min(MaxTendencias, _limiteTendencias + TamanoPagina);
			}

			var modelo = new PostGetPlantillasMeme
			{
				vchModo = Modo,
				vchPeriodo = Periodo,
				vchBusqueda = EsTendencias || string.IsNullOrWhiteSpace(Busqueda) ? null : Busqueda.Trim(),
				vchCategoria = string.IsNullOrWhiteSpace(Categoria) ? null : Categoria,
				iLimite = EsTendencias ? _limiteTendencias : TamanoPagina,
				iDesde = !EsTendencias && masResultados ? Plantillas.Count : 0
			};

			var respuesta = await Publicaciones.GetPlantillasMeme(modelo);
			if (consulta != _consulta)
			{
				return;
			}

			CargandoPlantillas = false;
			if (!respuesta.IsSuccess || respuesta.Data == null)
			{
				ErrorPlantillas = string.IsNullOrWhiteSpace(respuesta.Message) ? "No se pudieron cargar las plantillas." : respuesta.Message;
				if (!masResultados)
				{
					Plantillas = new();
				}
				return;
			}

			if (!EsTendencias && masResultados)
			{
				var conocidas = Plantillas.Select(p => p.vchId).ToHashSet();
				Plantillas.AddRange(respuesta.Data.Plantillas.Where(p => !conocidas.Contains(p.vchId)));
			}
			else
			{
				Plantillas = respuesta.Data.Plantillas;
			}

			_totalPlantillas = respuesta.Data.iTotal;
			if (respuesta.Data.Categorias.Count > 0)
			{
				Categorias = respuesta.Data.Categorias;
			}
		}

		public async Task CambiarModoAsync(string modo)
		{
			if (Modo == modo)
			{
				return;
			}

			Modo = modo;
			Categoria = string.Empty;
			await CargarPlantillasAsync();
		}

		public async Task CambiarPeriodoAsync(string periodo)
		{
			if (Periodo == periodo)
			{
				return;
			}

			Periodo = periodo;
			await CargarPlantillasAsync();
		}

		public async Task CambiarCategoriaAsync(string categoria)
		{
			if (Categoria == categoria)
			{
				return;
			}

			Categoria = categoria;
			await CargarPlantillasAsync();
		}

		public Task BuscarAsync(string _) => CargarPlantillasAsync();

		public void Seleccionar(PlantillaMeme plantilla)
		{
			if (Creando || Seleccion?.vchId == plantilla.vchId)
			{
				return;
			}

			Seleccion = plantilla;
			// El meme que se ve pertenece a la plantilla anterior.
			Resultado = null;
		}

		// ---------------------------------------------------------------- Crear

		public async Task Crear()
		{
			if (!PuedeCrear)
			{
				return;
			}

			Creando = true;
			var respuesta = await Publicaciones.CrearMeme(Seleccion!.vchId, Idea.Trim());
			Creando = false;

			if (!respuesta.IsSuccess || respuesta.Data?.Meme == null || string.IsNullOrWhiteSpace(respuesta.Data.Meme.vchResultadoUrl))
			{
				Snackbar.Add(string.IsNullOrWhiteSpace(respuesta.Message) ? "No se pudo crear el meme." : respuesta.Message, Severity.Error);
				await CargarResumenAsync();
				return;
			}

			Resultado = respuesta.Data.Meme;
			Resumen = new ResumenMemes { iUsoHoy = respuesta.Data.iUsoHoy, iLimiteDiario = respuesta.Data.iLimiteDiario };
			Recientes.Insert(0, Resultado);
			Snackbar.Add("Meme creado. Revísalo y publícalo cuando quieras.", Severity.Success);
		}

		// ---------------------------------------------------------------- Acciones del resultado

		public async Task Publicar()
		{
			if (!PuedePublicar)
			{
				return;
			}

			Publicando = true;
			var respuesta = await Publicaciones.PublicarFacebook(Mensaje.Trim(), Resultado!.vchResultadoUrl);
			Publicando = false;

			if (respuesta.IsSuccess)
			{
				Snackbar.Add("¡Meme publicado en Facebook con éxito!", Severity.Success);
			}
			else
			{
				Snackbar.Add("No se pudo publicar: " + respuesta.Message, Severity.Error);
			}
		}

		public async Task Descargar(MemeCreado meme)
		{
			if (Descargando)
			{
				return;
			}

			Descargando = true;
			try
			{
				// Blob no permite leerse directo desde el navegador (CORS): la imagen pasa por el servidor.
				var respuesta = await Publicaciones.ProxyImageBase64(meme.vchResultadoUrl);
				if (!respuesta.IsSuccess || string.IsNullOrWhiteSpace(respuesta.Data))
				{
					Snackbar.Add("No se pudo descargar el meme: " + respuesta.Message, Severity.Error);
					return;
				}

				var nombre = $"santiago-conecta-meme-{(meme.iIdMeme > 0 ? meme.iIdMeme.ToString() : DateTime.Now.ToString("yyyyMMdd-HHmmss"))}";
				await JSRuntime.InvokeVoidAsync("descargas.descargarDataUrl", respuesta.Data, nombre);
			}
			finally
			{
				Descargando = false;
			}
		}

		public async Task Descartar(MemeCreado meme)
		{
			if (meme.iIdMeme > 0)
			{
				var respuesta = await Publicaciones.DescartarMeme(meme.iIdMeme);
				if (!respuesta.IsSuccess)
				{
					Snackbar.Add("No se pudo quitar el meme: " + respuesta.Message, Severity.Error);
					return;
				}
			}

			Recientes.RemoveAll(m => m.iIdMeme == meme.iIdMeme);
			if (Resultado?.iIdMeme == meme.iIdMeme)
			{
				Resultado = null;
			}

			Snackbar.Add("Meme quitado.", Severity.Info);
		}

		public async Task EditarDeNuevo(MemeCreado meme)
		{
			Seleccion = new PlantillaMeme
			{
				vchId = meme.vchPlantillaId,
				nvchNombre = meme.nvchPlantillaNombre,
				vchImagenUrl = meme.vchPlantillaUrl
			};
			Idea = meme.nvchIdea;
			Resultado = meme;
			await JSRuntime.InvokeVoidAsync("scrollTo", 0, 0);
		}

		// ---------------------------------------------------------------- Datos

		private async Task CargarResumenAsync()
		{
			var respuesta = await Publicaciones.GetResumenMemes();
			if (respuesta.IsSuccess && respuesta.Data != null)
			{
				Resumen = respuesta.Data;
			}
		}

		private async Task CargarRecientesAsync()
		{
			var respuesta = await Publicaciones.GetMemesRecientes();
			if (respuesta.IsSuccess && respuesta.Data != null)
			{
				Recientes = respuesta.Data;
			}
		}

		public static string Extracto(string texto) =>
			texto.Length > 140 ? $"{texto[..140].TrimEnd()}…" : texto;
	}
}
