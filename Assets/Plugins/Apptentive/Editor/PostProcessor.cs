using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS || UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

using System.IO;

namespace ApptentiveConnectInternal
{
    static class BuildPostProcessBuild
    {
        #if UNITY_IOS || UNITY_IPHONE
        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string targetGUID = proj.TargetGuidByName("Unity-iPhone");
            proj.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC"); 
            File.WriteAllText(projPath, proj.WriteToString());
        }
        #endif
    }
}
