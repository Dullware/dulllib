using System;

namespace Dullware.Plotter
{
    public class AxisTicks
    {
        double del = 2e-5;
        double xminp;

        public double Min
        {
            get { return xminp; }
        }
        double xmaxp;

        public double Max
        {
            get { return xmaxp; }
        }
        double dist;

        public double Dist
        {
            get 
            {
                if (dist == 0) return Max - Min + del;
                else return dist; 
            }
        }

        double exp;

        public double Exp
        {
            get { return exp; }
        }

        public AxisTicks(double min, double max, int n_intervals, Scales scale)
        {
            if (scale == Scales.Linear) Scale2(min, max, n_intervals);
            else if (scale == Scales.Logarithmic) Scale4(min, max, n_intervals);
        }

        void Scale2(double min, double max, int n_intervals)
        {
            //Console.WriteLine(n_intervals);
            if (n_intervals < 2) n_intervals = 2; // eis van het algoritme
            int i,np;
            double[] vint = new double[5] { 1, 2, 5, 10, 20 };
            double a = (max - min) / n_intervals;
            double al = Math.Log10(a);
            int nal = (int)Math.Floor(al);
            exp = Math.Pow(10, nal);
            double b = a / exp;
            for (i = 0; i < 3; i++) if (b < vint[i] + del) break;
            do
            {
                dist = vint[i] * exp;
                double fm1 = min / dist;
                int m1 = (int)Math.Floor(fm1);
                if (Math.Abs(m1 + 1 - fm1) < del) m1++;
                xminp = dist * m1;
                double fm2 = max / dist;
                int m2 = (int)Math.Floor(fm2 + 1);
                if (Math.Abs(fm2 + 1 - m2) < del) m2--;
                xmaxp = dist * m2;
                np = m2 - m1;
                i++;
            } while (np > n_intervals);
            if (xminp > min) xminp = min;
            if (xmaxp < max) xmaxp = max;
            xmaxp += del;
            exp = 1;
        }

        void Scale3(double min, double max, int n_intervals)
        {
            int i, np;
            double[] vint = new double[11] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0.5 };
            double minl = Math.Log10(min);
            double maxl = Math.Log10(max);
            //double fn = n_intervals;
            double a = (maxl - minl) / n_intervals;
            double al = Math.Log10(a);
            int nal = (int)Math.Floor(al);
            exp = Math.Pow(10, nal);
            double b = a / exp;
            for (i = 0; i < 9 && b < 10 / vint[i] + del; i++) ;
            do
            {
                dist = Math.Pow(10, nal + 1) / vint[i];
                double fm1 = minl / dist;
                int m1 = (int)Math.Floor(fm1);
                //if (Math.Abs(m1 + 1 - fm1) < del) m1++;
                xminp = dist * m1;
                double fm2 = maxl / dist;
                int m2 = (int)Math.Floor(fm2 + 0);
                //if (Math.Abs(fm2 + 1 - m2) < del) m2--;
                xmaxp = dist * m2;
                np = m2 - m1;
                i++;
            } while (np > n_intervals);
            int nx = (n_intervals - np) / 2;
            //xminp -= nx * dist;
            //xmaxp += n_intervals * dist;
            dist = Math.Pow(10, dist);
            xminp = Math.Pow(10, xminp);
            xmaxp = Math.Pow(10, xmaxp);
            if (xminp > min) xminp = min;
            if (xmaxp < max) xmaxp = max;
        }

        void Scale4(double min, double max, int n_intervals)
        {
            if (n_intervals == 0) n_intervals = 1;
            xminp = Math.Pow(10, Math.Floor(Math.Log10(min)));
            xmaxp = Math.Pow(10, Math.Ceiling(Math.Log10(max)));
            dist = 10;
            while (Math.Log10(xmaxp / xminp) / Math.Log10(dist) > n_intervals) dist *= 10;
            xmaxp = xminp * Math.Pow(dist, Math.Ceiling(Math.Log10(xmaxp / xminp) / Math.Log10(dist)));

            exp = 1;
       }
    }
}
