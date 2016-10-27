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
using System.Threading.Tasks;
using System.Data.SqlTypes;

namespace DisplayMonkey
{
    /// <summary>
    /// Summary description for DataAccess
    /// </summary>
    public static class DataAccess
    {
        private static System.Resources.ResourceManager _rm = null;

        public static System.Resources.ResourceManager ResourceManager 
        { 
            get 
            { 
                if (_rm == null)
                    _rm = new System.Resources.ResourceManager(typeof(DisplayMonkey.Language.Resources));
                return _rm;
            }
        }
        
        public static async Task ExecuteNonQueryAsync(this SqlCommand cmd)
        {
            if (cmd.Connection == null)
            {
                using (cmd.Connection = new SqlConnection(ConnectionString))
                {
                    await cmd.Connection.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else
            {
                if (cmd.Connection.State == ConnectionState.Broken || cmd.Connection.State == ConnectionState.Closed)
                    await cmd.Connection.OpenAsync();

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static void ExecuteReader(this SqlCommand cmd, Func<SqlDataReader, bool> callback)
        {
            if (cmd.Connection == null)
            {
                using (cmd.Connection = new SqlConnection(ConnectionString))
                {
                    cmd.Connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read() && callback(reader)) ;
                    }
                }
            }
            else
            {
                if (cmd.Connection.State == ConnectionState.Broken || cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read() && callback(reader)) ;
                }
            }
        }

        public static async Task ExecuteReaderAsync(this SqlCommand cmd, Func<SqlDataReader, bool> callback)
        {
            if (cmd.Connection == null)
            {
                using (cmd.Connection = new SqlConnection(ConnectionString))
                {
                    await cmd.Connection.OpenAsync();
                    using (SqlDataReader reader = await Task<SqlDataReader>.Factory.FromAsync(cmd.BeginExecuteReader, cmd.EndExecuteReader, null))
                    {
                        while (await reader.ReadAsync() && callback(reader)) ;
                    }
                }
            }
            else
            {
                if (cmd.Connection.State == ConnectionState.Broken || cmd.Connection.State == ConnectionState.Closed)
                    await cmd.Connection.OpenAsync();

                using (SqlDataReader reader = await Task<SqlDataReader>.Factory.FromAsync(cmd.BeginExecuteReader, cmd.EndExecuteReader, null))
                {
                    while (await reader.ReadAsync() && callback(reader)) ;
                }
            }
        }

        public static async Task ExecuteTransactionAsync(Func<SqlConnection,SqlTransaction,Task> batch)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                SqlTransaction transaction = null;

                try
                {
                    await cnn.OpenAsync();
                    transaction = cnn.BeginTransaction();
                    await batch(cnn, transaction);
                    transaction.Commit();
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        #region Value helper extensions  //////////////////////////////////////////////

        public static T ValueOrDefault<T>(this SqlDataReader r, string column, T _default)
        {
            var t = r[column];
            if (t == DBNull.Value) 
                return _default;
            return (T)t;
        }

        public static T ValueOrNull<T>(this SqlDataReader r, string column) where T : class
        {
            return ValueOrDefault<T>(r, column, default(T));
        }

        public static T? AsNullable<T>(this SqlDataReader r, string column) where T : struct
        {
            var t = r[column];
            if (t == DBNull.Value) return (T?)null;
            return new Nullable<T>((T)t);
        }

        public static string StringOrBlank(this SqlDataReader reader, string column)
        {
            return ValueOrDefault<string>(reader, column, "");
        }

        public static string StringOrDefault(this SqlDataReader reader, string column, string _default)
        {
            return ValueOrDefault<string>(reader, column, _default);
        }

        public static int IntOrZero(this SqlDataReader reader, string column)
        {
            return ValueOrDefault<int>(reader, column, 0);
        }

        public static int IntOrDefault(this SqlDataReader reader, string column, int _default)
        {
            return ValueOrDefault<int>(reader, column, _default);
        }

        public static bool Boolean(this SqlDataReader reader, string column)
        {
            return ValueOrDefault<bool>(reader, column, false);
        }

        public static double DoubleOrZero(this SqlDataReader reader, string column)
        {
            return ValueOrDefault<double>(reader, column, 0);
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

        public static int IntOrZero(this SqlParameter param)
        {
            return IntOrZero(param.Value);
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

        #endregion // Value helper extensions  //////////////////////////////////////////////

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

		#endregion // Private Members ////////////////////////////////////////
    }
}