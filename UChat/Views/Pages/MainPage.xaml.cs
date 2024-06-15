using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using UChat.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Lazy<MainPageViewModel> LazyMainPageViewModel { get; } = new Lazy<MainPageViewModel>(() => App.ServiceProvider.GetRequiredService<MainPageViewModel>());
        public MainPageViewModel MainPageViewModel { get { return LazyMainPageViewModel.Value; } }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.DataContext = MainPageViewModel;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Capture the pointer to ensure you continue receiving pointer events even if the pointer moves outside the Border
            (sender as UIElement)?.CapturePointer(e.Pointer);
            MainPageViewModel.IsCancelAction = false; // Reset the flag
            // Start recording action
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            dispatcherQueue.TryEnqueue(async () =>
            {
                await MainPageViewModel.StartRecordingCommand.ExecuteAsync(null);
            });
        }

        private void Border_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            (sender as UIElement)?.ReleasePointerCapture(e.Pointer);
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            dispatcherQueue.TryEnqueue(async () =>
            {
                if (MainPageViewModel.IsCancelAction)
                {
                    // Cancel recording action
                    await MainPageViewModel.CancelRecordingCommand.ExecuteAsync(null);
                }
                else
                {
                    await MainPageViewModel.StopRecordingCommand.ExecuteAsync(null);
                    await MainPageViewModel.SendCommand.ExecuteAsync(null);
                }
            });
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void RecordBorder_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Assuming you have logic here to determine if the pointer is in the cancel area
            // Check if the pointer is within the special area (e.g., over the CancelButton)
            var position = e.GetCurrentPoint(null).Position;
            
            var cancelBtnPosition = CancelButton.TransformToVisual(MainWindow.Instance.Content).TransformPoint(new Point(0, 0));
            var cancelBtnRect = new Rect(cancelBtnPosition, new Size(CancelButton.ActualWidth, CancelButton.ActualHeight));

            /* Determine if in cancel area */
            MainPageViewModel.IsCancelAction = cancelBtnRect.Contains(position);
        }
    }
}
