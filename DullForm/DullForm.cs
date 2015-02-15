using System;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyProduct("DullForm")]
[assembly: System.Reflection.AssemblyTitle("DullForm by Dullware")]
[assembly: System.Reflection.AssemblyCompany("Dullware")]
[assembly: System.Reflection.AssemblyCopyright("(c) 2006 Dullware")]
[assembly: System.Reflection.AssemblyVersion("0.1.*")]

namespace Dullware.Library
{
    public class DullForm : Form
    {
        static protected event NewFormEventHandler NewFormRequest;

        public event FileNameEventHandler AdditionalFileExtensionDragDrop;

        static bool ExitCanceled;
        static int nuntitled = 1;
        static System.Collections.Generic.List<DullForm> DullFormCollection = new System.Collections.Generic.List<DullForm>();
        static System.Collections.Generic.List<DullForm> DullForms = new System.Collections.Generic.List<DullForm>();

        string untitledprefix = "Document";

        private ToolStripMenuItem mwindow = new ToolStripMenuItem("&Window");
        public ToolStripMenuItem WindowMenu
        {
            get { return mwindow; }
        }

        private ToolStripMenuItem mhelp = new ToolStripMenuItem("&Help");
        public ToolStripMenuItem HelpMenu
        {
            get { return mhelp; }
        }

        private ToolStripMenuItem mClose = new ToolStripMenuItem();
        public ToolStripMenuItem CloseMenu
        {
            get
            {
                return mClose;
            }
        }

        private string initialdirectory = "";
        public string InitialDirectory
        {
            get { return initialdirectory; }
            set { initialdirectory = value; }
        }

        private string[] additionalfiledropextensions;

        public string[] AdditionalFileDropExtensions
        {
            get { return additionalfiledropextensions; }
            set { additionalfiledropextensions = value; }
        }

        public delegate bool StartupCheck();
        static protected void Run(string[] args, StartupCheck check, bool OneDocuemntOnly)
        {
            bool cnew;
            System.Threading.Mutex m = new System.Threading.Mutex(true, "Dullware" + Application.ProductName /*+ Application.ProductVersion*/ + "erawlluD"/*Application.ExecutablePath.Replace(@"\", "/")*/, out cnew);
            if (cnew)
            {
                OneInstance.OpenDocumentRequest += NewDocumentWrapper;
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(OneInstance.Server));
                if (check == null || check()) StartMessageLoop(args != null && args.Length > 0 ? args[0] : null);
                m.ReleaseMutex();
            }
            else if (!OneDocuemntOnly)
            {
                OneInstance.Client(args);
            }
            else
            {
                // activeer de server door er een bericht heen te sturen.
            }
        }

        static protected void Run(string[] args, StartupCheck check)
        {
            Run(args, check, false);
        }

        static void NewDocumentWrapper(object o, FileNameEventArgs e)
        {
            NewDocument(e.FileName);
        }

