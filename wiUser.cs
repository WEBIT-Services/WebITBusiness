using System;
using System.Configuration;
using System.Data;
using System.Text;

using WebIt.Security;

namespace WebIt.Business
{
    /// <summary>
    /// WebIT business class for managing Users
    /// </summary>
    public class wiUser : wiBusinessObject
    {
        /// <summary>
        /// Encryption Algorithm to use for encrypting and decrypting passwords
        /// </summary>
        public EncryptionAlgorithm Algorithm = EncryptionAlgorithm.Rijndael;

        /// <summary>
        /// wiUser constructor
        /// </summary>
        public wiUser()
        {
            this.DatabaseKey = "";
        }

        #region Encryption Methods

        /// <summary>
        /// Returns true if user is valid in the database
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AuthenticateUser(string userName, string password)
        {
            bool ValidUser = false;

            DataSet ds = this.GetActiveUsersForUserName(userName);

            if (ds.Tables[0].Rows.Count > 0)
            {
                // Instantiate the Decryptor object
                KeyDecryptor Dec = new KeyDecryptor(this.Algorithm);

                // Get the password key
                string StoredKey = this.GetPasswordKey();
                byte[] Key = Convert.FromBase64String(StoredKey);

                // Decrypt the password stored in the database and compare it to the given password
                string Base64IV = ds.Tables[0].Rows[0]["IV"].ToString();
                string Base64Password = ds.Tables[0].Rows[0]["Password"].ToString();

                Dec.IV = Convert.FromBase64String(Base64IV);
                byte[] CipherText = Convert.FromBase64String(Base64Password);
                byte[] PlainText = Dec.Decrypt(CipherText, Key);

                if (Encoding.ASCII.GetString(PlainText) == password)
                {
                    ValidUser = true;
                }
            }

            return ValidUser;
        }

        /// <summary>
        /// Retrieve password key from the config file
        /// </summary>
        /// <returns></returns>
        private string GetPasswordKey()
        {
            string PasswordKey = "";

            PasswordKey = ConfigurationManager.AppSettings["SecureKey"].ToString();

            if (PasswordKey.Length == 0)
                throw new Exception("Password Key not found");

            return PasswordKey;
        }

        /// <summary>
        /// Encrypt the given password with strong encryption
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string[] EncryptPassword(string password)
        {
            string EncryptedPassword = "";
            string InitVector = "";
            string[] ReturnArray = new string[2];

            try
            {
                // Instantiate the Encryptor
                KeyEncryptor Enc = new KeyEncryptor(this.Algorithm);
                byte[] Password = Encoding.ASCII.GetBytes(password);

                // Get the password key
                string StoredKey = this.GetPasswordKey();
                byte[] Key = Convert.FromBase64String(StoredKey);

                // Set the Initialization vector property to null so a brand new one will get generated
                Enc.IV = null;

                // Encrypt the Password and Convert the resulting array of bytes to a Base64 string 
                byte[] CipherText = Enc.Encrypt(Password, Key);

                // Retrieve the intialization vector needed for decryption.
                byte[] IV = Enc.IV;

                // Update table with new values
                EncryptedPassword = Convert.ToBase64String(CipherText);
                InitVector = Convert.ToBase64String(IV);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception Encrypting Password. " + ex.Message);
            }

            ReturnArray[0] = EncryptedPassword;
            ReturnArray[1] = InitVector;
            return ReturnArray;
        }

        /// <summary>
        /// Decrypt the given password with the given initialization vector
        /// </summary>
        /// <param name="password"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public string DecryptPassword(string password, string iv)
        {
            // Instantiate the Decryptor object
            KeyDecryptor Dec = new KeyDecryptor(this.Algorithm);

            // Get the password key
            string StoredKey = this.GetPasswordKey();
            byte[] Key = Convert.FromBase64String(StoredKey);

            // Decrypt the password 
            Dec.IV = Convert.FromBase64String(iv);
            byte[] CipherText = Convert.FromBase64String(password);
            byte[] PlainText = Dec.Decrypt(CipherText, Key);

            return Encoding.ASCII.GetString(PlainText);
        }

        #endregion

        #region Data Retrieval Methods

        /// <summary>
        /// Retrieve user information from the Users table for the given username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private DataSet GetActiveUsersForUserName(string userName)
        {
            // Lookup the given user in the database and retrieve its password
            this.Sql = @"SELECT UserCd, UserName, Password, IV
                            FROM [User]
                            WHERE UserCd = @UserName
                            AND GetDate() BETWEEN EffectiveFrom and EffectiveThru";

            return this.GetDataSet(this.Sql, "User", this.CreateParameter("@UserName", userName));
        }

        /// <summary>
        /// Return all users in sql database
        /// </summary>
        /// <returns></returns>
        public DataSet GetAllUsers()
        {
            this.Sql = @"SELECT * 
                            FROM [User]
                            ORDER BY UserCd";

            return this.GetDataSet(this.Sql, "Users");
        }        

