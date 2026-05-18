using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveTimer;
    [SerializeField] private TextMeshProUGUI currentWaveTxt;
    
    [Header("Icon")]
    [SerializeField] private GameObject crateIcon;
    [SerializeField] private TextMeshProUGUI crateCountTxt;
    [SerializeField] private GameObject lvUpIcon;
    [SerializeField] private TextMeshProUGUI lvUpCountTxt;
    
    [Header("Store")]
    [SerializeField] private GameObject storeUI;
    [SerializeField] private ItemGridUI itemGridUI;
    [SerializeField] private Button nextWaveBtn;

    private StringBuilder _sb;

    private void Awake()
    {
        _sb = StatUtil.StringBuilder;
        storeUI.SetActive(false);
    }

    public void Init(PlayerManager playerManager)
    {
        itemGridUI.Init(playerManager);
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

    public void UpdateCrateCount(int count)
    {
        _sb.Clear();
        _sb.Append('x').Append(count);
        crateCountTxt.SetText(_sb);
        if(count > 0) crateIcon.SetActive(true);
    }

    public void UpdateLvUpCount(int count)
    {
        _sb.Clear();
        _sb.Append('x').Append(count);
        lvUpCountTxt.SetText(_sb);
        if (count > 0) lvUpIcon.SetActive(true);
    }
    
    public void OpenStoreUI(bool isOpen)
    {
        storeUI.SetActive(isOpen);
        if (isOpen)
        {
            
        }
    }
}
