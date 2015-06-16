using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;


namespace kAvatar
{
    class csv
    {
        string path;
        public csv(string path)
        {
            this.path = path;
        }

        public List<string[]> parseCSV()
        {
            List<string[]> parsedData = new List<string[]>();

            try
            {
                using(StreamReader readFile=new StreamReader(path))
                {
                    string line;
                    string[] row;
                    while ((line = readFile.ReadLine()) != null)
                    {
                        row = line.Split(',');
                        parsedData.Add(row);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Reading CSV File "+e.StackTrace);
            }

            return parsedData;
        }

   
    }
}
