using System;
using System.Windows.Forms;

namespace Dullware.Library
{
    public delegate void NewFormEventHandler(NewFormEventArgs e);

    public class NewFormEventArgs : EventArgs
    {
        public Form form;
    }
}