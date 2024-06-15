using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using UChat.Services;
using UChat.Services.Implementations;
using UChat.Services.Interfaces;
using UChat.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UChat
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

#if DEBUG
        public static void InitializeForTesting(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Setup global exception handler
            this.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Log or handle the exception
            throw e.Exception;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            #region Create dependencies injection first
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            #endregion

            m_window = new MainWindow();
            m_window.Activate();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add services
            services.AddSingleton<ISettings, LocalSettings>();
            services.AddSingleton<IRecordingService, RecordingService>();

            #region Add TextToSpeech services
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ITextToSpeech).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList()
                .ForEach(x => services.AddTransient(typeof(ITextToSpeech), x));
            services.AddSingleton<TextToSpeechContext>();
            #endregion

            services.AddSingleton<IAudioPlayer,  AudioPlayer>();

            #region Add IHttpClientFactory
            services.AddHttpClient("SecureHttpClient", client =>
            {
                int timeout = GetUserTimeoutSetting();
                client.Timeout = TimeSpan.FromSeconds(timeout);
            });

            services.AddHttpClient("InsecureHttpClient", client =>
            {
                int timeout = GetUserTimeoutSetting();
                client.Timeout = TimeSpan.FromSeconds(timeout);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            #endregion

            // Add ApiService after IHttpClientFactory to get the lastest IHttpClientFactory Settings
            services.AddSingleton<IApiService, ApiService>();

            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainPageViewModel>();
        }

        private int GetUserTimeoutSetting()
        {
            var settingsViewModel = ServiceProvider.GetRequiredService<SettingsViewModel>();
            return settingsViewModel.Timeout;
        }

        private Window m_window;

    }
}
