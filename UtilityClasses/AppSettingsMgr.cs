using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace WebIt.Business.UtilityClasses
{
    public class AppSettingsMgr
    {
        public static string GetSetting(string key)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string result = appSettings[key] ?? string.Empty;
            return result;
        }
    }
}
