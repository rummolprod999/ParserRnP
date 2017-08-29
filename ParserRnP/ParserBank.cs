using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using FluentFTP;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ParserRnP
{
    public class ParserBank : Parser
    {
        private string[] _fileBank = new[]
        {
            "bank_", "fcsgaranteeinfo_"
        };

        public ParserBank(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            List<String> arch = new List<string>();
            List<String> bankList = new List<string>();
            string pathParse = $"/fcs_banks/";
            ;

            switch (Program.Periodparsing)
            {
                case TypeArguments.LastBank:
                    bankList = GetListBank(pathParse);
                    foreach (var l in bankList)
                    {
                        pathParse = $"/fcs_banks/{l}/";
                        arch = GetListArchLast(pathParse);
                        ParserList(arch, pathParse);
                    }
                    break;
                case TypeArguments.RootBank:
                    arch = GetListArchRoot(pathParse);
                    ParserList(arch, pathParse);
                    break;
                case TypeArguments.CurrBank:
                    bankList = GetListBank(pathParse);
                    foreach (var l in bankList)
                    {
                        pathParse = $"/fcs_banks/{l}/currMonth/";
                        arch = GetListArchCurr(pathParse);
                        ParserList(arch, pathParse);
                    }
                    break;
                case TypeArguments.PrevBank:
                    bankList = GetListBank(pathParse);
                    foreach (var l in bankList)
                    {
                        pathParse = $"/fcs_banks/{l}/prevMonth/";
                        arch = GetListArchCurr(pathParse);
                        ParserList(arch, pathParse);
                    }
                    break;
            }
        }

        public void ParserList(List<String> arch, string pathParse)
        {
            if (arch.Count == 0)
            {
                Log.Logger("Получен пустой список архивов", pathParse);
            }

            foreach (var v in arch)
            {
                GetListFileArch(v, pathParse);
                //Console.WriteLine(v);
            }
        }

        public override void GetListFileArch(string arch, string pathParse)
        {
            string filea = "";
            string pathUnzip = "";
            filea = GetArch44(arch, pathParse);
            if (!String.IsNullOrEmpty(filea))
            {
                pathUnzip = Unzipped.Unzip(filea);
                if (pathUnzip != "")
                {
                    if (Directory.Exists(pathUnzip))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(pathUnzip);
                        FileInfo[] filelist = dirInfo.GetFiles();
                        List<FileInfo> arrayXmlBank = filelist
                            .Where(a => _fileBank.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        foreach (var f in arrayXmlBank)
                        {
                            Bolter(f, TypeFileBank.Bank);
                        }
                        dirInfo.Delete(true);
                    }
                }
            }
        }

        public override void Bolter(FileInfo f, TypeFileBank typefile)
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

        public void ParsingXml(FileInfo f, TypeFileBank typefile)
        {
            using (StreamReader sr = new StreamReader(f.ToString(), Encoding.Default))
            {
                var ftext = sr.ReadToEnd();
                ftext = ClearText.ClearString(ftext);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ftext);
                string jsons = JsonConvert.SerializeXmlNode(doc);
                JObject json = JObject.Parse(jsons);
                switch (typefile)
                {
                    case TypeFileBank.Bank:
                        Bank44 b = new Bank44(f, json);
                        b.Parsing();
                        break;
                }
            }
        }

        public override List<String> GetListArchCurr(string pathParse)
        {
            List<String> arch = new List<string>();
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44New(pathParse);
            foreach (var a in archtemp.Where(a =>
                _fileBank.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)))
            {
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    string selectArch =
                        $"SELECT id FROM {Program.Prefix}arhiv_bank WHERE arhiv = @archive";
                    MySqlCommand cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", a);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        string addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_bank SET arhiv = @archive";
                        MySqlCommand cmd1 = new MySqlCommand(addArch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", a);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }
            }

            return arch;
        }

        public List<String> GetListArchRoot(string pathParse)
        {
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44New(pathParse);
            archtemp = archtemp.Where(a => a.EndsWith(".zip"))
                .ToList();
            return archtemp.Where(a => _fileBank.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public override List<String> GetListArchLast(string pathParse)
        {
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44New(pathParse);
            archtemp = archtemp.Where(a => a.EndsWith(".zip"))
                .ToList();
            return archtemp.Where(a => _fileBank.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public List<String> GetListBank(string pathParse)
        {
            List<string> listtemp = new List<string>();
            listtemp = GetListFtp44(pathParse);
            listtemp = listtemp.Where(a => !a.EndsWith(".zip"))
                .ToList();
            return listtemp;
        }

        private List<string> GetListFtp44(string pathParse)
        {
            List<string> archtemp = new List<string>();
            int count = 1;
            while (true)
            {
                try
                {
                    WorkWithFtp ftp = ClientFtp44_old();
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

        private List<string> GetListFtp44New(string pathParse)
        {
            List<string> archtemp = new List<string>();

            int count = 1;
            while (true)
            {
                try
                {
                    FtpClient ftp = ClientFtp44();

                    FtpListItem[] l = ftp.GetListing(pathParse);
                    if (count > 1)
                    {
                        Log.Logger("Удалось получить список архивов после попытки", count);
                    }

                    archtemp = l.Where(i => i.Size > 22).Select(i => i.Name).ToList();

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
    }
}