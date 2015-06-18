using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace kAvatar
{
    class HeadStitch
    {
        string headStitchPath = @"C:\InkarneInternal\kAvatar\Headstiching\";
        string pbOBJInputPath = @"C:\InkarneInternal\kAvatar\PB\";
        string pbOBJOutputPath = @"C:\InkarneInternal\kAvatar\Headstiching\Interimfolder\PBobj\ ";
        string userOBJInputPath = @"C:\InkarneInternal\kAvatar\Processingfolder\";
        string userOBJOutputPath = @"C:\InkarneInternal\kAvatar\Headstiching\Interimfolder\RawAvatarfiles\ ";
        string TargetPath = @"C:\InkarneInternal\kAvatar\Headstiching\Interimfolder\CustomAvatarfiles\ ";
    
        Int64 userID;
        float h1;
        float h2;
        string PBID;
        public HeadStitch(float h1, float h2, Int64 userID, string PBID)
        {
            Directory.SetCurrentDirectory(headStitchPath);
            this.userID = userID;
            this.h1 = h1;
            this.h2 = h2;
            this.PBID = PBID;
        }
        void executeCommands(string fileName,string args)
        {

            /*ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + command);
            ProcessInfo.CreateNoWindow = false;
            ProcessInfo.UseShellExecute = false;
            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();
            Process.Close();*/

            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();

        }

        void executeCommands(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = @"/C "+command;
            process.StartInfo.RedirectStandardOutput = false;
            process.Start();
            process.WaitForExit();
            process.Close();
        }
        
        public void copy()
        {
            //COPY PB OBJ 
            string PBOBJ = PBID + ".obj ";
            string cmd11 = "copy " + pbOBJInputPath + PBOBJ + pbOBJOutputPath;
            executeCommands(cmd11);
            //NAMING THE USER OBJ FILES(3 FILES OBJ ,MTL AND JPG)
            string userOBJ = userOBJInputPath + userID + ".obj ";
            string userMTL = userOBJInputPath + userID + ".mtl ";
            string userJPG = userOBJInputPath + userID + ".jpg ";
            string cmd12;
            //COPY  USER OBJ MTL AND JPG 
            cmd12 = "copy " + userOBJ + userOBJOutputPath;
            executeCommands(cmd12);
            //MOVING THE USER MTL FILES
            string cmd13;
            cmd13 = "copy " + userMTL + userOBJOutputPath;
            executeCommands(cmd13);
            //MOVING THE USER JPG FILES
            string cmd14;
            cmd14 = "copy " + userJPG + userOBJOutputPath;
            executeCommands(cmd14);
            //MOVING THE USER JPG FILE TO TARGET PATH
            string cmd15;
            cmd15 = "copy " + userJPG + TargetPath;
            executeCommands(cmd15);

        }
        public void headStitchStart()
        {
            string cmd20;
            cmd20 = userOBJOutputPath + userID + ".obj";
            executeCommands("TriangulateObj.exe",cmd20);

            string cmd5;
            cmd5 = pbOBJOutputPath + PBID + ".obj " + userOBJOutputPath + userID + ".obj " + userOBJOutputPath + userID + ".mtl " +
                TargetPath + userID + "C.obj " + TargetPath + userID + "C.mtl " + h1 + " " + h2;
            executeCommands("mAvatar.exe",cmd5);
        }
    }
}
