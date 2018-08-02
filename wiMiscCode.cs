using System;
using System.Data;

namespace WebIt.Business
{
    /// <summary>
    /// Business class for handling Miscellaneous Codes
    /// </summary>
    public class wiMiscCode : wiBusinessObject
    {
        /// <summary>
        /// MiscCode constructor
        /// </summary>
        public wiMiscCode()
        {
            this.DatabaseKey = "BIDPOINTE";
        }

        /// <summary>
        /// Retrieve all States
        /// </summary>
        /// <returns></returns>
        public DataSet GetStates()
        {
            this.Sql = @"SELECT CdValue as State_Cd, CdDescription as State_Descr
                            FROM MiscCode
                            WHERE CdType = 'ST'";
            return this.GetDataSet(this.Sql, "States");
        }
    }
}
