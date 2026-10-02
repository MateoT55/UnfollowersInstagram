using System.Text.Json.Serialization;

namespace UnfollowersInstagram.DTOs
{
    public class FollowingResponseDto
    {
        [JsonPropertyName("relationships_following")]
        public List<FollowingDto>? RelationshipsFollowing { get; set; }
    }
}
