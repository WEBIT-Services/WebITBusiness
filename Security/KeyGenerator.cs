using System;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Win32;

namespace WebIt.Security
{
    /// <summary>
    /// KeyGenerator class used to create, store and retrieve keys.
    /// </summary>
    public class KeyGenerator
    {
        /// <summary>
        /// The KeyGenerator will manage encryption keys.
        /// </summary>
        public KeyGenerator()
        {
        }

        /// <summary>
        /// Writes a password key to the registry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="appName"></param>
        public void SavePasswordKeyToRegistry(byte[] key, string appName)
        {
            bool ErrorOnNoKey = false;

            // Create registry key and named values
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software", true);
            rk = rk.CreateSubKey(appName);

            // Write the password encryption key to the registry
            rk.DeleteValue("PwdKey", ErrorOnNoKey);
            rk.SetValue("PwdKey", Convert.ToBase64String(key));
            rk.Close();
        }

        /// <summary>
        /// Get Password Key from Registry
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public byte[] GetPasswordKeyFromRegistry(string appName)
        {
            string StoredKey = null;
            byte[] Key = null;

            // Open connection to the Registry to get the password key for the current application
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(
                @"Software\" + appName.ToString(), false);
            StoredKey = (string)rk.GetValue("PwdKey");
            Key = Convert.FromBase64String(StoredKey);
            rk.Close();

            return Key;
        }

        /// <summary>
        /// Generate new encryption key
        /// </summary>
        /// <param name="algId"></param>
        /// <returns></returns>
        public string GenerateKey(EncryptionAlgorithm algId)
        {
            byte[] NewKey = null;

            // Pick the provider.
            switch (algId)
            {
                case EncryptionAlgorithm.Des:
                    {
                        DES des = new DESCryptoServiceProvider();
                        des.Mode = CipherMode.CBC;
                        des.GenerateKey();
                        NewKey = des.Key;
                        des = null;
                        break;
                    }
                case EncryptionAlgorithm.TripleDes:
                    {
                        TripleDES des3 = new TripleDESCryptoServiceProvider();
                        des3.Mode = CipherMode.CBC;
                        des3.GenerateKey();
                        NewKey = des3.Key;
                        des3 = null;
                        break;
                    }
                case EncryptionAlgorithm.Rc2:
                    {
                        RC2 rc2 = new RC2CryptoServiceProvider();
                        rc2.Mode = CipherMode.CBC;
                        rc2.GenerateKey();
                        NewKey = rc2.Key;
                        rc2 = null;
                        break;
                    }
                case EncryptionAlgorithm.Rijndael:
                    {
                        Rijndael rijndael = new RijndaelManaged();
                        rijndael.Mode = CipherMode.CBC;
                        rijndael.Padding = PaddingMode.PKCS7;
                        rijndael.KeySize = 256;
                        rijndael.BlockSize = 256;
                        rijndael.GenerateKey();
                        NewKey = rijndael.Key;
                        rijndael = null;
                        break;
                    }
                default:
                    {
                        throw new CryptographicException("Algorithm ID '" + algId +
                            "' not supported.");
                    }
            }

            return Encoding.ASCII.GetString(NewKey);
        }
    }
}
