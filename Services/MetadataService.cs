using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Mp3ToM4b.Common;
using Mp3ToM4b.Models;

namespace Mp3ToM4b.Services
{
    public class MetadataService
    {
        private const string Url = "https://www.audible.com/search?keywords=";
        private readonly HttpClient _client;
        private const string BookRegex = "Book (\\.?\\d*)";
        private const string Content = @"C:\Users\darickc\Downloads\Overdrive\The Extraordinary Education of Nicholas Benedict/temp.html";

        public MetadataService(HttpClient client)
        {
            _client = client;
        }

        public async Task GetMetadata(Audiobook book)
        {
            try
            {
                string page;
                // if (File.Exists(Content))
                // {
                //     page = File.ReadAllText(Content);
                // }
                // else
                // {
                page = await _client.GetStringAsync(Url + HttpUtility.UrlEncode($"{book.Title} {book.Author} {book.Narrators}"));
                //     await File.WriteAllTextAsync(@"C:\Users\darickc\Downloads\Overdrive\The Extraordinary Education of Nicholas Benedict/temp.html", page);
                // }
                var html = new HtmlDocument();
                html.LoadHtml(page);
                var document = html.DocumentNode;
                var q = document.QuerySelector("#product-list-a11y-skiplink-target>span>ul>div>li:first-child>div>div:first-child>div");

                // get image
                var image = q.QuerySelector("img:first-child");
                if (image != null && image.Attributes.Contains("data-lazy"))
                {
                    var src = image.Attributes["data-lazy"].Value;
                    book.Image = await _client.GetByteArrayAsync(src);
                }
                
                var items = q.QuerySelector("div:last-child>div>div>span>ul");

                // get author
                var authorLi = items.ChildNodes.FirstOrDefault(n =>
                    n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("authorLabel"));
                var author = authorLi.QuerySelector("span>a")?.InnerText;
                if (author.NotEmpty())
                {
                    book.Author = author;
                }

                // get series
                var seriesLi = items.ChildNodes.FirstOrDefault(n =>
                    n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("seriesLabel"));
                var seriesNode = seriesLi.QuerySelector("span>a:first-child");
                var series = seriesNode?.InnerText;
                if (series.NotEmpty())
                {
                    book.Series = series;
                }

                var bookNode = seriesNode.NextSibling;
                if (bookNode != null && Regex.IsMatch(bookNode.InnerText, BookRegex))
                {
                    var match = Regex.Match(bookNode.InnerText, BookRegex);
                    if (match.Groups.Count > 1)
                    {
                        book.BookNumber = double.Parse(match.Groups[1].Value);
                    }
                }

            }
            catch(Exception e)
            {
                
            }
        }
    }
}