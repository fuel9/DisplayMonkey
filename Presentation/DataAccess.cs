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
        #region Resource helper extensions  //////////////////////////////////////////////

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

        public static string StringResource(string key)
        {
            return ResourceManager.GetString(key);
        }

        #endregion

        #region SqlCommand helper extensions  //////////////////////////////////////////////

        private static void ConnectionWrap(SqlCommand cmd, Action action)
        {
            if (cmd.Connection == null)
            {
                using (cmd.Connection = new SqlConnection(ConnectionString))
                {
                    cmd.Connection.Open();
                    action();
                }
            }
            else
            {
                if (cmd.Connection.State == ConnectionState.Broken || cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.ConnectionString = ConnectionString;
                    cmd.Connection.Open();
                }

                action();
            }
        }

        private static async Task ConnectionWrapAsync(SqlCommand cmd, Func<Task> funcAsync)
        {
            if (cmd.Connection == null)
            {
                using (cmd.Connection = new SqlConnection(ConnectionString))
                {
                    await cmd.Connection.OpenAsync();
                    await funcAsync();
                }
            }
            else
            {
                if (cmd.Connection.State == ConnectionState.Broken || cmd.Connection.State == ConnectionState.Closed)
                    await cmd.Connection.OpenAsync();

                await funcAsync();
            }
        }

        public static void ExecuteNonQueryExt(this SqlCommand cmd)
        {
            ConnectionWrap(cmd, () => cmd.ExecuteNonQuery());
        }

        public static void ExecuteReaderExt(this SqlCommand cmd, Func<SqlDataReader, bool> callback)
        {
            ConnectionWrap(cmd, () =>
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read() && callback(reader)) ;
                }
            });
        }

        public static async Task ExecuteNonQueryExtAsync(this SqlCommand cmd)
        {
            await ConnectionWrapAsync(cmd, async () => await cmd.ExecuteNonQueryAsync());
        }

        public static async Task ExecuteReaderExtAsync(this SqlCommand cmd, Func<SqlDataReader, bool> callback)
        {
            await ConnectionWrapAsync(cmd, async () =>
            {
                using (SqlDataReader reader = await Task<SqlDataReader>.Factory.FromAsync(cmd.BeginExecuteReader, cmd.EndExecuteReader, null))
                {
                    while (await reader.ReadAsync() && callback(reader)) ;
                }
            });
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

        #endregion

        #region Value helper extensions  //////////////////////////////////////////////

        private static T DbValueOrDefault<T>(object o, T _default)
        {
            var t = o;
            if (t == DBNull.Value)
                return _default;
            return (T)t;
        }

        private static T DbValueOrNull<T>(object o) where T : class
        {
            return DbValueOrDefault<T>(o, default(T));
        }

        private static T? DbAsNullable<T>(object o) where T : struct
        {
            var t = o;
            if (t == DBNull.Value) return (T?)null;
            return new Nullable<T>((T)t);
        }

        public static T ValueOrDefault<T>(this SqlDataReader reader, string column, T _default)
        {
            return DbValueOrDefault<T>(reader[column], _default);
        }

        public static T ValueOrNull<T>(this SqlDataReader reader, string column) where T : class
        {
            return DbValueOrDefault<T>(reader[column], default(T));
        }

        public static T? AsNullable<T>(this SqlDataReader reader, string column) where T : struct
        {
            return DbAsNullable<T>(reader[column]);
        }

        public static string StringOrBlank(this SqlDataReader reader, string column)
        {
            return DbValueOrDefault<string>(reader[column], "");
        }

        public static string StringOrDefault(this SqlDataReader reader, string column, string _default)
        {
            return DbValueOrDefault<string>(reader[column], _default);
        }

        public static int IntOrZero(this SqlDataReader reader, string column)
        {
            return DbValueOrDefault<int>(reader[column], 0);
        }

        public static int IntOrDefault(this SqlDataReader reader, string column, int _default)
        {
            return DbValueOrDefault<int>(reader[column], _default);
        }

        public static bool Boolean(this SqlDataReader reader, string column)
        {
            return DbValueOrDefault<bool>(reader[column], false);
        }

        public static double DoubleOrZero(this SqlDataReader reader, string column)
        {
            return DbValueOrDefault<double>(reader[column], 0);
        }

        public static byte[] BytesOrNull(this SqlDataReader reader, string column)
        {
            return DbValueOrNull<byte[]>(reader[column]);
        }

        public static int IntOrZero(this SqlParameter param)
        {
            return DbValueOrDefault<int>(param.Value, 0);
        }

        public static int IntOrZero(this HttpRequest request, string key)
        {
            int i = 0;
            Int32.TryParse(request.QueryString[key], out i);
            return i;
        }

        public static string StringOrBlank(this HttpRequest request, string key)
        {
            return request.QueryString[key] ?? "";
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
