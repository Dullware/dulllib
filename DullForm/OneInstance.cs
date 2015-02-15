using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;

namespace Dullware.Library
{
    public class OneInstance
    {
        static public event FileNameEventHandler OpenDocumentRequest;

        private static void OnOpenDocumentRequest(FileNameEventArgs e)
        {
            if (OpenDocumentRequest != null) OpenDocumentRequest(null, e);
        }

        static public void Server(object o)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Loopback, 0);
            server.Bind(ipep);
            WriteToRegistry(((IPEndPoint)server.LocalEndPoint).Port);
            //server.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.MaxConnections);
            try {
            	server.Listen(512);
            }
            catch {
            	System.Windows.Forms.MessageBox.Show("The OneInstance service cannot be set up.\r\n\r\nIs your firewall allowing connections to 127.0.0.1?", System.Windows.Forms.Application.ProductName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
            	//System.ComponentModel.CancelEventArgs args = new System.ComponentModel.CancelEventArgs();
            	//args.Cancel = true;
            	//System.Windows.Forms.Application.Exit(args);
            }
            while (true)
            {
                string s;
                Socket client = server.Accept();
                NetworkStream ns = new NetworkStream(client);
                StreamReader sr = new StreamReader(ns);
                while ((s = sr.ReadLine()) != null)
                {
                    OnOpenDocumentRequest(new FileNameEventArgs(s == "" ? null : s));
                }
                sr.Close();
                ns.Close();
            }
        }

        static public void Client(string[] args)
        {
            int port;
            while ((port = ReadFromRegistry()) == 0) System.Threading.Thread.Sleep(100);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Loopback, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    server.Connect(ipep);
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(50);
                }
            }
            NetworkStream ns = new NetworkStream(server);
            StreamWriter sw = new StreamWriter(ns); sw.AutoFlush = true;

            if (args == null || args.Length == 0) sw.WriteLine("");
            else foreach (string s in args) sw.WriteLine(s);

            sw.Close();
            ns.Close();
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        private static void WriteToRegistry(int port)
        {
            string regKeyString = string.Format(@"Software\Dullware\{0}", System.Windows.Forms.Application.ProductName);
            RegistryKey regapp = Registry.CurrentUser.OpenSubKey(regKeyString, true);
            if (regapp == null) regapp = Registry.CurrentUser.CreateSubKey(regKeyString);
            regapp.SetValue("Startup", port, RegistryValueKind.DWord);
        }

        private static int ReadFromRegistry()
        {
            string regKeyString = string.Format(@"Software\Dullware\{0}", System.Windows.Forms.Application.ProductName);
            RegistryKey regapp = Registry.CurrentUser.OpenSubKey(regKeyString, true);
            if (regapp == null) return 0;
            object o = regapp.GetValue("Startup", 0);
            return o == null ? 0 : (int)o;
        }
    }
}
