using System.Text.Json.Serialization;

namespace Mp3ToM4b.Models.Book;

public class Book
{
    public Title Title { get; set; }
    public Creator[] Creator { get; set; }
    public Description Description { get; set; }
    public Nav Nav { get; set; }
    public Spine[] Spine { get; set; }

    [JsonPropertyName("-odread-furbish-uri")]
    public string CoverUrl { get; set; }
    
}