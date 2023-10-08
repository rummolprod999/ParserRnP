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
    public class Bank44 : Bank
    {
        public event Action<int> AddBank44;
        public event Action<int> UpdateBank44;

        public Bank44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddBank44 += delegate(int d)
            {
                if (d > 0)
                {
                    Program.AddBankGuarantee++;
                }
                else
                {
                    Log.Logger("Не удалось добавить Bank44", FilePath);
                }
            };

            UpdateBank44 += delegate(int d)
            {
                if (d > 0)
                {
                    Program.UpdateBankGuarantee++;
                }
                else
                {
                    Log.Logger("Не удалось обновить Bank44", FilePath);
                }
            };
        }

        public override void Parsing()
        {
            var xml = GetXml(File.ToString());
            var root = (JObject)T.SelectToken("export");
            var firstOrDefault = root.Properties().FirstOrDefault(p => p.Name.Contains("bankGuarantee"));
            if (firstOrDefault != null)
            {
                var b = firstOrDefault.Value;
                var idGuarantee = ((string)b.SelectToken("id") ?? "").Trim();
                var regNumber = ((string)b.SelectToken("regNumber") ?? "").Trim();
                if (string.IsNullOrEmpty(regNumber))
                {
                    Log.Logger("Нет regNumber", FilePath);
                }

                var docNumber = ((string)b.SelectToken("docNumber") ?? "").Trim();
                var versionNumber = ((string)b.SelectToken("versionNumber") ?? "").Trim();
                var docPublishDate = (JsonConvert.SerializeObject(b.SelectToken("docPublishDate") ?? "") ??
                                      "").Trim('"');
                if (string.IsNullOrEmpty(docPublishDate))
                {
                    Log.Logger("Нет docPublishDate", FilePath);
                }

                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    var upd = 0;
                    var idGuar = 0;
                    if (!string.IsNullOrEmpty(regNumber) && !string.IsNullOrEmpty(docPublishDate))
                    {
                        var selectBankG =
                            $"SELECT id FROM {Program.Prefix}bank_guarantee WHERE regNumber = @regNumber AND docPublishDate = @docPublishDate";
                        var cmd = new MySqlCommand(selectBankG, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNumber", regNumber);
                        cmd.Parameters.AddWithValue("@docPublishDate", docPublishDate);
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idGuar = reader.GetInt32("id");
                            reader.Close();
                            upd = 1;
                            //Log.Logger("Такой документ уже есть в базе", regNumber);
                            //return;
                        }

                        reader.Close();
                    }

                    var idBank = 0;
                    var bankregNum = ((string)b.SelectToken("bank.regNum") ?? "").Trim();
                    if (!string.IsNullOrEmpty(bankregNum))
                    {
                        var selectBank =
                            $"SELECT id FROM {Program.Prefix}bank WHERE regNum = @regNum";
                        var cmd2 = new MySqlCommand(selectBank, connect);
                        cmd2.Prepare();
                        cmd2.Parameters.AddWithValue("@regNum", bankregNum);
                        var reader = cmd2.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idBank = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            var bankfullName = ((string)b.SelectToken("bank.fullName") ?? "").Trim();
                            var bankshortName = ((string)b.SelectToken("bank.shortName") ?? "").Trim();
                            var bankfactAddress = ((string)b.SelectToken("bank.factAddress") ?? "").Trim();
                            var bankInn = ((string)b.SelectToken("bank.INN") ?? "").Trim();
                            var bankKpp = ((string)b.SelectToken("bank.KPP") ?? "").Trim();
                            var banksubjectRfName = ((string)b.SelectToken("bank.subjectRF.name") ?? "").Trim();
                            var insertBank =
                                $"INSERT INTO {Program.Prefix}bank SET regNum = @regNum, fullName = @fullName, shortName = @shortName, factAddress = @factAddress, INN = @INN, KPP = @KPP, subjectRFName = @subjectRFName";
                            var cmd3 = new MySqlCommand(insertBank, connect);
                            cmd3.Prepare();
                            cmd3.Parameters.AddWithValue("@regNum", bankregNum);
                            cmd3.Parameters.AddWithValue("@fullName", bankfullName);
                            cmd3.Parameters.AddWithValue("@shortName", bankshortName);
                            cmd3.Parameters.AddWithValue("@factAddress", bankfactAddress);
                            cmd3.Parameters.AddWithValue("@INN", bankInn);
                            cmd3.Parameters.AddWithValue("@KPP", bankKpp);
                            cmd3.Parameters.AddWithValue("@subjectRFName", banksubjectRfName);
                            cmd3.ExecuteNonQuery();
                            idBank = (int)cmd3.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет bank_reg_num", FilePath);
                    }

                    var idPlacer = 0;
                    var placerRegNum = ((string)b.SelectToken("placingOrg.regNum") ?? "").Trim();
                    if (!string.IsNullOrEmpty(placerRegNum))
                    {
                        var selectPlacer =
                            $"SELECT id FROM {Program.Prefix}placer_org WHERE regNum = @regNum";
                        var cmd4 = new MySqlCommand(selectPlacer, connect);
                        cmd4.Prepare();
                        cmd4.Parameters.AddWithValue("@regNum", bankregNum);
                        var reader = cmd4.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idPlacer = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            var placerfullName = ((string)b.SelectToken("placingOrg.fullName") ?? "").Trim();
                            var placershortName = ((string)b.SelectToken("placingOrg.shortName") ?? "").Trim();
                            var placerfactAddress = ((string)b.SelectToken("placingOrg.factAddress") ?? "").Trim();
                            var placerInn = ((string)b.SelectToken("placingOrg.INN") ?? "").Trim();
                            var placerKpp = ((string)b.SelectToken("placingOrg.KPP") ?? "").Trim();
                            var placersubjectRfName =
                                ((string)b.SelectToken("placingOrg.subjectRF.name") ?? "").Trim();
                            var insertPlacer =
                                $"INSERT INTO {Program.Prefix}placer_org SET regNum = @regNum, fullName = @fullName, shortName = @shortName, factAddress = @factAddress, INN = @INN, KPP = @KPP, subjectRFName = @subjectRFName";
                            var cmd5 = new MySqlCommand(insertPlacer, connect);
                            cmd5.Prepare();
                            cmd5.Parameters.AddWithValue("@regNum", bankregNum);
                            cmd5.Parameters.AddWithValue("@fullName", placerfullName);
                            cmd5.Parameters.AddWithValue("@shortName", placershortName);
                            cmd5.Parameters.AddWithValue("@factAddress", placerfactAddress);
                            cmd5.Parameters.AddWithValue("@INN", placerInn);
                            cmd5.Parameters.AddWithValue("@KPP", placerKpp);
                            cmd5.Parameters.AddWithValue("@subjectRFName", placersubjectRfName);
                            cmd5.ExecuteNonQuery();
                            idPlacer = (int)cmd5.LastInsertedId;
                        }
                    }

                    //Log.Logger("Нет placer_reg_num", file_path);
                    var idSupplier = 0;
                    var innSup = ((string)b.SelectToken("supplier.inn") ?? "").Trim();
                    if (string.IsNullOrEmpty(innSup))
                    {
                        innSup = ((string)b.SelectToken("supplierInfo.legalEntityRF.INN") ?? "").Trim();
                    }

                    if (!string.IsNullOrEmpty(innSup))
                    {
                        var kppSup = ((string)b.SelectToken("supplier.kpp") ?? "").Trim();
                        if (string.IsNullOrEmpty(kppSup))
                        {
                            kppSup = ((string)b.SelectToken("supplierInfo.legalEntityRF.KPP") ?? "").Trim();
                        }

                        var selectSupplier =
                            $"SELECT id FROM {Program.Prefix}bank_supplier WHERE inn = @inn AND kpp = @kpp";
                        var cmd6 = new MySqlCommand(selectSupplier, connect);
                        cmd6.Prepare();
                        cmd6.Parameters.AddWithValue("@inn", innSup);
                        cmd6.Parameters.AddWithValue("@kpp", kppSup);
                        var reader = cmd6.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idSupplier = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            var participantTypeSup =
                                ((string)b.SelectToken("supplier.participantType") ?? "").Trim();
                            var ogrnSup = ((string)b.SelectToken("supplier.ogrn") ?? "").Trim();
                            var organizationNameSup =
                                ((string)b.SelectToken("supplier.organizationName") ?? "").Trim();
                            var firmNameSup = ((string)b.SelectToken("supplier.firmName") ?? "").Trim();
                            var registrationDateSup =
                                ((string)b.SelectToken("supplierInfo.legalEntityRF.registrationDate") ?? "").Trim();
                            var subjectRfNameSup =
                                ((string)b.SelectToken("supplierInfo.legalEntityRF.subjectRF.name") ?? "").Trim();
                            var addressSup = ((string)b.SelectToken("supplier.factualAddress") ?? "").Trim();
                            var insertSupplier =
                                $"INSERT INTO {Program.Prefix}bank_supplier SET participantType = @participantType, inn = @inn, kpp = @kpp, ogrn = @ogrn, organizationName = @organizationName, firmName = @firmName, registrationDate = @registrationDate, subjectRFName = @subjectRFName, address = @address";
                            var cmd7 = new MySqlCommand(insertSupplier, connect);
                            cmd7.Prepare();
                            cmd7.Parameters.AddWithValue("@inn", innSup);
                            cmd7.Parameters.AddWithValue("@kpp", kppSup);
                            cmd7.Parameters.AddWithValue("@participantType", participantTypeSup);
                            cmd7.Parameters.AddWithValue("@ogrn", ogrnSup);
                            cmd7.Parameters.AddWithValue("@organizationName", organizationNameSup);
                            cmd7.Parameters.AddWithValue("@firmName", firmNameSup);
                            cmd7.Parameters.AddWithValue("@registrationDate", registrationDateSup);
                            cmd7.Parameters.AddWithValue("@subjectRFName", subjectRfNameSup);
                            cmd7.Parameters.AddWithValue("@address", addressSup);
                            cmd7.ExecuteNonQuery();
                            idSupplier = (int)cmd7.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет inn_supplier", FilePath);
                    }

                    var idCustomer = 0;
                    var customerregNum = ((string)b.SelectToken("guarantee.customer.regNum") ?? "").Trim();
                    if (!string.IsNullOrEmpty(customerregNum))
                    {
                        var selectCustomer =
                            $"SELECT id FROM {Program.Prefix}bank_customer WHERE regNum = @regNum";
                        var cmd8 = new MySqlCommand(selectCustomer, connect);
                        cmd8.Prepare();
                        cmd8.Parameters.AddWithValue("@regNum", customerregNum);
                        var reader = cmd8.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            idCustomer = reader.GetInt32("id");
                            reader.Close();
                        }
                        else
                        {
                            reader.Close();
                            var customerfullName =
                                ((string)b.SelectToken("guarantee.customer.fullName") ?? "").Trim();
                            var customerInn = ((string)b.SelectToken("guarantee.customer.INN") ?? "").Trim();
                            var customerKpp = ((string)b.SelectToken("guarantee.customer.KPP") ?? "").Trim();
                            var customerfactAddress =
                                ((string)b.SelectToken("guarantee.customer.factAddress") ?? "").Trim();
                            var customerregistrationDate =
                                ((string)b.SelectToken("guarantee.customer.registrationDate") ?? "").Trim();
                            var insertCustomer =
                                $"INSERT INTO {Program.Prefix}bank_customer SET regNum = @regNum, fullName = @fullName, INN = @INN, KPP = @KPP, factAddress = @factAddress, registrationDate = @registrationDate";
                            var cmd9 = new MySqlCommand(insertCustomer, connect);
                            cmd9.Prepare();
                            cmd9.Parameters.AddWithValue("@regNum", customerregNum);
                            cmd9.Parameters.AddWithValue("@fullName", customerfullName);
                            cmd9.Parameters.AddWithValue("@INN", customerInn);
                            cmd9.Parameters.AddWithValue("@KPP", customerKpp);
                            cmd9.Parameters.AddWithValue("@factAddress", customerfactAddress);
                            cmd9.Parameters.AddWithValue("@registrationDate", customerregistrationDate);
                            cmd9.ExecuteNonQuery();
                            idCustomer = (int)cmd9.LastInsertedId;
                        }
                    }
                    else
                    {
                        Log.Logger("Нет customer_reg_num", FilePath);
                    }

                    var purchaseNumber =
                        ((string)b.SelectToken("guarantee.contractExecutionEnsure.purchase.purchaseNumber") ?? "")
                        .Trim();
                    if (string.IsNullOrEmpty(purchaseNumber))
                    {
                        purchaseNumber =
                            ((string)b.SelectToken("guarantee.purchaseRequestEnsure.purchaseNumber") ?? "")
                            .Trim();
                    }

                    var lotNumber =
                        ((string)b.SelectToken("guarantee.contractExecutionEnsure.purchase.lotNumber") ?? "").Trim();
                    if (string.IsNullOrEmpty(lotNumber))
                    {
                        lotNumber =
                            ((string)b.SelectToken("guarantee.purchaseRequestEnsure.lotNumber") ?? "")
                            .Trim();
                    }

                    var guaranteeDate =
                        (JsonConvert.SerializeObject(b.SelectToken("guarantee.guaranteeDate") ?? "") ??
                         "").Trim('"');
                    var guaranteeAmount = ((string)b.SelectToken("guarantee.guaranteeAmount") ?? "").Trim();
                    var currencyCode = ((string)b.SelectToken("guarantee.currency.code") ?? "").Trim();
                    var expireDate = (JsonConvert.SerializeObject(b.SelectToken("guarantee.expireDate") ?? "") ??
                                      "").Trim('"');
                    var entryForceDate =
                        (JsonConvert.SerializeObject(b.SelectToken("guarantee.entryForceDate") ?? "") ??
                         "").Trim('"');
                    var href = ((string)b.SelectToken("href") ?? "").Trim();
                    var printForm = ((string)b.SelectToken("printForm.url") ?? "").Trim();
                    var idG = 0;
                    if (upd == 1)
                    {
                        var deleteAtt = $"DELETE FROM {Program.Prefix}bank_attach WHERE id_guar = @id_guar";
                        var cmd0 = new MySqlCommand(deleteAtt, connect);
                        cmd0.Prepare();
                        cmd0.Parameters.AddWithValue("@id_guar", idGuar);
                        cmd0.ExecuteNonQuery();
                        var updateG =
                            $"UPDATE {Program.Prefix}bank_guarantee SET id_guarantee = @id_guarantee, regNumber = @regNumber, docNumber = @docNumber, versionNumber = @versionNumber, docPublishDate = @docPublishDate, purchaseNumber = @purchaseNumber, lotNumber = @lotNumber, guaranteeDate = @guaranteeDate, guaranteeAmount = @guaranteeAmount, currencyCode = @currencyCode, expireDate = @expireDate, entryForceDate = @entryForceDate, href = @href, print_form = @print_form, xml = @xml, id_bank = @id_bank, id_placer = @id_placer, id_customer = @id_customer, id_supplier = @id_supplier WHERE id = @id";
                        var cmd10 = new MySqlCommand(updateG, connect);
                        cmd10.Prepare();
                        cmd10.Parameters.AddWithValue("@id_guarantee", idGuarantee);
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
                        cmd10.Parameters.AddWithValue("@print_form", printForm);
                        cmd10.Parameters.AddWithValue("@xml", xml);
                        cmd10.Parameters.AddWithValue("@id_bank", idBank);
                        cmd10.Parameters.AddWithValue("@id_placer", idPlacer);
                        cmd10.Parameters.AddWithValue("@id_customer", idCustomer);
                        cmd10.Parameters.AddWithValue("@id_supplier", idSupplier);
                        cmd10.Parameters.AddWithValue("@id", idGuar);
                        var resUpdateG = cmd10.ExecuteNonQuery();
                        idG = idGuar;
                        UpdateBank44?.Invoke(resUpdateG);
                    }
                    else
                    {
                        var insertG =
                            $"INSERT INTO {Program.Prefix}bank_guarantee SET id_guarantee = @id_guarantee, regNumber = @regNumber, docNumber = @docNumber, versionNumber = @versionNumber, docPublishDate = @docPublishDate, purchaseNumber = @purchaseNumber, lotNumber = @lotNumber, guaranteeDate = @guaranteeDate, guaranteeAmount = @guaranteeAmount, currencyCode = @currencyCode, expireDate = @expireDate, entryForceDate = @entryForceDate, href = @href, print_form = @print_form, xml = @xml, id_bank = @id_bank, id_placer = @id_placer, id_customer = @id_customer, id_supplier = @id_supplier";
                        var cmd10 = new MySqlCommand(insertG, connect);
                        cmd10.Prepare();
                        cmd10.Parameters.AddWithValue("@id_guarantee", idGuarantee);
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
                        cmd10.Parameters.AddWithValue("@print_form", printForm);
                        cmd10.Parameters.AddWithValue("@xml", xml);
                        cmd10.Parameters.AddWithValue("@id_bank", idBank);
                        cmd10.Parameters.AddWithValue("@id_placer", idPlacer);
                        cmd10.Parameters.AddWithValue("@id_customer", idCustomer);
                        cmd10.Parameters.AddWithValue("@id_supplier", idSupplier);
                        var resInsertG = cmd10.ExecuteNonQuery();
                        idG = (int)cmd10.LastInsertedId;
                        AddBank44?.Invoke(resInsertG);
                    }

                    var attach =
                        GetElements(b, "agreementDocuments.attachment");
                    foreach (var att in attach)
                    {
                        var fileName = ((string)att.SelectToken("fileName") ?? "").Trim();
                        var docDescription = ((string)att.SelectToken("docDescription") ?? "").Trim();
                        var url = ((string)att.SelectToken("url") ?? "").Trim();
                        var insertAttach =
                            $"INSERT INTO {Program.Prefix}bank_attach SET id_guar = @id_guar, fileName = @fileName, docDescription = @docDescription, url = @url";
                        var cmd11 = new MySqlCommand(insertAttach, connect);
                        cmd11.Prepare();
                        cmd11.Parameters.AddWithValue("@id_guar", idG);
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
                Log.Logger("Не могу найти тег bankGuarantee", FilePath);
            }
        }
    }
}