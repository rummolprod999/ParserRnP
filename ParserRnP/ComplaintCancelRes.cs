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
    public class ComplaintCancelRes : Complaint
    {
        public event Action<int> AddComplaintCancelRes;
        public event Action<int> UpdateComplaintCancelRes;

        public ComplaintCancelRes(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaintCancelRes += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaintCancelResult++;
                else
                    Log.Logger("Не удалось добавить ComplaintCancelRes", FilePath);
            };
            
            UpdateComplaintCancelRes += delegate(int d)
            {
                if (d > 0)
                    Program.UpdateComplaintCancelResult++;
                else
                    Log.Logger("Не удалось обновить ComplaintCancelRes", FilePath);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(File.ToString());
            JObject root = (JObject) T.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("checkResultCancel"));
            if (firstOrDefault != null)
            {
                JToken c = firstOrDefault.Value;
                //Console.WriteLine(c.Type);
                if (c.Type == JTokenType.Array)
                {
                    List<JToken> comp = GetElements(root, "checkResultCancel");
                    if (comp.Count > 0)
                    {
                        c = comp[0];
                    }
                }
                string checkResultNumber = ((string) c.SelectToken("commonInfo.checkResultNumber") ?? "").Trim();
                //Console.WriteLine(checkResultNumber);
                string complaintNumber = ((string) c.SelectToken("complaint.complaintNumber") ?? "").Trim();
                //Console.WriteLine(complaintNumber);
                string regNumber = ((string) c.SelectToken("commonInfo.regNumber") ?? "").Trim();
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
                    if (!String.IsNullOrEmpty(createDate))
                    {
                        createDate = createDate.Substring(0, 19);
                        string selectComp44 =
                            $"SELECT id FROM {Program.Prefix}res_complaint WHERE purchaseNumber = @purchaseNumber AND createDate = @createDate AND complaintNumber = @complaintNumber";
                        MySqlCommand cmd = new MySqlCommand(selectComp44, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
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
                            $"UPDATE {Program.Prefix}res_complaint SET checkResultNumber = @checkResultNumber, complaintNumber = @complaintNumber, purchaseNumber = @purchaseNumber, regNumber = @regNumber, createDate = @createDate, xml = @xml, printForm = @printForm, decisionText = @decisionText, complaintResult = @complaintResult, complaintResultInfo = @complaintResultInfo, cancel = 1 WHERE id = @id";
                        MySqlCommand cmd1 = new MySqlCommand(updateC, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@checkResultNumber", checkResultNumber);
                        cmd1.Parameters.AddWithValue("@complaintNumber", complaintNumber);
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
                        UpdateComplaintCancelRes?.Invoke(resUpdComp);
                    }
                    else
                    {
                        string InsertC =
                            $"INSERT INTO {Program.Prefix}res_complaint SET checkResultNumber = @checkResultNumber, complaintNumber = @complaintNumber, purchaseNumber = @purchaseNumber, regNumber = @regNumber, createDate = @createDate, xml = @xml, printForm = @printForm, decisionText = @decisionText, complaintResult = @complaintResult, complaintResultInfo = @complaintResultInfo, cancel = 1";
                        MySqlCommand cmd1 = new MySqlCommand(InsertC, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@checkResultNumber", checkResultNumber);
                        cmd1.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd1.Parameters.AddWithValue("@regNumber", regNumber);
                        cmd1.Parameters.AddWithValue("@createDate", createDate);
                        cmd1.Parameters.AddWithValue("@printForm", printForm);
                        cmd1.Parameters.AddWithValue("@xml", xml);
                        cmd1.Parameters.AddWithValue("@decisionText", decisionText);
                        cmd1.Parameters.AddWithValue("@complaintResult", complaintResult);
                        cmd1.Parameters.AddWithValue("@complaintResultInfo", complaintResultInfo);
                        int resInsComp = cmd1.ExecuteNonQuery();
                        idCompRes = (int) cmd1.LastInsertedId;
                        AddComplaintCancelRes?.Invoke(resInsComp);
                    }
                    List<JToken> attach =
                        GetElements(c, "complaint.decision.attachments.attachment");
                    foreach (var att in attach)
                    {
                        string fileName = ((string) att.SelectToken("fileName") ?? "").Trim();
                        string docDescription = ((string) att.SelectToken("docDescription") ?? "").Trim();
                        string url = ((string) att.SelectToken("url") ?? "").Trim();
                        string insertAtt =
                            $"INSERT INTO {Program.Prefix}attach_complaint_res SET id_complaint_res = @id_complaint_res, fileName = @fileName, docDescription = @docDescription, url = @url";
                        MySqlCommand cmd1 = new MySqlCommand(insertAtt, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@id_complaint_res", idCompRes);
                        cmd1.Parameters.AddWithValue("@fileName", fileName);
                        cmd1.Parameters.AddWithValue("@docDescription", docDescription);
                        cmd1.Parameters.AddWithValue("@url", url);
                        cmd1.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                Log.Logger("Не могу найти тег checkResultCancel", FilePath);
            }
        }
    }
}