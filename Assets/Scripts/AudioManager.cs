using System.Collections.Generic;
using UnityEngine;

public enum BgmType
{
    Title,
    Stage,
    Store,
    None
}

public enum SfxType
{
    Sweep,
    Thrust,
    Gun01,
    Fire,
    EnemyHit,
    PlayerHit,
    LevelUp,
    Purchase,
    None
}

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private SfxData sfxData;
    [SerializeField] private BgmData bgmData;

    // 타입 → 클립 매핑 (sfxData/bgmData에서 구성)
    private readonly Dictionary<BgmType, AudioClip> _bgmClips = new();
    private readonly Dictionary<SfxType, AudioClip> _sfxClips = new();

    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    protected override void Awake()
    {
        base.Awake();

        if (!bgmSource)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;

        BuildClipDict();
    }

    private void BuildClipDict()
    {
        if (bgmData)
        {
            foreach (var kv in bgmData.Values)
            {
                if (kv.clip) _bgmClips[kv.bgmType] = kv.clip;
            }
        }
        else
        {
            Debug.LogWarning("[AudioManager] bgmData 미할당.");
        }

        if (sfxData)
        {
            foreach (var kv in sfxData.Values)
            {
                if (kv.clip) _sfxClips[kv.type] = kv.clip;
            }
        }
        else
        {
            Debug.LogWarning("[AudioManager] sfxData 미할당.");
        }
    }

    public void PlayBGM(BgmType type)
    {
        if (!_bgmClips.TryGetValue(type, out var clip) || !clip) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(SfxType type)
    {
        if (!_sfxClips.TryGetValue(type, out var clip) || !clip) return;

        sfxSource.PlayOneShot(clip);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}
