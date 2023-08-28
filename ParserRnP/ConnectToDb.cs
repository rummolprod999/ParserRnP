using System;
using MySql.Data.MySqlClient;

namespace ParserRnP
{
    public class ConnectToDb
    {
        public static MySqlConnection GetDbConnection()
        {
            // Connection String.
            var connString =
                $"Server={Program.Server};port={Program.Port};Database={Program.Database};User Id={Program.User};password={Program.Pass};CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600";

            var conn = new MySqlConnection(connString);

            return conn;
        }
    }
}