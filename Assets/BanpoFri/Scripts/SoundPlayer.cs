using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.AddressableAssets;

public class SoundPlayer : SingletonScriptableObject<SoundPlayer>, ILoader
{
    [System.Serializable]
    public class SoundData
    {
        public string soundKey;
        public AudioClip audioData;
        [Range(0, 1)]
        public float Volume = 1;
    }
    [SerializeField]
    private List<SoundData> soundDatas = new List<SoundData>();
    private Dictionary<string, AudioClip> soundDataAudioDic = new Dictionary<string, AudioClip>();
    public List<SoundData> SoundDataList { get { return soundDatas; } }
    public Dictionary<string, SoundData> SoundDataDic = new Dictionary<string, SoundData>();

    private List<AudioSource> cachedSources = new List<AudioSource>();
    private List<AudioSource> cachedSpaceSources = new List<AudioSource>();
    private AudioSource BGMSource = null;
    private AudioClip recoveryBGMClip = null;
    private Transform Root = null;

    public float BGM_VOLUME { get { return SoundDataDic["bgm"].Volume; } }

    private bool soundon = true;

    [SerializeField]
    private AudioClip TestAudioClip;

    [Range(0, 1)]
    public float TestEditorVolume = 1;

    private AudioSource EditorSource;

    public AudioClip GetSoundData(string key)
    {
        if (soundDataAudioDic.ContainsKey(key))
            return soundDataAudioDic[key];

        return null;
    }

    public void SetRoot(Transform _root)
    {
        Root = _root;
    }

    public void PlaySound(string key)
    {
        if (!soundon) return;

        var audio = GetAudioSource();
        audio.volume = SoundDataDic[key].Volume;
        audio.loop = false;
        audio.PlayOneShot(GetSoundData(key));

    }

    public AudioSource PlayLoopSound(string key)
    {
        if (!soundon) return null;

        var audio = GetAudioSource();
        audio.volume = SoundDataDic[key].Volume;
        audio.loop = true;
        audio.PlayOneShot(GetSoundData(key));

        return audio;
    }



    public void SpacePlaySound(string key, Transform root)
    {
        if (!soundon) return;

        var audio = GetSpaceAudioSource(root, PlaySoundAction, key);

        if (audio != null)
        {
            PlaySoundAction(audio, key);
        }
    }

    private void PlaySoundAction(AudioSource source, string key)
    {
        source.loop = false;
        source.volume = SoundDataDic[key].Volume;

        source.PlayOneShot(GetSoundData(key));
    }

    public void PlayBGM(string key, bool recovery = false)
    {
        if (BGMSource == null)
        {
            BGMSource = new GameObject("sound entity").AddComponent<AudioSource>();
            BGMSource.transform.SetParent(Root);
        }

        BGMSource.loop = true;
        BGMSource.volume = SoundDataDic[key].Volume;
        var clip = GetSoundData(key);
        if (recovery)
            recoveryBGMClip = clip;
        BGMSource.clip = clip;
        BGMSource.Play();
    }

    public void RecoveryBGM()
    {
        BGMSource.clip = recoveryBGMClip;
        BGMSource.Play();
    }

    public void BgmSwitch(bool value)
    {
        if (BGMSource == null)
        {
            BGMSource = new GameObject("sound entity").AddComponent<AudioSource>();
            BGMSource.transform.SetParent(Root);
        }

        BGMSource.mute = !value;
    }
    public void EffectSwitch(bool value)
    {
        soundon = value;
    }
    public void SetBGMVolume(float volume = -1)
    {
        if (volume == -1 && BGMSource != null)
        {
            BGMSource.volume = SoundDataDic["bgm"].Volume;
            return;
        }


        if (BGMSource != null)
        {
            BGMSource.volume = volume;
        }
    }


    private AudioSource GetAudioSource()
    {
        AudioSource audio = null;

        foreach (var source in cachedSources)
        {
            if(source == null) continue;

            if (!source.isPlaying)
            {
                audio = source;
                return audio;
            }
        }

        if (audio == null)
        {
            audio = new GameObject("sound entity").AddComponent<AudioSource>();
            cachedSources.Add(audio);
            audio.transform.SetParent(Root);
        }

        return audio;
    }

    private AudioSource GetSpaceAudioSource(Transform root, System.Action<AudioSource, string> AddAction = null, string key = "")
    {
        AudioSource audio = null;

        foreach (var source in cachedSpaceSources)
        {
            if (source == null)
                break;

            if (!source.isPlaying)
            {
                audio = source;
                audio.transform.SetParent(root);
                audio.transform.localPosition = Vector3.zero;

                audio.spatialBlend = 1f;
                audio.dopplerLevel = 0;
                return audio;
            }
        }


        if (audio == null)
        {
            Addressables.InstantiateAsync("SpaceSound").Completed += (handle) =>
            {
                audio = handle.Result.GetComponent<AudioSource>();
                cachedSpaceSources.Add(audio);
                audio.transform.SetParent(root);
                audio.transform.localPosition = Vector3.zero;
                audio.spatialBlend = 1f;
                audio.dopplerLevel = 0;
                AddAction?.Invoke(audio, key);
            };

        }

        return audio;
    }

    public void Init()
    {
        cachedSpaceSources.Clear();

        AudioSettings.OnAudioConfigurationChanged += (changed) =>
        {
            Debug.Log(changed ? "Device was changed" : "Reset was called");
            if (changed)
            {
                AudioConfiguration config = AudioSettings.GetConfiguration();
                config.dspBufferSize = 64;
                AudioSettings.Reset(config);
            }
            BGMSource.volume = SoundDataDic["bgm"].Volume;
            BGMSource.Play();
        };
    }

    public void Load()
    {
        cachedSources.Clear();
        cachedSpaceSources.Clear();
        soundDataAudioDic.Clear();
        SoundDataDic.Clear();
        foreach (var sd in soundDatas)
        {
            SoundDataDic.Add(sd.soundKey, sd);
            soundDataAudioDic.Add(sd.soundKey, sd.audioData);
        }
    }



    #region


    private AudioSource GetEditorAudioSource()
    {
        AudioSource audio = null;
        audio = new GameObject("sound entity").AddComponent<AudioSource>();
        return audio;
    }

    public void EditorPlaySound()
    {
        if (TestAudioClip != null)
        {
            if (EditorSource != null)
                Object.DestroyImmediate(EditorSource.gameObject);

            EditorSource = GetEditorAudioSource();
            EditorSource.volume = TestEditorVolume;
            EditorSource.loop = false;
            EditorSource.clip = TestAudioClip;
            EditorSource.PlayOneShot(TestAudioClip);
        }
        else
        {
            Debug.Log("사운드를 넣어주세요!!");
        }
    }




    public void EditorSoundVolume(float volume)
    {
        if (EditorSource != null)
        {
            EditorSource.volume = volume;
        }
    }


    public void EditorStopSound()
    {
        if (EditorSource != null)
        {
            EditorSource.Stop();

            Object.DestroyImmediate(EditorSource.gameObject);
        }
    }


    #endregion




}
