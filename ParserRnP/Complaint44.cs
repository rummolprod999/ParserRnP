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
                string planDecisionDate =
                (JsonConvert.SerializeObject(c.SelectToken("commonInfo.planDecisionDate") ?? "") ??
                 "").Trim('"');
                if (String.IsNullOrEmpty(planDecisionDate))
                {
                    Log.Logger("Нет planDecisionDate", file_path);
                }
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();

                    int GetIDOrg(string regNum, string fullName, string INN, string KPP, string table = "org_complaint")
                    {
                        int id_o = 0;
                        string select_org = $"SELECT id FROM {Program.Prefix}{table} WHERE regNum = @regNum";
                        MySqlCommand cmd = new MySqlCommand(select_org, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNum", regNum);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            id_o = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            string insert_org =
                                $"INSERT INTO {Program.Prefix}{table} SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP";
                            MySqlCommand cmd2 = new MySqlCommand(insert_org, connect);
                            cmd2.Prepare();
                            cmd2.Parameters.AddWithValue("@regNum", regNum);
                            cmd2.Parameters.AddWithValue("@fullName", fullName);
                            cmd2.Parameters.AddWithValue("@INN", INN);
                            cmd2.Parameters.AddWithValue("@KPP", KPP);
                            cmd2.ExecuteNonQuery();
                            id_o = (int) cmd2.LastInsertedId;
                        }
                        return id_o;
                    }

                    int upd = 0;
                    int id_comp = 0;
                    if (!String.IsNullOrEmpty(complaintNumber) && !String.IsNullOrEmpty(planDecisionDate) &&
                        !String.IsNullOrEmpty(versionNumber))
                    {
                        string select_comp44 =
                            $"SELECT id FROM {Program.Prefix}complaint WHERE complaintNumber = @complaintNumber AND versionNumber = @versionNumber AND planDecisionDate = @planDecisionDate";
                        MySqlCommand cmd = new MySqlCommand(select_comp44, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        cmd.Parameters.AddWithValue("@versionNumber", versionNumber);
                        cmd.Parameters.AddWithValue("@planDecisionDate", planDecisionDate);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            id_comp = reader.GetInt32("id");
                            reader.Close();
                            upd = 1;
                        }
                        reader.Close();
                        string decisionPlace = ((string) c.SelectToken("commonInfo.decisionPlace") ?? "").Trim();
                        int id_registrationKO = 0;
                        string regNum_registrationKO =
                            ((string) c.SelectToken("commonInfo.registrationKO.regNum") ?? "").Trim();
                        string fullName_registrationKO =
                            ((string) c.SelectToken("commonInfo.registrationKO.fullName") ?? "").Trim();
                        string INN_registrationKO =
                            ((string) c.SelectToken("commonInfo.registrationKO.INN") ?? "").Trim();
                        string KPP_registrationKO =
                            ((string) c.SelectToken("commonInfo.registrationKO.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNum_registrationKO))
                        {
                            id_registrationKO = GetIDOrg(regNum_registrationKO, fullName_registrationKO,
                                INN_registrationKO, KPP_registrationKO);
                        }
                        int id_considerationKO = 0;
                        string regNum_considerationKO =
                            ((string) c.SelectToken("commonInfo.considerationKO.regNum") ?? "").Trim();
                        string fullName_considerationKO =
                            ((string) c.SelectToken("commonInfo.considerationKO.fullName") ?? "").Trim();
                        string INN_considerationKO =
                            ((string) c.SelectToken("commonInfo.considerationKO.INN") ?? "").Trim();
                        string KPP_considerationKO =
                            ((string) c.SelectToken("commonInfo.considerationKO.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNum_considerationKO))
                        {
                            id_considerationKO = GetIDOrg(regNum_considerationKO, fullName_considerationKO,
                                INN_considerationKO, KPP_considerationKO);
                        }

                        string regDate = (JsonConvert.SerializeObject(c.SelectToken("commonInfo.regDate") ?? "") ??
                                          "").Trim('"');
                        string notice_number = ((string) c.SelectToken("commonInfo.notice.number") ?? "").Trim();
                        string notice_acceptDate =
                        (JsonConvert.SerializeObject(c.SelectToken("commonInfo.notice.acceptDate") ?? "") ??
                         "").Trim('"');
                        int id_createOrganization = 0;
                        string regNum_createOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.regNum") ?? "").Trim();
                        string fullName_createOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.fullName") ?? "")
                            .Trim();
                        string INN_createOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.INN") ?? "").Trim();
                        string KPP_createOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNum_createOrganization))
                        {
                            id_createOrganization = GetIDOrg(regNum_createOrganization, fullName_createOrganization,
                                INN_createOrganization, KPP_createOrganization);
                        }
                        string createDate =
                        (JsonConvert.SerializeObject(c.SelectToken("commonInfo.printFormInfo.createDate") ?? "") ??
                         "").Trim('"');
                        int id_publishOrganization = 0;
                        string regNum_publishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.regNum") ?? "")
                            .Trim();
                        string fullName_publishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.fullName") ?? "")
                            .Trim();
                        string INN_publishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.INN") ?? "").Trim();
                        string KPP_publishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNum_createOrganization))
                        {
                            id_publishOrganization = GetIDOrg(regNum_publishOrganization, fullName_publishOrganization,
                                INN_publishOrganization, KPP_publishOrganization);
                        }
                        int id_customer = 0;
                        string regNum_customer =
                            ((string) c.SelectToken("indicted.customer.regNum") ?? "")
                            .Trim();
                        string fullName_customer =
                            ((string) c.SelectToken("indicted.customer.fullName") ?? "")
                            .Trim();
                        string INN_customer =
                            ((string) c.SelectToken("indicted.customer.INN") ?? "").Trim();
                        string KPP_customer =
                            ((string) c.SelectToken("indicted.customer.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNum_createOrganization))
                        {
                            id_customer = GetIDOrg(regNum_customer, fullName_customer,
                                INN_customer, KPP_customer, "org_complaint");
                        }
                        
                    }
                }
            }
            else
            {
                Log.Logger("Не могу найти тег complaint", file_path);
            }
        }
    }
}