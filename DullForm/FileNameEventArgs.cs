using System;
using System.Windows.Forms;

namespace Dullware.Library
{
    public delegate void FileNameEventHandler(object o, FileNameEventArgs e);

    public class FileNameEventArgs : EventArgs
    {
        private string[] filenames;

        public FileNameEventArgs(string filename)
        {
            filenames = new string[1];
            filenames[0] = filename;
        }

        public FileNameEventArgs(string[] filenames)
        {
            this.filenames = filenames;
        }

        public string FileName
        {
            get { return filenames[0]; }
        }

        public string[] FileNames
        {
            get { return filenames; }
        }

        public int Count
        {
            get { return filenames.Length; }
        }

        bool cancel = false;

        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }
}