        /// <summary>
        /// Returns all users with their contact info
        /// </summary>
        /// <returns></returns>
        public DataSet GetAllUsersAndContacts()
        {
            this.Sql = @"SELECT u.UserCd, u.UserName, u.EffectiveFrom, u.EffectiveThru, c.FirstName, c.LastName, c.Email
                            FROM [User] AS u 
                            LEFT OUTER JOIN Contact AS c ON u.ContactId = c.ContactId
                            ORDER BY u.UserCd";

            return this.GetDataSet(this.Sql, "UserContacts");
        }

        /// <summary>
        /// Returns all User and Contact info for the given UserCd
        /// </summary>
        /// <param name="userCd"></param>
        /// <returns></returns>
        public DataSet GetUserAndContactForUserCd(string userCd)
        {
            this.Sql = @"SELECT *
                            FROM [User] AS u 
                            LEFT OUTER JOIN Contact AS c ON u.ContactId = c.ContactId
                            WHERE u.UserCd = @UserCd
                            AND GetDate() BETWEEN u.EffectiveFrom and u.EffectiveThru";

            return this.GetDataSet(this.Sql, "UserContact", this.CreateParameter("@UserCd", userCd));
        }

        /// <summary>
        /// Returns all User and Contact info for the given UserName
        /// </summary>
        /// <param name="userCd"></param>
        /// <returns></returns>
        public DataSet GetUserAndContactForUserName(string userName)
        {
            this.Sql = @"SELECT *
                            FROM [User] AS u 
                            LEFT OUTER JOIN Contact AS c ON u.ContactId = c.ContactId
                            WHERE u.UserName = @UserName
                            AND GetDate() BETWEEN u.EffectiveFrom and u.EffectiveThru";

            return this.GetDataSet(this.Sql, "UserContact", this.CreateParameter("@UserName", userName));
        }

        #endregion

        #region Data Saving Methods

        /// <summary>
        /// Insert a new record into the User table with the given values
        /// </summary>
        /// <param name="userCd"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="iv"></param>
        /// <param name="effFrom"></param>
        /// <param name="effThru"></param>
        /// <param name="contactId"></param>
        /// <param name="addUserCd"></param>
        /// <returns></returns>
        public bool InsertRecord(string userCd, string userName, string password, string iv
            , DateTime effFrom, DateTime effThru, int contactId, string addUserCd)
        {
            this.Sql = @"INSERT INTO [User]
                            (UserCd, UserName, Password, IV, EffectiveFrom, EffectiveThru, ContactId, AddUserCd)
                            VALUES (@UserCd, @UserName, @Password, @IV, @EffFrom, @EffThru, @ContactId, @AddUserCd)";

            int Result = this.ExecuteNonQuery(this.Sql
                , this.CreateParameter("@UserCd", userCd)
                , this.CreateParameter("@UserName", userName)
                , this.CreateParameter("@Password", password)
                , this.CreateParameter("@IV", iv)
                , this.CreateParameter("@EffFrom", effFrom)
                , this.CreateParameter("@EffThru", effThru)
                , this.CreateParameter("@ContactId", contactId)
                , this.CreateParameter("@AddUserCd", addUserCd)
                );

            if (Result == -1)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Update the User record for the given UserId with the given values
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userCd"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="iv"></param>
        /// <param name="effFrom"></param>
        /// <param name="effThru"></param>
        /// <param name="contactId"></param>
        /// <param name="UpdateUserCd"></param>
        /// <returns></returns>
        public bool UpdateRecordForUserId(int userId, string userCd, string userName, string password, string iv
            , DateTime effFrom, DateTime effThru, int contactId, string UpdateUserCd)
        {
            this.Sql = @"UPDATE [User]
                            SET UserCd = @UserCd
                            , UserName = @UserName
                            , Password = @Password
                            , IV = @IV
                            , EffectiveFrom = @EffFrom
                            , EffectiveThru = @EffThru
                            , ContactId = @ContactId
                            , UpdateUserCd = @UpdateUserCd
                            , UpdateDateTime = GetDate()
                            WHERE UserId = @UserId";

            int Result = this.ExecuteNonQuery(this.Sql
                , this.CreateParameter("@UserId", userId)
                , this.CreateParameter("@UserCd", userCd)
                , this.CreateParameter("@UserName", userName)
                , this.CreateParameter("@Password", password)
                , this.CreateParameter("@IV", iv)
                , this.CreateParameter("@EffFrom", effFrom)
                , this.CreateParameter("@EffThru", effThru)
                , this.CreateParameter("@ContactId", contactId)
                , this.CreateParameter("@UpdateUserCd", UpdateUserCd)
                );

            if (Result == -1)
                return false;
            else
                return true;
        }

        #endregion
    }
}
