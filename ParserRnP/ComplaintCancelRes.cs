#region

using System;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

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
                {
                    Program.AddComplaintCancelResult++;
                }
                else
                {
                    Log.Logger("Не удалось добавить ComplaintCancelRes", FilePath);
                }
            };

            UpdateComplaintCancelRes += delegate(int d)
            {
                if (d > 0)
                {
                    Program.UpdateComplaintCancelResult++;
                }
                else
                {
                    Log.Logger("Не удалось обновить ComplaintCancelRes", FilePath);
                }
            };
        }

        public override void Parsing()
        {
            var xml = GetXml(File.ToString());
            var root = (JObject)T.SelectToken("export");
            var firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("checkResultCancel"));
            if (firstOrDefault != null)
            {
                var c = firstOrDefault.Value;
                //Console.WriteLine(c.Type);
                if (c.Type == JTokenType.Array)
                {
                    var comp = GetElements(root, "checkResultCancel");
                    if (comp.Count > 0)
                    {
                        c = comp[0];
                    }
                }

                var checkResultNumber = ((string)c.SelectToken("commonInfo.checkResultNumber") ?? "").Trim();
                //Console.WriteLine(checkResultNumber);
                var complaintNumber = ((string)c.SelectToken("complaint.complaintNumber") ?? "").Trim();
                //Console.WriteLine(complaintNumber);
                var regNumber = ((string)c.SelectToken("commonInfo.regNumber") ?? "").Trim();
                var purchaseNumber =
                    ((string)c.SelectToken("complaint.checkedObject.purchase.purchaseNumber") ?? "").Trim();
                var createDate =
                    (JsonConvert.SerializeObject(c.SelectToken("commonInfo.createDate") ?? "") ??
                     "").Trim('"');
                if (string.IsNullOrEmpty(purchaseNumber) && string.IsNullOrEmpty(complaintNumber))
                    //Log.Logger("Нет purchaseNumber and complaintNumber", FilePath);
                {
                    return;
                }

                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    var upd = 0;
                    var idCompRes = 0;
                    if (!string.IsNullOrEmpty(createDate))
                    {
                        createDate = createDate.Substring(0, 19);
                        var selectComp44 =
                            $"SELECT id FROM {Program.Prefix}res_complaint WHERE purchaseNumber = @purchaseNumber AND createDate = @createDate AND complaintNumber = @complaintNumber";
                        var cmd = new MySqlCommand(selectComp44, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd.Parameters.AddWithValue("@createDate", createDate);
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idCompRes = reader.GetInt32("id");
                            reader.Close();
                            upd = 1;
                        }

                        reader.Close();
                    }

                    var decisionText = ((string)c.SelectToken("complaint.decision.decisionText") ?? "").Trim();
                    var complaintResult = ((string)c.SelectToken("complaint.complaintResult") ?? "").Trim();
                    var complaintResultInfo = ((string)c.SelectToken("complaint.complaintResultInfo") ?? "").Trim();
                    var printForm = ((string)c.SelectToken("printForm.url") ?? "").Trim();
                    if (upd == 1)
                    {
                        var deleteAtt =
                            $"DELETE FROM {Program.Prefix}attach_complaint_res WHERE id_complaint_res = @id_complaint_res";
                        var cmd0 = new MySqlCommand(deleteAtt, connect);
                        cmd0.Prepare();
                        cmd0.Parameters.AddWithValue("@id_complaint_res", idCompRes);
                        cmd0.ExecuteNonQuery();
                        var updateC =
                            $"UPDATE {Program.Prefix}res_complaint SET checkResultNumber = @checkResultNumber, complaintNumber = @complaintNumber, purchaseNumber = @purchaseNumber, regNumber = @regNumber, createDate = @createDate, xml = @xml, printForm = @printForm, decisionText = @decisionText, complaintResult = @complaintResult, complaintResultInfo = @complaintResultInfo, cancel = 1 WHERE id = @id";
                        var cmd1 = new MySqlCommand(updateC, connect);
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
                        var resUpdComp = cmd1.ExecuteNonQuery();
                        UpdateComplaintCancelRes?.Invoke(resUpdComp);
                    }
                    else
                    {
                        var InsertC =
                            $"INSERT INTO {Program.Prefix}res_complaint SET checkResultNumber = @checkResultNumber, complaintNumber = @complaintNumber, purchaseNumber = @purchaseNumber, regNumber = @regNumber, createDate = @createDate, xml = @xml, printForm = @printForm, decisionText = @decisionText, complaintResult = @complaintResult, complaintResultInfo = @complaintResultInfo, cancel = 1";
                        var cmd1 = new MySqlCommand(InsertC, connect);
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
                        var resInsComp = cmd1.ExecuteNonQuery();
                        idCompRes = (int)cmd1.LastInsertedId;
                        AddComplaintCancelRes?.Invoke(resInsComp);
                    }

                    var attach =
                        GetElements(c, "complaint.decision.attachments.attachment");
                    foreach (var att in attach)
                    {
                        var fileName = ((string)att.SelectToken("fileName") ?? "").Trim();
                        var docDescription = ((string)att.SelectToken("docDescription") ?? "").Trim();
                        var url = ((string)att.SelectToken("url") ?? "").Trim();
                        var insertAtt =
                            $"INSERT INTO {Program.Prefix}attach_complaint_res SET id_complaint_res = @id_complaint_res, fileName = @fileName, docDescription = @docDescription, url = @url";
                        var cmd1 = new MySqlCommand(insertAtt, connect);
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