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

using System.Security.Cryptography;
using System.Text;
using System.Security.AccessControl;
using System.IO;

namespace DisplayMonkey
{
    public interface IEncryptor
    {
        byte[] Encrypt(string _value);
        string Decrypt(byte[] _data);
    }

    public class RsaEncryptor : IEncryptor
    {
        #region Private Members

        private static CspParameters _container = null;
        private static CspParameters Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new CspParameters()
                    {
                        KeyContainerName = "DisplayMonkeyRsaKeyContainer",
                        Flags = CspProviderFlags.NoPrompt | 
                            CspProviderFlags.UseMachineKeyStore | 
                            CspProviderFlags.UseArchivableKey,
                        //CryptoKeySecurity = new CryptoKeySecurity(),
                    };
                    //CryptoKeyAccessRule rule = new CryptoKeyAccessRule("everyone", CryptoKeyRights.FullControl, AccessControlType.Allow);
                    //_container.CryptoKeySecurity.SetAccessRule(rule);
                }
                return _container;
            }
        }

        #endregion

        #region Public Helpers

        public byte[] Encrypt(string _value)
        {
            byte[] ret = null;

            if (_value == null)
            {
                return ret;
            }

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(Container))
            {
                ret = provider.Encrypt(
                    Encoding.UTF8.GetBytes(_value),
                    false
                    );
            }

            return ret;
        }

        public string Decrypt(byte[] _data)
        {
            byte[] buf = null;
            string ret = null;

            if (_data == null)
                return ret;

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(Container))
            {
                buf = provider.Decrypt(
                    _data,
                    false
                );

                if (buf != null)
                    ret = Encoding.UTF8.GetString(buf);
            }

            return ret;
        }

        #endregion
    }
    public class AesEncryptor : IEncryptor
    {
        #region Private Members

        private SymmetricAlgorithm _sa = new AesManaged() 
        { 
            Padding = PaddingMode.PKCS7, 
            //BlockSize = 128, 
            //KeySize = 256 
        };

        #endregion

        #region Public Helpers

        public byte[] Key
        {
            get
            {
                if (_sa.Key.Length == 0)
                {
                    _sa.GenerateKey();
                }

                byte[] x = new byte[_sa.Key.Length];
                _sa.Key.CopyTo(x, 0);

                return x;
            }

            set
            {
                byte[] x = new byte[value.Length];
                value.CopyTo(x, 0);
                _sa.Key = x;
            }
        }

        public byte[] IV
        {
            get
            {
                if (_sa.IV.Length == 0)
                {
                    _sa.GenerateIV();
                }

                byte[] x = new byte[_sa.IV.Length];
                _sa.IV.CopyTo(x, 0);

                return x;
            }

            set
            {
                byte[] x = new byte[value.Length];
                value.CopyTo(x, 0);
                _sa.IV = x;
            }
        }

        public byte[] Encrypt(string _value)
        {
            byte[] ret = null;

            if (_value == null)
            {
                return ret;
            }

            ICryptoTransform encryptor = _sa.CreateEncryptor(Key, IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(_value);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();
                ret = msEncrypt.ToArray();
            }

            return ret;
        }

        public string Decrypt(byte[] _data)
        {
            string ret = null;
            ICryptoTransform decryptor = _sa.CreateDecryptor(Key, IV);

            using (MemoryStream msDecrypt = new MemoryStream(_data))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                ret = srDecrypt.ReadToEnd();
            }

            return ret;
        }

        #endregion
    }

    // obsolete 1.4.0
    /*public static class RsaUtil
    {
        #region Private Members

        private static CspParameters _container = null;
        private static CspParameters Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new CspParameters()
                    {
                        KeyContainerName = "DisplayMonkeyRsaKeyContainer",
                        Flags = CspProviderFlags.NoPrompt |
                            CspProviderFlags.UseMachineKeyStore |
                            CspProviderFlags.UseArchivableKey,
                        //CryptoKeySecurity = new CryptoKeySecurity(),
                    };
                    //CryptoKeyAccessRule rule = new CryptoKeyAccessRule("everyone", CryptoKeyRights.FullControl, AccessControlType.Allow);
                    //_container.CryptoKeySecurity.SetAccessRule(rule);
                }
                return _container;
            }
        }

        #endregion

        #region Public Helpers

        public static byte[] Encrypt(string value)
        {
            if (value == null)
            {
                return null;
            }

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(Container))
            {
                return provider.Encrypt(
                    Encoding.UTF8.GetBytes(value),
                    false
                    );
            }
        }

        public static string Decrypt(byte[] data)
        {
            if (data == null)
                return null;

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(Container))
            {
                byte[] buf = null;

                try
                {
                    buf = provider.Decrypt(
                        data,
                        false
                    );
                }

                catch { }

                if (buf == null)
                    return null;
                else
                    return Encoding.UTF8.GetString(buf);
            }
        }

        #endregion
    }*/
}