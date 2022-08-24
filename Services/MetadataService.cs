using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using CSharpFunctionalExtensions;
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
            var url = Url + HttpUtility.UrlEncode($"{book.Title} {book.Author}");
            await Result.Try(() => _client.GetStringAsync(url))
                .Map(page =>
                {
                    var html = new HtmlDocument();
                    html.LoadHtml(page);
                    return html;
                })
                .Map(html => html.DocumentNode)
                .Map(document => document.QuerySelector("#product-list-a11y-skiplink-target>span>ul>div>li:first-child>div>div:first-child>div"))
                .Check(q=> Result.Try(()=> q.QuerySelector("img:first-child"))
                    .Ensure(image => image != null && image.Attributes.Contains("data-lazy"), "nothing")
                    .Map(image => image.Attributes["data-lazy"].Value)
                    .Bind(src => Result.Try(()=> _client.GetByteArrayAsync(src)))
                    .Map(data => book.Image = data))
                .Map(q=> q.QuerySelector("div:last-child>div>div>span>ul"))
                .Ensure(items => items != null, "node not found")
                // get auther
                .Check(items => Result.Try(()=> items.ChildNodes.FirstOrDefault(n => n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("authorLabel")))
                    .Map(authorLi=> authorLi.QuerySelector("span>a")?.InnerText)
                    .TapIf(author=> author.NotEmpty(), author =>book.Author = author))
                .Map(items => items.ChildNodes.FirstOrDefault(n => n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("seriesLabel")))
                // get series
                .Ensure(seriesLi => seriesLi != null, "Series not found")
                .Map(seriesLi => seriesLi.QuerySelector("span>a:first-child"))
                .Check(seriesNode => Result.Try(()=> seriesNode?.InnerText)
                    .TapIf(series=> series.NotEmpty(), series => book.Series = series))
                // get book number
                .Map(seriesNode => seriesNode?.NextSibling)
                .Ensure(bookNode => bookNode != null && Regex.IsMatch(bookNode.InnerText, BookRegex), "no book node")
                .Map(bookNode => Regex.Match(bookNode.InnerText, BookRegex))
                .TapIf(match => match.Groups.Count > 1, match => book.BookNumber = double.Parse(match.Groups[1].Value));
        }
    }
}