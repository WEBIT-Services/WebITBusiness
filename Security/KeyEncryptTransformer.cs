using System;
using System.Security.Cryptography;


namespace WebIt.Security
{
	/// <summary>
    /// Type of Algorithm Encryption
	/// </summary>
    public enum EncryptionAlgorithm 
    {
        /// <summary>
        /// DES Encryption
        /// </summary>
        Des = 1, 
        /// <summary>
        /// RC2 Encryption
        /// </summary>
        Rc2, 
        /// <summary>
        /// Rijndael Encryption
        /// </summary>
        Rijndael, 
        /// <summary>
        /// TripleDes Encryption
        /// </summary>
        TripleDes};

	/// <summary>
	/// The KeyEncryptTransformer class allows 1 of 4 encryption algorithms to be used:
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
	internal class KeyEncryptTransformer
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;
		private byte[] encKey;

		internal KeyEncryptTransformer(EncryptionAlgorithm algId)
		{
			//Save the algorithm being used.
			algorithmID = algId;
		}

		internal byte[] IV
		{
			get{return initVec;}
			set{initVec = value;}
		}
		internal byte[] Key
		{
			get{return encKey;}
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

					// See if a key was provided
					if (null == bytesKey)
					{
						des.GenerateKey();
						encKey = des.Key;
					}
					else
					{
						des.Key = bytesKey;
						encKey = des.Key;
					}

					// See if the client provided an initialization vector
					if (null == initVec)
					{ // Have the algorithm create one
						des.GenerateIV();
						initVec = des.IV;
					}
					else
					{ //No, give it to the algorithm
						des.IV = initVec;
					}
					return des.CreateEncryptor();
				}
				case EncryptionAlgorithm.TripleDes:
				{
					TripleDES des3 = new TripleDESCryptoServiceProvider();
					des3.Mode = CipherMode.CBC;
					// See if a key was provided
					if (null == bytesKey)
					{
						des3.GenerateKey();
						encKey = des3.Key;
					}
					else
					{
						des3.Key = bytesKey;
						encKey = des3.Key;
					}

					// See if the client provided an IV
					if (null == initVec)
					{ //Yes, have the alg create one
						des3.GenerateIV();
						initVec = des3.IV;
					}
					else
					{ //No, give it to the alg.
						des3.IV = initVec;
					}
					return des3.CreateEncryptor();
				}
				case EncryptionAlgorithm.Rc2:
				{
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;
					// Test to see if a key was provided
					if (null == bytesKey)
					{
						rc2.GenerateKey();
						encKey = rc2.Key;
					}
					else
					{
						rc2.Key = bytesKey;
						encKey = rc2.Key;
					}

					// See if the client provided an IV
					if (null == initVec)
					{ //Yes, have the alg create one
						rc2.GenerateIV();
						initVec = rc2.IV;
					}
					else
					{ //No, give it to the alg.
						rc2.IV = initVec;
					}
					return rc2.CreateEncryptor();
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

					// Test to see if a key was provided
					if(null == bytesKey)
					{
						// GenerateKey method utilizes the RNGCryptoServiceProvider
						// class to generate random bytes of necessary length.
						rijndael.GenerateKey();
						encKey = rijndael.Key;
					}
					else
					{
						rijndael.Key = bytesKey;
						encKey = rijndael.Key;
					}

					// See if the client provided an IV
					if(null == initVec)
					{ //Yes, have the alg create one
						rijndael.GenerateIV();
						initVec = rijndael.IV;
					}
					else
					{ //No, give it to the alg.
						rijndael.IV = initVec;
					}
					return rijndael.CreateEncryptor();
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
