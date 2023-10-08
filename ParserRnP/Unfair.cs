﻿#region

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class Unfair
    {
        protected readonly JObject T;
        protected readonly FileInfo File;
        protected readonly string FilePath;

        public Unfair(FileInfo f, JObject json)
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
                switch (elsObj.Type)
                {
                    case JTokenType.Object:
                        els.Add(elsObj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(elsObj);
                        break;
                }

            return els;
        }
    }
}