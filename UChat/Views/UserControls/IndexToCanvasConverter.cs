using Microsoft.UI.Xaml.Data;
using System;

namespace UChat
{
    public class IndexToCanvasLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Assuming value is the index and parameter is the total count
            int index = (int)value;
            int totalCount = (int)parameter;
            double canvasWidth = 400; // Assume a fixed canvas width or bind this as needed
            return (canvasWidth / totalCount) * index;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
