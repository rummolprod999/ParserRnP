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

namespace ParserRnP
{
    public class ParserBank : Parser
    {
        private string[] file_bank = new[]
        {
            "bank_", "fcsgaranteeinfo_"
        };
        public ParserBank(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            List<String> arch = new List<string>();
            List<String> bank_list = new List<string>();
            string PathParse = $"/fcs_banks/";;

            switch (Program.Periodparsing)
            {
                case TypeArguments.LastBank:;
                    bank_list = GetListBank(PathParse);
                    foreach (var l in bank_list)
                    {
                        PathParse = $"/fcs_banks/{l}/";
                        arch = GetListArchLast(PathParse);
                        ParserList(arch, PathParse);
                    }
                    break;
                case TypeArguments.RootBank:
                    arch = GetListArchRoot(PathParse);
                    ParserList(arch, PathParse);
                    break;
                case TypeArguments.CurrBank:
                    bank_list = GetListBank(PathParse);
                    foreach (var l in bank_list)
                    {
                        PathParse = $"/fcs_banks/{l}/currMonth/";
                        arch = GetListArchCurr(PathParse);
                        ParserList(arch, PathParse);
                    }
                    break;
                case TypeArguments.PrevBank:
                    bank_list = GetListBank(PathParse);
                    foreach (var l in bank_list)
                    {
                        PathParse = $"/fcs_banks/{l}/prevMonth/";
                        arch = GetListArchCurr(PathParse);
                        ParserList(arch, PathParse);
                    }
                    break;
            }
        }

        public void ParserList(List<String> arch, string PathParse)
        {
            if (arch.Count == 0)
            {
                Log.Logger("Получен пустой список архивов", PathParse);
            }

            foreach (var v in arch)
            {
                GetListFileArch(v, PathParse);
                //Console.WriteLine(v);
            }
        }
        
        public override void GetListFileArch(string Arch, string PathParse)
        {
            string filea = "";
            string path_unzip = "";
            filea = GetArch44(Arch, PathParse);
            if (!String.IsNullOrEmpty(filea))
            {
                path_unzip = Unzipped.Unzip(filea);
                if (path_unzip != "")
                {
                    if (Directory.Exists(path_unzip))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(path_unzip);
                        FileInfo[] filelist = dirInfo.GetFiles();
                        List<FileInfo> array_xml_bank = filelist
                            .Where(a => file_bank.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        foreach (var f in array_xml_bank)
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
                ParsingXML(f, typefile);
            }
            catch (Exception e)
            {
                Log.Logger("Ошибка при парсинге xml", e, f);
            }
        }
        
        public void ParsingXML(FileInfo f, TypeFileBank typefile)
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
        
        public override List<String> GetListArchCurr(string PathParse)
        {
            List<String> arch = new List<string>();
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(PathParse);
            foreach (var a in archtemp.Where(a =>
                file_bank.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)))
            {
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();
                    string select_arch =
                        $"SELECT id FROM {Program.Prefix}arhiv_bank WHERE arhiv = @archive";
                    MySqlCommand cmd = new MySqlCommand(select_arch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", a);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool res_read = reader.HasRows;
                    reader.Close();
                    if (!res_read)
                    {
                        string add_arch =
                            $"INSERT INTO {Program.Prefix}arhiv_bank SET arhiv = @archive";
                        MySqlCommand cmd1 = new MySqlCommand(add_arch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", a);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }
            }

            return arch;
        }
        
        public List<String> GetListArchRoot(string PathParse)
        {
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(PathParse);
            archtemp = archtemp.Where(a => a.EndsWith(".zip"))
                .ToList();
            return archtemp.Where(a => file_bank.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }
        
        public override List<String> GetListArchLast(string PathParse)
        {
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(PathParse);
            archtemp = archtemp.Where(a => a.EndsWith(".zip"))
                .ToList();
            return archtemp.Where(a => file_bank.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public List<String> GetListBank(string PathParse)
        {
            List<string> listtemp = new List<string>();
            listtemp = GetListFtp44(PathParse);
            listtemp = listtemp.Where(a => !a.EndsWith(".zip"))
                .ToList();
            return listtemp;
        }
        
        private List<string> GetListFtp44(string PathParse)
        {
            List<string> archtemp = new List<string>();
            int count = 1;
            while (true)
            {
                try
                {
                    WorkWithFtp ftp = ClientFtp44_old();
                    ftp.ChangeWorkingDirectory(PathParse);
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
                        Log.Logger($"Не смогли найти директорию после попытки {count}", PathParse, e);
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