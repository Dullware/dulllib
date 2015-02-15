using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dullware.Plotter
{
    public class PlotGraph : Control
    {
        public event EventHandler IgnoredChanged;
        public event EventHandler SelectedChanged;
        public event EventHandler PenChanged;
        public event EventHandler DataSetChanged;

        DataSet thedata;

        public DataSet DSet
        {
            get { return thedata; }
            set
            {
                if (thedata != value)
                {
                    if (thedata != null) thedata.DataChanged -= OnDataSetChanged;
                    thedata = value;
                    if (thedata != null)
                    {
                        thedata.DataChanged += OnDataSetChanged;
                    }
                    OnDataSetChanged(this, new EventArgs());
                }
            }
        }
        PointF[] datapoints;
        GraphicsPath datapath;

        bool ignored;
        bool selected;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    OnSelectedChanged(new EventArgs());
                }
            }
        }

        Pen pen = null;
        public PointF[] DataPoints
        {
            get
            {
                return datapoints;
            }
        }

        public GraphicsPath Datapath
        {
            get
            {
                return datapath;
            }
        }

        public bool Ignored
        {
            set
            {
                if (ignored != value)
                {
                    ignored = value;
                    Visible = !ignored;
                    OnIgnoredChanged(new EventArgs());
                }
            }
            get { return ignored; }
        }

        public Pen Pen
        {
            set
            {
                pen = value;
                OnPenChanged(new EventArgs());
            }
            get { return pen; }
        }

        public Color PenColor
        {
            set
            {
                pen.Color = value;
                OnPenChanged(new EventArgs());
            }
            get { return pen.Color; }
        }

        public float PenWidth
        {
            set
            {
                pen.Width = value;
                OnPenChanged(new EventArgs());
            }
            get { return pen.Width; }
        }

        public PlotGraph(string name, int size)
            : this(name, new double[size], new double[size])
        {
        }

        public PlotGraph(string name, double[] x, double[] y)
            : this(name, new DataSet(x, y))
        {
        }

        public PlotGraph(string name, DataSet dset)
        {
            Name = name;
            DSet = dset;
        }

        public PlotGraph(string name) { Name = name; }

        public PlotGraph(DataSet dset)
        {
        	if (dset.Name != "") Name = dset.Name;
            DSet = dset;
        }

        public PlotGraph() { }

        public void CreateNewDataPath()
        {
            Pen pen = new Pen(Color.Black, 10);
            if (datapath != null)
            {
                datapath.Dispose();
                datapath = null;
            }
            datapath = new GraphicsPath();
            datapath.AddLines(datapoints);
            datapath.Widen(pen);
            pen.Dispose();
        }

        public void SetZero()
        {
            for (int i = 0; i < X.Length; i++) X[i] = 0;
            for (int i = 0; i < Y.Length; i++) Y[i] = 0;
        }

        protected virtual void OnIgnoredChanged(EventArgs e)
        {
            if (IgnoredChanged != null) IgnoredChanged(this, e);
        }

        protected virtual void OnSelectedChanged(EventArgs e)
        {
            if (SelectedChanged != null) SelectedChanged(this, e);
        }

        protected virtual void OnPenChanged(EventArgs e)
        {
            if (PenChanged != null) PenChanged(this, e);
        }

        protected virtual void OnDataSetChanged(object o, EventArgs e)
        {
            datapoints = null;
            datapoints = new PointF[Length];
            if (DataSetChanged != null) DataSetChanged(this, e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            //Console.WriteLine("Got");
            Selected = true;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            //Console.WriteLine("Lost");
            Selected = false;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // disable left/right arrow navigation (interferes with plotsliders)
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                    return true;
            }
            return false;
        }

        public double[] X
        {
            get { return thedata.X; }
        }

        public double[] Y
        {
            get { return thedata.Y; }
        }

        public int Length
        {
            get { return thedata == null ? 0 : thedata.Length; }
        }

        public int LowerBound
        {
            get { return thedata == null ? 1 : thedata.LowerBound; }
        }

        public int UpperBound
        {
            get { return thedata == null ? -1 : thedata.UpperBound; }
        }
    }
}
