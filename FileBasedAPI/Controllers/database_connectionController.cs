using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileBasedAPI.Controllers
{
    public class database_connectionController : Controller
    {
        private string connectionstring = ConfigurationManager.ConnectionStrings["ConnectTitan"].ToString();


        public string Getconnectionstring(string companycode, string plantcode, string linecode)
        {

            /*
             Trim end is used to trim the digits for MPs line, line will come as MPS1,MPS2,..
             
             */
            if (linecode == "MPS1" || linecode == "MPS2" || linecode == "MPS3" || linecode == "MPS4")
            {
                var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

                linecode = linecode.TrimEnd(digits);
            }



            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT distinct connection from database_connection where CompanyCode=@CompanyCode and PlantCode=@PlantCode and LineCode=@Line_Code", connection);


                command.Parameters.AddWithValue("@CompanyCode", companycode);
                command.Parameters.AddWithValue("@PlantCode", plantcode);
                command.Parameters.AddWithValue("@Line_Code", linecode);
                SqlDataAdapter da = new SqlDataAdapter(command);
                object ret = command.ExecuteScalar();
                if (ret != null)
                {
                    return (string)ret;
                }

                else
                {
                    return "0";
                }
            }


        }
    }
}