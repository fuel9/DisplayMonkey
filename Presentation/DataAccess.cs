/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

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
    public static class DataAccess
    {
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

        public static bool BooleanOrDefault(object o, bool _default)
        {
            bool ret = _default;
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

        public static bool Boolean(object o)
        {
            return BooleanOrDefault(o, false);
        }

        public static bool BooleanOrDefault(this SqlParameter param, bool _default)
        {
            return BooleanOrDefault(param.Value, _default);
        }

        public static bool Boolean(this SqlParameter param)
        {
            return Boolean(param.Value);
        }

        public static bool BooleanOrDefault(this DataRow row, string column, bool _default)
        {
            return BooleanOrDefault(row[column], _default);
        }

        public static bool Boolean(this DataRow row, string column)
        {
            return Boolean(row[column]);
        }

        public static int IntOrDefault(object o, int _default)
        {
            int ret = _default;
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

        public static int IntOrZero(object o)
        {
            return IntOrDefault(o, 0);
        }

        public static int IntOrDefault(this SqlParameter param, int _default)
        {
            return IntOrDefault(param.Value, _default);
        }

        public static int IntOrZero(this SqlParameter param)
        {
            return IntOrZero(param.Value);
        }

        public static int IntOrDefault(this DataRow row, string column, int _default)
        {
            return IntOrDefault(row[column], _default);
        }

        public static int IntOrZero(this DataRow row, string column)
        {
            return IntOrZero(row[column]);
        }

        public static double DoubleOrDefault(object o, double _default)
        {
            double ret = _default;
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

        public static double DoubleOrZero(object o)
        {
            return DoubleOrDefault(o, 0);
        }

        public static double DoubleOrDefault(this SqlParameter param, double _default)
        {
            return DoubleOrDefault(param.Value, _default);
        }

        public static double DoubleOrZero(this SqlParameter param)
        {
            return DoubleOrZero(param.Value);
        }

        public static double DoubleOrDefault(this DataRow row, string column, double _default)
        {
            return DoubleOrDefault(row[column], _default);
        }

        public static double DoubleOrZero(this DataRow row, string column)
        {
            return DoubleOrZero(row[column]);
        }

        public static string StringOrDefault(object o, string _default)
        {
            string ret = _default;
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

        public static string StringOrBlank(object o)
        {
            return StringOrDefault(o, "");
        }

        public static string StringOrBlank(this SqlParameter param)
        {
            return StringOrBlank(param.Value);
        }

        public static string StringOrDefault(this SqlParameter param, string _default)
        {
            return StringOrDefault(param.Value, _default);
        }

        public static string StringOrBlank(this DataRow row, string column)
        {
            return StringOrBlank(row[column]);
        }

        public static string StringOrDefault(this DataRow row, string column, string _default)
        {
            return StringOrDefault(row[column], _default);
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