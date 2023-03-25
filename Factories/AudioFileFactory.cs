using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ATL;
using Mp3ToM4b.Common;
using Mp3ToM4b.Models;
using Mp3ToM4b.Models.Book;

namespace Mp3ToM4b.Factories
{
    public class AudioFileFactory
    {
        public AudioFile Create(string filename, string previousChapter, int filenumber)
        {

            var chapters = new List<Chapter>();
            var track = new Track(filename);
            if (track.AdditionalFields.TryGetValue("OverDrive MediaMarkers", out var xmarkers))
            {
                xmarkers = xmarkers.Replace("&nbsp;", "");
                XDocument xdoc = XDocument.Parse(xmarkers);
                var markers = xdoc.Descendants("Marker").Select(
                    m => new Chapter
                    {
                        Name = m.Descendants("Name").First().Value,
                        Time = m.Descendants("Time").First().Value.ToTimeSpan(),
                    }).ToList();
                foreach (var chapter in markers)
                {
                    if ((!string.IsNullOrWhiteSpace(chapter.Name) && chapter.Name.Length > 0 && (chapter.Name[0] == 160 || chapter.Name[0] == 32)) || previousChapter == chapter.Name)
                    {
                        previousChapter = chapter.Name;
                        continue;
                    }

                    previousChapter = chapter.Name;
                    chapter.FileStart = chapter.Time.TotalMilliseconds == 0;
                    chapters.Add(chapter);
                }
            }

            if (!chapters.Any())
            {
                chapters.Add(new Chapter
                {
                    Name = $"Part {filenumber}",
                    FileStart = true,
                    Time = TimeSpan.FromSeconds(0)
                });
            }

            var file = new AudioFile
            {
                Name = filename,
                EncodedName = Path.ChangeExtension(filename, "m4a"),
                Duration = TimeSpan.FromMilliseconds(track.DurationMs),
                Chapters = chapters
            };
            return file;
        }

        public AudioFile Create(Book book, string filename, string path)
        {
            var track = new Track(filename);
            var file = new AudioFile
            {
                Name = filename,
                EncodedName = Path.ChangeExtension(filename, "m4a"),
                Duration = TimeSpan.FromMilliseconds(track.DurationMs),
                Chapters = new List<Chapter>()
            };

            if (book.Nav?.Toc?.Any() == true)
            {
                var items = book.Nav.Toc.Where(t => t.Path.StartsWith(path)).ToList();
                foreach (var item in items)
                {
                    var time = 0;
                    if (item.Path.Contains("#"))
                    {
                        var temp = item.Path.Replace($"{path}#", "");
                        int.TryParse(temp, out time);
                    }

                    var chapter = new Chapter
                    {
                        Time = TimeSpan.FromSeconds(time),
                        FileStart = time == 0,
                        Filename = filename,
                        Name = item.Title
                    };
                    file.Chapters.Add(chapter);
                }
            }

            return file;
        }
    }
}