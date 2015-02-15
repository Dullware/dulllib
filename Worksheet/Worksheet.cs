using System;
using System.Drawing;
using CarlosAg.ExcelXmlWriter;

namespace Dullware.Library
{
    public class Worksheet
    {
        int xsize;
        public int XSize
        {
            get { return xsize; }
        }

        int ysize;
        public int YSize
        {
            get { return ysize; }
        }

        int xpos = 0;
        public int XPos
        {
            get { return xpos; }
            set { xpos = value; }
        }
        int ypos = 0;
        public int YPos
        {
            get { return ypos; }
            set { ypos = value; }
        }
        string[,] sheet;

        public Worksheet(int xsize, int ysize)
        {
            sheet = new string[xsize, ysize];
        }
        
        void doublex()
        {
        	string[,] newsheet = new string[2*sheet.GetLength(0), sheet.GetLength(1)];
        	for (int i=0; i<xsize; i++) 
        		for (int j=0; j<ysize; j++)
        		newsheet[i,j] = sheet[i,j];
        	sheet = null;
        	sheet = newsheet;
        }
        
        void doubley()
        {
        	string[,] newsheet = new string[sheet.GetLength(0), 2*sheet.GetLength(1)];
        	for (int i=0; i<xsize; i++) 
        		for (int j=0; j<ysize; j++)
        		newsheet[i,j] = sheet[i,j];
        	sheet = null;
        	sheet = newsheet;
        }

        public void Add(string s)
        {
        	if ( xpos + 1 > sheet.GetLength(0) ) doublex();
        	if ( ypos + 1 > sheet.GetLength(1) ) doubley();
            sheet[xpos, ypos] = s;
            if (xpos + 1 > xsize) xsize = xpos + 1;
            if (ypos + 1 > ysize) ysize = ypos + 1;
        }

        public void Add(int dx, int dy, string s)
        {
        	if ( xpos + dx + 1 > sheet.GetLength(0) ) doublex();
        	if ( ypos + dy + 1 > sheet.GetLength(1) ) doubley();
            sheet[xpos + dx, ypos + dy] = s;
            if (xpos + dx + 1 > xsize) xsize = xpos + dx + 1;
            if (ypos + dy + 1 > ysize) ysize = ypos + dy + 1;
        }

        public void Add(int dx, int dy, double d)
        {
        	if ( xpos + dx + 1 > sheet.GetLength(0) ) doublex();
        	if ( ypos + dy + 1 > sheet.GetLength(1) ) doubley();
            sheet[xpos + dx, ypos + dy] = d.ToString();
            if (xpos + dx + 1 > xsize) xsize = xpos + dx + 1;
            if (ypos + dy + 1 > ysize) ysize = ypos + dy + 1;
        }

        public void Add(double[] d, Orientation o)
        {
            if (o == Orientation.Right)
            {
	        	while ( xpos + d.Length > sheet.GetLength(0) ) doublex();
	        	while ( ypos  >= sheet.GetLength(1) ) doubley();
                for (int i = 0; i < d.Length; i++) sheet[xpos + i, ypos] = d[i].ToString();
                if (xpos + d.Length > xsize) xsize = xpos + d.Length;
                if (ypos + 1 > ysize) ysize = ypos + 1;
            }
            else if (o == Orientation.Down)
            {
	        	while ( xpos >= sheet.GetLength(0) ) doublex();
	        	while ( ypos + d.Length > sheet.GetLength(1) ) doubley();
                for (int j = 0; j < d.Length; j++) sheet[xpos, ypos + j] = d[j].ToString();
                if (xpos + 1 > xsize) xsize = xpos + 1;
                if (ypos + d.Length > ysize) ysize = ypos + d.Length;
            }
        }

        public void Add(int dx, int dy, double[] d, Orientation o)
        {
            if (o == Orientation.Right)
            {
	        	while ( xpos + dx + d.Length > sheet.GetLength(0) ) doublex();
	        	while ( ypos + dy >= sheet.GetLength(1) ) doubley();
                for (int i = 0; i < d.Length; i++) sheet[xpos + dx + i, ypos + dy] = d[i].ToString();
                if (xpos + dx + d.Length > xsize) xsize = xpos + dx + d.Length;
                if (ypos + dy + 1 > ysize) ysize = ypos + dy + 1;
            }
            else if (o == Orientation.Down)
            {
	        	while ( xpos + dx >= sheet.GetLength(0) ) doublex();
	        	while ( ypos + dy + d.Length > sheet.GetLength(1) ) doubley();
                for (int j = 0; j < d.Length; j++) sheet[xpos + dx, ypos + dy + j] = d[j].ToString();
                if (xpos + dx + 1 > xsize) xsize = xpos + dx + 1;
                if (ypos + dy + d.Length > ysize) ysize = ypos + dy + d.Length;
            }
        }

        public void SaveInCSV(System.IO.StreamWriter SW)
        {
            for (int j = 0; j < ysize; j++)
            {
                for (int i = 0; i < xsize; i++)
                {
                    SW.Write("{0},", sheet[i, j]);
                }
                SW.WriteLine();
            }
        }
        
        public void SaveAsXmlSheet(CarlosAg.ExcelXmlWriter.Worksheet wsheet)
        {
        	WorksheetRow row;
            for (int j = 0; j < ysize; j++)
            {
            	row = wsheet.Table.Rows.Add();
                for (int i = 0; i < xsize; i++)
                {
            		double d;
            		row.Cells.Add(new WorksheetCell(sheet[i,j],
            		MyTryParse(sheet[i,j],out d) ? DataType.Number : DataType.String));
                }
            }
        }
        
        bool MyTryParse(string s, out double result)
        {
        	return double.TryParse(s,out result) && !double.IsNaN(result);
        }
        
        public void DrawToGraphics(Graphics g,Font font, Brush brush, float xstart, float ystart, float xtab, float ytab)
        {
            for (int j = 0; j < ysize; j++)
            {
                for (int i = 0; i < xsize; i++)
                {
                	g.DrawString(sheet[i,j],font,brush,xstart + i*xtab,ystart + j*ytab);
                }
            }
        }
    }

    public enum Orientation
    {
        Right,
        Down
    }
}
