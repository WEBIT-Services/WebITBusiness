using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WebIt.Business.UtilityClasses
{
    /// <summary>
    /// String class for manipulating strings
    /// </summary>
    public class wiString
    {
        /// <summary>
        /// wiString constructor
        /// </summary>
        public wiString()
        {

        }

        /// <summary>
        /// Replicate the given expression nTimes
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="numberOfTimes"></param>
        /// <returns></returns>
        public static string Replicate(string expression, int numberOfTimes)
        {
            //Create a stringBuilder	
            StringBuilder sb = new StringBuilder();
            //Insert the expression into the StringBuilder for nTimes	
            sb.Insert(0, expression, numberOfTimes);
            //Convert it to a string and return it back	
            return sb.ToString();
        }

        /// <summary>
        /// Receives a string and the number of characters as parameters 
        ///     and returns the specified number of rightmost characters of that string
        /// </summary>
        /// <param name="cExpression"></param>
        /// <param name="nDigits"></param>
        /// <returns></returns>
        public static string Right(string cExpression, int nDigits)
        {
            return cExpression.Substring(cExpression.Length - nDigits);
        }

        /// <summary>
        /// Saves the given expression to the given file 
        /// with the option to add to the existing file or overwrite the existing file
        /// </summary>
        /// <param name="cExpression"></param>
        /// <param name="cFileName"></param>
        /// <param name="lAdditive"></param>
        public static void StrToFile(string cExpression, string cFileName, bool lAdditive = false)
        {
            //If file addition is not set, then call method that always create new file
            if (lAdditive == false)
                StrToFile(cExpression, cFileName);

            DateTime methodStart = DateTime.Now;
            bool appendCompleted = false;
            //If file write failed then try to open the file for next 60 seconds
            while (!appendCompleted && (methodStart.AddSeconds(60) > DateTime.Now))
            {
                try
                {
                    //Create the file if it does not exist and open it	
                    using (StreamWriter oWriter = File.AppendText(cFileName))
                    {
                        //Write the contents	
                        oWriter.Write(cExpression);
                        oWriter.Flush();
                        oWriter.Close();
                        appendCompleted = true;
                    }
                }
                catch
                {
                    appendCompleted = false;
                }

                if (!appendCompleted) //wait 100 ms before next attempt
                    System.Threading.Thread.Sleep(100);
            }

            //If write failed create another file with timestamp as suffix
            if (!appendCompleted)
            {
                string UniqueFileName = wiFile.GetUniqueFileName(cFileName);

                wiString.StrToFile(cExpression, UniqueFileName);
            }
        }

        /// <summary>
        /// Encodes the given Input String in a way that is valid for URLs
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string EncodeString(string data)
        {
            string encodedData = String.Empty;
            try
            {
                byte[] data_byte = Encoding.UTF8.GetBytes(data);
                encodedData = System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(data_byte));
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return encodedData;
        }

        /// <summary>
        /// Encodes the given Input String in a way that is valid for URLs
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string DecodeString(string encodedData)
        {
            string decodedData = String.Empty;
            try
            {
                byte[] data_byte = Convert.FromBase64String(System.Web.HttpUtility.UrlDecode(encodedData));
                decodedData = Encoding.UTF8.GetString(data_byte);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return decodedData;
        }
    }
}
