using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;

public class AtlasManager : Singleton<AtlasManager>
{
    public Dictionary<Atlas, SpriteAtlas> atlasDict = new Dictionary<Atlas, SpriteAtlas>();
    Dictionary<Atlas, System.Action<SpriteAtlas>> loadingRequests = new Dictionary<Atlas, System.Action<SpriteAtlas>>();
    Dictionary<Atlas, Dictionary<string, Sprite>> spriteCache = new Dictionary<Atlas, Dictionary<string, Sprite>>();
    int loadCount = 99;


    public Sprite GetSprite(Atlas atlasType, string key)
    {
        if (spriteCache.TryGetValue(atlasType, out var dict))
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, atlasDict[atlasType].GetSprite(key));

            return dict[key];
        }

        if (!atlasDict.TryGetValue(atlasType, out SpriteAtlas atlas))
        {
            return null;
        }

        // 아틀라스에서 직접 스프라이트 가져오기
        return atlas.GetSprite(key);
    }


    // 따로 캐싱할 아틀라스 설정
    public void Init()
    {
        SpriteAtlasManager.atlasRequested += OnAtlasRequest;

        spriteCache.Add(Atlas.Atlas_UI_Common, new Dictionary<string, Sprite>());
        spriteCache.Add(Atlas.Atlas_UI_Common, new Dictionary<string, Sprite>());
        //spriteCache.Add(Atlas.Atlas_UI_DynamicShop, new Dictionary<string, Sprite>());
    }

    // 전체 로드
    public void LoadAllAtlas()
    {
        // 기존 데이터 정리
        foreach (var atlas in atlasDict.Values)
        {
            if (atlas != null)
                Addressables.Release<SpriteAtlas>(atlas);
        }

        spriteCache.Clear();
        atlasDict.Clear();
        loadingRequests.Clear();

        // 모든 Atlas enum 값에 대해 로드
        System.Array atlasValues = System.Enum.GetValues(typeof(Atlas));
        loadCount = atlasValues.Length - ignoreAtlas.Count;

        foreach (Atlas atlasType in atlasValues)
        {
            if (ignoreAtlas.Contains(atlasType))
                continue;

            if (loadingRequests.ContainsKey(atlasType))
            {
                // 이미 로드 중인 경우 콜백 추가
                continue;
            }

            InternalLoadAtlas(atlasType, (atlas) =>
            {
                --loadCount;
            });
        }
    }

    // 아틀라스 로드 콜백
    private void OnAtlasRequest(string tag, System.Action<SpriteAtlas> callback)
    {
        Atlas atlasType;
        if (System.Enum.TryParse(tag, out atlasType))
        {
            // 이미 로드된 아틀라스가 있는 경우
            if (atlasDict.TryGetValue(atlasType, out SpriteAtlas atlas))
            {
                callback.Invoke(atlas);
                return;
            }

            // 로드 중인 경우 콜백 추가
            if (loadingRequests.ContainsKey(atlasType))
            {
                loadingRequests[atlasType] += callback;
                return;
            }

            // 새로 로드 시작
            InternalLoadAtlas(atlasType, callback);
        }
        else
        {
            // Atlas enum으로 변환할 수 없는 경우는 null 반환
            Debug.LogWarning($"Atlas tag '{tag}' could not be parsed to Atlas enum.");
            callback.Invoke(null);
        }
    }

    // 내부 아틀라스 로드 함수
    private void InternalLoadAtlas(Atlas atlasType, System.Action<SpriteAtlas> callback = null)
    {
        string addressableName = atlasType.ToString();

        // 콜백 등록
        loadingRequests[atlasType] = callback;

        Addressables.LoadAssetAsync<SpriteAtlas>(addressableName).Completed += (result) =>
        {
            var atlas = result.Result as SpriteAtlas;

            if (atlas != null)
            {
                if (!atlasDict.ContainsKey(atlasType))
                    atlasDict.Add(atlasType, atlas);
                else
                    atlasDict[atlasType] = atlas;

                // 대기 중인 콜백 호출
                if (loadingRequests.TryGetValue(atlasType, out var callbacks))
                {
                    callbacks?.Invoke(atlas);
                }
            }
            else
            {
                Debug.LogError($"Failed to load SpriteAtlas for {atlasType}");

                // 실패 시에도 콜백 호출 (null 전달)
                if (loadingRequests.TryGetValue(atlasType, out var callbacks))
                {
                    callbacks?.Invoke(null);
                }
            }

            // 로드 요청 목록에서 제거
            loadingRequests.Remove(atlasType);
        };
    }

    // 단일 아틀라스 로드
    public void LoadAtlas(Atlas atlasType, System.Action<SpriteAtlas> onComplete = null)
    {
        // 이미 로드된 아틀라스가 있는 경우
        if (atlasDict.TryGetValue(atlasType, out SpriteAtlas existingAtlas))
        {
            onComplete?.Invoke(existingAtlas);
            return;
        }

        // 로드 중인 경우 콜백 추가
        if (loadingRequests.ContainsKey(atlasType))
        {
            if (onComplete != null)
                loadingRequests[atlasType] += onComplete;
            return;
        }

        // 새 로드 시작
        InternalLoadAtlas(atlasType, onComplete);
    }

    #region stage atlas
    public void ClearStageAtlas()
    {
        if (atlasDict.ContainsKey(Atlas.Stage))
        {
            Addressables.Release<SpriteAtlas>(atlasDict[Atlas.Stage]);
            atlasDict.Remove(Atlas.Stage);
        }
    }

    void InitRequestAtlas()
    {
        AtlasManager.Instance.ReLoad(false);

    }

    public void ReLoad(bool isLow)
    {
        ReleaseAll();
        LoadAllAtlas();
    }



    public void LoadStageAtlas(int stage, System.Action onLoad)
    {
        string atlasName = $"Atlas_Stage_{stage:000}";
        if (atlasDict.ContainsKey(Atlas.Stage))
        {
            if (atlasDict[Atlas.Stage].name.Equals(atlasName))
            {
                Addressables.Release<SpriteAtlas>(atlasDict[Atlas.Stage]);
                atlasDict.Remove(Atlas.Stage);
            }
            else
            {
                Addressables.Release<SpriteAtlas>(atlasDict[Atlas.Stage]);
                atlasDict.Remove(Atlas.Stage);
            }
        }

        Addressables.LoadAssetAsync<SpriteAtlas>(atlasName).Completed += (result) =>
        {
            if (atlasDict.ContainsKey(Atlas.Stage))
                atlasDict[Atlas.Stage] = result.Result;
            else
                atlasDict.Add(Atlas.Stage, result.Result);

            onLoad?.Invoke();
        };

    }
    #endregion

    // 리소스 해제
    public void ReleaseAll()
    {
        foreach (var atlas in atlasDict.Values)
        {
            if (atlas != null)
                Addressables.Release<SpriteAtlas>(atlas);
        }

        atlasDict.Clear();
        loadingRequests.Clear();
        spriteCache.Clear();
    }

    public bool IsLoadComplete()
    {
        return loadCount <= 0;
    }

    // 전체 로드에서 제외될 아틀라스
    List<Atlas> ignoreAtlas = new List<Atlas>()
    {
        Atlas.Stage,
    };
}

public enum Atlas
{
// stage atlas
    Stage,

    // stage atlas

    Atlas_UI_Common,
        Atlas_UI_InGameFish,
    // @ add here

}