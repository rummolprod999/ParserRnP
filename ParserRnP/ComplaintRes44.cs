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
                string purchaseNumber =
                    ((string) c.SelectToken("complaint.checkedObject.purchase.purchaseNumber") ?? "").Trim();
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
                    int upd = 0;
                    int idCompRes = 0;
                    if (!String.IsNullOrEmpty(createDate) &&
                        !String.IsNullOrEmpty(versionNumber))
                    {
                        string selectComp44 =
                            $"SELECT id FROM {Program.Prefix}res_complaint WHERE purchaseNumber = @purchaseNumber AND versionNumber = @versionNumber AND createDate = @createDate AND complaintNumber = @complaintNumber";
                        MySqlCommand cmd = new MySqlCommand(selectComp44, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd.Parameters.AddWithValue("@versionNumber", versionNumber);
                        cmd.Parameters.AddWithValue("@createDate", createDate);
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idCompRes = reader.GetInt32("id");
                            reader.Close();
                            upd = 1;
                        }
                        reader.Close();
                    }
                    string decisionText = ((string) c.SelectToken("complaint.decision.decisionText") ?? "").Trim();
                    string complaintResult = ((string) c.SelectToken("complaint.complaintResult") ?? "").Trim();
                    string complaintResultInfo = ((string) c.SelectToken("complaint.complaintResultInfo") ?? "").Trim();
                    string printForm = ((string) c.SelectToken("printForm.url") ?? "").Trim();
                    if (upd == 1)
                    {
                        string deleteAtt =
                            $"DELETE FROM {Program.Prefix}attach_complaint_res WHERE id_complaint_res = @id_complaint_res";
                        MySqlCommand cmd0 = new MySqlCommand(deleteAtt, connect);
                        cmd0.Prepare();
                        cmd0.Parameters.AddWithValue("@id_complaint_res", idCompRes);
                        cmd0.ExecuteNonQuery();
                        string updateC =
                            $"UPDATE {Program.Prefix}res_complaint SET checkResultNumber = @checkResultNumber, complaintNumber = @complaintNumber, versionNumber = @versionNumber, purchaseNumber = @purchaseNumber, regNumber = @regNumber, createDate = @createDate, xml = @xml, printForm = @printForm, decisionText = @decisionText, complaintResult = @complaintResult, complaintResultInfo = @complaintResultInfo WHERE id = @id";
                        MySqlCommand cmd1 = new MySqlCommand(updateC, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@checkResultNumber", checkResultNumber);
                        cmd1.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        cmd1.Parameters.AddWithValue("@versionNumber", versionNumber);
                        cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd1.Parameters.AddWithValue("@regNumber", regNumber);
                        cmd1.Parameters.AddWithValue("@createDate", createDate);
                        cmd1.Parameters.AddWithValue("@printForm", printForm);
                        cmd1.Parameters.AddWithValue("@xml", xml);
                        cmd1.Parameters.AddWithValue("@decisionText", decisionText);
                        cmd1.Parameters.AddWithValue("@complaintResult", complaintResult);
                        cmd1.Parameters.AddWithValue("@complaintResultInfo", complaintResultInfo);
                        cmd1.Parameters.AddWithValue("@id", idCompRes);
                        int resUpdComp = cmd1.ExecuteNonQuery();
                        UpdateComplaintRes44?.Invoke(resUpdComp);
                    }
                    else
                    {
                        string InsertC =
                            $"INSERT INTO {Program.Prefix}res_complaint SET checkResultNumber = @checkResultNumber, complaintNumber = @complaintNumber, versionNumber = @versionNumber, purchaseNumber = @purchaseNumber, regNumber = @regNumber, createDate = @createDate, xml = @xml, printForm = @printForm, decisionText = @decisionText, complaintResult = @complaintResult, complaintResultInfo = @complaintResultInfo";
                        MySqlCommand cmd1 = new MySqlCommand(InsertC, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@checkResultNumber", checkResultNumber);
                        cmd1.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        cmd1.Parameters.AddWithValue("@versionNumber", versionNumber);
                        cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd1.Parameters.AddWithValue("@regNumber", regNumber);
                        cmd1.Parameters.AddWithValue("@createDate", createDate);
                        cmd1.Parameters.AddWithValue("@printForm", printForm);
                        cmd1.Parameters.AddWithValue("@xml", xml);
                        cmd1.Parameters.AddWithValue("@decisionText", decisionText);
                        cmd1.Parameters.AddWithValue("@complaintResult", complaintResult);
                        cmd1.Parameters.AddWithValue("@complaintResultInfo", complaintResultInfo);
                        int resInsComp = cmd1.ExecuteNonQuery();
                        AddComplaintRes44?.Invoke(resInsComp);
                    }
                }
            }
            else
            {
                Log.Logger("Не могу найти тег checkResult", FilePath);
            }
        }
    }
}