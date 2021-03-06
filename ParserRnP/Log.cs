﻿using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ParserRnP
{
    public class Log
    {
        private static object _locker = new object();
        public static void Logger(params object[] parametrs)
        {
            string s = "";
            s += DateTime.Now.ToString(CultureInfo.InvariantCulture);
            for (int i = 0; i < parametrs.Length; i++)
            {
                s = $"{s} {parametrs[i]}";
            }

            lock (_locker)
            {
                using (StreamWriter sw = new StreamWriter(Program.FileLog, true, Encoding.Default))
                {
                    sw.WriteLine(s);
                }
            }
        }
    }
}