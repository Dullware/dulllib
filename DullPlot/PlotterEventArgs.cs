using System;
//using System.Windows.Forms;

namespace Dullware.Plotter
{
    public delegate void PlotterEventHandler(Object o, PlotterEventArgs pea);

    public class PlotterEventArgs : EventArgs
    {
        public readonly int width, height;
        public readonly double xmin, xmax;
        public readonly double ymin, ymax;

        PlotterEventArgs(int width, int height, double xmin, double xmax, double ymin, double ymax)
        {
            this.width = width;
            this.height = height;
            this.xmin = xmin;
            this.xmax = xmax;
            this.ymin = ymin;
            this.ymax = ymax;
        }
    }
}
