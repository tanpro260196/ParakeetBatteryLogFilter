using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ParakeetBatteryLogFilter
{
    partial class LogFilter
    {
        [STAThread]
        static void Main()
        {
            //CALL FILE SELECTION FUNCTION
            List<FileInfo> fileselection = Get_filepath();
            if (fileselection == null)
                return;
            //Loop through all selected files.
            foreach (FileInfo file in fileselection)
            {
                //READ AND PROCESSED LOG CONTENT
                List<Loop> maintext_processed = Readtext(file.FullName);
                if (maintext_processed == null)
                {
                    continue;
                }
                //PARSE AND EXPORT TO CSV
                Data_export(maintext_processed, file.DirectoryName, file.Name);
            }
        }
        //THIS FUNCTION IDENTIFY THE BEGINNING AND END OF EACH LOOP IN THE LOG. 
        static List<Loop> Readtext(string file)
        {
            //THE LOG IS SPIT AND SAVED EACH LOOP TO THIS VARIABLE BELOW
            FileInfo inputfile = new FileInfo(file);
            List<Loop> loopdata = new List<Loop>();
            //Before processing, check if the file is readable and not in use by any other program.
            try
            {
                FileStream checkread = System.IO.File.OpenRead(@file);
                checkread.Close();
            }
            catch (System.IO.IOException)
            {
                //If the file cannot be read. Show an error box.
                string message_failed = "Cannot open " + inputfile.Name + Environment.NewLine + ". File is in use." + Environment.NewLine + Environment.NewLine + "Try again?";
                string caption_failed = "IO Error";
                MessageBoxButtons buttons_fail = MessageBoxButtons.YesNo;
                DialogResult result_fail;
                // Displays the MessageBox.
                result_fail = MessageBox.Show(message_failed, caption_failed, buttons_fail, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result_fail == System.Windows.Forms.DialogResult.Yes)
                {
                    return Readtext(file);
                    
                }
                if (result_fail == System.Windows.Forms.DialogResult.No)
                {
                    return null;
                }
            }

            //THEN READ THE CONTENT OF THE LOG FILE AND SAVE THEM TO A TEMP VARIABLE FOR PROCESSING.
            string[] text = System.IO.File.ReadAllLines(@file);
            int lastloop_end = 0;
            int i;
            for (i = 0; i < text.Count(); i++)
            {
                //FIND LOOP START IDENTIFIER FIRST
                //replace "LOOP: #" with loop start IDENTIFIER
                if ((text[i].Contains("LOOP: #")) && (!text[i].Contains("*")))
                {
                    Loop temploopdata = new Loop();
                    //Try to detect the line which contains the timestamp. The details for this function is in the Misc.cs file.
                    temploopdata.looptext.Add(Time_Stamp_Detection(lastloop_end, i, text));

                    int j;
                    //THEN GO THROUGH THE DATA LINE BY LINE (AND SAVE THOSE LINES TO THE LOOP ARRAY) UNTIL IT SEE THE END LOOP IDENTIFIER
                    //replace ***** with end loop IDENTIFIER
                    for (j = i; (j < text.Count() && (!text[j].Contains("**********************************************************************"))); j++)
                    {
                        temploopdata.looptext.Add(text[j]);
                    }
                    if (temploopdata != null)
                    {
                        loopdata.Add(temploopdata);
                        i = j;
                        lastloop_end = i + 2;
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
            openFileDialog1.Dispose();
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

            //Before writing to file. Check if file is writable or not.
            try
            {
                FileStream checkwrite = System.IO.File.OpenWrite(@Path.Combine(folderpath, filename.Remove(filename.Length - 4) + ".csv"));
                checkwrite.Close();
            }
            catch (System.IO.IOException)
            {
                string message_failed = "Cannot write to destination file. File is in use." + Environment.NewLine + Environment.NewLine + "Try again?";
                string caption_failed = "IO Error";
                MessageBoxButtons buttons_fail = MessageBoxButtons.YesNo;
                DialogResult result_fail;
                // Displays the MessageBox.
                result_fail = MessageBox.Show(message_failed, caption_failed, buttons_fail, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result_fail == System.Windows.Forms.DialogResult.Yes)
                {
                    Data_export(data, folderpath, filename);
                    return;
                }
                else
                    return;
            }

            // Write the string array to a new file.
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(folderpath, filename.Remove(filename.Length - 4) + ".csv")))
            {
                //THIS LINE IS THE LABEL OF EACH COLUMNS IN THE CSV FILE. CHANGE OR REMOVE THEM AS YOU SEE FIT.
                outputFile.WriteLine("Loop #, Date, Time, u16VacADCVal, u16VacADCFirstVal, sVacVoltage,VBUS(V), VBAT(V), VSYS(V), IBUS(mA), IBAT(mA), TS_JC(C), Discharging, Percentage (%), CHG_STAT,wSysTempADCValue,wSysTemperature,Heater Status,CHG_STATUS (0x42),JEITA (0x43),Charging Status (0x02),Reg(0x00),Reg(0x01),Reg(0x02),Reg(0x03),Reg(0x04),Reg(0x05),Reg(0x06),Reg(0x07),Reg(0x08),Reg(0x09),Reg(0x0a),Reg(0x0b),Reg(0x0c),Reg(0x0d),Reg(0x0e),Reg(0x0f),Reg(0x10),Reg(0x11),Reg(0x18),Reg(0x19),Reg(0x1a),Reg(0x40),Reg(0x42),Reg(0x43),Reg(0x44),Reg(0x45),Reg(0x50),Reg(0x51),Reg(0x52),Reg(0x53),Reg(0x54),Reg(0x55),Reg(0x60),Reg(0x61),Reg(0x62),Reg(0x63),Reg(0x64),Reg(0x65),FW Version,Wifi Version,Camera Version");
                foreach (string line in combinetocsv)
                    outputFile.WriteLine(line);
            }

            //Show a success message box after export and offer to open the file on the spot.
            string message = "Data exported to " + folderpath + "\\" + filename.Remove(filename.Length - 4) + ".csv." + Environment.NewLine + Environment.NewLine + "Open exported file?";
            string caption = "Success!";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;
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
}