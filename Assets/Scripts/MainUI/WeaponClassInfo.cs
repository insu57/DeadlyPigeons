using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponClassInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject infoPanelParent;
    [SerializeField] private GameObject infoPanel1;
    [SerializeField] private TMP_Text classText1;
    [SerializeField] private TMP_Text infoText1;
    [SerializeField] private GameObject infoPanel2;
    [SerializeField] private TMP_Text classText2;
    [SerializeField] private TMP_Text infoText2;
    
    private void Awake()
    {
        infoPanelParent.SetActive(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter: WeaponClass");
        infoPanelParent.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoPanelParent.SetActive(false);
    }

    public void ShowClassInfo(List<WeaponClasses> classes, StringBuilder sb)
    {
        infoPanelParent.SetActive(true);
        sb.Clear();

        infoPanel1.SetActive(true);
        var class1 = classes[0];
        classText1.text = WeaponData.WeaponClassToString(class1);
        var effectList1 = DataManager.Instance.WeaponClassDict[class1];
        GetClassEffectTxt(sb, effectList1);
        infoText1.SetText(sb);

        if (classes.Count <= 1)
        {
            infoPanel2.SetActive(false);
            return;
        }
        infoPanel2.SetActive(true);
        
        var class2 = classes[1];
        classText2.text = WeaponData.WeaponClassToString(class2);
        var effectList2 = DataManager.Instance.WeaponClassDict[class2];
        sb. Clear();
        GetClassEffectTxt(sb, effectList2);
        infoText2.SetText(sb);
    }

    private void GetClassEffectTxt(StringBuilder sb, List<WeaponClassEffect> effectList)
    {
        var statNameList = new List<string>();
        foreach (var effect in effectList)
        {
            if (effect.mainStat != MainStats.None)
            {
                statNameList.Add(effect.mainStat.MainStatsToString());
            }
            else
            {
                if(effect.subStat == SubStats.None) Debug.LogWarning("STAT NONE!");
                statNameList.Add(effect.subStat.SubStatsToString());
            }
        }
        
        for (int i = 0; i < 5; i++) //2~5단계 효과
        {
            sb.Append($"({i + 2}) ");
            for (int j = 0; j < effectList.Count; j++)
            {
                if(effectList[j].values[i] == 0) continue;
                if (j > 0) sb.Append(", ");//스탯 증감 효과가 1개가 넘는 경우
                sb.Append($"{effectList[j].values[i]} ").Append(statNameList[j]);
            }
            sb.AppendLine();
        }
    }
}
