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
    public class ParserUnFair : Parser
    {
        protected DataTable DtRegion;

        private readonly string[] _fileUnfair =
        {
            "unfairsupplier_"
        };

        public ParserUnFair(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            var arch = new List<string>();
            var pathParse = "";

            switch (Program.Periodparsing)
            {
                case TypeArguments.LastUn:
                    pathParse = "/fcs_fas/unfairSupplier/";
                    arch = GetListArchLast(pathParse);
                    break;
                case TypeArguments.RootUn:
                    pathParse = "/fcs_fas/";
                    arch = GetListArchRoot(pathParse);
                    break;
                case TypeArguments.CurrUn:
                    pathParse = "/fcs_fas/unfairSupplier/currMonth/";
                    arch = GetListArchCurr(pathParse);
                    break;
                case TypeArguments.PrevUn:
                    pathParse = "/fcs_fas/unfairSupplier/prevMonth/";
                    arch = GetListArchPrev(pathParse);
                    break;
            }

            if (arch.Count == 0)
            {
                Log.Logger("Получен пустой список архивов", pathParse);
            }

            foreach (var v in arch)
            {
                GetListFileArch(v, pathParse);
            }
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
                {
                    if (Directory.Exists(pathUnzip))
                    {
                        var dirInfo = new DirectoryInfo(pathUnzip);
                        var filelist = dirInfo.GetFiles();
                        var arrayXmlUnfair = filelist
                            .Where(a => _fileUnfair.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        foreach (var f in arrayXmlUnfair)
                        {
                            Bolter(f, TypeFileRnp.UnfairSupplier);
                        }

                        dirInfo.Delete(true);
                    }
                }
            }
        }

        public override void Bolter(FileInfo f, TypeFileRnp typefile)
        {
            if (!f.Name.ToLower().EndsWith(".xml", StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                ParsingXml(f, typefile);
            }
            catch (Exception e)
            {
                Log.Logger("Ошибка при парсинге xml", e, f);
            }
        }

        public void ParsingXml(FileInfo f, TypeFileRnp typefile)
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
                    case TypeFileRnp.UnfairSupplier:
                        var a = new Unfair44(f, json);
                        a.Parsing();
                        break;
                }
            }
        }

        private List<string> GetListFtp44(string pathParse)
        {
            var archtemp = new List<string>();
            var count = 1;
            while (true)
            {
                try
                {
                    var ftp = ClientFtp44_old();
                    ftp.ChangeWorkingDirectory(pathParse);
                    archtemp = ftp.ListDirectory();
                    if (count > 1)
                    {
                        Log.Logger("Удалось получить список архивов после попытки", count);
                    }

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
            }

            return archtemp;
        }

        public override List<string> GetListArchLast(string pathParse)
        {
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            return archtemp.Where(a => _fileUnfair.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public List<string> GetListArchRoot(string pathParse)
        {
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            return archtemp.Where(a => _fileUnfair.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public override List<string> GetListArchCurr(string pathParse)
        {
            var arch = new List<string>();
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            foreach (var a in archtemp.Where(a =>
                         _fileUnfair.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)))
            {
                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    var selectArch =
                        $"SELECT id FROM {Program.Prefix}arhiv_unfair_suppliers WHERE arhiv = @archive";
                    var cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", a);
                    var reader = cmd.ExecuteReader();
                    var resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        var addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_unfair_suppliers SET arhiv = @archive";
                        var cmd1 = new MySqlCommand(addArch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", a);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }
            }

            return arch;
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
                        $"SELECT id FROM {Program.Prefix}arhiv_unfair_suppliers WHERE arhiv = @archive";
                    var cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", prevA);
                    var reader = cmd.ExecuteReader();
                    var resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        var addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_unfair_suppliers SET arhiv = @archive";
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
    }
}