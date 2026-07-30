// Copyright (c) Laserfiche.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Laserfiche.ClientAutomation;

namespace Laserfiche.Samples
{
    static class ClientLauncherApp
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
            int folderid = 0;
            int documentid = 0;
            bool waitforclose = false;
            string searchphrase = "";
            bool opensingleresult = false;

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
                    else if (args[i] == "-search" && args.Count > i + 1)
                    {
                        searchphrase = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-waitforclose")
                        waitforclose = true;
                    else if (args[i] == "-openifone")
                        opensingleresult = true;
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

            if (args.Count == 0)
            {
                MessageBox.Show("No arguments specified");
                return true;
            }

            // Create the ClientManager object which allows us to launch the client.
            using (ClientManager clientmgr = new ClientManager())
            {
                LaunchOptions launchoptions = new LaunchOptions();
                launchoptions.RepositoryName = repository;
                launchoptions.ServerName = servername;
                launchoptions.UserName = username;
                launchoptions.Password = password;
                if (folderid > 0)
                {
                    launchoptions.InitialFolderId = folderid;
                }
                else if (String.IsNullOrEmpty(searchphrase) && documentid == 0)
                {
                    MessageBox.Show("No document/folder/search specified");
                    return true;
                }

                // Launch the client and store the process ID.
                ClientInstance client = clientmgr.LaunchClient(launchoptions);
                int pid = client.ProcessID;

                MainWindow mainwindow = null;

                // Find the main window.
                IEnumerable<ClientWindow> windows = client.GetAllClientWindows();
                foreach (ClientWindow window in windows)
                {
                    if (window.GetWindowType() == ClientWindowType.Main)
                    {
                        mainwindow = (MainWindow)window;
                        break;
                    }
                }

                // Launch the search specified on the command line
                if (!string.IsNullOrEmpty(searchphrase))
                {
                    SearchOptions searchoptions = new SearchOptions();
                    searchoptions.NewWindow = false;
                    searchoptions.OpenIfOneResult = opensingleresult;
                    searchoptions.EagerlyRetrieveResults = false; // Don't return the results since we don't care about them
                    searchoptions.Query = searchphrase;
                    mainwindow.LaunchSearch(searchoptions);
                }
                // Open the document specified on the command line
                else if (documentid > 0)
                {
                    OpenOptions openoptions = new OpenOptions();
                    openoptions.OpenStyle = DocumentOpenType.DocumentViewer;
                    openoptions.VisiblePanes = DocViewerPane.ImagePane | DocViewerPane.MetadataPane;
                    openoptions.MetadataVisibleTabs = MetadataTab.Fields | MetadataTab.Signatures;
                    mainwindow.OpenDocumentById(documentid, openoptions);
                }

                // If -waitforclose was specified, block until the client exits.
                if (waitforclose)
                {
                    while (true)
                    {
                        bool isrunning = false;

                        try
                        {
                            System.Diagnostics.Process.GetProcessById(pid);
                            isrunning = true;
                        }
                        catch (Exception)
                        {
                        }

                        if (!isrunning)
                            break;

                        System.Threading.Thread.Sleep(200);
                    }
                }
            }
            
            return true;
        }
    }
}
