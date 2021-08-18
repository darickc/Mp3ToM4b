using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Mp3ToM4b.Models
{
    public class AudioFile
    {
        public string Name { get; set; }
        public string EncodedName { get; set; }
        public TimeSpan Duration { get; set; }
        public List<Chapter> Chapters { get; set; }
    }
}