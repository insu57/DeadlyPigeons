using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StatAmount
{
    public MainStats mainStat;
    public SubStats subStat;
    public int amount;
}

[Serializable]
public struct ItemStat
{
    public string itemName;
    public string description;
    public int tier;
    public Sprite icon;
    public List<StatAmount> statValues;
    public List<StatAmount> statMultipliers;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    //캐릭터 패시브 아이템은 캐릭터ID와 동일하게
    //1. 단순 스탯 증감
    //2. 스탯 변화량 추가 증감
    //3. 무기 제한 -> 상점부터 구현하고
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string ItemName { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public List<StatAmount> StatValues { get; private set; } //스탯 수치 증감량
    [field: SerializeField] public List<StatAmount> StatMultipliers { get; private set; }//스탯 변화량 추가 증감

#if UNITY_EDITOR
    public void SyncItemData(ItemStat itemStat)
    {
        ItemName = itemStat.itemName;
        Description = itemStat.description;
        Icon = itemStat.icon;
        StatValues = itemStat.statValues;
        StatMultipliers = itemStat.statMultipliers;
    }
#endif
    // 아이템 데이터 싱크-> 캐릭터 패시브, 아이템 시트 => 둘 다 파싱해서 SO로
    // 캐릭터 데이터 -> 스탯 수치에서 패시브SO를 저장하는 것으로 수정.
}
