using System;
using System.Xml;
using System.IO;

namespace ParserRnP
{
    public class GetSettings
    {
        public readonly string Database;
        public readonly string TempPathRnp;
        public readonly string LogPathRnp;
        public readonly string TempPathBank;
        public readonly string LogPathBank;
        public readonly string TempPathComplaint;
        public readonly string LogPathComplaint;
        public readonly string Prefix;
        public readonly string UserDB;
        public readonly string PassDB;
        public readonly string Server;
        public readonly int Port;
        public readonly string Years;

        public GetSettings()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Program.PathProgram + Path.DirectorySeparatorChar + "setting_rnp.xml");
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                foreach (XmlNode xnode in xRoot)
                {
                    switch (xnode.Name)
                    {
                        case "database":
                            Database = xnode.InnerText;
                            break;
                        case "tempdir_rnp":
                            TempPathRnp = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_rnp":
                            LogPathRnp = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "tempdir_bank":
                            TempPathBank = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_bank":
                            LogPathBank = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "tempdir_complaint":
                            TempPathComplaint = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_complaint":
                            LogPathComplaint = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "prefix":
                            Prefix = xnode.InnerText;
                            break;
                        case "userdb":
                            UserDB = xnode.InnerText;
                            break;
                        case "passdb":
                            PassDB = xnode.InnerText;
                            break;
                        case "server":
                            Server = xnode.InnerText;
                            break;
                        case "port":
                            Port = Int32.TryParse(xnode.InnerText, out Port) ? Int32.Parse(xnode.InnerText) : 3306;
                            break;
                        case "years":
                            Years = xnode.InnerText;
                            break;
                    }
                }
            }

            if (String.IsNullOrEmpty(LogPathComplaint) || String.IsNullOrEmpty(TempPathComplaint) || String.IsNullOrEmpty(LogPathBank) || String.IsNullOrEmpty(TempPathBank) ||
                String.IsNullOrEmpty(LogPathRnp) || String.IsNullOrEmpty(TempPathRnp) ||
                String.IsNullOrEmpty(Database) || String.IsNullOrEmpty(UserDB) || String.IsNullOrEmpty(Server) ||
                String.IsNullOrEmpty(Years))
            {
                Console.WriteLine("Некоторые поля в файле настроек пустые");
                Environment.Exit(0);
            }
        }
    }
}