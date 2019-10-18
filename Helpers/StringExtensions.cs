using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace IseAddons {
    //________________________________________________________________________________________________________________________________________________________________________________
    public static class StringExtensions {
        //________________________________________________________________________________________________________________________________________________________________________________
        public static bool cEquals(this string value, string otherValue) { return value.Equals(otherValue, StringComparison.Ordinal); }
        public static bool iEquals(this string value, string otherValue) { return value.Equals(otherValue, StringComparison.OrdinalIgnoreCase); }
        public static int iIndexOf(this string value, string otherValue) { return value.IndexOf(otherValue, StringComparison.OrdinalIgnoreCase); }
        public static bool iStartsWith(this string value, string otherValue) { return value.StartsWith(otherValue, StringComparison.OrdinalIgnoreCase); }
        //_____________________________________________________________________________________________________________________________________________________________
        public static string RemoveInvalidChars(this string value) {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid) value = value.Replace(c.ToString(), "");
            return value.Trim().Replace(" ", "");
            }

        //________________________________________________________________________________________________________________________________________________________________________________
        public static T secParse<T>(this string value) where T : struct {
            T _authenticationType = default(T);
            if (string.IsNullOrWhiteSpace(value)) return _authenticationType;
            string _value = value.Replace("|", ", ");
            if (Enum.TryParse<T>(_value, out _authenticationType))
                if (Enum.IsDefined(typeof(T), _authenticationType) | _authenticationType.ToString().Contains(","))
                    return _authenticationType;
            throw new InvalidEnumArgumentException("Not possible to parse [" + value + "] to [" + typeof(T) + "]");
            }
        //________________________________________________________________________________________________________________________________________________________________________________
        public static DataTable ConvertToDataTable<T>(this IList<T> data, List<string> allowedAttributes = null) {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            if (allowedAttributes == null)
                foreach (PropertyDescriptor prop in properties) table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            else
                foreach (string k in allowedAttributes) {
                    PropertyDescriptor prop = properties.Find(k, true);
                    if(prop!=null) table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                    
            foreach (T item in data) {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    if (allowedAttributes == null || allowedAttributes.Contains(prop.Name))
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
                }
            return table;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static IEnumerable<TOut> ForEach<TIn, TOut>(this IEnumerable<TIn> source, Action2<TIn, TOut> action) {
            if (source == null) throw new ArgumentException();
            if (action == null) throw new ArgumentException();
            foreach (TIn element in source) {
                TOut result = action(element);
                yield return result;
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null) throw new ArgumentException();
            if (action == null) throw new ArgumentException();
            foreach (T obj in source) action(obj);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public delegate TOut Action2<TIn, TOut>(TIn element);
        }
    }
