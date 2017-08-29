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
    public class ComplaintRes44 : Complaint
    {
        public event Action<int> AddComplaintRes44;
        public event Action<int> UpdateComplaintRes44;
        
        public ComplaintRes44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaintRes44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaintResult++;
                else
                    Log.Logger("Не удалось добавить ComplaintRes44", FilePath);
            };
            UpdateComplaintRes44 += delegate(int d)
            {
                if (d > 0)
                    Program.UpdateComplaintResult++;
                else
                    Log.Logger("Не удалось обновить ComplaintRes44", FilePath);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(File.ToString());
            JObject root = (JObject) T.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("checkResult"));
            if (firstOrDefault != null)
            {
                JToken c = firstOrDefault.Value;
                //Console.WriteLine(c.Type);
                if (c.Type == JTokenType.Array)
                {
                    List<JToken> comp = GetElements(root, "complaint");
                    if (comp.Count > 0)
                    {
                        c = comp[0];
                    }
                }
                
                string checkResultNumber = ((string) c.SelectToken("commonInfo.checkResultNumber") ?? "").Trim();
                //Console.WriteLine(checkResultNumber);
                string complaintNumber = ((string) c.SelectToken("complaint.complaintNumber") ?? "").Trim();
                string regNumber = ((string) c.SelectToken("commonInfo.regNumber") ?? "").Trim();
                string versionNumber = ((string) c.SelectToken("commonInfo.versionNumber") ?? "").Trim();
                string purchaseNumber = ((string) c.SelectToken("checkedObject.purchase.purchaseNumber") ?? "").Trim();
                string createDate =
                (JsonConvert.SerializeObject(c.SelectToken("commonInfo.createDate") ?? "") ??
                 "").Trim('"');
                if (String.IsNullOrEmpty(purchaseNumber) && String.IsNullOrEmpty(complaintNumber))
                {
                    Log.Logger("Нет purchaseNumber and complaintNumber", FilePath);
                    return;
                }
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                }
            }
            else
            {
                Log.Logger("Не могу найти тег checkResult", FilePath);
            }
        }
    }
}