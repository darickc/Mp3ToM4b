using System;

namespace Mp3ToM4b.Models
{
    public class Chapter
    {
        public string Name { get; set; }
        public TimeSpan Time { get; set; }
        public bool FileStart { get; set; }

        public string Filename { get; set; }
    }
}