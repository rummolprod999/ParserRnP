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
    public class ParserUnFair : Parser
    {
        protected DataTable DtRegion;

        private string[] file_unfair = new[]
        {
            "unfairsupplier_"
        };

        public ParserUnFair(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            List<String> arch = new List<string>();
            string PathParse = "";

            switch (Program.Periodparsing)
            {
                case TypeArguments.Last:
                    PathParse = $"/fcs_fas/unfairSupplier";
                    arch = GetListArchLast(PathParse);
                    break;
                case TypeArguments.Curr:
                    PathParse = $"/fcs_fas/unfairSupplier/currMonth/";
                    arch = GetListArchCurr(PathParse);
                    break;
                case TypeArguments.Prev:
                    PathParse = $"/fcs_fas/unfairSupplier/prevMonth/";
                    arch = GetListArchPrev(PathParse);
                    break;
            }

            if (arch.Count == 0)
            {
                Log.Logger("Получен пустой список архивов", PathParse);
            }

            foreach (var v in arch)
            {
                GetListFileArch(v, PathParse);
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
                        List<FileInfo> array_xml_unfair = filelist
                            .Where(a => file_unfair.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        foreach (var f in array_xml_unfair)
                        {
                            Bolter(f, TypeFileRnp.unfairSupplier);
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
                ParsingXML(f, typefile);
            }
            catch (Exception e)
            {
                Log.Logger("Ошибка при парсинге xml", e, f);
            }
        }

        public void ParsingXML(FileInfo f, TypeFileRnp typefile)
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
                    case TypeFileRnp.unfairSupplier:
                        Unfair44 a = new Unfair44(f, json);
                        a.Parsing();
                        break;
                }
            }
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

        public override List<String> GetListArchLast(string PathParse)
        {
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(PathParse);
            return archtemp.Where(a => file_unfair.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)).ToList();
        }

        public override List<String> GetListArchCurr(string PathParse)
        {
            List<String> arch = new List<string>();
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(PathParse);
            foreach (var a in archtemp.Where(a =>
                file_unfair.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)))
            {
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();
                    string select_arch =
                        $"SELECT id FROM {Program.Prefix}arhiv_unfair_suppliers WHERE arhiv = @archive";
                    MySqlCommand cmd = new MySqlCommand(select_arch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", a);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool res_read = reader.HasRows;
                    reader.Close();
                    if (!res_read)
                    {
                        string add_arch =
                            $"INSERT INTO {Program.Prefix}arhiv_unfair_suppliers SET arhiv = @archive";
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

        public override List<String> GetListArchPrev(string PathParse)
        {
            List<String> arch = new List<string>();
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(PathParse);
            string serachd = $"{Program.LocalDate:yyyyMMdd}";
            foreach (var a in archtemp.Where(a => a.ToLower().IndexOf(serachd, StringComparison.Ordinal) != -1))
            {
                string prev_a = $"prev_{a}";
                using (MySqlConnection connect = ConnectToDb.GetDBConnection())
                {
                    connect.Open();
                    string select_arch =
                        $"SELECT id FROM {Program.Prefix}arhiv_unfair_suppliers WHERE arhiv = @archive";
                    MySqlCommand cmd = new MySqlCommand(select_arch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", prev_a);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool res_read = reader.HasRows;
                    reader.Close();
                    if (!res_read)
                    {
                        string add_arch =
                            $"INSERT INTO {Program.Prefix}arhiv_unfair_suppliers SET arhiv = @archive";
                        MySqlCommand cmd1 = new MySqlCommand(add_arch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", prev_a);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }
            }

            return arch;
        }
    }
}