using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject optionPanel;
    public bool IsOpen => optionPanel.activeSelf;
    [SerializeField] private Button closeButton;
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TMP_Text bgmVolumeText;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeText;

    private void Awake()
    {
        // 슬라이더 값(0~1) → 텍스트(0~100) + 믹서 볼륨
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        closeButton.onClick.AddListener(() => ShowOptionPanel(false));
    }

    private void OnEnable()
    {
        // 패널을 열 때마다 현재 볼륨으로 슬라이더 동기화
        var audioManager = AudioManager.Instance;

        masterVolumeSlider.value = audioManager.MasterVolume;
        bgmVolumeSlider.value = audioManager.BGMVolume;
        sfxVolumeSlider.value = audioManager.SFXVolume;

        // value가 그대로일 경우 onValueChanged가 안 불릴 수 있어 텍스트 직접 갱신
        UpdateVolumeText(masterVolumeText, masterVolumeSlider.value);
        UpdateVolumeText(bgmVolumeText, bgmVolumeSlider.value);
        UpdateVolumeText(sfxVolumeText, sfxVolumeSlider.value);
    }

    private void OnDestroy()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    public void ShowOptionPanel(bool isOpen)
    {
        optionPanel.SetActive(isOpen);
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        UpdateVolumeText(masterVolumeText, value);
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        UpdateVolumeText(bgmVolumeText, value);
        AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        UpdateVolumeText(sfxVolumeText, value);
        AudioManager.Instance.SetSFXVolume(value);
    }

    private static void UpdateVolumeText(TMP_Text text, float value)
    {
        value = Mathf.RoundToInt(value * 100f);
        text.SetText("{0}", value);
    }
}
