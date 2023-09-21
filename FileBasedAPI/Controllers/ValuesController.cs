using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace FileBasedAPI.Controllers
{
    public class ValuesController : ApiController
    {
        private const string Purpose = "Authentication";
        private string constring = ConfigurationManager.ConnectionStrings["ConnectTitan"].ToString();
        //private string constring1 = ConfigurationManager.ConnectionStrings["ConnectTitan1"].ToString();
        // private string constring1 = ConfigurationManager.ConnectionStrings["ConnectTitan1"].ToString();
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        [Route("api/Values/Store_Rawtable_Data")]
        [HttpPost]
        // public string Store_Rawtable_Data(string devicename,string t2maccount,string t2musername,string t2mpassword,string t2mdeveloperid,string t2mdeviceusername,string t2mdevicepassword)
        public string Store_Rawtable_Data(string devicename, string filename, string path)
        {
            string dt_check = "";
            try
            {
                String result1 = "";
                DataTable details = new DataTable();
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                var i = 0;
                //for proper insertion
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                dt.Columns.Add("Shift_Id");
                dt.Columns.Add("Line_Code");
                dt.Columns.Add("Machine_Code");
                dt.Columns.Add("Variant_Code");
                dt.Columns.Add("Machine_Status");
                dt.Columns.Add(new DataColumn("OK_Parts", typeof(int)));
                dt.Columns.Add(new DataColumn("NOK_Parts", typeof(int)));
                dt.Columns.Add(new DataColumn("Rework_Parts", typeof(int)));
                dt.Columns.Add("Rejection_Reasons");
                dt.Columns.Add(new DataColumn("Auto__Mode_Selected", typeof(int)));
                dt.Columns.Add(new DataColumn("Manual_Mode_Slected", typeof(int)));
                dt.Columns.Add(new DataColumn("Auto_Mode_Running", typeof(int)));
                dt.Columns.Add("CompanyCode");
                dt.Columns.Add("PlantCode");
                dt.Columns.Add("OperatorID");
                dt.Columns.Add("Live_Alarm");
                dt.Columns.Add("Live_Loss");
                dt.Columns.Add("Batch_code");


                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Time_Stamp");
                dt_temp.Columns.Add("Date");
                dt_temp.Columns.Add("Shift_Id");
                dt_temp.Columns.Add("Line_Code");
                dt_temp.Columns.Add("Machine_Code");
                dt_temp.Columns.Add("Variant_Code");
                dt_temp.Columns.Add("Machine_Status");
                dt_temp.Columns.Add(new DataColumn("OK_Parts", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("NOK_Parts", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Rework_Parts", typeof(int)));
                dt_temp.Columns.Add("Rejection_Reasons");
                dt_temp.Columns.Add(new DataColumn("Auto__Mode_Selected", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Manual_Mode_Slected", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Auto_Mode_Running", typeof(int)));
                dt_temp.Columns.Add("CompanyCode");
                dt_temp.Columns.Add("PlantCode");
                dt_temp.Columns.Add("OperatorID");
                dt_temp.Columns.Add("Live_Alarm");
                dt_temp.Columns.Add("Live_Loss");
                dt_temp.Columns.Add("Batch_code");
                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                    for (i = 0; i < (allTextLines.Length); i++)
                    {

                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                        if ((DateTime.TryParse(entries[0].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue)) && (DateTime.TryParse(entries[1].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue)))
                        {
                            dc["Time_Stamp"] = Convert.ToDateTime(entries[0].ToString());
                            dc["Date"] = Convert.ToDateTime(entries[1].ToString());
                            dc["Shift_Id"] = entries[2];
                            dc["Line_Code"] = entries[3];
                            dc["Machine_Code"] = entries[4];
                            dc["Variant_Code"] = entries[5];
                            dc["Machine_Status"] = entries[6]; ;
                            dc["OK_Parts"] = Convert.ToInt32(entries[7]);
                            dc["NOK_Parts"] = Convert.ToInt32(entries[8]);
                            dc["Rework_Parts"] = Convert.ToInt32(entries[9]);
                            dc["Rejection_Reasons"] = entries[10];
                            dc["Auto__Mode_Selected"] = Convert.ToInt32(entries[11]);
                            dc["Manual_Mode_Slected"] = Convert.ToInt32(entries[12]);
                            dc["Auto_Mode_Running"] = Convert.ToInt32(entries[13]);
                            dc["CompanyCode"] = entries[14];
                            dc["PlantCode"] = entries[15];
                            dc["OperatorID"] = entries[16];
                            dc["Live_Alarm"] = entries[17];
                            dc["Live_Loss"] = entries[18];
                            dc["Batch_code"] = entries[19];
                            dt.Rows.Add(dc);
                        }

                        else
                        {
                            dc_temp["Time_Stamp"] = entries[0].ToString();
                            dc_temp["Date"] = entries[1].ToString();
                            dc_temp["Shift_Id"] = entries[2];
                            dc_temp["Line_Code"] = entries[3];
                            dc_temp["Machine_Code"] = entries[4];
                            dc_temp["Variant_Code"] = entries[5];
                            dc_temp["Machine_Status"] = entries[6]; ;
                            dc_temp["OK_Parts"] = Convert.ToInt32(entries[7]);
                            dc_temp["NOK_Parts"] = Convert.ToInt32(entries[8]);
                            dc_temp["Rework_Parts"] = Convert.ToInt32(entries[9]);
                            dc_temp["Rejection_Reasons"] = entries[10];
                            dc_temp["Auto__Mode_Selected"] = Convert.ToInt32(entries[11]);
                            dc_temp["Manual_Mode_Slected"] = Convert.ToInt32(entries[12]);
                            dc_temp["Auto_Mode_Running"] = Convert.ToInt32(entries[13]);
                            dc_temp["CompanyCode"] = entries[14];
                            dc_temp["PlantCode"] = entries[15];
                            dc_temp["OperatorID"] = entries[16];
                            dc_temp["Live_Alarm"] = entries[17];
                            dc_temp["Live_Loss"] = entries[18];
                            dc_temp["Batch_code"] = entries[19];
                            dt_temp.Rows.Add(dc_temp);
                        }
                        //dt_check = entries[0].ToString();



                        //i++;
                    }
                    var entries_connection = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[14].ToString(), entries_connection[15].ToString(), entries_connection[3].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "RAWTable";
                                sqlBulk.BatchSize = dt.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();


                                //for static column mappings
                                sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                sqlBulk.ColumnMappings.Add("Date", "Date");
                                sqlBulk.ColumnMappings.Add("Shift_Id", "Shift_Id");
                                sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                sqlBulk.ColumnMappings.Add("Variant_Code", "Variant_Code");
                                sqlBulk.ColumnMappings.Add("Machine_Status", "Machine_Status");
                                sqlBulk.ColumnMappings.Add("OK_Parts", "OK_Parts");
                                sqlBulk.ColumnMappings.Add("NOK_Parts", "NOK_Parts");
                                sqlBulk.ColumnMappings.Add("Rework_Parts", "Rework_Parts");
                                sqlBulk.ColumnMappings.Add("Rejection_Reasons", "Rejection_Reasons");
                                sqlBulk.ColumnMappings.Add("Auto__Mode_Selected", "Auto__Mode_Selected");
                                sqlBulk.ColumnMappings.Add("Manual_Mode_Slected", "Manual_Mode_Slected");
                                sqlBulk.ColumnMappings.Add("Auto_Mode_Running", "Auto_Mode_Running");
                                sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");
                                sqlBulk.ColumnMappings.Add("OperatorID", "OperatorID");
                                sqlBulk.ColumnMappings.Add("Live_Alarm", "Live_Alarm");
                                sqlBulk.ColumnMappings.Add("Live_Loss", "Live_Loss");
                                sqlBulk.ColumnMappings.Add("Batch_code", "Batch_code");

                                try
                                {
                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted in RAWTable";
                                    inserted = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                finally
                                {
                                    conn.Close();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();

                            }

                        }

                        //sql insetion for improper date format
                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "RawTable_Date";
                                    sqlBulk.BatchSize = dt_temp.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();


                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    sqlBulk.ColumnMappings.Add("Shift_Id", "Shift_Id");
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Variant_Code", "Variant_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Status", "Machine_Status");
                                    sqlBulk.ColumnMappings.Add("OK_Parts", "OK_Parts");
                                    sqlBulk.ColumnMappings.Add("NOK_Parts", "NOK_Parts");
                                    sqlBulk.ColumnMappings.Add("Rework_Parts", "Rework_Parts");
                                    sqlBulk.ColumnMappings.Add("Rejection_Reasons", "Rejection_Reasons");
                                    sqlBulk.ColumnMappings.Add("Auto__Mode_Selected", "Auto__Mode_Selected");
                                    sqlBulk.ColumnMappings.Add("Manual_Mode_Slected", "Manual_Mode_Slected");
                                    sqlBulk.ColumnMappings.Add("Auto_Mode_Running", "Auto_Mode_Running");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");
                                    sqlBulk.ColumnMappings.Add("OperatorID", "OperatorID");
                                    sqlBulk.ColumnMappings.Add("Live_Alarm", "Live_Alarm");
                                    sqlBulk.ColumnMappings.Add("Live_Loss", "Live_Loss");
                                    sqlBulk.ColumnMappings.Add("Batch_code", "Batch_code");

                                    try
                                    {
                                        sqlBulk.WriteToServer(dt_temp);
                                        dt_temp.Dispose();
                                        statusinsert1 = "Inserted in RawTable_Date";
                                        inserted1 = true;
                                    }
                                    catch (Exception e)
                                    {
                                        return e.ToString();
                                    }
                                    finally
                                    {
                                        conn.Close();
                                    }
                                    //sqlBulk.WriteToServer(dt);
                                    //dt.Dispose();

                                }

                            }

                        }
                    }

                }


                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                string ex = dt_check + e.ToString();
                return ex;
            }

            return "false";
        }


        [Route("api/Values/Store_Operator_Data")]
        [HttpPost]
        public string Store_Operator_Data(string devicename, string filename, string path)
        {
            try
            {
                String result1 = "";
                DataTable details = new DataTable();

                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    // conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][6] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));
                dt.Columns.Add("Line_Code");
                dt.Columns.Add("Machine_Code");
                dt.Columns.Add("CompanyCode");
                dt.Columns.Add("PlantCode");
                dt.Columns.Add("Shift_Id");
                dt.Columns.Add("Variant_Code");
                dt.Columns.Add(new DataColumn("Machine_Status", typeof(int)));
                dt.Columns.Add("OperatorID");
                dt.Columns.Add(new DataColumn("Manual_CycleTime", typeof(int)));
                dt.Columns.Add(new DataColumn("StartTime_Operation", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("EndTime_Operation", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Ok_Parts", typeof(int)));
                dt.Columns.Add(new DataColumn("NOk_Parts", typeof(int)));

                // datatable for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Time_Stamp");
                dt_temp.Columns.Add("Line_Code");
                dt_temp.Columns.Add("Machine_Code");
                dt_temp.Columns.Add("CompanyCode");
                dt_temp.Columns.Add("PlantCode");
                dt_temp.Columns.Add("Shift_Id");
                dt_temp.Columns.Add("Variant_Code");
                dt_temp.Columns.Add(new DataColumn("Machine_Status", typeof(int)));
                dt_temp.Columns.Add("OperatorID");
                dt_temp.Columns.Add(new DataColumn("Manual_CycleTime", typeof(int)));
                dt_temp.Columns.Add("StartTime_Operation");
                dt_temp.Columns.Add("EndTime_Operation");
                dt_temp.Columns.Add(new DataColumn("Ok_Parts", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("NOk_Parts", typeof(int)));
                
                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                    var i = 0;

                    for (i = 0; i < (allTextLines.Length); i++)
                    {
                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                        if (DateTime.TryParse(entries[0].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue) && DateTime.TryParse(entries[10].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue) && DateTime.TryParse(entries[11].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue))
                        {
                            dc["Time_Stamp"] = Convert.ToDateTime(entries[0].ToString());
                            dc["Line_Code"] = entries[1];
                            dc["Machine_Code"] = entries[2];
                            dc["CompanyCode"] = entries[3];
                            dc["PlantCode"] = entries[4];
                            dc["Shift_Id"] = entries[5];
                            dc["Variant_Code"] = entries[6];
                            dc["Machine_Status"] = Convert.ToInt32(entries[7]);
                            dc["OperatorID"] = entries[8];
                            dc["Manual_CycleTime"] = Convert.ToInt32(entries[9]);
                            dc["StartTime_Operation"] = Convert.ToDateTime(entries[10]);
                            dc["EndTime_Operation"] = Convert.ToDateTime(entries[11]);
                            dc["Ok_Parts"] = Convert.ToInt32(entries[12]);
                            dc["NOk_Parts"] = Convert.ToInt32(entries[13]);
                            dt.Rows.Add(dc);
                        }
                        else
                        {
                            dc_temp["Time_Stamp"] = entries[0];
                            dc_temp["Line_Code"] = entries[1];
                            dc_temp["Machine_Code"] = entries[2];
                            dc_temp["CompanyCode"] = entries[3];
                            dc_temp["PlantCode"] = entries[4];
                            dc_temp["Shift_Id"] = entries[5];
                            dc_temp["Variant_Code"] = entries[6];
                            dc_temp["Machine_Status"] = Convert.ToInt32(entries[7]);
                            dc_temp["OperatorID"] = entries[8];
                            dc_temp["Manual_CycleTime"] = Convert.ToInt32(entries[9]);
                            dc_temp["StartTime_Operation"] = entries[10];
                            dc_temp["EndTime_Operation"] = entries[11];
                            dc_temp["Ok_Parts"] = Convert.ToInt32(entries[12]);
                            dc_temp["NOk_Parts"] = Convert.ToInt32(entries[13]);
                            dt_temp.Rows.Add(dc_temp);
                        }

                        //i++;
                    }
                    var entries_connection = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[3].ToString(), entries_connection[4].ToString(), entries_connection[1].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "tbl_Raw_Operator_Efficiency";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();


                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");
                                    sqlBulk.ColumnMappings.Add("Shift_Id", "Shift_Id");
                                    sqlBulk.ColumnMappings.Add("Variant_Code", "Variant_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Status", "Machine_Status");
                                    sqlBulk.ColumnMappings.Add("OperatorID", "OperatorID");
                                    sqlBulk.ColumnMappings.Add("Manual_CycleTime", "Manual_CycleTime");
                                    sqlBulk.ColumnMappings.Add("StartTime_Operation", "StartTime_Operation");
                                    sqlBulk.ColumnMappings.Add("EndTime_Operation", "EndTime_Operation");
                                    sqlBulk.ColumnMappings.Add("Ok_Parts", "Ok_Parts");
                                    sqlBulk.ColumnMappings.Add("NOk_Parts", "NOk_Parts");


                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into tbl_Raw_Operator_Efficiency";
                                    inserted = true;
                                    // conn.Close();
                                }

                            }

                        }



                        // bulk insert for improper date
                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "tbl_Raw_Operator_Efficiency_Date";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();


                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");
                                    sqlBulk.ColumnMappings.Add("Shift_Id", "Shift_Id");
                                    sqlBulk.ColumnMappings.Add("Variant_Code", "Variant_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Status", "Machine_Status");
                                    sqlBulk.ColumnMappings.Add("OperatorID", "OperatorID");
                                    sqlBulk.ColumnMappings.Add("Manual_CycleTime", "Manual_CycleTime");
                                    sqlBulk.ColumnMappings.Add("StartTime_Operation", "StartTime_Operation");
                                    sqlBulk.ColumnMappings.Add("EndTime_Operation", "EndTime_Operation");
                                    sqlBulk.ColumnMappings.Add("Ok_Parts", "Ok_Parts");
                                    sqlBulk.ColumnMappings.Add("NOk_Parts", "NOk_Parts");


                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into tbl_Raw_Operator_Efficiency_Date";
                                    inserted1 = true;
                                    // conn.Close();
                                }

                            }



                        }

                    }
                }


                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }



        [Route("api/Values/Store_Toolist_Data")]
        [HttpPost]
        public string Store_Toolist_Data(string devicename, string filename, string path)
        {
            try
            {

                String result1 = "";

                DataTable details = new DataTable();
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);

                    //  conn.Close();
                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                DataTable dt = new DataTable();
                dt.Columns.Add("Line_Code");
                dt.Columns.Add("Machine_Code");
                dt.Columns.Add("ToolID");
                dt.Columns.Add("Classification");
                dt.Columns.Add(new DataColumn("CurrentLifeCycle", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));
                dt.Columns.Add("CompanyCode");
                dt.Columns.Add("PlantCode");
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));

                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Line_Code");
                dt_temp.Columns.Add("Machine_Code");
                dt_temp.Columns.Add("ToolID");
                dt_temp.Columns.Add("Classification");
                dt_temp.Columns.Add(new DataColumn("CurrentLifeCycle", typeof(decimal)));
                dt_temp.Columns.Add("Time_Stamp");
                dt_temp.Columns.Add("CompanyCode");
                dt_temp.Columns.Add("PlantCode");
                dt_temp.Columns.Add("Date");

                
                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {
                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                    var i = 0;

                    for (i = 0; i < (allTextLines.Length); i++)
                    {
                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                        if (DateTime.TryParse(entries[5].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue) && DateTime.TryParse(entries[8].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue))
                        {
                            dc["Line_Code"] = entries[0];
                            dc["Machine_Code"] = entries[1];
                            dc["ToolID"] = entries[2];
                            dc["Classification"] = entries[3];
                            dc["CurrentLifeCycle"] = Convert.ToDecimal(entries[4].ToString());
                            dc["Time_Stamp"] = Convert.ToDateTime(entries[5].ToString());
                            dc["CompanyCode"] = entries[6];
                            dc["PlantCode"] = entries[7];
                            dc["Date"] = Convert.ToDateTime(entries[8].ToString());
                            dt.Rows.Add(dc);
                        }
                        else
                        {
                            dc_temp["Line_Code"] = entries[0];
                            dc_temp["Machine_Code"] = entries[1];
                            dc_temp["ToolID"] = entries[2];
                            dc_temp["Classification"] = entries[3];
                            dc_temp["CurrentLifeCycle"] = Convert.ToDecimal(entries[4].ToString());
                            dc_temp["Time_Stamp"] = entries[5].ToString();
                            dc_temp["CompanyCode"] = entries[6];
                            dc_temp["PlantCode"] = entries[7];
                            dc_temp["Date"] = entries[8].ToString();
                            dt_temp.Rows.Add(dc_temp);
                        }


                        //i++;
                    }
                    var entries_connection = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[6].ToString(), entries_connection[7].ToString(), entries_connection[0].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "tbl_Raw_Toollife";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();

                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("ToolID", "ToolID");
                                    sqlBulk.ColumnMappings.Add("Classification", "Classification");
                                    sqlBulk.ColumnMappings.Add("CurrentLifeCycle", "CurrentLifeCycle");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    SqlCommand cmd = conn.CreateCommand();
                                    cmd.CommandText = "DISABLE TRIGGER INSERT_RAWTable_TOOLLIFE_DATA_INTO_tbl_temp_toollife_rawdata ON [tbl_Raw_Toollife]";
                                    cmd.CommandTimeout = 15;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();

                                    try
                                    {
                                        sqlBulk.WriteToServer(dt);
                                        SqlCommand cmd1 = conn.CreateCommand();
                                        cmd1.CommandText = "Enable TRIGGER INSERT_RAWTable_TOOLLIFE_DATA_INTO_tbl_temp_toollife_rawdata ON [tbl_Raw_Toollife]";
                                        cmd1.CommandTimeout = 15;
                                        cmd1.CommandType = CommandType.Text;
                                        cmd1.ExecuteNonQuery();
                                        dt.Dispose();
                                        statusinsert = "Inserted into tbl_Raw_Toollife";
                                        inserted = true;
                                    }
                                    catch (Exception e)
                                    {
                                        return e.ToString();
                                    }
                                    //  conn.Close();
                                }

                            }

                        }

                        // sql bulk copy for improper date
                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "tbl_Raw_Toollife_Date";
                                    sqlBulk.BatchSize = dt_temp.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();

                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("ToolID", "ToolID");
                                    sqlBulk.ColumnMappings.Add("Classification", "Classification");
                                    sqlBulk.ColumnMappings.Add("CurrentLifeCycle", "CurrentLifeCycle");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    SqlCommand cmd = conn.CreateCommand();
                                    cmd.CommandText = "DISABLE TRIGGER INSERT_RAWTable_TOOLLIFE_DATA_INTO_tbl_temp_toollife_rawdata ON [tbl_Raw_Toollife]";
                                    cmd.CommandTimeout = 15;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();

                                    try
                                    {
                                        sqlBulk.WriteToServer(dt_temp);
                                        SqlCommand cmd1 = conn.CreateCommand();
                                        cmd1.CommandText = "Enable TRIGGER INSERT_RAWTable_TOOLLIFE_DATA_INTO_tbl_temp_toollife_rawdata ON [tbl_Raw_Toollife]";
                                        cmd1.CommandTimeout = 15;
                                        cmd1.CommandType = CommandType.Text;
                                        cmd1.ExecuteNonQuery();
                                        dt_temp.Dispose();
                                        statusinsert1 = "Inserted into tbl_Raw_Toollife_Date";
                                        inserted1 = true;
                                    }
                                    catch (Exception e)
                                    {
                                        return e.ToString();
                                    }
                                    //conn.Close();
                                }

                            }


                        }
                    }

                }

                    //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {

            }
            return "false";
        }


        [Route("api/Values/Store_Alarm_Data")]
        [HttpPost]
        public string Store_Alarm_Data(string devicename, string filename, string path)
        {
            try
            {
                String result1 = "";

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                DataTable details = new DataTable();
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);

                    //  conn.Close();
                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                DataTable dt = new DataTable();
                dt.Columns.Add("Line_Code");
                dt.Columns.Add("Machine_Code");
                dt.Columns.Add("Shift_ID");
                dt.Columns.Add("Alarm_ID");
                dt.Columns.Add(new DataColumn("Start_Time", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("End_Time", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                dt.Columns.Add("CompanyCode");
                dt.Columns.Add("PlantCode");


                // for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Line_Code");
                dt_temp.Columns.Add("Machine_Code");
                dt_temp.Columns.Add("Shift_ID");
                dt_temp.Columns.Add("Alarm_ID");
                dt_temp.Columns.Add("Start_Time");
                dt_temp.Columns.Add("End_Time");
                dt_temp.Columns.Add("Time_Stamp");
                dt_temp.Columns.Add("Date");
                dt_temp.Columns.Add("CompanyCode");
                dt_temp.Columns.Add("PlantCode");
                
                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                    var i = 0;


                    for (i = 0; i < (allTextLines.Length); i++)
                    {
                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);

                        if (DateTime.TryParse(entries[4].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue) && DateTime.TryParse(entries[5].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue) && DateTime.TryParse(entries[6].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue) && DateTime.TryParse(entries[7].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue))
                        {
                            dc["Line_Code"] = entries[0];
                            dc["Machine_Code"] = entries[1];
                            dc["Shift_ID"] = entries[2];
                            dc["Alarm_ID"] = entries[3];
                            dc["Start_Time"] = Convert.ToDateTime(entries[4].ToString());
                            dc["End_Time"] = Convert.ToDateTime(entries[5].ToString());
                            dc["Time_Stamp"] = Convert.ToDateTime(entries[6].ToString());
                            dc["Date"] = Convert.ToDateTime(entries[7].ToString());
                            dc["CompanyCode"] = entries[8];
                            dc["PlantCode"] = entries[9];
                            dt.Rows.Add(dc);
                        }
                        else
                        {
                            dc_temp["Line_Code"] = entries[0];
                            dc_temp["Machine_Code"] = entries[1];
                            dc_temp["Shift_ID"] = entries[2];
                            dc_temp["Alarm_ID"] = entries[3];
                            dc_temp["Start_Time"] = entries[4];
                            dc_temp["End_Time"] = entries[5];
                            dc_temp["Time_Stamp"] = entries[6];
                            dc_temp["Date"] = entries[7];
                            dc_temp["CompanyCode"] = entries[8];
                            dc_temp["PlantCode"] = entries[9];
                            dt_temp.Rows.Add(dc_temp);
                        }

                        //i++;
                    }
                    var entries_connection = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[8].ToString(), entries_connection[9].ToString(), entries_connection[0].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "MachineAlarm";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;



                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Shift_ID", "Shift_ID");
                                    sqlBulk.ColumnMappings.Add("Alarm_ID", "Alarm_ID");
                                    sqlBulk.ColumnMappings.Add("Start_Time", "Start_Time");
                                    sqlBulk.ColumnMappings.Add("End_Time", "End_Time");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");



                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into MachineAlarm";
                                    inserted = true;
                                    // conn.Close();
                                }

                            }

                        }


                        // bulk insert for improper date
                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "MachineAlarm_Date";
                                    sqlBulk.BatchSize = dt_temp.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;



                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Shift_ID", "Shift_ID");
                                    sqlBulk.ColumnMappings.Add("Alarm_ID", "Alarm_ID");
                                    sqlBulk.ColumnMappings.Add("Start_Time", "Start_Time");
                                    sqlBulk.ColumnMappings.Add("End_Time", "End_Time");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");



                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into MachineAlarm_Date";
                                    inserted1 = true;
                                    // conn.Close();
                                }

                            }


                        }
                    }

                }


                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "false";
        }


        [Route("api/Values/Store_Losses_Data")]
        [HttpPost]
        public string Store_Losses_Data(string devicename, string filename, string path)
        {
            try
            {
                String result1 = "";

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                DataTable details = new DataTable();
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    //  conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                DataTable dt = new DataTable();
                dt.Columns.Add("Line_Code");
                dt.Columns.Add("Machine_Code");
                dt.Columns.Add("Shift_ID");
                dt.Columns.Add("Loss_ID");
                dt.Columns.Add(new DataColumn("Start_Time", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("End_Time", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                dt.Columns.Add("CompanyCode");
                dt.Columns.Add("PlantCode");

                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Line_Code");
                dt_temp.Columns.Add("Machine_Code");
                dt_temp.Columns.Add("Shift_ID");
                dt_temp.Columns.Add("Loss_ID");
                dt_temp.Columns.Add("Start_Time");
                dt_temp.Columns.Add("End_Time");
                dt_temp.Columns.Add("Time_Stamp");
                dt_temp.Columns.Add("Date");
                dt_temp.Columns.Add("CompanyCode");
                dt_temp.Columns.Add("PlantCode");
                
                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                    var i = 0;
                    for (i = 0; i < (allTextLines.Length); i++)
                    {
                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                        if (DateTime.TryParse(entries[4].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue) && DateTime.TryParse(entries[5].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue) && DateTime.TryParse(entries[6].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue) && DateTime.TryParse(entries[9].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue))
                        {
                            dc["Line_Code"] = entries[0];
                            dc["Machine_Code"] = entries[1];
                            dc["Shift_ID"] = entries[2];
                            dc["Loss_ID"] = entries[3];
                            dc["Start_Time"] = Convert.ToDateTime(entries[4].ToString());
                            dc["End_Time"] = Convert.ToDateTime(entries[5].ToString());
                            dc["Time_Stamp"] = Convert.ToDateTime(entries[6].ToString());

                            dc["CompanyCode"] = entries[7];
                            dc["PlantCode"] = entries[8];
                            dc["Date"] = Convert.ToDateTime(entries[9].ToString());
                            dt.Rows.Add(dc);
                        }
                        else
                        {
                            dc_temp["Line_Code"] = entries[0];
                            dc_temp["Machine_Code"] = entries[1];
                            dc_temp["Shift_ID"] = entries[2];
                            dc_temp["Loss_ID"] = entries[3];
                            dc_temp["Start_Time"] = entries[4].ToString();
                            dc_temp["End_Time"] = entries[5].ToString();
                            dc_temp["Time_Stamp"] = entries[6].ToString();

                            dc_temp["CompanyCode"] = entries[7];
                            dc_temp["PlantCode"] = entries[8];
                            dc_temp["Date"] = entries[9].ToString();
                            dt_temp.Rows.Add(dc_temp);

                        }

                    }
                    var entries_connection = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[7].ToString(), entries_connection[8].ToString(), entries_connection[0].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "MachineLoss";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;



                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Shift_ID", "Shift_ID");
                                    sqlBulk.ColumnMappings.Add("Loss_ID", "Loss_ID");
                                    sqlBulk.ColumnMappings.Add("Start_Time", "Start_Time");
                                    sqlBulk.ColumnMappings.Add("End_Time", "End_Time");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");



                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into MachineLoss";
                                    inserted = true;
                                    //  conn.Close();
                                }

                            }

                        }



                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "MachineLoss_Date";
                                    sqlBulk.BatchSize = dt_temp.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;



                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Shift_ID", "Shift_ID");
                                    sqlBulk.ColumnMappings.Add("Loss_ID", "Loss_ID");
                                    sqlBulk.ColumnMappings.Add("Start_Time", "Start_Time");
                                    sqlBulk.ColumnMappings.Add("End_Time", "End_Time");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");
                                    sqlBulk.ColumnMappings.Add("Date", "Date");
                                    sqlBulk.ColumnMappings.Add("CompanyCode", "CompanyCode");
                                    sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");



                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into MachineLoss_Date";
                                    inserted1 = true;
                                    // conn.Close();
                                }

                            }


                        }
                    }

                }

                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "false";
        }

        [Route("api/Values/check_encryption")]
        [HttpPost]
        public string check_encryption()
        {
            var encrypt1 = Base64Encode("Teal@123");
            var encrypt2 = Base64Encode(encrypt1);
            return encrypt2;

        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public void insertimproper_rawtable()
        {

        }

        [Route("api/Values/Store_Cycletime_Data")]
        [HttpPost]
        public string Store_Cycletime_Data(string devicename, string filename, string path)
        {
            try
            {

                String result1 = "";
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                DataTable details = new DataTable();
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    // conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                DataTable dt = new DataTable();
                dt.Columns.Add("Line_Code");
                dt.Columns.Add("Machine_Code");
                dt.Columns.Add("Shift_Id");
                dt.Columns.Add("Variant_Code");
                dt.Columns.Add("Companycode");
                dt.Columns.Add("Plantcode");
                dt.Columns.Add("OperatorID");
                dt.Columns.Add(new DataColumn("Ok_Parts", typeof(int)));
                dt.Columns.Add(new DataColumn("NOk_Parts", typeof(int)));
                dt.Columns.Add(new DataColumn("Rework_Parts", typeof(int)));
                dt.Columns.Add("Reject_Reason");
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));


                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Line_Code");
                dt_temp.Columns.Add("Machine_Code");
                dt_temp.Columns.Add("Shift_Id");
                dt_temp.Columns.Add("Variant_Code");
                dt_temp.Columns.Add("Companycode");
                dt_temp.Columns.Add("Plantcode");
                dt_temp.Columns.Add("OperatorID");
                dt_temp.Columns.Add(new DataColumn("Ok_Parts", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("NOk_Parts", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Rework_Parts", typeof(int)));
                dt_temp.Columns.Add("Reject_Reason");
                dt_temp.Columns.Add("Time_Stamp");
                
                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);
                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                    var i = 0;


                    for (i = 1; i < (allTextLines.Length); i++)
                    {
                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                        if (DateTime.TryParse(entries[11].ToString(),
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dateValue))
                        {
                            dc["Line_Code"] = entries[0];
                            dc["Machine_Code"] = entries[1];
                            dc["Shift_Id"] = entries[2];
                            dc["Variant_Code"] = entries[3];
                            dc["Companycode"] = entries[4];
                            dc["Plantcode"] = entries[5];
                            dc["OperatorID"] = entries[6];
                            dc["Ok_Parts"] = Convert.ToInt32(entries[7]);
                            dc["NOk_Parts"] = Convert.ToInt32(entries[8]);
                            dc["Rework_Parts"] = Convert.ToInt32(entries[9]);
                            dc["Reject_Reason"] = entries[10];
                            dc["Time_Stamp"] = Convert.ToDateTime(entries[11].ToString());
                            dt.Rows.Add(dc);
                        }
                        else
                        {
                            dc_temp["Line_Code"] = entries[0];
                            dc_temp["Machine_Code"] = entries[1];
                            dc_temp["Shift_Id"] = entries[2];
                            dc_temp["Variant_Code"] = entries[3];
                            dc_temp["Companycode"] = entries[4];
                            dc_temp["Plantcode"] = entries[5];
                            dc_temp["OperatorID"] = entries[6];
                            dc_temp["Ok_Parts"] = Convert.ToInt32(entries[7]);
                            dc_temp["NOk_Parts"] = Convert.ToInt32(entries[8]);
                            dc_temp["Rework_Parts"] = Convert.ToInt32(entries[9]);
                            dc_temp["Reject_Reason"] = entries[10];
                            dc_temp["Time_Stamp"] = entries[11].ToString();
                            dt_temp.Rows.Add(dc_temp);

                        }



                        //i++;
                    }
                    var entries_connection = allTextLines[1].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[4].ToString(), entries_connection[5].ToString(), entries_connection[0].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "Cycletime";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;



                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Shift_Id", "Shift_Id");
                                    sqlBulk.ColumnMappings.Add("Variant_Code", "Variant_Code");
                                    sqlBulk.ColumnMappings.Add("Companycode", "Companycode");
                                    sqlBulk.ColumnMappings.Add("Plantcode", "Plantcode");
                                    sqlBulk.ColumnMappings.Add("OperatorID", "OperatorID");
                                    sqlBulk.ColumnMappings.Add("Ok_Parts", "Ok_Parts");
                                    sqlBulk.ColumnMappings.Add("NOk_Parts", "NOk_Parts");
                                    sqlBulk.ColumnMappings.Add("Rework_Parts", "Rework_Parts");
                                    sqlBulk.ColumnMappings.Add("Reject_Reason", "Reject_Reason");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");



                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into Cycletime";
                                    inserted = true;
                                    // conn.Close();
                                }

                            }

                        }


                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "Cycletime_Date";
                                    sqlBulk.BatchSize = dt_temp.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;



                                    //for static column mappings
                                    sqlBulk.ColumnMappings.Add("Line_Code", "Line_Code");
                                    sqlBulk.ColumnMappings.Add("Machine_Code", "Machine_Code");
                                    sqlBulk.ColumnMappings.Add("Shift_Id", "Shift_Id");
                                    sqlBulk.ColumnMappings.Add("Variant_Code", "Variant_Code");
                                    sqlBulk.ColumnMappings.Add("Companycode", "Companycode");
                                    sqlBulk.ColumnMappings.Add("Plantcode", "Plantcode");
                                    sqlBulk.ColumnMappings.Add("OperatorID", "OperatorID");
                                    sqlBulk.ColumnMappings.Add("Ok_Parts", "Ok_Parts");
                                    sqlBulk.ColumnMappings.Add("NOk_Parts", "NOk_Parts");
                                    sqlBulk.ColumnMappings.Add("Rework_Parts", "Rework_Parts");
                                    sqlBulk.ColumnMappings.Add("Reject_Reason", "Reject_Reason");
                                    sqlBulk.ColumnMappings.Add("Time_Stamp", "Time_Stamp");



                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into Cycletime_Date";
                                    inserted1 = true;
                                }

                            }


                        }
                    }


                }

                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "false";
        }


        [Route("api/Values/Store_Errors")]
        [HttpPost]

        public string Store_Errors(string devicename, string filename, string path)
        {
            string dt_check = "";
            try
            {
                String result1 = "";
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                DataTable details = new DataTable();
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][6] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);

                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }


                    //var entries = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                    var i = 0;
                    //for proper insertion
                    DataTable dt = new DataTable();
                    //dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));
                    //dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                    //dt.Columns.Add("Shift_Id");
                    //dt.Columns.Add("Line_Code");
                    //dt.Columns.Add("Machine_Code");
                    //dt.Columns.Add("Variant_Code");
                    //dt.Columns.Add("Machine_Status");
                    //dt.Columns.Add(new DataColumn("OK_Parts", typeof(int)));
                    //dt.Columns.Add(new DataColumn("NOK_Parts", typeof(int)));
                    //dt.Columns.Add(new DataColumn("Rework_Parts", typeof(int)));
                    //dt.Columns.Add("Rejection_Reasons");
                    //dt.Columns.Add(new DataColumn("Auto__Mode_Selected", typeof(int)));
                    //dt.Columns.Add(new DataColumn("Manual_Mode_Slected", typeof(int)));
                    //dt.Columns.Add(new DataColumn("Auto_Mode_Running", typeof(int)));
                    //dt.Columns.Add("CompanyCode");
                    //dt.Columns.Add("PlantCode");
                    //dt.Columns.Add("OperatorID");
                    //dt.Columns.Add("Live_Alarm");
                    //dt.Columns.Add("Live_Loss");
                    //dt.Columns.Add("Batch_code");


                    // datatable column mapping 
                    var entries1 = allTextLines[0].Split(new string[] { ",", "\r" }, StringSplitOptions.None);

                    dt.Columns.Add(new DataColumn(entries1[0].ToString(), typeof(DateTime)));
                    dt.Columns.Add(new DataColumn(entries1[1].ToString()));
                    dt.Columns.Add(new DataColumn(entries1[2].ToString()));
                    dt.Columns.Add(new DataColumn(entries1[3].ToString()));
                    dt.Columns.Add(new DataColumn(entries1[4].ToString()));
                    dt.Columns.Add(new DataColumn(entries1[5].ToString()));
                    for (int i1 = 6; i1 < entries1.Length; i1++)
                    {
                        dt.Columns.Add(new DataColumn(entries1[i1].Replace("\r\n", "")).ToString().Trim(), typeof(int));
                    }



                    //for improper date
                    DataTable dt_temp = new DataTable();
                    dt_temp.Columns.Add(new DataColumn(entries1[0].ToString()));
                    dt_temp.Columns.Add(new DataColumn(entries1[1].ToString()));
                    dt_temp.Columns.Add(new DataColumn(entries1[2].ToString()));
                    dt_temp.Columns.Add(new DataColumn(entries1[3].ToString()));
                    dt_temp.Columns.Add(new DataColumn(entries1[4].ToString()));
                    dt_temp.Columns.Add(new DataColumn(entries1[5].ToString()));
                    for (int i1 = 6; i1 < entries1.Length; i1++)
                    {
                        dt_temp.Columns.Add(new DataColumn(entries1[i1].Replace("\r\n", "")).ToString().Trim(), typeof(int));
                    }

                    for (i = 1; i < (allTextLines.Length); i++)
                    {

                        DataRow dc = dt.NewRow();
                        DataRow dc_temp = dt_temp.NewRow();
                        DateTime dateValue;
                        var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                        if ((DateTime.TryParse(entries[0].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateValue)))
                        {
                            dc[entries1[0].ToString()] = entries[0].ToString();
                            dc[entries1[1].ToString()] = entries[1].ToString();
                            dc[entries1[2].ToString()] = entries[2].ToString();
                            dc[entries1[3].ToString()] = entries[3].ToString();
                            dc[entries1[4].ToString()] = entries[4].ToString();
                            dc[entries1[5].ToString()] = entries[5].ToString();

                            for (int i2 = 6; i2 < entries.Length; i2++)
                            {
                                dc[entries1[i2].Replace("\r\n", "").ToString().Trim()] = Convert.ToInt32(entries[i2]);
                            }

                            dt.Rows.Add(dc);

                        }

                        else
                        {
                            dc_temp[entries1[0].ToString()] = entries[0].ToString();
                            dc_temp[entries1[1].ToString()] = entries[1].ToString();
                            dc_temp[entries1[2].ToString()] = entries[2].ToString();
                            dc_temp[entries1[3].ToString()] = entries[3].ToString();
                            dc_temp[entries1[4].ToString()] = entries[4].ToString();
                            dc_temp[entries1[5].ToString()] = entries[5].ToString();

                            for (int i2 = 6; i2 < entries.Length; i2++)
                            {
                                dc_temp[entries1[i2].ToString()] = Convert.ToInt32(entries[i2]);
                            }
                            dt_temp.Rows.Add(dc_temp);
                        }
                        //dt_check = entries[0].ToString();



                        //i++;
                    }
                    var entries_connection = allTextLines[1].Split(new string[] { "," }, StringSplitOptions.None);
                    database_connectionController d = new database_connectionController();
                    string con_string = d.Getconnectionstring(entries_connection[5].ToString(), entries_connection[4].ToString(), entries_connection[3].ToString());
                    if (con_string == "0")
                    {
                        return "Couldnot connect to database";
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            //sql insetion for proper date format
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "Errors";
                                    sqlBulk.BatchSize = dt.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();




                                    // for dynamic column mappings if column name is in csv
                                    string[] a = { };
                                    for (i = 0; i < 1; i++)
                                    {
                                        a = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);


                                    }
                                    for (i = 0; i < ((a.Length)); i++)
                                    {

                                        sqlBulk.ColumnMappings.Add((a[i].Replace("\r\n", "")).Trim(), (a[i].Replace("\r\n", "")).Trim());
                                    }
                                    try
                                    {
                                        sqlBulk.WriteToServer(dt);
                                        dt.Dispose();
                                        statusinsert = "Inserted into Errors";
                                        inserted = true;
                                    }
                                    catch (Exception e)
                                    {
                                        return e.ToString();
                                    }
                                    //sqlBulk.WriteToServer(dt);
                                    //dt.Dispose();
                                    conn.Close();
                                }

                            }

                        }



                        //sql insetion for improper date format
                        if (dt_temp.Rows.Count > 0)
                        {
                            using (SqlConnection conn = new SqlConnection(con_string))
                            {
                                using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                                {
                                    conn.Open();
                                    sqlBulk.DestinationTableName = "Errors_Date";
                                    sqlBulk.BatchSize = dt_temp.Rows.Count;
                                    sqlBulk.BulkCopyTimeout = 6000000;


                                    sqlBulk.ColumnMappings.Clear();


                                    string[] a = { };
                                    for (i = 0; i < 1; i++)
                                    {
                                        a = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);


                                    }
                                    for (i = 0; i < ((a.Length)); i++)
                                    {

                                        sqlBulk.ColumnMappings.Add(a[i], a[i]);
                                    }

                                    try
                                    {
                                        sqlBulk.WriteToServer(dt_temp);
                                        dt_temp.Dispose();
                                        statusinsert1 = "Inserted into Errors_Date";
                                        inserted1 = true;
                                    }
                                    catch (Exception e)
                                    {
                                        return e.ToString();
                                    }
                                    //sqlBulk.WriteToServer(dt);
                                    //dt.Dispose();
                                    conn.Close();
                                }

                            }


                        }
                    }
                }
                

                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                string ex = dt_check + e.ToString();
                return ex;
            }

            return "false";
        }

        [Route("api/Values/Store_Errors_bitswise")]
        [HttpPost]
        public string Store_Errors_bitswise(string devicename, string filename, string path)
        {
            try
            {
                String result1 = "";

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                DataTable details = new DataTable();
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][6] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);

                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                }

                //var entries = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                var i = 0;
                //for proper insertion
                DataTable dt = new DataTable();



                // datatable column mapping 
                var entries1 = allTextLines[0].Split(new string[] { ",", "\r" }, StringSplitOptions.None);

                dt.Columns.Add(new DataColumn("TimeStamp", typeof(DateTime)));
                dt.Columns.Add("Shift_ID");
                dt.Columns.Add("Machine_code");
                dt.Columns.Add("Line_code");
                dt.Columns.Add("PlantCode");
                dt.Columns.Add("Companycode");

                int k = 1;
                int j = 32;
                for (int i1 = 6; i1 < entries1.Length; i1++)
                {

                    for (int i2 = k; i2 <= j; i2++)
                    {
                        dt.Columns.Add(new DataColumn("E" + i2).ToString().Trim(), typeof(int));
                    }
                    k += 32;
                    j += 32;
                }



                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("Timestamp");
                dt_temp.Columns.Add("Shift_ID");
                dt_temp.Columns.Add("Machine_code");
                dt_temp.Columns.Add("Line_code");
                dt_temp.Columns.Add("PlantCode");
                dt_temp.Columns.Add("Companycode");

                int kk = 1;
                int jj = 32;
                for (int i1 = 6; i1 < entries1.Length; i1++)
                {

                    for (int i2 = kk; i2 <= jj; i2++)
                    {
                        dt_temp.Columns.Add(new DataColumn("E" + i2).ToString().Trim(), typeof(int));
                    }
                    kk += 32;
                    jj += 32;
                }

                for (i = 0; i < (allTextLines.Length); i++)
                {

                    DataRow dc = dt.NewRow();
                    DataRow dc_temp = dt_temp.NewRow();
                    DateTime dateValue;
                    var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                    if ((DateTime.TryParse(entries[0].ToString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dateValue)))
                    {
                        dc["Timestamp"] = Convert.ToDateTime(entries[0].ToString());
                        dc["Shift_ID"] = entries[1].ToString();
                        dc["Machine_code"] = entries[2].ToString();
                        dc["Line_code"] = entries[3].ToString();
                        dc["PlantCode"] = entries[4].ToString();
                        dc["Companycode"] = entries[5].ToString();

                        int kk1 = 1;
                        for (int i1 = 6; i1 < entries1.Length; i1++)
                        {
                            int number = Convert.ToInt32(entries1[i1]);
                            string result2 = Reverse(Convert.ToString(number, 2).PadLeft(32, '0'));
                            for (int i2 = 0; i2 < result2.Length; i2++)
                            {
                                kk1++;
                                string ee = "E" + kk1 + "";
                                dc[ee] = result2[i2].ToString();

                            }

                        }



                        dt.Rows.Add(dc);

                    }

                    else
                    {
                        dc_temp["Timestamp"] = entries[0].ToString();
                        dc_temp["Shift_ID"] = entries[1].ToString();
                        dc_temp["Machine_code"] = entries[2].ToString();
                        dc_temp["Line_code"] = entries[3].ToString();
                        dc_temp["PlantCode"] = entries[4].ToString();
                        dc_temp["Companycode"] = entries[5].ToString();

                        int kk11 = 0;
                        for (int i1 = 6; i1 < entries1.Length; i1++)
                        {
                            int number = Convert.ToInt32(entries1[i1]);
                            string result2 = Reverse(Convert.ToString(number, 2).PadLeft(32, '0'));
                            for (int i2 = 0; i2 < result2.Length; i2++)
                            {
                                kk11++;
                                string ee = "E" + kk11 + "";
                                dc_temp[ee] = result2[i2].ToString();

                            }

                        }


                        dt_temp.Rows.Add(dc_temp);
                    }
                    //dt_check = entries[0].ToString();



                    //i++;
                }
                var entries_connection = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                database_connectionController d = new database_connectionController();
                string con_string = d.Getconnectionstring(entries_connection[5].ToString(), entries_connection[4].ToString(), entries_connection[3].ToString());
                if (con_string == "0")
                {
                    return "Couldnot connect to database";
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Errors";
                                sqlBulk.BatchSize = dt.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();

                                foreach (DataColumn column in dt.Columns)
                                {
                                    sqlBulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                                }


                                try
                                {
                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into Errors";
                                    inserted = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();
                                conn.Close();
                            }

                        }

                    }
                    //sql insetion for proper date format



                    //sql insetion for improper date format
                    if (dt_temp.Rows.Count > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Errors_Date";
                                sqlBulk.BatchSize = dt_temp.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();

                                foreach (DataColumn column in dt_temp.Columns)
                                {
                                    sqlBulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                                }


                                try
                                {
                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into Errors_Date";
                                    inserted1 = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();
                                conn.Close();
                            }

                        }

                    }
                }

                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;

            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "false";
        }


        [Route("api/Values/Store_Warnings")]
        [HttpPost]

        public string Store_Warnings(string devicename, string filename, string path)
        {
            string dt_check = "";
            try
            {
                String result1 = "";
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                DataTable details = new DataTable();
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7 ] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);

                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                }

                //var entries = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                var i = 0;
                //for proper insertion
                DataTable dt = new DataTable();



                // datatable column mapping 
                var entries1 = allTextLines[0].Split(new string[] { ",", "\r" }, StringSplitOptions.None);

                dt.Columns.Add(new DataColumn(entries1[0].ToString(), typeof(DateTime)));
                dt.Columns.Add(new DataColumn(entries1[1].ToString()));
                dt.Columns.Add(new DataColumn(entries1[2].ToString()));
                dt.Columns.Add(new DataColumn(entries1[3].ToString()));
                dt.Columns.Add(new DataColumn(entries1[4].ToString()));
                dt.Columns.Add(new DataColumn(entries1[5].ToString()));
                for (int i1 = 6; i1 < entries1.Length; i1++)
                {
                    dt.Columns.Add(new DataColumn(entries1[i1].Replace("\r\n", "")).ToString().Trim(), typeof(int));
                }



                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add(new DataColumn(entries1[0].ToString()));
                dt_temp.Columns.Add(new DataColumn(entries1[1].ToString()));
                dt_temp.Columns.Add(new DataColumn(entries1[2].ToString()));
                dt_temp.Columns.Add(new DataColumn(entries1[3].ToString()));
                dt_temp.Columns.Add(new DataColumn(entries1[4].ToString()));
                dt_temp.Columns.Add(new DataColumn(entries1[5].ToString()));
                for (int i1 = 6; i1 < entries1.Length; i1++)
                {
                    dt_temp.Columns.Add(new DataColumn(entries1[i1].Replace("\r\n", "")).ToString().Trim(), typeof(int));
                }

                for (i = 1; i < (allTextLines.Length); i++)
                {

                    DataRow dc = dt.NewRow();
                    DataRow dc_temp = dt_temp.NewRow();
                    DateTime dateValue;
                    var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                    if ((DateTime.TryParse(entries[0].ToString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dateValue)))
                    {
                        dc[entries1[0].ToString()] = entries[0].ToString();
                        dc[entries1[1].ToString()] = entries[1].ToString();
                        dc[entries1[2].ToString()] = entries[2].ToString();
                        dc[entries1[3].ToString()] = entries[3].ToString();
                        dc[entries1[4].ToString()] = entries[4].ToString();
                        dc[entries1[5].ToString()] = entries[5].ToString();

                        for (int i2 = 6; i2 < entries.Length; i2++)
                        {
                            dc[entries1[i2].Replace("\r\n", "").ToString().Trim()] = Convert.ToInt32(entries[i2]);
                        }

                        dt.Rows.Add(dc);

                    }

                    else
                    {
                        dc_temp[entries1[0].ToString()] = entries[0].ToString();
                        dc_temp[entries1[1].ToString()] = entries[1].ToString();
                        dc_temp[entries1[2].ToString()] = entries[2].ToString();
                        dc_temp[entries1[3].ToString()] = entries[3].ToString();
                        dc_temp[entries1[4].ToString()] = entries[4].ToString();
                        dc_temp[entries1[5].ToString()] = entries[5].ToString();

                        for (int i2 = 6; i2 < entries.Length; i2++)
                        {
                            dc_temp[entries1[i2].ToString()] = Convert.ToInt32(entries[i2]);
                        }
                        dt_temp.Rows.Add(dc_temp);
                    }
                    //dt_check = entries[0].ToString();



                    //i++;
                }
                var entries_connection = allTextLines[1].Split(new string[] { "," }, StringSplitOptions.None);
                database_connectionController d = new database_connectionController();
                string con_string = d.Getconnectionstring(entries_connection[5].ToString(), entries_connection[4].ToString(), entries_connection[3].ToString());
                if (con_string == "0")
                {
                    return "Couldnot connect to database";
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        //sql insetion for proper date format
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Warnings";
                                sqlBulk.BatchSize = dt.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();




                                // for dynamic column mappings if column name is in csv
                                string[] a = { };
                                for (i = 0; i < 1; i++)
                                {
                                    a = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);


                                }
                                for (i = 0; i < ((a.Length)); i++)
                                {

                                    sqlBulk.ColumnMappings.Add((a[i].Replace("\r\n", "")).Trim(), (a[i].Replace("\r\n", "")).Trim());
                                }
                                try
                                {
                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into Warnings";
                                    inserted = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();
                                conn.Close();
                            }

                        }
                    }

                    //sql insetion for improper date format
                    if (dt_temp.Rows.Count > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Warnings_Date";
                                sqlBulk.BatchSize = dt_temp.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();


                                string[] a = { };
                                for (i = 0; i < 1; i++)
                                {
                                    a = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);


                                }
                                for (i = 0; i < ((a.Length)); i++)
                                {

                                    sqlBulk.ColumnMappings.Add(a[i], a[i]);
                                }

                                try
                                {
                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into Warnings_Date";
                                    inserted1 = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();
                                conn.Close();
                            }

                        }

                    }
                }


                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                string ex = dt_check + e.ToString();
                return ex;
            }

            return "false";
        }



        [Route("api/Values/Store_CycleTimeNew")]
        [HttpPost]

        public string Store_CycleTimeNew(string devicename, string filename, string path)
        {
            string dt_check = "";
            try
            {
                String result1 = "";
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                DataTable details = new DataTable();
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);

                if (result1 == "")
                {
                    return "File is empty or File Not found in the given path ";
                }
                else
                {
                    int num = 0;
                    for (num = 0; num < 1; num++)
                    {


                        // result = result.Replace("\\", "");
                        string final = Regex.Replace(allTextLines[num], @"[^\w\d\s]", "");

                        if (String.Equals(final.ToString(), "message"))
                        {
                            return "Timeout while reaching device . Device Unreachable.Data not inserted";
                        }
                    }
                }

                //var entries = allTextLines[0].Split(new string[] { "," }, StringSplitOptions.None);
                var i = 0;
                //for proper insertion
                DataTable dt = new DataTable();


                dt.Columns.Add("Date");
                dt.Columns.Add("MachineCode");
                dt.Columns.Add("VariantCode");
                dt.Columns.Add("Operations");
                dt.Columns.Add(new DataColumn("Operation_time", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Actual_cycletime", typeof(decimal)));
                dt.Columns.Add("OKParts");
                dt.Columns.Add("NOKParts");
                dt.Columns.Add("TotalParts");
                dt.Columns.Add("Type");
                dt.Columns.Add("Shift");
                dt.Columns.Add("CompanyCode");
                dt.Columns.Add("PlantCode");
                dt.Columns.Add("LineCode");
                dt.Columns.Add(new DataColumn("Time_Stamp", typeof(DateTime)));

                // datatable column mapping 
                //var entries1 = allTextLines[0].Split(new string[] { ",", "\r" }, StringSplitOptions.None);

                //for (int i1 = 0; i1 < entries1.Length; i1++)
                //{
                //    dt.Columns.Add(new DataColumn(entries1[i1].Replace("\r\n", "")).ToString().Trim());
                //}



                //for improper date
                DataTable dt_temp = new DataTable();

                dt_temp.Columns.Add("Date");
                dt_temp.Columns.Add("MachineCode");
                dt_temp.Columns.Add("VariantCode");
                dt_temp.Columns.Add("Operations");
                dt_temp.Columns.Add(new DataColumn("Operation_time", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Actual_cycletime", typeof(decimal)));
                dt_temp.Columns.Add("OKParts");
                dt_temp.Columns.Add("NOKParts");
                dt_temp.Columns.Add("TotalParts");
                dt_temp.Columns.Add("Type");
                dt_temp.Columns.Add("Shift");
                dt_temp.Columns.Add("CompanyCode");
                dt_temp.Columns.Add("PlantCode");
                dt_temp.Columns.Add("LineCode");
                dt_temp.Columns.Add("Time_Stamp");

                //for (int i1 = 0; i1 < entries1.Length; i1++)
                //{
                //    dt_temp.Columns.Add(new DataColumn(entries1[i1].Replace("\r\n", "")).ToString().Trim());
                //}

                for (i = 1; i < (allTextLines.Length); i++)
                {

                    DataRow dc = dt.NewRow();
                    DataRow dc_temp = dt_temp.NewRow();
                    DateTime dateValue;
                    var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                    if ((DateTime.TryParse(entries[14].ToString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dateValue)))
                    {

                        dc["Date"] = entries[0].ToString();
                        dc["MachineCode"] = entries[1];
                        dc["VariantCode"] = entries[2];
                        dc["Operations"] = entries[3];
                        dc["Operation_time"] = entries[4];
                        dc["Actual_cycletime"] = entries[5];
                        dc["OKParts"] = entries[6];
                        dc["NOKParts"] = entries[7];
                        dc["TotalParts"] = entries[8];
                        dc["Type"] = entries[9];
                        dc["Shift"] = entries[10];
                        dc["CompanyCode"] = entries[11];
                        dc["PlantCode"] = entries[12];
                        dc["LineCode"] = entries[13];
                        //dc["Time_Stamp"] = entries[14].Replace("T", " ").ToString();
                        dc["Time_Stamp"] = Convert.ToDateTime(entries[14].ToString());
                        dt.Rows.Add(dc);
                        //for (int i2 = 0; i2 < entries.Length; i2++)
                        //{
                        //    var temp = entries1[i2].Replace("\r\n", "").ToString().Trim();
                        //    Int32 temp1 = Convert.ToInt32(entries[i2]);
                        //    dc[entries1[i2].Replace("\r\n", "").ToString().Trim()] = Convert.ToInt32(entries[i2]);

                        //}

                        //dt.Rows.Add(dc);

                    }

                    else
                    {

                        dc_temp["Date"] = entries[0].ToString();
                        dc_temp["MachineCode"] = entries[1];
                        dc_temp["VariantCode"] = entries[2];
                        dc_temp["Operations"] = entries[3];
                        dc_temp["Operation_time"] = entries[4];
                        dc_temp["Actual_cycletime"] = entries[5];
                        dc_temp["OKParts"] = entries[6];
                        dc_temp["NOKParts"] = entries[7];
                        dc_temp["TotalParts"] = entries[8];
                        dc_temp["Type"] = entries[9];
                        dc_temp["Shift"] = entries[10];
                        dc_temp["CompanyCode"] = entries[11];
                        dc_temp["PlantCode"] = entries[12];
                        dc_temp["LineCode"] = entries[13];
                        dc_temp["Time_Stamp"] = entries[14];
                        //dc_temp["Time_Stamp"] = entries[14].ToString();
                        dt_temp.Rows.Add(dc_temp);
                        //for (int i2 = 0; i2 < entries.Length; i2++)
                        //{
                        //    dc_temp[entries1[i2].ToString()] = Convert.ToInt32(entries[i2]);
                        //}
                        //dt_temp.Rows.Add(dc_temp);
                    }
                    //dt_check = entries[0].ToString();



                    //i++;
                }
                var entries_connection = allTextLines[1].Split(new string[] { "," }, StringSplitOptions.None);
                database_connectionController d = new database_connectionController();
                string con_string = d.Getconnectionstring(entries_connection[11].ToString(), entries_connection[12].ToString(), entries_connection[13].ToString());
                if (con_string == "0")
                {
                    return "Couldnot connect to database";
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        //sql insetion for proper date format
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Raw_Cycletime";
                                sqlBulk.BatchSize = dt.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();




                                // for dynamic column mappings if column name is in csv
                                string[] a = { };
                                for (i = 0; i < 1; i++)
                                {
                                    a = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);


                                }
                                for (i = 0; i < ((a.Length)); i++)
                                {

                                    sqlBulk.ColumnMappings.Add((a[i].Replace("\r\n", "")).Trim(), (a[i].Replace("\r\n", "")).Trim());
                                }
                                try
                                {
                                    sqlBulk.WriteToServer(dt);
                                    dt.Dispose();
                                    statusinsert = "Inserted into Raw_Cycletime";
                                    inserted = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();
                                conn.Close();
                            }

                        }

                    }

                    //sql insetion for improper date format
                    if (dt_temp.Rows.Count > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Raw_Cycletime_Date";
                                sqlBulk.BatchSize = dt_temp.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;


                                sqlBulk.ColumnMappings.Clear();


                                string[] a = { };
                                for (i = 0; i < 1; i++)
                                {
                                    a = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);


                                }
                                for (i = 0; i < ((a.Length)); i++)
                                {

                                    sqlBulk.ColumnMappings.Add(a[i], a[i]);
                                }

                                try
                                {
                                    sqlBulk.WriteToServer(dt_temp);
                                    dt_temp.Dispose();
                                    statusinsert1 = "Inserted into Raw_Cycletime_Date";
                                    inserted1 = true;
                                }
                                catch (Exception e)
                                {
                                    return e.ToString();
                                }
                                //sqlBulk.WriteToServer(dt);
                                //dt.Dispose();
                                conn.Close();
                            }

                        }

                    }
                }



                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                string ex = dt_check + e.ToString();
                return ex;
            }

            return "false";
        }



        [Route("api/Values/Store_Parameter")]
        [HttpPost]
        public string Store_Parameter(string devicename, string filename, string path)
        {
            try
            {
                String result1 = "";
                string statusinsert = "Details Not Inserted in original table";
                string statusinsert1 = "Details Not Inserted in _date table";
                bool inserted = false;
                bool inserted1 = false;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                DataTable details = new DataTable();

                using (SqlConnection conn = new SqlConnection(this.constring))
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "Select distinct t2maccount,t2musername,t2mpassword,t2mdeveloperid,t2mdeviceusername,t2mdevicepassword,devicename,deviceip from tbl_Ewon_details where device_id='" + devicename + "' and status='Active'";
                    cmd.CommandTimeout = 15;
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(details);
                    conn.Close();

                }

                // to get file contents using ftp
                WebClient request = new WebClient();
                string url = "ftp://" + details.Rows[0][7] + "/" + path + "/" + filename;
                string username = details.Rows[0][4].ToString();
                request.Credentials = new NetworkCredential(details.Rows[0][4].ToString(), Base64Decode(Base64Decode(details.Rows[0][5].ToString())));

                request.Proxy = null;

                byte[] newFileData = request.DownloadData(url);
                result1 = System.Text.Encoding.UTF8.GetString(newFileData);

                var allTextLines = result1.Split(new string[] { "\n" }, StringSplitOptions.None);

                var i = 0;
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("TimeStamp", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                dt.Columns.Add("Shift_ID");
                dt.Columns.Add("Line_code");
                dt.Columns.Add("Machine_code");
                dt.Columns.Add("Variant_code");
                dt.Columns.Add("Batch_no");
                dt.Columns.Add(new DataColumn("Part_Counter", typeof(int)));
                dt.Columns.Add(new DataColumn("OK_Part", typeof(int)));
                dt.Columns.Add(new DataColumn("NOK_Part", typeof(int)));
                dt.Columns.Add("ParameterName");
                dt.Columns.Add(new DataColumn("Min_Value", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Max_Value", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Part1", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Part1_status", typeof(int)));
                dt.Columns.Add(new DataColumn("Part2", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Part2_status", typeof(int)));
                dt.Columns.Add(new DataColumn("Part3", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Part3_status", typeof(int)));
                dt.Columns.Add(new DataColumn("Part4", typeof(decimal)));
                dt.Columns.Add(new DataColumn("Part4_status", typeof(int)));
                dt.Columns.Add("Companycode");
                dt.Columns.Add("PlantCode");


                //for improper date
                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add("TimeStamp");
                dt_temp.Columns.Add("Date");
                dt_temp.Columns.Add("Shift_ID");
                dt_temp.Columns.Add("Line_code");
                dt_temp.Columns.Add("Machine_code");
                dt_temp.Columns.Add("Variant_code");
                dt_temp.Columns.Add("Batch_no");
                dt_temp.Columns.Add(new DataColumn("Part_Counter", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("OK_Part", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("NOK_Part", typeof(int)));
                dt_temp.Columns.Add("ParameterName");
                dt_temp.Columns.Add(new DataColumn("Min_Value", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Max_Value", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Part1", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Part1_status", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Part2", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Part2_status", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Part3", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Part3_status", typeof(int)));
                dt_temp.Columns.Add(new DataColumn("Part4", typeof(decimal)));
                dt_temp.Columns.Add(new DataColumn("Part4_status", typeof(int)));
                dt_temp.Columns.Add("Companycode");
                dt_temp.Columns.Add("PlantCode");

                for (i = 0; i < (allTextLines.Length); i++)
                {
                    DataRow dc = dt.NewRow();
                    DataRow dc_temp = dt_temp.NewRow();
                    DateTime dateValue;
                    var entries = allTextLines[i].Split(new string[] { "," }, StringSplitOptions.None);
                    if (DateTime.TryParse(entries[0].ToString(),
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out dateValue) && (DateTime.TryParse(entries[1].ToString(),
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out dateValue)))
                    {
                        dc["TimeStamp"] = Convert.ToDateTime(entries[0].ToString());
                        dc["Date"] = Convert.ToDateTime(entries[1].ToString());
                        dc["Shift_ID"] = entries[2];
                        dc["Line_code"] = entries[3];
                        dc["Machine_code"] = entries[4];
                        dc["Variant_code"] = entries[5];
                        dc["Batch_no"] = entries[6];
                        dc["Part_Counter"] = Convert.ToInt32(entries[7]);
                        dc["OK_Part"] = Convert.ToInt32(entries[8]);
                        dc["NOK_Part"] = Convert.ToInt32(entries[9]);
                        dc["ParameterName"] = entries[10];
                        dc["Min_Value"] = Convert.ToDecimal(entries[11].ToString());
                        dc["Max_Value"] = Convert.ToDecimal(entries[12].ToString());
                        dc["Part1"] = Convert.ToDecimal(entries[13].ToString());
                        dc["Part1_status"] = Convert.ToInt32(entries[14].ToString());
                        dc["Part2"] = Convert.ToDecimal(entries[15].ToString());
                        dc["Part2_status"] = Convert.ToInt32(entries[16].ToString());
                        dc["Part3"] = Convert.ToDecimal(entries[17].ToString());
                        dc["Part3_status"] = Convert.ToInt32(entries[18].ToString());
                        dc["Part4"] = Convert.ToDecimal(entries[19].ToString());
                        dc["Part4_status"] = Convert.ToInt32(entries[20].ToString());
                        dc["Companycode"] = entries[21];
                        dc["PlantCode"] = entries[22];

                        dt.Rows.Add(dc);
                    }
                    else
                    {
                        dc_temp["TimeStamp"] = entries[0];
                        dc_temp["Date"] = entries[1];
                        dc_temp["Shift_ID"] = entries[2];
                        dc_temp["Line_code"] = entries[3];
                        dc_temp["Machine_code"] = entries[4];
                        dc_temp["Variant_code"] = entries[5];
                        dc_temp["Batch_no"] = entries[6];
                        dc_temp["Part_Counter"] = Convert.ToInt32(entries[7]);
                        dc_temp["OK_Part"] = Convert.ToInt32(entries[8]);
                        dc_temp["NOK_Part"] = Convert.ToInt32(entries[9]);
                        dc_temp["ParameterName"] = entries[10];
                        dc_temp["Min_Value"] = Convert.ToDecimal(entries[11].ToString());
                        dc_temp["Max_Value"] = Convert.ToDecimal(entries[12].ToString());
                        dc_temp["Part1"] = Convert.ToDecimal(entries[13].ToString());
                        dc_temp["Part1_status"] = Convert.ToInt32(entries[14].ToString());
                        dc_temp["Part2"] = Convert.ToDecimal(entries[15].ToString());
                        dc_temp["Part2_status"] = Convert.ToInt32(entries[16].ToString());
                        dc_temp["Part3"] = Convert.ToDecimal(entries[17].ToString());
                        dc_temp["Part3_status"] = Convert.ToInt32(entries[18].ToString());
                        dc_temp["Part4"] = Convert.ToDecimal(entries[19].ToString());
                        dc_temp["Part4_status"] = Convert.ToInt32(entries[20].ToString());
                        dc_temp["Companycode"] = entries[21];
                        dc_temp["PlantCode"] = entries[22];
                        dt_temp.Rows.Add(dc_temp);

                    }



                    //i++;
                }
                var entries_connection = allTextLines[1].Split(new string[] { "," }, StringSplitOptions.None);
                database_connectionController d = new database_connectionController();
                string con_string = d.Getconnectionstring(entries_connection[21].ToString(), entries_connection[22].ToString(), entries_connection[3].ToString());
                if (con_string == "0")
                {
                    return "Couldnot connect to database";
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Tbl_Raw_Parameters";
                                sqlBulk.BatchSize = dt.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;



                                //for static column mappings
                                sqlBulk.ColumnMappings.Add("TimeStamp", "TimeStamp");
                                sqlBulk.ColumnMappings.Add("Date", "Date");
                                sqlBulk.ColumnMappings.Add("Shift_ID", "Shift_ID");
                                sqlBulk.ColumnMappings.Add("Line_code", "Line_code");
                                sqlBulk.ColumnMappings.Add("Machine_code", "Machine_code");
                                sqlBulk.ColumnMappings.Add("Variant_code", "Variant_code");
                                sqlBulk.ColumnMappings.Add("Batch_no", "Batch_no");
                                sqlBulk.ColumnMappings.Add("Part_Counter", "Part_Counter");
                                sqlBulk.ColumnMappings.Add("OK_Part", "OK_Part");
                                sqlBulk.ColumnMappings.Add("NOK_Part", "NOK_Part");
                                sqlBulk.ColumnMappings.Add("ParameterName", "ParameterName");
                                sqlBulk.ColumnMappings.Add("Min_Value", "Min_Value");
                                sqlBulk.ColumnMappings.Add("Max_Value", "Max_Value");
                                sqlBulk.ColumnMappings.Add("Part1", "Part1");
                                sqlBulk.ColumnMappings.Add("Part1_status", "Part1_status");
                                sqlBulk.ColumnMappings.Add("Part2", "Part2");
                                sqlBulk.ColumnMappings.Add("Part2_status", "Part2_status");
                                sqlBulk.ColumnMappings.Add("Part3", "Part3");
                                sqlBulk.ColumnMappings.Add("Part3_status", "Part3_status");
                                sqlBulk.ColumnMappings.Add("Part4", "Part4");
                                sqlBulk.ColumnMappings.Add("Part4_status", "Part4_status");
                                sqlBulk.ColumnMappings.Add("Companycode", "Companycode");
                                sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");

                                sqlBulk.WriteToServer(dt);
                                dt.Dispose();
                                statusinsert = "Inserted into Tbl_Raw_Parameters";
                                inserted = true;
                                conn.Close();
                            }

                        }

                    }


                    if (dt_temp.Rows.Count > 0)
                    {
                        using (SqlConnection conn = new SqlConnection(con_string))
                        {
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con_string, SqlBulkCopyOptions.FireTriggers))
                            {
                                conn.Open();
                                sqlBulk.DestinationTableName = "Tbl_Raw_Parameters_Date";
                                sqlBulk.BatchSize = dt_temp.Rows.Count;
                                sqlBulk.BulkCopyTimeout = 6000000;



                                //for static column mappings
                                sqlBulk.ColumnMappings.Add("TimeStamp", "TimeStamp");
                                sqlBulk.ColumnMappings.Add("Date", "Date");
                                sqlBulk.ColumnMappings.Add("Shift_ID", "Shift_ID");
                                sqlBulk.ColumnMappings.Add("Line_code", "Line_code");
                                sqlBulk.ColumnMappings.Add("Machine_code", "Machine_code");
                                sqlBulk.ColumnMappings.Add("Variant_code", "Variant_code");
                                sqlBulk.ColumnMappings.Add("Batch_no", "Batch_no");
                                sqlBulk.ColumnMappings.Add("Part_Counter", "Part_Counter");
                                sqlBulk.ColumnMappings.Add("OK_Part", "OK_Part");
                                sqlBulk.ColumnMappings.Add("NOK_Part", "NOK_Part");
                                sqlBulk.ColumnMappings.Add("ParameterName", "ParameterName");
                                sqlBulk.ColumnMappings.Add("Min_Value", "Min_Value");
                                sqlBulk.ColumnMappings.Add("Max_Value", "Max_Value");
                                sqlBulk.ColumnMappings.Add("Part1", "Part1");
                                sqlBulk.ColumnMappings.Add("Part1_status", "Part1_status");
                                sqlBulk.ColumnMappings.Add("Part2", "Part2");
                                sqlBulk.ColumnMappings.Add("Part2_status", "Part2_status");
                                sqlBulk.ColumnMappings.Add("Part3", "Part3");
                                sqlBulk.ColumnMappings.Add("Part3_status", "Part3_status");
                                sqlBulk.ColumnMappings.Add("Part4", "Part4");
                                sqlBulk.ColumnMappings.Add("Part4_status", "Part4_status");
                                sqlBulk.ColumnMappings.Add("Companycode", "Companycode");
                                sqlBulk.ColumnMappings.Add("PlantCode", "PlantCode");

                                sqlBulk.WriteToServer(dt_temp);
                                dt_temp.Dispose();
                                statusinsert1 = "Inserted into Tbl_Raw_Parameters_Date";
                                inserted1 = true;
                                conn.Close();
                            }

                        }

                    }
                } 


                //return statusinsert +", "+ statusinsert1;
                string rtrnstatement = "False";
                if (inserted || inserted1)
                {
                    rtrnstatement = "True";
                }
                return rtrnstatement;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "false";
        }


        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

    }
}
