using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ParserRnP
{
    internal class Program
    {
        private static string _database;
        private static string _tempPathRnp;
        private static string _logPathRnp;
        private static string _tempPathNsi;
        private static string _logPathNsi;
        private static string _tempPathBank;
        private static string _logPathBank;
        private static string _tempPathComplaint;
        private static string _logPathComplaint;
        private static string _tempPathComplaintResult;
        private static string _logPathComplaintResult;
        private static string _prefix;
        private static string _user;
        private static string _pass;
        private static string _server;
        private static int _port;
        private static List<string> _years = new List<string>();
        public static string Database => _database;
        public static string Prefix => _prefix;
        public static string User => _user;
        public static string Pass => _pass;
        public static string Server => _server;
        public static int Port => _port;
        public static List<string> Years => _years;
        public static readonly DateTime LocalDate = DateTime.Now;
        public static string FileLog;
        public static string StrArg;
        public static TypeArguments Periodparsing;
        public static string PathProgram;

        public static string TempPath
        {
            get
            {
                if (Periodparsing == TypeArguments.CurrUn || Periodparsing == TypeArguments.PrevUn ||
                    Periodparsing == TypeArguments.LastUn || Periodparsing == TypeArguments.RootUn)
                    return _tempPathRnp;
                else if (Periodparsing == TypeArguments.CurrBank || Periodparsing == TypeArguments.PrevBank ||
                         Periodparsing == TypeArguments.LastBank || Periodparsing == TypeArguments.RootBank)
                    return _tempPathBank;

                else if (Periodparsing == TypeArguments.CurrComplaint || Periodparsing == TypeArguments.PrevComplaint ||
                         Periodparsing == TypeArguments.LastComplaint)
                    return _tempPathComplaint;
                else if (Periodparsing == TypeArguments.CurrComplaintRes ||
                         Periodparsing == TypeArguments.LastComplaintRes ||
                         Periodparsing == TypeArguments.PrevComplaintRes)
                    return _tempPathComplaintResult;
                else if (Periodparsing == TypeArguments.Nsi)
                    return _tempPathNsi;


                return "";
            }
        }

        public static string LogPath
        {
            get
            {
                if (Periodparsing == TypeArguments.CurrUn || Periodparsing == TypeArguments.PrevUn ||
                    Periodparsing == TypeArguments.LastUn || Periodparsing == TypeArguments.RootUn)
                    return _logPathRnp;
                else if (Periodparsing == TypeArguments.CurrBank || Periodparsing == TypeArguments.PrevBank ||
                         Periodparsing == TypeArguments.LastBank || Periodparsing == TypeArguments.RootBank)
                    return _logPathBank;
                else if (Periodparsing == TypeArguments.CurrComplaint || Periodparsing == TypeArguments.PrevComplaint ||
                         Periodparsing == TypeArguments.LastComplaint)
                    return _logPathComplaint;
                else if (Periodparsing == TypeArguments.CurrComplaintRes ||
                         Periodparsing == TypeArguments.LastComplaintRes ||
                         Periodparsing == TypeArguments.PrevComplaintRes)
                    return _logPathComplaintResult;
                else if (Periodparsing == TypeArguments.Nsi )
                    return _logPathNsi;


                return "";
            }
        }

        public static int AddRnp = 0;
        public static int AddNsi = 0;
        public static int UpdateNsi = 0;
        public static int AddBankGuarantee = 0;
        public static int UpdateBankGuarantee = 0;
        public static int AddComplaint = 0;
        public static int UpdateComplaint = 0;
        public static int AddComplaintCancel = 0;
        public static int AddComplaintSuspend = 0;
        public static int AddComplaintResult = 0;
        public static int UpdateComplaintResult = 0;
        public static int AddComplaintCancelResult = 0;
        public static int UpdateComplaintCancelResult = 0;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Недостаточно аргументов для запуска, используйте lastUn, prevUn, currUn, rootUn, lastBank, prevBank, currBank, rootBank, lastComplaint, prevComplaint, currComplaint, currComplaintRes, lastComplaintRes, prevComplaintRes в качестве аргумента");
                return;
            }
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
                .CodeBase);
            if (path != null) PathProgram = path.Substring(5);
            StrArg = args[0];
            switch (args[0])
            {
                case "lastUn":
                    Periodparsing = TypeArguments.LastUn;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                case "prevUn":
                    Periodparsing = TypeArguments.PrevUn;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                case "currUn":
                    Periodparsing = TypeArguments.CurrUn;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                case "rootUn":
                    Periodparsing = TypeArguments.RootUn;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                case "rootBank":
                    Periodparsing = TypeArguments.RootBank;
                    Init(Periodparsing);
                    ParserBank(Periodparsing);
                    break;
                case "lastBank":
                    Periodparsing = TypeArguments.LastBank;
                    Init(Periodparsing);
                    ParserBank(Periodparsing);
                    break;
                case "prevBank":
                    Periodparsing = TypeArguments.PrevBank;
                    Init(Periodparsing);
                    ParserBank(Periodparsing);
                    break;
                case "currBank":
                    Periodparsing = TypeArguments.CurrBank;
                    Init(Periodparsing);
                    ParserBank(Periodparsing);
                    break;
                case "lastComplaint":
                    Periodparsing = TypeArguments.LastComplaint;
                    Init(Periodparsing);
                    ParserComplint(Periodparsing);
                    break;
                case "prevComplaint":
                    Periodparsing = TypeArguments.PrevComplaint;
                    Init(Periodparsing);
                    ParserComplint(Periodparsing);
                    break;
                case "currComplaint":
                    Periodparsing = TypeArguments.CurrComplaint;
                    Init(Periodparsing);
                    ParserComplint(Periodparsing);
                    break;
                case "currComplaintRes":
                    Periodparsing = TypeArguments.CurrComplaintRes;
                    Init(Periodparsing);
                    ParserComplintResult(Periodparsing);
                    break;
                case "lastComplaintRes":
                    Periodparsing = TypeArguments.LastComplaintRes;
                    Init(Periodparsing);
                    ParserComplintResult(Periodparsing);
                    break;
                case "prevComplaintRes":
                    Periodparsing = TypeArguments.PrevComplaintRes;
                    Init(Periodparsing);
                    ParserComplintResult(Periodparsing);
                    break;   
                case "nsi":
                    Periodparsing = TypeArguments.Nsi;
                    Init(Periodparsing);
                    ParserNsi(Periodparsing);
                    break;   
                default:
                    Console.WriteLine(
                        "Неправильно указан аргумент, используйте lastUn, prevUn, currUn, rootUn, lastBank, prevBank, currBank, rootBank, lastComplaint, prevComplaint, currComplaint, currComplaintRes, lastComplaintRes, prevComplaintRes, nsi");
                    break;
            }
        }

        private static void Init(TypeArguments arg)
        {
            var set = new GetSettings();
            _database = set.Database;
            _logPathRnp = set.LogPathRnp;
            _logPathNsi = set.LogPathNsi;
            _logPathBank = set.LogPathBank;
            _logPathComplaint = set.LogPathComplaint;
            _logPathComplaintResult = set.LogPathComplaintResult;
            _prefix = set.Prefix;
            _user = set.UserDb;
            _pass = set.PassDb;
            _tempPathRnp = set.TempPathRnp;
            _tempPathNsi = set.TempPathNsi;
            _tempPathBank = set.TempPathBank;
            _tempPathComplaint = set.TempPathComplaint;
            _tempPathComplaintResult = set.TempPathComplaintResult;
            _server = set.Server;
            _port = set.Port;
            var tmp = set.Years;
            var tempYears = tmp.Split(new char[] {','});
            foreach (var s in tempYears.Select(v => $"_{v.Trim()}"))
            {
                _years.Add(s);
            }
            if (String.IsNullOrEmpty(TempPath) || String.IsNullOrEmpty(LogPath))
            {
                Console.WriteLine("Не получится создать папки для парсинга");
                Environment.Exit(0);
            }
            if (Directory.Exists(TempPath))
            {
                var dirInfo = new DirectoryInfo(TempPath);
                dirInfo.Delete(true);
                Directory.CreateDirectory(TempPath);
            }
            else
            {
                Directory.CreateDirectory(TempPath);
            }
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            if (arg == TypeArguments.CurrUn || arg == TypeArguments.LastUn || arg == TypeArguments.PrevUn ||
                arg == TypeArguments.RootUn)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}Rnp_{LocalDate:dd_MM_yyyy}.log";
            else if (arg == TypeArguments.CurrBank || arg == TypeArguments.LastBank || arg == TypeArguments.PrevBank ||
                     arg == TypeArguments.RootBank)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}BankGuarantee_{LocalDate:dd_MM_yyyy}.log";

            else if (arg == TypeArguments.CurrComplaint || arg == TypeArguments.LastComplaint ||
                     arg == TypeArguments.PrevComplaint)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}Complaint_{LocalDate:dd_MM_yyyy}.log";
            else if (Periodparsing == TypeArguments.CurrComplaintRes ||
                     Periodparsing == TypeArguments.LastComplaintRes ||
                     Periodparsing == TypeArguments.PrevComplaintRes)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}ComplaintResult_{LocalDate:dd_MM_yyyy}.log";
            else if (Periodparsing == TypeArguments.Nsi)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}Nsi_{LocalDate:dd_MM_yyyy}.log";
        }

        private static void ParserRnp(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Rnp");
            var rnp = new ParserUnFair(Periodparsing);
            rnp.Parsing();
            /*ParserUnFair rnp = new ParserUnFair(Periodparsing);
            FileInfo f = new FileInfo("/home/alex/Рабочий стол/parser/unfairSupplier_40556-15_40556.xml");
            rnp.ParsingXML(f, TypeFileRnp.unfairSupplier);*/
            Log.Logger("Время окончания парсинга Rnp");
            Log.Logger("Добавили Unfair44", AddRnp);
        }
        
        private static void ParserNsi(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Nsi");
            var rnp = new ParserNsi(Periodparsing);
            rnp.Parsing();
            Log.Logger("Время окончания парсинга Nsi");
            Log.Logger("Добавили Nsi", AddNsi);
            Log.Logger("Обновили Nsi", UpdateNsi);
        }

        private static void ParserBank(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Bank");
            var b = new ParserBank(Periodparsing);
            b.Parsing();
            /*ParserBank b = new ParserBank(Periodparsing);
            FileInfo f = new FileInfo("/home/alex/Рабочий стол/parser/fcsGaranteeInfo_02T2720323440915000201_408424.xml");
            b.ParsingXML(f, TypeFileBank.Bank);*/
            Log.Logger("Время окончания парсинга Bank");
            Log.Logger("Добавили Bank44", AddBankGuarantee);
            Log.Logger("Обновили Bank44", UpdateBankGuarantee);
        }

        private static void ParserComplint(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Complaint");
            var c = new ParserComplaint(Periodparsing);
            c.Parsing();
            /*ParserComplaint b = new ParserComplaint(Periodparsing);
            FileInfo f = new FileInfo("/home/alex/Рабочий стол/parser/complaint_20170019274004049_1743623.xml");
            b.ParsingXml(f, TypeFileComplaint.Complaint);*/

            Log.Logger("Время окончания парсинга Complaint");
            Log.Logger("Добавили Complaint", AddComplaint);
            Log.Logger("Обновили Complaint", UpdateComplaint);
            Log.Logger("Добавили ComplaintCancel", AddComplaintCancel);
            Log.Logger("Добавили ComplaintSuspend", AddComplaintSuspend);
        }

        private static void ParserComplintResult(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга ComplaintResult");
            var c = new ParserComplaintResult(Periodparsing);
            c.Parsing();
            /*ParserComplaintResult b = new ParserComplaintResult(Periodparsing);
            FileInfo f = new FileInfo("/home/alex/Рабочий стол/parser/checkResult_201600116204000002_243146.xml");
            b.ParsingXml(f, TypeFileComplaintRes.ComplaintRes);*/
            Log.Logger("Время окончания парсинга ComplaintResult");
            Log.Logger("Добавили ComplaintResult", AddComplaintResult);
            Log.Logger("Обновили ComplaintResult", UpdateComplaintResult);
            Log.Logger("Добавили ComplaintResultCancel", AddComplaintCancelResult);
            Log.Logger("Обновили ComplaintResulCancel", UpdateComplaintCancelResult);
        }
    }
}