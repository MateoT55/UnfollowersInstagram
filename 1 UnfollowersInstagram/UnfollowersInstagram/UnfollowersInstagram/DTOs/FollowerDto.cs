using System.Text.Json.Serialization;

namespace UnfollowersInstagram.DTOs
{
    public class FollowerDto
    {
        [JsonPropertyName("string_list_data")]
        public List<StringListDataDto>? StringListData { get; set; }
    }
}
