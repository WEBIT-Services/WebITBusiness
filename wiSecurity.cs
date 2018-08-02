using System;
using System.Data;
using System.Text;

using WebItBusiness.Security;

namespace WebIt.Business
{
    /// <summary>
    /// WebIT business class for managing Contacts
    /// </summary>
    public class wiSecurity : wiBusinessObject
    {
        /// <summary>
        /// wiContact constructor
        /// </summary>
        public wiSecurity()
        {
            this.DatabaseKey = "";
        }

        #region Data Retrieval Methods

        /// <summary>
        /// Return all Security Objects in sql database
        /// </summary>
        /// <returns></returns>
        public DataSet GetAllSecurityObjects()
        {
            this.Sql = @"SELECT * 
                            FROM Security
                            ORDER BY Description";

            return this.GetDataSet(this.Sql, "Security");
        }

        public SecurityLevel GetSecurityLevelForUserID(int userID, Guid SecurityCd)
        {
            // TODO: Join the User for the given UserId to the security table and get the highest Security Level
            //  Then join to the roles table and to the security table and get the highest Security Level

            SecurityLevel SecurityLevel = SecurityLevel.Full;

            this.Sql = @"";

            return SecurityLevel;
        }

        #endregion

        #region Data Saving Methods


        #endregion
    }
}
