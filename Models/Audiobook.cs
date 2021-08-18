using System;
using System.Collections.Generic;
using System.Linq;

namespace Mp3ToM4b.Models
{
    public class Audiobook
    {
        public List<AudioFile> Files { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Narrators { get; set; }
        public string Series { get; set; }
        public double? BookNumber { get; set; }
        public string Genre { get; set; }
        public string Comment { get; set; }
        public TimeSpan Duration { get; set; }
        public byte[] Image { get; set; }

        public List<Chapter> Chapters { get; set; } = new();

        public List<Part> Parts { get; set; } = new();

        public Audiobook(List<AudioFile> files)
        {
            Files = files;
            Duration = TimeSpan.FromMilliseconds(Files.Sum(f => f.Duration.TotalMilliseconds));
        }

        // public void Split()
        // {
        //     var partCount = (int) Math.Ceiling(Duration.TotalHours / 22);
        //     var timeSplit = Duration.TotalMilliseconds / partCount;
        //     var chapters = Chapters.ToList();
        //     var files = Files.ToList();
        //
        //     for (int i = 1; i < partCount + 1; i++)
        //     {
        //         var temp = chapters.Last(c => c.FileStart && c.Time.TotalMilliseconds <= (timeSplit * i) || i == partCount);
        //
        //         var index = chapters.IndexOf(temp);
        //         if (index > 0 && index != chapters.Count - 1)
        //         {
        //             index--;
        //         }
        //
        //         var j = index + 1;
        //         var part = new Part
        //         {
        //             PartNumber = i,
        //             Chapters = chapters.GetRange(0, j)
        //         };
        //         chapters.RemoveRange(0, j);
        //         var lastFileIndex = files.IndexOf(part.Chapters.Last().Filename) + 1;
        //         part.Files = files.GetRange(0, lastFileIndex);
        //         files.RemoveRange(0, lastFileIndex);
        //
        //         // adjust chapters
        //         if (part.Chapters.First().Time.TotalMilliseconds > 0)
        //         {
        //             var time = part.Chapters.First().Time;
        //             foreach (var chapter in part.Chapters)
        //             {
        //                 chapter.Time -= time;
        //             }
        //         }
        //     }
        // }
    }
}