using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using ATL;
using CSharpFunctionalExtensions;
using Mp3ToM4b.Common;
using Mp3ToM4b.Models;
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
            return await Result.Try(() => LoadFiles(folder))
                .Map(files => new Audiobook(files))
                .Tap(LoadMetadata)
                .Tap(LoadChapters);
        }

        private async Task LoadMetadata(Audiobook book)
        {
            if (book.Files.Any())
            {
                Track track = new Track(book.Files.First().Name);
                book.Title = Regex.Replace(track.Title," -? ?Part 0?1", "", RegexOptions.IgnoreCase);

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

            await _metadataService.GetMetadata(book);
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

        private void LoadChapters(Audiobook book)
        {
            
            var durationLimit = TimeSpan.FromHours(22);
            var desiredParts =(int) Math.Ceiling(book.Duration.TotalMilliseconds / durationLimit.TotalMilliseconds);
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
                part.Files.ForEach(f=>files.Remove(f));
                part.UpdateChapters();
                book.Parts.Add(part);
                partNum++;
            } while (files.Any());
        }
    }
}