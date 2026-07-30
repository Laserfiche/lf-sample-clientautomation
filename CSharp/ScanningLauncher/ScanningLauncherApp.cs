// Copyright (c) Laserfiche.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Laserfiche.ClientAutomation;

namespace Laserfiche.Samples
{
    static class ScanningLauncherApp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string strCommands = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                    strCommands += " ";
                strCommands += args[i];
            }

            while (true)
            {
                try
                {
                    if (MainHandler(args))
                        break;
                }
                catch (Exception e)
                {
                    DialogResult result = MessageBox.Show(e.Message + ", Retry?", "Error", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                        break;
                }
            }
        }

        static bool MainHandler(string[] _args)
        {
            string servername = "";
            string repository = "";
            string serializedconn = "";
            string username = "";
            string password = "";
            bool usessl = false;
            int folderid = 0;
            int documentid = 0;
            int insertat = -1; // -3 = Default, -2 = Ask, -1 = End, 0 = Beginning
            bool waitforclose = false;
            bool closeafterstoring = false;
            ScanMode scanmode = ScanMode.Standard;

            List<string> args = new List<string>();

            for (int i = 0; i < _args.Length; i++)
            {
                args.Add(_args[i]);
            }

            for (int i = 0; i < args.Count; i++)
            {
                try
                {
                    if (args[i] == "-server" && args.Count > i + 1)
                    {
                        servername = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-repository" && args.Count > i + 1)
                    {
                        repository = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-ssl")
                        usessl = true;
                    else if (args[i] == "-username" && args.Count > i + 1)
                    {
                        username = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-password" && args.Count > i + 1)
                    {
                        password = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-serializedconn" && args.Count > i + 1)
                    {
                        serializedconn = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-folder" && args.Count > i + 1)
                    {
                        folderid = int.Parse(args[i + 1]);
                        i++;
                    }
                    else if (args[i] == "-document" && args.Count > i + 1)
                    {
                        documentid = int.Parse(args[i + 1]);
                        i++;
                    }
                    else if (args[i] == "-insertat" && args.Count > i + 1)
                    {
                        insertat = int.Parse(args[i + 1]);
                        i++;
                    }
                    else if (args[i] == "-waitforclose")
                        waitforclose = true;
                    else if (args[i] == "-closeafterstoring")
                        closeafterstoring = true;
                    else if (args[i] == "-mode" && args.Count > i + 1)
                    {
                        if (args[i + 1] == "basic")
                            scanmode = ScanMode.Basic;
                        else if (args[i + 1] == "standard")
                            scanmode = ScanMode.Standard;
                        else if (args[i + 1] == "default")
                            scanmode = ScanMode.Default;
                        else
                        {
                            MessageBox.Show("Invalid scan mode '" + args[i + 1] + "'");
                            return true;
                        }

                        i++;
                    }
                    else
                    {
                        MessageBox.Show("Invalid option '" + args[i] + "'");
                        return true;
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            if (args.Count > 0)
            {
                using (ClientManager lfclient = new ClientManager())
                {
                    ScanOptions scanoptions = new ScanOptions();
                    scanoptions.ConnectionString = serializedconn;
                    scanoptions.RepositoryName = repository;
                    scanoptions.ServerName = servername;
                    scanoptions.UserName = username;
                    scanoptions.Password = password;
                    scanoptions.IsSecureConnection = usessl;
                    scanoptions.WaitForExit = waitforclose;
                    scanoptions.CloseAfterStoring = closeafterstoring;
                    scanoptions.ScanMode = scanmode;
                    if (documentid > 0)
                    {
                        scanoptions.EntryId = documentid;
                        scanoptions.IsDocument = true;
                        scanoptions.InsertPagesAt = insertat;
                    }
                    else if (folderid > 0)
                    {
                        scanoptions.EntryId = folderid;
                        scanoptions.IsDocument = false;
                    }
                    else
                    {
                        MessageBox.Show("No document/folder specified");
                        return true;
                    }
                    lfclient.LaunchScanning(scanoptions);
                }
            }
            else
            {
                MessageBox.Show("No arguments specified");
                return true;
            }
            return true;
        }
    }
}
