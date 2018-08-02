using System;
using System.Security.Cryptography;


namespace WebIt.Security
{
	/// <summary>
	/// The KeyDecryptTransformer class allows 1 of 4 encryption algorithms to be used:
	/// DES is the least secure and pretty slow 
	///		- 8 bit Key, 64 bit Block Size
	/// Triple DES is a little more secure and a little slower 
	///		- 128 and 192 bit Key, 64 bit Block Size
	/// RC2 is faster and more secure than DES and Triple DES 
	///		- 40 to 128 bit Key, 64 bit Block Size
	/// Rijndael (a.k.a. AES) is the U.S. government's Federal Information Processing Standard (FIPS) 
	///		Advanced Encryption Standard (AES) cipher algorithm.  It is the newest and most secure algorithm.
	///		- 128, 192 and 256 bits Keys and Block Sizes
	/// </summary>
	internal class KeyDecryptTransformer
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;

		internal KeyDecryptTransformer(EncryptionAlgorithm deCryptId)
		{
			algorithmID = deCryptId;
		}

		internal byte[] IV
		{
			get{return initVec;}
			set{initVec = value;}
		}

		internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
		{
			// Pick the provider.
			switch (algorithmID)
			{
				case EncryptionAlgorithm.Des:
				{
					DES des = new DESCryptoServiceProvider();
					des.Mode = CipherMode.CBC;
					des.Key = bytesKey;
					des.IV = initVec;
					return des.CreateDecryptor();
				}
				case EncryptionAlgorithm.TripleDes:
				{
					TripleDES des3 = new TripleDESCryptoServiceProvider();
					des3.Mode = CipherMode.CBC;
					return des3.CreateDecryptor(bytesKey, initVec);
				}
				case EncryptionAlgorithm.Rc2:
				{
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;
					return rc2.CreateDecryptor(bytesKey, initVec);
				}
				case EncryptionAlgorithm.Rijndael:
				{
					Rijndael rijndael = new RijndaelManaged();
					rijndael.Mode = CipherMode.CBC;
					rijndael.Padding = PaddingMode.PKCS7;
					
					// Set the key and block size.
					// Although the key size defaults to 256, it's better to be explicit.
					rijndael.KeySize = 256;
  
					// BlockSize defaults to 128 bits, so let's set this
					// to 256 for better security
					rijndael.BlockSize = 256; 

					return rijndael.CreateDecryptor(bytesKey, initVec);
				} 
				default:
				{
					throw new CryptographicException("Algorithm ID '" + algorithmID + 
						"' not supported.");
				}
			}
		}

	}
}
