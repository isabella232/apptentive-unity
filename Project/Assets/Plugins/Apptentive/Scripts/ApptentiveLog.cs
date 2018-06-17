using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ApptentiveSDKInternal
{
    static class ApptentiveLog
    {
        // [Conditional("APPTENTIVE_DEBUG")]
        public static void d(string message, params object[] args)
        {
            UnityEngine.Debug.Log("[Apptentive] " + string.Format(message, args));
        }
    }
}