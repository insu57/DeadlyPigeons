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

    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;
    
    //DataManager에서 읽어오도록 수정.

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

        string t = "Title";
        var r = StringToBgmType(t);
        if (BgmType.Title == r)
        {
            Debug.Log("same!");
        }

    }

    public void PlayBGM(BgmType type)
    {
        int index = (int)type;
        if (index >= bgmClips.Length || !bgmClips[index]) return;

        if (bgmSource.clip == bgmClips[index] && bgmSource.isPlaying) return;

        bgmSource.clip = bgmClips[index];
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(SfxType type)
    {
        int index = (int)type;
        if (index >= sfxClips.Length || !sfxClips[index]) return;

        sfxSource.PlayOneShot(sfxClips[index], sfxVolume);
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

    public static BgmType StringToBgmType(string value)
    {
        switch (value)
        {
            case nameof(BgmType.Title): return BgmType.Title;
            case nameof(BgmType.Stage): return BgmType.Stage;
            case nameof(BgmType.Store): return BgmType.Store;
            default: return BgmType.None;
        }
    }

    public static SfxType StringToSfxType(string value)
    {
        switch (value)
        {
            case nameof(SfxType.Sweep): return SfxType.Sweep;
            case nameof(SfxType.Thrust): return SfxType.Thrust;
            case nameof(SfxType.Gun01): return SfxType.Gun01;
            case nameof(SfxType.Fire): return SfxType.Fire;
            case nameof(SfxType.EnemyHit): return SfxType.EnemyHit;
            case nameof(SfxType.PlayerHit): return SfxType.PlayerHit;
            case nameof(SfxType.LevelUp): return SfxType.LevelUp;
            case nameof(SfxType.Purchase): return SfxType.Purchase;
            default: return SfxType.None;
        }
    }
}
