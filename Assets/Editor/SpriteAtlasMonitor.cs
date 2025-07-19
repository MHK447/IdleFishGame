using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using System;
using System.Collections.Generic;
using UnityEditor.U2D;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

// 에셋 생성/변경 감지를 위한 AssetPostprocessor
public class SpriteAtlasAssetPostprocessor : AssetPostprocessor
{
    // 에셋이 생성되거나 변경된 후 호출됨
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // 변경된 에셋 중 SpriteAtlas만 처리
        foreach (string assetPath in importedAssets)
        {
            ProcessSpriteAtlasIfNeeded(assetPath);
        }
        
        // 이동된 에셋 처리 (복사된 에셋 또는 이름이 변경된 에셋)
        for (int i = 0; i < movedAssets.Length; i++)
        {
            string assetPath = movedAssets[i];
            string oldAssetPath = i < movedFromAssetPaths.Length ? movedFromAssetPaths[i] : string.Empty;
            
            // 이름 변경 감지
            if (!string.IsNullOrEmpty(oldAssetPath) && 
                oldAssetPath.EndsWith(".spriteatlas") && 
                assetPath.EndsWith(".spriteatlas"))
            {
                // 이름 변경된 아틀라스 처리
                ProcessRenamedSpriteAtlas(assetPath, oldAssetPath);
            }
            else
            {
                // 일반적인 이동 또는 복사된 아틀라스 처리
                ProcessSpriteAtlasIfNeeded(assetPath);
            }
        }
        
        // 삭제된 에셋 처리
        foreach (string deletedAssetPath in deletedAssets)
        {
            if (deletedAssetPath.EndsWith(".spriteatlas") && deletedAssetPath.StartsWith("Assets/Arts/Atlas"))
            {
                // 삭제된 아틀라스 처리
                ProcessDeletedSpriteAtlas(deletedAssetPath);
            }
        }
    }
    
    // 이름이 변경된 스프라이트 아틀라스 처리
    private static void ProcessRenamedSpriteAtlas(string newAssetPath, string oldAssetPath)
    {
        if (newAssetPath.StartsWith("Assets/Arts/Atlas"))
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(newAssetPath);
            if (atlas != null)
            {
                string oldAtlasName = Path.GetFileNameWithoutExtension(oldAssetPath);
                string newAtlasName = atlas.name;
                
                Debug.Log($"아틀라스 이름 변경 감지: {oldAtlasName} -> {newAtlasName}");
                
                // enum에서 이전 이름 항목 제거 및 새 이름 추가 (숫자가 없는 이름만)
                if (!SpriteAtlasMonitor.ContainsNumber(oldAtlasName) && !SpriteAtlasMonitor.ContainsNumber(newAtlasName))
                {
                    SpriteAtlasMonitor.RemoveFromAtlasEnum(oldAtlasName);
                    SpriteAtlasMonitor.AddToAtlasEnum(newAtlasName);
                }
                
                // 아틀라스 처리 (어드레서블에 재등록 등)
                SpriteAtlasMonitor.ProcessAtlas(atlas, newAssetPath);
            }
        }
    }
    
    // 스프라이트 아틀라스 처리가 필요한지 확인하고 처리
    private static void ProcessSpriteAtlasIfNeeded(string assetPath)
    {
        if (assetPath.EndsWith(".spriteatlas") && assetPath.StartsWith("Assets/Arts/Atlas"))
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            if (atlas != null)
            {
                // 아틀라스가 어드레서블에 등록되어 있지 않거나 Addressable 그룹에 없는 경우 처리
                if (!SpriteAtlasMonitor.IsAddressable(assetPath) || !SpriteAtlasMonitor.IsInAddressableGroup(assetPath))
                {
                    Debug.Log($"아틀라스 처리 시작: {atlas.name} (경로: {assetPath})");
                    SpriteAtlasMonitor.ProcessAtlas(atlas, assetPath);
                }
            }
        }
    }

    // 삭제된 스프라이트 아틀라스 처리
    private static void ProcessDeletedSpriteAtlas(string deletedAssetPath)
    {
        string atlasName = Path.GetFileNameWithoutExtension(deletedAssetPath);
        Debug.Log($"아틀라스 삭제 감지: {atlasName} (경로: {deletedAssetPath})");
        
        // 어드레서블에서 제거
        SpriteAtlasMonitor.RemoveFromAddressables(deletedAssetPath);
        
        // enum에서 제거 (숫자가 없는 이름만)
        if (!SpriteAtlasMonitor.ContainsNumber(atlasName))
        {
            SpriteAtlasMonitor.RemoveFromAtlasEnum(atlasName);
            Debug.Log($"삭제된 아틀라스 '{atlasName}'를 Atlas enum에서 제거했습니다.");
        }
    }
}

