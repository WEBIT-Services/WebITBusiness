using System;
using System.Configuration;
using System.Data;
using System.Text;

namespace WebIt.Business
{
    /// <summary>
    /// WebIT business class for managing Contacts
    /// </summary>
    public class wiContact : wiBusinessObject
    {
        /// <summary>
        /// wiContact constructor
        /// </summary>
        public wiContact()
        {
            this.DatabaseKey = "";
        }

        #region Data Retrieval Methods

        /// <summary>
        /// Return all users in sql database
        /// </summary>
        /// <returns></returns>
        public DataSet GetAllContacts()
        {
            this.Sql = @"SELECT * 
                            FROM Contact
                            ORDER BY LastName, FirstName";

            return this.GetDataSet(this.Sql, "Contacts");
        }

        #endregion

        #region Data Saving Methods

        /// <summary>
        /// Insert a new record into the Contact table with the given values
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="Phone"></param>
        /// <param name="phoneExt"></param>
        /// <param name="fax"></param>
        /// <param name="address"></param>
        /// <param name="address2"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalCd"></param>
        /// <param name="addUserCd"></param>
        /// <returns></returns>
        public bool InsertRecord(string firstName, string lastName, string email, string Phone, string phoneExt
            , string fax, string address, string address2, string city, string state, string postalCd
            , string addUserCd)
        {
            this.Sql = @"INSERT INTO Contact
                            (FirstName, LastName, Email, Phone, PhoneExt, Fax, Address, Address2
                            , City, State, PostalCd, AddUserCd)
                            VALUES (@FirstName, @LastName, @Email, @Phone, @PhoneExt, @Fax, @Address, @Address2
                            , @City, @State, @PostalCd, @AddUserCd)";

            int Result = this.ExecuteNonQuery(this.Sql
                , this.CreateParameter("@FirstName", firstName)
                , this.CreateParameter("@LastName", lastName)
                , this.CreateParameter("@Email", email)
                , this.CreateParameter("@Phone", Phone)
                , this.CreateParameter("@PhoneExt", phoneExt)
                , this.CreateParameter("@Fax", fax)
                , this.CreateParameter("@Address", address)
                , this.CreateParameter("@Address2", address2)
                , this.CreateParameter("@City", city)
                , this.CreateParameter("@State", state)
                , this.CreateParameter("@PostalCd", postalCd)
                , this.CreateParameter("@AddUserCd", addUserCd)
                );

            if (Result == -1)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Update the Contact record for the given ContactId with the given values
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="Phone"></param>
        /// <param name="phoneExt"></param>
        /// <param name="fax"></param>
        /// <param name="address"></param>
        /// <param name="address2"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalCd"></param>
        /// <param name="UpdateUserCd"></param>
        /// <returns></returns>
        public bool UpdateRecordForContactId(int contactId, string firstName, string lastName, string email, string Phone, string phoneExt
            , string fax, string address, string address2, string city, string state, string postalCd
            , string UpdateUserCd)
        {
            this.Sql = @"UPDATE Contact
                            SET FirstName = @FirstName
                            , LastName = @LastName
                            , Email = @Email
                            , Phone = @Phone
                            , PhoneExt = @PhoneExt
                            , Fax = @Fax
                            , Address = @Address
                            , Address2 = @Address2
                            , City = @City
                            , State = @State
                            , PostalCd = @PostalCd
                            , UpdateUserCd = @UpdateUserCd
                            , UpdateDateTime = GetDate()
                            WHERE ContactId = @ContactId";

            int Result = this.ExecuteNonQuery(this.Sql
                , this.CreateParameter("@ContactId", contactId)
                , this.CreateParameter("@FirstName", firstName)
                , this.CreateParameter("@LastName", lastName)
                , this.CreateParameter("@Email", email)
                , this.CreateParameter("@Phone", Phone)
                , this.CreateParameter("@PhoneExt", phoneExt)
                , this.CreateParameter("@Fax", fax)
                , this.CreateParameter("@Address", address)
                , this.CreateParameter("@Address2", address2)
                , this.CreateParameter("@City", city)
                , this.CreateParameter("@State", state)
                , this.CreateParameter("@PostalCd", postalCd)
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
