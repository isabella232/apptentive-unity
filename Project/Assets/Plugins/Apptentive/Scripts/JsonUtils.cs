using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace ApptentiveSDKInternal
{
    static class JsonUtils
    {
        public static object Parse(string json)
        {
            return JSON.Parse(json).RawValue;
        }

        public static string ToJson(IDictionary<string, object> dictionary)
        {
            var root = new JSONObject();
            foreach (var e in dictionary)
            {
                root.Add(e.Key, StringUtils.ToString(e.Value));
            }

            return root.ToString();
        }
    }
}