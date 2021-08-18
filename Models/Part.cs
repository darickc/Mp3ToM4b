using System;
using System.Collections.Generic;

namespace Mp3ToM4b.Models
{
    public class Part
    {

        public int PartNumber { get; }
        public List<Chapter> Chapters { get; set; } = new();
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(0);
        public List<AudioFile> Files { get; set; } = new();

        public Part(int partNumber)
        {
            PartNumber = partNumber;
        }

        public void AddFile(AudioFile file)
        {
            Files.Add(file);
            Duration += file.Duration;
        }

        public void RemoveFile(AudioFile file)
        {
            if (Files.Contains(file))
            {
                Files.Remove(file);
                Duration -= file.Duration;
            }
        }

        public void UpdateChapters()
        {
            var time = TimeSpan.FromSeconds(0);
            foreach (var file in Files)
            {
                foreach (var chapter in file.Chapters)
                {
                    chapter.Time += time;
                    Chapters.Add(chapter);
                }

                time += file.Duration;
            }
        }
    }
}