using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;

namespace AuthenticationTokenServiceAPI.Extensions
{
    public static class DataDictionaryExtensions
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> Converters
        = new ConcurrentDictionary<Type, TypeConverter>();
        private static readonly IDictionary<Type, Func<string, object?>> StringConverters = new Dictionary<Type, Func<string, object?>>() {
            { typeof(Guid), str=>  Guid.TryParse(str, out Guid value) ? value: default(Guid) },
            { typeof(Guid?), str=>  Guid.TryParse(str, out Guid value) ? value: default(Guid?) },
            { typeof(DateTime), str=>  DateTime.TryParse(str, out DateTime value) ? value: default(DateTime) },
            { typeof(DateTime?), str=>  DateTime.TryParse(str, out DateTime value) ? value: default(DateTime?) },
            { typeof(TimeSpan), str=>  TimeSpan.TryParse(str, out TimeSpan value) ? value: default(TimeSpan) },
            { typeof(TimeSpan?), str=>  TimeSpan.TryParse(str, out TimeSpan value) ? value: default(TimeSpan?) },
            { typeof(byte), str=>  byte.TryParse(str, out byte value) ? value: default(byte) },
            { typeof(byte?), str=>  byte.TryParse(str, out byte value) ? value: default(byte?) },
            { typeof(sbyte), str=>  sbyte.TryParse(str, out sbyte value) ? value: default(sbyte) },
            { typeof(sbyte?), str=>  sbyte.TryParse(str, out sbyte value) ? value: default(sbyte?) },
            { typeof(short), str=>  short.TryParse(str, out short value) ? value: default(short) },
            { typeof(short?), str=>  short.TryParse(str, out short value) ? value: default(short?) },
            { typeof(ushort), str=>  ushort.TryParse(str, out ushort value) ? value: default(ushort) },
            { typeof(ushort?), str=>  ushort.TryParse(str, out ushort value) ? value: default(ushort?) },
            { typeof(int), str=>  int.TryParse(str, out int value) ? value: default(int) },
            { typeof(int?), str=>  int.TryParse(str, out int value) ? value: default(int?) },
            { typeof(uint), str=>  uint.TryParse(str, out uint value) ? value: default(uint) },
            { typeof(uint?), str=>  uint.TryParse(str, out uint value) ? value: default(uint?) },
            { typeof(long), str=>  long.TryParse(str, out long value) ? value: default(long) },
            { typeof(long?), str=>  long.TryParse(str, out long value) ? value: default(long?) },
            { typeof(ulong), str=>  ulong.TryParse(str, out ulong value) ? value: default(ulong) },
            { typeof(ulong?), str=>  ulong.TryParse(str, out ulong value) ? value: default(ulong?) },
            { typeof(float), str=>  float.TryParse(str, out float value) ? value: default(float) },
            { typeof(float?), str=>  float.TryParse(str, out float value) ? value: default(float?) },
            { typeof(double), str=>  double.TryParse(str, out double value) ? value: default(double) },
            { typeof(double?), str=>  double.TryParse(str, out double value) ? value: default(double?) },
            { typeof(decimal), str=>  decimal.TryParse(str, out decimal value) ? value: default(decimal) },
            { typeof(decimal?), str=>  decimal.TryParse(str, out decimal value) ? value: default(decimal?) },
            { typeof(char), str=>  char.TryParse(str, out char value) ? value: default(char) },
            { typeof(char?), str=>  char.TryParse(str, out char value) ? value: default(char?) },
            { typeof(bool), str=>  bool.TryParse(str, out bool value) ? value: default(bool) },
            { typeof(bool?), str=>  bool.TryParse(str, out bool value) ? value: default(bool?) },
            { typeof(MailAddress), str =>  {
                try
                {
                    return new MailAddress(str);
                }
                catch
                {
                    return null;
                }
            } }
        };
        private static T? ChangeType<T>(object value)
        {
            if (typeof(T?).IsPrimitive)
            {
                return (T?)ResolveTypeConverter<T>().ConvertFrom(value);
            }
            else if (value is string strValue)
            {
                try
                {
                    object? convertedValue = StringConverters.TryGet(typeof(T?), out Func<string, object?>? converter) && converter != null ? converter(strValue) :
                    Convert.ChangeType(value, typeof(T));
                    return (T?)convertedValue;
                }
                catch
                {
                    return default(T);
                }

            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        private static TypeConverter ResolveTypeConverter<T>()
        {
            if (Converters.TryGetValue(typeof(T), out TypeConverter? tc) && tc != null)
            {
                return tc;
            }
            TypeConverter newTC = TypeDescriptor.GetConverter(typeof(T));
            Converters.TryAdd(typeof(T), newTC);
            return newTC;
        }
        public static T? Parse<T>(object value, T? defaultValue = default)
        {
            if (value == null)
            {
                return default(T?);
            }
            try
            {
                Type returnType = typeof(T?);
                Type? underlyingType = Nullable.GetUnderlyingType(returnType);
                Type castType = underlyingType != null ? underlyingType : returnType;
                if (value is T tVal)
                {
                    return tVal;
                }
                if (castType == typeof(string))
                {
                    return (T?)(object?)value.ToString();
                }
                else if (value is string strVal && int.TryParse(strVal, out int intVal) && IsEnum(castType, intVal))
                {
                    return (T)Enum.ToObject(castType, intVal);
                }
                return IsEnum(castType, value) && value is int intValue ?
                    (T)Enum.ToObject(castType, intValue) :
                    ChangeType<T>(value);
            }
            catch
            {
                return defaultValue;
            }
        }
        private static string? GetSettingName(this IDictionary<string, object> data, string key)
            => data.ContainsKey(key) ? key :
            data.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.CurrentCultureIgnoreCase));
        private static bool IsEnum(Type enumType, object value)
        {
            try
            {
                return Enum.IsDefined(enumType, value);
            }
            catch
            {
                return false;
            }
        }
        public static T? GetSetting<T>(this IDictionary<string, object> data, string key, Func<string, T?> convert, T? defaultValue = default)
            => data.GetSetting<T, string>(key, convert, defaultValue);
        public static T? GetSetting<T, TParse>(this IDictionary<string, object> data, string key, Func<TParse, T?> convert, T? defaultValue = default)
            where TParse : IComparable
        {
            TParse? defaultParseValue = default(TParse?);
            TParse? parseValue = data.GetSetting(key, defaultParseValue);
            return parseValue == null || parseValue.CompareTo(defaultParseValue) == 0 ? defaultValue : convert(parseValue);
        }
        public static T? GetSetting<T>(this IDictionary<string, object> data, string key, T? defaultValue = default)
        {
            string? name = data.GetSettingName(key);

            return name != null &&
                data.TryGetValue(name, out object? value) &&
                value != null &&
                value != DBNull.Value ?
                Parse(value, defaultValue) : defaultValue;
        }

