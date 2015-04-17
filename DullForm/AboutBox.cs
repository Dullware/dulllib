using System;
using System.Drawing;
using System.Windows.Forms;

namespace Dullware.Library
{
    public class AboutBox : Form
    {
        public AboutBox()
        {
            int xpos = 180;
            int ypos = 10;
            Icon = ActiveForm.Icon;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(390, 200);

            //BackColor = Color.FromArgb(204, 51, 51);
            BackColor = Color.FromArgb(255, 204, 153);
            //ForeColor = Color.White;


            Button btn = new Button();
            btn.Parent = this;
            btn.Text = "OK";
            btn.Location = new Point(ClientSize.Width - btn.Width - 20, ClientSize.Height - btn.Height - 20);
            btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btn.DialogResult = DialogResult.OK;
            AcceptButton = btn;

            System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

            Label lb;
            lb = new Label();
            lb.Parent = this;
            lb.AutoSize = true;
            lb.Location = new Point(xpos, ypos);
            lb.Text = info.ProductName;

            lb = new Label();
            lb.Parent = this;
            lb.AutoSize = true;
            ypos += 20;
            lb.Location = new Point(xpos, ypos);
            lb.Text = string.Format("{0}", info.Comments);

            lb = new Label();
            lb.Parent = this;
            lb.AutoSize = true;
            ypos += 20;
            lb.Location = new Point(xpos, ypos);
            lb.Text = string.Format("{0}.{1} (Build {2})", info.ProductMajorPart, info.ProductMinorPart, info.ProductBuildPart);

            lb = new Label();
            lb.Parent = this;
            lb.AutoSize = true;
            ypos += 20;
            lb.Location = new Point(xpos, ypos);
            lb.Text = info.LegalCopyright;
            
            lb = new Label();
            lb.Parent = this;
            lb.AutoSize = true;
            xpos -= 150;
            ypos += 50;
            lb.Location = new Point(xpos, ypos);
            lb.Text = "For updates check:";

            TextBox tb;
            tb = new TextBox();
            tb.Parent = this;
            tb.Width = 140;
            ypos += 20;
            tb.Location = new Point(xpos, ypos);
            tb.ReadOnly = true;
            tb.BorderStyle = 0;
            tb.Text = " https://github.com/Dullware";

            PictureBox pxbox = new PictureBox();
            pxbox.Parent = this;
            pxbox.SizeMode = PictureBoxSizeMode.AutoSize;
            pxbox.Image = new Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DullForm.DullwareLogo0.bmp"));
        }
    }
}
