using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using UChat.ViewModels;
using Windows.Foundation;

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

        private Canvas WaveformCanvas => MainPageViewModel.IsReleaseToSendVisible ? SendCanvas : CancelCanvas;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.DataContext = MainPageViewModel;

            MainPageViewModel.WaveformPoints.CollectionChanged += WaveformPoints_CollectionChanged;
            MainPageViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private async void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Capture the pointer to ensure you continue receiving pointer events even if the pointer moves outside the Border
            (sender as UIElement)?.CapturePointer(e.Pointer);
            MainPageViewModel.IsCancelAction = false; // Reset the flag
            // Start recording action
            await MainPageViewModel.StartRecordingCommand.ExecuteAsync(null);
        }

        private async void Border_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            (sender as UIElement)?.ReleasePointerCapture(e.Pointer);
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

        private void WaveformPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            dispatcherQueue.TryEnqueue(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // Handle added items
                        foreach (var newItem in e.NewItems)
                        {
                            var point = (WaveformPoint)newItem; // Assuming WaveformPoint is your data type
                            AddWaveformPointToCanvas(point);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Handle removed items
                        // This requires tracking which UI elements correspond to which data items
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        // Handle replaced items
                        break;
                    case NotifyCollectionChangedAction.Move:
                        // Handle moved items
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        // Handle the collection being cleared
                        WaveformCanvas.Children.Clear();
                        break;
                }
            });
        }

        private void AddWaveformPointToCanvas(WaveformPoint point)
        {
            Rectangle rect = new Rectangle
            {
                Width = 2,
                Height = point.Height,
                Fill = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(rect, point.CanvasLeft);
            Canvas.SetTop(rect, point.CanvasTop);
            WaveformCanvas.Children.Add(rect);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MainPageViewModel.OperationHistory.Clear();
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    dispatcherQueue.TryEnqueue(() => ScrollToBottom());
                    break;
                default:
                    break;
            }
        }

        private async void ScrollToBottom()
        {
            await Task.Delay(100);
            // Assuming 'MessageListScrollViewer' is the x:Name of your ScrollViewer
            MessageListScrollViewer.ChangeView(null, double.MaxValue, null);
        }
    }
}
