using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class ATTPostProcessBuild
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddAppTrackingTransparencyFramework(pathToBuiltProject);
        }
    }

    private static void AddAppTrackingTransparencyFramework(string pathToBuiltProject)
    {
        // Info.plist 파일 경로
        string plistPath = pathToBuiltProject + "/Info.plist";
        
        // PBXProject 파일 경로
        string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        
        // PBXProject 수정
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        
#if UNITY_2019_3_OR_NEWER
        string target = proj.GetUnityMainTargetGuid();
        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();
#else
        string target = proj.TargetGuidByName("Unity-iPhone");
        string frameworkTarget = target;
#endif

        // AppTrackingTransparency 프레임워크 추가
        proj.AddFrameworkToProject(target, "AppTrackingTransparency.framework", true);
        proj.AddFrameworkToProject(frameworkTarget, "AppTrackingTransparency.framework", true);
        
        // AdSupport 프레임워크도 추가 (IDFA 관련)
        proj.AddFrameworkToProject(target, "AdSupport.framework", false);
        proj.AddFrameworkToProject(frameworkTarget, "AdSupport.framework", false);

        // 변경사항 저장
        File.WriteAllText(projPath, proj.WriteToString());
        
        Debug.Log("AppTrackingTransparency 프레임워크가 Xcode 프로젝트에 추가되었습니다.");
        
        // Info.plist에 NSUserTrackingUsageDescription 추가
        AddTrackingUsageDescription(plistPath);
    }
    
    private static void AddTrackingUsageDescription(string plistPath)
    {
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        
        PlistElementDict rootDict = plist.root;
        
        // ATT 사용 목적 설명 추가
        rootDict.SetString("NSUserTrackingUsageDescription", 
            "이 앱은 광고 개인화 및 더 나은 사용자 경험을 제공하기 위해 사용자 활동을 추적합니다.");
        
        // 변경사항 저장
        File.WriteAllText(plistPath, plist.WriteToString());
        
        Debug.Log("NSUserTrackingUsageDescription이 Info.plist에 추가되었습니다.");
    }
} 