using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ApptentiveSDKInternal
{
    static class ApptentiveLog
    {
        [Conditional("APPTENTIVE_DEBUG")]
        public static void d(string message)
        {
            UnityEngine.Debug.Log("[Apptentive] " + message);
        }

        [Conditional("APPTENTIVE_DEBUG")]
        public static void d(string format, params object[] args)
        {
            UnityEngine.Debug.Log("[Apptentive] " + string.Format(format, args));
        }
    }
}