using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
 
namespace WebIt.Security
{
	/// <summary>
	/// The KeyDecryptor class decrypts data with 1 of 4 algorthims that are based on a Secret Key.
	/// </summary>
	public class KeyDecryptor
	{
		private KeyDecryptTransformer transformer;
		private byte[] initVec;

        /// <summary>
        /// KeyDecryptor constructor
        /// </summary>
        /// <param name="algId"></param>
		public KeyDecryptor(EncryptionAlgorithm algId)
		{
			this.transformer = new KeyDecryptTransformer(algId);
		}

        /// <summary>
        /// Initialization Vendor property
        /// </summary>
		public byte[] IV
		{
			get{return this.initVec;}
			set{this.initVec = value;}
		}

        /// <summary>
        /// Decrypt with DPAPI Encryptor object
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public virtual string DpapiDecrypt(byte[] cipherText)
        {
            DpDecryptor dp = new DpDecryptor(DpDecryptor.Store.USE_USER_STORE);
            byte[] PlainText = null;
            string PlainTextString = null;

            try
            {
                PlainText = dp.Decrypt(cipherText, null);
                PlainTextString = Encoding.ASCII.GetString(PlainText);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception decrypting. " + ex.Message);
            }
            return PlainTextString;
        }

        /// <summary>
        /// Decrypt data with the given key
        /// </summary>
        /// <param name="bytesData"></param>
        /// <param name="bytesKey"></param>
        /// <returns></returns>
		public byte[] Decrypt(byte[] bytesData, byte[] bytesKey)
		{
			//Set up the memory stream for the decrypted data.
			MemoryStream memStreamDecryptedData = new MemoryStream();

			//Pass in the initialization vector.
			this.transformer.IV = this.initVec;
			ICryptoTransform transform = this.transformer.GetCryptoServiceProvider(bytesKey);
			CryptoStream decStream = new CryptoStream(memStreamDecryptedData, 
				transform, 
				CryptoStreamMode.Write);
			try
			{
				decStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("Error while writing encrypted data to the stream: \n" 
					+ ex.Message);
			}
			decStream.FlushFinalBlock();
			decStream.Close();
			// Send the data back.
			return memStreamDecryptedData.ToArray();
		}
	}
}
