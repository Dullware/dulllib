using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyProduct("DullPlot")]
[assembly: System.Reflection.AssemblyTitle("DullPlot by Dullware")]
[assembly: System.Reflection.AssemblyCompany("Dullware")]
[assembly: System.Reflection.AssemblyCopyright("(c) 2006 Dullware")]
[assembly: System.Reflection.AssemblyVersion("0.1.*")]

namespace Dullware.Plotter
{
    public class Plotter : Control
    {
        static Random random = new Random(DateTime.Now.Millisecond);

        const double fracx = 0.10;
        const double fracy = 0.20;
        GraphCollection graphs;
        PlotSliderCollection plotsliders;

        public PlotSliderCollection PlotSliders
        {
            get { return plotsliders; }
        }
        bool autoscalex = true;
        bool autoscaley = true;
        double xmin = double.MaxValue, xmax = double.MinValue;
        double ymin = double.MaxValue, ymax = double.MinValue;

        Scales xscale = Scales.Linear;
        Scales yscale = Scales.Linear;

        AxisTicks xticks;
        AxisTicks yticks;

        Panel plot;

        static PrintDocument printplot = new PrintDocument();

        public Plotter()
        {
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            graphs = new GraphCollection(this);
            plotsliders = new PlotSliderCollection(this);
            BackColor = Color.LightGray;
            TabStop = false;

            plot = new Panel();
            plot.BackColor = SystemColors.Window;
            plot.ForeColor = SystemColors.WindowText;
            plot.Paint += plot_Paint;
            plot.MouseDown += new MouseEventHandler(plot_MouseDown);
            plot.MouseMove += new MouseEventHandler(plot_MouseMove);
            plot.GotFocus += new EventHandler(plot_GotFocus);
            plot.LostFocus += new EventHandler(plot_LostFocus);
            plot.BorderStyle = BorderStyle.None;
            plot.Parent = this;

            Size = new Size(200, 200);

            plot.ContextMenuStrip = new ContextMenuStrip();
            plot.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);
            plot.ContextMenuStrip.Items.Add("Change Line Color...", null, SetColor);
            plot.ContextMenuStrip.Items.Add("Print...", null, Print);

            printplot.DefaultPageSettings.Landscape = true;

        }

        public Plotter(string title)
            : this()
        {
            Name = title;
        }

