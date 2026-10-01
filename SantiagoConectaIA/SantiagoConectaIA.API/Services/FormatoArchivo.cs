using System.Text;

namespace SantiagoConectaIA.API.Services
{
    /// <summary>
    /// Identifica el formato real de un archivo por sus primeros bytes. El nombre que manda el cliente
    /// no es confiable: versiones anteriores de EngramaCoreStandar mandaban "document.pdf" o un nombre sin extensión.
    /// </summary>
    public sealed record FormatoArchivo(string MimeType, string Extension)
    {
        private const int BytesCabecera = 16;

        public static readonly FormatoArchivo Png = new("image/png", "png");
        public static readonly FormatoArchivo Jpeg = new("image/jpeg", "jpg");
        public static readonly FormatoArchivo Webp = new("image/webp", "webp");
        public static readonly FormatoArchivo Gif = new("image/gif", "gif");
        public static readonly FormatoArchivo Heic = new("image/heic", "heic");
        public static readonly FormatoArchivo Heif = new("image/heif", "heif");
        public static readonly FormatoArchivo Pdf = new("application/pdf", "pdf");

        public bool EsImagen => MimeType.StartsWith("image/", StringComparison.Ordinal);

        public static FormatoArchivo? Detectar(ReadOnlySpan<byte> bytes)
        {
            if (bytes.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47 }))
            {
                return Png;
            }

            if (bytes.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                return Jpeg;
            }

            if (bytes.StartsWith("RIFF"u8) && bytes.Length >= 12 && bytes[8..12].SequenceEqual("WEBP"u8))
            {
                return Webp;
            }

            if (bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8))
            {
                return Gif;
            }

            if (bytes.Length >= 12 && bytes[4..8].SequenceEqual("ftyp"u8))
            {
                var marca = Encoding.ASCII.GetString(bytes[8..12]);
                if (marca is "heic" or "heix" or "hevc" or "hevx")
                {
                    return Heic;
                }

                if (marca is "mif1" or "msf1" or "heif")
                {
                    return Heif;
                }
            }

            if (bytes.StartsWith("%PDF"u8))
            {
                return Pdf;
            }

            return null;
        }

        /// <summary>
        /// Lee la cabecera del stream y lo regresa al inicio para que se pueda subir completo.
        /// </summary>
        public static async Task<FormatoArchivo?> DetectarAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var cabecera = new byte[BytesCabecera];
            var leidos = await stream.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false, cancellationToken);
            stream.Position = 0;
            return Detectar(cabecera.AsSpan(0, leidos));
        }
    }
}
