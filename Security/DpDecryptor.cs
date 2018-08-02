using System;
using System.Text;
using System.Runtime.InteropServices;

namespace WebIt.Security
{
	/// <summary>
	/// The DpDecryptor class encypts data using the Data Protection API algorithmn
	/// DPAPI uses an NT user account's password as the key instead of a key stored on the file system.
	/// </summary>
	public class DpDecryptor
	{
		[DllImport("Crypt32.dll", SetLastError=true, 
			 CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool CryptUnprotectData(
			ref DATA_BLOB pDataIn, 
			String szDataDescr, 
			ref DATA_BLOB pOptionalEntropy, 
			IntPtr pvReserved, 
			ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct, 
			int dwFlags, 
			ref DATA_BLOB pDataOut);

		[DllImport("kernel32.dll", 
			 CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private unsafe static extern int FormatMessage(int dwFlags, 
			ref IntPtr lpSource, 
			int dwMessageId,
			int dwLanguageId, 
			ref String lpBuffer, int nSize, 
			IntPtr *Arguments);

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct DATA_BLOB
		{
			public int cbData;
			public IntPtr pbData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
			internal struct CRYPTPROTECT_PROMPTSTRUCT
		{
			public int cbSize;
			public int dwPromptFlags;
			public IntPtr hwndApp;
			public String szPrompt;
		}
		static private IntPtr NullPtr = ((IntPtr)((int)(0)));
		private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
		private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

        /// <summary>
        /// Type of Store property
        /// </summary>
        public enum Store
        {
            /// <summary>
            /// Stored per machine
            /// </summary> 
            USE_MACHINE_STORE = 1,
            /// <summary>
            /// Stored per User
            /// </summary>
            USE_USER_STORE
        };

		private Store _store;

        /// <summary>
        /// DpDecryptor constructor
        /// </summary>
        /// <param name="tempStore"></param>
		public DpDecryptor(Store tempStore)
		{
			this._store = tempStore;
		}

		/// <summary>
		/// Decrypt takes encrypted text and decypts is using DPAPI
		/// </summary>
		/// <param name="cipherText"></param>
		/// <param name="optionalEntropy"></param>
		/// <returns>byte array of plain text</returns>
		public byte[] Decrypt(byte[] cipherText, byte[] optionalEntropy)
		{
			bool retVal = false;
			DATA_BLOB plainTextBlob = new DATA_BLOB();
			DATA_BLOB cipherBlob = new DATA_BLOB();
			CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
			InitPromptstruct(ref prompt);
			try
			{
				try
				{
					int cipherTextSize = cipherText.Length;
					cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
					if(IntPtr.Zero == cipherBlob.pbData)
					{
						throw new Exception("Unable to allocate cipherText buffer.");
					}
					cipherBlob.cbData = cipherTextSize;  
					Marshal.Copy(cipherText, 0, cipherBlob.pbData, cipherBlob.cbData);
				}
				catch(Exception ex)
				{
					throw new Exception("Exception marshalling data. " + ex.Message);
				}
				DATA_BLOB entropyBlob = new DATA_BLOB();
				int dwFlags;
				if(Store.USE_MACHINE_STORE == this._store)
				{//Using the machine store, should be providing entropy.
					dwFlags = CRYPTPROTECT_LOCAL_MACHINE|CRYPTPROTECT_UI_FORBIDDEN;
					//Check to see if the entropy is null
					if(null == optionalEntropy)
					{//Allocate something
						optionalEntropy = new byte[0];
					}
					try
					{
						int bytesSize = optionalEntropy.Length;
						entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);
						if(IntPtr.Zero == entropyBlob.pbData)
						{
							throw new Exception("Unable to allocate entropy buffer.");
						}
						entropyBlob.cbData = bytesSize;
						Marshal.Copy(optionalEntropy, 0, entropyBlob.pbData, bytesSize);
					}
					catch(Exception ex)
					{
						throw new Exception("Exception entropy marshalling data. " + 
							ex.Message);
					}
				}
				else
				{//Using the user store
					dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
				}
				retVal = CryptUnprotectData(ref cipherBlob, null, ref entropyBlob, 
					IntPtr.Zero, ref prompt, dwFlags, 
					ref plainTextBlob);
				if(false == retVal)
				{
					throw new Exception("Decryption failed. " + 
						GetErrorMessage(Marshal.GetLastWin32Error()));
				}
				//Free the blob and entropy.
				if(IntPtr.Zero != cipherBlob.pbData)
				{
					Marshal.FreeHGlobal(cipherBlob.pbData);
				}
				if(IntPtr.Zero != entropyBlob.pbData)
				{
					Marshal.FreeHGlobal(entropyBlob.pbData);
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Exception decrypting. " + ex.Message);
			}
			byte[] plainText = new byte[plainTextBlob.cbData];
			Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
			return plainText;
		}

		private void InitPromptstruct(ref CRYPTPROTECT_PROMPTSTRUCT ps) 
		{
			ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
			ps.dwPromptFlags = 0;
			ps.hwndApp = NullPtr;
			ps.szPrompt = null;
		}

		private unsafe static String GetErrorMessage(int errorCode)
		{
			int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
			int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
			int FORMAT_MESSAGE_FROM_SYSTEM  = 0x00001000;
			int messageSize = 255;
			String lpMsgBuf = "";
			int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | 
				FORMAT_MESSAGE_IGNORE_INSERTS;
			IntPtr ptrlpSource = new IntPtr();
			IntPtr prtArguments = new IntPtr();
			int retVal = FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0, 
				ref lpMsgBuf, messageSize, &prtArguments);
			if(0 == retVal)
			{
				throw new Exception("Failed to format message for error code " + 
					errorCode + ". ");
			}
			return lpMsgBuf;
		}
	}
}
