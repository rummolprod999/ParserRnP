using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ParserRnP
{
    public class Unfair44 : Unfair
    {
        public event Action<int> AddUnfair44;

        public Unfair44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddUnfair44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddRnp++;
                else
                    Log.Logger("Не удалось добавить Unfair44", file_path);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(file.ToString());
            JObject root = (JObject) t.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("unfairSupplier"));
            if (firstOrDefault != null)
            {
                JToken r = firstOrDefault.Value;
                string registryNum = ((string) r.SelectToken("registryNum") ?? "").Trim();
                if (String.IsNullOrEmpty(registryNum))
                {
                    Log.Logger("У тендера нет id", file_path);
                    return;
                }
                string publishDate = (JsonConvert.SerializeObject(r.SelectToken("publishDate") ?? "") ??
                                      "").Trim('"');
                
                
            }
            else
            {
                Log.Logger("Не могу найти тег unfairSupplier", file_path);
            }
        }
    }
}