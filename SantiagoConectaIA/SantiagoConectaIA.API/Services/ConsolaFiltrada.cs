using System.Text;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// La librería EngramaCoreStandar imprime con <c>Console.WriteLine</c> cada <c>EXEC SCIA.sp...</c> que ejecuta,
	/// y no pasa por el sistema de logging de ASP.NET, así que <c>appsettings</c> no puede silenciarlo.
	/// Este filtro envuelve la salida de la consola y descarta únicamente las líneas de los procedimientos indicados
	/// (por ejemplo la bitácora de la API, que genera una por cada petición y tapa el resto de la consola).
	/// </summary>
	public sealed class ConsolaFiltrada : TextWriter
	{
		private readonly TextWriter _destino;
		private readonly string[] _procedimientosOmitidos;

		private ConsolaFiltrada(TextWriter destino, string[] procedimientosOmitidos)
		{
			_destino = destino;
			_procedimientosOmitidos = procedimientosOmitidos;
		}

		public override Encoding Encoding => _destino.Encoding;

		/// <summary>Reemplaza la salida estándar por una que omite los EXEC de los procedimientos indicados.</summary>
		public static void Instalar(params string[] procedimientosOmitidos)
		{
			if (procedimientosOmitidos.Length == 0 || Console.Out is ConsolaFiltrada)
			{
				return;
			}

			var filtrada = new ConsolaFiltrada(Console.Out, procedimientosOmitidos);
			Console.SetOut(TextWriter.Synchronized(filtrada));
		}

		private bool DebeOmitir(string? texto)
		{
			if (string.IsNullOrEmpty(texto) || !texto.StartsWith("EXEC ", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			foreach (var procedimiento in _procedimientosOmitidos)
			{
				if (texto.Contains(procedimiento, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		public override void Write(char value) => _destino.Write(value);

		public override void Write(char[] buffer, int index, int count) => _destino.Write(buffer, index, count);

		public override void Write(string? value)
		{
			if (!DebeOmitir(value))
			{
				_destino.Write(value);
			}
		}

		public override void WriteLine(string? value)
		{
			if (!DebeOmitir(value))
			{
				_destino.WriteLine(value);
			}
		}

		public override void WriteLine() => _destino.WriteLine();

		public override void Flush() => _destino.Flush();
	}
}
