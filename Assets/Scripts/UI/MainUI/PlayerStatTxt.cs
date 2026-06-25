using TMPro;
using UnityEngine;

public class PlayerStatTxt : MonoBehaviour
{
    [field: SerializeField] public TMP_Text StatLabel { get; private set; }
    [field: SerializeField] public TMP_Text StatValue { get; private set; }
}
