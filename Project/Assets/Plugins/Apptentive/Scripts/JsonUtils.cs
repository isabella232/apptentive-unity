using System;
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

        public static string ToJson(IDictionary<string, object> target)
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
    }
}