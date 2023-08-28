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
            var xml = GetXml(File.ToString());
            var idComplaint = "";
            var fN = File.Name.Split('_');
            if (fN.Length > 1)
            {
                idComplaint = fN[1];
            }
            else
            {
                Log.Logger("Not id_complaint", FilePath);
            }
            //Console.WriteLine(id_complaint);
            var root = (JObject) T.SelectToken("export");
            var firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("complaint"));
            if (firstOrDefault != null)
            {
                var c = firstOrDefault.Value;
                //Console.WriteLine(c.Type);
                if (c.Type == JTokenType.Array)
                {
                    var comp = GetElements(root, "complaint");
                    if (comp.Count > 0)
                    {
                        c = comp[0];
                    }
                }

                var complaintNumber = ((string) c.SelectToken("commonInfo.complaintNumber") ?? "").Trim();
                //Console.WriteLine(complaintNumber);
                var regNumber = ((string) c.SelectToken("commonInfo.regNumber") ?? "").Trim();
                var versionNumber = ((string) c.SelectToken("commonInfo.versionNumber") ?? "").Trim();
                var purchaseNumber = ((string) c.SelectToken("object.purchase.purchaseNumber") ?? "").Trim();
                var planDecisionDate =
                (JsonConvert.SerializeObject(c.SelectToken("commonInfo.planDecisionDate") ?? "") ??
                 "").Trim('"');
                if (String.IsNullOrEmpty(purchaseNumber) && String.IsNullOrEmpty(complaintNumber))
                {
                    Log.Logger("Нет purchaseNumber and complaintNumber", FilePath);
                    return;
                }
                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();

                    int GetIdOrg(string regNum, string fullName, string inn, string kpp, string table = "org_complaint")
                    {
                        var idO = 0;
                        var selectOrg = $"SELECT id FROM {Program.Prefix}{table} WHERE regNum = @regNum";
                        var cmd = new MySqlCommand(selectOrg, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNum", regNum);
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idO = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            var insertOrg =
                                $"INSERT INTO {Program.Prefix}{table} SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP";
                            var cmd2 = new MySqlCommand(insertOrg, connect);
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

                    var upd = 0;
                    var idComp = 0;
                    if (!String.IsNullOrEmpty(planDecisionDate) &&
                        !String.IsNullOrEmpty(versionNumber))
                    {
                        planDecisionDate = planDecisionDate.Substring(0, 19);
                        var selectComp44 =
                            $"SELECT id FROM {Program.Prefix}complaint WHERE purchaseNumber = @purchaseNumber AND versionNumber = @versionNumber AND planDecisionDate = @planDecisionDate AND complaintNumber = @complaintNumber";
                        var cmd = new MySqlCommand(selectComp44, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                        cmd.Parameters.AddWithValue("@versionNumber", versionNumber);
                        cmd.Parameters.AddWithValue("@planDecisionDate", planDecisionDate);
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idComp = reader.GetInt32("id");
                            reader.Close();
                            upd = 1;
                        }
                        reader.Close();
                    }
                    var decisionPlace = ((string) c.SelectToken("commonInfo.decisionPlace") ?? "").Trim();
                        var idRegistrationKo = 0;
                        var regNumRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.regNum") ?? "").Trim();
                        var fullNameRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.fullName") ?? "").Trim();
                        var innRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.INN") ?? "").Trim();
                        var kppRegistrationKo =
                            ((string) c.SelectToken("commonInfo.registrationKO.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumRegistrationKo))
                        {
                            idRegistrationKo = GetIdOrg(regNumRegistrationKo, fullNameRegistrationKo,
                                innRegistrationKo, kppRegistrationKo);
                        }
                        var idConsiderationKo = 0;
                        var regNumConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.regNum") ?? "").Trim();
                        var fullNameConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.fullName") ?? "").Trim();
                        var innConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.INN") ?? "").Trim();
                        var kppConsiderationKo =
                            ((string) c.SelectToken("commonInfo.considerationKO.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumConsiderationKo))
                        {
                            idConsiderationKo = GetIdOrg(regNumConsiderationKo, fullNameConsiderationKo,
                                innConsiderationKo, kppConsiderationKo);
                        }

                        var regDate = (JsonConvert.SerializeObject(c.SelectToken("commonInfo.regDate") ?? "") ??
                                       "").Trim('"');
                        var noticeNumber = ((string) c.SelectToken("commonInfo.notice.number") ?? "").Trim();
                        var noticeAcceptDate =
                        (JsonConvert.SerializeObject(c.SelectToken("commonInfo.notice.acceptDate") ?? "") ??
                         "").Trim('"');
                        var idCreateOrganization = 0;
                        var regNumCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.regNum") ?? "").Trim();
                        var fullNameCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.fullName") ?? "")
                            .Trim();
                        var innCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.INN") ?? "").Trim();
                        var kppCreateOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.createOrganization.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumCreateOrganization))
                        {
                            idCreateOrganization = GetIdOrg(regNumCreateOrganization, fullNameCreateOrganization,
                                innCreateOrganization, kppCreateOrganization);
                        }
                        var createDate =
                        (JsonConvert.SerializeObject(c.SelectToken("commonInfo.printFormInfo.createDate") ?? "") ??
                         "").Trim('"');
                        var idPublishOrganization = 0;
                        var regNumPublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.regNum") ?? "")
                            .Trim();
                        var fullNamePublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.fullName") ?? "")
                            .Trim();
                        var innPublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.INN") ?? "").Trim();
                        var kppPublishOrganization =
                            ((string) c.SelectToken("commonInfo.printFormInfo.publishOrganization.KPP") ?? "").Trim();
                        if (!String.IsNullOrEmpty(regNumCreateOrganization))
                        {
                            idPublishOrganization = GetIdOrg(regNumPublishOrganization, fullNamePublishOrganization,
                                innPublishOrganization, kppPublishOrganization);
                        }
                        var idCustomer = 0;
                        var regNumCustomer =
                            ((string) c.SelectToken("indicted.customer.regNum") ?? "")
                            .Trim();
                        if (String.IsNullOrEmpty(regNumCustomer))
                        {
                            regNumCustomer =
                                ((string) c.SelectToken("indicted.customerNew.regNum") ?? "")
                                .Trim();
                        }
                        var fullNameCustomer =
                            ((string) c.SelectToken("indicted.customer.fullName") ?? "")
                            .Trim();
                        if (String.IsNullOrEmpty(fullNameCustomer))
                        {
                            fullNameCustomer = ((string) c.SelectToken("indicted.customerNew.fullName") ?? "")
                                .Trim();
                        }
                        var innCustomer =
                            ((string) c.SelectToken("indicted.customer.INN") ?? "").Trim();
                        if (String.IsNullOrEmpty(innCustomer))
                        {
                            innCustomer =
                                ((string) c.SelectToken("indicted.customerNew.INN") ?? "").Trim();
                        }
                        var kppCustomer =
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
                        var applicantFullName =
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
                        var applicantInn = ((string) c.SelectToken("applicantNew.legalEntity.INN") ?? "").Trim();
                        if (String.IsNullOrEmpty(applicantInn))
                        {
                            applicantInn = ((string) c.SelectToken("applicantNew.individualPerson.INN") ?? "").Trim();
                        }
                        if (String.IsNullOrEmpty(applicantInn))
                        {
                            applicantInn = ((string) c.SelectToken("applicantNew.individualBusinessman.INN") ?? "")
                                .Trim();
                        }
                        var applicantKpp = ((string) c.SelectToken("applicantNew.legalEntity.KPP") ?? "").Trim();

                        if (String.IsNullOrEmpty(purchaseNumber))
                        {
                            purchaseNumber =
                                ((string) c.SelectToken("object.purchase.notificationNumber") ?? "").Trim();
                        }
                        var lotNumbers = "";
                        var lotnumbersList = GetElementsLots(c, "object.purchase.lots.lotNumber");
                        lotNumbers = String.Join(",", lotnumbersList);
                        var lotsInfo = ((string) c.SelectToken("object.purchase.lots.info") ?? "").Trim();
                        var purchaseName = ((string) c.SelectToken("object.purchase.purchaseName") ?? "").Trim();
                        var purchasePlacingDate =
                        (JsonConvert.SerializeObject(
                             c.SelectToken("object.purchase.purchaseName.purchasePlacingDate") ?? "") ??
                         "").Trim('"');
                        var textComplaint = ((string) c.SelectToken("text") ?? "").Trim();
                        var returnInfobase = ((string) c.SelectToken("returnInfo.base") ?? "").Trim();
                        var returnInfo = ((string) c.SelectToken("returnInfo.decision.info") ?? "").Trim();
                        returnInfo = $"{returnInfobase} {returnInfo}".Trim();
                        var printForm = ((string) c.SelectToken("printForm.url") ?? "").Trim();
                        //int id_c = 0;
                        if (upd == 1)
                        {
                            var deleteAtt =
                                $"DELETE FROM {Program.Prefix}attach_complaint WHERE id_complaint = @id_complaint";
                            var cmd0 = new MySqlCommand(deleteAtt, connect);
                            cmd0.Prepare();
                            cmd0.Parameters.AddWithValue("@id_complaint", idComp);
                            cmd0.ExecuteNonQuery();
                            var updateC =
                                $"UPDATE {Program.Prefix}complaint SET id_complaint = @id_complaint, complaintNumber = @complaintNumber, versionNumber = @versionNumber, xml = @xml, planDecisionDate = @planDecisionDate, id_registrationKO = @id_registrationKO, id_considerationKO = @id_considerationKO, regDate = @regDate, notice_number = @notice_number, notice_acceptDate = @notice_acceptDate, id_createOrganization = @id_createOrganization, createDate = @createDate, id_publishOrganization = @id_publishOrganization, id_customer = @id_customer, applicant_fullName = @applicant_fullName, applicant_INN = @applicant_INN, applicant_KPP = @applicant_KPP, purchaseNumber = @purchaseNumber, lotNumbers = @lotNumbers, lots_info = @lots_info, purchaseName = @purchaseName, purchasePlacingDate = @purchasePlacingDate, text_complaint = @text_complaint, printForm = @printForm, returnInfo = @returnInfo, regNumber = @regNumber WHERE id = @id_comp";
                            var cmd1 = new MySqlCommand(updateC, connect);
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
                            var resUpdComp = cmd1.ExecuteNonQuery();
                            //id_c = id_comp;
                            UpdateComplaint44?.Invoke(resUpdComp);
                        }
                        else
                        {
                            var insertC =
                                $"INSERT INTO {Program.Prefix}complaint SET id_complaint = @id_complaint, complaintNumber = @complaintNumber, versionNumber = @versionNumber, xml = @xml, planDecisionDate = @planDecisionDate, id_registrationKO = @id_registrationKO, id_considerationKO = @id_considerationKO, regDate = @regDate, notice_number = @notice_number, notice_acceptDate = @notice_acceptDate, id_createOrganization = @id_createOrganization, createDate = @createDate, id_publishOrganization = @id_publishOrganization, id_customer = @id_customer, applicant_fullName = @applicant_fullName, applicant_INN = @applicant_INN, applicant_KPP = @applicant_KPP, purchaseNumber = @purchaseNumber, lotNumbers = @lotNumbers, lots_info = @lots_info, purchaseName = @purchaseName, purchasePlacingDate = @purchasePlacingDate, text_complaint = @text_complaint, printForm = @printForm, returnInfo = @returnInfo, regNumber = @regNumber";
                            var cmd1 = new MySqlCommand(insertC, connect);
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
                            var resInsertComp = cmd1.ExecuteNonQuery();
                            idComp = (int) cmd1.LastInsertedId;
                            AddComplaint44?.Invoke(resInsertComp);
                        }
                        var attach =
                            GetElements(c, "attachments.attachment");
                        foreach (var att in attach)
                        {
                            var publishedContentId = ((string) att.SelectToken("publishedContentId") ?? "").Trim();
                            var fileName = ((string) att.SelectToken("fileName") ?? "").Trim();
                            var docDescription = ((string) att.SelectToken("docDescription") ?? "").Trim();
                            var url = ((string) att.SelectToken("url") ?? "").Trim();
                            var insertAtt =
                                $"INSERT INTO {Program.Prefix}attach_complaint SET id_complaint = @id_complaint, publishedContentId = @publishedContentId, fileName = @fileName, docDescription = @docDescription, url = @url";
                            var cmd1 = new MySqlCommand(insertAtt, connect);
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
            else
            {
                Log.Logger("Не могу найти тег complaint", FilePath);
            }
        }
    }
}