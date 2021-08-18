using System;
using System.Threading.Tasks;
using System.Windows;
using ATL;
using Mp3ToM4b.ViewModels;

namespace Mp3ToM4b.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel MainViewModel { get; }
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            MainViewModel = mainViewModel;
            DataContext = mainViewModel;
            Loaded += MainWindow_Loaded;
            // mainViewModel.LoadAudiobook(@"C:\Users\darickc\Downloads\Overdrive\The Extraordinary Education of Nicholas Benedict");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // await MainViewModel.LoadAudiobook(@"C:\Users\darickc\Downloads\Overdrive\The Extraordinary Education of Nicholas Benedict");
        }
    }
}
