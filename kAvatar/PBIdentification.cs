using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;


namespace kAvatar
{
    class PBIdentification
    {
        string PB_Path = @"C:\Users\PRIYASH_11\Documents\Visual Studio 2013\Projects\kAvatar\ClosestFit\Debug\";
        char gender;
        float height;
        float bust;
        float waist;
        float hips;
        public PBIdentification()
        {
            Directory.SetCurrentDirectory(PB_Path);
        }

        string executeCommands(string args)
        {
            //THIS ONLY WORKS FOR C# CONSOLE PROGRAM NOT GUI 
            /*ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + command);
            ProcessInfo.CreateNoWindow = false;
            ProcessInfo.UseShellExecute = false;
            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();
            Process.Close();*/


            //THIS ONLY WORKS FOR c# WINFORM GUI PLATFORM
            Process process = new Process();
            process.StartInfo.FileName = "ClosestFit.exe ";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string s = process.StandardOutput.ReadToEnd();
            return s;
        }
        public string[] PB_CATEGORIZER(char gender,float height,float bust ,float waist ,float hips)
        {
            this.gender = gender;
            this.height = height;
            this.bust = bust;
            this.waist = waist;
            this.hips = hips;
            //string path = Directory.GetCurrentDirectory();
            string args;
            args = gender + " " + height + " " + bust + " " + waist + " " + hips;
            string s=executeCommands(args);
            string[] str = s.Split(',');
            List<string[]> list = new List<string[]>();
            list.Add(str);
            //PASS THE PBID, H1 AND H2 IN STRING ARRAY I.E list[0]
            return list[0];
        }

        private string getMaleBodyType()
        {
            List<string> bodyTypeCode = new List<string>(); ;
            bodyTypeCode.Add("BTM1");
            bodyTypeCode.Add("BTM2");
            bodyTypeCode.Add("BTM3");
            bodyTypeCode.Add("BTM4");
            bodyTypeCode.Add("BTM5");
            bodyTypeCode.Add("BTM6");
            bodyTypeCode.Add("BTM7");

            if (((bust < waist) && (waist <= hips)) || ((bust <= waist) && (waist < hips)))
            {
                return bodyTypeCode[0];
            }
            else if (((bust > waist) && (waist >= hips)) || ((bust >= waist) && (waist > hips)))
            {
                return bodyTypeCode[1];
            }
            else if (((76.20 <= waist) && (waist <= 81.28)) && ((0.95 * waist <= bust) && (bust <= 1.05 * waist)) && ((0.95 * waist <= hips) && (hips <= 1.05 * waist)))
            {
                return bodyTypeCode[2];
            }
            else if ((waist > 81.28) && ((0.95 * waist <= bust) && (bust <= 1.05 * waist)) && ((0.95 * waist <= hips) && (hips <= 1.05 * waist)))
            {
                return bodyTypeCode[3];
            }
            else if (((bust < waist) && (waist > hips)) && waist < 91.44)
            {
                return bodyTypeCode[4];
            }
            else if ((((bust < waist) && (waist > hips)) && waist >= 91.44))
            {
                return bodyTypeCode[5];
            }
            else if ((waist < 76.20) && (0.95 * waist <= bust) && (bust <= 1.05 * waist) && (0.95 * waist <= hips) && (hips <= 1.05 * waist))
            {
                return bodyTypeCode[6];
            }
            return "No_BodyType_Found";
        }

        private string getFemaleBodyType()
        {
            List<string> bodyTypeCode = new List<string>(); ;

            bodyTypeCode.Add("BTF1");
            bodyTypeCode.Add("BTF2");
            bodyTypeCode.Add("BTF3");
            bodyTypeCode.Add("BTF4");
            bodyTypeCode.Add("BTF5");

            if (bust < waist && waist <= hips || bust <= waist && waist < hips)
            {
                return bodyTypeCode[7];
            }
            else if (bust > waist && waist < hips)
            {
                return bodyTypeCode[8];
            }
            else if ((bust > waist) && (waist >= hips) || (bust >= waist && waist > hips))
            {
                return bodyTypeCode[9];
            }
            else if ((0.95 * waist <= bust) && (bust <= 1.05 * waist) && (0.95 * waist <= hips) && (hips <= 1.05 * waist))
            {
                return bodyTypeCode[10];
            }
            else if (bust < waist && waist > hips)
            {
                return bodyTypeCode[11];
            }

            return "No_BodyType_Found";
        }
        //THIS PIECE OF CODE IS NOT GIVING EXPECTED RESULTS NEED TO CHECK THE LOGIC
        public string getBodyType()
        {

            if (gender == 'M')
            {
                return getMaleBodyType();
            }
            else if (gender == 'F')
            {
                return getFemaleBodyType();
            }

            return "No_BodyType_Found";
        }
       

    }
}
