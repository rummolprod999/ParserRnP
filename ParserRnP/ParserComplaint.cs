﻿using System;
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
    public class ParserComplaint : Parser
    {
        protected DataTable DtRegion;

        private string[] _fileUcomplaint = new[]
        {
            "complaintcancel_", "complaint_", "tendersuspension_"
        };
        private string[] _fileCancel = new[] {"complaintcancel_"};
        private string[] _fileComplaint = new[] {"complaint_"};
        private string[] _fileSuspend = new[] {"tendersuspension_"};

        public ParserComplaint(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            List<String> arch = new List<string>();
            string pathParse = "";
            switch (Program.Periodparsing)
            {
                case TypeArguments.LastComplaint:
                    pathParse = $"/fcs_fas/complaint/";
                    arch = GetListArchLast(pathParse);
                    break;
                case TypeArguments.CurrComplaint:
                    pathParse = $"/fcs_fas/complaint/currMonth/";
                    arch = GetListArchCurr(pathParse);
                    break;
                case TypeArguments.PrevComplaint:
                    pathParse = $"/fcs_fas/complaint/prevMonth/";
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
                        List<FileInfo> arrayXmlCancel = filelist
                            .Where(a => _fileCancel.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        List<FileInfo> arrayXmlComplaint = filelist
                            .Where(a => _fileComplaint.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        List<FileInfo> arrayXmlSuspend = filelist
                            .Where(a => _fileSuspend.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        foreach (var f in arrayXmlComplaint)
                        {
                            Bolter(f, TypeFileComplaint.Complaint);
                        }
                        
                        foreach (var f in arrayXmlCancel)
                        {
                            Bolter(f, TypeFileComplaint.Cancel);
                        }
                        
                        foreach (var f in arrayXmlSuspend)
                        {
                            Bolter(f, TypeFileComplaint.Suspend);
                        }
                        dirInfo.Delete(true);
                    }
                }
            }
        }
        
        public override void Bolter(FileInfo f, TypeFileComplaint typefile)
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
        
        public void ParsingXml(FileInfo f, TypeFileComplaint typefile)
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
                    case TypeFileComplaint.Complaint:
                        Complaint44 a = new Complaint44(f, json);
                        a.Parsing();
                        break;
                    case TypeFileComplaint.Cancel:
                        ComplaintCancel b = new ComplaintCancel(f, json);
                        b.Parsing();
                        break;
                    case TypeFileComplaint.Suspend:
                        ComplaintSuspend c = new ComplaintSuspend(f, json);
                        c.Parsing();
                        break;
                }
            }
        }

        public override List<String> GetListArchLast(string pathParse)
        {
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            return archtemp.Where(a => _fileUcomplaint.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }
        
        public override List<String> GetListArchCurr(string pathParse)
        {
            List<String> arch = new List<string>();
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            foreach (var a in archtemp.Where(a =>
                _fileUcomplaint.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1)))
            {
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    string selectArch =
                        $"SELECT id FROM {Program.Prefix}arhiv_complaint WHERE arhiv = @archive";
                    MySqlCommand cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", a);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        string addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_complaint SET arhiv = @archive";
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
        
        public override List<String> GetListArchPrev(string pathParse)
        {
            List<String> arch = new List<string>();
            List<string> archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            string serachd = $"{Program.LocalDate:yyyyMMdd}";
            foreach (var a in archtemp.Where(a => a.ToLower().IndexOf(serachd, StringComparison.Ordinal) != -1))
            {
                string prevA = $"prev_{a}";
                using (MySqlConnection connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    string selectArch =
                        $"SELECT id FROM {Program.Prefix}arhiv_complaint WHERE arhiv = @archive";
                    MySqlCommand cmd = new MySqlCommand(selectArch, connect);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@archive", prevA);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool resRead = reader.HasRows;
                    reader.Close();
                    if (!resRead)
                    {
                        string addArch =
                            $"INSERT INTO {Program.Prefix}arhiv_complaint SET arhiv = @archive";
                        MySqlCommand cmd1 = new MySqlCommand(addArch, connect);
                        cmd1.Prepare();
                        cmd1.Parameters.AddWithValue("@archive", prevA);
                        cmd1.ExecuteNonQuery();
                        arch.Add(a);
                    }
                }
            }

            return arch;
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
    }
}