/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2017 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using DisplayMonkey.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DisplayMonkey
{
    public class Encryptor
    {
        private static Guid[] _keys = {
            new Guid("9C2F6600-1DC5-4F7B-9CDE-AF6C3FDC073B"),   // method
            new Guid("8CA22FD4-2A2B-47D1-87A3-2B3E277E9DED"),   // IV
            new Guid("538D9121-5698-42D5-BA32-CD4548A100B3"),   // Key
        };

        private static string _sql = "SELECT TOP 1 * FROM Settings WHERE [Key]=@k;";
        
        public static IEncryptor Current 
        {
            get 
            {
                IEncryptor encryptor = null;
                
                using (SqlCommand cmd = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandText = _sql,
                })
                {
                    EncryptionMethods method = EncryptionMethods.RsaContainer;

                    // get method first
                    cmd.Parameters.AddWithValue("@k", _keys[0]);
                    cmd.ExecuteReaderExt((dr) =>
                    {
                        byte [] v = dr.BytesOrNull("Value");
                        method = (EncryptionMethods)(v == null ? 0 : BitConverter.ToInt32(v.Reverse().ToArray(), 0));
                        return false;
                    });

                    switch (method)
                    {
                        case EncryptionMethods.RsaContainer:
                            encryptor = new RsaEncryptor();
                            break;

                        case EncryptionMethods.Aes:
                            encryptor = new AesEncryptor();

                            // IV
                            cmd.Parameters[0].Value = _keys[1];
                            cmd.ExecuteReaderExt((dr) =>
                            {
                                (encryptor as AesEncryptor).IV = dr.BytesOrNull("Value");
                                return false;
                            });

                            // Key
                            cmd.Parameters[0].Value = _keys[2];
                            cmd.ExecuteReaderExt((dr) =>
                            {
                                (encryptor as AesEncryptor).Key = dr.BytesOrNull("Value");
                                return false;
                            });

                            break;
                    }
                }

                return encryptor; 
            }
        }
    }
}