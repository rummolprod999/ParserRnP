#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class ParserNsi : Parser
    {
        private readonly string[] _file =
        {
            "nsi"
        };

        public ParserNsi(TypeArguments a) : base(a)
        {
        }

        public override void Parsing()
        {
            var arch = new List<string>();
            var bankList = new List<string>();
            var pathParse = "/fcs_nsi/nsiOKEI/";
            ;

            bankList = GetListBank(pathParse);
            foreach (var l in bankList)
            {
                pathParse = "/fcs_nsi/nsiOKEI/";
                GetListFileArch(l, pathParse);
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

                        foreach (var f in filelist)
                        {
                            Bolter(f);
                        }

                        dirInfo.Delete(true);
                    }
                }
            }
        }

        public List<string> GetListBank(string pathParse)
        {
            var listtemp = new List<string>();
            listtemp = GetListFtp44(pathParse);
            return listtemp.Where(a => _file.Any(t => a.ToLower().IndexOf(t, StringComparison.Ordinal) != -1))
                .ToList();
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
                var b = new Nsi(f, json);
                b.Parsing();
            }
        }
    }
}