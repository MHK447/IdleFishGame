using System.Collections.Generic;
using UnityEditor;

namespace RotaryHeart.Lib
{
    public static class Definer
    {
        public static void ApplyDefines(List<string> defines)
        {
            if (defines == null || defines.Count == 0)
                return;

            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (targetGroup == BuildTargetGroup.Unknown)
            {
                UnityEngine.Debug.LogWarning("ApplyDefines: BuildTargetGroup is Unknown. Skipping define.");
                return;
            }

            string availableDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            List<string> definesSplit = new List<string>(availableDefines.Split(';'));

            foreach (string define in defines)
                if (!definesSplit.Contains(define))
                    definesSplit.Add(define);

            _ApplyDefine(targetGroup, string.Join(";", definesSplit.ToArray()));
        }

        public static void RemoveDefines(List<string> defines)
        {
            if (defines == null || defines.Count == 0)
                return;

            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (targetGroup == BuildTargetGroup.Unknown)
            {
                UnityEngine.Debug.LogWarning("RemoveDefines: BuildTargetGroup is Unknown. Skipping define.");
                return;
            }

            string availableDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            List<string> definesSplit = new List<string>(availableDefines.Split(';'));

            foreach (string define in defines)
                definesSplit.Remove(define);

            _ApplyDefine(targetGroup, string.Join(";", definesSplit.ToArray()));
        }

        static void _ApplyDefine(BuildTargetGroup targetGroup, string define)
        {
            if (string.IsNullOrEmpty(define))
                return;

            if (targetGroup == BuildTargetGroup.Unknown)
            {
                UnityEngine.Debug.LogWarning("_ApplyDefine: BuildTargetGroup is Unknown. Skipping define.");
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, define);
        }
    }
}
