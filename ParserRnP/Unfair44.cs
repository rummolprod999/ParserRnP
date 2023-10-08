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
                    Log.Logger("Не удалось добавить Unfair44", FilePath);
            };
        }

        public override void Parsing()
        {
            var xml = GetXml(File.ToString());
            var root = (JObject)T.SelectToken("export");
            var firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("unfairSupplier"));
            if (firstOrDefault != null)
            {
                var r = firstOrDefault.Value;
                var registryNum = ((string)r.SelectToken("registryNum") ?? "").Trim();
                if (string.IsNullOrEmpty(registryNum)) Log.Logger("У unfair нет registryNum", FilePath);

                var publishDate = (JsonConvert.SerializeObject(r.SelectToken("publishDate") ?? "") ??
                                   "").Trim('"');
                if (string.IsNullOrEmpty(publishDate)) Log.Logger("Нет publishDate", FilePath);

                var approveDate = (JsonConvert.SerializeObject(r.SelectToken("approveDate") ?? "") ??
                                   "").Trim('"');
                if (string.IsNullOrEmpty(approveDate)) Log.Logger("Нет approveDate", FilePath);

                var state = ((string)r.SelectToken("state") ?? "").Trim();
                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    if (!string.IsNullOrEmpty(registryNum) && !string.IsNullOrEmpty(publishDate))
                    {
                        var selectUnf =
                            $"SELECT id FROM {Program.Prefix}unfair WHERE registryNum = @registryNum AND publishDate = @publishDate AND state = @state";
                        var cmd = new MySqlCommand(selectUnf, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@registryNum", registryNum);
                        cmd.Parameters.AddWithValue("@publishDate", publishDate);
                        cmd.Parameters.AddWithValue("@state", state);
                        var reader = cmd.ExecuteReader();
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

                    var idOrg = 0;
                    var publishOrgregNum = ((string)r.SelectToken("publishOrg.regNum") ?? "").Trim();
                    var publishOrgfullName = ((string)r.SelectToken("publishOrg.fullName") ?? "").Trim();
                    if (!string.IsNullOrEmpty(publishOrgregNum))
                    {
                        var selectPubOrg =
                            $"SELECT id FROM {Program.Prefix}unfair_publish_org WHERE regNum = @regNum";
                        var cmd4 = new MySqlCommand(selectPubOrg, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@regNum", publishOrgregNum);
                        var reader3 = cmd4.ExecuteReader();
                        if (reader3.HasRows)
                        {
                            reader3.Read();
                            idOrg = reader3.GetInt32("id");
                            reader3.Close();
                        }
                        else
                        {
                            reader3.Close();
                            var insertPubOrg =
                                $"INSERT INTO {Program.Prefix}unfair_publish_org SET regNum = @regNum, fullName = @fullName";
                            var cmd5 = new MySqlCommand(insertPubOrg, connect);
                            cmd5.Prepare();
                            cmd5.Parameters.AddWithValue("@regNum", publishOrgregNum);
                            cmd5.Parameters.AddWithValue("@fullName", publishOrgfullName);
                            cmd5.ExecuteNonQuery();
                            idOrg = (int)cmd5.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет organizer_reg_num", FilePath);
                    }

                    var createReason = ((string)r.SelectToken("createReason") ?? "").Trim();
                    var approveReason = ((string)r.SelectToken("approveReason") ?? "").Trim();
                    var idCustomer = 0;
                    var customerregNum = ((string)r.SelectToken("customer.regNum") ?? "").Trim();
                    var customerfullName = ((string)r.SelectToken("customer.fullName") ?? "").Trim();
                    var customerInn = ((string)r.SelectToken("customer.INN") ?? "").Trim();
                    var customerKpp = ((string)r.SelectToken("customer.KPP") ?? "").Trim();
                    if (!string.IsNullOrEmpty(customerregNum))
                    {
                        var selectCustomer =
                            $"SELECT id FROM {Program.Prefix}unfair_customer WHERE regNum = @regNum";
                        var cmd6 = new MySqlCommand(selectCustomer, connect);
                        cmd6.Prepare();
                        cmd6.Parameters.AddWithValue("@regNum", customerregNum);
                        var reader4 = cmd6.ExecuteReader();
                        if (reader4.HasRows)
                        {
                            reader4.Read();
                            idCustomer = reader4.GetInt32("id");
                            reader4.Close();
                        }
                        else
                        {
                            reader4.Close();
                            var insertCustomer =
                                $"INSERT INTO {Program.Prefix}unfair_customer SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP";
                            var cmd7 = new MySqlCommand(insertCustomer, connect);
                            cmd7.Prepare();
                            cmd7.Parameters.AddWithValue("@regNum", customerregNum);
                            cmd7.Parameters.AddWithValue("@fullName", customerfullName);
                            cmd7.Parameters.AddWithValue("@INN", customerInn);
                            cmd7.Parameters.AddWithValue("@KPP", customerKpp);
                            cmd7.ExecuteNonQuery();
                            idCustomer = (int)cmd7.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет customer_reg_num", FilePath);
                    }

                    var idSupplier = 0;
                    var innSupplier = ((string)r.SelectToken("unfairSupplier.inn") ?? "").Trim();
                    var kppSupplier = ((string)r.SelectToken("unfairSupplier.kpp") ?? "").Trim();
                    var fullNameSupplier = ((string)r.SelectToken("unfairSupplier.fullName") ?? "").Trim();
                    var placefullName = ((string)r.SelectToken("unfairSupplier.place.kladr.fullName") ?? "")
                        .Trim();
                    var subjectRf = ((string)r.SelectToken("unfairSupplier.place.kladr.subjectRF") ?? "")
                        .Trim();
                    var area = ((string)r.SelectToken("unfairSupplier.place.kladr.area") ?? "")
                        .Trim();
                    var street = ((string)r.SelectToken("unfairSupplier.place.kladr.street") ?? "")
                        .Trim();
                    var building = ((string)r.SelectToken("unfairSupplier.place.kladr.building") ?? "")
                        .Trim();
                    placefullName = $"{subjectRf}, {area}, {street}, {building}, {placefullName}";
                    if (!string.IsNullOrEmpty(innSupplier))
                    {
                        var selectSupplier =
                            $"SELECT id FROM {Program.Prefix}unfair_suppplier WHERE inn = @inn AND kpp = @kpp";
                        var cmd8 = new MySqlCommand(selectSupplier, connect);
                        cmd8.Prepare();
                        cmd8.Parameters.AddWithValue("@inn", innSupplier);
                        cmd8.Parameters.AddWithValue("@kpp", kppSupplier);
                        var reader5 = cmd8.ExecuteReader();
                        if (reader5.HasRows)
                        {
                            reader5.Read();
                            idSupplier = reader5.GetInt32("id");
                            reader5.Close();
                        }
                        else
                        {
                            reader5.Close();
                            var email = ((string)r.SelectToken("unfairSupplier.place.email") ?? "").Trim();
                            var foundersNames =
                                ((string)r.SelectToken("unfairSupplier.founders.names") ?? "").Trim();
                            var foundersInn = ((string)r.SelectToken("unfairSupplier.founders.inn") ?? "").Trim();
                            var insertSupplier =
                                $"INSERT INTO {Program.Prefix}unfair_suppplier SET inn = @inn, kpp = @kpp, fullName = @fullName, placefullName = @placefullName, email = @email, founders_names = @founders_names, founders_inn = @founders_inn";
                            var cmd9 = new MySqlCommand(insertSupplier, connect);
                            cmd9.Prepare();
                            cmd9.Parameters.AddWithValue("@inn", innSupplier);
                            cmd9.Parameters.AddWithValue("@kpp", kppSupplier);
                            cmd9.Parameters.AddWithValue("@fullName", fullNameSupplier);
                            cmd9.Parameters.AddWithValue("@placefullName", placefullName);
                            cmd9.Parameters.AddWithValue("@email", email);
                            cmd9.Parameters.AddWithValue("@founders_names", foundersNames);
                            cmd9.Parameters.AddWithValue("@founders_inn", foundersInn);
                            cmd9.ExecuteNonQuery();
                            idSupplier = (int)cmd9.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет inn_supplier", FilePath);
                    }

                    var purchaseNumber = ((string)r.SelectToken("purchase.purchaseNumber") ?? "").Trim();
                    var purchaseObjectInfo = ((string)r.SelectToken("purchase.purchaseObjectInfo") ?? "").Trim();
                    var lotNumber = ((string)r.SelectToken("purchase.lotNumber") ?? "").Trim();
                    var contractRegNum = ((string)r.SelectToken("contract.regNum") ?? "").Trim();
                    var contractProductInfo = ((string)r.SelectToken("contract.productInfo") ?? "").Trim();
                    var contractOkpdCode = ((string)r.SelectToken("contract.OKPD.code") ?? "").Trim();
                    var contractOkpdName = ((string)r.SelectToken("contract.OKPD.name") ?? "").Trim();
                    var contractCurrencyCode = ((string)r.SelectToken("contract.currency.code") ?? "").Trim();
                    var contractPrice = ((string)r.SelectToken("contract.price") ?? "").Trim();
                    var contractCancelSignDate = ((string)r.SelectToken("contract.cancel.signDate") ?? "").Trim();
                    var contractCancelPerformanceDate =
                        ((string)r.SelectToken("contract.cancel.performanceDate") ?? "").Trim();
                    var contractCancelBaseName =
                        ((string)r.SelectToken("contract.cancel.base.name") ?? "").Trim();
                    var contractCancelCancelDate =
                        ((string)r.SelectToken("contract.cancel.cancelDate") ?? "").Trim();
                    var insertUnfair =
                        $"INSERT INTO {Program.Prefix}unfair SET publishDate = @publishDate, approveDate = @approveDate, registryNum = @registryNum, state = @state, createReason = @createReason, approveReason  = @approveReason, id_customer = @id_customer, id_supplier = @id_supplier, id_org = @id_org, purchaseNumber = @purchaseNumber, purchaseObjectInfo = @purchaseObjectInfo, lotNumber = @lotNumber, contract_regNum = @contract_regNum, contract_productInfo = @contract_productInfo, contract_OKPD_code = @contract_OKPD_code, contract_OKPD_name = @contract_OKPD_name, contract_currency_code = @contract_currency_code, contract_price = @contract_price, contract_cancel_signDate = @contract_cancel_signDate, contract_cancel_performanceDate = @contract_cancel_performanceDate, contract_cancel_base_name = @contract_cancel_base_name, contract_cancel_cancelDate = @contract_cancel_cancelDate, full_name_supplier = @full_name_supplier, placefullName_supplier = @placefullName_supplier";
                    var cmd10 = new MySqlCommand(insertUnfair, connect);
                    cmd10.Prepare();
                    cmd10.Parameters.AddWithValue("@publishDate", publishDate);
                    cmd10.Parameters.AddWithValue("@approveDate", approveDate);
                    cmd10.Parameters.AddWithValue("@registryNum", registryNum);
                    cmd10.Parameters.AddWithValue("@state", state);
                    cmd10.Parameters.AddWithValue("@createReason", createReason);
                    cmd10.Parameters.AddWithValue("@approveReason", approveReason);
                    cmd10.Parameters.AddWithValue("@id_customer", idCustomer);
                    cmd10.Parameters.AddWithValue("@id_supplier", idSupplier);
                    cmd10.Parameters.AddWithValue("@id_org", idOrg);
                    cmd10.Parameters.AddWithValue("@purchaseNumber", purchaseNumber);
                    cmd10.Parameters.AddWithValue("@purchaseObjectInfo", purchaseObjectInfo);
                    cmd10.Parameters.AddWithValue("@lotNumber", lotNumber);
                    cmd10.Parameters.AddWithValue("@contract_regNum", contractRegNum);
                    cmd10.Parameters.AddWithValue("@contract_productInfo", contractProductInfo);
                    cmd10.Parameters.AddWithValue("@contract_OKPD_code", contractOkpdCode);
                    cmd10.Parameters.AddWithValue("@contract_OKPD_name", contractOkpdName);
                    cmd10.Parameters.AddWithValue("@contract_currency_code", contractCurrencyCode);
                    cmd10.Parameters.AddWithValue("@contract_price", contractPrice);
                    cmd10.Parameters.AddWithValue("@contract_cancel_signDate", contractCancelSignDate);
                    cmd10.Parameters.AddWithValue("@contract_cancel_performanceDate", contractCancelPerformanceDate);
                    cmd10.Parameters.AddWithValue("@contract_cancel_base_name", contractCancelBaseName);
                    cmd10.Parameters.AddWithValue("@contract_cancel_cancelDate", contractCancelCancelDate);
                    cmd10.Parameters.AddWithValue("@full_name_supplier", fullNameSupplier);
                    cmd10.Parameters.AddWithValue("@placefullName_supplier", placefullName);
                    var resInsertUnf = cmd10.ExecuteNonQuery();
                    AddUnfair44?.Invoke(resInsertUnf);
                }
            }
            else
            {
                Log.Logger("Не могу найти тег unfairSupplier", FilePath);
            }
        }
    }
}