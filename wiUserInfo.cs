using System;

namespace WebIt.Business
{
    /// <summary>
    /// wiUserInfo class to hold information about a User
    /// </summary>
    public class wiUserInfo : wiBusinessObject
    {
        /// <summary>
        /// Primary key of the user's record
        /// </summary>
        public int UserId;
        /// <summary>
        /// User's Username used to log in
        /// </summary>
        public string Username;
        /// <summary>
        /// User's First Name
        /// </summary>
        public string FirstName;
        /// <summary>
        /// User's Last Name
        /// </summary>
        public string LastName;
    }
}
