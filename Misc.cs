using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParakeetBatteryLogFilter
{
    partial class LogFilter
    {
        static public string Time_Stamp_Detection(int start, int end, string[] text)
        {
            string datefound = null;
            //Go line by line from the end of last loop to the "LOOP: #" line.
            for (int x = start; x <= end; x++)
            {
                //Check each line for certain basic timestamp character. Such as '[', ']'
                if (text[x].Contains('[') && text[x].Contains(']') && !text[x].Contains("pega_i2c") && !text[x].Contains("wifi"))
                {
                    //Go through that line character by character.
                    for (int i = 0; i <= text[x].Length - 1; i++)
                    {
                        //Detect '[' as starting character.
                        if (text[x].ElementAt(i) == '[')
                        {
                            //Check every chacracter follow the starting character to make sure they follow proper format: [MM dd yyyy, hh:mm:sstt]
                            if (!int.TryParse(text[x].ElementAt(i + 1).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 2).ToString(), out _))
                                continue;
                            if (text[x].ElementAt(i + 3) != ' ')
                                continue;
                            if (!int.TryParse(text[x].ElementAt(i + 4).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 5).ToString(), out _))
                                continue;
                            if (text[x].ElementAt(i + 6) != ' ')
                                continue;
                            if (!int.TryParse(text[x].ElementAt(i + 7).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 8).ToString(), out _))
                                continue;
                            if (!int.TryParse(text[x].ElementAt(i + 9).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 10).ToString(), out _))
                                continue;
                            if (text[x].ElementAt(i + 11) != ',')
                                continue;
                            if (text[x].ElementAt(i + 12) != ' ')
                                continue;
                            if (!int.TryParse(text[x].ElementAt(i + 13).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 14).ToString(), out _))
                                continue;
                            if (text[x].ElementAt(i + 15) != ':')
                                continue;
                            if (!int.TryParse(text[x].ElementAt(i + 16).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 17).ToString(), out _))
                                continue;
                            if (text[x].ElementAt(i + 18) != ':')
                                continue;
                            if (!int.TryParse(text[x].ElementAt(i + 19).ToString(), out _) || !int.TryParse(text[x].ElementAt(i + 20).ToString(), out _))
                                continue;
                            //If all conditions above is false, the line does contain the timestamp. Then we return the result and break the loops.
                            datefound = text[x].Substring(i, 24);
                            x = end + 1;
                            break;
                        }
                    }
                    //temploopdata.looptext.Add(text[x]);
                    //datefound = text[x];
                    //return datefound;
                }
            }
            if (datefound == null)
            {
                return "[Date not found, Time not found  ]";
            }
            return datefound;
        }
    }
}
