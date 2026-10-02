using System.Text.Json.Serialization;

namespace UnfollowersInstagram.DTOs
{
    public class FollowingDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}
