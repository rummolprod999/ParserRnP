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
    public class Bank44 : Bank
    {
        public event Action<int> AddBank44;

        public Bank44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddBank44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddBankGuarantee++;
                else
                    Log.Logger("Не удалось добавить Bank44", file_path);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(file.ToString());
            JObject root = (JObject) t.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("bankGuarantee"));
            if (firstOrDefault != null)
            {
                JToken b = firstOrDefault.Value;
                string id_guarantee = ((string) b.SelectToken("id") ?? "").Trim();
                string regNumber = ((string) b.SelectToken("regNumber") ?? "").Trim();
                if (String.IsNullOrEmpty(regNumber))
                {
                    Log.Logger("Нет regNumber", file_path);
                }
                string docNumber = ((string) b.SelectToken("docNumber") ?? "").Trim();
                string versionNumber = ((string) b.SelectToken("versionNumber") ?? "").Trim();
                string docPublishDate = (JsonConvert.SerializeObject(b.SelectToken("docPublishDate") ?? "") ??
                                         "").Trim('"');
                if (String.IsNullOrEmpty(docPublishDate))
                {
                    Log.Logger("Нет docPublishDate", file_path);
                }
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();
                    if (!String.IsNullOrEmpty(regNumber) && !String.IsNullOrEmpty(docPublishDate))
                    {
                        string select_bank_g =
                            $"SELECT id FROM {Program.Prefix}bank_guarantee WHERE regNumber = @regNumber AND docPublishDate = @docPublishDate";
                        MySqlCommand cmd = new MySqlCommand(select_bank_g, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNumber", regNumber);
                        cmd.Parameters.AddWithValue("@docPublishDate", docPublishDate);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Close();
                            Log.Logger("Такой документ уже есть в базе", regNumber);
                            return;
                        }
                        reader.Close();
                    }
                    int id_bank = 0;
                    string BankregNum = ((string) b.SelectToken("bank.regNum") ?? "").Trim();
                    if (!String.IsNullOrEmpty(BankregNum))
                    {
                        string select_bank =
                            $"SELECT id FROM {Program.Prefix}bank WHERE regNum = @regNum";
                        MySqlCommand cmd2 = new MySqlCommand(select_bank, connect);
                        cmd2.Prepare();
                        cmd2.Parameters.AddWithValue("@regNum", BankregNum);
                        MySqlDataReader reader = cmd2.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            id_bank = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            string BankfullName = ((string) b.SelectToken("bank.fullName") ?? "").Trim();
                            string BankshortName = ((string) b.SelectToken("bank.shortName") ?? "").Trim();
                            string BankfactAddress = ((string) b.SelectToken("bank.factAddress") ?? "").Trim();
                            string BankINN = ((string) b.SelectToken("bank.INN") ?? "").Trim();
                            string BankKPP = ((string) b.SelectToken("bank.KPP") ?? "").Trim();
                            string BanksubjectRFName = ((string) b.SelectToken("bank.subjectRF.name") ?? "").Trim();
                            string insert_bank =
                                $"INSERT INTO {Program.Prefix}bank SET regNum = @regNum, fullName = @fullName, shortName = @shortName, factAddress = @factAddress, INN = @INN, KPP = @KPP, subjectRFName = @subjectRFName";
                            MySqlCommand cmd3 = new MySqlCommand(insert_bank, connect);
                            cmd3.Prepare();
                            cmd3.Parameters.AddWithValue("@regNum", BankregNum);
                            cmd3.Parameters.AddWithValue("@fullName", BankfullName);
                            cmd3.Parameters.AddWithValue("@shortName", BankshortName);
                            cmd3.Parameters.AddWithValue("@factAddress", BankfactAddress);
                            cmd3.Parameters.AddWithValue("@INN", BankINN);
                            cmd3.Parameters.AddWithValue("@KPP", BankKPP);
                            cmd3.Parameters.AddWithValue("@subjectRFName", BanksubjectRFName);
                            cmd3.ExecuteNonQuery();
                            id_bank = (int) cmd3.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет bank_reg_num", file_path);
                    }

                    int id_placer = 0;
                    string PlacerRegNum = ((string) b.SelectToken("placingOrg.regNum") ?? "").Trim();
                    if (!String.IsNullOrEmpty(PlacerRegNum))
                    {
                        string select_placer =
                            $"SELECT id FROM {Program.Prefix}placer_org WHERE regNum = @regNum";
                        MySqlCommand cmd4 = new MySqlCommand(select_placer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@regNum", BankregNum);
                        MySqlDataReader reader = cmd4.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            id_placer = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            string PlacerfullName = ((string) b.SelectToken("placingOrg.fullName") ?? "").Trim();
                            string PlacershortName = ((string) b.SelectToken("placingOrg.shortName") ?? "").Trim();
                            string PlacerfactAddress = ((string) b.SelectToken("placingOrg.factAddress") ?? "").Trim();
                            string PlacerINN = ((string) b.SelectToken("placingOrg.INN") ?? "").Trim();
                            string PlacerKPP = ((string) b.SelectToken("placingOrg.KPP") ?? "").Trim();
                            string PlacersubjectRFName =
                                ((string) b.SelectToken("placingOrg.subjectRF.name") ?? "").Trim();
                            string insert_placer =
                                $"INSERT INTO {Program.Prefix}placer_org SET regNum = @regNum, fullName = @fullName, shortName = @shortName, factAddress = @factAddress, INN = @INN, KPP = @KPP, subjectRFName = @subjectRFName";
                            MySqlCommand cmd5 = new MySqlCommand(insert_placer, connect);
                            cmd5.Prepare();
                            cmd5.Parameters.AddWithValue("@regNum", BankregNum);
                            cmd5.Parameters.AddWithValue("@fullName", PlacerfullName);
                            cmd5.Parameters.AddWithValue("@shortName", PlacershortName);
                            cmd5.Parameters.AddWithValue("@factAddress", PlacerfactAddress);
                            cmd5.Parameters.AddWithValue("@INN", PlacerINN);
                            cmd5.Parameters.AddWithValue("@KPP", PlacerKPP);
                            cmd5.Parameters.AddWithValue("@subjectRFName", PlacersubjectRFName);
                            cmd5.ExecuteNonQuery();
                            id_placer = (int) cmd5.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет placer_reg_num", file_path);
                    }

                    int id_supplier = 0;
                    string inn_sup = ((string) b.SelectToken("supplier.inn") ?? "").Trim();
                    if (String.IsNullOrEmpty(inn_sup))
                    {
                        inn_sup = ((string) b.SelectToken("supplierInfo.legalEntityRF.INN") ?? "").Trim();
                    }
                    if (!String.IsNullOrEmpty(inn_sup))
                    {
                        string kpp_sup = ((string) b.SelectToken("supplier.kpp") ?? "").Trim();
                        if (String.IsNullOrEmpty(kpp_sup))
                        {
                            kpp_sup = ((string) b.SelectToken("supplierInfo.legalEntityRF.KPP") ?? "").Trim();
                        }
                        string select_supplier =
                            $"SELECT id FROM {Program.Prefix}bank_supplier WHERE inn = @inn AND kpp = @kpp";
                        MySqlCommand cmd6 = new MySqlCommand(select_supplier, connect);
                        cmd6.Prepare();
                        cmd6.Parameters.AddWithValue("@inn", inn_sup);
                        cmd6.Parameters.AddWithValue("@kpp", kpp_sup);
                        MySqlDataReader reader = cmd6.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            id_supplier = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            string participantType_sup =
                                ((string) b.SelectToken("supplier.participantType") ?? "").Trim();
                            string ogrn_sup = ((string) b.SelectToken("supplier.ogrn") ?? "").Trim();
                            string organizationName_sup =
                                ((string) b.SelectToken("supplier.organizationName") ?? "").Trim();
                            string firmName_sup = ((string) b.SelectToken("supplier.firmName") ?? "").Trim();
                            string registrationDate_sup =
                                ((string) b.SelectToken("supplierInfo.legalEntityRF.registrationDate") ?? "").Trim();
                            string subjectRFName_sup =
                                ((string) b.SelectToken("supplierInfo.legalEntityRF.subjectRF.name") ?? "").Trim();
                            string address_sup = ((string) b.SelectToken("supplier.factualAddress") ?? "").Trim();
                            string insert_supplier =
                                $"INSERT INTO {Program.Prefix}bank_supplier SET participantType = @participantType, inn = @inn, kpp = @kpp, ogrn = @ogrn, organizationName = @organizationName, firmName = @firmName, registrationDate = @registrationDate, subjectRFName = @subjectRFName, address = @address";
                            MySqlCommand cmd7 = new MySqlCommand(insert_supplier, connect);
                            cmd7.Prepare();
                            cmd7.Parameters.AddWithValue("@inn", inn_sup);
                            cmd7.Parameters.AddWithValue("@kpp", kpp_sup);
                            cmd7.Parameters.AddWithValue("@participantType", participantType_sup);
                            cmd7.Parameters.AddWithValue("@ogrn", ogrn_sup);
                            cmd7.Parameters.AddWithValue("@organizationName", organizationName_sup);
                            cmd7.Parameters.AddWithValue("@firmName", firmName_sup);
                            cmd7.Parameters.AddWithValue("@registrationDate", registrationDate_sup);
                            cmd7.Parameters.AddWithValue("@subjectRFName", subjectRFName_sup);
                            cmd7.Parameters.AddWithValue("@address", address_sup);
                            cmd7.ExecuteNonQuery();
                            id_supplier = (int) cmd7.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет inn_supplier", file_path);
                    }

                    int id_customer = 0;
                    string customerregNum = ((string) b.SelectToken("guarantee.customer.regNum") ?? "").Trim();
                    if (!String.IsNullOrEmpty(customerregNum))
                    {
                        string select_customer =
                            $"SELECT id FROM {Program.Prefix}bank_customer WHERE regNum = @regNum";
                        MySqlCommand cmd8 = new MySqlCommand(select_customer, connect);
                        cmd8.Prepare();
                        cmd8.Parameters.AddWithValue("@regNum", customerregNum);
                        MySqlDataReader reader = cmd8.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            id_customer = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            string customerfullName =
                                ((string) b.SelectToken("guarantee.customer.fullName") ?? "").Trim();
                            string customerINN = ((string) b.SelectToken("guarantee.customer.INN") ?? "").Trim();
                            string customerKPP = ((string) b.SelectToken("guarantee.customer.KPP") ?? "").Trim();
                            string customerfactAddress =
                                ((string) b.SelectToken("guarantee.customer.factAddress") ?? "").Trim();
                            string customerregistrationDate =
                                ((string) b.SelectToken("guarantee.customer.registrationDate") ?? "").Trim();
                            string insert_customer =
                                $"INSERT INTO {Program.Prefix}bank_customer SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP, factAddress = @factAddress, registrationDate = @registrationDate";
                            MySqlCommand cmd9 = new MySqlCommand(insert_customer, connect);
                            cmd9.Prepare();
                            cmd9.Parameters.AddWithValue("@regNum", customerregNum);
                            cmd9.Parameters.AddWithValue("@fullName", customerfullName);
                            cmd9.Parameters.AddWithValue("@INN", customerINN);
                            cmd9.Parameters.AddWithValue("@KPP", customerKPP);
                            cmd9.Parameters.AddWithValue("@factAddress", customerfactAddress);
                            cmd9.Parameters.AddWithValue("@registrationDate", customerregistrationDate);
                            cmd9.ExecuteNonQuery();
                            id_customer = (int) cmd9.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет customer_reg_num", file_path);
                    }

                    string purchaseNumber =
                        ((string) b.SelectToken("guarantee.contractExecutionEnsure.purchase.purchaseNumber") ?? "")
                        .Trim();
                    string lotNumber =
                        ((string) b.SelectToken("guarantee.contractExecutionEnsure.purchase.lotNumber") ?? "").Trim();
                    string guaranteeDate =
                    (JsonConvert.SerializeObject(b.SelectToken("guarantee.guaranteeDate") ?? "") ??
                     "").Trim('"');
                    string guaranteeAmount = ((string) b.SelectToken("guarantee.guaranteeAmount") ?? "").Trim();
                    string currencyCode = ((string) b.SelectToken("guarantee.currency.code") ?? "").Trim();
                    string expireDate = (JsonConvert.SerializeObject(b.SelectToken("guarantee.expireDate") ?? "") ??
                                         "").Trim('"');
                    string entryForceDate =
                    (JsonConvert.SerializeObject(b.SelectToken("guarantee.entryForceDate") ?? "") ??
                     "").Trim('"');
                    string href = ((string) b.SelectToken("href") ?? "").Trim();
                    string print_form = ((string) b.SelectToken("printForm.url") ?? "").Trim();
                    int id_g = 0;
                    string insert_g =
                        $"INSERT INTO {Program.Prefix}bank_guarantee SET id_guarantee = @id_guarantee, regNumber = @regNumber, docNumber = @docNumber, versionNumber = @versionNumber, docPublishDate = @docPublishDate, purchaseNumber = @purchaseNumber, lotNumber = @lotNumber, guaranteeDate = @guaranteeDate, guaranteeAmount = @guaranteeAmount, currencyCode = @currencyCode, expireDate = @expireDate, entryForceDate = @entryForceDate, href = @href, print_form = @print_form, xml = @xml, id_bank = @id_bank, id_placer = @id_placer, id_customer = @id_customer, id_supplier = @id_supplier";
                    MySqlCommand cmd10 = new MySqlCommand(insert_g, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@id_guarantee", id_guarantee);
                    cmd10.Parameters.AddWithValue("@regNumber", regNumber);
                    cmd10.Parameters.AddWithValue("@docNumber", docNumber);
                    cmd10.Parameters.AddWithValue("@versionNumber", versionNumber);
                    cmd10.Parameters.AddWithValue("@docPublishDate", docPublishDate);
                    cmd10.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                    cmd10.Parameters.AddWithValue("@lotNumber", lotNumber);
                    cmd10.Parameters.AddWithValue("@guaranteeDate", guaranteeDate);
                    cmd10.Parameters.AddWithValue("@guaranteeAmount", guaranteeAmount);
                    cmd10.Parameters.AddWithValue("@currencyCode", currencyCode);
                    cmd10.Parameters.AddWithValue("@expireDate", expireDate);
                    cmd10.Parameters.AddWithValue("@entryForceDate", entryForceDate);
                    cmd10.Parameters.AddWithValue("@href", href);
                    cmd10.Parameters.AddWithValue("@print_form", print_form);
                    cmd10.Parameters.AddWithValue("@xml", xml);
                    cmd10.Parameters.AddWithValue("@id_bank", id_bank);
                    cmd10.Parameters.AddWithValue("@id_placer", id_placer);
                    cmd10.Parameters.AddWithValue("@id_customer", id_customer);
                    cmd10.Parameters.AddWithValue("@id_supplier", id_supplier);
                    int res_insert_g = cmd10.ExecuteNonQuery();
                    id_g = (int) cmd10.LastInsertedId;
                    AddBank44?.Invoke(res_insert_g);
                    List<JToken> attach =
                        GetElements(b, "agreementDocuments.attachment");
                    foreach (var att in attach)
                    {
                        string fileName = ((string) att.SelectToken("fileName") ?? "").Trim();
                        string docDescription = ((string) att.SelectToken("docDescription") ?? "").Trim();
                        string url = ((string) att.SelectToken("url") ?? "").Trim();
                        string insert_attach =
                            $"INSERT INTO {Program.Prefix}bank_attach SET id_guar = @id_guar, fileName = @fileName, docDescription = @docDescription, url = @url";
                        MySqlCommand cmd11 = new MySqlCommand(insert_attach, connect);
                        cmd11.Prepare();
                        cmd11.Parameters.AddWithValue("@id_guar", id_g);
                        cmd11.Parameters.AddWithValue("@fileName", fileName);
                        cmd11.Parameters.AddWithValue("@docDescription", docDescription);
                        cmd11.Parameters.AddWithValue("@url", url);
                        cmd11.ExecuteNonQuery();
                    }
                }


                //Console.WriteLine(id);
            }
            else
            {
                Log.Logger("Не могу найти тег bankGuarantee", file_path);
            }
        }
    }
}