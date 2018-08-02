using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace WebIt.Security
{
	/// <summary>
	/// The KeyEncryptor class encrypts data with 1 of 4 algorthims that are based on a Secret Key.
	/// </summary>
	public class KeyEncryptor
	{
		private KeyEncryptTransformer transformer;
		private byte[] initVec;
		private byte[] encKey;
		
        /// <summary>
        /// KeyEncryptor constructor
        /// </summary>
        /// <param name="algId"></param>
		public KeyEncryptor(EncryptionAlgorithm algId)
		{
			this.transformer = new KeyEncryptTransformer(algId);
		}

        /// <summary>
        /// Initialization Vector property
        /// </summary>
		public byte[] IV
		{
			get{return this.initVec;}
			set{this.initVec = value;}
		}

        /// <summary>
        /// Encryption Key property
        /// </summary>
		public byte[] Key
		{
			get{return this.encKey;}
		}

        /// <summary>
        /// Encrypt with DPAPI Encryptor object
        /// </summary>
        /// <param name="plainTextString"></param>
        /// <returns></returns>
        public virtual byte[] DpapiEncrypt(string plainTextString)
        {
            byte[] plainText = null;
            byte[] cipherText = null;

            plainText = Encoding.ASCII.GetBytes(plainTextString);

            // Create DPAPI Encryptor object
            DpEncryptor dp = new DpEncryptor(DpEncryptor.Store.USE_USER_STORE);

            try
            {
                cipherText = dp.Encrypt(plainText, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception encrypting. " + ex.Message);
            }

            return cipherText;
        }

        /// <summary>
        /// Encrypt given data
        /// </summary>
        /// <param name="bytesData"></param>
        /// <param name="bytesKey"></param>
        /// <returns></returns>
		public byte[] Encrypt(byte[] bytesData, byte[] bytesKey)
		{
			//Set up the stream that will hold the encrypted data.
			MemoryStream memStreamEncryptedData = new MemoryStream();

			this.transformer.IV = this.initVec;
			ICryptoTransform transform = transformer.GetCryptoServiceProvider(bytesKey);
			CryptoStream encStream = new CryptoStream(memStreamEncryptedData, 
				transform, 
				CryptoStreamMode.Write);
			try
			{
				//Encrypt the data, write it to the memory stream.
				encStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("Error while writing encrypted data to the stream: \n" 
					+ ex.Message);
			}
			//Set the IV and key for the client to retrieve
			this.encKey = this.transformer.Key;
			this.initVec = this.transformer.IV;
			encStream.FlushFinalBlock();
			encStream.Close();

			//Send the data back.
			return memStreamEncryptedData.ToArray();
		}

	}
}
