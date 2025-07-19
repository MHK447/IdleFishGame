using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextureRenamer : EditorWindow
{
    private string searchString = "";
    private string replaceString = "";
    private Vector2 scrollPosition;
    private List<Texture2D> selectedTextures = new List<Texture2D>();

    [MenuItem("Tools/Texture Renamer")]
    public static void ShowWindow()
    {
        GetWindow<TextureRenamer>("Texture Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture Renamer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Search and Replace fields
        searchString = EditorGUILayout.TextField("Search String", searchString);
        replaceString = EditorGUILayout.TextField("Replace With", replaceString);
        EditorGUILayout.Space();

        // Show selected textures
        EditorGUILayout.LabelField("Selected Textures:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var texture in selectedTextures)
        {
            if (texture != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
                EditorGUILayout.LabelField(texture.name);
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();

        // Update selected textures
        if (GUILayout.Button("Update Selection"))
        {
            UpdateSelectedTextures();
        }

        EditorGUILayout.Space();

        // Rename button
        GUI.enabled = !string.IsNullOrEmpty(searchString) && selectedTextures.Count > 0;
        if (GUILayout.Button("Rename Selected Textures"))
        {
            RenameTextures();
        }
        GUI.enabled = true;
    }

    private void UpdateSelectedTextures()
    {
        selectedTextures.Clear();
        Object[] selection = Selection.objects;
        
        foreach (Object obj in selection)
        {
            if (obj is Texture2D texture)
            {
                selectedTextures.Add(texture);
            }
        }
    }

    private void RenameTextures()
    {
        if (string.IsNullOrEmpty(searchString) || selectedTextures.Count == 0)
            return;

        foreach (var texture in selectedTextures)
        {
            if (texture != null)
            {
                string newName = texture.name.Replace(searchString, replaceString);
                if (newName != texture.name)
                {
                    string assetPath = AssetDatabase.GetAssetPath(texture);
                    AssetDatabase.RenameAsset(assetPath, newName);
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
} 