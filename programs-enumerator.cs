using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace MadSpy
{
    class ProgramsEnumerator
    {
        public static bool IsRunning(String process)
        {
            // Get all instances of process running on the local computer.
            // This will return an empty array if notepad isn't running.
            Process[] localByName = Process.GetProcessesByName(process);

            if (localByName != null && localByName.Length > 0)
                return true;

            return false;         
        }
    }
}
