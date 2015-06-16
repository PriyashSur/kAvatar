using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace kAvatar
{
    class ReconstructMe
    {
        string reme_path = @"C:\Users\PRIYASH_11\Documents\Visual Studio 2013\Projects\kAvatar\ReconstructMe\ReconstructMeApp\Debug\";
        Int64 userID;
        
        public ReconstructMe(Int64 userID)
        {
            Directory.SetCurrentDirectory(reme_path);
            this.userID = userID;
        }

        void executeCommands()
        {
            Process process = new Process();
            process.StartInfo.FileName = "ReconstructMeApp.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.Arguments = Convert.ToString(userID);
            process.Start();
            process.WaitForExit();
        }

        public void Start_Scan()
        {
            executeCommands();
        }
    }
}