[InitializeOnLoad]
public class SpriteAtlasMonitor
{
    static string enumFilePath = "Assets/BanpoFri/Scripts/AtlasManager.cs";

    // 아틀라스가 어드레서블 그룹에 정상적으로 포함되어 있는지 확인
    public static bool IsInAddressableGroup(string assetPath)
    {
        // Addressables 설정이 있는지 확인
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            return false;

        // 에셋의 GUID 가져오기
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        
        // 해당 GUID가 Addressables에 등록되어 있는지 확인
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        if (entry == null) 
            return false;
            
        // 그룹이 존재하고 유효한지 확인
        return entry.parentGroup != null;
    }

    // 단일 아틀라스 처리 메소드 (AssetPostprocessor에서 호출)
    public static void ProcessAtlas(SpriteAtlas atlas, string assetPath)
    {
        if (atlas == null) return;

        // 어드레서블에서 삭제 (재등록을 위해)
        RemoveFromAddressables(assetPath);

        // 설정 적용
        ConfigureNewAtlas(atlas);

        // 어드레서블에 추가
        AddToAddressables(assetPath, atlas.name);

        // 아틀라스 이름에 숫자가 포함되어 있지 않은 경우에만 Atlas enum에 추가
        if (!ContainsNumber(atlas.name))
        {
            AddToAtlasEnum(atlas.name);
        }

        Debug.Log($"아틀라스 처리 완료: {atlas.name}");
    }
    
