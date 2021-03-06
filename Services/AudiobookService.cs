using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using Mp3ToM4b.Common;
using Mp3ToM4b.Models;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Mp3ToM4b.Services
{
    public class AudiobookService
    {
        private string _currentFile;
        private int _currentFileNumber;
        private int _totalFiles;
        public Action<ProgressDetail> OnProgress { get; set; }
        public async Task Save(Audiobook book, string directory)
        {
            var di = CreateDirectories(book, directory);
            var filename = GetFilename(book, di);
            await SetupEncoder();
            await EncodeFiles(book);
            await CombineFiles(book, filename);
            //SetMetadataAndChapters(book, filename);
        }

        private void SendProgress(string detail, double? percent = null)
        {
            OnProgress?.Invoke(new ProgressDetail
            {
                Detail = detail,
                PercentComplete = percent
            });
        }

        private DirectoryInfo CreateDirectories(Audiobook book, string directory)
        {
            var di = new DirectoryInfo(directory);
            SendProgress("Creating Directory");
            var folderName = book.Author.RemoveInvalidChars();
            var dir = di.GetDirectories().FirstOrDefault(d => d.Name == folderName);
            if (dir != null)
            {
                return dir;
            }
            return di.CreateSubdirectory(folderName);
        }

        private string GetFilename(Audiobook book, DirectoryInfo di)
        {
            var filename = book.Series + (!string.IsNullOrEmpty(book.Series) ? " - " : "");
            if (book.BookNumber.HasValue)
            {
                filename += $"{book.BookNumber} ";
            }

            filename += book.Title;
            return Path.Combine(di.FullName, filename.RemoveInvalidChars());
        }

        private async Task SetupEncoder()
        {
            SendProgress("Downloading FFmpeg");
            double tempPercent = 0;
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full, new Progress<ProgressInfo>(p =>
            {
                double percent = ((double)p.DownloadedBytes / p.TotalBytes) * 100;
                if (percent > tempPercent)
                {
                    SendProgress("Downloading FFmpeg", percent);
                    tempPercent = percent;
                }
            }));
            SendProgress("Download Complete");
        }

        private async Task EncodeFiles(Audiobook book)
        {
            var files = book.Parts.SelectMany(p => p.Files).ToList();
            _totalFiles = files.Count;
            var x = 0;
            foreach (var file in files)
            {
                _currentFileNumber = ++x;
                _currentFile = Path.GetFileName(file.Name);
                if (!File.Exists(file.EncodedName))
                {
                    string arguments = $"-i \"{file.Name}\" -c:a aac -b:a 64k -vn -f mp4 \"{file.EncodedName}\"";
                    var conversion = FFmpeg.Conversions.New();
                    conversion.OnProgress += Conversion_OnProgress;
                    await conversion.Start(arguments);
                }
            }
        }

        private async Task CombineFiles(Audiobook book, string filename)
        {
            foreach (var bookPart in book.Parts)
            {
                var name = filename;
                if (book.Parts.Count > 1)
                {
                    name += $" Part {bookPart.PartNumber}";
                }

                name += ".m4b";
                if (!File.Exists(name))
                {
                    var txt = Path.Combine(Path.GetDirectoryName(bookPart.Files.First().Name), "files.txt");
                    await File.WriteAllTextAsync(txt, string.Join(Environment.NewLine, bookPart.Files.Select(f => $"file '{f.EncodedName.EscapeName()}'")));
                    string arguments = $"-safe 0 -f concat -i \"{txt}\" -c copy \"{name}\"";
                    var conversion = FFmpeg.Conversions.New();
                    conversion.OnProgress += Concat_OnProgress;
                    await conversion.Start(arguments);
                }

                SetMetadataAndChapters(book, bookPart, name);
            }
        }

        private void SetMetadataAndChapters(Audiobook book, Part bookPart, string filename)
        {
            SendProgress("Saving Metadata");
            Track track = new Track(filename)
            {
                Title = book.Title,
                Album = book.Series,
                Artist = book.Author,
                DiscNumber = (int)(book.BookNumber ?? 0),
                Comment = book.Comment,
                Genre = book.Genre,
                TrackNumber = bookPart.PartNumber
            };
            if (book.Image != null && book.Image.Length > 0)
            {
                track.EmbeddedPictures.Add(PictureInfo.fromBinaryData(book.Image));
            }

            if (bookPart.Chapters?.Any() == true)
            {
                track.Chapters = bookPart.Chapters.Select(c => new ChapterInfo
                {
                    Title = c.Name,
                    StartTime = (uint)c.Time.TotalMilliseconds
                }).ToList();
            }

            track.Save();
        }

        private void Conversion_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            SendProgress($"Encoding {_currentFile} ({_currentFileNumber} of {_totalFiles}): {args.Duration} / {args.TotalLength}", args.Percent);
        }
        private void Concat_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            SendProgress($"Combining Files: {args.Duration} / {args.TotalLength}", args.Percent);
        }
    }
}