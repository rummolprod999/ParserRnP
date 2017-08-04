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
                if (Periodparsing == TypeArguments.Curr || Periodparsing == TypeArguments.Prev ||
                    Periodparsing == TypeArguments.Last)
                    return _tempPathRnp;
                

                return "";
            }
        }
        public static string LogPath
        {
            get
            {
                if (Periodparsing == TypeArguments.Curr || Periodparsing == TypeArguments.Prev ||
                    Periodparsing == TypeArguments.Last)
                    return _logPathRnp;
                

                return "";
            }
        }
        public static int AddRnp = 0;
        
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Недостаточно аргументов для запуска, используйте last, prev, curr в качестве аргумента");
                return;
            }
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
                .CodeBase);
            if (path != null) PathProgram = path.Substring(5);
            StrArg = args[0];
            switch (args[0])
            {
                case "last":
                    Periodparsing = TypeArguments.Last;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                case "prev":
                    Periodparsing = TypeArguments.Prev;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                case "curr":
                    Periodparsing = TypeArguments.Curr;
                    Init(Periodparsing);
                    ParserRnp(Periodparsing);
                    break;
                default:
                    Console.WriteLine(
                        "Неправильно указан аргумент, используйте last, prev, curr");
                    break;
            }
        }

        private static void Init(TypeArguments arg)
        {
            GetSettings set = new GetSettings();
            _database = set.Database;
            _logPathRnp = set.LogPathRnp;
            _prefix = set.Prefix;
            _user = set.UserDB;
            _pass = set.PassDB;
            _tempPathRnp = set.TempPathRnp;
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
            if (arg == TypeArguments.Curr || arg == TypeArguments.Last || arg == TypeArguments.Prev)
                FileLog = $"{LogPath}{Path.DirectorySeparatorChar}Rnp_{LocalDate:dd_MM_yyyy}.log";
        }

        private static void ParserRnp(TypeArguments arg)
        {
            Log.Logger("Время начала парсинга Rnp");
            ParserUnFair rnp = new ParserUnFair(Periodparsing);
            rnp.Parsing();
            Log.Logger("Время окончания парсинга Rnp");
        }
    }
}