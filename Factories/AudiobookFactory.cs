using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using ATL;
using CSharpFunctionalExtensions;
using Mp3ToM4b.Common;
using Mp3ToM4b.Models;
using Mp3ToM4b.Models.Book;
using Mp3ToM4b.Services;

namespace Mp3ToM4b.Factories
{
    public class AudiobookFactory
    {
        private readonly MetadataService _metadataService;
        private readonly AudioFileFactory _audioFileFactory;

        public AudiobookFactory(MetadataService metadataService, AudioFileFactory audioFileFactory)
        {
            _metadataService = metadataService;
            _audioFileFactory = audioFileFactory;
        }

        public async Task<Result<Audiobook>> Create(string folder)
        {
            if (File.Exists(folder))
            {
                return LoadAudibleFile(folder);
            }

            var jsonFile = Directory.GetFiles(folder, "*.json").FirstOrDefault(f => Path.GetFileName(f) == "book.json");
            if (string.IsNullOrEmpty(jsonFile))
            {
                return await Result.Try(() => LoadFiles(folder))
                    .Map(files => new Audiobook(files))
                    .Tap(LoadMetadata)
                    .Tap(LoadChapters);
            }
            else
            {
                Book b = null;
                return Result.Try(() => File.ReadAllText(jsonFile))
                    .Map(json => JsonSerializer.Deserialize<Book>(json, new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true
                    }))
                    .Ensure(book=> book != null, "Invalid Json")
                    .Tap(book=> b=book)
                    .Map(book => LoadFiles(folder, book))
                    .Map(files => new Audiobook(files))
                    .Tap(audiobook => LoadMetadata(audiobook,b, folder))
                    .Tap(LoadChapters);
            }
        }

        private Result<Audiobook> LoadAudibleFile(string file)
        {
            return Result.Try(()=> new Track(file))
                .Map(track =>
                {
                    var chapters = GetAudibleChapters(track).Value;
                    var audioFile = new AudioFile
                    {
                        Chapters = chapters,
                        Duration = TimeSpan.FromMilliseconds(track.DurationMs),
                        Name = track.Path,
                        EncodedName = Path.ChangeExtension(track.Path, "m4b")
                    };
                    var files = new List<AudioFile>
                    {
                        audioFile
                    };
                    var book = new Audiobook(files)
                    {
                        IsAudibleFile = true,
                        Title = track.Title,
                        Author = track.Artist,
                        Comment = track.Description
                    };

                    if (track.EmbeddedPictures.Any())
                    {
                        book.Image = track.EmbeddedPictures.First().PictureData;
                    }
                    var part = new Part(1);
                    part.AddFile(audioFile);
                    part.UpdateChapters();
                    book.Parts.Add(part);
                    return book;
                });
        }

        private Result<List<Chapter>> GetAudibleChapters(Track track)
        {
                return Result.Success(Path.ChangeExtension(track.Path, "json")) 
                    .Ensure(file=> File.Exists(file),"no file")
                    .Bind(file=> Result.Try(() => File.ReadAllText(file)))
                    .Map(json => JsonSerializer.Deserialize<List<AudibleChapter>>(json, new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNameCaseInsensitive = true
                    }))
                    .Map(chapters => chapters.Select(c=> new Chapter
                    {
                        Time = TimeSpan.FromMilliseconds(c.Start),
                        Name = c.Title
                    }).ToList())
                    .OnFailureCompensate(_=> track.Chapters.Select(c => new Chapter
                    {
                        Name = c.Title,
                        Filename = track.Path,
                        Time = TimeSpan.FromMilliseconds(c.StartTime)
                    }).ToList());
           
        }

        private async Task LoadMetadata(Audiobook book)
        {
            if (book.Files.Any())
            {
                Track track = new Track(book.Files.First().Name);
                book.Title = Regex.Replace(track.Title, " -? ?Part 0?1", "", RegexOptions.IgnoreCase);

                var artists = track.Artist?.Split("/").ToList() ?? new List<string>();
                book.Author = artists.FirstOrDefault();
                if (artists.Count > 1)
                {
                    artists.RemoveAt(0);
                    book.Narrators = string.Join(' ', artists);
                }
                book.Series = track.Album;
                book.Genre = track.Genre;
                book.Comment = track.Comment;
            }

            await RefreshMetadata(book);
        }

        private void LoadMetadata(Audiobook audiobook, Book book, string folder)
        {
            audiobook.Title = book.Title?.Main;
            audiobook.Series = book.Title?.Collection;
            audiobook.Author = book.Creator.FirstOrDefault(c => c.Role == "author")?.Name;
            audiobook.Comment = book.Description?.Short;
            var image = Directory.GetFiles(folder, "*.jpg").FirstOrDefault(f => Path.GetFileName(f) == "cover.jpg");
            if (!string.IsNullOrEmpty(image))
            {
                var bitmap = new Bitmap(image);
                audiobook.Image = bitmap.ResizeImage(250, 250).ImageToByte2();
            }
        }

        private List<AudioFile> LoadFiles(string folder)
        {
            var audioFiles = new List<AudioFile>();
            var files = Directory.GetFiles(folder, "*.mp3").ToList();
            var x = 1;
            string previousChapterName = null;
            foreach (var file in files)
            {
                var audioFile = _audioFileFactory.Create(file, previousChapterName, x);
                previousChapterName = audioFile.Chapters.Last().Name;
                audioFiles.Add(audioFile);
            }
            return audioFiles;
        }

        private List<AudioFile> LoadFiles(string folder, Book book)
        {
            var files = Directory.GetFiles(folder, "*.mp3").ToList();
            var audioFiles = new List<AudioFile>();
            foreach (var spine in book.Spine)
            {
                Result.Success(files.FirstOrDefault(f => Path.GetFileName(f) == spine.OriginalPath))
                    .Ensure(file => !string.IsNullOrEmpty(file), "No file")
                    .Map(file => _audioFileFactory.Create(book, file, spine.OriginalPath))
                    .Tap(file => audioFiles.Add(file));
            }
            return audioFiles;
        }

        private void LoadChapters(Audiobook book)
        {

            var durationLimit = TimeSpan.FromHours(22);
            var desiredParts = (int)Math.Ceiling(book.Duration.TotalMilliseconds / durationLimit.TotalMilliseconds);
            var desiredDurationLimit = TimeSpan.FromMilliseconds(book.Duration.TotalMilliseconds / desiredParts);

            var files = book.Files.ToList();
            var partNum = 1;
            do
            {
                var part = new Part(partNum);
                foreach (var file in files)
                {
                    // add files until either all files have been added or the duration limit has been reached
                    part.AddFile(file);
                    if ((part.Duration > desiredDurationLimit && partNum < desiredParts) || part.Duration > durationLimit && part.Files.Count > 1)
                    {
                        // remove files until a file that has a chapter that starts at the beginning of the file has been found
                        AudioFile lastFile;
                        do
                        {
                            lastFile = part.Files.Last();
                            part.RemoveFile(lastFile);
                        } while (!lastFile.Chapters.First().FileStart && part.Files.Count > 1);
                        break;
                    }
                }
                part.Files.ForEach(f => files.Remove(f));
                part.UpdateChapters();
                book.Parts.Add(part);
                partNum++;
            } while (files.Any());
        }

        public async Task RefreshMetadata(Audiobook book)
        {
            await _metadataService.GetMetadata(book);
        }
    }
}