using UnityEditor;
using UnityEngine;

public class SimpleAnimationGenerator
{
    [MenuItem("Tools/Create Animations/Natural Breathing Animation")]
    static void CreateNaturalBreathingAnimation()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject first.");
            return;
        }

        Transform target = selected.transform;
        Vector3 baseScale = target.localScale;

        float xTarget = baseScale.x * 1.03f;
        float yTarget = baseScale.y * 1.08f;

        string path = "Assets/NaturalBreathing.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        float inhale = 1.2f;
        float pause1 = inhale + 0.3f;
        float exhale = pause1 + 1.2f;
        float pause2 = exhale + 0.3f;

        AnimationCurve scaleX = new AnimationCurve();
        scaleX.AddKey(0f, baseScale.x);
        scaleX.AddKey(new Keyframe(inhale, xTarget, 0, 0));
        scaleX.AddKey(new Keyframe(pause1, xTarget, 0, 0));
        scaleX.AddKey(new Keyframe(exhale, baseScale.x, 0, 0));
        scaleX.AddKey(new Keyframe(pause2, baseScale.x, 0, 0));

        AnimationCurve scaleY = new AnimationCurve();
        scaleY.AddKey(0f, baseScale.y);
        scaleY.AddKey(new Keyframe(inhale, yTarget, 0, 0));
        scaleY.AddKey(new Keyframe(pause1, yTarget, 0, 0));
        scaleY.AddKey(new Keyframe(exhale, baseScale.y, 0, 0));
        scaleY.AddKey(new Keyframe(pause2, baseScale.y, 0, 0));

        clip.SetCurve("", typeof(Transform), "localScale.x", scaleX);
        clip.SetCurve("", typeof(Transform), "localScale.y", scaleY);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Natural breathing animation created at {path}");
    }

    [MenuItem("Tools/Create Animations/Simple Walking Animation")]
    static void CreateWalkingAnimation()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject first.");
            return;
        }

        Transform target = selected.transform;
        Vector3 baseEuler = target.localEulerAngles;

        string path = "Assets/WalkingRotation.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        float halfCycle = 0.25f;
        float fullCycle = 0.5f;
        float maxTilt = 8f; // 좌우로 흔들리는 각도

        AnimationCurve rotZ = new AnimationCurve();
        rotZ.AddKey(0f, baseEuler.z);
        rotZ.AddKey(new Keyframe(halfCycle, baseEuler.z + maxTilt, 0, 0));
        rotZ.AddKey(new Keyframe(fullCycle, baseEuler.z, 0, 0));
        rotZ.AddKey(new Keyframe(fullCycle + halfCycle, baseEuler.z - maxTilt, 0, 0));
        rotZ.AddKey(new Keyframe(2 * fullCycle, baseEuler.z, 0, 0));

        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", rotZ);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"🚶‍♂️ Walking rotation animation created at {path}");
    }

    [MenuItem("Tools/Create Animations/Improved Walking Rotation Animation")]
    static void CreateImprovedWalkingRotation()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject first.");
            return;
        }

        Transform target = selected.transform;
        Vector3 baseEuler = target.localEulerAngles;
        float baseZ = NormalizeAngle(baseEuler.z); // e.g. 0~360 -> -180~180

        string path = "Assets/ImprovedWalkingRotation.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        // 자연스럽게 좌우로 기울어지는 형태
        float maxTilt = 6f; // 좌우로 기울어지는 최대 각도
        float halfCycle = 0.25f;
        float fullCycle = halfCycle * 2;

        // 부드럽게 좌 → 우 → 좌 순서로 1초 루프
        AnimationCurve rotZ = new AnimationCurve();
        rotZ.AddKey(new Keyframe(0f, baseZ));
        rotZ.AddKey(new Keyframe(halfCycle, baseZ + maxTilt));
        rotZ.AddKey(new Keyframe(fullCycle, baseZ));
        rotZ.AddKey(new Keyframe(fullCycle + halfCycle, baseZ - maxTilt));
        rotZ.AddKey(new Keyframe(fullCycle * 2, baseZ));

        // 각 키프레임에 부드러운 곡선 적용
        for (int i = 0; i < rotZ.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(rotZ, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(rotZ, i, AnimationUtility.TangentMode.Auto);
        }

        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", rotZ);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Improved walking rotation animation created at {path}");
    }

    // 회전 각도를 -180~180 사이로 정규화
    static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    [MenuItem("Tools/Create Animations/Nutcracker Style Walk")]
    static void CreateNutcrackerWalk()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject first.");
            return;
        }

        Transform target = selected.transform;
        Vector3 baseEuler = target.localEulerAngles;
        float baseZ = NormalizeAngle(baseEuler.z);

        string path = "Assets/NutcrackerWalk.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        // 걷는 리듬 설정
        float totalDuration = 1.2f; // 1.2초 루프
        float swingTime = totalDuration / 4f; // 왼쪽 → 중심 → 오른쪽 → 중심

        float tiltAngle = 10f; // 좌우 회전 각도

        // Z 회전 커브 생성
        AnimationCurve rotZ = new AnimationCurve();
        rotZ.AddKey(0f, baseZ);
        rotZ.AddKey(new Keyframe(swingTime, baseZ + tiltAngle));
        rotZ.AddKey(new Keyframe(swingTime * 2, baseZ));
        rotZ.AddKey(new Keyframe(swingTime * 3, baseZ - tiltAngle));
        rotZ.AddKey(new Keyframe(totalDuration, baseZ));

        // 약간의 둔한 느낌을 주기 위해 탄젠트는 살짝 딱딱하게
        for (int i = 0; i < rotZ.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(rotZ, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(rotZ, i, AnimationUtility.TangentMode.Auto);
        }

        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", rotZ);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"🪵 Nutcracker-style walk animation created at {path}");
    }

      [MenuItem("Tools/Create Animations/Headbutt Animation")]
   [MenuItem("Tools/Create Animations/Headbutt Attack")]
    static void CreateHeadbuttAttack()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject first.");
            return;
        }

        Transform target = selected.transform;
        Vector3 baseEuler = target.localEulerAngles;
        float baseX = NormalizeAngle(baseEuler.x);

        string path = "Assets/HeadbuttAttack.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        // 타이밍
        float dipTime = 0.1f;
        float holdTime = 0.12f;
        float returnTime = 0.3f;
        float endTime = 0.5f;

        float dipAngle = 45f; // 고개 숙이기 강하게

        AnimationCurve rotX = new AnimationCurve();
        rotX.AddKey(0f, baseX);                        // 초기자세
        rotX.AddKey(new Keyframe(dipTime, baseX + dipAngle));  // 확 숙임
        rotX.AddKey(new Keyframe(holdTime, baseX + dipAngle)); // 공격 유지
        rotX.AddKey(new Keyframe(returnTime, baseX));          // 복귀
        rotX.AddKey(new Keyframe(endTime, baseX));             // 마무리 정리

        for (int i = 0; i < rotX.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(rotX, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(rotX, i, AnimationUtility.TangentMode.Auto);
        }

        clip.SetCurve("", typeof(Transform), "localEulerAngles.x", rotX);

        // 단발 애니메이션
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();

        Debug.Log("💥 Headbutt attack animation created at " + path);
    }

}
