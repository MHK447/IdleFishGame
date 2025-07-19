using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundPlayer))]
public class SoundPlayerInspector : Editor
{
    private SoundPlayer SoundPlayer;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        SoundPlayer = (SoundPlayer)target;


        EditorGUILayout.Space();
        EditorGUILayout.Space();

        var styleText = new GUIStyle(GUI.skin.label);
        styleText.normal.textColor = Color.green;
        EditorGUILayout.LabelField("EDITOR ONLY(테스트 하다가 소리가 안나오면 아무 mp3 파일 눌러서 재생후 실행)", styleText);

        var style = new GUIStyle(GUI.skin.button);
        EditorGUILayout.BeginHorizontal();
        style.fixedHeight = 40;
        style.fixedWidth = 80;

        if(SoundPlayer != null)
        {
            SoundVolumeModify();
        }


        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("PlayButton On").image), style)) { StartTestSoundPlayer(); }
        // if(GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("PauseButton On").image), style)) { SetAnimation_Pause(); }
        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("animationdopesheetkeyframe").image), style)) { StopTestSoundPlayer(); }





        EditorGUILayout.EndHorizontal();

    }

    private void OnDisable()
    {
        StopTestSoundPlayer();
    }

    public void StartTestSoundPlayer()
    {
        if(SoundPlayer != null)
        {
            SoundPlayer.EditorPlaySound();
        }

    }

    public void SoundVolumeModify()
    {
        if(SoundPlayer != null)
        {
            SoundPlayer.EditorSoundVolume(SoundPlayer.TestEditorVolume);
        }
    }


    public void StopTestSoundPlayer()
    {
        if(SoundPlayer != null)
        {
            SoundPlayer.EditorStopSound();
        }
    }
}
