using System.Text.Json.Serialization;

namespace Mp3ToM4b.Models;

public class AudibleChapter
{
    [JsonPropertyName("start_offset_ms")]
    public double Start { get; set; }
    public string Title { get; set; }
}