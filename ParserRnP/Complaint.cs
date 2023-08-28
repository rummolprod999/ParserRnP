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
        protected readonly JObject T;
        protected readonly FileInfo File;
        protected readonly string FilePath;
        
        public Complaint(FileInfo f, JObject json)
        {
            T = json;
            File = f;
            FilePath = File.ToString();
        }

        public virtual void Parsing()
        {
        }

        public string GetXml(string xml)
        {
            var xmlt = xml.Split('/');
            var t = xmlt.Length;
            if (t >= 2)
            {
                var sxml = xmlt[t - 2] + "/" + xmlt[t - 1];
                return sxml;
            }

            return "";
        }

        public List<JToken> GetElements(JToken j, string s)
        {
            var els = new List<JToken>();
            var elsObj = j.SelectToken(s);
            if (elsObj != null && elsObj.Type != JTokenType.Null)
            {
                //Console.WriteLine(els_obj.Type);
                switch (elsObj.Type)
                {
                    case JTokenType.Object:
                        els.Add(elsObj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(elsObj);
                        break;
                }
            }

            return els;
        }
        
        public List<JToken> GetElementsLots(JToken j, string s)
        {
            var els = new List<JToken>();
            var elsObj = j.SelectToken(s);
            if (elsObj != null && elsObj.Type != JTokenType.Null)
            {
                switch (elsObj.Type)
                {
                    case JTokenType.Object:
                        els.Add(elsObj);
                        break;
                    case JTokenType.String:
                        els.Add(elsObj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(elsObj);
                        break;
                }
            }

            return els;
        }
    }
}