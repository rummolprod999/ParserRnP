using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ParserRnP
{
    public class Bank44 : Bank
    {
        public event Action<int> AddBank44;
        public Bank44(FileInfo f, JObject json)
            : base(f, json)
        {
            AddBank44 += delegate(int d)
            {
                if (d > 0)
                    Program.AddBankGuarantee++;
                else
                    Log.Logger("Не удалось добавить Bank44", file_path);
            };
        }

        public override void Parsing()
        {
            string xml = GetXml(file.ToString());
            
        }
    }
}