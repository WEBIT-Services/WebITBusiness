using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebIt.Business.UtilityClasses
{
    public class wiFile
    {
        /// <summary>
        /// Generate a unique file name for the given file name
        /// </summary>
        /// <param name="pathAndFileName"></param>
        /// <returns></returns>
        public static string GetUniqueFileName(string pathAndFileName)
        {
            if (!File.Exists(pathAndFileName))
                return pathAndFileName;

            string UniqueFileName = pathAndFileName;
            string JustFile = Path.Combine(Path.GetDirectoryName(pathAndFileName), Path.GetFileNameWithoutExtension(pathAndFileName));
            string JustExt = Path.GetExtension(pathAndFileName);

            // Try 10,000 times to get a unique file name by adding an incremental number to the end
            for (int i = 0; i < 10000; i++)
            {
                UniqueFileName = JustFile + "_" + i.ToString() + JustExt;
                if (!File.Exists(UniqueFileName))
                    break;
            }

            // If no luck then just use a big ugly GUID to ensure uniqueness
            if (File.Exists(UniqueFileName))
                UniqueFileName = JustFile + "_" + Guid.NewGuid().ToString() + JustExt;

            return UniqueFileName;
        }
    }
}