        public static DateTime? GetDateTimeSetting(this IDictionary<string, object> data, string key, string format, DateTimeStyles style = DateTimeStyles.None)
            => data.GetDateTimeSetting(key, format, CultureInfo.InvariantCulture, style);
        public static DateTime? GetDateTimeSetting(this IDictionary<string, object> data, string key, string format, IFormatProvider provider, DateTimeStyles style = DateTimeStyles.None)
        => data.GetSetting(key, value =>
            !string.IsNullOrWhiteSpace(value) &&
            DateTime.TryParseExact(value, format, provider, style, out DateTime outValue) ? outValue : (DateTime?)null
            );
        public static string? GetFormattedDateTimeStringSetting(this IDictionary<string, object> data, string key, string format)
        => data.GetSetting<DateTime?>(key) is DateTime convertedValue ? convertedValue.ToString(format) : null;
        public static IEnumerable<T?>? GetEnumerableSetting<T>(this IDictionary<string, object> data, string key, string delimeter = ",")
        => data.GetSetting(key, value => value.Split(delimeter.ToArray()).Select(v => Parse<T>(v)));
        public static bool GetFlagSetting<T>(this IDictionary<string, object> data, string key, T trueValue)
            where T : IComparable
        => data.GetSetting<T>(key) is T val && val.CompareTo(trueValue) == 0;

        public static bool TryAdd(this HybridDictionary dictionary, string key, object value)
        {
            try
            {
                dictionary.Add(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryGet(this HybridDictionary dictionary, string key, out object? value)
        {
            try
            {
                value = dictionary[key];
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
        public static bool TrySet(this HybridDictionary dictionary, string key, object value)
        {
            try
            {
                if (!dictionary.Contains(key))
                {
                    return dictionary.TryAdd(key, value);
                }
                dictionary[key] = value;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            try
            {
                dictionary.Add(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue? value)
        {
            try
            {
                value = dictionary[key];
                return true;
            }
            catch
            {
                value = default(TValue);
                return false;
            }
        }
        public static bool TrySet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            try
            {
                if (!dictionary.ContainsKey(key))
                {
                    return dictionary.TryAdd(key, value);
                }
                dictionary[key] = value;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static IDictionary<string, object> ToDataDictionary(this NameValueCollection collection)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (string? key in collection.AllKeys)
            {
                if (key is string k && collection[k] is object obj)
                {
                    dictionary[k] = obj;
                }
            }
            return dictionary;
        }
    }
}
