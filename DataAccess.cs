using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace DisplayMonkey
{
    /// <summary>
    /// Summary description for DataAccess
    /// </summary>
    public class DataAccess
    {
		public static void ExecuteNonQuery(SqlCommand cmd)
		{
			try
			{
				_mutex.WaitOne();
				cmd.ExecuteNonQuery();
			}

			finally
			{
				_mutex.ReleaseMutex();
			}
		}

		public static DataSet RunSql(SqlCommand cmd)
		{
			DataSet ds = new DataSet();
			
			try
			{
				_mutex.WaitOne();
				new SqlDataAdapter(cmd).Fill(ds);
			}

			finally
			{
				_mutex.ReleaseMutex();
			}

			return ds;
		}

		public static DataSet RunSql(string sql)
		{
			using (SqlCommand cmd = new SqlCommand(sql, Connection))
			{
				return RunSql(cmd);
			}
		}

		public static int IntOrZero(object integer)
		{
			int ret = 0;
			try
			{
				if (integer != DBNull.Value)
					ret = Convert.ToInt32(integer);
			}
			catch (FormatException)
			{
			}
			return ret;
		}

		public static string StringOrBlank(object str)
		{
			string ret = "";
			try
			{
				if (str != DBNull.Value)
					ret = Convert.ToString(str);
			}
			catch (FormatException)
			{
			}
			return ret;
		}

		public static SqlConnection Connection
		{
			get
			{
				if (_cnn == null)
					_cnn = new SqlConnection(ConnectionString);
				if (_cnn.State == ConnectionState.Broken || _cnn.State == ConnectionState.Closed)
					_cnn.Open();
				return _cnn;
			}
		}

		#region Private Members //////////////////////////////////////////////

		private static string ConnectionString
		{
			get
			{
				// TODO: put to app settings
				return string.Format(
					"Data Source={0};User ID={1};Password={2};Persist Security Info=True;Initial Catalog={3};",
					Properties.Settings.Default.db_server,
					Properties.Settings.Default.db_user,
					Properties.Settings.Default.db_password,
					Properties.Settings.Default.db_catalog
					);
			}
		}

		private static Mutex _mutex = new Mutex(false);
		private static SqlConnection _cnn = null;

		#endregion // Private Members ////////////////////////////////////////

		//public string registerDisplay(string DisplayAddress, string DisplayName, string DisplayType, string WhoCreated)
		//{
		//    try
		//    {
		//        using (SqlConnection conn = new SqlConnection(connectionString))
		//        {
		//            string sqlCmd = string.Format("INSERT INTO Registered_Displays (Display_Name, IP_Address, Display_Type, Who_Created)  SELECT '{0}', '{1}', '{2}', '{3}' ", DisplayName, DisplayAddress, DisplayType, WhoCreated);

		//            conn.Open();
		//            using (SqlCommand sqlCMD = new SqlCommand(sqlCmd, conn))
		//            {
		//                sqlCMD.CommandType = System.Data.CommandType.Text;
		//                sqlCMD.ExecuteNonQuery();
		//            }
		//            conn.Close();
		//        }
		//    }
		//    catch (SqlException)
		//    {
		//    }

		//    return getDisplayType(DisplayAddress);
		//}

		//public string getRegisteredDisplaysHTMLLink()
		//{
		//    string htmOut = "";

		//    try
		//    {
		//        using (SqlConnection conn = new SqlConnection(connectionString))
		//        {
		//            string sqlCmd = string.Format("SELECT Display_Name, URL_Link FROM Registered_Display_URL");

		//            conn.Open();
		//            using (SqlCommand sqlCMD = new SqlCommand(sqlCmd, conn))
		//            {
		//                int cnt = 0;
		//                sqlCMD.CommandType = System.Data.CommandType.Text;
		//                SqlDataReader dr = sqlCMD.ExecuteReader();
		//                while (dr.Read())
		//                {
		//                    if (dr["URL_Link"] != System.DBNull.Value)
		//                        htmOut += (string)dr["URL_Link"];
		//                    cnt++;
		//                    if ((cnt % 4) != 0)
		//                    {
		//                        htmOut += "&nbsp;&nbsp;|&nbsp;&nbsp;";
		//                    }
		//                    else
		//                    {
		//                        htmOut += "<br>";
		//                    }
		//                }
		//            }
		//            conn.Close();
		//        }
		//    }
		//    catch (SqlException)
		//    {
		//    }
		//    return htmOut;
		//}

		//public string getNextContentItemData(string DisplayAddress, int LastContentId, int Quadrant)
		//{
		//    return getNextContentItemData(DisplayAddress, LastContentId, Quadrant, 0);
		//}

		//public void getContentDataType1n4(int ContentId, ref string String1, ref string String2, ref string String3, ref string String4)
		//{
		//    try
		//    {
		//        using (SqlConnection conn = new SqlConnection(connectionString))
		//        {
		//            string sqlCmd = string.Format("SELECT String1, String2, String3, String4 FROM Content_Items WHERE Content_Id={0}", ContentId);

		//            conn.Open();
		//            using (SqlCommand sqlCMD = new SqlCommand(sqlCmd, conn))
		//            {
		//                sqlCMD.CommandType = System.Data.CommandType.Text;
		//                SqlDataReader dr = sqlCMD.ExecuteReader();
		//                if (dr.Read())
		//                {
		//                    if (dr["String1"] != System.DBNull.Value)
		//                        String1 = (string)dr["String1"];
		//                    if (dr["String2"] != System.DBNull.Value)
		//                        String2 = (string)dr["String2"];
		//                    if (dr["String3"] != System.DBNull.Value)
		//                        String3 = (string)dr["String3"];
		//                    if (dr["String4"] != System.DBNull.Value)
		//                        String4 = (string)dr["String4"];
		//                }
		//            }
		//            conn.Close();
		//        }
		//    }
		//    catch (SqlException)
		//    {
		//    }
		//}

		//public void getContentDataType2n5(int ContentId, ref object Image1)
		//{
		//    try
		//    {
		//        using (SqlConnection conn = new SqlConnection(connectionString))
		//        {
		//            string sqlCmd = string.Format("SELECT Image1 FROM Content_Items WHERE Content_Id={0}", ContentId);

		//            conn.Open();
		//            using (SqlCommand sqlCMD = new SqlCommand(sqlCmd, conn))
		//            {
		//                sqlCMD.CommandType = System.Data.CommandType.Text;
		//                SqlDataReader dr = sqlCMD.ExecuteReader();
		//                if (dr.Read())
		//                {
		//                    if (dr["Image1"] != System.DBNull.Value)
		//                        Image1 = dr["Image1"];
		//                }
		//            }
		//            conn.Close();
		//        }
		//    }
		//    catch (SqlException)
		//    {
		//    }
		//}
    }
}