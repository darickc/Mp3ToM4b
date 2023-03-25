using System.Text.Json.Serialization;

namespace Mp3ToM4b.Models.Book;

public class Spine
{
    public string Path { get; set; }
    [JsonPropertyName("-odread-original-path")]
    public string OriginalPath { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public bool Downloaded { get; set; }

    [JsonIgnore]
    public double Progress { get; set; }
    
}