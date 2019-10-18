using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;

namespace ParakeetBatteryLogFilter
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            //CALL FILE SELECTION FUNCTION
            List <FileInfo> fileselection = Get_filepath();
            if (fileselection == null)
                return;
            //Loop through all selected files.
            foreach (FileInfo file in fileselection)
            {
                //READ AND PROCESSED LOG CONTENT
                List<Loop> maintext_processed = Readtext(file.FullName);
                if (maintext_processed == null)
                {
                    Main();
                    return;
                }
                //PARSE AND EXPORT TO CSV
                Data_export(maintext_processed, file.DirectoryName, file.Name);
            }
        }
        //THIS FUNCTION IDENTIFY THE BEGINNING AND END OF EACH LOOP IN THE LOG. 
        static List<Loop> Readtext(string file)
        {
            //THE LOG IS SPIT AND SAVED EACH LOOP TO THIS VARIABLE BELOW
            List<Loop> loopdata = new List<Loop>();
            //READ THE CONTENT OF THE LOG FILE AND SAVE THEM TO A TEMP VARIABLE FOR PROCESSING.
            try { FileStream checkread = System.IO.File.OpenRead(@file);
                checkread.Close();
            }
            catch (System.IO.IOException)
            {
                string message_failed = "Cannot open input file(s). File(s) is in use.";
                string caption_failed = "IO Error";
                MessageBoxButtons buttons_fail = MessageBoxButtons.OK;
                DialogResult result_fail;
                // Displays the MessageBox.
                result_fail = MessageBox.Show(message_failed, caption_failed, buttons_fail, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result_fail == System.Windows.Forms.DialogResult.OK)
                {
                    return null;
                }
                //Console.WriteLine("Can't open file. Did you close Tera Term log yet?");
                //Console.WriteLine("Press any key to continue...");
                //Console.ReadKey(true);
                //return null;
            }
            string[] text = System.IO.File.ReadAllLines(@file);
            int i = 0;
            for (i = 0; i < text.Count(); i++)
            {
                //FIND LOOP START IDENTIFIER FIRST
                //replace "LOOP: #" with loop start IDENTIFIER
                if ((text[i].Contains("LOOP: #")) && (!text[i].Contains("*")))
                {
                    Loop temploopdata = new Loop();
                    int j = 0;
                    //THEN GO THROUGH THE DATA LINE BY LINE (AND SAVE THOSE LINES TO THE LOOP ARRAY) UNTIL IT SEE THE END LOOP IDENTIFIER
                    //replace ***** with end loop IDENTIFIER
                    int offset;
                    if (text[i - 1].Length == 0)
                        offset = 2;
                    else offset = 1;
                    for (j = i - offset; (j < text.Count() && (!text[j].Contains("**********************************************************************")));j++)
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
        //THIS FUNCTION HANDLE USER INPUT. IGNORE THIS.
        static List<FileInfo> Get_filepath()
        {
            bool pathcheck = false;
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            List<FileInfo> File = new List<FileInfo>();
            while (!pathcheck)
            {
                openFileDialog1.Multiselect = true;
                openFileDialog1.InitialDirectory = @"C:\BatteryTest";
                openFileDialog1.DefaultExt = "log";
                openFileDialog1.Title = "Open Log File";
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;
                openFileDialog1.Filter = "Log Files(*.log)| *.log";
                DialogResult result = openFileDialog1.ShowDialog();
                //Console.WriteLine(openFileDialog1.FileName);
                if (result == DialogResult.Cancel)
                    break;
                foreach (string filename in openFileDialog1.FileNames)
                {
                    if (filename.Length == 0)
                        break;
                    if ((filename.Substring(filename.Length - 3)) == "log")
                    {
                        pathcheck = true;
                        File.Add(new FileInfo(filename));
                    }
                    
                }
            }
            //openFileDialog1.Dispose();
            return File;
        }
        //THIS FUNCTION HANDLE DATA EXPORT.
        static void Data_export(List<Loop> data, string folderpath, string filename)
        {
            //CALL PARSE FUNCTION AND EXTRACT WANTED DATA. SEE THE CLASS FUNCTION FOR DETAILS.
            foreach (Loop showdata in data)
            {
                showdata.LoopNumber_Parse();
                showdata.DateTime_Parse();
                showdata.VACparse();
                showdata.Batteryparse();
                showdata.TemperatureParse();
                showdata.HeaterParse();
                showdata.CHG_STATUS42_Parse();
                showdata.JEITA43_Parse();
                showdata.Charging02_Parse();
                showdata.Registerdump_parse();
                showdata.FW_Version_Parse();
                //COMBINE ALL PARSED DATA INTO A SINGLE STRING. THIS SHOULD ALWAYS BE THE LAST STEP.
                showdata.Combinedata();
            }
            List<string> combinetocsv = new List<string>();
            //CREATE AN ARRAY WITH ALL THE DATA YOU WANT TO EXPORT
            foreach (Loop showdata in data)
            {
                combinetocsv.Add(string.Join(",", showdata.all_processed_parsed_data.ToArray()));
            }
            // Write the string array to a new file.
            try            
            {
                FileStream checkwrite = System.IO.File.OpenWrite(@Path.Combine(folderpath, filename.Remove(filename.Length - 4) + ".csv"));
                checkwrite.Close();
            }
            catch(System.IO.IOException)
            {
                string message_failed = "Cannot write to destination file. File is in use.";
                string caption_failed = "IO Error";
                MessageBoxButtons buttons_fail = MessageBoxButtons.OK;
                DialogResult result_fail;
                // Displays the MessageBox.
                result_fail = MessageBox.Show(message_failed, caption_failed, buttons_fail, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result_fail == System.Windows.Forms.DialogResult.OK)
                {
                    Main();
                    return;
                }
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(folderpath, filename.Remove(filename.Length - 4) + ".csv")))
            {
                //THIS LINE IS THE LABEL OF EACH COLUMNS IN THE CSV FILE. CHANGE OR REMOVE THEM AS YOU SEE FIT.
                outputFile.WriteLine("Loop #, Date, Time, u16VacADCVal, u16VacADCFirstVal, sVacVoltage,VBUS(V), VBAT(V), VSYS(V), IBUS(mA), IBAT(mA), TS_JC(C), Discharging, Percentage, CHG_STAT,wSysTempADCValue,wSysTemperature,Heater Status,CHG_STATUS (0x42),JEITA (0x43),Charging Status (0x02),Reg(0x00),Reg(0x01),Reg(0x02),Reg(0x03),Reg(0x04),Reg(0x05),Reg(0x06),Reg(0x07),Reg(0x08),Reg(0x09),Reg(0x0a),Reg(0x0b),Reg(0x0c),Reg(0x0d),Reg(0x0e),Reg(0x0f),Reg(0x10),Reg(0x11),Reg(0x18),Reg(0x19),Reg(0x1a),Reg(0x40),Reg(0x42),Reg(0x43),Reg(0x44),Reg(0x45),Reg(0x50),Reg(0x51),Reg(0x52),Reg(0x53),Reg(0x54),Reg(0x55),Reg(0x60),Reg(0x61),Reg(0x62),Reg(0x63),Reg(0x64),Reg(0x65),FW Version,Wifi Version,Camera Version");
                foreach (string line in combinetocsv)
                    outputFile.WriteLine(line);
            }
            string message = "Data exported to " + folderpath + "\\" + filename.Remove(filename.Length - 4) + ".csv." + Environment.NewLine + Environment.NewLine + "Open exported file?";
            string caption = "Success!";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;
            //test
            //Form1 newform = new Form1(caption,message, folderpath + "\\" + filename.Remove(filename.Length - 4) + ".csv.");
            //Application.Run(newform);
            //test

            // Displays the MessageBox.
            result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(folderpath + "\\" + filename.Remove(filename.Length - 4) + ".csv.");
                return;
            }
            else
                return;
        }
    }
    //THIS CLASS STORE AND PARSE DATA FROM EACH LOOP INTO NUMERIC DATA.
    public class Loop
    {
        //THIS VARIABLE CONTAIN RAW LOG DATA OF THE LOOP
        public List<string> looptext;
        //THESE VARIABLES STORE DATA AFTER YOU PARSED THEM.
        public string loopnumber;
        public string date_parsed;
        public List<string> VACDATA;
        public List<string> battery_pegacmd;
        public List<string> temperature;
        public string heaterstatus;
        public string CHG_STATUS42;
        public string JEITA43;
        public string Charging02;
        public List<string> registerdump;
        public List<string> FWVersion;
        //THIS VARIABLE COMBINE ALL THE ABOVE INTO ONE.
        public List<string> all_processed_parsed_data;
        
        //THIS FUNCTION INITIALIZE THE CLASS. IGNORE THIS.
        public Loop()
        {
            looptext = new List<string>();
        }
        //THESE FUNCTIONS BELOW PARSE THE DATA FROM "looptext".
        public void LoopNumber_Parse()
        {
            //This search and extract loop number from the second string (index 1, using zero-based counting) in each loop.
            //The string we are looking for is in this format: "LOOP #X".The X position in the string is 7 (count on zero-based position). 
            //Then we use this function to extract the data: looptext[Y].Substring(Z): Y is the line number count from zero. Z is the position of X count from zero.
            foreach (string line in looptext)
            {
                if (line.Contains("LOOP: #"))
                {
                    loopnumber =line.Substring(7);
                    break;
                }
            }
        }
        public void DateTime_Parse()
        {
            //0 is the line number that contain date and time. Zero based.
            //The date-time string looks like this: [MM DD YYYY, HH:MM:SSPM/AM]
            //This line remove the bracket from the string.
            if (looptext[0].Length > 3)
            {
                date_parsed = (looptext[0].Remove(looptext[0].Length - 3)).Substring(1);
                if (!(looptext[0].Remove(looptext[0].Length - 3)).Substring(1).Contains(','))
                    date_parsed = date_parsed + ", ";
                //this line search for the seperator between date and time
                int datelocation = date_parsed.IndexOf(", ");
                //this line remove the space between date and time.
                date_parsed = date_parsed.Remove(datelocation + 1, 1);
            }
            else
                date_parsed = "Parse Error,Parse Error";
        }
        public void VACparse()
        {
            //AC voltage parse.
            //any variable in List<> type need to be initialize before use like this: example_var = new List<xxx>();
            VACDATA = new List<string>();
            int resultfound = 0;
            foreach (string line in looptext)
            {
                //This is the most common type of result. A string will contain some identifier before each value. Like this:
                // AAAA xxx YYY: AAA is the fixed identifier, xxx is some middle string (can be : or =>) and YYY is the value we want
                //First we search for AAA using this line: line.Contains("AAA")
                if (line.Contains("m_stADC.stVac.u16VacADCVal"))
                {
                    //Then we search for location of xxx: "int datalocation = line.IndexOf("xxx") + length_of_xxx";
                    int datalocation = line.IndexOf("=> ") + 3;
                    //Finally we extract the data we want (aka YYY) and assign it to one of the variable we declared above.
                    VACDATA.Add(line.Substring(datalocation));
                    resultfound++;
                }
                //repeat as many times as you need to get all the data for each line.
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
                if (resultfound >= 3)
                {
                    break;
                }
            }
            //if for some reason the result is not recorded in that loop. We add this instead.
            if (resultfound == 0)
                VACDATA.Add("No result found., ,");
        }
        public void Batteryparse()
        {
            //Special case. The result is present in a table-like manner.
            //This type usually have 1 line contain the data label and the data is in the next line.
            bool resultfound = false;
            battery_pegacmd = new List<string>();
            //int i = 0;
            for (int i=0; i< looptext.Count();i++)
            {
                //search for the lable line by literally search for the entire line.
                if (looptext[i].Contains("VBUS(V) VBAT(V) VSYS(V) IBUS(mA) IBAT(mA) TS_JC(C) Discharging Percentage CHG_STAT"))
                {
                    //the data we want will usually in the next line. Hence we use i+1.
                    string batterydata = looptext[i + 1];
                    //replace junk data with space.
                    batterydata = batterydata.Replace('/', ' ');
                    batterydata = batterydata.Replace('#', ' ');
                    //Then we remove all white space and replace with a single space using NormalizeWhiteSpace funtion.
                    batterydata = NormalizeWhiteSpace(batterydata);
                    //spit the normalized string into an array of number.
                    string[] parseddata = batterydata.Split(' ');
                    //add those number to the variable we declared above
                    foreach (string temp in parseddata)
                    {
                        resultfound = true;
                        battery_pegacmd.Add(temp);
                    }
                }
            }
            if (!resultfound)
                battery_pegacmd.Add("No result found.,,,,,,,,");
            else
                battery_pegacmd.RemoveAt(0);
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
                if (resultfound >= 2)
                {
                    break;
                }
            }
            if (resultfound == 0)
                temperature.Add("No result found.,");
        }
        public void HeaterParse()
        {
            //This is a tricky part. The result only contain a single integer, no indentifier whatsoever.
            bool resultfound = false;
            foreach (string line in looptext)
            {
                //Lucky for us. The result can only be 0 or 1. So we each for a line with length = 1 and contain an integer.
                if ((line.Length == 1) && int.TryParse(line, out int _))
                {
                    //Assign 1 = ON and 0 = OFF
                    if (line.Contains("1"))
                        heaterstatus = "ON";
                    else if (line.Contains("0"))
                        heaterstatus = "OFF";
                    resultfound = true;
                }
            }
            if (!resultfound)
                heaterstatus = "No result found";
            //This could get much more complicated. What if instead of 1 and 0 it's just a random number or word? 
            //What if there are multiple commands that return result like this in a script? 
            //This should be take into consideration when you write your script. 
            //Ex: add a pause long enough for the result to return right after the command, then use the command as identifier.
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
                if (resultfound)
                {
                    break;
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
                if (resultfound)
                {
                    break;
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
                if (resultfound)
                {
                    break;
                }
            }
            if (!resultfound)
                Charging02 = "No result found.";
        }
        public void Registerdump_parse()
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
                if (resultfound >= 38)
                    break;
            }
            if (resultfound == 0)
                registerdump.Add("No result found.");
        }
        public void FW_Version_Parse()
        {
            FWVersion = new List<string>();
            int resultfound = 0;
            foreach (string line in looptext)
            {
                if (line.Contains("	version = "))
                {
                    int datalocation = line.IndexOf("= ") + 2;
                    FWVersion.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("wifi version = "))
                {
                    int datalocation = line.IndexOf("= ") + 2;
                    FWVersion.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (line.Contains("camif --version:"))
                {
                    int datalocation = line.IndexOf(":") + 1;
                    FWVersion.Add(line.Substring(datalocation));
                    resultfound++;
                }
                if (resultfound >= 3)
                {
                    break;
                }
            }
            if (resultfound == 0)
                FWVersion.Add("No result found.,,");
        }
        public void Combinedata()
        {
            //here you combine add variables you declared in the first part of this class into one long string.
            //If the variable is of List<> type, use AddRange(), if not, use Add().
            all_processed_parsed_data = new List<string>();
            all_processed_parsed_data.Add(loopnumber);
            all_processed_parsed_data.Add(date_parsed);
            all_processed_parsed_data.AddRange(VACDATA);
            all_processed_parsed_data.AddRange(battery_pegacmd);
            all_processed_parsed_data.AddRange(temperature);
            all_processed_parsed_data.Add(heaterstatus);
            all_processed_parsed_data.Add(CHG_STATUS42);
            all_processed_parsed_data.Add(JEITA43);
            all_processed_parsed_data.Add(Charging02);
            all_processed_parsed_data.AddRange(registerdump);
            all_processed_parsed_data.AddRange(FWVersion);
        }
        //This function remove all white spaces that are like this "     " and replace that with " "
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