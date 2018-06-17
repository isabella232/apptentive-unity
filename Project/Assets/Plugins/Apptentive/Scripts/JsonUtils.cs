using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace ApptentiveSDKInternal
{
    static class JsonUtils
    {
        public static string ToJson(object target)
        {
            try
            {
                return JsonUtility.ToJson(target);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ToJson(IDictionary<string, object> dictionary)
        {
            var result = new StringBuilder();
            result.Append("{");
            int count = 0;
            foreach (var e in dictionary)
            {
                result.Append(Escape(e.Key));
                result.Append(":");
                result.Append(Escape(e.Value));

                if (++count < dictionary.Count)
                {
                    result.Append(",");
                }
            }
            result.Append("}");
            return result.ToString();
        }

        private static string Escape(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is bool)
            {
                return (bool) value ? "true" : "false";
            }

            if (value is sbyte ||
                value is byte ||
                value is short ||
                value is ushort ||
                value is int ||
                value is uint ||
                value is long ||
                value is ulong ||
                value is float ||
                value is double ||
                value is decimal)
            {
                return value.ToString();
            }

            return EscapeString(value.ToString());
        }

        private static string EscapeString(string value)
        {
            if (value == null) {
                return "null";
            }

            value = value.Replace("\b", "\\b");
            value = value.Replace("\f", "\\f");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\r", "\\r");
            value = value.Replace("\t", "\\t");

            return '"' + value + '"';
        }
    }
}