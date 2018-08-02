using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using WebIt.Business.UtilityClasses;

namespace WebIt.Business
{
    /// <summary>
    /// WebIT Business class for dealing with data access
    /// </summary>
    public class wiBusinessObject
    {
        #region Properties

        /// <summary>
        /// SQL Statement
        /// </summary>
        public string Sql;

        /// <summary>
        /// Name of the connection to the database in the config file
        /// </summary>
        public string DatabaseKey;
        
        private int _CommandTimeout = 0;

        public int CommandTimeout
        {
            get { return _CommandTimeout; }
            set { _CommandTimeout = value; }
        }
        
        /// <summary>
        /// (Optional Path and) Name of the file for this class to log messages to
        /// </summary>
        public string LogFile
        {
            get
            {
                // If the Error log has not been set on this class, look up the name in the config file
                if (String.IsNullOrEmpty(_LogFile) == true)
                {
                    string LogFileName = AppSettingsMgr.GetSetting("LogFileName");
                    if (!String.IsNullOrEmpty(LogFileName))
                        _LogFile = LogFileName;
                    else
                        _LogFile = "Unknown.log";
                }
                return _LogFile;
            }
            set { _LogFile = value; }
        }
        private string _LogFile = string.Empty;

        #endregion

        /// <summary>
        /// wiBusinessObject constructor
        /// </summary>
        public wiBusinessObject()
        {
        }

        #region Execution methods

        /// <summary>
        /// Executes a SQL query, using the given SQL parameters and returns the number of rows affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataParams"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            int Rows;
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                SqlCommand Command = new SqlCommand(sql, Conn);
                Command.CommandTimeout = this.CommandTimeout;
                Rows = Command.ExecuteNonQuery();    
            }

