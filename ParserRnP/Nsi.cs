using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace ParserRnP
{
    public class Nsi
    {
        protected readonly JObject T;
        protected readonly FileInfo File;
        protected readonly string FilePath;
        
        public event Action<int> AddNsi;
        public event Action<int> UpdateNsi;
        
        public Nsi(FileInfo f, JObject json)
        {
            T = json;
            File = f;
            FilePath = File.ToString();
            
            AddNsi += delegate(int d)
            {
                if (d > 0)
                    Program.AddNsi++;
                else
                    Log.Logger("Не удалось добавить Nsi", FilePath);
            };
            
            UpdateNsi += delegate(int d)
            {
                if (d > 0)
                    Program.UpdateNsi++;
                else
                    Log.Logger("Не удалось обновить Nsi", FilePath);
            };
        }
        
        public void Parsing()
        {
            var xml = GetXml(File.ToString());
            var root = (JObject) T.SelectToken("export");
            var el = GetElements(root, "nsiOKEIList.nsiOKEI");
            foreach (var  nsi in el)
            {
                var code =  ((string) nsi.SelectToken("code") ?? "").Trim();
                var fullName =  ((string) nsi.SelectToken("fullName") ?? "").Trim();
                var sectionCode =  ((string) nsi.SelectToken("section.code") ?? "").Trim();
                var sectionName =  ((string) nsi.SelectToken("section.name") ?? "").Trim();
                var groupCode =  ((string) nsi.SelectToken("group.id") ?? "").Trim();
                var groupName =  ((string) nsi.SelectToken("group.name") ?? "").Trim();
                var localName =  ((string) nsi.SelectToken("localName") ?? "").Trim();
                var InternationalName =  ((string) nsi.SelectToken("internationalName") ?? "").Trim();
                var localSymbol =  ((string) nsi.SelectToken("localSymbol") ?? "").Trim();
                var internationalSymbol =  ((string) nsi.SelectToken("internationalSymbol") ?? "").Trim();
                var trueNationalCode = ((string)nsi.SelectToken("trueNationalCode") ?? "").Trim();
                var actual =  ((string) nsi.SelectToken("actual") ?? "").Trim();
                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    var id = 0;
                    var selectBank =
                        $"SELECT id_okei FROM nsi_okei WHERE code = @code AND fullName= @fullName";
                    var cmd2 = new MySqlCommand(selectBank, connect);
                    cmd2.Prepare();
                    cmd2.Parameters.AddWithValue("@code", code);
                    cmd2.Parameters.AddWithValue("@fullName", fullName);
                    var reader = cmd2.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        id = reader.GetInt32("id_okei");
                        
                    }
                    reader.Close();
                    if (id != 0)
                    {
                        var insertBank =
                            $"UPDATE nsi_okei SET code= @code,fullName= @fullName,section_code=@section_code,section_name=@section_name,group_code=@group_code,group_name=@group_name,localName=@localName,internationalName=@internationalName,localSymbol=@localSymbol,internationalSymbol=@internationalSymbol,trueNationalCode=@trueNationalCode,actual=@actual WHERE id_okei = @id_okei";
                        var cmd3 = new MySqlCommand(insertBank, connect);
                        cmd3.Prepare();
                        cmd3.Parameters.AddWithValue("@code", code);
                        cmd3.Parameters.AddWithValue("@fullName", fullName);
                        cmd3.Parameters.AddWithValue("@section_code", sectionCode);
                        cmd3.Parameters.AddWithValue("@section_name", sectionName);
                        cmd3.Parameters.AddWithValue("@group_code", groupCode);
                        cmd3.Parameters.AddWithValue("@group_name", groupName);
                        cmd3.Parameters.AddWithValue("@localName", localName);
                        cmd3.Parameters.AddWithValue("@internationalName", InternationalName);
                        cmd3.Parameters.AddWithValue("@localSymbol", localSymbol);
                        cmd3.Parameters.AddWithValue("@internationalSymbol", internationalSymbol);
                        cmd3.Parameters.AddWithValue("@trueNationalCode", trueNationalCode);
                        cmd3.Parameters.AddWithValue("@actual", actual);
                        cmd3.Parameters.AddWithValue("@id_okei", id);
                        cmd3.ExecuteNonQuery();
                        UpdateNsi?.Invoke(1);
                   }
                    else
                    {
                        var insertBank =
                            $"INSERT INTO nsi_okei SET code= @code,fullName= @fullName,section_code=@section_code,section_name=@section_name,group_code=@group_code,group_name=@group_name,localName=@localName,internationalName=@internationalName,localSymbol=@localSymbol,internationalSymbol=@internationalSymbol,trueNationalCode=@trueNationalCode,actual=@actual";
                        var cmd3 = new MySqlCommand(insertBank, connect);
                        cmd3.Prepare();
                        cmd3.Parameters.AddWithValue("@code", code);
                        cmd3.Parameters.AddWithValue("@fullName", fullName);
                        cmd3.Parameters.AddWithValue("@section_code", sectionCode);
                        cmd3.Parameters.AddWithValue("@section_name", sectionName);
                        cmd3.Parameters.AddWithValue("@group_code", groupCode);
                        cmd3.Parameters.AddWithValue("@group_name", groupName);
                        cmd3.Parameters.AddWithValue("@localName", localName);
                        cmd3.Parameters.AddWithValue("@internationalName", InternationalName);
                        cmd3.Parameters.AddWithValue("@localSymbol", localSymbol);
                        cmd3.Parameters.AddWithValue("@internationalSymbol", internationalSymbol);
                        cmd3.Parameters.AddWithValue("@trueNationalCode", trueNationalCode);
                        cmd3.Parameters.AddWithValue("@actual", actual);
                        cmd3.ExecuteNonQuery();
                        AddNsi?.Invoke(1);
                    }
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