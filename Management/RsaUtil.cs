using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using System.Text;

namespace DisplayMonkey
{
    public static class RsaUtil
    {

        private static CspParameters _container = new CspParameters()
        {
            KeyContainerName = "DisplayMonkeyRsaKeyContainer",
            Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseArchivableKey,
        };
        
        public static string Decrypt(byte [] data)
        {
            if (data == null) 
                return null;

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(_container))
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

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(_container))
            {
                return provider.Encrypt(
                    Encoding.UTF8.GetBytes(value),
                    false
                    );
            }
        }
    }
}