            return Rows;
        }

        /// <summary>
        /// Executes a SQL query, using the given SQL parameters and returns the number of rows affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataParams"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params IDbDataParameter[] dataParams)
        {
            int Rows;
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                SqlCommand Command = new SqlCommand(sql, Conn);
                Command.CommandTimeout = this.CommandTimeout;

                // Add any parameters to the Command object
                if (dataParams != null)
                {
                    foreach (IDbDataParameter param in dataParams)
                    {
                        Command.Parameters.Add(param);
                    }
                }

                Rows = Command.ExecuteNonQuery();
            }

            return Rows;
        }

        /// <summary>
        /// Executes a SQL query, using the given SQL parameters and returns a single value
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataParams"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            object Result;
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                SqlCommand Command = new SqlCommand(sql, Conn);
                Command.CommandTimeout = this.CommandTimeout;
                Result = Command.ExecuteScalar();
            }
            return Result;
        }

        /// <summary>
        /// Executes a SQL query, using the given SQL parameters and returns a single value
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataParams"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params IDbDataParameter[] dataParams)
        {
            object Result;
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                SqlCommand Command = new SqlCommand(sql, Conn);
                Command.CommandTimeout = this.CommandTimeout;

                // Add any parameters to the Command object
                if (dataParams != null)
                {
                    foreach (IDbDataParameter param in dataParams)
                    {
                        Command.Parameters.Add(param);
                    }
                }

                Result = Command.ExecuteScalar();
            }
            return Result;
        }

        /// <summary>
        /// Executes the specified stored procedure, passing the specified parameters
        /// and returning a scalar value
        /// </summary>
        /// <param name="sprocName">Stored procedure name</param>
        /// <param name="dataParams">Parameters</param>
        /// <returns>Sproc return value</returns>
        public virtual object ExecSprocScalar(string sprocName, params IDbDataParameter[] dataParams)
        {
            object Result;
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                SqlCommand Command = new SqlCommand(sprocName, Conn);
                //Command.CommandText = sprocName;
                Command.CommandType = CommandType.StoredProcedure;
                Command.CommandTimeout = this.CommandTimeout;

                // Add any parameters to the Command object
                if (dataParams != null)
                {
                    foreach (IDbDataParameter param in dataParams)
                    {
                        Command.Parameters.Add(param);
                    }
                }

                Result = Command.ExecuteScalar();
            }
            return Result;
        }

        /// <summary>
        /// Configures the stored procedure for execution
        /// </summary>
        /// <param name="sprocName">Stored procedure name</param>
        /// <param name="dataParams">Parameters</param>
        /// <returns>Command object</returns>
        protected virtual IDbCommand ConfigureSproc(string sprocName, params IDbDataParameter[] dataParams)
        {
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                IDbCommand cmd = Conn.CreateCommand();
                cmd.CommandText = sprocName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = this.CommandTimeout;
                if (dataParams != null)
                {
                    foreach (IDbDataParameter parm in dataParams)
                    {
                        cmd.Parameters.Add(parm);
                    }
                }
                Conn.Close();
                return cmd;
            }
        }

        /// <summary>
        /// Returns a dataset with the given name, using the given SQL statements
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, string datasetName)
        {
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                DataSet ds = new DataSet();
                SqlDataAdapter Adapter = new SqlDataAdapter(sql, Conn);
                Adapter.Fill(ds, datasetName);
                Conn.Close();

                return ds;
            }
        }

        /// <summary>
        /// Returns a dataset with the given name, using the given SQL statements
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="datasetName"></param>
        /// <param name="dataParams"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, string datasetName, params IDbDataParameter[] dataParams)
        {
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                DataSet ds = new DataSet();

                SqlDataAdapter Adapter = new SqlDataAdapter();
                SqlCommand Command = new SqlCommand(sql, Conn);
                Command.CommandTimeout = this.CommandTimeout;

                // Add any parameters to the Command object
                if (dataParams != null)
                {
                    foreach (IDbDataParameter param in dataParams)
                    {
                        Command.Parameters.Add(param);
                    }
                }

                Adapter.SelectCommand = Command;
                Adapter.Fill(ds, datasetName);
                Conn.Close();

                return ds;
            }
        }

        /// <summary>
        /// Returns a dataset, using the given SQL statements
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataParams"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, params IDbDataParameter[] dataParams)
        {
            using (SqlConnection Conn = new SqlConnection(this.GetConnectionString()))
            {
                Conn.Open();
                DataSet ds = new DataSet();

                SqlDataAdapter Adapter = new SqlDataAdapter();
                SqlCommand Command = new SqlCommand(sql, Conn);
                Command.CommandTimeout = this.CommandTimeout;

                // Add any parameters to the Command object
                if (dataParams != null)
                {
                    foreach (IDbDataParameter param in dataParams)
                    {
                        Command.Parameters.Add(param);
                    }
                }

                Adapter.SelectCommand = Command;
                Adapter.Fill(ds);
                Conn.Close();

                return ds;
            }
        }

        #endregion

        #region Excel Methods

        /// <summary>
        /// Returns a DataSet containing information from an Excel Spreadsheet by using the given SQL Statement to query it with OLEDB
        /// </summary>
        /// <param name="excelFileName"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet GetDataSetFromExcel(string excelFileName, string sql, string extendedProperties)
        {
            // Create connection string variable. Modify the "Data Source"
            // parameter as appropriate for your environment.
            String ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                "Data Source=" + excelFileName + ";" +
                "Extended Properties=Excel 8.0;" + extendedProperties;

            // Create connection object by using the preceding connection string.
            using (OleDbConnection Conn = new OleDbConnection(ConnectionString))
            {
                // Open connection with the database.
                Conn.Open();

                // Create new OleDbCommand to return data from worksheet.
                OleDbCommand CmdSelect = new OleDbCommand(sql, Conn);
                OleDbDataAdapter Adapter = new OleDbDataAdapter();
                Adapter.SelectCommand = CmdSelect;

                // Create new DataSet to hold information from the worksheet.
                DataSet ds = new DataSet();

                // Fill the DataSet with the information from the worksheet.
                Adapter.Fill(ds, "XLData");
                Conn.Close();

                return ds;
            }
        }

        #endregion
        

        private string GetConnectionString()
        {
            string ConnectionString = "";

            ConnectionStringSettings settings;
            settings = ConfigurationManager.ConnectionStrings[this.DatabaseKey];
            if (settings != null)
                ConnectionString = settings.ConnectionString;

            return ConnectionString;
        }

        /// <summary>
        /// Creates a SQL Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlParameter CreateParameter(string name, object value)
        {
            SqlParameter Param = new SqlParameter(name, value);
            return Param;
        }

        /// <summary>
        /// Return the value of the Primary Key of the last row inserted into a table from this class
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int GetLastPrimaryKey(string tableName)
        {
            int PK;
            this.Sql = "Select IDENT_CURRENT('" + tableName + "') AS PK";
            PK = Convert.ToInt32(this.ExecuteScalar(this.Sql));
            return PK;
        }

        /// <summary>
        /// Log the given error's detalied info and optional custom message to the current output
        /// Also write message to the LogFileName stored in the app's config file
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        public static void LogError(Exception e, string message = "")
        {
            string Message = Environment.NewLine + wiString.Replicate("-", 40) + Environment.NewLine
                + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()
                + (e.Message != null && e.Message != "" ? "\r\nMESSAGE: " + e.Message : "")
                + (e.Source != null && e.Source != "" ? "\r\nSOURCE: " + e.Source : "")
                + (e.TargetSite != null && e.TargetSite.ToString() != "" ? "\r\nTARGET SITE: " + e.TargetSite.ToString() : "")
                + (e.StackTrace != null && e.StackTrace != "" ? "\r\nSTACK TRACE: " + e.StackTrace : "")
                + (message != null && message != "" ? message : "")
                + "\r\n\r\n";

            Console.WriteLine(Message);

            string LogFileName = AppSettingsMgr.GetSetting("ErrorLogFileName") ?? "Error.log";
            wiString.StrToFile(Message, LogFileName, true);
        }

        /// <summary>
        /// Log the given error message to the current output
        /// Also write message to the LogFileName stored in the app's config file
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            message = Environment.NewLine + wiString.Replicate("-", 40) + Environment.NewLine + message;
            Console.WriteLine(message);

            string LogFileName = AppSettingsMgr.GetSetting("ErrorLogFileName") ?? "Error.log";
            wiString.StrToFile(message, LogFileName, true);
        }

        /// <summary>
        /// Log the given message to the current output
        /// If logToFile is true, then also write message to the LogFileName stored in the app's config file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logToFile"></param>
        public static void LogMessage(string message, bool logToFile = false)
        {
            Console.WriteLine(message);

            if (logToFile)
            {
                string LogFileName = AppSettingsMgr.GetSetting("LogFileName") ?? "Unknown.log";
                wiString.StrToFile(message, LogFileName, true);
            }
        }
    }
}
