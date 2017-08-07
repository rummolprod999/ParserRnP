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
        private static string _tempPathBank;
        private static string _logPathBank;
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
                

                return "";
            }
        }
        public static int AddRnp = 0;
        public static int AddBankGuarantee = 0;
        
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Недостаточно аргументов для запуска, используйте lastUn, prevUn, currUn, rootUn, lastBank, prevBank, currBank, rootBank в качестве аргумента");
                return;
            }
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
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
                default:
                    Console.WriteLine(
                        "Неправильно указан аргумент, используйте lastUn, prevUn, currUn, rootUn, lastBank, prevBank, currBank, rootBank");
                    break;
            }
        }

        private static void Init(TypeArguments arg)
        {
            GetSettings set = new GetSettings();
            _database = set.Database;
            _logPathRnp = set.LogPathRnp;
            _logPathBank = set.LogPathBank;
            _prefix = set.Prefix;
            _user = set.UserDB;
            _pass = set.PassDB;
            _tempPathRnp = set.TempPathRnp;
            _tempPathBank = set.TempPathBank;
            _server = set.Server;
            _port = set.Port;
            string tmp = set.Years;
            string[] temp_years = tmp.Split(new char[] {','});
            foreach (var s in temp_years.Select(v => $"_{v.Trim()}"))
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
                DirectoryInfo dirInfo = new DirectoryInfo(TempPath);
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
            if (arg == TypeArguments.CurrUn || arg == TypeArguments.LastUn || arg == TypeArguments.PrevUn || arg == TypeArguments.RootUn)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}Rnp_{LocalDate:dd_MM_yyyy}.log";
            else if (arg == TypeArguments.CurrBank || arg == TypeArguments.LastBank || arg == TypeArguments.PrevBank || arg == TypeArguments.RootBank)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}BankGuarantee_{LocalDate:dd_MM_yyyy}.log";
        }

        private static void ParserRnp(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Rnp");
            ParserUnFair rnp = new ParserUnFair(Periodparsing);
            rnp.Parsing();
            /*ParserUnFair rnp = new ParserUnFair(Periodparsing);
            FileInfo f = new FileInfo("/home/alex/Рабочий стол/parser/unfairSupplier_40556-15_40556.xml");
            rnp.ParsingXML(f, TypeFileRnp.unfairSupplier);*/
            Log.Logger("Время окончания парсинга Rnp");
            Log.Logger("Добавили Unfair44", AddRnp);
        }

        private static void ParserBank(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Bank");
            ParserBank b = new ParserBank(Periodparsing);
            b.Parsing();
            /*ParserUnFair rnp = new ParserUnFair(Periodparsing);
            FileInfo f = new FileInfo("/home/alex/Рабочий стол/parser/unfairSupplier_40556-15_40556.xml");
            rnp.ParsingXML(f, TypeFileRnp.unfairSupplier);*/
            Log.Logger("Время окончания парсинга Bank");
            Log.Logger("Добавили Bank44", AddBankGuarantee);
        }
    }
}