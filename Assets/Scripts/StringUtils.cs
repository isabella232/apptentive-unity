using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ApptentiveConnectInternal
{
    static class StringUtils
    {
        #region String Representation

        public static string ToString(object value)
        {
            return value != null ? value.ToString() : "null";
        }

        #endregion

        #region Serialization

        public static IDictionary<string, string> DeserializeString(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            // can't use Json here since Unity doesn't support Json-to-Dictionary deserialization
            // don't want to use 3rd party so custom format it is
            var lines = data.Split('\n');
            var dict = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                int index = line.IndexOf(':');
                string key = line.Substring(0, index);
                string value = line.Substring(index + 1, line.Length - (index + 1)).Replace(@"\n", "\n"); // restore new lines
                dict[key] = value;
            }
            return dict;
        }

        public static string SerializeString(IDictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            var index = 0;
            foreach (var e in data)
            {
                var key = e.Value;
                var value = ToString(e.Value);
                value = value.Replace("\n", @"\n"); // we use new lines as separators
                result.Append(key);
                result.Append(':');
                result.Append(value);

                if (++index < data.Count)
                {
                    result.Append("\n");
                }
            }

            return result.ToString();
        }

        #endregion
    }
}
