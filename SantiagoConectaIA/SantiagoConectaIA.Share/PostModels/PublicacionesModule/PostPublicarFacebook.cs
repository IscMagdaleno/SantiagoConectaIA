using System.Text.Json.Serialization;

namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
    public class PostPublicarFacebook
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
