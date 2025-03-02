#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class ParserComplaint : Parser
    {
        protected DataTable DtRegion;

        private readonly string[] _fileUcomplaint =
        {
            "complaintcancel_", "complaint_", "tendersuspension_"
        };

        private readonly string[] types =
        {
            "complaint",
            "complaintCancel",
            "tenderSuspension"
        };

        private readonly string[] _fileCancel = { "complaintcancel_" };
        private readonly string[] _fileComplaint = { "complaint_" };
        private readonly string[] _fileSuspend = { "tendersuspension_" };

        public ParserComplaint(TypeArguments arg) : base(arg)
        {
        }

        public override void Parsing()
        {
            DtRegion = GetRegions();
            for (var i = Program._days; i >= 0; i--)
            {
                foreach (DataRow row in DtRegion.Rows)
                {
                    foreach (var type in types)
                    {
                        try
                        {
                            var arch = new List<string>();
                            var regionKladr = (string)row["conf"];
                            switch (Program.Periodparsing)
                            {
                                case TypeArguments.CurrComplaint:
                                    arch = GetListArchCurr(regionKladr, type, i);
                                    break;
                            }

                            if (arch.Count == 0)
                            {
                                Log.Logger($"Получен пустой список архивов регион {regionKladr} тип {type}");
                                continue;
                            }

                            foreach (var v in arch)
                            {
                                try
                                {
                                    GetListFileArch(v, (string)row["conf"], (int)row["id"], type);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Log.Logger(v, e);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Logger("Ошибка ", e);
                        }
                    }
                }
            }
        }

        public void GetListFileArch(string arch, string region, int regionId, string type)
        {
            var filea = "";
            var pathUnzip = "";
            filea = downloadArchive(arch);
            if (!string.IsNullOrEmpty(filea))
            {
                pathUnzip = Unzipped.Unzip(filea);
                if (pathUnzip != "")
                {
                    if (Directory.Exists(pathUnzip))
                    {
                        var dirInfo = new DirectoryInfo(pathUnzip);
                        var filelist = dirInfo.GetFiles();
                        var arrayXmlCancel = filelist
                            .Where(a => _fileCancel.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        var arrayXmlComplaint = filelist
                            .Where(a => _fileComplaint.Any(
                                t => a.Name.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                            .ToList();
                        var arrayXmlSuspend = filelist
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

        protected string downloadArchive(string url)
        {
            var count = 5;
            var sleep = 2000;
            var dest = $"{Program.TempPath}{Path.DirectorySeparatorChar}array.zip";
            while (true)
            {
                try
                {
                    using (var client = new TimedWebClient())
                    {
                        client.Headers.Add("individualPerson_token", Program._token);
                        client.DownloadFile(url, dest);
                    }

                    break;
                }
                catch (Exception e)
                {
                    if (count <= 0)
                    {
                        Log.Logger($"Не удалось скачать {url}");
                        break;
                    }

                    count--;
                    Thread.Sleep(sleep);
                    sleep *= 2;
                }
            }

            return dest;
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
                    case TypeFileComplaint.Complaint:
                        var a = new Complaint44(f, json);
                        a.Parsing();
                        break;
                    case TypeFileComplaint.Cancel:
                        var b = new ComplaintCancel(f, json);
                        b.Parsing();
                        break;
                    case TypeFileComplaint.Suspend:
                        var c = new ComplaintSuspend(f, json);
                        c.Parsing();
                        break;
                }
            }
        }

        public override List<string> GetListArchLast(string pathParse)
        {
            var archtemp = new List<string>();
            /*FtpClient ftp = ClientFtp44();*/
            archtemp = GetListFtp44(pathParse);
            return archtemp.Where(a => _fileUcomplaint.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
        }

        public List<string> GetListArchCurr(string regionKladr, string type, int i)
        {
            var arch = new List<string>();
            var resp = soap44(regionKladr, type, i);
            var xDoc = new XmlDocument();
            xDoc.LoadXml(resp);
            var nodeList = xDoc.SelectNodes("//dataInfo/archiveUrl");
            foreach (XmlNode node in nodeList)
            {
                var nodeValue = node.InnerText;
                arch.Add(nodeValue);
            }

            return arch;
        }

        public static string soap44(string regionKladr, string type, int i)
        {
            var count = 5;
            var sleep = 2000;
            while (true)
            {
                try
                {
                    var guid = Guid.NewGuid();
                    var currDate = DateTime.Now.ToString("s");
                    var prevday = DateTime.Now.AddDays(-1 * i).ToString("yyyy-MM-dd");
                    var request =
                        $"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ws=\"http://zakupki.gov.ru/fz44/get-docs-ip/ws\">\n<soapenv:Header>\n<individualPerson_token>{Program._token}</individualPerson_token>\n</soapenv:Header>\n<soapenv:Body>\n<ws:getDocsByOrgRegionRequest>\n<index>\n<id>{guid}</id>\n<createDateTime>{currDate}</createDateTime>\n<mode>PROD</mode>\n</index>\n<selectionParams>\n<orgRegion>{regionKladr}</orgRegion>\n<subsystemType>RJ</subsystemType>\n<documentType44>{type}</documentType44>\n<periodInfo>\n<exactDate>{prevday}</exactDate>\n</periodInfo>  </selectionParams>\n</ws:getDocsByOrgRegionRequest>\n</soapenv:Body>\n</soapenv:Envelope>";
                    var url = "https://int44.zakupki.gov.ru/eis-integration/services/getDocsIP";
                    var response = "";
                    using (WebClient wc = new TimedWebClient())
                    {
                        wc.Headers[HttpRequestHeader.ContentType] = "text/xml; charset=utf-8";
                        response = wc.UploadString(url,
                            request);
                    }

                    //Console.WriteLine(response);
                    return response;
                }
                catch (Exception e)
                {
                    if (count <= 0)
                    {
                        throw;
                    }

                    count--;
                    Thread.Sleep(sleep);
                    sleep *= 2;
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
    }
}