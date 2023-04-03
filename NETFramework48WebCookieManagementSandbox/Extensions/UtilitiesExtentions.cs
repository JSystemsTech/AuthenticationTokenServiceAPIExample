using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NETFramework48WebCookieManagementSandbox.Extensions
{
    public static class UtilitiesExtentions
    {
        public static T ToEnum<T>(this object val, T defaultValue = default)
            where T : struct
        {
            try
            {
                return val is T enumVal ? enumVal :
                    val is int intVal ? (T)Enum.ToObject(typeof(T), intVal) :
                    val is string strVal ? (T)Enum.Parse(typeof(T), strVal, true) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        public static string AppendQueryParameter(this string url, string name, object value)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[name] = value.ToString();
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        
    }
}