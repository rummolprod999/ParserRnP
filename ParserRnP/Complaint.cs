using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace ParserRnP
{
    public class Complaint
    {
        protected readonly JObject t;
        protected readonly FileInfo file;
        protected readonly string file_path;
        
        public Complaint(FileInfo f, JObject json)
        {
            t = json;
            file = f;
            file_path = file.ToString();
        }

        public virtual void Parsing()
        {
        }

        public string GetXml(string xml)
        {
            string[] xmlt = xml.Split('/');
            int t = xmlt.Length;
            if (t >= 2)
            {
                string sxml = xmlt[t - 2] + "/" + xmlt[t - 1];
                return sxml;
            }

            return "";
        }

        public List<JToken> GetElements(JToken j, string s)
        {
            List<JToken> els = new List<JToken>();
            var els_obj = j.SelectToken(s);
            if (els_obj != null && els_obj.Type != JTokenType.Null)
            {
                Console.WriteLine(els_obj.Type);
                switch (els_obj.Type)
                {
                    case JTokenType.Object:
                        els.Add(els_obj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(els_obj);
                        break;
                }
            }

            return els;
        }
        
        public List<JToken> GetElementsLots(JToken j, string s)
        {
            List<JToken> els = new List<JToken>();
            var els_obj = j.SelectToken(s);
            if (els_obj != null && els_obj.Type != JTokenType.Null)
            {
                switch (els_obj.Type)
                {
                    case JTokenType.Object:
                        els.Add(els_obj);
                        break;
                    case JTokenType.String:
                        els.Add(els_obj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(els_obj);
                        break;
                }
            }

            return els;
        }
    }
}