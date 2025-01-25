#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class ParserFarmDrug : Parser
    {
        public ParserFarmDrug(TypeArguments a) : base(a)
        {
        }

        public override void Parsing()
        {
            var bankList = new List<string>();

            bankList = GetListBank();
            foreach (var l in bankList)
            {
                try
                {
                    GetListFileArch(l);
                }
                catch (Exception e)
                {
                    Log.Logger("Ошибка при парсинге", e);
                }
            }
        }

        public void GetListFileArch(string url)
        {
            var filea = "";
            var pathUnzip = "";
            filea = downloadArchive(url);
            if (!string.IsNullOrEmpty(filea))
            {
                pathUnzip = Unzipped.Unzip(filea);
                if (pathUnzip != "")
                {
                    if (Directory.Exists(pathUnzip))
                    {
                        var dirInfo = new DirectoryInfo(pathUnzip);
                        var filelist = dirInfo.GetFiles();

                        foreach (var f in filelist)
                        {
                            Bolter(f);
                        }

                        dirInfo.Delete(true);
                    }
                }
            }
        }

        public List<string> GetListBank()
        {
            var arch = new List<string>();
            var resp = soap44();
            var xDoc = new XmlDocument();
            try
            {
                xDoc.LoadXml(resp);
            }
            catch (Exception e)
            {
                Log.Logger(e, resp);
                throw;
            }

            var nodeList = xDoc.SelectNodes("//dataInfo/nsiArchiveInfo/archiveUrl");
            foreach (XmlNode node in nodeList)
            {
                var nodeValue = node.InnerText;
                arch.Add(nodeValue);
            }

            return arch;
        }

        public static string soap44()
        {
            var count = 5;
            var sleep = 2000;
            while (true)
            {
                try
                {
                    var guid = Guid.NewGuid();
                    var currDate = DateTime.Now.ToString("s");
                    var request =
                        $"<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ws=\"http://zakupki.gov.ru/fz44/get-docs-ip/ws\">\n<soapenv:Header>\n<individualPerson_token>{Program._token}</individualPerson_token>\n</soapenv:Header>\n<soapenv:Body>\n<ws:getNsiRequest>\n<index>\n<id>{guid}</id>\n<createDateTime>{currDate}</createDateTime>\n<mode>PROD</mode>\n</index>\n<selectionParams>\n<nsiCode44>nsiFarmDrugDictionary</nsiCode44>\n<nsiKind>{Program._kind}</nsiKind>\n</selectionParams>\n</ws:getNsiRequest>\n</soapenv:Body>\n</soapenv:Envelope>";
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


        public void Bolter(FileInfo f)
        {
            if (!f.Name.ToLower().EndsWith(".xml", StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                ParsingXml(f);
            }
            catch (Exception e)
            {
                Log.Logger("Ошибка при парсинге xml", e, f);
            }
        }

        public void ParsingXml(FileInfo f)
        {
            using (var sr = new StreamReader(f.ToString(), Encoding.Default))
            {
                var ftext = sr.ReadToEnd();
                ftext = ClearText.ClearString(ftext);
                var doc = new XmlDocument();
                doc.LoadXml(ftext);
                var jsons = JsonConvert.SerializeXmlNode(doc);
                var json = JObject.Parse(jsons);
                var b = new FarmDrug(f, json);
                b.Parsing();
            }
        }
    }
}