#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class ParserComplaintResult : Parser
    {
        protected DataTable DtRegion;

        private readonly string[] _fileUcomplaintRes =
        {
            "checkresult_", "checkresultcancel_"
        };

        private readonly string[] _fileCancelRes = { "checkresultcancel_" };
        private readonly string[] _fileComplaintRes = { "checkresult_" };

        public ParserComplaintResult(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            var arch = new List<string>();
            var pathParse = "";
            switch (Program.Periodparsing)
            {
                case TypeArguments.LastComplaintRes:
                    pathParse = "/fcs_fas/checkResult/";
                    arch = GetListArchLast(pathParse);
                    break;
                case TypeArguments.CurrComplaintRes:
                    pathParse = "/fcs_fas/checkResult/currMonth/";
                    arch = GetListArchCurr(pathParse);
                    break;
                case TypeArguments.PrevComplaintRes:
                    pathParse = "/fcs_fas/checkResult/prevMonth/";
                    arch = GetListArchPrev(pathParse);
                    break;
            }

            if (arch.Count == 0) Log.Logger("Получен пустой список архивов", pathParse);

            foreach (var v in arch) GetListFileArch(v, pathParse);
        }

        public override void GetListFileArch(string arch, string pathParse)
        {
            var filea = "";
            var pathUnzip = "";
            filea = GetArch44(arch, pathParse);
            if (!string.IsNullOrEmpty(filea))
            {
                pathUnzip = Unzipped.Unzip(filea);
                if (pathUnzip != "")
                    if (Directory.Exists(pathUnzip))
                    {
                        var dirInfo = new DirectoryInfo(pathUnzip);
                        var filelist = dirInfo.GetFiles();
                        var arrayXmlCancelRes = filelist
                            .Where(a => _fileCancelRes.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        var arrayXmlComplaintRes = filelist
                            .Where(a => _fileComplaintRes.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        foreach (var f in arrayXmlComplaintRes) Bolter(f, TypeFileComplaintRes.ComplaintRes);

                        foreach (var f in arrayXmlCancelRes) Bolter(f, TypeFileComplaintRes.CancelRes);

                        dirInfo.Delete(true);
                    }
            }
        }

        public override void Bolter(FileInfo f, TypeFileComplaintRes typefile)
        {
            if (!f.Name.ToLower().EndsWith(".xml", StringComparison.Ordinal)) return;

            try
            {
                ParsingXml(f, typefile);
            }
            catch (Exception e)
            {
                Log.Logger("Ошибка при парсинге xml", e, f);
            }
        }

        public void ParsingXml(FileInfo f, TypeFileComplaintRes typefile)
        {
            using (var sr = new StreamReader(f.ToString(), Encoding.Default))
            {
                var ftext = sr.ReadToEnd();
                ftext = ClearText.ClearString(ftext);
                var doc = new XmlDocument();
                doc.LoadXml(ftext);
                var jsons = JsonConvert.SerializeXmlNode(doc);
                var json = JObject.Parse(jsons);
                switch (typefile)
                {
                    case TypeFileComplaintRes.ComplaintRes:
                        var a = new ComplaintRes44(f, json);
                        a.Parsing();
                        break;
                    case TypeFileComplaintRes.CancelRes:
                        var b = new ComplaintCancelRes(f, json);
                        b.Parsing();
                        break;
                }
            }
        }

        public override List<string> GetListArchLast(string pathParse)
        {
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            return archtemp.Where(a =>
                    _fileUcomplaintRes.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public override List<string> GetListArchPrev(string pathParse)
        {
            var arch = new List<string>();
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            var serachd = $"{Program.LocalDate:yyyyMMdd}";
            foreach (var a in archtemp.Where(a => a.ToLower().IndexOf(serachd, StringComparison.Ordinal) != -1))
            {
                var prevA = $"prev_{a}";
                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    var selectArch =
                        $"SELECT id FROM {Program.Prefix}arhiv_complaint_result WHERE arhiv = @archive";
                    var cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", prevA);
                    var reader = cmd.ExecuteReader();
                    var resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        var addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_complaint_result SET arhiv = @archive";
                        var cmd1 = new MySqlCommand(addArch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", prevA);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }
            }

            return arch;
        }

        public override List<string> GetListArchCurr(string pathParse)
        {
            var arch = new List<string>();
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            foreach (var a in archtemp.Where(a =>
                         _fileUcomplaintRes.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)))
                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    var selectArch =
                        $"SELECT id FROM {Program.Prefix}arhiv_complaint_result WHERE arhiv = @archive";
                    var cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", a);
                    var reader = cmd.ExecuteReader();
                    var resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        var addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_complaint_result SET arhiv = @archive";
                        var cmd1 = new MySqlCommand(addArch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", a);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }

            return arch;
        }

        private List<string> GetListFtp44(string pathParse)
        {
            var archtemp = new List<string>();
            var count = 1;
            while (true)
                try
                {
                    var ftp = ClientFtp44_old();
                    ftp.ChangeWorkingDirectory(pathParse);
                    archtemp = ftp.ListDirectory();
                    if (count > 1) Log.Logger("Удалось получить список архивов после попытки", count);

                    break;
                }
                catch (Exception e)
                {
                    if (count > 3)
                    {
                        Log.Logger($"Не смогли найти директорию после попытки {count}", pathParse, e);
                        break;
                    }

                    count++;
                    Thread.Sleep(2000);
                }

            return archtemp;
        }
    }
}