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
    public class Unfair44 : Unfair
    {
        public event Action<int> AddUnfair44;

        public Unfair44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddUnfair44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddRnp++;
                else
                    Log.Logger("Не удалось добавить Unfair44", file_path);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(file.ToString());
            JObject root = (JObject) t.SelectToken("export");
            JProperty firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("unfairSupplier"));
            if (firstOrDefault != null)
            {
                JToken r = firstOrDefault.Value;
                string registryNum = ((string) r.SelectToken("registryNum") ?? "").Trim();
                if (String.IsNullOrEmpty(registryNum))
                {
                    Log.Logger("У unfair нет registryNum", file_path);
                }
                string publishDate = (JsonConvert.SerializeObject(r.SelectToken("publishDate") ?? "") ??
                                      "").Trim('"');
                if (String.IsNullOrEmpty(publishDate))
                {
                    Log.Logger("Нет publishDate", file_path);
                }
                string approveDate = (JsonConvert.SerializeObject(r.SelectToken("approveDate") ?? "") ??
                                      "").Trim('"');
                if (String.IsNullOrEmpty(approveDate))
                {
                    Log.Logger("Нет approveDate", file_path);
                }
                string state = ((string) r.SelectToken("state") ?? "").Trim();
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();
                    if (!String.IsNullOrEmpty(registryNum) && !String.IsNullOrEmpty(publishDate))
                    {
                        string select_unf =
                            $"SELECT id FROM {Program.Prefix}unfair WHERE registryNum = @registryNum AND publishDate = @publishDate AND state = @state";
                        MySqlCommand cmd = new MySqlCommand(select_unf, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@registryNum", registryNum);
                        cmd.Parameters.AddWithValue("@publishDate", publishDate);
                        cmd.Parameters.AddWithValue("@state", state);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Close();
                            Log.Logger("Такой документ уже есть в базе", registryNum);
                            return;
                        }
                        reader.Close();
                        /*string select_unf2 =
                            $"SELECT id FROM {Program.Prefix}unfair WHERE registryNum = @registryNum AND publishDate = @publishDate";
                        MySqlCommand cmd2 = new MySqlCommand(select_unf2, connect);
                        cmd2.Prepare();
                        cmd2.Parameters.AddWithValue("@registryNum", registryNum);
                        cmd2.Parameters.AddWithValue("@publishDate", publishDate);
                        MySqlDataReader reader2 = cmd2.ExecuteReader();
                        if (reader2.HasRows)
                        {
                            reader2.Close();
                            string update_unf =
                                $"UPDATE {Program.Prefix}unfair SET state = @state WHERE registryNum = @registryNum AND publishDate = @publishDate";
                            MySqlCommand cmd3 = new MySqlCommand(update_unf, connect);
                            cmd3.Prepare();
                            cmd3.Parameters.AddWithValue("@registryNum", registryNum);
                            cmd3.Parameters.AddWithValue("@publishDate", publishDate);
                            cmd3.Parameters.AddWithValue("@state", state);
                            cmd3.ExecuteNonQuery();
                            Log.Logger("Обновили документ в базе", registryNum);
                            return;
                        }
                        reader2.Close();*/
                    }

                    int idOrg = 0;
                    string publishOrgregNum = ((string) r.SelectToken("publishOrg.regNum") ?? "").Trim();
                    string publishOrgfullName = ((string) r.SelectToken("publishOrg.fullName") ?? "").Trim();
                    if (!String.IsNullOrEmpty(publishOrgregNum))
                    {
                        string select_pub_org =
                            $"SELECT id FROM {Program.Prefix}unfair_publish_org WHERE regNum = @regNum";
                        MySqlCommand cmd4 = new MySqlCommand(select_pub_org, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@regNum", publishOrgregNum);
                        MySqlDataReader reader3 = cmd4.ExecuteReader();
                        if (reader3.HasRows)
                        {
                            reader3.Read();
                            idOrg = reader3.GetInt32("id");
                            reader3.Close();
                        }
                        else
                        {
                            reader3.Close();
                            string insert_pub_org =
                                $"INSERT INTO {Program.Prefix}unfair_publish_org SET regNum = @regNum, fullName = @fullName";
                            MySqlCommand cmd5 = new MySqlCommand(insert_pub_org, connect);
                            cmd5.Prepare();
                            cmd5.Parameters.AddWithValue("@regNum", publishOrgregNum);
                            cmd5.Parameters.AddWithValue("@fullName", publishOrgfullName);
                            cmd5.ExecuteNonQuery();
                            idOrg = (int) cmd5.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет organizer_reg_num", file_path);
                    }
                    string createReason = ((string) r.SelectToken("createReason") ?? "").Trim();
                    string approveReason = ((string) r.SelectToken("approveReason") ?? "").Trim();
                    int id_customer = 0;
                    string customerregNum = ((string) r.SelectToken("customer.regNum") ?? "").Trim();
                    string customerfullName = ((string) r.SelectToken("customer.fullName") ?? "").Trim();
                    string customerINN = ((string) r.SelectToken("customer.INN") ?? "").Trim();
                    string customerKPP = ((string) r.SelectToken("customer.KPP") ?? "").Trim();
                    if (!String.IsNullOrEmpty(customerregNum))
                    {
                        string select_customer =
                            $"SELECT id FROM {Program.Prefix}unfair_customer WHERE regNum = @regNum";
                        MySqlCommand cmd6 = new MySqlCommand(select_customer, connect);
                        cmd6.Prepare();
                        cmd6.Parameters.AddWithValue("@regNum", customerregNum);
                        MySqlDataReader reader4 = cmd6.ExecuteReader();
                        if (reader4.HasRows)
                        {
                            reader4.Read();
                            id_customer = reader4.GetInt32("id");
                            reader4.Close();
                        }
                        else
                        {
                            reader4.Close();
                            string insert_customer =
                                $"INSERT INTO {Program.Prefix}unfair_customer SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP";
                            MySqlCommand cmd7 = new MySqlCommand(insert_customer, connect);
                            cmd7.Prepare();
                            cmd7.Parameters.AddWithValue("@regNum", customerregNum);
                            cmd7.Parameters.AddWithValue("@fullName", customerfullName);
                            cmd7.Parameters.AddWithValue("@INN", customerINN);
                            cmd7.Parameters.AddWithValue("@KPP", customerKPP);
                            cmd7.ExecuteNonQuery();
                            id_customer = (int) cmd7.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет customer_reg_num", file_path);
                    }
                    int id_supplier = 0;
                    string inn_supplier = ((string) r.SelectToken("unfairSupplier.inn") ?? "").Trim();
                    string kpp_supplier = ((string) r.SelectToken("unfairSupplier.kpp") ?? "").Trim();
                    string fullName_supplier = ((string) r.SelectToken("unfairSupplier.fullName") ?? "").Trim();
                    string placefullName = ((string) r.SelectToken("unfairSupplier.place.kladr.fullName") ?? "")
                        .Trim();
                    string subjectRF = ((string) r.SelectToken("unfairSupplier.place.kladr.subjectRF") ?? "")
                        .Trim();
                    string area = ((string) r.SelectToken("unfairSupplier.place.kladr.area") ?? "")
                        .Trim();
                    string street = ((string) r.SelectToken("unfairSupplier.place.kladr.street") ?? "")
                        .Trim();
                    string building = ((string) r.SelectToken("unfairSupplier.place.kladr.building") ?? "")
                        .Trim();
                    placefullName = $"{subjectRF}, {area}, {street}, {building}, {placefullName}";
                    if (!String.IsNullOrEmpty(inn_supplier))
                    {
                        string select_supplier =
                            $"SELECT id FROM {Program.Prefix}unfair_suppplier WHERE inn = @inn AND kpp = @kpp";
                        MySqlCommand cmd8 = new MySqlCommand(select_supplier, connect);
                        cmd8.Prepare();
                        cmd8.Parameters.AddWithValue("@inn", inn_supplier);
                        cmd8.Parameters.AddWithValue("@kpp", kpp_supplier);
                        MySqlDataReader reader5 = cmd8.ExecuteReader();
                        if (reader5.HasRows)
                        {
                            reader5.Read();
                            id_supplier = reader5.GetInt32("id");
                            reader5.Close();
                        }
                        else
                        {
                            reader5.Close();
                            string email = ((string) r.SelectToken("unfairSupplier.place.email") ?? "").Trim();
                            string founders_names =
                                ((string) r.SelectToken("unfairSupplier.founders.names") ?? "").Trim();
                            string founders_inn = ((string) r.SelectToken("unfairSupplier.founders.inn") ?? "").Trim();
                            string insert_supplier =
                                $"INSERT INTO {Program.Prefix}unfair_suppplier SET inn = @inn, kpp = @kpp, fullName = @fullName, placefullName = @placefullName, email = @email, founders_names = @founders_names, founders_inn = @founders_inn";
                            MySqlCommand cmd9 = new MySqlCommand(insert_supplier, connect);
                            cmd9.Prepare();
                            cmd9.Parameters.AddWithValue("@inn", inn_supplier);
                            cmd9.Parameters.AddWithValue("@kpp", kpp_supplier);
                            cmd9.Parameters.AddWithValue("@fullName", fullName_supplier);
                            cmd9.Parameters.AddWithValue("@placefullName", placefullName);
                            cmd9.Parameters.AddWithValue("@email", email);
                            cmd9.Parameters.AddWithValue("@founders_names", founders_names);
                            cmd9.Parameters.AddWithValue("@founders_inn", founders_inn);
                            cmd9.ExecuteNonQuery();
                            id_supplier = (int) cmd9.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет inn_supplier", file_path);
                    }
                    string purchaseNumber = ((string) r.SelectToken("purchase.purchaseNumber") ?? "").Trim();
                    string purchaseObjectInfo = ((string) r.SelectToken("purchase.purchaseObjectInfo") ?? "").Trim();
                    string lotNumber = ((string) r.SelectToken("purchase.lotNumber") ?? "").Trim();
                    string contract_regNum = ((string) r.SelectToken("contract.regNum") ?? "").Trim();
                    string contract_productInfo = ((string) r.SelectToken("contract.productInfo") ?? "").Trim();
                    string contract_OKPD_code = ((string) r.SelectToken("contract.OKPD.code") ?? "").Trim();
                    string contract_OKPD_name = ((string) r.SelectToken("contract.OKPD.name") ?? "").Trim();
                    string contract_currency_code = ((string) r.SelectToken("contract.currency.code") ?? "").Trim();
                    string contract_price = ((string) r.SelectToken("contract.price") ?? "").Trim();
                    string contract_cancel_signDate = ((string) r.SelectToken("contract.cancel.signDate") ?? "").Trim();
                    string contract_cancel_performanceDate =
                        ((string) r.SelectToken("contract.cancel.performanceDate") ?? "").Trim();
                    string contract_cancel_base_name =
                        ((string) r.SelectToken("contract.cancel.base.name") ?? "").Trim();
                    string contract_cancel_cancelDate =
                        ((string) r.SelectToken("contract.cancel.cancelDate") ?? "").Trim();
                    string insert_unfair =
                        $"INSERT INTO {Program.Prefix}unfair SET publishDate = @publishDate, approveDate = @approveDate, registryNum = @registryNum, state = @state, createReason = @createReason, approveReason  = @approveReason, id_customer = @id_customer, id_supplier = @id_supplier, id_org = @id_org, purchaseNumber = @purchaseNumber, purchaseObjectInfo = @purchaseObjectInfo, lotNumber = @lotNumber, contract_regNum = @contract_regNum, contract_productInfo = @contract_productInfo, contract_OKPD_code = @contract_OKPD_code, contract_OKPD_name = @contract_OKPD_name, contract_currency_code = @contract_currency_code, contract_price = @contract_price, contract_cancel_signDate = @contract_cancel_signDate, contract_cancel_performanceDate = @contract_cancel_performanceDate, contract_cancel_base_name = @contract_cancel_base_name, contract_cancel_cancelDate = @contract_cancel_cancelDate, full_name_supplier = @full_name_supplier, placefullName_supplier = @placefullName_supplier";
                    MySqlCommand cmd10 = new MySqlCommand(insert_unfair, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@publishDate", publishDate);
                    cmd10.Parameters.AddWithValue("@approveDate", approveDate);
                    cmd10.Parameters.AddWithValue("@registryNum", registryNum);
                    cmd10.Parameters.AddWithValue("@state", state);
                    cmd10.Parameters.AddWithValue("@createReason", createReason);
                    cmd10.Parameters.AddWithValue("@approveReason", approveReason);
                    cmd10.Parameters.AddWithValue("@id_customer", id_customer);
                    cmd10.Parameters.AddWithValue("@id_supplier", id_supplier);
                    cmd10.Parameters.AddWithValue("@id_org", idOrg);
                    cmd10.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                    cmd10.Parameters.AddWithValue("@purchaseObjectInfo", purchaseObjectInfo);
                    cmd10.Parameters.AddWithValue("@lotNumber", lotNumber);
                    cmd10.Parameters.AddWithValue("@contract_regNum", contract_regNum);
                    cmd10.Parameters.AddWithValue("@contract_productInfo", contract_productInfo);
                    cmd10.Parameters.AddWithValue("@contract_OKPD_code", contract_OKPD_code);
                    cmd10.Parameters.AddWithValue("@contract_OKPD_name", contract_OKPD_name);
                    cmd10.Parameters.AddWithValue("@contract_currency_code", contract_currency_code);
                    cmd10.Parameters.AddWithValue("@contract_price", contract_price);
                    cmd10.Parameters.AddWithValue("@contract_cancel_signDate", contract_cancel_signDate);
                    cmd10.Parameters.AddWithValue("@contract_cancel_performanceDate", contract_cancel_performanceDate);
                    cmd10.Parameters.AddWithValue("@contract_cancel_base_name", contract_cancel_base_name);
                    cmd10.Parameters.AddWithValue("@contract_cancel_cancelDate", contract_cancel_cancelDate);
                    cmd10.Parameters.AddWithValue("@full_name_supplier", fullName_supplier);
                    cmd10.Parameters.AddWithValue("@placefullName_supplier", placefullName);
                    int res_insert_unf = cmd10.ExecuteNonQuery();
                    AddUnfair44?.Invoke(res_insert_unf);
                }
            }
            else
            {
                Log.Logger("Не могу найти тег unfairSupplier", file_path);
            }
        }
    }
}