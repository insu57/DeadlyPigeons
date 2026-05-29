using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StoreButtons : MonoBehaviour
{
    [field: SerializeField] public Button CombineButton { get; private set; }
    [field: SerializeField] public Button RecycleButton { get; private set; }
    [field: SerializeField] public TextMeshProUGUI RecyclePriceTxt { get; private set; }
    [field: SerializeField] public Button CancelButton { get; private set; }
}
