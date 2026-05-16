using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveTimer;
    [SerializeField] private TextMeshProUGUI currentWaveTxt;
    [Header("Store")]
    [SerializeField] private GameObject storeUI;
    
    [SerializeField] private Button nextWaveBtn;

    private StringBuilder _sb;

    private void Awake()
    {
        _sb = StatUtil.StringBuilder;
    }
    
    
    public void SetCurrentWaveText(int wave)
    {
        _sb.Clear();
        _sb.Append("Wave ").Append(wave);
        currentWaveTxt.SetText(_sb);
    }

    public void UpdateWaveTimer(float time)
    {
        _sb.Clear();
        _sb.Append("Time\n").Append(time);
        waveTimer.SetText(_sb);
    }
}
