using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Configuration;
using System.IO;

namespace WebIt.Security
{
	/// <summary>
	/// This is an Active Directory helper class.
	/// </summary>
	public class ADHelper
	{
		#region Private Variables

        private string ADPath = ConfigurationManager.AppSettings["ADPath"].ToString();
        private string ADUser = ConfigurationManager.AppSettings["ADAdminUser"].ToString();
        private string ADPassword = ConfigurationManager.AppSettings["ADAdminPassword"].ToString();
        private string ADServer = ConfigurationManager.AppSettings["ADServer"].ToString();
        
        //This is the managed definition of this interface also found in ActiveDs.tlb
        [ComImport]
        [Guid("9068270B-0939-11D1-8BE1-00C04FD8D503")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IADsLargeInteger
        {
            [DispId(0x00000002)]
            int HighPart { get; set;}
            [DispId(0x00000003)]
            int LowPart { get; set;}
        }

		#endregion

		#region Enumerations
		
        /// <summary>
        /// Properties of an entity in Active Directory
        /// </summary>
        public enum ADAccountOptions
		{
			/// <summary>
			/// Duplicate Account
			/// </summary>
            UF_TEMP_DUPLICATE_ACCOUNT = 0x0100,
			/// <summary>
			/// Normal Account
			/// </summary>
            UF_NORMAL_ACCOUNT =0x0200,
            /// <summary>
            /// INTERDOMAIN TRUST ACCOUNT
            /// </summary>
			UF_INTERDOMAIN_TRUST_ACCOUNT =0x0800,
            /// <summary>
            /// WORKSTATION TRUST ACCOUNT
            /// </summary>
            UF_WORKSTATION_TRUST_ACCOUNT = 0x1000,
            /// <summary>
            /// SERVER TRUST ACCOUNT
            /// </summary>
            UF_SERVER_TRUST_ACCOUNT =0x2000,
            /// <summary>
            /// UF DONT EXPIRE PASSWD
            /// </summary>
            UF_DONT_EXPIRE_PASSWD=0x10000,
            /// <summary>
            /// UF SCRIPT
            /// </summary>
            UF_SCRIPT =0x0001,
            /// <summary>
            /// UF ACCOUNTDISABLE
            /// </summary>
			UF_ACCOUNTDISABLE=0x0002,
            /// <summary>
            /// UF HOMEDIR REQUIRED
            /// </summary>
			UF_HOMEDIR_REQUIRED =0x0008,
            /// <summary>
            /// UF LOCKOUT
            /// </summary>
			UF_LOCKOUT=0x0010,
            /// <summary>
            /// UF PASSWD NOTREQD
            /// </summary>
			UF_PASSWD_NOTREQD=0x0020,
            /// <summary>
            /// UF PASSWD CANT CHANGE
            /// </summary>
			UF_PASSWD_CANT_CHANGE=0x0040,
            /// <summary>
            /// UF ACCOUNT LOCKOUT
            /// </summary>
			UF_ACCOUNT_LOCKOUT=0X0010,
            /// <summary>
            /// UF ENCRYPTED TEXT PASSWORD ALLOWED
            /// </summary>
			UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED=0X0080,
		}	
        
        /// <summary>
        /// Result of an attempted Login
        /// </summary>
		public enum LoginResult
		{
            /// <summary>
            /// LOGIN OK
            /// </summary>
			LOGIN_OK=0,
            /// <summary>
            /// LOGIN USER DOESNT EXIST
            /// </summary>
			LOGIN_USER_DOESNT_EXIST,
            /// <summary>
            /// LOGIN USER ACCOUNT INACTIVE
            /// </summary>
			LOGIN_USER_ACCOUNT_INACTIVE
		}

		#endregion
        
		/// <summary>
		/// ADHelper constructor
		/// </summary>
        public ADHelper()
		{
		}
		
        #region Display Methods
        /// <summary>
        /// Get the Date and Time out of a DirectoryEntry COM object
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetComDateTime(DirectoryEntry entry, string propertyName)
        {
            string DateTimeString = "Unknown";

            try
            {
                if (entry.Properties[propertyName].Count > 0)
                {
                    IADsLargeInteger li = (IADsLargeInteger)entry.Properties[propertyName][0];
                    long date = (long)li.HighPart << 32 | (uint)li.LowPart;
                    if (date == 9223372036854775807)
                        DateTimeString = "Never";
                    else
                    {
                        DateTimeString = DateTime.FromFileTime(date).ToString();
                    }
                }
            }
            catch
            {
                DateTimeString = "Unknown";
            }            

            return DateTimeString;
        }

        /// <summary>
        /// Build string ALL information stored in Active Directory
        /// for all users that match the User Expression
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetAllUserInfo(string userExpr)
        {
            string UserInfo = "Domain:  " + this.ADPath + this.ADServer + "\r\n";
            SearchResultCollection Results = this.GetUsers(userExpr);

            foreach (SearchResult Result in Results)
            {
                DirectoryEntry Entry = Result.GetDirectoryEntry();

                foreach (string propertyName in Entry.Properties.PropertyNames)
                {
                    if (Entry.Properties[propertyName].Count > 0)
                    {
                        if (propertyName != "lastLogoff"
                            && propertyName != "lastLogon"
                            && propertyName != "pwdLastSet"
                            && propertyName != "badPasswordTime"
                            && propertyName != "uSNChanged"
                            && propertyName != "uSNCreated"
                            && propertyName != "accountExpires"
                            )
                            UserInfo += this.AddInfo(propertyName, propertyName, Entry);
                        else
                            UserInfo += propertyName + ":  " + this.GetComDateTime(Entry, propertyName) + "\r\n";
                    }
                }
            }

            return UserInfo;
        }        

        /// <summary>
        /// Build string of most important information for all users that match the User Expression
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetUserInfo(string userExpr)
        {
            string UserInfo = "Domain:  " + this.ADPath + this.ADServer + "\r\n";
            SearchResultCollection Results = this.GetUsers(userExpr);
            
            foreach (SearchResult Result in Results)
            {
                try
                {
                    DirectoryEntry de = Result.GetDirectoryEntry();
                                        
                    UserInfo += this.AddInfo("User Name", "sAMAccountName", de);
                    UserInfo += this.AddInfo("First Name", "GivenName", de);
                    UserInfo += this.AddInfo("Last Name", "description", de);
                    UserInfo += this.AddInfo("Display Name", "displayName", de);
                    UserInfo += this.AddInfo("Email", "Mail", de);
                    UserInfo += this.AddInfo("Title", "Title", de);
                    UserInfo += this.AddInfo("Company", "company", de);
                    UserInfo += this.AddInfo("Office", "physicalDeliveryOfficeName", de);
                    UserInfo += this.AddInfo("Department", "department", de);
                    UserInfo += this.AddInfo("Telephone Number", "telephoneNumber", de);
                    UserInfo += this.AddInfo("Home Phone", "homePhone", de);
                    UserInfo += this.AddInfo("Mobile", "mobile", de);
                    UserInfo += this.AddInfo("User Created", "whenCreated", de);
                    UserInfo += this.AddInfo("User Changed", "whenChanged", de);
                    UserInfo += "Groups: \r\n";
                    foreach (string  Group in de.Properties["memberOf"])
                    {
                        UserInfo += "\t" + Group.ToString() + "\r\n";
                    }
                    UserInfo += "Last Logoff: " + this.GetComDateTime(de, "lastLogoff") + "\r\n";
                    UserInfo += "Last Logon: " + this.GetComDateTime(de, "lastLogon") + "\r\n";
                    UserInfo += "Pwd Last Set: " + this.GetComDateTime(de, "pwdLastSet") + "\r\n";
                    UserInfo += "Bad Password Time: " + this.GetComDateTime(de, "badPasswordTime") + "\r\n";
                    UserInfo += "USN Changed: " + this.GetComDateTime(de, "uSNChanged") + "\r\n";
                    UserInfo += "USN Created: " + this.GetComDateTime(de, "uSNCreated") + "\r\n";
                    UserInfo += "Account Expires: " + this.GetComDateTime(de, "accountExpires") + "\r\n";
                    UserInfo += "Account Enabled: " + (de.Properties["userAccountControl"].Value.ToString() == "66048" ? "YES\r\n" : "NO\r\n");
                    
                }
                catch (Exception e)
                {
                    UserInfo += e.Message;
                }
                UserInfo += "\r\n\r\n====================================\r\n\r\n";
            }

            return UserInfo;
        }

        private string AddInfo(string description, string property, DirectoryEntry entry)
        {
            string Info = "";

            try
            {
                Info = description + ":  " + entry.Properties[property].Value.ToString() + "\r\n";
            }
            catch (Exception)
            {
                Info = description + ":  Unknown\r\n";
            }

            return Info;
        }

        #endregion

        #region Search Methods
        /// <summary>
		/// This will return a DirectoryEntry object if the user does exist
		/// </summary>
        /// <param name="userExpr"></param>
		/// <returns></returns>
        public DirectoryEntry GetUser(string userExpr)
		{
            //create an instance of the DirectoryEntry
            DirectoryEntry de = GetDirectoryObject();

            //create instance of the direcory searcher
            DirectorySearcher deSearch = new DirectorySearcher();

            deSearch.SearchRoot = de;

            //set the search filter
            deSearch.Filter = "(anr=" + userExpr + ")";
            deSearch.SearchScope = SearchScope.Subtree;
            
			//find the first instance
			SearchResult results= deSearch.FindOne();

			//if found then return, otherwise return Null
			if(results !=null)
			{
				de= new DirectoryEntry(results.Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				//if so then return the DirectoryEntry object
				return de;
			}
			else
			{
				return null;
			}
		}

        /// <summary>
        /// This will lookup a user in Active Directory and return their Account Name
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetUserAccountName(string userExpr)
        {
            SearchResultCollection Results = this.GetUsers(userExpr);

            switch (Results.Count)
	        {
		        case 0:
                    return "User Not Found: " + userExpr;
                case 1:
                    try
                    {
                        DirectoryEntry de = Results[0].GetDirectoryEntry();
                        return de.Properties["sAMAccountName"].Value.ToString();
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                default:
                    return "Too many users found for: " + userExpr;
	        }
        }

        /// <summary>
        /// This will lookup a user in Active Directory and return their email address
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetUserEmail(string userExpr)
        {
            SearchResultCollection Results = this.GetUsers(userExpr);

            switch (Results.Count)
            {
                case 0:
                    return "User Not Found: " + userExpr;
                case 1:
                    try
                    {
                        DirectoryEntry de = Results[0].GetDirectoryEntry();
                        if (de.Properties["Mail"].Value != null)
                            return de.Properties["Mail"].Value.ToString();
                        else
                            return "Not Specified";
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                default:
                    return "Too many users found for: " + userExpr;
            }
        }

        /// <summary>
        /// This will lookup a user in Active Directory and return their Telephone Number
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetUserPhone(string userExpr)
        {
            SearchResultCollection Results = this.GetUsers(userExpr);

            switch (Results.Count)
            {
                case 0:
                    return "User Not Found: " + userExpr;
                case 1:
                    try
                    {
                        DirectoryEntry de = Results[0].GetDirectoryEntry();
                        if (de.Properties["telephoneNumber"].Value != null)
                            return de.Properties["telephoneNumber"].Value.ToString();
                        else
                            return "Not Specified";
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                default:
                    return "Too many users found for: " + userExpr;
            }
        }

        /// <summary>
        /// This will lookup a user in Active Directory and return their Office Name
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetUserOffice(string userExpr)
        {
            SearchResultCollection Results = this.GetUsers(userExpr);

            switch (Results.Count)
            {
                case 0:
                    return "User Not Found: " + userExpr;
                case 1:
                    try
                    {
                        DirectoryEntry de = Results[0].GetDirectoryEntry();
                        if (de.Properties["physicalDeliveryOfficeName"].Value != null)
                            return de.Properties["physicalDeliveryOfficeName"].Value.ToString();
                        else
                            return "Not Specified";
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                default:
                    return "Too many users found for: " + userExpr;
            }
        }

        /// <summary>
        /// This will lookup a user in Active Directory and return their Office Name
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string GetUserCompany(string userExpr)
        {
            SearchResultCollection Results = this.GetUsers(userExpr);

            switch (Results.Count)
            {
                case 0:
                    return "User Not Found: " + userExpr;
                case 1:
                    try
                    {
                        DirectoryEntry de = Results[0].GetDirectoryEntry();
                        if (de.Properties["Company"].Value != null)
                            return de.Properties["Company"].Value.ToString();
                        else
                            return "Not Specified";
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                default:
                    return "Too many users found for: " + userExpr;
            }
        }

        /// <summary>
        /// This will lookup a user in Active Directory and return their email address
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public string IsUserEnabled(string userExpr)
        {
            SearchResultCollection Results = this.GetUsers(userExpr);

            switch (Results.Count)
            {
                case 0:
                    return "User Not Found: " + userExpr;
                case 1:
                    try
                    {
                        DirectoryEntry de = Results[0].GetDirectoryEntry();
                        if (de.Properties["userAccountControl"].Value.ToString() == "66048")
                            return "YES";
                        else
                            return "NO";
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                default:
                    return "Too many users found for: " + userExpr;
            }
        }

        /// <summary>
        /// This will return a SearchResultCollection object with all users found for the User Expression
        /// </summary>
        /// <param name="userExpr"></param>
        /// <returns></returns>
        public SearchResultCollection GetUsers(string userExpr)
        {

            //DirectoryEntry de = new DirectoryEntry("LDAP://" + ADServer);
            //DirectorySearcher mySearcher = new DirectorySearcher(entry);
            //mySearcher.Filter = ("(objectClass=user)");

            //create an instance of the DirectoryEntry
            DirectoryEntry de = GetDirectoryObject();

            //create instance of the direcory searcher
            DirectorySearcher mySearcher = new DirectorySearcher();

            mySearcher.SearchRoot = de;

            //set the search filter
            mySearcher.Filter = "(anr=" + userExpr + ")";
            mySearcher.SearchScope = SearchScope.Subtree;

            //find the first instance
            SearchResultCollection results = mySearcher.FindAll();

            //if found then return, otherwise return Null
            if (results != null)
            {
                return results;
            }
            else
            {
                return null;
            }
        }

		/// <summary>
		/// Override method which will perfrom query based on combination of username and password
		/// This is used with the login process to validate the user credentials and return a user
		/// object for further validation.  This is slightly different from the other GetUser... methods as this
		/// will use the UserName and Password supplied as the authentication to check if the user exists, if so then
		/// the users object will be queried using these credentials.s
		/// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
		/// <returns></returns>
		public DirectoryEntry GetUser(string userName, string password)
		{
			//create an instance of the DirectoryEntry
			DirectoryEntry de = GetDirectoryObject(userName, password);

			//create instance fo the direcory searcher
			DirectorySearcher deSearch = new DirectorySearcher();
			
			deSearch.SearchRoot =de;
			//set the search filter
			deSearch.Filter = "(&(objectClass=user)(cn=" + userName + "))";
			deSearch.SearchScope = SearchScope.Subtree;

			//set the property to return
			//deSearch.PropertiesToLoad.Add("givenName");
            
			//find the first instance
			SearchResult results= deSearch.FindOne();
		
			//if a match is found, then create directiry object and return, otherwise return Null
			if(results !=null)
			{
				//create the user object based on the admin priv.
				de= new DirectoryEntry(results.Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				return de;
			}
			else
			{
				return null;
			}

			
		}
		/// <summary>
		/// This will take a username and query the AD for the user.  When found it will transform
		/// the results from the poperty collection into a Dataset which can be used by the client
		/// </summary>
		/// <param name="UserName"></param>
		/// <returns></returns>
		public DataSet GetUserDataSet(string UserName)
		{
			DirectoryEntry de = GetDirectoryObject();

			//create instance fo the direcory searcher
			DirectorySearcher deSearch = new DirectorySearcher();
			
			deSearch.SearchRoot =de;
			//set the search filter
			deSearch.Filter = "(&(objectClass=user)(cn=" + UserName + "))";
			deSearch.SearchScope = SearchScope.Subtree;
            
			//find the first instance
			SearchResult results= deSearch.FindOne();

			//get Empty user dataset
			DataSet dsUser = CreateUserDataSet();

			//If no user record returned, then dont do anything, otherwise
			//populate
			if(results != null)
			{
				//populate the dataset with the values from the results
				dsUser.Tables["User"].Rows.Add( PopulateUserDataSet(results,dsUser.Tables["User"]));
				
			}
			de.Close();
			
			return dsUser;

		}

		/// <summary>
		/// This method will return a dataset of user details based on criteria
		/// passed to the query.  The criteria is in the LDAP format ie
		/// (cn='xxx')(sn='eee') etc
		/// </summary>
		/// <param name="Criteria"></param>
		/// <returns></returns>
		public DataSet GetUsersDataSet(string Criteria)
		{
			DirectoryEntry de = GetDirectoryObject();

			//create instance fo the direcory searcher
			DirectorySearcher deSearch = new DirectorySearcher();
			
			deSearch.SearchRoot =de;
			//set the search filter
			deSearch.Filter = "(&(objectClass=user)(objectCategory=person)" + Criteria +")";
			deSearch.SearchScope = SearchScope.Subtree;
            
			//find the first instance
			SearchResultCollection  results= deSearch.FindAll();

			//get Empty user dataset
			DataSet dsUser = CreateUserDataSet();

			//If no user record returned, then dont do anything, otherwise
			//populate
			if(results.Count>0)
			{
				foreach(SearchResult result in results)
				{
					//populate the dataset with the values from the results
					dsUser.Tables["User"].Rows.Add( PopulateUserDataSet(result,dsUser.Tables["User"]));
				}
			}

			de.Close();
			return dsUser;

		}

		/// <summary>
		/// This method will query all of the defined AD groups
		/// and will turn the results into a dataset to be returned
		/// </summary>
		/// <returns></returns>
		public DataSet GetGroups()
		{
			DataSet dsGroup = new DataSet();
			DirectoryEntry de = GetDirectoryObject();

			//create instance fo the direcory searcher
			DirectorySearcher deSearch = new DirectorySearcher();

			//set the search filter
			deSearch.SearchRoot =de;
			//deSearch.PropertiesToLoad.Add("cn");
			deSearch.Filter = "(&(objectClass=group)(cn=CS_*))";
            
			//find the first instance
			SearchResultCollection   results= deSearch.FindAll();

			//Create a new table object within the dataset
			DataTable tbGroup = dsGroup.Tables.Add("Groups");
			tbGroup.Columns.Add("GroupName");

			//if there are results (there should be some!!), then convert the results
			//into a dataset to be returned.
			if(results.Count >0)
			{
		
				//iterate through collection and populate the table with
				//the Group Name
				foreach (SearchResult Result in results)
				{
					//set a new empty row
					DataRow rwGroup = tbGroup.NewRow();
					
					//populate the column
					rwGroup["GroupName"]= Result.Properties["cn"][0];

					//append the row to the table of the dataset
					tbGroup.Rows.Add(rwGroup);
				}
			}
			return dsGroup;
		}

		/// <summary>
		/// This method will return all users for the specified group in a dataset
		/// </summary>
		/// <param name="GroupName"></param>
		/// <returns></returns>
		public DataSet GetUsersForGroup(string GroupName)
		{
			DataSet dsUser = new DataSet();
			DirectoryEntry de = GetDirectoryObject();
  
			//create instance fo the direcory searcher
			DirectorySearcher deSearch = new DirectorySearcher();

			//set the search filter
			deSearch.SearchRoot =de;
			//deSearch.PropertiesToLoad.Add("cn");
			deSearch.Filter = "(&(objectClass=group)(cn=" + GroupName +"))";
            
			//get the group result
			SearchResult results= deSearch.FindOne();

			//Create a new table object within the dataset
			DataTable tbUser = dsUser.Tables.Add("Users");
			tbUser.Columns.Add("UserName");
			tbUser.Columns.Add("DisplayName");
			tbUser.Columns.Add("EMailAddress");

			//Create default row
			DataRow rwDefaultUser = tbUser.NewRow();
			rwDefaultUser ["UserName"]= "0";
			rwDefaultUser ["DisplayName"]="(Not Specified)";
			rwDefaultUser ["EMailAddress"]="(Not Specified)";
			tbUser.Rows.Add(rwDefaultUser);
		
			//if the group is valid, then continue, otherwise return a blank dataset
			if(results !=null)
			{
				//create a link to the group object, so we can get the list of members
				//within the group
				DirectoryEntry  deGroup= new DirectoryEntry(results.Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				//assign a property collection
				System.DirectoryServices.PropertyCollection  pcoll = deGroup.Properties;
				int n = pcoll["member"].Count;
				
				//if there are members fo the group, then get the details and assign to the table
				for (int l = 0; l < n ; l++)
				{
					//create a link to the user object sot hat the FirstName, LastName and SUername can be gotten
					 DirectoryEntry deUser= new DirectoryEntry(ADPath + "/" +pcoll["member"][l].ToString(),ADUser,ADPassword,AuthenticationTypes.Secure);
				
					//set a new empty row
					DataRow rwUser = tbUser.NewRow();
					
					//populate the column
					rwUser["UserName"]= GetProperty(deUser,"cn");
					rwUser["DisplayName"]= GetProperty(deUser,"givenName") + " "  + GetProperty(deUser,"sn");
					rwUser["EMailAddress"]= GetProperty(deUser,"mail");
					//append the row to the table of the dataset
					tbUser.Rows.Add(rwUser);

					//close the directory entry object
					deUser.Close();
					
				}
				de.Close();
				deGroup.Close();
			}
			

			return dsUser;
		}


        /// <summary>
        /// This is used mainly for the logon process to ensure that the username and password match
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public DirectoryEntry UserExists(string UserName, string Password)
        {
            //create an instance of the DirectoryEntry
            DirectoryEntry de = GetDirectoryObject();//UserName,Password);

            //create instance fo the direcory searcher
            DirectorySearcher deSearch = new DirectorySearcher();

            //set the search filter
            deSearch.SearchRoot = de;
            deSearch.Filter = "((objectClass=user)(cn=" + UserName + ")(userPassword=" + Password + "))";
            deSearch.SearchScope = SearchScope.Subtree;

            //set the property to return
            //deSearch.PropertiesToLoad.Add("givenName");

            //find the first instance
            SearchResult results = deSearch.FindOne();


            //if the username and password do match, then this implies a valid login
            //if so then return the DirectoryEntry object
            de = new DirectoryEntry(results.Path, ADUser, ADPassword, AuthenticationTypes.Secure);

            return de;

        }

        /// <summary>
        /// Check to see if the specified user exists
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public bool UserExists(string UserName)
        {
            //create an instance of the DirectoryEntry
            DirectoryEntry de = GetDirectoryObject();

            //create instance fo the direcory searcher
            DirectorySearcher deSearch = new DirectorySearcher();

            //set the search filter
            deSearch.SearchRoot = de;
            deSearch.Filter = "(&(objectClass=user) (cn=" + UserName + "))";

            //find the first instance
            SearchResultCollection results = deSearch.FindAll();

            //if the username and password do match, then this implies a valid login
            //if so then return the DirectoryEntry object
            if (results.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        /// <summary>
        /// This method will not actually log a user in, but will perform tests to ensure
        /// that the user account exists (matched by both the username and password), and also
        /// checks if the account is active.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public LoginResult Login(string UserName, string Password)
        {
            //first, check if the logon exists based on the username and password
            //DirectoryEntry de = GetUser(UserName,Password);

            if (IsUserValid(UserName, Password))
            {
                DirectoryEntry de = GetUser(UserName);
                if (de != null)
                {
                    //convert the accountControl value so that a logical operation can be performed
                    //to check of the Disabled option exists.
                    int userAccountControl = Convert.ToInt32(de.Properties["userAccountControl"][0]);
                    de.Close();

                    //if the disabled item does not exist then the account is active
                    if (!IsAccountActive(userAccountControl))
                    {
                        return LoginResult.LOGIN_USER_ACCOUNT_INACTIVE;
                    }
                    else
                    {
                        return LoginResult.LOGIN_OK;
                    }

                }
                else
                {
                    return LoginResult.LOGIN_USER_DOESNT_EXIST;
                }
            }
            else
            {
                return LoginResult.LOGIN_USER_DOESNT_EXIST;
            }
        }

        /// <summary>
        /// This will perfrom a logical operation on the userAccountControl values
        /// to see if the user account is enabled or disabled.  The flag for determining if the
        /// account is active is a bitwise value (decimal =2)
        /// </summary>
        /// <param name="userAccountControl"></param>
        /// <returns></returns>
        public bool IsAccountActive(int userAccountControl)
        {
            int userAccountControl_Disabled = Convert.ToInt32(ADAccountOptions.UF_ACCOUNTDISABLE);
            int flagExists = userAccountControl & userAccountControl_Disabled;
            //if a match is found, then the disabled flag exists within the control flags
            if (flagExists > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// This method will attempt to log in a user based on the username and password
        /// to ensure that they have been set up within the Active Directory.  This is the basic UserName, Password
        /// check.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public bool IsUserValid(string UserName, string Password)
        {
            try
            {
                //if the object can be created then return true
                DirectoryEntry deUser = GetUser(UserName, Password);
                deUser.Close();
                return true;
            }
            catch (Exception)
            {
                //otherwise return false
                return false;
            }
        }

        /// <summary>
        /// This will query the user (by using the administrator role) and will set the new password
        /// This will not validate the existing password, as it will be assumed that if there logged in then
        /// the password can be changed.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="newPassword"></param>
        public void SetUserPassword(string userName, string newPassword)
        {
            //get reference to user
            //string LDAPDomain = "/CN=" + UserName + ",CN=Users," + GetLDAPDomain();
            //DirectoryEntry oUser = GetDirectoryObject(LDAPDomain);//,UserName,OldPassword);
            DirectoryEntry oUser = this.GetUser(userName);
            oUser.Invoke("SetPassword", new Object[] { newPassword });
            oUser.Close();
        }

        /// <summary>
        /// This method will be used by the admin query screen, and is a method
        /// to return users based on a possible combination of lastname, email address or corporate
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="emailAddress"></param>
        /// <param name="corporate"></param>
        /// <returns></returns>
        public DataSet GetUsersByNameEmailCorporate(string lastName, string emailAddress, string corporate)
        {
            StringBuilder SQLWhere = new StringBuilder();

            //if the LastName is present, then include in the where clause
            if (lastName != string.Empty)
            {
                SQLWhere.Append("(sn=" + lastName + ")");
            }


            //if the emailaddress is present, then include in the where clause
            if (emailAddress != string.Empty)
            {
                SQLWhere.Append("(mail=" + emailAddress + ")");
            }

            //if the corporate is present, then include in the where clause
            if ((corporate != string.Empty) && (corporate != "1"))
            {
                SQLWhere.Append("(extensionAttribute12=" + corporate + ")");
            }

            //append the where clause, remove the last 'AND'
            //SQLStmt.Append(";(objectClass=*); sn, givenname, mail");

            return GetUsersDataSet(SQLWhere.ToString());

        }

		#endregion
	
        #region Set User Details Methods
		/// <summary>
		/// Set the user password
		/// </summary>
		/// <param name="oDE"></param>
		/// <param name="Password"></param>
		public void SetUserPassword(DirectoryEntry oDE, string Password)
		{
			
			oDE.Invoke("SetPassword",new Object[]{Password});

			//string[] yourpw={Password};
			//oDE.Invoke("SetPassword", yourpw);
			//oDE.CommitChanges();
			
			//object[] password = new object[] {Password};
			//object ret = oDE.Invoke("SetPassword", password );
			//oDE.CommitChanges();

		}
		
        /// <summary>
		/// This will enable a user account based on the username
		/// </summary>
		/// <param name="UserName"></param>
		public void EnableUserAccount(string UserName)
		{
			//get the directory entry fot eh user and enable the password
			EnableUserAccount(GetUser(UserName));
		}

        /// <summary>
        /// Enable the user
        /// </summary>
        /// <param name="oDE"></param>
		public void EnableUserAccount(DirectoryEntry oDE)
		{
			//we enable the account by resetting all the account options excluding the disable flag
            oDE.Properties["userAccountControl"][0] = ADAccountOptions.UF_NORMAL_ACCOUNT | ADAccountOptions.UF_DONT_EXPIRE_PASSWD;
			oDE.CommitChanges();

//			oDE.Invoke("accountDisabled",new Object[]{"false"});
			oDE.Close();
		}
        
		/// <summary>
		/// This will disable the user account based on the username passed to it
		/// </summary>
        /// <param name="userName"></param>
		public void DisableUserAccount(string userName)
		{
			//get the directory entry fot eh user and enable the password
			DisableUserAccount(GetUser(userName));
		}
        
		/// <summary>
		/// Enable the user account based on the DirectoryEntry object passed to it
		/// </summary>
		/// <param name="oDE"></param>
		public void DisableUserAccount(DirectoryEntry oDE)
		{
			//we disable the account by resetting all the default properties
			oDE.Properties["userAccountControl"][0] = ADAccountOptions.UF_NORMAL_ACCOUNT|ADAccountOptions.UF_DONT_EXPIRE_PASSWD|ADAccountOptions.UF_ACCOUNTDISABLE;
			oDE.CommitChanges();
//			oDE.Invoke("accountDisabled",new Object[]{"true"});
			oDE.Close();
		}

		/// <summary>
		/// Override method for adding a user to a group.  The group will be specified
		/// so that a group object can be located, then the user will be queried and added to the group
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="GroupName"></param>
		public void AddUserToGroup(string UserName, string GroupName)
		{
			string LDAPDomain = string.Empty;
			//get reference to group
			LDAPDomain="/CN="+ GroupName+",CN=Users," + GetLDAPDomain() ;
			DirectoryEntry oGroup= GetDirectoryObject(LDAPDomain);

			//get reference to user
			LDAPDomain="/CN="+ UserName +",CN=Users," + GetLDAPDomain() ;
			DirectoryEntry oUser= GetDirectoryObject(LDAPDomain);
			
			//Add the user to the group via the invoke method
			oGroup.Invoke("Add",new Object[]{oUser.Path.ToString()});

			oGroup.Close();
			oUser.Close();
		}

        /// <summary>
        /// Remove the user from the given group
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="GroupName"></param>
		public void RemoveUserFromGroup(string UserName, string GroupName)
		{
			string LDAPDomain = string.Empty;

			//get reference to group
			LDAPDomain="/CN="+ GroupName+",CN=Users," + GetLDAPDomain() ;
			DirectoryEntry oGroup= GetDirectoryObject(LDAPDomain);

			//get reference to user
			LDAPDomain="/CN="+ UserName +",CN=Users," + GetLDAPDomain() ;
			DirectoryEntry oUser= GetDirectoryObject(LDAPDomain);
			
			//Add the user to the group via the invoke method
			oGroup.Invoke("Remove",new Object[]{oUser.Path.ToString()});

			oGroup.Close();
			oUser.Close();
		}

		#endregion

		#region Helper Methods
		/// <summary>
		/// This will retreive the specified poperty value from the DirectoryEntry object (if the property exists)
		/// </summary>
		/// <param name="oDE"></param>
		/// <param name="PropertyName"></param>
		/// <returns></returns>
		public string GetProperty(DirectoryEntry oDE, string PropertyName)
		{
			if(oDE.Properties.Contains(PropertyName))
			{
				return oDE.Properties[PropertyName][0].ToString() ;
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// This is an override that will allow a property to be extracted directly from
		/// a searchresult object
		/// </summary>
		/// <param name="searchResult"></param>
		/// <param name="PropertyName"></param>
		/// <returns></returns>
		public string GetProperty(SearchResult searchResult, string PropertyName)
		{
			if(searchResult.Properties.Contains(PropertyName))
			{
				return searchResult.Properties[PropertyName][0].ToString() ;
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// This will test the value of the propertyvalue and if empty will not set the property
		/// as AD is particular about being sent blank values
		/// </summary>
        /// <param name="de"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
		public void SetProperty(DirectoryEntry de, string propertyName, string propertyValue)
		{
			//check if the value is valid, otherwise dont update
            if (propertyValue != string.Empty)
			{
				//check if the property exists before adding it to the list
                if (de.Properties.Contains(propertyName))
				{
                    de.Properties[propertyName][0] = propertyValue;
                    de.CommitChanges();
				}
				else
				{
                    de.Properties[propertyName].Add(propertyValue);
                    de.CommitChanges();
				}
			}
		}

		/// <summary>
		/// This is an internal method for retreiving a new directoryentry object
		/// </summary>
		/// <returns></returns>
		private DirectoryEntry GetDirectoryObject()
		{
            return this.GetDirectoryObject(ADPath, ADServer, ADUser, ADPassword, AuthenticationTypes.Secure);
		}

		/// <summary>
		/// Override function that that will attempt a logon based on the users credentials
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		private DirectoryEntry GetDirectoryObject(string UserName, string Password)
		{
			return this.GetDirectoryObject(ADPath, ADServer, UserName, Password, AuthenticationTypes.Secure);
		}

		/// <summary>
		/// This will create the directory entry based on the domain object to return
		/// The DomainReference will contain the qualified syntax for returning an entry
		/// at the location rather than returning the root.  
		/// i.e. /CN=Users,DC=creditsights, DC=cyberelves, DC=Com
		/// </summary>
		/// <param name="DomainReference"></param>
		/// <returns></returns>
		private DirectoryEntry GetDirectoryObject(string DomainReference)
		{
			return this.GetDirectoryObject(ADPath, DomainReference, ADUser, ADPassword, AuthenticationTypes.Secure);
		}

		/// <summary>
		/// Addition override that will allow ovject to be created based on the users credentials.
		/// This is useful for instances such as setting password etc.
		/// </summary>
		/// <param name="DomainReference"></param>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		private DirectoryEntry GetDirectoryObject(string DomainReference, string UserName, string Password)
		{
            return this.GetDirectoryObject(ADPath, DomainReference, UserName, Password, AuthenticationTypes.Secure);
		}

        private DirectoryEntry GetDirectoryObject(string adPath, string adServer, string userName, string password, AuthenticationTypes securityLevel)
        {
            DirectoryEntry oDE;
                        
            oDE = new DirectoryEntry(adPath + adServer, userName, password, securityLevel);

            // Test to see if current logged in user gets used if a username is not specified
            //oDE = new DirectoryEntry(adPath + adServer);

            return oDE;
        }


		#endregion

		#region Internal Methods
		/// <summary>
		/// This method will create a new directory object and pass it back so that
		/// it can be populated
		/// </summary>
		/// <param name="cn"></param>
		/// <returns></returns>
		public DirectoryEntry  CreateNewUser(string cn)
		{
			//set the LDAP qualification so that the user will be created under the Users
			//container
			string LDAPDomain ="/CN=Users," + GetLDAPDomain() ;
			DirectoryEntry oDE= GetDirectoryObject(LDAPDomain);
			DirectoryEntry oDEC=oDE.Children.Add("CN=" + cn,"User");
			oDE.Close();
			return oDEC;

		}

		/// <summary>
		/// This will read in the ADServer value from the web.config and will return it
		/// as an LDAP path ie DC=creditsights, DC=cyberelves, DC=com.
		/// This is required when creating directoryentry other than the root.
		/// </summary>
		/// <returns></returns>
		private string GetLDAPDomain()
		{
			StringBuilder LDAPDomain = new StringBuilder();
			string[] LDAPDC = ADServer.Split('.');
			
			for(int i=0;i < LDAPDC.GetUpperBound(0)+1;i++)
			{
				LDAPDomain.Append("DC="+LDAPDC[i]);
				if(i <LDAPDC.GetUpperBound(0))
				{
					LDAPDomain.Append(",");
				}
			}

			return LDAPDomain.ToString();
		}


		/// <summary>
		/// This method will create a Dataset stucture containing all relevant fields
		/// that match to a user.
		/// </summary>
		/// <returns></returns>
		private DataSet CreateUserDataSet()
		{

			DataSet ds = new DataSet();
			//Create a new table object within the dataset
			DataTable tb = ds.Tables.Add("User");

			//Create all the columns
			tb.Columns.Add("LoginName");
			tb.Columns.Add("FirstName");
			tb.Columns.Add("MiddleInitial");
			tb.Columns.Add("LastName");
			tb.Columns.Add("Address1");
			tb.Columns.Add("Address2");
			tb.Columns.Add("Title");
			tb.Columns.Add("Company");
			tb.Columns.Add("City");
			tb.Columns.Add("State");
			tb.Columns.Add("Country");
			tb.Columns.Add("Zip");
			tb.Columns.Add("Phone");
			tb.Columns.Add("Extension");
			tb.Columns.Add("Fax");
			tb.Columns.Add("EmailAddress");
			tb.Columns.Add("ChallengeQuestion");
			tb.Columns.Add("ChallengeResponse");
			tb.Columns.Add("MemberCompany");
			tb.Columns.Add("CompanyRelationShipExists");
			tb.Columns.Add("Status");
			tb.Columns.Add("AssignedSalesPerson");
			tb.Columns.Add("AcceptTAndC");
			tb.Columns.Add("Jobs");
			tb.Columns.Add("Email_Overnight");
			tb.Columns.Add("Email_DailyEmergingMarkets");
			tb.Columns.Add("Email_DailyCorporateAlerts");
			tb.Columns.Add("AssetMgtRange");
			tb.Columns.Add("ReferralCompany");
			tb.Columns.Add("CorporateAffiliation");
			tb.Columns.Add("DateCreated");
			tb.Columns.Add("DateLastModified");
			tb.Columns.Add("DateOfExpiry");
			tb.Columns.Add("AccountIsActive");
		
			return ds;
		}

		/// <summary>
		/// This method will return a DataRow object which will be added to the userdataset object
		/// This will also allow the iteration of multiple rows
		/// </summary>
		/// <param name="userSearchResult"></param>
        /// /// <param name="userTable"></param>
		/// <returns></returns>
		private DataRow PopulateUserDataSet(SearchResult userSearchResult, DataTable userTable)
		{
			//set a new empty row
			DataRow rwUser = userTable.NewRow();

			rwUser["LoginName"]=GetProperty(userSearchResult,"cn");		
			rwUser["FirstName"]=GetProperty(userSearchResult,"givenName");
			rwUser["MiddleInitial"]=GetProperty(userSearchResult,"initials");
			rwUser["LastName"]=GetProperty(userSearchResult,"sn");

			string tempAddress =GetProperty(userSearchResult,"homePostalAddress");
			//if the address does not exist, then default to blank fields
			if(tempAddress !=string.Empty)
			{
				string[] addressArray = tempAddress.Split(';');
				rwUser["Address1"]=addressArray[0];
				rwUser["Address2"]=addressArray[1];
			}
			else
			{
				rwUser["Address1"]=string.Empty;
				rwUser["Address2"]=string.Empty;
			}

			rwUser["Title"]=GetProperty(userSearchResult,"title");
			rwUser["Company"]=GetProperty(userSearchResult,"company");
			rwUser["State"]=GetProperty(userSearchResult,"st");
			rwUser["City"]=GetProperty(userSearchResult,"l");
			rwUser["Country"]=GetProperty(userSearchResult,"co");
			rwUser["Zip"]=GetProperty(userSearchResult,"postalCode");
			rwUser["Phone"]=GetProperty(userSearchResult,"telephoneNumber");
			rwUser["Extension"]=GetProperty(userSearchResult,"otherTelephone");
			rwUser["Fax"]=GetProperty(userSearchResult,"facsimileTelephoneNumber");
			rwUser["EmailAddress"]=GetProperty(userSearchResult,"mail");
			rwUser["ChallengeQuestion"]=GetProperty(userSearchResult,"extensionAttribute1");
			rwUser["ChallengeResponse"]=GetProperty(userSearchResult,"extensionAttribute2");
			rwUser["MemberCompany"]=GetProperty(userSearchResult,"extensionAttribute3");
			rwUser["CompanyRelationShipExists"]=GetProperty(userSearchResult,"extensionAttribute4");
			rwUser["Status"]=GetProperty(userSearchResult,"extensionAttribute5");
			rwUser["AssignedSalesPerson"]=GetProperty(userSearchResult,"extensionAttribute6");
			rwUser["AcceptTAndC"]=GetProperty(userSearchResult,"extensionAttribute7");
			rwUser["Jobs"]=GetProperty(userSearchResult,"extensionAttribute8");
			
			//handle the split of the email options
			string tempTempEmail =GetProperty(userSearchResult,"extensionAttribute9");
			
			//if no email address are present, then default to blank
			if(tempTempEmail !=string.Empty)
			{
				string[] emailArray = tempTempEmail.Split(';');
				rwUser["Email_Overnight"]=emailArray[0];
				rwUser["Email_DailyEmergingMarkets"]=emailArray[1];
				rwUser["Email_DailyCorporateAlerts"]=emailArray[2];
			}
			else 
			{
				rwUser["Email_Overnight"]="false";
				rwUser["Email_DailyEmergingMarkets"]="false";
				rwUser["Email_DailyCorporateAlerts"]="false";
			}

			rwUser["AssetMgtRange"]=GetProperty(userSearchResult,"extensionAttribute10");
			rwUser["ReferralCompany"]=GetProperty(userSearchResult,"extensionAttribute11");
			rwUser["CorporateAffiliation"]=GetProperty(userSearchResult,"extensionAttribute12");
			rwUser["DateCreated"]=GetProperty(userSearchResult,"whenCreated");
			rwUser["DateLastModified"]=GetProperty(userSearchResult,"whenChanged");
			rwUser["DateOfExpiry"]=GetProperty(userSearchResult,"extensionAttribute12");
			rwUser["AccountIsActive"]=IsAccountActive(Convert.ToInt32(GetProperty(userSearchResult,"userAccountControl")));
			return rwUser;
				
		}
		#endregion

        #region Processing Methods

        /// <summary>
        /// This method will prompt the user for a .csv file containing a list of users to process
        /// It will lookup each user in Active Directory to get their username and email address
        /// It will generate a new strong password for each user
        /// and save all three pieces of info to a new .csv specified by the user
        /// </summary>
        public void GenerateNewPasswords(Stream inStream, Stream outStream)
        {            
            // Generate a new password and put it in the password box
            PasswordGenerator PwdGen = new PasswordGenerator();
            PwdGen.Minimum = 8;
            PwdGen.Maximum = 8;
            PwdGen.ConsecutiveCharacters = false;
            PwdGen.RepeatCharacters = false;
            
            try 
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader and StreamWriter.
                using (StreamReader sr = new StreamReader(inStream)) 
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(outStream))
                        {
                            String line;
                            char[] ca ={ ',' };

                            // Read and process lines from the file until the end of 
                            // the file is reached.
                            while ((line = sr.ReadLine()) != null)
                            {
                                try
                                {
                                    // Get the user's Account Name from Active Directory
                                    string[] sa = line.Split(ca, 4);
                                    string NameExpr = sa[0].ToString().Trim().Replace("User Not Found: ", "").Replace("Too many users found for: ", "");
                                    string UserName = this.GetUserAccountName(NameExpr).ToString().Trim();
                                    string Email = this.GetUserEmail(NameExpr).ToString().Trim();
                                    string Phone = this.GetUserPhone(NameExpr).ToString().Trim();
                                    string Office = this.GetUserOffice(NameExpr).ToString().Trim();
                                    string Company = this.GetUserCompany(NameExpr).ToString().Trim();
                                    string Enabled = this.IsUserEnabled(NameExpr).ToString().Trim();
                                    string Password = PwdGen.Generate();
                                    sw.WriteLine(UserName + "," + Password + "," + Email + "," + Phone + "," + Enabled + "," + Company + "," + Office);
                                }
                                catch (Exception eSplit)
                                {
                                    Console.WriteLine(eSplit.Message + "\r\n" + line);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Let the user know what went wrong.
                        Console.WriteLine("The output file could not be opened:");
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e) 
            {
                // Let the user know what went wrong.
                Console.WriteLine("The input file could not be opened:");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Retrieve all information in Active Directory for the given user
        /// </summary>
        /// <param name="inStream"></param>
        /// <param name="outStream"></param>
        public void GetAllUserInfo(Stream inStream, Stream outStream)
        {
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader and StreamWriter.
                using (StreamReader sr = new StreamReader(inStream))
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(outStream))
                        {
                            String line;
                            char[] ca ={ ',' };

                            // Read and process lines from the file until the end of 
                            // the file is reached.
                            while ((line = sr.ReadLine()) != null)
                            {
                                try
                                {
                                    // Get the user's Account Name from Active Directory
                                    string[] sa = line.Split(ca, 4);
                                    string NameExpr = sa[0].ToString().Trim().Replace("User Not Found: ", "").Replace("Too many users found for: ", "");
                                    string UserName = this.GetUserAccountName(NameExpr).ToString().Trim();
                                    string Email = this.GetUserEmail(NameExpr).ToString().Trim();
                                    string Phone = this.GetUserPhone(NameExpr).ToString().Trim();
                                    string Office = this.GetUserOffice(NameExpr).ToString().Trim();
                                    string Company = this.GetUserCompany(NameExpr).ToString().Trim();
                                    string Enabled = this.IsUserEnabled(NameExpr).ToString().Trim();
                                    string Password = sa[1].ToString().Trim();
                                    sw.WriteLine(UserName + "," + Password + "," + Email + "," + Phone + "," + Enabled + "," + Company + "," + Office);
                                }
                                catch (Exception eSplit)
                                {
                                    Console.WriteLine(eSplit.Message + "\r\n" + line);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Let the user know what went wrong.
                        Console.WriteLine("The output file could not be opened:");
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The input file could not be opened:");
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}
