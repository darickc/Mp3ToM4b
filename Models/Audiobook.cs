using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ATL;
using Mp3ToM4b.Annotations;

namespace Mp3ToM4b.Models
{
    public class Audiobook : INotifyPropertyChanged
    {
        private byte[] _image;
        private string _title;
        private string _author;
        private string _narrators;
        private string _series;
        private double? _bookNumber;
        private string _genre;
        private string _comment;
        public List<AudioFile> Files { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                if (value == _author) return;
                _author = value;
                OnPropertyChanged();
            }
        }

        public string Narrators
        {
            get => _narrators;
            set
            {
                if (value == _narrators) return;
                _narrators = value;
                OnPropertyChanged();
            }
        }

        public string Series
        {
            get => _series;
            set
            {
                if (value == _series) return;
                _series = value;
                OnPropertyChanged();
            }
        }

        public double? BookNumber
        {
            get => _bookNumber;
            set
            {
                if (Nullable.Equals(value, _bookNumber)) return;
                _bookNumber = value;
                OnPropertyChanged();
            }
        }

        public string Genre
        {
            get => _genre;
            set
            {
                if (value == _genre) return;
                _genre = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan Duration { get; set; }

        public byte[] Image
        {
            get => _image;
            set
            {
                if (Equals(value, _image)) return;
                _image = value;
                OnPropertyChanged();
            }
        }

        public List<Chapter> Chapters { get; set; } = new();

        public List<Part> Parts { get; set; } = new();

        public bool IsAudibleFile { get; set; }

        public Audiobook(List<AudioFile> files)
        {
            Files = files;
            Duration = TimeSpan.FromMilliseconds(Files.Sum(f => f.Duration.TotalMilliseconds));
        }

        // public Audiobook(Track track)
        // {
        //     IsAudibleFile = true;
        //     Chapters = track.Chapters.Select(c => new Chapter
        //     {
        //         Name = c.Title,
        //         Filename = track.Path,
        //         Time = TimeSpan.FromMilliseconds(c.StartTime)
        //     }).ToList();
        //     Duration = TimeSpan.FromMilliseconds(track.DurationMs);
        //     Files = new List<AudioFile>
        //     {
        //         new()
        //         {
        //             Chapters =Chapters.ToList(),
        //             Duration = TimeSpan.FromMilliseconds(track.DurationMs),
        //             Name = track.Path,
        //             EncodedName = Path.ChangeExtension(track.Path,"m4b")
        //         }
        //     }; 
        //     Parts = new List<Part>
        //     {
        //         new (1)
        //         {
        //             Chapters = new ObservableCollection<Chapter>(Chapters),
        //             Files = Files,
        //             Duration = Duration
        //         }
        //     };
        //    
        //     Title = track.Title;
        //     Author = track.Artist;
        //     Comment = track.Description;
        //     if (track.EmbeddedPictures.Any())
        //     {
        //         Image = track.EmbeddedPictures.First().PictureData;
        //     }
        // }

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
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}