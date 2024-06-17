using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UChat.ViewModels
{
    public struct WaveformPoint
    {
        public double Height { get; set; }
        public double CanvasLeft { get; set; }
        public double CanvasTop { get; set; }

        public WaveformPoint(double height, double canvasLeft, double canvasTop)
        {
            Height = height;
            CanvasLeft = canvasLeft;
            CanvasTop = canvasTop;
        }
    }
}
