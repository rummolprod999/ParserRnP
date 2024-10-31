#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using FluentFTP;
using MySql.Data.MySqlClient;

#endregion

namespace ParserRnP
{
    public class Parser : IParser
    {
        protected TypeArguments Arg;

        public Parser(TypeArguments a)
        {
            Arg = a;
        }

        public virtual void Parsing()
        {
        }

        public DataTable GetRegions()
        {
            var reg = "SELECT * FROM region";
            DataTable dt;
            using (var connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                var adapter = new MySqlDataAdapter(reg, connect);
                var ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
            }

            return dt;
        }

        public virtual List<string> GetListArchLast(string pathParse)
        {
            var arch = new List<string>();

            return arch;
        }

        public virtual List<string> GetListArchCurr(string pathParse)
        {
            var arch = new List<string>();

            return arch;
        }

        public virtual List<string> GetListArchPrev(string pathParse)
        {
            var arch = new List<string>();

            return arch;
        }

        public virtual List<string> GetListArchLast(string pathParse, string regionPath, string purchase)
        {
            var arch = new List<string>();

            return arch;
        }

        public virtual List<string> GetListArchDaily(string pathParse, string regionPath, string purchase)
        {
            var arch = new List<string>();

            return arch;
        }

        public WorkWithFtp ClientFtp44_old()
        {
            var ftpCl = new WorkWithFtp("ftp://ftp.zakupki.gov.ru", "free", "VNIMANIE!_otkluchenie_FTP_s_01_01_2025_podrobnee_v_ATFF");
            return ftpCl;
        }

        public FtpClient ClientFtp44()
        {
            var client = new FtpClient("ftp.zakupki.gov.ru", "free", "VNIMANIE!_otkluchenie_FTP_s_01_01_2025_podrobnee_v_ATFF");
            client.Connect();
            return client;
        }

        public FtpClient ClientFtp223()
        {
            var client = new FtpClient("ftp.zakupki.gov.ru", "fz223free", "VNIMANIE!_otkluchenie_FTP_s_01_01_2025_podrobnee_v_ATFF");
            client.Connect();
            return client;
        }

        public WorkWithFtp ClientFtp223_old()
        {
            var ftpCl = new WorkWithFtp("ftp://ftp.zakupki.gov.ru", "fz223free", "VNIMANIE!_otkluchenie_FTP_s_01_01_2025_podrobnee_v_ATFF");
            return ftpCl;
        }

        public virtual void GetListFileArch(string arch, string pathParse)
        {
        }

        public virtual void GetListFileArch(string arch, string pathParse, string region, int regionId)
        {
        }

        public virtual void GetListFileArch(string arch, string pathParse, string region, int regionId,
            string purchase)
        {
        }

        public virtual void Bolter(FileInfo f, TypeFileRnp typefile)
        {
        }

        public virtual void Bolter(FileInfo f, TypeFileComplaint typefile)
        {
        }

        public virtual void Bolter(FileInfo f, TypeFileBank typefile)
        {
        }

        public virtual void Bolter(FileInfo f, TypeFileComplaintRes typefile)
        {
        }

        public string GetArch44(string arch, string pathParse)
        {
            var file = "";
            var count = 1;
            while (true)
            {
                try
                {
                    /*string FileOnServer = $"{PathParse}/{Arch}";*/
                    var fileOnServer = $"{arch}";
                    file = $"{Program.TempPath}{Path.DirectorySeparatorChar}{arch}";
                    var ftp = ClientFtp44();
                    ftp.SetWorkingDirectory(pathParse);
                    ftp.DownloadFile(file, fileOnServer);
                    ftp.Disconnect();
                    /*using (Ftp client = new Ftp())
                    {
                        client.Connect("ftp.zakupki.gov.ru");    // or ConnectSSL for SSL
                        client.Login("free", "free");
                        client.ChangeFolder(PathParse);
                        client.Download(FileOnServer, file);

                        client.Close();
                    }*/
                    if (count > 1)
                    {
                        Log.Logger("Удалось скачать архив после попытки", count, arch);
                    }

                    return file;
                }
                catch (Exception e)
                {
                    if (count > 50)
                    {
                        Log.Logger($"Не удалось скачать файл после попытки {count}", arch, e);
                        return file;
                    }

                    count++;
                    Thread.Sleep(5000);
                }
            }
        }
    }
}