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
                    Log.Logger("Не удалось добавить ComplaintSuspend", FilePath);
            };
        }

        public override void Parsing()
        {
            JObject root = (JObject) T.SelectToken("export");
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
                    purchaseNumber = ((string) c.SelectToken("tendersInfo.order.notificationNumber") ?? "").Trim();
                }
                if (String.IsNullOrEmpty(purchaseNumber))
                {
                    Log.Logger("Нет purchaseNumber у Suspend", FilePath);
                    return;
                }
                string action = ((string) c.SelectToken("action") ?? "").Trim();
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    string updateComp =
                        $"UPDATE {Program.Prefix}complaint SET tender_suspend = @tender_suspend WHERE purchaseNumber = @purchaseNumber";
                    MySqlCommand cmd = new MySqlCommand(updateComp, connect);
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
                Log.Logger("Не могу найти тег suspend", FilePath);
            }
        }
    }
}