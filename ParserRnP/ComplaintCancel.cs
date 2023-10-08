#region

using System;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

#endregion

namespace ParserRnP
{
    public class ComplaintCancel : Complaint
    {
        public event Action<int> AddComplaintCancel;

        public ComplaintCancel(FileInfo f, JObject json)
            : base(f, json)
        {
            AddComplaintCancel += delegate(int d)
            {
                if (d > 0)
                {
                    Program.AddComplaintCancel++;
                }
                else
                {
                    Log.Logger("Не удалось добавить ComplaintCancel", FilePath);
                }
            };
        }

        public override void Parsing()
        {
            var root = (JObject)T.SelectToken("export");
            var cancel = GetElements(root, "complaintCancel");
            if (cancel.Count > 0)
            {
                var complaintNumber = ((string)cancel[0].SelectToken("complaintNumber") ?? "").Trim();
                var regNumber = ((string)cancel[0].SelectToken("regNumber") ?? "").Trim();
                if ((string.IsNullOrEmpty(complaintNumber) || complaintNumber.Length < 3) &&
                    string.IsNullOrEmpty(regNumber))
                {
                    Log.Logger("Нет complaintNumber and regNumber у Cancel", FilePath, complaintNumber);
                    return;
                }

                using (var connect = ConnectToDb.GetDbConnection())
                {
                    connect.Open();
                    if (!string.IsNullOrEmpty(complaintNumber) && complaintNumber.Length >= 3)
                    {
                        var updateComp =
                            $"UPDATE {Program.Prefix}complaint SET cancel = 1 WHERE complaintNumber = @complaintNumber";
                        var cmd = new MySqlCommand(updateComp, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@complaintNumber", complaintNumber);
                        var status = cmd.ExecuteNonQuery();
                        if (status > 0)
                        {
                            AddComplaintCancel?.Invoke(status);
                        }
                    }
                    else if (!string.IsNullOrEmpty(regNumber))
                    {
                        var updateComp =
                            $"UPDATE {Program.Prefix}complaint SET cancel = 1 WHERE regNumber = @regNumber";
                        var cmd = new MySqlCommand(updateComp, connect);
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@regNumber", regNumber);
                        var status = cmd.ExecuteNonQuery();
                        if (status > 0)
                        {
                            AddComplaintCancel?.Invoke(status);
                        }
                    }
                }
            }
            else
            {
                Log.Logger("Не могу найти тег cancel", FilePath);
            }
        }
    }
}