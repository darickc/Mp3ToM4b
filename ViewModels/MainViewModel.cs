using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ATL;
using CSharpFunctionalExtensions;
using Mp3ToM4b.Annotations;
using Mp3ToM4b.Factories;
using Mp3ToM4b.Models;
using Mp3ToM4b.Services;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using Settings = ATL.Settings;

namespace Mp3ToM4b.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AudiobookFactory _audiobookFactory;
        private readonly AudiobookService _audiobookService;
        private Maybe<Audiobook> _book;
        private string _mp3Directory;
        private string _saveDirectory;
        private bool _loading;
        private bool _isIndeterminate;
        private double _percentComplete;
        private string _progressDetail;

        public Maybe<Audiobook> Book
        {
            get => _book;
            set
            {
                if (value.Equals(_book)) return;
                _book = value;
                OnPropertyChanged();
            }
        }

        public string Mp3Directory
        {
            get => _mp3Directory;
            set
            {
                if (value == _mp3Directory) return;
                _mp3Directory = value;
                OnPropertyChanged();
            }
        }

        public string SaveDirectory
        {
            get => _saveDirectory;
            set
            {
                if (value == _saveDirectory) return;
                _saveDirectory = value;
                OnPropertyChanged();
            }
        }

        public bool Loading
        {
            get => _loading;
            set
            {
                if (value == _loading) return;
                _loading = value;
                OnPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                if (value == _isIndeterminate) return;
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        public double PercentComplete
        {
            get => _percentComplete;
            set
            {
                if (value.Equals(_percentComplete)) return;
                _percentComplete = value;
                OnPropertyChanged();
            }
        }

        public string ProgressDetail
        {
            get => _progressDetail;
            set
            {
                if (value == _progressDetail) return;
                _progressDetail = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand OpenDirectoryCommand => new(OpenDirectory);
        public DelegateCommand OpenAudibleFileCommand => new(OpenAudibleFile);
        public DelegateCommand SelectSaveDirectoryCommand => new(SelectSaveDirectory);
        public DelegateCommand SaveBookCommand => new(SaveAudibook);
        public DelegateCommand RefreshCommand => new(Refresh);
        public DelegateCommand ImageCommand => new(ChooseImage);
        public DelegateCommand<object> RemoveChapterCommand => new(o => RemoveChapter((Chapter)o));

        public MainViewModel()
        {
            Settings.FileBufferSize = 4096;
            Book = new Audiobook(new List<AudioFile>())
            {
                Title = "Test Title",
                Author = "Test Author",
                Duration = TimeSpan.FromSeconds(342.2),
                Parts = new List<Part>
                {
                    new(1)
                    {
                        Chapters = new ObservableCollection<Chapter>
                        {
                            new() { Name = "Chapter 1", Time = TimeSpan.FromSeconds(0) },
                            new() { Name = "Chapter 2", Time = TimeSpan.FromMinutes(45) }
                        }
                    },
                    new(2)
                    {
                        Chapters = new ObservableCollection<Chapter>
                        {
                            new() { Name = "Chapter 1", Time = TimeSpan.FromSeconds(0) },
                            new() { Name = "Chapter 2", Time = TimeSpan.FromMinutes(304) }
                        }
                    }
                },
                Genre = "Fiction",
                Series = "Test Series"
            };
            Mp3Directory = "C:\\Test";
            Loading = true;
            PercentComplete = 42.8;
            ProgressDetail = "Loading ...";
        }

        public MainViewModel(AudiobookFactory audiobookFactory, AudiobookService audiobookService)
        {
            _audiobookFactory = audiobookFactory;
            _audiobookService = audiobookService;
            audiobookService.OnProgress = OnProgress;
            SaveDirectory = Properties.Settings.Default.SaveDirectory;
        }

        private void OnProgress(ProgressDetail detail)
        {
            if (detail.Complete)
            {
                Loading = false;
                return;
            }

            Loading = true;
            IsIndeterminate = !detail.PercentComplete.HasValue;
            PercentComplete = detail.PercentComplete ?? 0;
            ProgressDetail = detail.Detail;
        }

        private async void OpenDirectory()
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                Mp3Directory = ookiiDialog.SelectedPath;
                await LoadAudiobook();
            }
        }

        private void SelectSaveDirectory()
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                SaveDirectory = ookiiDialog.SelectedPath;
                Properties.Settings.Default.SaveDirectory = SaveDirectory;
                Properties.Settings.Default.Save();
            }
        }

        public async Task LoadAudiobook()
        {
            IsIndeterminate = true;
            Loading = true;
            ProgressDetail = "Loading Files";
            await _audiobookFactory.Create(Mp3Directory)
                .Tap(book => Book = book)
                .TapError(e=> MessageBox.Show(e));
            Loading = false;
        }

        private async void SaveAudibook()
        {
            if (Book.HasNoValue)
            {
                return;
            }

            if (string.IsNullOrEmpty(SaveDirectory))
            {
                SelectSaveDirectory();
            }

            await _audiobookService.Save(Book.Value, SaveDirectory);
            Loading = false;
            MessageBox.Show("Creation Complete");
        }

        private async void Refresh()
        {
            IsIndeterminate = true;
            Loading = true;
            await Book.ToResult("nothing")
                .Tap(b => _audiobookFactory.RefreshMetadata(b));
            Loading = false;
        }

        public void RemoveChapter(Chapter c)
        {
            Book.ToResult("nothing")
                .Tap(b =>
                {
                    foreach (var part in b.Parts)
                    {
                        if (part.Chapters.Contains(c))
                        {
                            part.Chapters.Remove(c);
                            return;
                        }
                    }
                });
        }

        public void ChooseImage()
        {
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                Book.Value.Image = File.ReadAllBytes(dialog.FileName);
            }
        }

        private async void OpenAudibleFile()
        {
            var ookiiDialog = new VistaOpenFileDialog();
            ookiiDialog.Filter = "Audible (*.aax)|*.aax";
            if (ookiiDialog.ShowDialog() == true)
            {
                Mp3Directory = ookiiDialog.FileName;
                await LoadAudiobook();
            }
        }

    public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
