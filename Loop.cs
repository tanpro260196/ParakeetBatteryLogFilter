using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParakeetBatteryLogFilter
{
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
                    int dataposition = line.IndexOf("LOOP: #");
                    loopnumber = line.Substring(dataposition + 7);
                    break;
                }
            }
        }
        public void DateTime_Parse()
        {
            //0 is the line number that contain date and time. Zero based.
            //The date-time string looks like this: [MM dd yyyy, hh:mm:ssPM/AM]
            //This line remove the bracket from the string.
            if (looptext[0].Length > 3)
            {
                date_parsed = (looptext[0].Remove(looptext[0].Length - 3)).Substring(1);
                if (!(looptext[0].Remove(looptext[0].Length - 3)).Substring(1).Contains(','))
                    date_parsed += ", ";
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
            for (int i = 0; i < looptext.Count(); i++)
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
                    List<string> parseddata = new List<string>();
                    parseddata.AddRange(batterydata.Split(' '));
                    //for (int x = 0; x < (parseddata.Count() - 1); x++)
                    //{
                    //    if (!double.TryParse(parseddata[x], out _))
                    //        parseddata[x] = "Data not found";
                    //}
                    while (parseddata.Count() < 9)
                    {
                        parseddata.Add("Data not found");
                    }
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
            else if (battery_pegacmd.Count() > 9)
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
