using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mp3ToM4b.Factories;
using Mp3ToM4b.Services;
using Mp3ToM4b.ViewModels;
using Mp3ToM4b.Views;

namespace Mp3ToM4b
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();

            // AppSettings settings = new AppSettings();
            // Configuration.GetSection("AppSettings").Bind(settings);
            // serviceCollection.AddSingleton(settings);

            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<AudiobookFactory>();
            services.AddHttpClient<MetadataService>();
            services.AddTransient<AudiobookService>();
            services.AddTransient<AudioFileFactory>();
        }
    }
}
