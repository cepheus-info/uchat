using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private Lazy<SettingsViewModel> LazySettingsViewModel { get; } = new Lazy<SettingsViewModel>(() => App.ServiceProvider.GetRequiredService<SettingsViewModel>());
        public SettingsViewModel SettingsViewModel { get { return LazySettingsViewModel.Value; } }

        //#region delegate ManipulationCompletedEventHandler will be initialized in constructor
        //private readonly DragCompletedEventHandler manipulationCompletedEventHandler;

        //public void Slider_ManipulationCompleted(object sender, DragCompletedEventArgs e)
        //{
        //    SettingsViewModel.TimeoutSetting = SettingsViewModel.Timeout;
        //}
        //#endregion

        public Settings()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.DataContext = SettingsViewModel;

            SettingsViewModel.PropertyChanged += ViewModel_PropertyChanged;

            //#region AddHandler for delegate ManipulationCompletedEventHandler
            //this.manipulationCompletedEventHandler = Slider_ManipulationCompleted;
            //slider.AddHandler(DragLeaveEvent, manipulationCompletedEventHandler, handledEventsToo: true);
            //#endregion
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingsViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            //slider.RemoveHandler(ManipulationCompletedEvent, manipulationCompletedEventHandler);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.AcceptInsecureConnection))
            {
                UpdateAcceptInsecureConnection();
                var mainPageViewModel = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
                mainPageViewModel.OperationHistory.Add($"Accept Insecure Connection: {SettingsViewModel.AcceptInsecureConnection}");
            }
        }

        private void UpdateAcceptInsecureConnection()
        {
            if (SettingsViewModel.AcceptInsecureConnection)
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }
        }
    }
}
