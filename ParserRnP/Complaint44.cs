using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    public class Complaint44 : Complaint
    {
        public event Action<int> AddComplaint44;

        public Complaint44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaint44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaint++;
                else
                    Log.Logger("Не удалось добавить Complaint44", file_path);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(file.ToString());
            string id_complaint = "";
            string[] f_n = file.Name.Split('_');
            if (f_n.Length > 1)
            {
                id_complaint = f_n[1];
            }
            else
            {
                Log.Logger("Not id_complaint", file_path);
            }
            //Console.WriteLine(id_complaint);
            JObject root = (JObject) t.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("complaint"));
            if (firstOrDefault != null)
            {
                JToken c = firstOrDefault.Value;
                string complaintNumber = ((string) c.SelectToken("commonInfo.complaintNumber") ?? "").Trim();
                string versionNumber = ((string) c.SelectToken("commonInfo.versionNumber") ?? "").Trim();
                //Console.WriteLine(complaintNumber);
                
            }
            else
            {
                Log.Logger("Не могу найти тег complaint", file_path);
            }
        }
    }
}