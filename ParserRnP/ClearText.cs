﻿namespace ParserRnP
{
    public class ClearText
    {
        public static string ClearString(string s)
        {
            string st = s;
            st = st.Replace("ns2:", "");
            st = st.Replace("oos:", "");
            st = st.Replace("", "");
            return st;
        }
    }
}