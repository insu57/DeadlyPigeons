using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    [field: SerializeField] private TMP_Text statName;
    [field: SerializeField] private TMP_Text statValue;
    [field: SerializeField] private Image tierBorder;
    [field: SerializeField] private Image statIcon;
    [field: SerializeField] public Button SelectBtn { get; private set; }

    public void SetUpgrade(MainStats mainStat, int tier)
    {
        statName.text = mainStat.MainStatsToString();
        var sb = StatUtil.StringBuilder;
        sb.Clear();
        var upgradeValue = DataManager.Instance.LevelUpStatsDict[mainStat][tier - 1];//Tier 1~4(idx는 0~3)
        sb.Append(mainStat.GetIcons());
        sb.AppendColor("+", 1);
        sb.AppendColorValue(upgradeValue);
        sb.Append(mainStat.MainStatsToString());
        statValue.SetText(sb);
        statIcon.sprite = DataManager.Instance.MainStatSpriteDict[mainStat];
        var colorStr = DataManager.Instance.TierColorDict[tier];
        tierBorder.color = DataManager.Instance.GetHexToColor(colorStr);
    }
}
