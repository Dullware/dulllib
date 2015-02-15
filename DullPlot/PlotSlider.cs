using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Dullware.Plotter
{
    public class PlotSlider : Control
    {
        delegate void FireValueChangedDelegate(object o);
        public System.Threading.Timer timer;
        public System.Windows.Forms.Timer timer1;
        ToolTip tooltip = new ToolTip();

        public PlotSlider()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(50, Color.Crimson);
            Width = 17;
            Cursor = Cursors.Hand;
            tooltip.ShowAlways = true;
            tooltip.AutoPopDelay = int.MaxValue;
            tooltip.ReshowDelay = 0;

            //timer = new System.Threading.Timer(new TimerCallback(FireValueChanged), this, Timeout.Infinite, Timeout.Infinite);
            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 1500;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        void SetMyPosition()
        {
            if (Enabled && Parent is Panel && Parent.Parent is Plotter)
            {
                Location = new Point((int)((Plotter)Parent.Parent).RealX2Window(Value) - Width / 2, 0);
            }
            else if (Enabled) throw new Exception("The slider is not bound to a plotter");
        }

        void SetMyValue()
        {
            if (Parent is Panel && Parent.Parent is Plotter)
            {
                value = ((Plotter)Parent.Parent).WindowX2Real(Location.X + Width / 2);
            }
            else throw new Exception("The slider is not bound to a plotter");
        }

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged(EventArgs e)
        {
            tooltip.SetToolTip(this, InfoString());
            if (ValueChanged != null) ValueChanged(this, e);
        }

        double value;
        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
                {
                    this.value = ((Plotter)Parent.Parent).SnapToGraph.X[GetSnapIndex(((Plotter)Parent.Parent).SnapToGraph, value)];
                }
                else this.value = value;
                Invalidate(); //Nodig om de waarde te updated als de slider niet van plaats verandert.
                SetMyPosition();
            }
        }

        public int Index
        {
            get
            {
                return Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null ? GetSnapIndex(((Plotter)Parent.Parent).SnapToGraph, Value) : -1;
            }

            set
            {
                if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
                {
                    Value = ((Plotter)Parent.Parent).SnapToGraph.X[value];
                }
                else
                {
                    throw new Exception("SnapToGrid not set");
                }
            }
        }

        bool upperlimitenabled;

        public bool UpperLimitEnabled
        {
            get { return upperlimitenabled; }
            set { upperlimitenabled = value; }
        }

        bool lowerlimitenabled;

        public bool LowerLimitEnabled
        {
            get { return lowerlimitenabled; }
            set { lowerlimitenabled = value; }
        }

        double upperlimit;

        public double UpperLimit
        {
            get { return upperlimit; }
            set { upperlimit = value; }
        }

        public int UpperLimitIndex
        {
            get { return Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null ? GetSnapIndex(((Plotter)Parent.Parent).SnapToGraph, upperlimit) : -1; }
            set
            {
                if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
                {
                    upperlimit = ((Plotter)Parent.Parent).SnapToGraph.X[value];
                }
                else
                {
                    throw new Exception("SnapToGrid not set");
                }

            }
        }

        double lowerlimit;

        public double LowerLimit
        {
            get { return lowerlimit; }
            set { lowerlimit = value; }
        }

        public int LowerLimitIndex
        {
            get { return Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null ? GetSnapIndex(((Plotter)Parent.Parent).SnapToGraph, lowerlimit) : -1; }
            set
            {
                if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
                {
                    lowerlimit = ((Plotter)Parent.Parent).SnapToGraph.X[value];
                }
                else
                {
                    throw new Exception("SnapToGrid not set");
                }

            }
        }
        bool dragging;
        Point click;
        double click_value;


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawLine(new Pen(ForeColor), Width / 2, 0, Width / 2, Height / 5 - 5);

            StringFormat sft = StringFormat.GenericTypographic;
            sft.FormatFlags |= StringFormatFlags.DirectionVertical | StringFormatFlags.NoClip;
            SizeF ms = e.Graphics.MeasureString(InfoString(), new Font("Arial", 11), new SizeF(Size), sft);
            e.Graphics.DrawString(InfoString(), new Font("Arial", 11), new SolidBrush(Color.Blue), 0, Height / 5, sft);
            e.Graphics.DrawLine(new Pen(ForeColor), Width / 2, Height / 5 + ms.Height + 5, Width / 2, Height);
            if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
        }

        private string InfoString()
        {
            if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
            {
                return string.Format("[{1}]: {0}", value, GetSnapIndex(((Plotter)Parent.Parent).SnapToGraph, value));
            }
            else return string.Format("{0}", value);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
            BringToFront();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        bool ignoremove;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            ignoremove = true;
            base.OnMouseDown(e);
            Focus();

            click = e.Location;
            click_value = value;
            dragging = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            //BringToFront();
            //Focus();
            dragging = false;
            if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
            {
                DataSet dset = ((Plotter)Parent.Parent).SnapToGraph;
                int idx = GetSnapIndex(dset, value);
                Value = dset.X[idx];
                SetMyPosition();
            }

            if (value != click_value) OnValueChanged(new EventArgs());
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ignoremove && e.Location == click) return; else ignoremove = false;
            base.OnMouseMove(e);
            Point pt = Parent.PointToClient(PointToScreen(e.Location));

            pt = new Point(pt.X - click.X, 0);

            if (dragging && Capture /*&& e.Location != click*/)
            {
                //if (!Focused )Focus();

                if (pt.X < -Width / 2) pt.X = -Width / 2;
                if (pt.X > Parent.Width - Width / 2 - 0) pt.X = Parent.Width - Width / 2 - 0;
                if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
                {
                    DataSet dset = ((Plotter)Parent.Parent).SnapToGraph;
                    int idx = GetSnapIndex(dset, ((Plotter)Parent.Parent).WindowX2Real(pt.X + Width / 2));
                    if (lowerlimitenabled)
                    {
                        if (dset.X[idx] < lowerlimit) idx = GetSnapIndex(dset, lowerlimit);
                        if (dset.X[idx] < lowerlimit) idx++;
                    }
                    if (upperlimitenabled)
                    {
                        if (dset.X[idx] > upperlimit) idx = GetSnapIndex(dset, upperlimit);
                        if (dset.X[idx] > upperlimit) idx--;
                    }
                    Value = dset.X[idx];
                }
                else
                {
                    double val = ((Plotter)Parent.Parent).WindowX2Real(pt.X + Width / 2);
                    if (lowerlimitenabled && val < lowerlimit) Value = lowerlimit;
                    else if (upperlimitenabled && val > upperlimit) Value = upperlimit;
                    else
                    {
                        Location = pt;
                        SetMyValue();
                    }
                }

            }
        }

        static void FireValueChanged(object o)
        {
            PlotSlider me = o as PlotSlider;

            if (!me.InvokeRequired)
            {
                me.OnValueChanged(new EventArgs());
            }
            else //We are on a non GUI thread.
            {
                FireValueChangedDelegate del = new FireValueChangedDelegate(FireValueChanged);
                me.Invoke(del, new object[] { me });
            }
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            OnValueChanged(new EventArgs());
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                    return true;
            }
            return false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Right:
                    Advance(Keys.Right);
                    break;
                case Keys.Left:
                    Advance(Keys.Left);
                    break;
            }
        }

        private void Advance(Keys keys)
        {
            if (Parent is Panel && Parent.Parent is Plotter && ((Plotter)Parent.Parent).SnapToGraph != null)
            {
                DataSet dset = ((Plotter)Parent.Parent).SnapToGraph;
                int idx = GetSnapIndex(dset, value);
                if (keys == Keys.Right && idx < dset.UpperBound && dset.X[idx + 1] <= ((Plotter)Parent.Parent).xMax && (!upperlimitenabled || dset.X[idx + 1] <= upperlimit))
                {
                    Value = dset.X[idx + 1];
                    SetMyPosition();
                    ((Plotter)Parent.Parent).CancelSliderTimers();
                    //timer.Change(1000, Timeout.Infinite);
                    timer1.Start();
                }
                else if (keys == Keys.Left && idx > dset.LowerBound && dset.X[idx - 1] >= ((Plotter)Parent.Parent).xMin && (!lowerlimitenabled || dset.X[idx - 1] >= lowerlimit))
                {
                    Value = dset.X[idx - 1];
                    SetMyPosition();
                    ((Plotter)Parent.Parent).CancelSliderTimers();
                    //timer.Change(1000, Timeout.Infinite);
                    timer1.Start();
                }
            }
            else
            {

                if (keys == Keys.Right && Location.X < Parent.Width - Width / 2 - 0)
                {
                    double val = ((Plotter)Parent.Parent).WindowX2Real(Location.X + 1 + Width / 2);
                    if (upperlimitenabled && val > upperlimit)
                    {
                        if (value != upperlimit)
                        {
                            Value = upperlimit;
                            ((Plotter)Parent.Parent).CancelSliderTimers();
                            //timer.Change(1000, Timeout.Infinite);
                            timer1.Start();
                        }
                    }
                    else
                    {
                        Location += new Size(1, 0);
                        SetMyValue();
                        ((Plotter)Parent.Parent).CancelSliderTimers();
                        //timer.Change(1000, Timeout.Infinite);
                        timer1.Start();
                    }
                }
                else if (keys == Keys.Left && Location.X > -Width / 2)
                {
                    double val = ((Plotter)Parent.Parent).WindowX2Real(Location.X - 1 + Width / 2);
                    if (lowerlimitenabled && val < lowerlimit)
                    {
                        if (value != lowerlimit)
                        {
                            Value = lowerlimit;
                            ((Plotter)Parent.Parent).CancelSliderTimers();
                            //timer.Change(1000, Timeout.Infinite);
                            timer1.Start();
                        }
                    }
                    else
                    {
                        Location -= new Size(1, 0);
                        SetMyValue();
                        ((Plotter)Parent.Parent).CancelSliderTimers();
                        //timer.Change(1000, Timeout.Infinite);
                        timer1.Start();
                    }
                }
            }
        }

        private int GetSnapIndex(DataSet dset, double value)
        {
            int i;
            if (value <= dset.X[dset.LowerBound]) return dset.LowerBound;
            if (value >= dset.X[dset.UpperBound]) return dset.UpperBound;

            for (i = dset.LowerBound; i <= dset.UpperBound && dset.X[i] < value; i++) ;
            if (((Plotter)Parent.Parent).XScale == Scales.Linear)
                return value - dset.X[i - 1] < dset.X[i] - value ? i - 1 : i;
            else if (((Plotter)Parent.Parent).XScale == Scales.Logarithmic)
                return value / dset.X[i - 1] < dset.X[i] / value ? i - 1 : i;
            else throw new Exception("Unknown scale value used");
        }
    }
}
