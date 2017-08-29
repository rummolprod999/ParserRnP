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
    public class ComplaintCancel : Complaint
    {
        public event Action<int> AddComplaintCancel;

        public ComplaintCancel(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaintCancel += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaintCancel++;
                else
                    Log.Logger("Не удалось добавить ComplaintCancel", FilePath);
            };
        }

        public override void Parsing()
        {
            JObject root = (JObject) T.SelectToken("export");
            List<JToken> cancel = GetElements(root, "complaintCancel");
            if (cancel.Count > 0)
            {
                
                string complaintNumber = ((string) cancel[0].SelectToken("complaintNumber") ?? "").Trim();
                string regNumber = ((string) cancel[0].SelectToken("regNumber") ?? "").Trim();
                if ((String.IsNullOrEmpty(complaintNumber) || complaintNumber.Length < 3) && String.IsNullOrEmpty(regNumber))
                {
                    Log.Logger("Нет complaintNumber and regNumber у Cancel", FilePath, complaintNumber);
                    return;
                }
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    if (!String.IsNullOrEmpty(complaintNumber) && complaintNumber.Length >= 3)
                    {
                        string updateComp = $"UPDATE {Program.Prefix}complaint SET cancel = 1 WHERE complaintNumber = @complaintNumber";
                        MySqlCommand cmd = new MySqlCommand(updateComp, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        int status = cmd.ExecuteNonQuery();
                        if (status > 0)
                        {
                            AddComplaintCancel?.Invoke(status);
                        }
                    }
                    else if (!String.IsNullOrEmpty(regNumber))
                    {
                        string updateComp = $"UPDATE {Program.Prefix}complaint SET cancel = 1 WHERE regNumber = @regNumber";
                        MySqlCommand cmd = new MySqlCommand(updateComp, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNumber", regNumber);
                        int status = cmd.ExecuteNonQuery();
                        if (status > 0)
                        {
                            AddComplaintCancel?.Invoke(status);
                        }
                    }
                    
                    
                }
            }
            else
            {
                Log.Logger("Не могу найти тег cancel", FilePath);
            }
        }
    }
}