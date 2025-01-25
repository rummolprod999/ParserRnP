#region

using System;
using System.IO;
using System.Xml;

#endregion

namespace ParserRnP
{
    public class GetSettings
    {
        public readonly string Database;
        public readonly string TempPathRnp;
        public readonly string LogPathRnp;
        public readonly string TempPathNsi;
        public readonly string LogPathNsi;
        public readonly string TempPathKtru;
        public readonly string LogPathKtru;
        public readonly string TempPathfarmDrug;
        public readonly string LogPathFarmDrug;
        public readonly string TempPathBank;
        public readonly string LogPathBank;
        public readonly string TempPathComplaint;
        public readonly string LogPathComplaint;
        public readonly string TempPathComplaintResult;
        public readonly string LogPathComplaintResult;
        public readonly string Prefix;
        public readonly string UserDb;
        public readonly string PassDb;
        public readonly string Server;
        public readonly int Port;
        public readonly string Years;
        public readonly int Days;
        public readonly string Token;
        public readonly string Kind;

        public GetSettings()
        {
            var xDoc = new XmlDocument();
            xDoc.Load(Program.PathProgram + Path.DirectorySeparatorChar + "setting_rnp.xml");
            var xRoot = xDoc.DocumentElement;
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
                        case "tempdir_nsi":
                            TempPathNsi = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "tempdir_ktru":
                            TempPathKtru = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_farmdrug":
                            LogPathFarmDrug = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "tempdir_farmdrug":
                            TempPathfarmDrug = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_nsi":
                            LogPathNsi = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_ktru":
                            LogPathKtru = $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
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
                        case "tempdir_complaint_result":
                            TempPathComplaintResult =
                                $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "logdir_complaint_result":
                            LogPathComplaintResult =
                                $"{Program.PathProgram}{Path.DirectorySeparatorChar}{xnode.InnerText}";
                            break;
                        case "prefix":
                            Prefix = xnode.InnerText;
                            break;
                        case "userdb":
                            UserDb = xnode.InnerText;
                            break;
                        case "passdb":
                            PassDb = xnode.InnerText;
                            break;
                        case "server":
                            Server = xnode.InnerText;
                            break;
                        case "port":
                            Port = int.TryParse(xnode.InnerText, out Port) ? int.Parse(xnode.InnerText) : 3306;
                            break;
                        case "years":
                            Years = xnode.InnerText;
                            break;
                        case "token":
                            Token = xnode.InnerText;
                            break;
                        case "kind":
                            Kind = xnode.InnerText;
                            break;
                        case "days":
                            Days = int.TryParse(xnode.InnerText, out Days) ? int.Parse(xnode.InnerText) : 3;
                            break;
                    }
                }
            }

            if (string.IsNullOrEmpty(LogPathComplaint) || string.IsNullOrEmpty(TempPathComplaint) ||
                string.IsNullOrEmpty(LogPathBank) || string.IsNullOrEmpty(TempPathBank) ||
                string.IsNullOrEmpty(LogPathRnp) || string.IsNullOrEmpty(TempPathRnp) ||
                string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(UserDb) || string.IsNullOrEmpty(Server) ||
                string.IsNullOrEmpty(Years) || string.IsNullOrEmpty(TempPathComplaintResult) ||
                string.IsNullOrEmpty(LogPathComplaintResult) ||
                string.IsNullOrEmpty(LogPathNsi) || string.IsNullOrEmpty(TempPathNsi) ||
                string.IsNullOrEmpty(LogPathFarmDrug) || string.IsNullOrEmpty(TempPathfarmDrug) ||
                string.IsNullOrEmpty(LogPathKtru) || string.IsNullOrEmpty(TempPathKtru))
            {
                Console.WriteLine("Некоторые поля в файле настроек пустые");
                Environment.Exit(0);
            }
        }
    }
}