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
    public class ComplaintSuspend : Complaint
    {
        public event Action<int> AddComplaintSuspend;

        public ComplaintSuspend(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaintSuspend += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaintSuspend++;
                else
                    Log.Logger("Не удалось добавить ComplaintSuspend", file_path);
            };
        }

        public override void Parsing()
        {
            JObject root = (JObject) t.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("tenderSuspension"));
            if (firstOrDefault != null)
            {
                JToken c = firstOrDefault.Value;
                if (c.Type == JTokenType.Array)
                {
                    List<JToken> comp = GetElements(root, "tenderSuspension");
                    if (comp.Count > 0)
                    {
                        c = comp[0];
                    }
                }
                string complaintNumber = ((string) c.SelectToken("complaintNumber") ?? "").Trim();
                string purchaseNumber = ((string) c.SelectToken("tendersInfo.purchase.purchaseNumber") ?? "").Trim();
                if (String.IsNullOrEmpty(purchaseNumber))
                {
                    Log.Logger("Нет purchaseNumber у Suspend", file_path);
                    return;
                }
                string action = ((string) c.SelectToken("action") ?? "").Trim();
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();
                    string update_comp =
                        $"UPDATE {Program.Prefix}complaint SET tender_suspend = @tender_suspend WHERE purchaseNumber = @purchaseNumber";
                    MySqlCommand cmd = new MySqlCommand(update_comp, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                    cmd.Parameters.AddWithValue("@tender_suspend", action);
                    int status = cmd.ExecuteNonQuery();
                    if (status > 0)
                    {
                        AddComplaintSuspend?.Invoke(status);
                    }
                }
            }
            else
            {
                Log.Logger("Не могу найти тег suspend", file_path);
            }
        }
    }
}