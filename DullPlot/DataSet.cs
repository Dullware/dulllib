using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Dullware.Plotter
{
    public class DataSet
    {
    	string name = "";
    	
		public string Name {
			get {
				return name;
			}
			set {
    			if ( value == null ) throw new Exception("Name cannot be set to null");
				name = value;
			}
		}
    	
        public event EventHandler DataChanged;

        double[] x, y;

        int lb, ub;

        public double[] X
        {
            get
            {
                return x;
            }

            set
            {
                if (value.Length == y.Length)
                {
                    x = value;
                    OnDataChanged(new EventArgs());
                }
                else
                    throw new Exception("incompatible array");
            }
        }

        public double[] Y
        {
            get
            {
                return y;
            }

            set
            {
                if (value.Length == x.Length)
                {
                    y = value;
                    OnDataChanged(new EventArgs());
                }
                else
                    throw new Exception("incompatible array");
            }
        }

        public int LowerBound
        {
            set
            {
                if (lb != value)
                {
                    lb = value;
                    OnDataChanged(new EventArgs());
                }
            }
            get
            {
                return lb;
            }
        }

        public int UpperBound
        {
            set
            {
                if (ub != value)
                {
                    ub = value;
                    OnDataChanged(new EventArgs());
                }
            }
            get
            {
                return ub;
            }
        }

        public int Length
        {
            get { return ub - lb + 1; }
        }

        public DataSet(string name, double[] x, double[] y)
        {
            if (x.Length != y.Length) throw new Exception("Arrays not of equal size");
            lb = 0; ub = x.Length - 1;
            this.x = x;
            this.y = y;
            if ( name != null ) Name = name;
        }

        public DataSet(string name, int size)
            : this(name, new double[size], new double[size])
        {
        }

        public DataSet(double[] x, double[] y)
            : this(null, x ,y)
        {
        }

        public DataSet(int size)
            : this(null, new double[size], new double[size])
        {
        }

        public void SetZero()
        {
            for (int i = 0; i < x.Length; i++) x[i] = 0;
            for (int i = 0; i < y.Length; i++) y[i] = 0;
        }

        public void TheDataChanged()
        {
            OnDataChanged(new EventArgs());
        }

        protected virtual void OnDataChanged(EventArgs e)
        {
            if (DataChanged != null) DataChanged(this, e);
        }
    }
}
