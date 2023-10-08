#region

using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class Ktru
    {
        protected readonly JObject T;
        protected readonly FileInfo File;
        protected readonly string FilePath;

        public event Action<int> Add;
        public event Action<int> Update;

        public Ktru(FileInfo f, JObject json)
        {
            T = json;
            File = f;
            FilePath = File.ToString();

            Add += delegate(int d)
            {
                if (d > 0)
                    Program.AddKtru++;
                else
                    Log.Logger("Не удалось добавить KTRU", FilePath);
            };

            Update += delegate(int d)
            {
                if (d > 0)
                    Program.UpdateKtru++;
                else
                    Log.Logger("Не удалось обновить KTRU", FilePath);
            };
        }

        public void Parsing()
        {
            var xml = GetXml(File.ToString());
            var root = (JObject)T.SelectToken("export");
            var el = GetElements(root, "nsiKTRUs.position");
            foreach (var m in el)
                try
                {
                    var t = m.SelectToken("data");
                    parseElement(t);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
        }

        private void parseElement(JToken m)
        {
            var code = ((string)m.SelectToken("code") ?? "").Trim();
            if (string.IsNullOrEmpty(code)) throw new Exception($"{nameof(code)} is empty");

            var version = (int?)m.SelectToken("version") ?? throw new Exception("version is empty");
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var selectTender =
                    "SELECT id FROM nsi_ktru WHERE code = @code and  version = @version";
                var cmd = new MySqlCommand(selectTender, connect);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@version", version);
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Close();
                    return;
                }

                reader.Close();
                var inclusionDate = (JsonConvert.SerializeObject(m.SelectToken("inclusionDate") ?? "") ??
                                     "").Trim('"');
                var publishDate = (JsonConvert.SerializeObject(m.SelectToken("publishDate") ?? "") ??
                                   "").Trim('"');
                var name_ktru = ((string)m.SelectToken("name") ?? "").Trim();
                var actual = (bool?)m.SelectToken("actual") ?? false;
                if (FilePath.Contains("nsiKTRUNew_all_actual")) actual = true;

                var applicationDateStart = (JsonConvert.SerializeObject(m.SelectToken("applicationDateStart") ?? "") ??
                                            "").Trim('"');
                var applicationDateEnd = (JsonConvert.SerializeObject(m.SelectToken("applicationDateEnd") ?? "") ??
                                          "").Trim('"');
                var cancelDate = (JsonConvert.SerializeObject(m.SelectToken("cancelInfo.cancelDate") ?? "") ??
                                  "").Trim('"');
                var cancelReason = ((string)m.SelectToken("cancelInfo.cancelReason") ?? "").Trim();
                var isTemplate = (bool?)m.SelectToken("isTemplate") ?? false;
                var externalCode = ((string)m.SelectToken("parentPositionInfo.externalCode") ?? "").Trim();
                var codeOKEI = ((string)m.SelectToken("OKEIs.OKEI.code") ?? "").Trim();
                var nameOKEI = ((string)m.SelectToken("OKEIs.OKEI.name") ?? "").Trim();
                var docName_standards_NSI = ((string)m.SelectToken("NSI.standarts.standart.docName") ?? "").Trim();
                var name_classifier_OKVED = "Информация о классификаторе";
                var name_classifier_OKPD2 =
                    "Общероссийский классификатор продукции по видам экономической деятельности (ОКПД2)";
                var name_classifier_medical_product = "НОМЕНКЛАТУРНАЯ КЛАССИФИКАЦИЯ МЕДИЦИНСКИХ ИЗДЕЛИЙ ПО ВИДАМ";
                var name_classifier_program =
                    "Классификатор программ для электронных вычислительных машин и баз данных";
                var code_OKVED = "";
                var name_OKVED = "";
                var code_OKPD2 = "";
                var name_OKPD2 = "";
                var code_medical_product = "";
                var name_medical_product = "";
                var descriptionValue_medical_product = "";
                var code_program = "";
                var name_program = "";
                var classifiers = GetElements(m, "NSI.classifiers.classifier");
                foreach (var c in classifiers)
                {
                    var n = ((string)c.SelectToken("name") ?? "").Trim();
                    if (n.Contains(name_classifier_OKVED))
                    {
                        code_OKVED = ((string)c.SelectToken("values.value.code") ?? "").Trim();
                        name_OKVED = ((string)c.SelectToken("values.value.name") ?? "").Trim();
                    }
                    else if (n.Contains(name_classifier_OKPD2))
                    {
                        code_OKPD2 = ((string)c.SelectToken("values.value.code") ?? "").Trim();
                        name_OKPD2 = ((string)c.SelectToken("values.value.name") ?? "").Trim();
                    }
                    else if (n.Contains(name_classifier_medical_product))
                    {
                        code_medical_product = ((string)c.SelectToken("values.value.code") ?? "").Trim();
                        name_medical_product = ((string)c.SelectToken("values.value.name") ?? "").Trim();
                        descriptionValue_medical_product =
                            ((string)c.SelectToken("values.value.descriptionValue") ?? "").Trim();
                    }
                    else if (n.Contains(name_classifier_program))
                    {
                        code_program = ((string)c.SelectToken("values.value.code") ?? "").Trim();
                        name_program = ((string)c.SelectToken("values.value.name") ?? "").Trim();
                    }
                }

                var name_rubricatorInfo = ((string)m.SelectToken("rubricators.rubricatorInfo.name") ?? "").Trim();
                var code_parentPositionInfo = ((string)m.SelectToken("parentPositionInfo.code") ?? "").Trim();
                var version_parentPositionInfo = ((string)m.SelectToken("parentPositionInfo.version") ?? "").Trim();
                var name_parentPositionInfo = ((string)m.SelectToken("parentPositionInfo.name") ?? "").Trim();
                var insert =
                    "INSERT INTO nsi_ktru (id, code, version, inclusionDate, publishDate, name_ktru, actual, applicationDateStart, applicationDateEnd, cancelDate, cancelReason, isTemplate, externalCode, codeOKEI, nameOKEI, docName_standards_NSI, name_classifier_OKVED, code_OKVED, name_OKVED, name_classifier_OKPD2, code_OKPD2, name_OKPD2, name_classifier_medical_product, code_medical_product, name_medical_product, descriptionValue_medical_product, name_classifier_program, code_program, name_program, name_rubricatorInfo, code_parentPositionInfo, version_parentPositionInfo, name_parentPositionInfo) VALUES (null,@code,@version,@inclusiondate,@publishdate,@nameKtru,@actual,@applicationdatestart,@applicationdateend,@canceldate,@cancelreason,@istemplate,@externalcode,@codeokei,@nameokei,@docnameStandardsNsi,@nameClassifierOkved,@codeOkved,@nameOkved,@nameClassifierOkpd2,@codeOkpd2,@nameOkpd2,@nameClassifierMedicalProduct,@codeMedicalProduct,@nameMedicalProduct,@descriptionvalueMedicalProduct,@nameClassifierProgram,@codeProgram,@nameProgram,@nameRubricatorinfo,@codeParentpositioninfo,@versionParentpositioninfo,@nameParentpositioninfo)";
                var cmd9 = new MySqlCommand(insert, connect);
                cmd9.Prepare();
                cmd9.Parameters.AddWithValue("@code", code);
                cmd9.Parameters.AddWithValue("@version", version);
                cmd9.Parameters.AddWithValue("@inclusiondate", inclusionDate);
                cmd9.Parameters.AddWithValue("@publishdate", publishDate);
                cmd9.Parameters.AddWithValue("@nameKtru", name_ktru);
                cmd9.Parameters.AddWithValue("@actual", actual);
                cmd9.Parameters.AddWithValue("@applicationdatestart", applicationDateStart);
                cmd9.Parameters.AddWithValue("@applicationdateend", applicationDateEnd);
                cmd9.Parameters.AddWithValue("@canceldate", cancelDate);
                cmd9.Parameters.AddWithValue("@cancelreason", cancelReason);
                cmd9.Parameters.AddWithValue("@istemplate", isTemplate);
                cmd9.Parameters.AddWithValue("@externalcode", externalCode);
                cmd9.Parameters.AddWithValue("@codeokei", codeOKEI);
                cmd9.Parameters.AddWithValue("@nameokei", nameOKEI);
                cmd9.Parameters.AddWithValue("@docnameStandardsNsi", docName_standards_NSI);
                cmd9.Parameters.AddWithValue("@nameClassifierOkved", name_classifier_OKVED);
                cmd9.Parameters.AddWithValue("@codeOkved", code_OKVED);
                cmd9.Parameters.AddWithValue("@nameOkved", name_OKVED);
                cmd9.Parameters.AddWithValue("@nameClassifierOkpd2", name_classifier_OKPD2);
                cmd9.Parameters.AddWithValue("@codeOkpd2", code_OKPD2);
                cmd9.Parameters.AddWithValue("@nameOkpd2", name_OKPD2);
                cmd9.Parameters.AddWithValue("@nameClassifierMedicalProduct", name_classifier_medical_product);
                cmd9.Parameters.AddWithValue("@codeMedicalProduct", code_medical_product);
                cmd9.Parameters.AddWithValue("@nameMedicalProduct", name_medical_product);
                cmd9.Parameters.AddWithValue("@descriptionvalueMedicalProduct", descriptionValue_medical_product);
                cmd9.Parameters.AddWithValue("@nameClassifierProgram", name_classifier_program);
                cmd9.Parameters.AddWithValue("@codeProgram", code_program);
                cmd9.Parameters.AddWithValue("@nameProgram", name_program);
                cmd9.Parameters.AddWithValue("@nameRubricatorinfo", name_rubricatorInfo);
                cmd9.Parameters.AddWithValue("@codeParentpositioninfo", code_parentPositionInfo);
                cmd9.Parameters.AddWithValue("@versionParentpositioninfo", version_parentPositionInfo);
                cmd9.Parameters.AddWithValue("@nameParentpositioninfo", name_parentPositionInfo);
                var resInsertTender = cmd9.ExecuteNonQuery();
                var idKtru = (int)cmd9.LastInsertedId;
                Add?.Invoke(resInsertTender);
                var characteristics = GetElements(m, "characteristics.characteristic");
                foreach (var ch in characteristics)
                    try
                    {
                        insertChar(ch, connect, idKtru, m);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
            }
        }

        private void insertChar(JToken pos, MySqlConnection connect, int idKtru, JToken m)
        {
            var code = ((string)pos.SelectToken("code") ?? "").Trim();
            var name = ((string)pos.SelectToken("name") ?? "").Trim();
            var type = ((string)pos.SelectToken("type") ?? "").Trim();
            var kind = ((string)pos.SelectToken("kind") ?? "").Trim();
            var actual = (bool?)pos.SelectToken("actual") ?? false;
            var isRequired = (bool?)pos.SelectToken("isRequired") ?? false;
            var choiceType = ((string)pos.SelectToken("choiceType") ?? "").Trim();
            var insertN =
                "INSERT INTO nsi_ktru_characteristics (id, id_nsi_ktru, code, name, type, kind, actual, isRequired, choiceType) VALUES (null,@idNsiKtru,@code,@name,@type,@kind,@actual,@isrequired,@choicetype)";
            var cmd10 = new MySqlCommand(insertN, connect);
            cmd10.Prepare();
            cmd10.Parameters.AddWithValue("@idNsiKtru", idKtru);
            cmd10.Parameters.AddWithValue("@code", code);
            cmd10.Parameters.AddWithValue("@name", name);
            cmd10.Parameters.AddWithValue("@type", type);
            cmd10.Parameters.AddWithValue("@kind", kind);
            cmd10.Parameters.AddWithValue("@actual", actual);
            cmd10.Parameters.AddWithValue("@isrequired", isRequired);
            cmd10.Parameters.AddWithValue("@choicetype", choiceType);
            var resInsertTender = cmd10.ExecuteNonQuery();
            var idCh = (int)cmd10.LastInsertedId;
            var values = GetElements(pos, "values.value");
            foreach (var v in values)
                try
                {
                    var qualityDescription = ((string)v.SelectToken("qualityDescription") ?? "").Trim();
                    var codeOKEI = ((string)v.SelectToken("OKEI.code") ?? "").Trim();
                    var nameOKEI = ((string)v.SelectToken("OKEI.name") ?? "").Trim();
                    var valueFormat = ((string)v.SelectToken("valueFormat") ?? "").Trim();
                    var minMathNotation = ((string)v.SelectToken("rangeSet.valueRange.minMathNotation") ?? "").Trim();
                    var min = (decimal?)v.SelectToken("rangeSet.valueRange.min") ?? 0.0m;
                    var maxMathNotation = ((string)v.SelectToken("rangeSet.valueRange.maxMathNotation") ?? "").Trim();
                    var max = (decimal?)v.SelectToken("rangeSet.valueRange.max") ?? 0.0m;
                    var concreteValue = (decimal?)v.SelectToken("valueSet.concreteValue") ?? 0.0m;
                    var insertV =
                        "INSERT INTO nsi_ktru_values (id, id_nsi_characteristic, qualityDescription, codeOKEI, nameOKEI, valueFormat, minMathNotation, min, maxMathNotation, max, concreteValue) VALUES (null,@idNsiCharacteristic,@qualitydescription,@codeokei,@nameokei,@valueformat,@minmathnotation,@min,@maxmathnotation,@max,@concretevalue)";
                    var cmd11 = new MySqlCommand(insertV, connect);
                    cmd11.Prepare();
                    cmd11.Parameters.AddWithValue("@idNsiCharacteristic", idCh);
                    cmd11.Parameters.AddWithValue("@qualityDescription", qualityDescription);
                    cmd11.Parameters.AddWithValue("@codeokei", codeOKEI);
                    cmd11.Parameters.AddWithValue("@nameokei", nameOKEI);
                    cmd11.Parameters.AddWithValue("@valueformat", valueFormat);
                    cmd11.Parameters.AddWithValue("@minmathnotation", minMathNotation);
                    cmd11.Parameters.AddWithValue("@min", min);
                    cmd11.Parameters.AddWithValue("@maxmathnotation", maxMathNotation);
                    cmd11.Parameters.AddWithValue("@max", max);
                    cmd11.Parameters.AddWithValue("@concretevalue", concreteValue);
                    var resInsert = cmd11.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
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
                switch (elsObj.Type)
                {
                    case JTokenType.Object:
                        els.Add(elsObj);
                        break;
                    case JTokenType.Array:
                        els.AddRange(elsObj);
                        break;
                }

            return els;
        }
    }
}