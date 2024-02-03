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

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
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
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainPageViewModel>();
            #region Use AddTransient HttpClient for simple requirements
            //services.AddTransient(_ =>
            //{
            //    var client = new HttpClient
            //    {
            //        Timeout = TimeSpan.FromSeconds(20)
            //    };
            //    return client;
            //});
            #endregion
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
        }

        private int GetUserTimeoutSetting()
        {
            var settingsViewModel = ServiceProvider.GetRequiredService<SettingsViewModel>();
            return settingsViewModel.Timeout;
        }

        private Window m_window;

    }
}
