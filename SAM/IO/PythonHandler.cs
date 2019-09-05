using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using SAM.Interfaces;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using static SAM.IO.RemoteHandler;

namespace SAM
{
    class PythonHandler
    {
        private static string _method;
        private static ProcessStartInfo _pyProcessInfo;

        /// <summary>
        /// Initliazes a process that runs the python server
        /// </summary>
        /// <param name="method"></param>
        /// <returns>returns the process id of the python server</returns>

        public static int InitServer(string method)
        {
            _method = method;

            _pyProcessInfo = new ProcessStartInfo("python");
            _pyProcessInfo.ArgumentList.Add(Program.src_dir + @"\PythonServer.py");
            _pyProcessInfo.ArgumentList.Add(method);
            _pyProcessInfo.UseShellExecute = false; // if we want server to run in a different window
            _pyProcessInfo.RedirectStandardOutput = false; // can be true

            return RunPythonServer();
        }


        /// <summary>
        /// Creates the process and starts it, returns the process id, so process can be identified and killed elsewhere
        /// </summary>
        /// <returns>process id</returns>
        private static int RunPythonServer()
        {
            int id;
            using (var pyProcess = new Process())
            {
                pyProcess.StartInfo = _pyProcessInfo;
                pyProcess.Start();
                AwaitSignal(); // blocks untill signal recieved
                id = pyProcess.Id;
            
            }
            return id;
        }

    }
}