        static protected void NewDocument(string newdocumentname)
        {
            foreach (DullForm form in DullFormCollection)
            {
                if (form.DocumentName == newdocumentname)
                {
                    form.ActivateDocument();
                    return;
                }
            }

            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(StartMessageLoop));
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start(newdocumentname);
        }

        private delegate void ActivateDocumentDel();
        private void ActivateDocument()
        {
            if (InvokeRequired)
            {
                Invoke(new ActivateDocumentDel(ActivateDocument));
            }
            else
            {
                WindowState = FormWindowState.Normal;
                Activate();
            }
        }

        static protected void StartMessageLoop(object o)
        {
            NewFormEventArgs nfea = new NewFormEventArgs();
            if (NewFormRequest != null) NewFormRequest(nfea);
            else
            {
                throw new Exception("you must catch the NewFormRequest event in the main Form()");
            }
            if (o != null)
            {
                ((DullForm)nfea.form).DocumentName = o as string;
            }
            Application.Run(nfea.form);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = true;
            base.OnLoad(e);
            ToolStripMenuItem it = new ToolStripMenuItem("&About...", null, mHelpAboutClick);
            mhelp.DropDownItems.Add(it);

            if (documentname == null)
            {
                SetUntitled();
            }
            else
            {
                FileNameEventArgs fnea = new FileNameEventArgs(DocumentName);
                OnReadRequest(fnea);
                if (fnea.Cancel)
                {
                    if (DullForms.Count > 1) CloseDocument();
                    else ZombieDocument = true;
                }
            }
        }

        private void SetUntitled()
        {
            ZombieDocument = false;

            documentname = untitledprefix + nuntitled.ToString();
            nuntitled++;
            OnDocumentNameChanged(new EventArgs());
        }

        void mHelpAboutClick(object sender, EventArgs e)
        {
            AboutBox bx = new AboutBox();
            bx.ShowDialog();
        }

        protected DullForm()
        {
            mClose.Image = new System.Drawing.Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DullForm.close.bmp"));
            mClose.ImageTransparentColor = System.Drawing.Color.FromArgb(0, 0, 0);
            mClose.Alignment = ToolStripItemAlignment.Right;
            mClose.ToolTipText = "Close Window";

            mClose.Click += FileClose_Click;

            DullFormCollection.Insert(0, this);
            DullForms.Add(this);

            AllowDrop = true;
        }

        public void FileNew_Click(Object sender, EventArgs e)
        {
            if (!ZombieDocument) NewDocument(null);
            else
            {
                SetUntitled();
            }
        }

        public void FileOpen_Click(Object sender, EventArgs e)
        {
            OpenDocument();
        }

        public void FileClose_Click(Object sender, EventArgs e)
        {
            if (ZombieDocument) return;

            if (DullForms.Count > 1) CloseDocument();
            else
            {
                if (SaveChangedDocument()) ZombieDocument = true;
            }
        }

        bool zombiedocument = false;
        public bool ZombieDocument
        {
            get { return zombiedocument; }
            set
            {
                if (zombiedocument != value)
                {
                    zombiedocument = value;
                    OnZombieDocumentChanged(new EventArgs());
                }
            }
        }
        event EventHandler ZombieDocumentChanged;
        protected virtual void OnZombieDocumentChanged(EventArgs eventArgs)
        {
            if (ZombieDocument)
            {
                untitled = true;
                changed = false;
            }
            mClose.Visible = !ZombieDocument;
            OnDocumentNameChanged(new EventArgs());

            if (ZombieDocumentChanged != null) ZombieDocumentChanged(this, eventArgs);
        }

        public void FileSave_Click(Object sender, EventArgs e)
        {
            if (Changed && !ZombieDocument) SaveDocument();
        }

        public void FileSaveAs_Click(Object sender, EventArgs e)
        {
            if ((Changed || !Untitled) && !ZombieDocument) SaveDocumentAs();
        }

        public void FileExit_Click(Object sender, EventArgs e)
        {
            DullForm[] DullFormArray = new DullForm[DullFormCollection.Count];
            DullFormCollection.CopyTo(DullFormArray);

            ExitCanceled = false;
            foreach (DullForm form in DullFormArray)
            {
                if (!ExitCanceled) form.CloseDocument();
            }
        }

        private delegate void CloseDocumentDel();
        private void CloseDocument()
        {
            if (InvokeRequired)
            {
                Invoke(new CloseDocumentDel(CloseDocument));
            }
            else
            {
                Close();
            }
        }

        public string UntitledPrefix
        {
            get { return untitledprefix; }
            set
            {
                if (untitledprefix != value)
                {
                    untitledprefix = value;
                }
            }
        }

        string filenamefilter;
        public string FileNameFilter
        {
            get
            {
                return filenamefilter;
            }
            set
            {
                filenamefilter = value;
            }
        }

        string filenamedefaultext;
        public string FileNameDefaultExt
        {
            get
            {
                return filenamedefaultext;
            }
            set
            {
                filenamedefaultext = value;
            }
        }

        string documentname = null;
        public string DocumentName
        {
            get { return documentname; }
            set
            {
                if (documentname != value)
                {
                    documentname = value;
                    untitled = false;
                    OnDocumentNameChanged(new EventArgs());
                }
            }
        }

        public string DocumentTitle
        {
            get
            {
                return ZombieDocument ? Application.ProductName : string.Format("{0} - [{1}{2}]", Application.ProductName, DocumentName, Changed ? " *" : "");
            }
        }

        bool untitled = true;
        public bool Untitled
        {
            get { return untitled; }
        }

        bool changed;
        public bool Changed
        {
            get { return changed; }
            set
            {
                if (changed != value)
                {
                    changed = value;
                    OnChangedChanged(new EventArgs());
                }
            }
        }

        private void OpenDocument()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = InitialDirectory;
            ofd.DefaultExt = FileNameDefaultExt;
            ofd.Filter = FileNameFilter;

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                if (Untitled && !Changed)
                {
                    string olddocumentname = DocumentName;
                    bool oldzombiestatus = ZombieDocument;

                    ZombieDocument = false;
                    DocumentName = ofd.FileName;

                    FileNameEventArgs fnea = new FileNameEventArgs(DocumentName);
                    OnReadRequest(fnea);

                    if (fnea.Cancel)
                    {
                        ZombieDocument = oldzombiestatus;
                        DocumentName = olddocumentname;
                        untitled = true;
                        changed = false;
                    }
                    else InitialDirectory = System.IO.Path.GetDirectoryName(ofd.FileName);
                }
                else
                {
                    //Open in new form.
                    NewDocument(ofd.FileName);
                }
            }

            ofd.Dispose();
        }

        private bool SaveDocument()
        {
            if (InvokeRequired)
            {
                return true;
            }
            else
            {
                if (untitled)
                {
                    return SaveDocumentAs();
                }
                FileNameEventArgs fnea = new FileNameEventArgs(DocumentName);
                OnWriteRequest(fnea);
                if (!fnea.Cancel) Changed = false;
                return !fnea.Cancel;
            }
        }

        private bool SaveDocumentAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = InitialDirectory;
            sfd.DefaultExt = FileNameDefaultExt;
            sfd.FileName = DocumentName;
            sfd.Filter = FileNameFilter;

            bool result = sfd.ShowDialog(this) == DialogResult.OK;

            if (result)
            {
                FileNameEventArgs fnea = new FileNameEventArgs(sfd.FileName);
                OnWriteRequest(fnea);
                if (!fnea.Cancel)
                {
                    DocumentName = sfd.FileName;
                    untitled = Changed = false;
                    InitialDirectory = System.IO.Path.GetDirectoryName(sfd.FileName);
                }
                result = !fnea.Cancel;
            }

            sfd.Dispose();
            return result;
        }

        protected virtual void OnDocumentNameChanged(EventArgs e)
        {
            Text = DocumentTitle;
            //if (DocumentNameChanged != null) DocumentNameChanged(this, e);

            PopulateWindowMenu();
        }

        private void PopulateWindowMenu()
        {
            foreach (DullForm df in DullForms)
            {
                df.ClearWindowMenu();
                foreach (DullForm form in DullForms)
                {
                    df.AddToWindowMenu(form.DocumentName, df == form);
                }
            }

            if (DullForms.Count == 0)
            {
                string regKeyString = string.Format(@"Software\Dullware\{0}", System.Windows.Forms.Application.ProductName);
                Microsoft.Win32.RegistryKey regapp = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(regKeyString, true);
                try
                {
                    if (regapp != null) regapp.DeleteValue("Startup");
                }
                catch { };
            }

            mwindow.Enabled = !(DullForms.Count == 1 && DullForms[0].ZombieDocument);
        }

        private delegate void ClearWindowMenuDel();
        private void ClearWindowMenu()
        {
            if (InvokeRequired)
            {
                Invoke(new ClearWindowMenuDel(ClearWindowMenu));
            }
            else
            {
                mwindow.DropDownItems.Clear();
            }
        }

        private delegate void AddToWindowMenuDel(string s, bool c);
        private void AddToWindowMenu(string s, bool c)
        {
            if (InvokeRequired)
            {
                Invoke(new AddToWindowMenuDel(AddToWindowMenu), new object[] { s, c });
            }
            else
            {
                ToolStripMenuItem it = new ToolStripMenuItem(s);
                it.Click += WindowItem_Click;
                it.Checked = c;
                mwindow.DropDownItems.Add(it);

            }
        }

        private void WindowItem_Click(Object sender, EventArgs e)
        {
            ToolStripMenuItem it = (ToolStripMenuItem)sender;
            DullForms[mwindow.DropDownItems.IndexOf(it)].ActivateDocument();
        }

        protected virtual void OnChangedChanged(EventArgs e)
        {
            Text = DocumentTitle;
            //if (ChangedChanged != null) ChangedChanged(this, e);
        }

        public event FileNameEventHandler ReadRequest;

        protected virtual void OnReadRequest(FileNameEventArgs e)
        {
            ZombieDocument = false;
            if (ReadRequest != null) ReadRequest(this, e);
        }

        public event FileNameEventHandler WriteRequest;

        protected virtual void OnWriteRequest(FileNameEventArgs e)
        {
            if (WriteRequest != null) WriteRequest(this, e);
        }

        bool SaveChangedDocument()
        {
            // Probeert een veranderde file te saven in response op een Close event
            // Geeft true als de file gesaved wordt (mogelijk onder een andere naam), of saven niet nodig is, of saven niet nodig wordt gevonden
            // Geeft false als de Close-operatie gecanceled moet worden: er is dan niets gesaved.
            bool cancel = false;
            if (changed)
            {
                DialogResult dres;
                dres = MessageBox.Show(this, "Do you want to save the changes to " + DocumentName + "?", Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                switch (dres)
                {
                    case DialogResult.Yes:
                        if (!SaveDocument()) cancel = true;
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        cancel = true;
                        break;
                }
            }
            return !cancel;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            ExitCanceled = e.Cancel = !SaveChangedDocument();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            DullFormCollection.Remove(this);
            DullForms.Remove(this);

            PopulateWindowMenu();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            DullFormCollection.Remove(this);
            DullFormCollection.Insert(0, this);
        }

        public bool AllowedAdditionalExension(string p)
        {
            if (additionalfiledropextensions != null)
            {
                foreach (string ext in additionalfiledropextensions)
                {
                    if (ext.ToLower() == p.ToLower()) return true;
                }
            }
            return false;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop) && (drgevent.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (fi.Extension.Substring(1).ToLower() != filenamedefaultext && !AllowedAdditionalExension(fi.Extension)) return;
                }
                drgevent.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);
                System.Collections.Generic.List<string> additionalfiles = new System.Collections.Generic.List<string>();
                foreach (string file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (fi.Extension.Substring(1).ToLower() == filenamedefaultext)
                    {
                        if (Untitled && !Changed)
                        {
                            string olddocumentname = DocumentName;
                            bool oldzombiestatus = ZombieDocument;

                            ZombieDocument = false;
                            DocumentName = file;

                            FileNameEventArgs fnea = new FileNameEventArgs(DocumentName);
                            OnReadRequest(fnea);

                            if (fnea.Cancel)
                            {
                                ZombieDocument = oldzombiestatus;
                                DocumentName = olddocumentname;
                                untitled = true;
                                changed = false;
                            }
                        }
                        else
                        {
                            //Open in new form.
                            NewDocument(file);
                        }
                    }
                    else //Additional filedrop extension
                    {
                        additionalfiles.Add(file);
                    }
                }

                if (additionalfiles.Count > 0)
                {
                    OnAdditionalFileExtensionDragDrop(new FileNameEventArgs(additionalfiles.ToArray()));
                    additionalfiles.Clear();
                }
            }
        }

        protected virtual void OnAdditionalFileExtensionDragDrop(FileNameEventArgs fileNameEventArgs)
        {
            if (AdditionalFileExtensionDragDrop != null) AdditionalFileExtensionDragDrop(this, fileNameEventArgs);
        }
    }
}
