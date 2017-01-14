using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace ApptentiveConnectInternal
{
    static class BuildPostProcessBuild
    {
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
    }
}
