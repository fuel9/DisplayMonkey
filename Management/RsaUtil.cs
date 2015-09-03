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

namespace DisplayMonkey
{
    public static class RsaUtil
    {

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
        
        public static string Decrypt(byte [] data)
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
            
        public static byte [] Encrypt(string value)
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
    }
}