    // 어드레서블에서 아틀라스 제거
    public static void RemoveFromAddressables(string assetPath)
    {
        if (IsAddressable(assetPath))
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                settings.RemoveAssetEntry(assetGuid);
                Debug.Log($"어드레서블에서 아틀라스 제거: {assetPath}");
            }
        }
    }

    // 문자열에 숫자가 포함되어 있는지 확인
    public static bool ContainsNumber(string text)
    {
        // 정규식을 사용하여 숫자 포함 여부 확인
        return Regex.IsMatch(text, @"\d");
    }

    // Atlas enum에 새로운 항목 추가
    public static void AddToAtlasEnum(string atlasName)
    {
        // 파일이 존재하는지 확인
        if (!File.Exists(enumFilePath))
        {
            Debug.LogWarning($"Atlas enum 파일을 찾을 수 없습니다: {enumFilePath}");
            return;
        }

        // 파일 내용 읽기
        string fileContent = File.ReadAllText(enumFilePath);

        // Atlas enum에 해당 항목이 이미 존재하는지 확인
        string enumItemPattern = @"public\s+enum\s+Atlas\s*{([^}]*)}";
        Match enumMatch = Regex.Match(fileContent, enumItemPattern, RegexOptions.Singleline);

        if (!enumMatch.Success)
        {
            Debug.LogWarning("Atlas enum을 찾을 수 없습니다.");
            return;
        }

        string enumContent = enumMatch.Groups[1].Value;

        // 이미 존재하는 항목인지 확인
        if (Regex.IsMatch(enumContent, $@"\b{atlasName}\b"))
        {
            Debug.Log($"Atlas enum에 이미 '{atlasName}' 항목이 존재합니다.");
            return;
        }

        // 주석 위치 찾기 ("// @ add here")
        string commentPattern = @"//\s*@\s*add\s*here";
        Match commentMatch = Regex.Match(enumContent, commentPattern);

        if (!commentMatch.Success)
        {
            Debug.LogError("Atlas enum에 '// @ add here' 주석을 찾을 수 없습니다.");
            return;
        }

        // 새 항목 추가
        string newEnumContent = enumContent.Insert(
            commentMatch.Index,
            $"    {atlasName},\n    "
        );

        // 전체 파일 내용 업데이트
        string newFileContent = fileContent.Replace(enumContent, newEnumContent);

        // 파일 저장
        File.WriteAllText(enumFilePath, newFileContent);

        // 에셋 데이터베이스 새로고침 및 스크립트 컴파일
        AssetDatabase.ImportAsset(enumFilePath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        Debug.Log($"Atlas enum에 '{atlasName}' 항목이 추가되었습니다. 스크립트 컴파일 중...");
    }

    // Atlas enum에서 항목 제거
    public static void RemoveFromAtlasEnum(string atlasName)
    {
        // 파일이 존재하는지 확인
        if (!File.Exists(enumFilePath))
        {
            Debug.LogWarning($"Atlas enum 파일을 찾을 수 없습니다: {enumFilePath}");
            return;
        }

        // 파일 내용 읽기
        string fileContent = File.ReadAllText(enumFilePath);

        // Atlas enum에 해당 항목이 존재하는지 확인
        string enumItemPattern = @"public\s+enum\s+Atlas\s*{([^}]*)}";
        Match enumMatch = Regex.Match(fileContent, enumItemPattern, RegexOptions.Singleline);

        if (!enumMatch.Success)
        {
            Debug.LogWarning("Atlas enum을 찾을 수 없습니다.");
            return;
        }

        string enumContent = enumMatch.Groups[1].Value;

        // 해당 항목이 존재하는지 확인
        Regex itemRegex = new Regex($@"(\s+{atlasName}\s*,)", RegexOptions.Multiline);
        Match itemMatch = itemRegex.Match(enumContent);

        if (!itemMatch.Success)
        {
            Debug.Log($"Atlas enum에 '{atlasName}' 항목이 존재하지 않습니다.");
            return;
        }

        // 항목 제거
        string newEnumContent = enumContent.Replace(itemMatch.Groups[1].Value, "");

        // 연속된 빈 줄 제거
        newEnumContent = Regex.Replace(newEnumContent, @"\n\s*\n\s*\n", "\n\n");

        // 전체 파일 내용 업데이트
        string newFileContent = fileContent.Replace(enumContent, newEnumContent);

        // 파일 저장
        File.WriteAllText(enumFilePath, newFileContent);

        // 에셋 데이터베이스 새로고침
        AssetDatabase.ImportAsset(enumFilePath);

        Debug.Log($"Atlas enum에서 '{atlasName}' 항목이 제거되었습니다.");
    }

    public static bool IsAddressable(string assetPath)
    {
        // Addressables 설정이 있는지 확인
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            return false;

        // 에셋의 GUID 가져오기
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        // 해당 GUID가 Addressables에 등록되어 있는지 확인
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        return entry != null;
    }

    static void AddToAddressables(string assetPath, string atlasName)
    {
        // Addressables 설정이 있는지 확인
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables 설정을 찾을 수 없습니다. Addressables 패키지를 설치하고 설정해주세요.");
            return;
        }

        // 아틀라스를 추가할 그룹 찾기 (또는 생성)
        string groupName = "Default Local Group";
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
        }

        // 에셋의 GUID 가져오기
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        // 어드레서블에 추가
        string address = atlasName; // 아틀라스 이름을 어드레스로 사용
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = address;

        // 설정 저장
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
        Debug.Log($"아틀라스 '{atlasName}'를 Addressables에 추가했습니다. 주소: {address}");
    }

    static void ConfigureNewAtlas(SpriteAtlas atlas)
    {
        // 현재 텍스처 설정 가져오기
        SpriteAtlasTextureSettings currentTextureSettings = atlas.GetTextureSettings();

        // 텍스처 설정 적용 (기존 커스텀 설정이 있으면 최대한 유지)
        SpriteAtlasTextureSettings newTextureSettings = new SpriteAtlasTextureSettings();

        // 필터 모드 복사(기본값: Bilinear)
        newTextureSettings.filterMode = currentTextureSettings.filterMode != FilterMode.Point ?
                                         currentTextureSettings.filterMode : FilterMode.Bilinear;

        // 밉맵 설정 복사(기본값: false)
        newTextureSettings.generateMipMaps = currentTextureSettings.generateMipMaps;

        // 기타 설정 복사
        newTextureSettings.sRGB = currentTextureSettings.sRGB;
        newTextureSettings.readable = currentTextureSettings.readable;

        // 텍스처 설정 적용
        atlas.SetTextureSettings(newTextureSettings);

        // 현재 패킹 설정 가져오기
        SpriteAtlasPackingSettings currentPackingSettings = atlas.GetPackingSettings();

        // 패킹 설정 적용 (기존 커스텀 설정이 있으면 최대한 유지)
        SpriteAtlasPackingSettings newPackingSettings = new SpriteAtlasPackingSettings();

        // 설정 복사 (기본값으로 덮어쓰기 전에 확인)
        newPackingSettings.enableRotation = currentPackingSettings.enableRotation;
        newPackingSettings.enableTightPacking = currentPackingSettings.enableTightPacking;

        // padding 설정 (기본값: 4, 하지만 0보다 작으면 기본값 적용)
        newPackingSettings.padding = currentPackingSettings.padding > 0 ?
                                     currentPackingSettings.padding : 4;

        // 블록 오프셋 설정 복사
        newPackingSettings.blockOffset = currentPackingSettings.blockOffset;

        // 패킹 설정 적용
        atlas.SetPackingSettings(newPackingSettings);

        // Android 설정 가져오기
        TextureImporterPlatformSettings androidSettings = atlas.GetPlatformSettings("Android");

        // Android 플랫폼 설정 적용 (기존 설정된 값이 있으면 유지)
        bool androidOverridden = androidSettings.overridden;
        if (!androidOverridden)
        {
            // 기존 설정이 없는 경우에만 기본값 적용
            androidSettings.maxTextureSize = 2048;
            androidSettings.format = TextureImporterFormat.ASTC_6x6;
            androidSettings.overridden = true;
            atlas.SetPlatformSettings(androidSettings);
        }

        // iOS 설정 가져오기
        TextureImporterPlatformSettings iosSettings = atlas.GetPlatformSettings("iPhone");

        // iOS 플랫폼 설정 적용 (기존 설정된 값이 있으면 유지)
        bool iosOverridden = iosSettings.overridden;
        if (!iosOverridden)
        {
            // 기존 설정이 없는 경우에만 기본값 적용
            iosSettings.maxTextureSize = 2048;
            iosSettings.format = TextureImporterFormat.ASTC_6x6;
            iosSettings.overridden = true;
            atlas.SetPlatformSettings(iosSettings);
        }

        // 변경사항 저장
        AssetDatabase.SaveAssets();
    }

    // 메뉴 항목을 추가하여 수동으로 모든 아틀라스 재처리하기
    [MenuItem("BanpoFri/SpriteAtlas/Process All Atlas")]
    static void ProcessAllAtlas()
    {
        string targetPath = "Assets/Arts/Atlas";
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { targetPath });

        int processedCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

            if (atlas != null && !IsAddressable(path))
            {
                ProcessAtlas(atlas, path);
                processedCount++;
            }
        }

        if (processedCount > 0)
            Debug.Log($"총 {processedCount}개의 아틀라스가 처리되었습니다.");
        else
            Debug.Log("처리할 새 아틀라스가 없습니다.");
    }

    // 모든 아틀라스를 강제로 재처리하기
    [MenuItem("BanpoFri/SpriteAtlas/Force Process All Atlas")]
    static void ForceProcessAllAtlas()
    {
        string targetPath = "Assets/Arts/Atlas";
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { targetPath });

        int processedCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

            if (atlas != null)
            {
                // 어드레서블에서 먼저 제거 (재설정을 위해)
                if (IsAddressable(path))
                {
                    AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                    if (settings != null)
                    {
                        string assetGuid = AssetDatabase.AssetPathToGUID(path);
                        settings.RemoveAssetEntry(assetGuid);
                    }
                }

                // 설정 다시 적용 및 어드레서블 등록
                ProcessAtlas(atlas, path);
                processedCount++;
            }
        }

        Debug.Log($"총 {processedCount}개의 아틀라스가 강제로 재처리되었습니다.");
    }

    // 어드레서블 상태 확인
    [MenuItem("BanpoFri/SpriteAtlas/Check Addressable Status")]
    static void CheckAddressableStatus()
    {
        string targetPath = "Assets/Arts/Atlas";
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { targetPath });

        List<string> addressableAtlases = new List<string>();
        List<string> nonAddressableAtlases = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

            if (atlas != null)
            {
                if (IsAddressable(path))
                {
                    addressableAtlases.Add(atlas.name);
                }
                else
                {
                    nonAddressableAtlases.Add(atlas.name);
                }
            }
        }

        Debug.Log($"어드레서블로 등록된 아틀라스 ({addressableAtlases.Count}개): {string.Join(", ", addressableAtlases)}");
        Debug.Log($"어드레서블로 등록되지 않은 아틀라스 ({nonAddressableAtlases.Count}개): {string.Join(", ", nonAddressableAtlases)}");
    }

    // 모든 아틀라스를 어드레서블로 등록
    [MenuItem("BanpoFri/SpriteAtlas/Add All to Addressables")]
    static void AddAllToAddressables()
    {
        string targetPath = "Assets/Arts/Atlas";
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { targetPath });

        int addedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

            if (atlas != null && !IsAddressable(path))
            {
                AddToAddressables(path, atlas.name);
                addedCount++;
            }
        }

        Debug.Log($"총 {addedCount}개의 아틀라스를 어드레서블에 추가했습니다.");
    }

    // 모든 아틀라스(숫자가 없는 이름만)를 Atlas enum에 추가
    [MenuItem("BanpoFri/SpriteAtlas/Add All Atlas to Enum")]
    static void AddAllAtlasToEnum()
    {
        string targetPath = "Assets/Arts/Atlas";
        string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { targetPath });

        int addedCount = 0;
        int removedCount = 0;
        List<string> skippedAtlases = new List<string>();
        List<string> removedAtlases = new List<string>();
        bool anyChanges = false;

        // 파일이 존재하는지 확인
        if (!File.Exists(enumFilePath))
        {
            Debug.LogWarning($"Atlas enum 파일을 찾을 수 없습니다: {enumFilePath}");
            return;
        }

        // 파일 내용 읽기
        string fileContent = File.ReadAllText(enumFilePath);

        // Atlas enum 블록 찾기
        string enumItemPattern = @"public\s+enum\s+Atlas\s*{([^}]*)}";
        Match enumMatch = Regex.Match(fileContent, enumItemPattern, RegexOptions.Singleline);

        if (!enumMatch.Success)
        {
            Debug.LogWarning("Atlas enum을 찾을 수 없습니다.");
            return;
        }

        string enumContent = enumMatch.Groups[1].Value;

        // 주석 위치 찾기 ("// @ add here")
        string commentPattern = @"//\s*@\s*add\s*here";
        Match commentMatch = Regex.Match(enumContent, commentPattern);

        if (!commentMatch.Success)
        {
            Debug.LogError("Atlas enum에 '// @ add here' 주석을 찾을 수 없습니다.");
            return;
        }

        // 현재 프로젝트에 존재하는 모든 아틀라스 이름 수집 (숫자 없는 것만)
        HashSet<string> existingAtlasNames = new HashSet<string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

            if (atlas != null && !ContainsNumber(atlas.name))
            {
                existingAtlasNames.Add(atlas.name);
            }
        }

        // STEP 1: enum 내용을 구역별로 분리
        // "// stage atlas" 섹션과 그 이하 내용 찾기
        string stageAtlasSectionPattern = @"//\s*stage\s*atlas(.+?)(?=\n\s*\n|\n\s*//(?!\s*@\s*add\s*here))";
        Match stageAtlasSectionMatch = Regex.Match(enumContent, stageAtlasSectionPattern, RegexOptions.Singleline);

        // stage atlas 섹션 내용
        string stageAtlasSection = "";
        int stageAtlasSectionEndPos = 0;

        if (stageAtlasSectionMatch.Success)
        {
            stageAtlasSection = stageAtlasSectionMatch.Value;
            stageAtlasSectionEndPos = stageAtlasSectionMatch.Index + stageAtlasSectionMatch.Length;

            // 섹션 내 항목 추출 (특별 항목)
            List<string> specialItems = new List<string>();
            MatchCollection sectionItemMatches = Regex.Matches(stageAtlasSection, @"^\s*([A-Za-z_][A-Za-z0-9_]*),?\s*(?://.*)?$", RegexOptions.Multiline);
            foreach (Match match in sectionItemMatches)
            {
                string itemName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(itemName) && !itemName.StartsWith("//"))
                {
                    specialItems.Add(itemName);
                    // 특별 항목은 이미 처리된 것으로 표시
                    existingAtlasNames.Remove(itemName);
                }
            }
        }

        // regular section (stage atlas 섹션 이후부터 "// @ add here" 주석 전까지)
        string regularSection = "";
        HashSet<string> regularItems = new HashSet<string>();

        if (stageAtlasSectionEndPos > 0 && commentMatch.Index > stageAtlasSectionEndPos)
        {
            regularSection = enumContent.Substring(stageAtlasSectionEndPos, commentMatch.Index - stageAtlasSectionEndPos);

            // 일반 항목 추출
            MatchCollection regularItemMatches = Regex.Matches(regularSection, @"^\s*([A-Za-z_][A-Za-z0-9_]*),?\s*(?://.*)?$", RegexOptions.Multiline);
            foreach (Match match in regularItemMatches)
            {
                string itemName = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(itemName) && !itemName.StartsWith("//"))
                {
                    // 존재하지 않는 아틀라스 항목 제거
                    if (existingAtlasNames.Contains(itemName))
                    {
                        regularItems.Add(itemName);
                        existingAtlasNames.Remove(itemName);
                    }
                    else
                    {
                        removedAtlases.Add(itemName);
                        removedCount++;
                        anyChanges = true;
                    }
                }
            }
        }

        // STEP 2: 새로운 enum 내용 구성
        StringBuilder newContentBuilder = new StringBuilder();

        // stage atlas 섹션 그대로 유지 
        if (!string.IsNullOrEmpty(stageAtlasSection))
        {
            newContentBuilder.Append(stageAtlasSection);

            // 섹션 뒤에 개행이 없으면 추가
            if (!stageAtlasSection.EndsWith("\n"))
                newContentBuilder.AppendLine();
        }

        // 빈 줄 추가
        newContentBuilder.AppendLine();

        // 기존 regular section의 주석 및 레이아웃 보존
        // 항목만 필터링하여 유지 (존재하는 항목만)
        if (!string.IsNullOrEmpty(regularSection))
        {
            // 주석 라인과 빈 줄은 그대로 유지
            string[] lines = regularSection.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // 주석이나 빈 줄은 그대로 유지
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("//"))
                {
                    newContentBuilder.AppendLine(line);
                    continue;
                }

                // 항목 라인인 경우 (기존 항목 중 유효한 것만 유지)
                Match itemMatch = Regex.Match(line, @"^\s*([A-Za-z_][A-Za-z0-9_]*),?\s*(?://.*)?$");
                if (itemMatch.Success)
                {
                    string itemName = itemMatch.Groups[1].Value.Trim();
                    if (regularItems.Contains(itemName))
                    {
                        newContentBuilder.AppendLine(line);
                        regularItems.Remove(itemName); // 이미 처리됨
                    }
                }
            }
        }

        // 아직 처리되지 않은 regularItems 추가
        foreach (string item in regularItems)
        {
            newContentBuilder.AppendLine($"    {item},");
        }

        // 새로 추가할 항목
        if (existingAtlasNames.Count > 0)
        {
            // 새 항목 추가 전에 빈 줄 하나 더 추가
            newContentBuilder.AppendLine();

            // 새 항목 추가
            foreach (string atlasName in existingAtlasNames)
            {
                newContentBuilder.AppendLine($"    {atlasName},");
                addedCount++;
                anyChanges = true;
            }
        }

        // 주석 추가
        newContentBuilder.AppendLine("    // @ add here");

        if (anyChanges)
        {
            // 기존 enum 내용을 새로운 내용으로 대체
            string newEnumContent = enumMatch.Value.Replace(enumContent, "\n" + newContentBuilder.ToString() + "\n");
            string newFileContent = fileContent.Replace(enumMatch.Value, newEnumContent);

            // 파일 저장
            File.WriteAllText(enumFilePath, newFileContent);

            // 에셋 데이터베이스 새로고침 및 스크립트 컴파일
            AssetDatabase.ImportAsset(enumFilePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            string message = "";
            if (addedCount > 0)
                message += $"{addedCount}개의 아틀라스를 추가하고 ";
            if (removedCount > 0)
                message += $"{removedCount}개의 없어진 아틀라스를 제거했습니다. ";

            Debug.Log(message + "스크립트 컴파일 중...");

            if (removedAtlases.Count > 0)
                Debug.Log($"제거된 아틀라스: {string.Join(", ", removedAtlases)}");
        }
        else
        {
            Debug.Log("변경사항이 없습니다.");
        }

        if (skippedAtlases.Count > 0)
            Debug.Log($"숫자가 포함되어 건너뛴 아틀라스 ({skippedAtlases.Count}개): {string.Join(", ", skippedAtlases)}");
    }

    // 여러 아틀라스에 대해 한번에 컴파일
    static void CompileAfterMultipleAdditions(int addedCount)
    {
        if (addedCount > 0)
        {
            // 모든 추가가 완료된 후 한 번만 컴파일
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"총 {addedCount}개의 아틀라스가 추가되었습니다. 스크립트 컴파일 중...");
        }
    }
}