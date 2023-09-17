using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ParserRnP
{
    public class FarmDrug
    {
        protected readonly JObject T;
        protected readonly FileInfo File;
        protected readonly string FilePath;

        public event Action<int> AddDrug;
        public event Action<int> UpdateDrug;

        public FarmDrug(FileInfo f, JObject json)
        {
            T = json;
            File = f;
            FilePath = File.ToString();

            AddDrug += delegate(int d)
            {
                if (d > 0)
                    Program.AddDrug++;
                else
                    Log.Logger("Не удалось добавить FarmDrug", FilePath);
            };

            AddDrug += delegate(int d)
            {
                if (d > 0)
                    Program.UpdateDrug++;
                else
                    Log.Logger("Не удалось обновить FarmDrug", FilePath);
            };
        }

        public void Parsing()
        {
            var xml = GetXml(File.ToString());
            var root = (JObject)T.SelectToken("export");
            var el = GetElements(root, "nsiFarmDrugsDictionary.nsiFarmDrugDictionary");
            foreach (var m in el)
            {
                try
                {
                    var t = m.SelectToken("MNNInfo");
                    parseElement(t);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
                finally
                {
                    GC.Collect();
                    //GC.WaitForPendingFinalizers();
                }
                
            }
        }

        private void parseElement(JToken m)
        {
            var MNNCode = ((string)m.SelectToken("MNNCode") ?? "").Trim();
            if (string.IsNullOrEmpty(MNNCode))
            {
                throw new Exception("MNNCode is empty");
            }

            var lastChangeDate = (JsonConvert.SerializeObject(m.SelectToken("lastChangeDate") ?? "") ??
                                  "").Trim('"');
            if (string.IsNullOrEmpty(lastChangeDate))
            {
                throw new Exception("MNNCode is lastChangeDate");
            }

            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTender =
                    $"SELECT id FROM nsi_drug_FarmDictionaryMNN WHERE MNNCode = @MNNCode and  lastChangeDate = @lastChangeDate";
                var cmd = new MySqlCommand(selectTender, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@MNNCode", MNNCode);
                cmd.Parameters.AddWithValue("@lastChangeDate", lastChangeDate);
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Close();
                    return;
                }

                reader.Close();
                var isZNVLP = (bool?)m.SelectToken("isZNVLP") ?? false;
                var isNarcotiс = (bool?)m.SelectToken("isNarcotiс") ?? false;
                var createDate = (JsonConvert.SerializeObject(m.SelectToken("createDate") ?? "") ??
                                  "").Trim('"');
                var startDate = (JsonConvert.SerializeObject(m.SelectToken("startDate") ?? "") ??
                                 "").Trim('"');
                var endDate = (JsonConvert.SerializeObject(m.SelectToken("endDate") ?? "") ??
                               "").Trim('"');
                var changeDate = (JsonConvert.SerializeObject(m.SelectToken("changeDate") ?? "") ??
                                  "").Trim('"');
                var actual = (bool?)m.SelectToken("actual") ?? false;
                var MNNDrugCode = ((string)m.SelectToken("MNNDrugCode") ?? "").Trim();
                var MNNExternalCode = ((string)m.SelectToken("MNNExternalCode") ?? "").Trim();
                var MNNHash = ((string)m.SelectToken("MNNHash") ?? "").Trim();
                var MNNName = ((string)m.SelectToken("MNNName") ?? "").Trim();
                var insert =
                    $"INSERT INTO nsi_drug_FarmDictionaryMNN (id, isZNVLP, isNarcotiс, createDate, startDate, endDate, changeDate, actual, lastChangeDate, MNNCode, MNNDrugCode, MNNExternalCode, MNNHash, MNNName, version) VALUES (null,@isznvlp,@isnarcotiс,@createdate,@startdate,@enddate,@changedate,@actual,@lastchangedate,@mnncode,@mnndrugcode,@mnnexternalcode,@mnnhash,@mnnname,@version)";
                var cmd9 = new MySqlCommand(insert, connect);
                cmd9.Prepare();
                cmd9.Parameters.AddWithValue("@isznvlp", isZNVLP);
                cmd9.Parameters.AddWithValue("@isnarcotiс", isNarcotiс);
                cmd9.Parameters.AddWithValue("@createdate", createDate);
                cmd9.Parameters.AddWithValue("@startdate", startDate);
                cmd9.Parameters.AddWithValue("@enddate", endDate);
                cmd9.Parameters.AddWithValue("@changedate", changeDate);
                cmd9.Parameters.AddWithValue("@actual", actual);
                cmd9.Parameters.AddWithValue("@lastchangedate", lastChangeDate);
                cmd9.Parameters.AddWithValue("@mnncode", MNNCode);
                cmd9.Parameters.AddWithValue("@mnndrugcode", MNNDrugCode);
                cmd9.Parameters.AddWithValue("@mnnexternalcode", MNNExternalCode);
                cmd9.Parameters.AddWithValue("@mnnhash", MNNHash);
                cmd9.Parameters.AddWithValue("@mnnname", MNNName);
                cmd9.Parameters.AddWithValue("@version", 0);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idMnn = (int)cmd9.LastInsertedId;
                AddDrug?.Invoke(resInsertTender);
                AddVerNumber(connect, MNNCode);
                var positionsTradeName = GetElements(m, "positionsTradeName.positionTradeName");
                foreach (var pos in positionsTradeName)
                {
                    insertPos(pos, connect, idMnn, m);
                }
            }
        }

        private void insertPos(JToken pos, MySqlConnection connect, int idMnn, JToken m)
        {
            var positionTradeNameCode = ((string)pos.SelectToken("positionTradeNameCode") ?? "").Trim();
            var positionTradeNameExternalCode =
                ((string)pos.SelectToken("positionTradeNameExternalCode") ?? "").Trim();
            var positionTradeNameHash = ((string)pos.SelectToken("positionTradeNameHash") ?? "").Trim();
            var drugCode = ((string)pos.SelectToken("drugCode") ?? "").Trim();
            var MNNNormName = ((string)pos.SelectToken("MNNNormName") ?? "").Trim();
            var dosageNormName = ((string)pos.SelectToken("dosageNormName") ?? "").Trim();
            var medicamentalFormNormName = ((string)pos.SelectToken("medicamentalFormNormName") ?? "").Trim();
            var isDosed = (bool?)pos.SelectToken("isDosed") ?? false;
            var isconv = ((string)pos.SelectToken("isconv") ?? "").Trim();
            var id_OKEI_dos_pack = 0;
            id_OKEI_dos_pack = inserttOkei(pos, connect, m, id_OKEI_dos_pack);

            var completeness = ((string)pos.SelectToken("completeness") ?? "").Trim();
            var id_cert_manuf_origin = 0;
            id_cert_manuf_origin = addCountry(pos, connect, id_cert_manuf_origin);

            var certificateNumber = ((string)pos.SelectToken("certificateNumber") ?? "").Trim();
            var certificateDate = (JsonConvert.SerializeObject(pos.SelectToken("certificateDate") ?? "") ??
                                   "").Trim('"');
            var certificateUpdateDate =
                (JsonConvert.SerializeObject(pos.SelectToken("certificateReceiptDate") ?? "") ??
                 "").Trim('"');
            var barcode = ((string)pos.SelectToken("limPricesInfo.limPriceInfo.barcode") ?? "").Trim();
            var barcodeNew = ((string)pos.SelectToken("limPricesInfo.limPriceInfo.barcodeNew") ?? "").Trim();
            var tradeCode = ((string)pos.SelectToken("tradeInfo.tradeCode") ?? "").Trim();
            var tradeName = ((string)pos.SelectToken("tradeInfo.tradeName") ?? "").Trim();
            var insertN =
                $"INSERT INTO nsi_drug_positionTradeName (id, id_FarmDictionaryMNN, positionTradeNameCode, positionTradeNameExternalCode, positionTradeNameHash, drugCode, MNNNormName, dosageNormName, medicamentalFormNormName, isDosed, isconv, id_OKEI_dos_pack, completeness, id_cert_manuf_origin, certificateNumber, certificateDate, certificateUpdateDate, barcode, barcodeNew, tradeCode, tradeName) VALUES (null,@idFarmdictionarymnn,@positiontradenamecode,@positiontradenameexternalcode,@positiontradenamehash,@drugcode,@mnnnormname,@dosagenormname,@medicamentalformnormname,@isdosed,@isconv,@idOkeiDosPack,@completeness,@idCertManufOrigin,@certificatenumber,@certificatedate,@certificateupdatedate,@barcode,@barcodenew,@tradecode,@tradename)";
            var cmd10 = new MySqlCommand(insertN, connect);
            cmd10.Prepare();
            cmd10.Parameters.AddWithValue("@idFarmdictionarymnn", idMnn);
            cmd10.Parameters.AddWithValue("@positiontradenamecode", positionTradeNameCode);
            cmd10.Parameters.AddWithValue("@positiontradenameexternalcode", positionTradeNameExternalCode);
            cmd10.Parameters.AddWithValue("@positiontradenamehash", positionTradeNameHash);
            cmd10.Parameters.AddWithValue("@drugcode", drugCode);
            cmd10.Parameters.AddWithValue("@mnnnormname", MNNNormName);
            cmd10.Parameters.AddWithValue("@dosagenormname", dosageNormName);
            cmd10.Parameters.AddWithValue("@medicamentalformnormname", medicamentalFormNormName);
            cmd10.Parameters.AddWithValue("@isdosed", isDosed);
            cmd10.Parameters.AddWithValue("@isconv", isconv);
            cmd10.Parameters.AddWithValue("@idOkeiDosPack", id_OKEI_dos_pack);
            cmd10.Parameters.AddWithValue("@completeness", completeness);
            cmd10.Parameters.AddWithValue("@idCertManufOrigin", id_cert_manuf_origin);
            cmd10.Parameters.AddWithValue("@certificatenumber", certificateNumber);
            cmd10.Parameters.AddWithValue("@certificatedate", certificateDate);
            cmd10.Parameters.AddWithValue("@certificateupdatedate", certificateUpdateDate);
            cmd10.Parameters.AddWithValue("@barcode", barcode);
            cmd10.Parameters.AddWithValue("@barcodenew", barcodeNew);
            cmd10.Parameters.AddWithValue("@tradecode", tradeCode);
            cmd10.Parameters.AddWithValue("@tradename", tradeName);
            var resInsertM = cmd10.ExecuteNonQuery();
        }

        private int inserttOkei(JToken pos, MySqlConnection connect, JToken m, int id_OKEI_dos_pack)
        {
            var dosageOKEIcode = ((string)m.SelectToken("dosagesInfo.dosageInfo.dosageOKEI.code") ?? "").Trim();
            var dosageOKEIname = ((string)m.SelectToken("dosagesInfo.dosageInfo.dosageOKEI.name") ?? "").Trim();
            var dosageGRLSValue =
                ((string)m.SelectToken("dosagesInfo.dosageInfo.dosageGRLSValue") ?? "").Trim();
            var dosageValue = ((string)m.SelectToken("dosagesInfo.dosageInfo.dosageValue") ?? "").Trim();
            var OKEIcode =
                ((string)m.SelectToken("dosagesInfo.dosageInfo.dosageUser.dosageUserOKEI.code") ?? "")
                .Trim();
            var OKEIname =
                ((string)m.SelectToken("dosagesInfo.dosageInfo.dosageUser.dosageUserOKEI.name") ?? "")
                .Trim();
            var packaging1Quantity =
                ((string)pos.SelectToken("packagingsInfo.packagingInfo.packaging1Quantity") ?? "").Trim();
            var packaging2Quantity =
                ((string)pos.SelectToken("packagingsInfo.packagingInfo.packaging2Quantity") ?? "").Trim();
            var primaryPackagingCode =
                ((string)pos.SelectToken(
                     "packagingsInfo.packagingInfo.primaryPackagingInfo.primaryPackagingCode") ??
                 "").Trim();
            var primaryPackagingName =
                ((string)pos.SelectToken(
                     "packagingsInfo.packagingInfo.primaryPackagingInfo.primaryPackagingName") ??
                 "").Trim();
            var consumerPackagingCode =
                ((string)pos.SelectToken(
                     "packagingsInfo.packagingInfo.consumerPackagingInfo.consumerPackagingCode") ??
                 "").Trim();
            var consumerPackagingName =
                ((string)pos.SelectToken(
                     "packagingsInfo.packagingInfo.consumerPackagingInfo.consumerPackagingName") ??
                 "").Trim();
            if (dosageOKEIcode != "" && dosageOKEIname != "")
            {
                var country =
                    $"SELECT id FROM nsi_drug_OKEI_dos_pack WHERE dosageOKEIcode = @dosageOKEIcode AND dosageOKEIname = @dosageOKEIname AND dosageGRLSValue = @dosageGRLSValue AND dosageValue = @dosageValue AND OKEIcode = @OKEIcode AND OKEIname = @OKEIname AND packaging1Quantity = @packaging1Quantity AND packaging2Quantity = @packaging2Quantity AND primaryPackagingCode = @primaryPackagingCode AND primaryPackagingName = @primaryPackagingName AND consumerPackagingCode = @consumerPackagingCode AND consumerPackagingName = @consumerPackagingName";
                var cmd4 = new MySqlCommand(country, connect);
                cmd4.Prepare();
                cmd4.Parameters.AddWithValue("@dosageOKEIcode", dosageOKEIcode);
                cmd4.Parameters.AddWithValue("@dosageOKEIname", dosageOKEIname);
                cmd4.Parameters.AddWithValue("@dosageGRLSValue", dosageGRLSValue);
                cmd4.Parameters.AddWithValue("@dosageValue", dosageValue);
                cmd4.Parameters.AddWithValue("@OKEIcode", OKEIcode);
                cmd4.Parameters.AddWithValue("@OKEIname", OKEIname);
                cmd4.Parameters.AddWithValue("@packaging1Quantity", packaging1Quantity);
                cmd4.Parameters.AddWithValue("@packaging2Quantity", packaging2Quantity);
                cmd4.Parameters.AddWithValue("@primaryPackagingCode", primaryPackagingCode);
                cmd4.Parameters.AddWithValue("@primaryPackagingName", primaryPackagingName);
                cmd4.Parameters.AddWithValue("@consumerPackagingCode", consumerPackagingCode);
                cmd4.Parameters.AddWithValue("@consumerPackagingName", consumerPackagingName);
                var reader2 = cmd4.ExecuteReader();
                if (reader2.HasRows)
                {
                    reader2.Read();
                    id_OKEI_dos_pack = reader2.GetInt32("id");
                    reader2.Close();
                }
                else
                {
                    reader2.Close();
                    var actual = (bool?)pos.SelectToken("actual") ?? false;
                    var addc =
                        $"INSERT INTO nsi_drug_OKEI_dos_pack (id, actual, dosageOKEIcode, dosageOKEIname, dosageGRLSValue, dosageValue, OKEIcode, OKEIname, packaging1Quantity, packaging2Quantity, primaryPackagingCode, primaryPackagingName, consumerPackagingCode, consumerPackagingName) VALUES (null,@actual,@dosageokeicode,@dosageokeiname,@dosagegrlsvalue,@dosagevalue,@okeicode,@okeiname,@packaging1quantity,@packaging2quantity,@primarypackagingcode,@primarypackagingname,@consumerpackagingcode,@consumerpackagingname)";
                    var cmd5 = new MySqlCommand(addc, connect);
                    cmd5.Prepare();
                    cmd5.Parameters.AddWithValue("@actual", actual);
                    cmd5.Parameters.AddWithValue("@dosageokeicode", dosageOKEIcode);
                    cmd5.Parameters.AddWithValue("@dosageokeiname", dosageOKEIname);
                    cmd5.Parameters.AddWithValue("@dosagegrlsvalue", dosageGRLSValue);
                    cmd5.Parameters.AddWithValue("@dosagevalue", dosageValue);
                    cmd5.Parameters.AddWithValue("@okeicode", OKEIcode);
                    cmd5.Parameters.AddWithValue("@okeiname", OKEIname);
                    cmd5.Parameters.AddWithValue("@packaging1quantity", packaging1Quantity);
                    cmd5.Parameters.AddWithValue("@packaging2quantity", packaging2Quantity);
                    cmd5.Parameters.AddWithValue("@primarypackagingcode", primaryPackagingCode);
                    cmd5.Parameters.AddWithValue("@primarypackagingname", primaryPackagingName);
                    cmd5.Parameters.AddWithValue("@consumerpackagingcode", consumerPackagingCode);
                    cmd5.Parameters.AddWithValue("@consumerpackagingname", consumerPackagingName);
                    cmd5.ExecuteNonQuery();
                    id_OKEI_dos_pack = (int)cmd5.LastInsertedId;
                }
            }

            return id_OKEI_dos_pack;
        }

        private int addCountry(JToken pos, MySqlConnection connect, int id_cert_manuf_origin)
        {
            var countryCode = ((string)pos.SelectToken("manufacturerInfo.manufacturerOKSM.countryCode") ?? "").Trim();
            var countryFullName = ((string)pos.SelectToken("manufacturerInfo.manufacturerOKSM.countryFullName") ?? "")
                .Trim();
            if (countryCode != "" && countryFullName != "")
            {
                var country =
                    $"SELECT id FROM nsi_drug_cert_manuf_origin WHERE countryCode = @countryCode AND countryFullName = @countryFullName";
                var cmd4 = new MySqlCommand(country, connect);
                cmd4.Prepare();
                cmd4.Parameters.AddWithValue("@countryCode", countryCode);
                cmd4.Parameters.AddWithValue("@countryFullName", countryFullName);
                var reader2 = cmd4.ExecuteReader();
                if (reader2.HasRows)
                {
                    reader2.Read();
                    id_cert_manuf_origin = reader2.GetInt32("id");
                    reader2.Close();
                }
                else
                {
                    reader2.Close();
                    var certificateKeeperName = ((string)pos.SelectToken("owner.certificateKeeperName") ?? "").Trim();
                    var manufacturerAdress =
                        ((string)pos.SelectToken("manufacturerInfo.manufacturerAdress") ?? "").Trim();
                    var manufacturerName = ((string)pos.SelectToken("manufacturerInfo.manufacturerName") ?? "").Trim();
                    var actual = (bool?)pos.SelectToken("actual") ?? false;
                    var type = ((string)pos.SelectToken("type") ?? "").Trim();
                    var addc =
                        $"INSERT INTO nsi_drug_cert_manuf_origin (id, countryCode, countryFullName, certificateKeeperName, manufacturerAdress, manufacturerName, actual, type) VALUES (null,@countrycode,@countryfullname,@certificatekeepername,@manufactureradress,@manufacturername,@actual,@type)";
                    var cmd5 = new MySqlCommand(addc, connect);
                    cmd5.Prepare();
                    cmd5.Parameters.AddWithValue("@countrycode", countryCode);
                    cmd5.Parameters.AddWithValue("@countryfullname", countryFullName);
                    cmd5.Parameters.AddWithValue("@certificatekeepername", certificateKeeperName);
                    cmd5.Parameters.AddWithValue("@manufactureradress", manufacturerAdress);
                    cmd5.Parameters.AddWithValue("@manufacturername", manufacturerName);
                    cmd5.Parameters.AddWithValue("@actual", actual);
                    cmd5.Parameters.AddWithValue("@type", type);
                    cmd5.ExecuteNonQuery();
                    id_cert_manuf_origin = (int)cmd5.LastInsertedId;
                }
            }

            return id_cert_manuf_origin;
        }

        public void AddVerNumber(MySqlConnection connect, string nnmCode)
        {
            var verNum = 1;
            var selectTenders =
                $"SELECT id FROM nsi_drug_FarmDictionaryMNN WHERE MNNCode = @MNNCode ORDER BY UNIX_TIMESTAMP(lastChangeDate) ASC";
            var cmd1 = new MySqlCommand(selectTenders, connect);
            cmd1.Prepare();
            cmd1.Parameters.AddWithValue("@MNNCode", nnmCode);
            var dt1 = new DataTable();
            var adapter1 = new MySqlDataAdapter { SelectCommand = cmd1 };
            adapter1.Fill(dt1);
            if (dt1.Rows.Count > 0)
            {
                var updateTender =
                    $"UPDATE nsi_drug_FarmDictionaryMNN SET version = @version WHERE id = @id";
                foreach (DataRow ten in dt1.Rows)
                {
                    var id = (int)ten["id"];
                    var cmd2 = new MySqlCommand(updateTender, connect);
                    cmd2.Prepare();
                    cmd2.Parameters.AddWithValue("@id", id);
                    cmd2.Parameters.AddWithValue("@version", verNum);
                    cmd2.ExecuteNonQuery();
                    verNum++;
                }
            }
        }


        public string GetXml(string xml)
        {
            var xmlt = xml.Split('/');
            var t = xmlt.Length;
            if (t >= 2)
            {
                var sxml = xmlt[t - 2] + "/" + xmlt[t - 1];
                return sxml;
            }

            return "";
        }

        public List<JToken> GetElements(JToken j, string s)
        {
            var els = new List<JToken>();
            var elsObj = j.SelectToken(s);
            if (elsObj != null && elsObj.Type != JTokenType.Null)
            {
                switch (elsObj.Type)
                {
                    case JTokenType.Object:
                        els.Add(elsObj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(elsObj);
                        break;
                }
            }

            return els;
        }
    }
}