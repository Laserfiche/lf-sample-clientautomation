// Copyright (c) Laserfiche.

using System;
using System.Collections.Generic;
using Laserfiche.ClientAutomation;

namespace Laserfiche.Samples
{
    class MonitorConsoleApp
    {
        static void Main(string[] args)
        {
            // The main loop.
            while (true)
            {
                try
                {
                    if (MainHandler(args))
                        break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }

                System.Threading.Thread.Sleep(500);
            }
            Console.Write("Done");
        }

        // Sit in a loop and report any new client windows.
        static bool MainHandler(string[] args)
        {
            List<string> lastlines = new List<string>();

            while (true)
            {
                try
                {
                    /*if (lfclient.IsClientOpen())
                    {
                        Console.WriteLine("Open");
                    }
                    else
                    {
                        Console.WriteLine("Closed");
                    }*/

                    List<string> curlines = ReportClientInstances();
                    if (!ListsEqual(lastlines, curlines))
                    {
                        string strNow = DateTime.Now.ToString("hh:mm:ss.fff tt");

                        foreach (string line in curlines)
                        {
                            bool bNew = true;
                            for (int i = 0; i < lastlines.Count; i++)
                            {
                                if (lastlines[i] == line)
                                    bNew = false;
                            }

                            if (bNew)
                                Console.WriteLine(strNow + " " + line);
                        }

                        lastlines = curlines;
                    }
                }
                catch (ClientAutomationException e)
                {
                    System.Console.Out.WriteLine(e.Message);
                    System.Threading.Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    System.Console.Out.WriteLine(e.Message);
                    System.Threading.Thread.Sleep(500);
                }

                System.Threading.Thread.Sleep(200);
            }
        }

        // Find all open clients and print the current folder/selected pages/etc to the console.
        static List<string> ReportClientInstances()
        {
            List<string> lines = new List<string>();
            using (ClientManager lfclient = new ClientManager())
            {
                IEnumerable<ClientInstance> clients = lfclient.GetAllClientInstances();
                foreach (ClientInstance client in clients)
                {
                    IEnumerable<ClientWindow> windows = client.GetAllClientWindows();
                    foreach (ClientWindow window in windows)
                    {
                        if (window == null)
                            continue;

                        string strdetails = "";

                        RepositoryConnection conn = window.GetCurrentRepository();
                        if (conn == null)
                            strdetails += " Not logged in";
                        else
                            strdetails += " (" + conn.UserName + " logged into " + conn.RepositoryName + ")";

                        if (window.GetWindowType() == ClientWindowType.Main)
                        {
                            MainWindow mainwindow = window as MainWindow;
                            int nCurrentFolderID = mainwindow.GetCurrentFolderId();
                            if (nCurrentFolderID > 0)
                                strdetails += " (Folder: " + mainwindow.GetCurrentFolderName() + ", FolderID: " + nCurrentFolderID.ToString() + ")";

                            IList<int> listSelectedEntries = mainwindow.GetSelectedEntries();
                            if (listSelectedEntries.Count > 0)
                            {
                                strdetails += " (Selected entries: ";
                                for (int j = 0; j < listSelectedEntries.Count; j++)
                                {
                                    if (j > 0)
                                        strdetails += ",";
                                    strdetails += listSelectedEntries[j].ToString();
                                }
                                strdetails += ")";
                            }
                        }
                        else if (window.GetWindowType() == ClientWindowType.DocumentViewer)
                        {
                            DocumentViewer docwindow = window as DocumentViewer;

                            int nPages = docwindow.GetPageCount();
                            strdetails += " (Page: " + docwindow.GetCurrentPageNumber().ToString() + "/" + nPages.ToString() + ")";

                            PageSet listSelectedThumbnails = docwindow.GetSelectedThumbnails();
                            if (listSelectedThumbnails.RangeCount > 0)
                            {
                                strdetails += " (Selected pages: " + listSelectedThumbnails.ToString() + ")";
                            }
                        }

                        lines.Add(strdetails);
                    }
                }
            }
            return lines;
        }

        // Find all open clients
        static void FindClientWindows()
        {
            using (ClientManager lfclient = new ClientManager())
            {
                IEnumerable<ClientInstance> clients = lfclient.GetAllClientInstances();
                foreach (ClientInstance client in clients)
                {
                    IEnumerable<ClientWindow> windows = client.GetAllClientWindows();
                    foreach (ClientWindow window in windows)
                    {
                        if (window.GetWindowType() == ClientWindowType.Main)
                        {
                            MainWindow mainwindow = window as MainWindow;
                            Console.WriteLine("Current folder: " + mainwindow.GetCurrentFolderId());
                        }
                        else if (window.GetWindowType() == ClientWindowType.DocumentViewer)
                        {
                            DocumentViewer docwindow = window as DocumentViewer;
                            Console.WriteLine("Current document: " + docwindow.GetDocumentId());
                        }
                    }
                }
            }
        }

        // Find all open clients
        static string FindClientWindows2()
        {
            string strdetails = "";
            using (ClientManager lfclient = new ClientManager())
            {
                IEnumerable<ClientInstance> clients = lfclient.GetAllClientInstances();
                foreach (ClientInstance client in clients)
                {
                    IEnumerable<ClientWindow> windows = client.GetAllClientWindows();
                    foreach (ClientWindow window in windows)
                    {
                        if (strdetails.Length > 0)
                            strdetails += "\r\n";
                        strdetails = window.GetWindowTitle();

                        RepositoryConnection conn = window.GetCurrentRepository();
                        if (conn != null)
                            strdetails += " (" + conn.UserName + " logged into " + conn.RepositoryName + ")";

                        if (window.GetWindowType() == ClientWindowType.Main)
                        {
                            MainWindow mainwindow = window as MainWindow;
                            int nCurrentFolderID = mainwindow.GetCurrentFolderId();
                            if (nCurrentFolderID > 0)
                                strdetails += " (FolderID: " + nCurrentFolderID.ToString() + ")";
                        }
                        else if (window.GetWindowType() == ClientWindowType.DocumentViewer)
                        {
                            DocumentViewer docwindow = window as DocumentViewer;

                            int nPages = docwindow.GetPageCount();
                            strdetails += " (Page: " + docwindow.GetCurrentPageNumber().ToString();
                            strdetails += "/" + nPages.ToString() + ")";
                        }
                    }
                }
            }
            return strdetails;
        }

        // Do the two string lists contain the same thing?
        static bool ListsEqual(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }
    }
}
