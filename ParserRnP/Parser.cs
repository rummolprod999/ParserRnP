using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using FluentFTP;
using Limilabs.FTP.Client;
using MySql.Data.MySqlClient;

namespace ParserRnP
{
    public class Parser : IParser
    {
        protected TypeArguments arg;

        public Parser(TypeArguments a)
        {
            this.arg = a;
        }

        public virtual void Parsing()
        {
        }
        
        public DataTable GetRegions()
        {
            string reg = "SELECT * FROM region";
            DataTable dt;
            using (MySqlConnection connect = ConnectToDb.GetDBConnection())
            {
                connect.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(reg, connect);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
            }
            return dt;
        }
        
        public virtual List<String> GetListArchLast(string PathParse)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchCurr(string PathParse)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchPrev(string PathParse)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchLast(string PathParse, string RegionPath, string purchase)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchDaily(string PathParse, string RegionPath, string purchase)
        {
            List<String> arch = new List<string>();

            return arch;
        }
        public WorkWithFtp ClientFtp44_old()
        {
            WorkWithFtp ftpCl = new WorkWithFtp("ftp://ftp.zakupki.gov.ru", "free", "free");
            return ftpCl;
        }

        public FtpClient ClientFtp44()
        {
            FtpClient client = new FtpClient("ftp.zakupki.gov.ru", "free", "free");
            client.Connect();
            return client;
        }

        public FtpClient ClientFtp223()
        {
            FtpClient client = new FtpClient("ftp.zakupki.gov.ru", "fz223free", "fz223free");
            client.Connect();
            return client;
        }
        
        public WorkWithFtp ClientFtp223_old()
        {
            WorkWithFtp ftpCl = new WorkWithFtp("ftp://ftp.zakupki.gov.ru", "fz223free", "fz223free");
            return ftpCl;
        }

        public virtual void GetListFileArch(string Arch, string PathParse)
        {
        }

        public virtual void GetListFileArch(string Arch, string PathParse, string region, int region_id)
        {
        }

        public virtual void GetListFileArch(string Arch, string PathParse, string region, int region_id,
            string purchase)
        {
        }

        public virtual void Bolter(FileInfo f, TypeFileRnp typefile)
        {
        }
        
        public virtual void Bolter(FileInfo f, TypeFileBank typefile)
        {
        }
        
        public string GetArch44(string Arch, string PathParse)
        {
            string file = "";
            int count = 1;
            while (true)
            {
                try
                {
                    /*string FileOnServer = $"{PathParse}/{Arch}";*/
                    string FileOnServer = $"{Arch}";
                    file = $"{Program.TempPath}{Path.DirectorySeparatorChar}{Arch}";
                    FtpClient ftp = ClientFtp44();
                    ftp.SetWorkingDirectory(PathParse);
                    ftp.DownloadFile(file, FileOnServer);
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
                        Log.Logger("Удалось скачать архив после попытки", count, PathParse);
                    }
                    return file;
                }
                catch (Exception e)
                {
                    
                    if (count > 50)
                    {
                        Log.Logger($"Не удалось скачать файл после попытки {count}", Arch, e);
                        return file;
                    }

                    count++;
                    Thread.Sleep(5000);
                }
            }
        }
    }
}