        void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = Graphs.SelectedGraph == null;
            //e.Cancel = MouseNearGraph(plot.PointToClient(MousePosition)) == null;
            plot.ContextMenuStrip.Items[0].Enabled = Graphs.SelectedGraph != null;
        }

        public void SetColor(object sender, EventArgs e)
        {
            int[] customcolors;
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = true;
            //cd.FullOpen = true;
            cd.AnyColor = true;
            cd.CustomColors = customcolors = Plotter.GetColorsFromRegistry("CustomColors");
            cd.ShowHelp = true;
            if (Graphs.SelectedGraph != null)
                cd.Color = Graphs.SelectedGraph.PenColor;
            else return;

            if (cd.ShowDialog(plot) == DialogResult.OK)
            {
                Graphs.SelectedGraph.PenColor = cd.Color;
                if (customcolors != cd.CustomColors) Plotter.SaveColorsToRegistry("CustomColors", cd.CustomColors);
            }
        }

        public void Print(object sender, EventArgs e)
        {
            printplot.PrintPage += printdocument_PrintPage;
            printplot.DocumentName = Name;
            PrintDialog pdial = new PrintDialog();
            pdial.Document = printplot;
            if (pdial.ShowDialog() == DialogResult.OK) 
            {
                printplot.Print();
                //PrintPreviewDialog ppd = new PrintPreviewDialog();
                //ppd.Document = printplot;
                //ppd.ShowDialog();
                //ppd.Dispose();
            }
            printplot.PrintPage -= printdocument_PrintPage;
            pdial.Dispose();
        }

        void printdocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Point Location = e.MarginBounds.Location;
            int Width = e.MarginBounds.Width;
            int Height = e.MarginBounds.Height;
            Point PlotLocation = Location + new Size((int)(fracx * Width), (int)(fracy * Height));
            float PlotWidth = (float)((1 - 2 * fracx) * Width);
            float PlotHeight = (float)((1 - 2 * fracy) * Height);

            g.DrawRectangle(new Pen(Color.Red), Location.X, Location.Y, Width, Height);

            if (xticks == null) return;

            StringFormat sfmt = new StringFormat();
            sfmt.Alignment = StringAlignment.Center;

            ReScaleXY(null, new SizeF(PlotWidth, PlotHeight));

            int xoffset = Location.X + (int)(fracx * Width);//plot.Location.X - scalex.Location.X;
            int yoffset = Location.Y + (int)((1 - fracy) * Height);
            for (double x = xticks.Min; x <= xticks.Max; x = xscale == Scales.Linear ? x + xticks.Dist : x * xticks.Dist)
            {
                if (true || x >= xmin && x <= xmax)
                {
                    int xwin = xoffset + (int)RealX2Window(x, PlotWidth);
                    g.DrawLine(new Pen(ForeColor), xwin, yoffset + 0, xwin, yoffset + 5);
                    g.DrawString((x / xticks.Exp).ToString(), Font, new SolidBrush(ForeColor), xwin, yoffset + 6, sfmt);
                }
            }
            //e.Graphics.DrawLine(new Pen(ForeColor), xoffset, yoffset, xoffset + plot.Width - 1, yoffset);
            if (xticks.Exp != 1)
            {
                g.DrawString("× 10", Font, new SolidBrush(ForeColor), (float)(xoffset + 3 * PlotWidth / 4), yoffset + 20, sfmt);
                g.DrawString(Math.Log10(xticks.Exp).ToString(), new Font(Font.FontFamily, 3 * Font.Height / 4), new SolidBrush(ForeColor), (float)(xoffset + 3 * PlotWidth / 4 + 18), yoffset + 15, sfmt);
            }

            g.DrawString(CaptionX, Font, new SolidBrush(ForeColor), (float)(xoffset + PlotWidth / 2), (float)(yoffset + fracy * Height / 2), sfmt);

            if (yticks == null) return;

            sfmt = new StringFormat();
            sfmt.Alignment = StringAlignment.Center;

            xoffset = Location.X + (int)(fracx * Width);
            yoffset = Location.Y + (int)(fracy * Height);

            g.DrawString(Caption, Font, new SolidBrush(ForeColor), xoffset + PlotWidth / 2, yoffset / 2, sfmt);

            sfmt.FormatFlags = StringFormatFlags.DirectionVertical;

            for (double y = yticks.Min; y <= yticks.Max; y = yscale == Scales.Linear ? y + yticks.Dist : y * yticks.Dist)
            {
                if (true || y >= ymin && y <= ymax)
                {
                    int ywin = yoffset + (int)RealY2Window(y, PlotHeight);
                    g.DrawLine(new Pen(ForeColor), xoffset - 1, ywin, xoffset - 6, ywin);
                    g.DrawString((y / yticks.Exp).ToString(), Font, new SolidBrush(ForeColor), xoffset - 25, ywin, sfmt);
                }
            }
            //e.Graphics.DrawLine(new Pen(ForeColor), xoffset - 1, yoffset + 1, xoffset - 1, yoffset + plot.Height);
            if (yticks.Exp != 1)
            {
                g.DrawString("× 10", Font, new SolidBrush(ForeColor), xoffset - 45, yoffset + PlotHeight / 4, sfmt);
                g.DrawString(Math.Log10(yticks.Exp).ToString(), new Font(Font.FontFamily, 3 * Font.Height / 4), new SolidBrush(ForeColor), xoffset - 40, yoffset + PlotHeight / 4 + 18, sfmt);
            }

            g.DrawString(CaptionY, Font, new SolidBrush(ForeColor), Location.X + (xoffset - 35 - Location.X) / 2, yoffset + PlotHeight / 4 + 18, sfmt);

            g.DrawLines(new Pen(Color.Blue), new Point[5] { PlotLocation, PlotLocation + new Size(0, (int)(PlotHeight - 1)), PlotLocation + new Size((int)(PlotWidth - 1), (int)(PlotHeight - 1)), PlotLocation + new Size((int)PlotWidth - 1, 0), PlotLocation });

            g.TranslateTransform(PlotLocation.X, PlotLocation.Y);
            foreach (PlotGraph pg in graphs)
                if (!pg.Ignored && pg.DSet != null)
                {
                    g.DrawLines(pg.Pen, pg.DataPoints);
                }

            ReScaleXY(null, plot.Size);
        }

        private static int[] GetColorsFromRegistry(string subkey)
        {
            string regKeyString = string.Format(@"Software\Dullware\{0}\{1}", System.Windows.Forms.Application.ProductName, subkey);
            RegistryKey regapp = Registry.CurrentUser.OpenSubKey(regKeyString);
            if (regapp != null)
            {
                List<int> colors = new List<int>();
                foreach (string valueName in regapp.GetValueNames())
                {
                    if (valueName.StartsWith("Color")) colors.Add((int)regapp.GetValue(valueName));
                }
                regapp.Close();
                return colors.ToArray();
            }
            return null;
        }

        private static void SaveColorsToRegistry(string subkey, int[] p)
        {
            string regKeyString = string.Format(@"Software\Dullware\{0}\{1}", System.Windows.Forms.Application.ProductName, subkey);
            RegistryKey regapp = Registry.CurrentUser.OpenSubKey(regKeyString, true);
            if (regapp == null) regapp = Registry.CurrentUser.CreateSubKey(regKeyString);

            foreach (string valueName in regapp.GetValueNames())
            {
                if (valueName.StartsWith("Color")) regapp.DeleteValue(valueName);
            }

            for (int i = 0; i < p.Length; i++)
            {
                regapp.SetValue(string.Format("Color{0:00}", i), p[i]);
            }

            regapp.Close();
        }

        void scalex_Paint(Graphics g)
        {
            if (xticks == null) return;

            StringFormat sfmt = new StringFormat();
            sfmt.Alignment = StringAlignment.Center;

            int xoffset = (int)(fracx * Width);//plot.Location.X - scalex.Location.X;
            int yoffset = (int)(fracy * Height) + plot.Height;
            for (double x = xticks.Min; x <= xticks.Max; x = xscale == Scales.Linear ? x + xticks.Dist : x * xticks.Dist)
            {
                if (true || x >= xmin && x <= xmax)
                {
                    int xwin = xoffset + (int)RealX2Window(x);
                    g.DrawLine(new Pen(ForeColor), xwin, yoffset + 0, xwin, yoffset + 5);
                    g.DrawString((x / xticks.Exp).ToString(), Font, new SolidBrush(ForeColor), xwin, yoffset + 6, sfmt);
                }
            }
            //e.Graphics.DrawLine(new Pen(ForeColor), xoffset, yoffset, xoffset + plot.Width - 1, yoffset);
            if (xticks.Exp != 1)
            {
                g.DrawString("× 10", Font, new SolidBrush(ForeColor), xoffset + 3 * plot.Width / 4, yoffset + 20, sfmt);
                g.DrawString(Math.Log10(xticks.Exp).ToString(), new Font(Font.FontFamily, 3 * Font.Height / 4), new SolidBrush(ForeColor), xoffset + 3 * plot.Width / 4 + 18, yoffset + 15, sfmt);
            }

            g.DrawString(CaptionX, Font, new SolidBrush(ForeColor), xoffset + plot.Width / 2, (float)(yoffset + fracy * Height / 2), sfmt);
        }

        void scaley_Paint(Graphics g)
        {
            if (yticks == null) return;

            StringFormat sfmt = new StringFormat();
            sfmt.Alignment = StringAlignment.Center;

            int xoffset = (int)(fracx * Width);
            int yoffset = (int)(fracy * Height);

            g.DrawString(Caption, Font, new SolidBrush(ForeColor), xoffset + plot.Width / 2, yoffset / 2, sfmt);

            sfmt.FormatFlags = StringFormatFlags.DirectionVertical;

            for (double y = yticks.Min; y <= yticks.Max; y = yscale == Scales.Linear ? y + yticks.Dist : y * yticks.Dist)
            {
                if (true || y >= ymin && y <= ymax)
                {
                    int ywin = yoffset + (int)RealY2Window(y, plot.Height);
                    g.DrawLine(new Pen(ForeColor), xoffset - 1, ywin, xoffset - 6, ywin);
                    g.DrawString((y / yticks.Exp).ToString(), Font, new SolidBrush(ForeColor), xoffset - 25, ywin, sfmt);
                }
            }
            //e.Graphics.DrawLine(new Pen(ForeColor), xoffset - 1, yoffset + 1, xoffset - 1, yoffset + plot.Height);
            if (yticks.Exp != 1)
            {
                g.DrawString("× 10", Font, new SolidBrush(ForeColor), xoffset - 45, yoffset + plot.Height / 4, sfmt);
                g.DrawString(Math.Log10(yticks.Exp).ToString(), new Font(Font.FontFamily, 3 * Font.Height / 4), new SolidBrush(ForeColor), xoffset - 40, yoffset + plot.Height / 4 + 18, sfmt);
            }

            g.DrawString(CaptionY, Font, new SolidBrush(ForeColor), (xoffset - 35) / 2, yoffset + plot.Height / 4 + 18, sfmt);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            scalex_Paint(e.Graphics);
            scaley_Paint(e.Graphics);
        }

        public event PlotterEventHandler PlotterEvent;
        protected virtual void OnPlotterEvent(PlotterEventArgs pea)
        {
            if (PlotterEvent != null) PlotterEvent(this, pea);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            plot.Location = new Point((int)(fracx * Width), (int)(fracy * Height));
            plot.Size = new Size((int)((1 - 2 * fracx) * Width), (int)((1 - 2 * fracy) * Height));
            foreach (PlotSlider ps in plotsliders)
            {
                ps.Height = plot.Height;
                ps.Value = ps.Value;
            }
            ReScaleXY(null, plot.Size);
            //DSetXY2Graph();

            Invalidate();
        }

        void plot_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLines(new Pen(ContainsFocus ? Color.Red : Color.Blue), new Point[5] { new Point(0, 0), new Point(0, plot.Height - 1), new Point(plot.Width - 1, plot.Height - 1), new Point(plot.Width - 1, 0), new Point(0, 0) });
            PlotGraph selection = null;

            foreach (PlotGraph g in graphs)
                if (!g.Ignored && g.DSet != null)
                {
                    if (g.Selected)
                    {
                        selection = g;
                    }
                    else
                    {
                        e.Graphics.DrawLines(g.Pen, g.DataPoints);
                        g.CreateNewDataPath();
                    }
                }
            if (selection != null) // plot de gelecteerde line er bovenop.
            {
                PlotGraph g = selection;
                e.Graphics.DrawLines(g.Pen, g.DataPoints);
                g.CreateNewDataPath();

                double len = 0;
                double totlen;
                for (int i = 0; i < g.Length - 1; i++)
                {
                    len += Math.Sqrt(Math.Pow(g.DataPoints[i + 1].X - g.DataPoints[i].X, 2) + Math.Pow(g.DataPoints[i + 1].Y - g.DataPoints[i].Y, 2));
                }
                totlen = len; len = 0; int j = 0;
                double interval = Math.Min(100, plot.Width / 8d);
                for (int i = 0; i < g.Length - 1; i++)
                {
                    double dx = g.DataPoints[i + 1].X - g.DataPoints[i].X;
                    double dy = g.DataPoints[i + 1].Y - g.DataPoints[i].Y;
                    double dlen = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                    while (len + dlen >= j * interval)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.Blue), g.DataPoints[i].X + (float)((j * interval - len) / dlen * dx) - 3, g.DataPoints[i].Y + (float)((j * interval - len) / dlen * dy) - 3, 6, 6);
                        j++;
                    }
                    len += dlen;
                }
                e.Graphics.FillRectangle(new SolidBrush(Color.Blue), g.DataPoints[g.Length - 1].X - 3, g.DataPoints[g.Length - 1].Y - 3, 6, 6);
            }
        }

        void plot_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseNearGraph(e.Location) != null) Cursor.Current = Cursors.Hand;
        }
        void plot_MouseDown(object sender, MouseEventArgs e)
        {
            if (!ContainsFocus)
            {
                Focus();
                Invalidate();
                plot.Invalidate();
            }
            if (e.Button == MouseButtons.Right) return;

            PlotGraph g = MouseNearGraph(e.Location);

            foreach (PlotGraph ds in graphs) if (ds != g) ds.Selected = false;

            if (g != null)
            {
                Cursor.Current = Cursors.Hand;
                //g.Selected = true;
                g.Focus();
            }
            else
            {
                //plot.BringToFront();
                plot.Focus();
            }
        }

        PlotGraph MouseNearGraph(Point mouse)
        {
            foreach (PlotGraph g in graphs)
                if (!g.Ignored && g.Datapath != null && g.Datapath.IsVisible(mouse)) return g;

            return null;
        }

        public GraphCollection Graphs
        {
            get { return graphs; }
        }

        public Scales XScale
        {
            set
            {
                if (xscale != value)
                {
                    xscale = value;
                    ReScaleX(null);
                }
            }

            get { return xscale; }
        }

        public Scales YScale
        {
            set
            {
                if (yscale != value)
                {
                    yscale = value;
                    ReScaleY(null);
                }
            }

            get { return yscale; }
        }

        public double xMin
        {
            set
            {
                //if (autoscalex) throw new Exception("It makes no sense to change a bound when AutoScale is in effect.");
                autoscalex = false;
                if (xmin != value)
                {
                    xmin = value;
                    ReScaleX(null);
                }
            }
            get { return xmin; }
        }

        public double xMax
        {
            set
            {
                //if (autoscalex) throw new Exception("It makes no sense to change a bound when AutoScale is in effect.");
                autoscalex = false;
                if (xmax != value)
                {
                    xmax = value;
                    ReScaleX(null);
                }
            }
            get { return xmax; }
        }

        public double yMin
        {
            set
            {
                //if (autoscaley) throw new Exception("It makes no sense to change a bound when AutoScale is in effect.");
                autoscaley = false;
                if (ymin != value)
                {
                    ymin = value;
                    ReScaleY(null);
                }
            }
            get { return ymin; }
        }

        public double yMax
        {
            set
            {
                //if (autoscaley) throw new Exception("It makes no sense to change a bound when AutoScale is in effect.");
                autoscaley = false;
                if (ymax != value)
                {
                    ymax = value;
                    ReScaleY(null);
                }
            }
            get { return ymax; }
        }

        public bool AutoScaleX
        {
            set
            {
                if (autoscalex != value)
                {
                    autoscalex = value;
                    if (autoscalex) ReScaleX(null);
                }
            }
            get { return autoscalex; }
        }

        public bool AutoScaleY
        {
            set
            {
                if (autoscaley != value)
                {
                    autoscaley = value;
                    if (autoscaley) ReScaleY(null);
                }
            }
            get { return autoscaley; }
        }

        bool NewAutoMinMaxX(float width)
        {
            // de vraag is of je ignored Graphs er wel bij moet betrekken.
            double xmaxold = xmax;
            double xminold = xmin;
            if (autoscalex)
            {
                xmax = double.MinValue;
                xmin = double.MaxValue;
                foreach (PlotGraph g in graphs)
                {
                    for (int i = g.LowerBound; i <= g.UpperBound; i++)
                    {
                        if (g.X[i] > xmax) xmax = g.X[i];
                        if (g.X[i] < xmin) xmin = g.X[i];
                    }
                }
            }
            if (xmin == xmax)
            {
                if (xmin != 0)
                {
                    xmin *= 0.9;
                    xmax *= 1.1;
                }
                else
                {
                    xmin = -0.1;
                    xmax = 0.1;
                }
            }
            if (xmin < xmax)
            {
                xticks = new AxisTicks(xmin, xmax, (int)Math.Round(width / 35), xscale);
                xmin = xticks.Min;
                xmax = xticks.Max;
            }
            return xmaxold != xmax || xminold != xmin;
        }

        bool NewAutoMinMaxY(float height)
        {
            double ymaxold = ymax;
            double yminold = ymin;
            if (autoscaley)
            {
                ymax = double.MinValue;
                ymin = double.MaxValue;
                foreach (PlotGraph g in graphs)
                {
                    for (int i = g.LowerBound; i <= g.UpperBound; i++)
                    {
                        if (g.Y[i] > ymax) ymax = g.Y[i];
                        if (g.Y[i] < ymin) ymin = g.Y[i];
                    }
                }
            }
            if (ymin == ymax)
            {
                if (ymin != 0)
                {
                    ymin *= 0.9;
                    ymax *= 1.1;
                }
                else
                {
                    ymin = -0.1;
                    ymax = 0.1;
                }
            }
            if (ymin < ymax)
            {
                yticks = new AxisTicks(ymin, ymax, (int)Math.Round(height / 35), yscale);
                ymin = yticks.Min;
                ymax = yticks.Max;
            }
            return ymaxold != ymax || yminold != ymin;
        }

        void DSetX2Graph(PlotGraph g, float width)
        {
            for (int i = g.LowerBound; i <= g.UpperBound; i++)
            {
                g.DataPoints[i - g.LowerBound].X = RealX2Window(g.X[i], width);
            }
        }

        void DSetY2Graph(PlotGraph g, float height)
        {
            for (int i = g.LowerBound; i <= g.UpperBound; i++)
            {
                g.DataPoints[i - g.LowerBound].Y = RealY2Window(g.Y[i], height);
            }
        }

        void DSetXY2Graph(PlotGraph g, SizeF plotarea)
        {
            for (int i = g.LowerBound; i <= g.UpperBound; i++)
            {
                g.DataPoints[i - g.LowerBound].X = RealX2Window(g.X[i], plotarea.Width);
                g.DataPoints[i - g.LowerBound].Y = RealY2Window(g.Y[i], plotarea.Height);
            }
        }

        void ReScaleX(PlotGraph g)
        {
            if (g == null | NewAutoMinMaxX(plot.Width))
            {
                foreach (PlotGraph ds in graphs)
                {
                    DSetX2Graph(ds, plot.Width);
                }
            }
            else DSetX2Graph(g, plot.Width);
            foreach (PlotSlider ps in plotsliders) ps.Value = ps.Value;
            plot.Invalidate();
            Invalidate();
        }

        void ReScaleY(PlotGraph g)
        {
            if (g == null | NewAutoMinMaxY(plot.Height))
            {
                foreach (PlotGraph ds in graphs)
                {
                    DSetY2Graph(ds, plot.Height);
                }
            }
            else DSetY2Graph(g, plot.Height);
            plot.Invalidate();
            Invalidate();
        }

        void ReScaleXY(PlotGraph g, SizeF plotarea)
        {
            if (g == null | NewAutoMinMaxX(plotarea.Width) | NewAutoMinMaxY(plotarea.Height))
            {
                foreach (PlotGraph pg in graphs)
                {
                    DSetXY2Graph(pg, plotarea);
                }
            }
            else DSetXY2Graph(g, plotarea);
            foreach (PlotSlider ps in plotsliders) ps.Value = ps.Value;
            plot.Invalidate();
            Invalidate();
        }


        void plot_rescaleXY(object o, EventArgs e)
        {
            if (((PlotGraph)o).DSet != null) ReScaleXY((PlotGraph)o, plot.Size);
            else plot.Invalidate();
        }

        void plot_Invalidate(object o, EventArgs e)
        {
            plot.Invalidate();
        }

        List<int> colorpalet = null;

        public List<int> ColorPalet
        {
            get
            {
                if (colorpalet == null)
                {
                    colorpalet = new List<int>();
                    int[] colors = Name != "" ? Plotter.GetColorsFromRegistry(string.Format("Colors from Plotter {0}", Name)) : null;
                    if (colors != null) foreach (int c in colors) colorpalet.Add(c);
                }
                return colorpalet;
            }
        }
        void plot_PenChanged(object sender, EventArgs e)
        {
            PlotGraph g = sender as PlotGraph;
            int idx = Graphs.IndexOf(g);
            if (idx < ColorPalet.Count)
                ColorPalet[idx] = g.PenColor.ToArgb();
            else ColorPalet.Add(g.PenColor.ToArgb());
        }

        public float RealX2Window(double p)
        {
            return RealX2Window(p, plot.Width);
        }

        private float RealX2Window(double p, float width)
        {
            float result = 0;
            if (!double.IsNaN(xmin) && !double.IsNaN(xmax) && xmax > xmin)
            {
                if (xscale == Scales.Linear) result = (float)((p - xmin) / (xmax - xmin) * (width - 1));
                else if (xscale == Scales.Logarithmic) result = (float)((width - 1) * Math.Log(p / xmin) / Math.Log(xmax / xmin));
                else throw new Exception("Unknown scale value used");
                if (float.IsNaN(result)) throw new Exception("Conversion to window coordinates failed. Check your data and min/max values.");
            }
            return result;
        }

        public double WindowX2Real(float p)
        {
            return WindowX2Real(p, plot.Width);
        }

        private double WindowX2Real(float p, float width)
        {
            double result;
            if (xscale == Scales.Linear) result = xmin + p * (xmax - xmin) / (width - 1);
            else if (xscale == Scales.Logarithmic) result = xmin * Math.Pow(xmax / xmin, p / (width - 1));
            else throw new Exception("Unknown scale value used");
            if (double.IsNaN(result)) throw new Exception("Conversion to window coordinates failed. Check your data and min/max values.");
            return result;
        }

        private float RealY2Window(double p, float height)
        {
            float result;
            if (yscale == Scales.Linear) result = (float)((ymax - p) / (ymax - ymin) * (height - 1));
            else if (yscale == Scales.Logarithmic) result = (float)((height - 1) - (height - 1) * Math.Log(p / ymin) / Math.Log(ymax / ymin));
            else throw new Exception("Unknown scale value used");
            if (float.IsNaN(result)) throw new Exception("Conversion to window coordinates failed. Check your data and min/max values.");
            return result;
        }

        private double WindowY2Real(float p, float height)
        {
            double result;
            if (yscale == Scales.Linear) result = ymax - p * (ymax - ymin) / (height - 1);
            else if (yscale == Scales.Logarithmic) result = ymax * Math.Pow(ymin / ymax, p / (height - 1));
            else throw new Exception("Unknown scale value used");
            if (double.IsNaN(result)) throw new Exception("Conversion to window coordinates failed. Check your data and min/max values.");
            return result;
        }

        private float X2Y(PlotGraph ds, float X)
        {
            int i; float Y;
            for (i = 0; i < ds.Length && ds.DataPoints[i].X < X; i++) ;
            if (i > 0 && i < ds.Length)
            {
                Y = ds.DataPoints[i - 1].Y + (X - ds.DataPoints[i - 1].X) / (ds.DataPoints[i].X - ds.DataPoints[i - 1].X) * (ds.DataPoints[i].Y - ds.DataPoints[i - 1].Y);
            }
            else throw new Exception("X out of bounds");
            return Y;
        }

        DataSet snaptoGraph = null;

        public DataSet SnapToGraph
        {
            get { return snaptoGraph; }
            set { snaptoGraph = value; }
        }

        public void CancelSliderTimers()
        {
            foreach (PlotSlider slider in PlotSliders)
            {
                //slider.timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                slider.timer1.Stop();
            }
        }

        public class GraphCollection : ArrayList
        {
            Plotter p;

            public GraphCollection(Plotter p)
            {
                this.p = p;
            }

            public virtual PlotGraph PlotGraphOf(DataSet ds)
            {
                foreach (PlotGraph pg in this) if (pg.DSet == ds) return pg;
                return null;
            }

            public override int IndexOf(object value)
            {
                if (value is DataSet)
                {
                    foreach (PlotGraph pg in this) if (pg.DSet == value) return base.IndexOf(pg);
                    return -1;
                }
                else if (value is PlotGraph) return base.IndexOf(value);
                else throw new Exception("object type not supported");
            }

            public override object this[int index]
            {
                get
                {
                    return base[index];
                }
                set
                {
                    base[index] = value;
                }
            }

            public PlotGraph this[string index]
            {
                get
                {
                    foreach (PlotGraph pg in this) if (pg.Name == index) return pg;
                    return null;
                }
            }

            public override int Add(object o)
            {
                throw new Exception("object type not supported");
            }

            public int Add(PlotGraph pg)
            {
                int idx = -1;
                if (!Contains(pg))
                {
                    idx = base.Add(pg);
                    pg.PenChanged += p.plot_PenChanged;

                    if (pg.Pen == null)
                    {
                        if (idx < p.ColorPalet.Count)
                        {
                            pg.Pen = new Pen(Color.FromArgb(p.ColorPalet[idx]), 2);
                        }
                        else pg.Pen = new Pen(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)), 2);
                    }

                    pg.IgnoredChanged += p.plot_Invalidate;
                    pg.DataSetChanged += p.plot_rescaleXY;
                    pg.PenChanged += p.plot_Invalidate;
                    pg.SelectedChanged += p.plot_Invalidate;
                    pg.Parent = p.plot;

                    p.ReScaleXY(pg, p.plot.Size);
                }
                return idx;
            }

            public int Add(string name, DataSet ds)
            {
                return Add(new PlotGraph(name, ds));
            }

            public int Add(DataSet ds)
            {
                return Add(new PlotGraph(ds));
            }

            public int Add(string name)
            {
                return Add(new PlotGraph(name));
            }

            public override void Remove(object o)
            {
                throw new Exception("object type not supported");
            }

            public void Remove(PlotGraph pg)
            {
                if (Contains(pg))
                {
                    pg.IgnoredChanged -= p.plot_Invalidate;
                    pg.DataSetChanged -= p.plot_rescaleXY;
                    pg.PenChanged -= p.plot_Invalidate;
                    pg.PenChanged -= p.plot_PenChanged;
                    pg.SelectedChanged -= p.plot_Invalidate;
                    pg.Parent = null;
                    base.Remove(pg);
                    p.ReScaleXY(null, p.plot.Size);

                    p.plot.Invalidate();
                    p.Invalidate();
                }
            }

            public void Remove(DataSet ds)
            {
                Remove(PlotGraphOf(ds));
                p.ReScaleXY(null, p.plot.Size);
            }

            public override void Clear()
            {
                base.Clear();
                p.plot.Invalidate();
                p.Invalidate();
            }

            public PlotGraph SelectedGraph
            {
                get
                {
                    foreach (PlotGraph g in this)
                        if (!g.Ignored && g.DSet != null && g.Selected) return g;
                    return null;
                }
            }
        }

        public class PlotSliderCollection : ArrayList
        {
            Plotter p;

            public PlotSliderCollection(Plotter p)
            {
                this.p = p;
            }

            public override int Add(object o)
            {
                throw new Exception("object not of type PlotSlider");
            }

            public int Add(PlotSlider ps)
            {
                int ret;
                ps.Parent = p.plot;
                ret = base.Add(ps);
                return ret;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Name != "") Plotter.SaveColorsToRegistry(string.Format("Colors from Plotter {0}", Name), ColorPalet.ToArray());
        }

        string caption = "";

        public string Caption
        {
            get { return caption; }
            set { caption = value; Invalidate(); }
        }

        string captionx = "";

        public string CaptionX
        {
            get { return captionx; }
            set { captionx = value; Invalidate(); }
        }

        string captiony = "";

        public string CaptionY
        {
            get { return captiony; }
            set { captiony = value; Invalidate(); }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
            plot.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
            plot.Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
        }

        void plot_GotFocus(object sender, EventArgs e)
        {
            Invalidate();
            plot.Invalidate();
        }

        void plot_LostFocus(object sender, EventArgs e)
        {
            Invalidate();
            plot.Invalidate();
        }
    }
}
