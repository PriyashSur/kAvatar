using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace kAvatar
{
    class Cleaner
    {
        string rawAvatarPath = @"C:\InkarneInternal\kAvatar\RawAvatars\";
        Int64 userID;
        string PBID;
        //CUSTOM AVATAR INPUT AND OUTPUT PATH 
        string customAvatarInputPath = @"C:\InkarneInternal\kAvatar\Headstiching\Interimfolder\CustomAvatarfiles\";
        string customAvatarOutputPath = @"C:\InkarneInternal\UserAvatar\";
        //FILES TO BE DELETED FROM BELOW PATH
        string PBOBJPath = @"C:\InkarneInternal\kAvatar\Headstiching\Interimfolder\PBobj\";
        //RAW AVATAR INPUT AND OUTPUT PATH
        string rawAvatarInputPath = @"C:\InkarneInternal\kAvatar\Headstiching\Interimfolder\RawAvatarfiles\";
        string rawAvatarOutputPath = @"C:\InkarneInternal\kAvatar\RawAvatars\";
        //ALL FILES TO BE MOVED FROM PROCESSING FOLDER TO POST PROCESSING FOLDER
        string processingPath = @"C:\InkarneInternal\kAvatar\Processingfolder";
        string postProcessingPath = @"C:\InkarneInternal\kAvatar\Postprocessingfiles\";

        public Cleaner(Int64 userID, string PBID)
        {
            this.userID = userID;
            this.PBID = PBID;
        }

        void executeCommands(string command)
        {

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = @"/C " + command;
            process.StartInfo.RedirectStandardOutput = false;
            process.Start();
            process.WaitForExit();
            process.Close();

        }

        void CreateFolder()
        {
            //MAKING USER UNDER AVATAR SUBFOLDER BASED ON UserID
            string cmd = "mkdir " + customAvatarOutputPath + userID;
            executeCommands(cmd);
            //MAKING A SUBFOLDER UNDER RAW AVATAR FOLDER BASED ON USER ID
            string cmd2 = "mkdir " + rawAvatarPath + userID;
            executeCommands(cmd2);
            //MAKING A SUBFOLDER UNDER POST PROCESSING FILES BASED ON USER ID
            string cmd3 = "mkdir " + postProcessingPath + userID;
            executeCommands(cmd3);
        }

        void moveCustomAvatar()
        {
            //MOVING CUSTOM AVATAR OBJ FROM INPUT TO OUTPUT PATH 
            string userOBJ = userID + "C.obj ";
            string cmd4 = "move " + customAvatarInputPath + userOBJ + customAvatarOutputPath + userID;
            executeCommands(cmd4);
            //MOVING CUSTOM AVATAR MTL FROM INPUT TO OUTPUT PATH 
            string userMTL = userID + "C.mtl ";
            string cmd5 = "move " + customAvatarInputPath + userMTL + customAvatarOutputPath + userID;
            executeCommands(cmd5);
            //MOVING CUSTOM AVATAR JPG FROM INPUT TO OUTPUT PATH 
            string userJPG = userID + "1.jpg ";
            string cmd6 = "move " + customAvatarInputPath + userJPG + customAvatarOutputPath + userID;
            executeCommands(cmd6);

        }

        //DELETING FILES
        void deletePBOBJ()
        {
            string PBIDOBJ = PBID + ".obj ";
            string cmd7 = "del " + PBOBJPath + @"\" + PBIDOBJ;
            executeCommands(cmd7);
        }

        void moveRawAvatar()
        {
            //MOVING CUSTOM AVATAR OBJ FROM INPUT TO OUTPUT PATH 
            string userOBJ = userID + ".obj ";
            string cmd8 = "move " + rawAvatarInputPath + userOBJ + rawAvatarOutputPath + userID;
            executeCommands(cmd8);
            //MOVING CUSTOM AVATAR MTL FROM INPUT TO OUTPUT PATH 
            string userMTL = userID + ".mtl ";
            string cmd9 = "move " + rawAvatarInputPath + userMTL + rawAvatarOutputPath + userID;
            executeCommands(cmd9);
            //MOVING CUSTOM AVATAR JPG FROM INPUT TO OUTPUT PATH 
            string userJPG = userID + "1.jpg ";
            string cmd10 = "move " + rawAvatarInputPath + userJPG + rawAvatarOutputPath + userID;
            executeCommands(cmd10);
        }

        void moveALLFilesToPostProcessingFolder()
        {
            string cmd11 = "move " + processingPath + "*.* " + postProcessingPath + userID;
            executeCommands(cmd11);
        }

        //MOVE EVERYTHING 
        void move()
        {
            moveCustomAvatar();
            deletePBOBJ();
            moveRawAvatar();
            moveALLFilesToPostProcessingFolder();
        }

        public void MakeFolderAndClean()
        {
            CreateFolder();
            move();
        }


    }
}
