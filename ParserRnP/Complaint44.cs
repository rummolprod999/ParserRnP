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
        public event Action<int> UpdateComplaint44;

        public Complaint44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaint44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaint++;
                else
                    Log.Logger("Не удалось добавить Complaint44", FilePath);
            };
            UpdateComplaint44 += delegate(int d)
            {
                if (d > 0)
                    Program.UpdateComplaint++;
                else
                    Log.Logger("Не удалось обновить Complaint44", FilePath);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(File.ToString());
            string idComplaint = "";
            string[] fN = File.Name.Split('_');
            if (fN.Length > 1)
            {
                idComplaint = fN[1];
            }
            else
            {
                Log.Logger("Not id_complaint", FilePath);
            }
            //Console.WriteLine(id_complaint);
            JObject root = (JObject) T.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("complaint"));
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

                string complaintNumber = ((string) c.SelectToken("commonInfo.complaintNumber") ?? "").Trim();
                //Console.WriteLine(complaintNumber);
                string regNumber = ((string) c.SelectToken("commonInfo.regNumber") ?? "").Trim();
                string versionNumber = ((string) c.SelectToken("commonInfo.versionNumber") ?? "").Trim();
                string purchaseNumber = ((string) c.SelectToken("object.purchase.purchaseNumber") ?? "").Trim();
                string planDecisionDate =
                (JsonConvert.SerializeObject(c.SelectToken("commonInfo.planDecisionDate") ?? "") ??
                 "").Trim('"');
                if (String.IsNullOrEmpty(purchaseNumber) && String.IsNullOrEmpty(complaintNumber))
                {
                    Log.Logger("Нет purchaseNumber and complaintNumber", FilePath);
                    return;
                }
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();

                    int GetIdOrg(string regNum, string fullName, string inn, string kpp, string table = "org_complaint")
                    {
                        int idO = 0;
                        string selectOrg = $"SELECT id FROM {Program.Prefix}{table} WHERE regNum = @regNum";
                        MySqlCommand cmd = new MySqlCommand(selectOrg, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNum", regNum);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idO = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            string insertOrg =
                                $"INSERT INTO {Program.Prefix}{table} SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP";
                            MySqlCommand cmd2 = new MySqlCommand(insertOrg, connect);
                            cmd2.Prepare();
                            cmd2.Parameters.AddWithValue("@regNum", regNum);
                            cmd2.Parameters.AddWithValue("@fullName", fullName);
                            cmd2.Parameters.AddWithValue("@INN", inn);
                            cmd2.Parameters.AddWithValue("@KPP", kpp);
                            cmd2.ExecuteNonQuery();
                            idO = (int) cmd2.LastInsertedId;
                        }
                        return idO;
                    }

                    int upd = 0;
                    int idComp = 0;
                    if (!String.IsNullOrEmpty(planDecisionDate) &&
                        !String.IsNullOrEmpty(versionNumber))
                    {
                        string selectComp44 =
                            $"SELECT id FROM {Program.Prefix}complaint WHERE purchaseNumber = @purchaseNumber AND versionNumber = @versionNumber AND planDecisionDate = @planDecisionDate AND complaintNumber = @complaintNumber";
                        MySqlCommand cmd = new MySqlCommand(selectComp44, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd.Parameters.AddWithValue("@versionNumber", versionNumber);
                        cmd.Parameters.AddWithValue("@planDecisionDate", planDecisionDate);
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idComp = reader.GetInt32("id");
                            reader.Close();
                            upd = 1;
                        }
                        reader.Close();
                        string decisionPlace = ((string) c.SelectToken("commonInfo.decisionPlace") ?? "").Trim();
                        int idRegistrationKo = 0;
                        string regNumRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.regNum") ?? "").Trim();
                        string fullNameRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.fullName") ?? "").Trim();
                        string innRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.INN") ?? "").Trim();
                        string kppRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumRegistrationKo))
                        {
                            idRegistrationKo = GetIdOrg(regNumRegistrationKo, fullNameRegistrationKo,
                                innRegistrationKo, kppRegistrationKo);
                        }
                        int idConsiderationKo = 0;
                        string regNumConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.regNum") ?? "").Trim();
                        string fullNameConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.fullName") ?? "").Trim();
                        string innConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.INN") ?? "").Trim();
                        string kppConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumConsiderationKo))
                        {
                            idConsiderationKo = GetIdOrg(regNumConsiderationKo, fullNameConsiderationKo,
                                innConsiderationKo, kppConsiderationKo);
                        }

                        string regDate = (JsonConvert.SerializeObject(c.SelectToken("commonInfo.regDate") ?? "") ??
                                          "").Trim('"');
                        string noticeNumber = ((string) c.SelectToken("commonInfo.notice.number") ?? "").Trim();
                        string noticeAcceptDate =
                        (JsonConvert.SerializeObject(c.SelectToken("commonInfo.notice.acceptDate") ?? "") ??
                         "").Trim('"');
                        int idCreateOrganization = 0;
                        string regNumCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.regNum") ?? "").Trim();
                        string fullNameCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.fullName") ?? "")
                            .Trim();
                        string innCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.INN") ?? "").Trim();
                        string kppCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumCreateOrganization))
                        {
                            idCreateOrganization = GetIdOrg(regNumCreateOrganization, fullNameCreateOrganization,
                                innCreateOrganization, kppCreateOrganization);
                        }
                        string createDate =
                        (JsonConvert.SerializeObject(c.SelectToken("commonInfo.printFormInfo.createDate") ?? "") ??
                         "").Trim('"');
                        int idPublishOrganization = 0;
                        string regNumPublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.regNum") ?? "")
                            .Trim();
                        string fullNamePublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.fullName") ?? "")
                            .Trim();
                        string innPublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.INN") ?? "").Trim();
                        string kppPublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumCreateOrganization))
                        {
                            idPublishOrganization = GetIdOrg(regNumPublishOrganization, fullNamePublishOrganization,
                                innPublishOrganization, kppPublishOrganization);
                        }
                        int idCustomer = 0;
                        string regNumCustomer =
                            ((string) c.SelectToken("indicted.customer.regNum") ?? "")
                            .Trim();
                        if (String.IsNullOrEmpty(regNumCustomer))
                        {
                            regNumCustomer =
                                ((string) c.SelectToken("indicted.customerNew.regNum") ?? "")
                                .Trim();
                        }
                        string fullNameCustomer =
                            ((string) c.SelectToken("indicted.customer.fullName") ?? "")
                            .Trim();
                        if (String.IsNullOrEmpty(fullNameCustomer))
                        {
                            fullNameCustomer = ((string) c.SelectToken("indicted.customerNew.fullName") ?? "")
                                .Trim();
                        }
                        string innCustomer =
                            ((string) c.SelectToken("indicted.customer.INN") ?? "").Trim();
                        if (String.IsNullOrEmpty(innCustomer))
                        {
                            innCustomer =
                                ((string) c.SelectToken("indicted.customerNew.INN") ?? "").Trim();
                        }
                        string kppCustomer =
                            ((string) c.SelectToken("indicted.customer.KPP") ?? "").Trim();
                        if (String.IsNullOrEmpty(kppCustomer))
                        {
                            kppCustomer =
                                ((string) c.SelectToken("indicted.customerNew.KPP") ?? "").Trim();
                        }
                        if (!String.IsNullOrEmpty(regNumCreateOrganization))
                        {
                            idCustomer = GetIdOrg(regNumCustomer, fullNameCustomer,
                                innCustomer, kppCustomer, "customer_complaint");
                        }
                        string applicantFullName =
                            ((string) c.SelectToken("applicantNew.legalEntity.fullName") ?? "").Trim();
                        if (String.IsNullOrEmpty(applicantFullName))
                        {
                            applicantFullName =
                                ((string) c.SelectToken("applicantNew.individualPerson.name") ?? "").Trim();
                        }
                        if (String.IsNullOrEmpty(applicantFullName))
                        {
                            applicantFullName =
                                ((string) c.SelectToken("applicantNew.individualBusinessman.name") ?? "").Trim();
                        }
                        if (String.IsNullOrEmpty(applicantFullName))
                        {
                            applicantFullName =
                                ((string) c.SelectToken("applicant.organizationName") ?? "").Trim();
                        }
                        string applicantInn = ((string) c.SelectToken("applicantNew.legalEntity.INN") ?? "").Trim();
                        if (String.IsNullOrEmpty(applicantInn))
                        {
                            applicantInn = ((string) c.SelectToken("applicantNew.individualPerson.INN") ?? "").Trim();
                        }
                        if (String.IsNullOrEmpty(applicantInn))
                        {
                            applicantInn = ((string) c.SelectToken("applicantNew.individualBusinessman.INN") ?? "")
                                .Trim();
                        }
                        string applicantKpp = ((string) c.SelectToken("applicantNew.legalEntity.KPP") ?? "").Trim();

                        if (String.IsNullOrEmpty(purchaseNumber))
                        {
                            purchaseNumber =
                                ((string) c.SelectToken("object.purchase.notificationNumber") ?? "").Trim();
                        }
                        string lotNumbers = "";
                        List<JToken> lotnumbersList = GetElementsLots(c, "object.purchase.lots.lotNumber");
                        lotNumbers = String.Join(",", lotnumbersList);
                        string lotsInfo = ((string) c.SelectToken("object.purchase.lots.info") ?? "").Trim();
                        string purchaseName = ((string) c.SelectToken("object.purchase.purchaseName") ?? "").Trim();
                        string purchasePlacingDate =
                        (JsonConvert.SerializeObject(
                             c.SelectToken("object.purchase.purchaseName.purchasePlacingDate") ?? "") ??
                         "").Trim('"');
                        string textComplaint = ((string) c.SelectToken("text") ?? "").Trim();
                        string returnInfobase = ((string) c.SelectToken("returnInfo.base") ?? "").Trim();
                        string returnInfo = ((string) c.SelectToken("returnInfo.decision.info") ?? "").Trim();
                        returnInfo = $"{returnInfobase} {returnInfo}".Trim();
                        string printForm = ((string) c.SelectToken("printForm.url") ?? "").Trim();
                        //int id_c = 0;
                        if (upd == 1)
                        {
                            string deleteAtt =
                                $"DELETE FROM {Program.Prefix}attach_complaint WHERE id_complaint = @id_complaint";
                            MySqlCommand cmd0 = new MySqlCommand(deleteAtt, connect);
                            cmd0.Prepare();
                            cmd0.Parameters.AddWithValue("@id_complaint", idComp);
                            cmd0.ExecuteNonQuery();
                            string updateC =
                                $"UPDATE {Program.Prefix}complaint SET id_complaint = @id_complaint, complaintNumber = @complaintNumber, versionNumber = @versionNumber, xml = @xml, planDecisionDate = @planDecisionDate, id_registrationKO = @id_registrationKO, id_considerationKO = @id_considerationKO, regDate = @regDate, notice_number = @notice_number, notice_acceptDate = @notice_acceptDate, id_createOrganization = @id_createOrganization, createDate = @createDate, id_publishOrganization = @id_publishOrganization, id_customer = @id_customer, applicant_fullName = @applicant_fullName, applicant_INN = @applicant_INN, applicant_KPP = @applicant_KPP, purchaseNumber = @purchaseNumber, lotNumbers = @lotNumbers, lots_info = @lots_info, purchaseName = @purchaseName, purchasePlacingDate = @purchasePlacingDate, text_complaint = @text_complaint, printForm = @printForm, returnInfo = @returnInfo, regNumber = @regNumber WHERE id = @id_comp";
                            MySqlCommand cmd1 = new MySqlCommand(updateC, connect);
                            cmd1.Prepare();
                            cmd1.Parameters.AddWithValue("@id_complaint", idComplaint);
                            cmd1.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                            cmd1.Parameters.AddWithValue("@versionNumber", versionNumber);
                            cmd1.Parameters.AddWithValue("@xml", xml);
                            cmd1.Parameters.AddWithValue("@planDecisionDate", planDecisionDate);
                            cmd1.Parameters.AddWithValue("@id_registrationKO", idRegistrationKo);
                            cmd1.Parameters.AddWithValue("@id_considerationKO", idConsiderationKo);
                            cmd1.Parameters.AddWithValue("@regDate", regDate);
                            cmd1.Parameters.AddWithValue("@notice_number", noticeNumber);
                            cmd1.Parameters.AddWithValue("@notice_acceptDate", noticeAcceptDate);
                            cmd1.Parameters.AddWithValue("@id_createOrganization", idCreateOrganization);
                            cmd1.Parameters.AddWithValue("@createDate", createDate);
                            cmd1.Parameters.AddWithValue("@id_publishOrganization", idPublishOrganization);
                            cmd1.Parameters.AddWithValue("@id_customer", idCustomer);
                            cmd1.Parameters.AddWithValue("@applicant_fullName", applicantFullName);
                            cmd1.Parameters.AddWithValue("@applicant_INN", applicantInn);
                            cmd1.Parameters.AddWithValue("@applicant_KPP", applicantKpp);
                            cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                            cmd1.Parameters.AddWithValue("@lotNumbers", lotNumbers);
                            cmd1.Parameters.AddWithValue("@lots_info", lotsInfo);
                            cmd1.Parameters.AddWithValue("@purchaseName", purchaseName);
                            cmd1.Parameters.AddWithValue("@purchasePlacingDate", purchasePlacingDate);
                            cmd1.Parameters.AddWithValue("@text_complaint", textComplaint);
                            cmd1.Parameters.AddWithValue("@printForm", printForm);
                            cmd1.Parameters.AddWithValue("@id_comp", idComp);
                            cmd1.Parameters.AddWithValue("@returnInfo", returnInfo);
                            cmd1.Parameters.AddWithValue("@regNumber", regNumber);
                            int resUpdComp = cmd1.ExecuteNonQuery();
                            //id_c = id_comp;
                            UpdateComplaint44?.Invoke(resUpdComp);
                        }
                        else
                        {
                            string insertC =
                                $"INSERT INTO {Program.Prefix}complaint SET id_complaint = @id_complaint, complaintNumber = @complaintNumber, versionNumber = @versionNumber, xml = @xml, planDecisionDate = @planDecisionDate, id_registrationKO = @id_registrationKO, id_considerationKO = @id_considerationKO, regDate = @regDate, notice_number = @notice_number, notice_acceptDate = @notice_acceptDate, id_createOrganization = @id_createOrganization, createDate = @createDate, id_publishOrganization = @id_publishOrganization, id_customer = @id_customer, applicant_fullName = @applicant_fullName, applicant_INN = @applicant_INN, applicant_KPP = @applicant_KPP, purchaseNumber = @purchaseNumber, lotNumbers = @lotNumbers, lots_info = @lots_info, purchaseName = @purchaseName, purchasePlacingDate = @purchasePlacingDate, text_complaint = @text_complaint, printForm = @printForm, returnInfo = @returnInfo, regNumber = @regNumber";
                            MySqlCommand cmd1 = new MySqlCommand(insertC, connect);
                            cmd1.Prepare();
                            cmd1.Parameters.AddWithValue("@id_complaint", idComplaint);
                            cmd1.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                            cmd1.Parameters.AddWithValue("@versionNumber", versionNumber);
                            cmd1.Parameters.AddWithValue("@xml", xml);
                            cmd1.Parameters.AddWithValue("@planDecisionDate", planDecisionDate);
                            cmd1.Parameters.AddWithValue("@id_registrationKO", idRegistrationKo);
                            cmd1.Parameters.AddWithValue("@id_considerationKO", idConsiderationKo);
                            cmd1.Parameters.AddWithValue("@regDate", regDate);
                            cmd1.Parameters.AddWithValue("@notice_number", noticeNumber);
                            cmd1.Parameters.AddWithValue("@notice_acceptDate", noticeAcceptDate);
                            cmd1.Parameters.AddWithValue("@id_createOrganization", idCreateOrganization);
                            cmd1.Parameters.AddWithValue("@createDate", createDate);
                            cmd1.Parameters.AddWithValue("@id_publishOrganization", idPublishOrganization);
                            cmd1.Parameters.AddWithValue("@id_customer", idCustomer);
                            cmd1.Parameters.AddWithValue("@applicant_fullName", applicantFullName);
                            cmd1.Parameters.AddWithValue("@applicant_INN", applicantInn);
                            cmd1.Parameters.AddWithValue("@applicant_KPP", applicantKpp);
                            cmd1.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                            cmd1.Parameters.AddWithValue("@lotNumbers", lotNumbers);
                            cmd1.Parameters.AddWithValue("@lots_info", lotsInfo);
                            cmd1.Parameters.AddWithValue("@purchaseName", purchaseName);
                            cmd1.Parameters.AddWithValue("@purchasePlacingDate", purchasePlacingDate);
                            cmd1.Parameters.AddWithValue("@text_complaint", textComplaint);
                            cmd1.Parameters.AddWithValue("@printForm", printForm);
                            cmd1.Parameters.AddWithValue("@returnInfo", returnInfo);
                            cmd1.Parameters.AddWithValue("@regNumber", regNumber);
                            int resInsertComp = cmd1.ExecuteNonQuery();
                            idComp = (int) cmd1.LastInsertedId;
                            AddComplaint44?.Invoke(resInsertComp);
                        }
                        List<JToken> attach =
                            GetElements(c, "attachments.attachment");
                        foreach (var att in attach)
                        {
                            string publishedContentId = ((string) att.SelectToken("publishedContentId") ?? "").Trim();
                            string fileName = ((string) att.SelectToken("fileName") ?? "").Trim();
                            string docDescription = ((string) att.SelectToken("docDescription") ?? "").Trim();
                            string url = ((string) att.SelectToken("url") ?? "").Trim();
                            string insertAtt =
                                $"INSERT INTO {Program.Prefix}attach_complaint SET id_complaint = @id_complaint, publishedContentId = @publishedContentId, fileName = @fileName, docDescription = @docDescription, url = @url";
                            MySqlCommand cmd1 = new MySqlCommand(insertAtt, connect);
                            cmd1.Prepare();
                            cmd1.Parameters.AddWithValue("@id_complaint", idComp);
                            cmd1.Parameters.AddWithValue("@publishedContentId", publishedContentId);
                            cmd1.Parameters.AddWithValue("@fileName", fileName);
                            cmd1.Parameters.AddWithValue("@docDescription", docDescription);
                            cmd1.Parameters.AddWithValue("@url", url);
                            cmd1.ExecuteNonQuery();
                        }
                    }
                }
            }
            else
            {
                Log.Logger("Не могу найти тег complaint", FilePath);
            }
        }
    }
}