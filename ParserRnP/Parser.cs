﻿using System;
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
        protected TypeArguments Arg;

        public Parser(TypeArguments a)
        {
            this.Arg = a;
        }

        public virtual void Parsing()
        {
        }

        public DataTable GetRegions()
        {
            string reg = "SELECT * FROM region";
            DataTable dt;
            using (MySqlConnection connect = ConnectToDb.GetDbConnection())
            {
                connect.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(reg, connect);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
            }
            return dt;
        }

        public virtual List<String> GetListArchLast(string pathParse)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchCurr(string pathParse)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchPrev(string pathParse)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchLast(string pathParse, string regionPath, string purchase)
        {
            List<String> arch = new List<string>();

            return arch;
        }

        public virtual List<String> GetListArchDaily(string pathParse, string regionPath, string purchase)
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
            string file = "";
            int count = 1;
            while (true)
            {
                try
                {
                    /*string FileOnServer = $"{PathParse}/{Arch}";*/
                    string fileOnServer = $"{arch}";
                    file = $"{Program.TempPath}{Path.DirectorySeparatorChar}{arch}";
                    FtpClient ftp = ClientFtp44();
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
                        Log.Logger("Удалось скачать архив после попытки", count, pathParse);
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