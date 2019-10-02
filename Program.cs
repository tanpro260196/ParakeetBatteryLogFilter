using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;
using System.Globalization;

namespace ParakeetBatteryLogFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo fileselection = get_filepath();
            List<loop> maintext_processed = readtext(fileselection.FullName);
            data_export(maintext_processed, fileselection.DirectoryName, fileselection.Name);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        static List<loop> readtext(string file)
        {
            List<loop> loopdata = new List<loop>();
            string[] text = System.IO.File.ReadAllLines(@file);
            int i = 0;
            for (i = 0; i < text.Count(); i++)
            {
                //replace LOOP: # with loop start string
                if ((text[i].Contains("LOOP: #")) && (!text[i].Contains("*")))
                {
                    loop temploopdata = new loop();
                    int j = 0;
                    //replace ***** with end loop string
                    for (j = i - 1; (j < text.Count() && (!text[j].Contains("**********************************************************************")));j++)
                    { 
                        temploopdata.looptext.Add(text[j]);
                    }
                    if (temploopdata != null)
                    {
                        loopdata.Add(temploopdata);
                        i = j;
                    }
                }
            }
            return loopdata;
        }
        static FileInfo get_filepath()
        {
            bool pathcheck = false;
            bool fileselectcheck = false;
            string fileno = null;
            FileInfo[] Files = null;
            while (!pathcheck || !fileselectcheck)
            {
                Console.WriteLine("Enter Log Folder Path:");
                string filepath = Console.ReadLine();
                DirectoryInfo d = new DirectoryInfo(@filepath);//Your Folder
                if (!d.Exists)
                {
                    Console.WriteLine("That folder does not exist. Press any key to continue...");
                    Console.ReadKey(true);
                    continue;
                }
                Files = d.GetFiles("*.log"); //Getting Log files
                if (!(Files.Count() == 0))
                {
                    pathcheck = true;
                }
                else
                {
                    Console.WriteLine("No log found. Press any key to continue...");
                    Console.ReadKey(true);
                    continue;
                }
                int filecount = 0;
                foreach (FileInfo file in Files)
                {
                    filecount++;
                    Console.WriteLine("[" + filecount + "] " + file.Name);
                }

                Console.WriteLine("Select log file number to open:");
                fileno = Console.ReadLine();
                if ((int.TryParse(fileno, out int _)) && fileno != "0")
                {
                    fileselectcheck = true;
                }
                else
                {
                    Console.WriteLine("Incorrect input. Press any key to continue...");
                    Console.ReadKey(true);
                    continue;
                }
                if (Convert.ToInt32(fileno) > Files.Count())
                {
                    Console.WriteLine("Incorrect input. Press any key to continue...");
                    Console.ReadKey(true);
                    continue;
                }
            }
            //string completefile = Files[Convert.ToInt32(fileno) - 1].FullName;
            return Files[Convert.ToInt32(fileno) - 1];
        }
        static void data_export(List<loop> data, string folderpath, string filename)
        {
            foreach (loop showdata in data)
            {
                showdata.VACparse();
                showdata.batteryparse();
                showdata.TemperatureParse();
                showdata.HeaterParse();
                showdata.CHG_STATUS42_Parse();
                showdata.JEITA43_Parse();
                showdata.Charging02_Parse();
                showdata.registerdump_parse();
                showdata.combinedata();
            }
            List<string> combinetocsv = new List<string>();
            foreach (loop showdata in data)
            {
                combinetocsv.Add(string.Join(",", showdata.all_processed_parsed_data));
            }
            // Write the string array to a new file.
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(folderpath, filename + ".csv")))
            {
                outputFile.WriteLine("Date, Time, u16VacADCVal, u16VacADCFirstVal, sVacVoltage, ,VBUS(V), VBAT(V), VSYS(V), IBUS(mA), IBAT(mA), TS_JC(C), Discharging, Percentage, CHG_STAT,wSysTempADCValue,wSysTemperature,Heater Status,CHG_STATUS,JEITA,Charging Status (0x02),Reg(0x00),Reg(0x01),Reg(0x02),Reg(0x03),Reg(0x04),Reg(0x05),Reg(0x06),Reg(0x07),Reg(0x08),Reg(0x09),Reg(0x0a),Reg(0x0b),Reg(0x0c),Reg(0x0d),Reg(0x0e),Reg(0x0f),Reg(0x10),Reg(0x11),Reg(0x18),Reg(0x19),Reg(0x1a),Reg(0x40),Reg(0x42),Reg(0x43),Reg(0x44),Reg(0x45),Reg(0x50),Reg(0x51),Reg(0x52),Reg(0x53),Reg(0x54),Reg(0x55),Reg(0x60),Reg(0x61),Reg(0x62),Reg(0x63),Reg(0x64),Reg(0x65)");
                foreach (string line in combinetocsv)
                    outputFile.WriteLine(line);
            }
            Console.WriteLine("Data Exported. Saved to the same folder as the input.");

        }
    }
    public class loop
    {
        public List<string> looptext;
        public List<string> VACDATA;
        public List<string> battery_pegacmd;
        public List<string> temperature;
        public string heaterstatus;
        public string CHG_STATUS42;
        public string JEITA43;
        public string Charging02;
        public List<string> registerdump;
        public List<string> all_processed_parsed_data;
        public loop()
        {
            looptext = new List<string>();
        }
        public void VACparse()
        {
            VACDATA = new List<string>();
            int resultfound = 0;
            foreach (string line in looptext)
            {
                if (line.Contains("m_stADC.stVac.u16VacADCVal"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    VACDATA.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("m_stADC.stVac.u16VacADCFirstVal"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    VACDATA.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("m_stADC.stVac.sVacVoltage"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    VACDATA.Add(line.Substring(datalocation));
                    resultfound++;
                }
            }
            if (resultfound == 0)
                VACDATA.Add("No result found.");
        }
        public void batteryparse()
        {
            bool resultfound = false;
            battery_pegacmd = new List<string>();
            int i = 0;
            for (i=0; i< looptext.Count();i++)
            {
                if (looptext[i].Contains("VBUS(V) VBAT(V) VSYS(V) IBUS(mA) IBAT(mA) TS_JC(C) Discharging Percentage CHG_STAT"))
                {
                    string batterydata = looptext[i + 1];
                    batterydata = NormalizeWhiteSpace(batterydata);
                    string[] parseddata = batterydata.Split(' ');
                    
                    foreach (string temp in parseddata)
                    {
                        //Console.WriteLine(temp);
                        resultfound = true;
                        battery_pegacmd.Add(temp);
                    }
                }
            }
            if (!resultfound)
                battery_pegacmd.Add("Result not found.");
        }
        public void TemperatureParse()
        {
            temperature = new List<string>();
            int resultfound = 0;
            foreach (string line in looptext)
            {
                if (line.Contains("m_stADC.wSysTempADCValue"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    temperature.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("m_stADC.wSysTemperature(C)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    temperature.Add(line.Substring(datalocation));
                    resultfound++;
                }
            }
            if (resultfound == 0)
                temperature.Add("Result not found.");
        }
        public void HeaterParse()
        {
            bool resultfound = false;
            foreach (string line in looptext)
            {
                if ((line.Length == 1) && int.TryParse(line, out int _))
                {
                    if (line.Contains("1"))
                        heaterstatus = "ON";
                    else if (line.Contains("0"))
                        heaterstatus = "OFF";
                    resultfound = true;
                }
            }
            if (!resultfound)
                heaterstatus = "No result found";
        }
        public void CHG_STATUS42_Parse()
        {
            bool resultfound = false;
            foreach (string line in looptext)
            {
                if (line.Contains("read:regAddr(0x0x42)"))
                {
                    int datalocation = line.IndexOf("= ") + 2;
                    CHG_STATUS42 = line.Substring(datalocation);
                    resultfound = true;
                }
            }
            if (!resultfound)
                CHG_STATUS42 = "No result found.";
        }
        public void JEITA43_Parse()
        {
            bool resultfound = false;
            foreach (string line in looptext)
            {
                if (line.Contains("read:regAddr(0x0x43)"))
                {
                    int datalocation = line.IndexOf("= ") + 2;
                    JEITA43 = line.Substring(datalocation);
                    resultfound = true;
                }
            }
            if (!resultfound)
                JEITA43 = "No result found.";
        }
        public void Charging02_Parse()
        {
            bool resultfound = false;
            foreach (string line in looptext)
            {
                if (line.Contains("read:regAddr(0x0x2)"))
                {
                    int datalocation = line.IndexOf("= ") + 2;
                    Charging02 = line.Substring(datalocation);
                    resultfound = true;
                }
            }
            if (!resultfound)
                Charging02 = "No result found.";
        }
        public void registerdump_parse()
        {
            registerdump = new List<string>();
            int resultfound = 0;
            foreach (string line in looptext)
            {
                if (line.Contains("Reg(0x00)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x01)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x02)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x03)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x04)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x05)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x06)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x07)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x08)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x09)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x0a)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x0b)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x0c)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x0d)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x0e)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x0f)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x10)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x11)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x18)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x19)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x1a)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x40)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x42)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x43)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x44)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x45)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x50)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x51)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x52)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x53)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x54)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x55)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x60)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x61)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x62)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x63)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x64)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("Reg(0x65)"))
                {
                    int datalocation = line.IndexOf("=> ") + 3;
                    registerdump.Add(line.Substring(datalocation));
                    resultfound++;
                }
            }
            if (resultfound == 0)
                registerdump.Add("Result not found.");
        }
        public void combinedata()
        {
            all_processed_parsed_data = new List<string>();
            all_processed_parsed_data.Add(looptext[0]);
            all_processed_parsed_data.AddRange(VACDATA);
            all_processed_parsed_data.AddRange(battery_pegacmd);
            all_processed_parsed_data.AddRange(temperature);
            all_processed_parsed_data.Add(heaterstatus);
            all_processed_parsed_data.Add(CHG_STATUS42);
            all_processed_parsed_data.Add(JEITA43);
            all_processed_parsed_data.Add(Charging02);
            all_processed_parsed_data.AddRange(registerdump);
        }
        private static string NormalizeWhiteSpace(string input, char normalizeTo = ' ')
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            StringBuilder output = new StringBuilder();
            bool skipped = false;

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!skipped)
                    {
                        output.Append(normalizeTo);
                        skipped = true;
                    }
                }
                else
                {
                    skipped = false;
                    output.Append(c);
                }
            }

            return output.ToString();
        }
    }
}