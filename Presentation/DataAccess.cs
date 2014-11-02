using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Globalization;
using System.Configuration;

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
				cmd.Connection = DataAccess.Connection;
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
				cmd.Connection = DataAccess.Connection;
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
			using (SqlCommand cmd = new SqlCommand(sql))
			{
				return RunSql(cmd);
			}
		}

		public static bool Boolean(object o)
		{
			bool ret = false;
			try
			{
				if (o != DBNull.Value)
					ret = Convert.ToBoolean(o);
			}
			catch (FormatException)
			{
			}
			return ret;
		}

		public static int IntOrZero(object o)
		{
			int ret = 0;
			try
			{
				if (o != DBNull.Value)
					ret = Convert.ToInt32(o);
			}
			catch (FormatException)
			{
			}
			return ret;
		}

		public static double DoubleOrZero(object o)
		{
			double ret = 0;
			try
			{
				if (o != DBNull.Value)
					ret = Convert.ToDouble(o, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
			}
			return ret;
		}

		public static string StringOrBlank(object o)
		{
			string ret = "";
			try
			{
				if (o != DBNull.Value)
					ret = Convert.ToString(o);
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
                return (
                    new SqlConnectionStringBuilder(
                        ConfigurationManager.ConnectionStrings["DisplayMonkeyDB"].ConnectionString
                    ) { ApplicationName = "DisplayMonkey Presentation Services" }
                ).ConnectionString
                ;
			}
		}

		private static Mutex _mutex = new Mutex(false);
		private static SqlConnection _cnn = null;

		#endregion // Private Members ////////////////////////////////////////
    }
}