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
    public class ComplaintCancelRes : Complaint
    {
        public event Action<int> AddComplaintCancelRes;

        public ComplaintCancelRes(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaintCancelRes += delegate(int d)
            {
                if (d > 0)
                    Program.AddComplaintCancelResult++;
                else
                    Log.Logger("Не удалось добавить ComplaintCancelRes", FilePath);
            };
        }

        public override void Parsing()
        {
        }
    }
}