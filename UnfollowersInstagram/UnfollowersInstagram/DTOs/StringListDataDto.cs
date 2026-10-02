using System.Text.Json.Serialization;

namespace UnfollowersInstagram.DTOs
{
    public class StringListDataDto
    {
        public string? Href { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
