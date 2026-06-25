using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    // AudioMixer에 노출(Expose)한 파라미터 이름과 일치해야 함
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string bgmVolumeParam = "BGMVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private SfxData sfxData;
    [SerializeField] private BgmData bgmData;

    // 타입 → 클립 매핑 (sfxData/bgmData에서 구성)
    private readonly Dictionary<BgmType, AudioClip> _bgmClips = new();
    private readonly Dictionary<SfxType, AudioClip> _sfxClips = new();

    // 선형 볼륨(0~1). UI 초기화 및 dB 변환에 사용
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.5f;

    // 선형 볼륨(0~1) 외부 노출 — UI 슬라이더 초기값 세팅용
    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;

    private const float MinVolumeDb = -80f;

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

        // 소스 출력을 믹서 그룹으로 라우팅 (볼륨은 믹서 파라미터로 제어)
        bgmSource.outputAudioMixerGroup = bgmMixer;
        sfxSource.outputAudioMixerGroup = sfxMixer;

        BuildClipDict();
    }

    private void Start()
    {
        // AudioMixer는 첫 프레임에 스냅샷을 로드하므로 SetFloat을 Awake에서
        // 호출하면 무시된다. Start에서 직렬화된 초기 볼륨을 믹서에 반영.
        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
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

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume(masterVolumeParam, masterVolume);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume(bgmVolumeParam, bgmVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume(sfxVolumeParam, sfxVolume);
    }

    // 선형 볼륨(0~1)을 dB로 변환해 믹서 노출 파라미터에 적용
    private void ApplyMixerVolume(string param, float linearVolume)
    {
        float db = linearVolume <= 0.0001f
            ? MinVolumeDb
            : Mathf.Log10(linearVolume) * 20f;

        mixer.SetFloat(param, db);
    }
}
