using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageEndPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageEndText;
    [field: SerializeField] public PlayerStatInfo PlayerStatInfo { get; private set; }
    [SerializeField] private Button stageEndButton;
    [field: SerializeField] public ItemGridUI ItemGridUI { get; private set; }

    private void Awake()
    {
        stageEndButton.onClick.AddListener(() => SceneChanger.Instance.LoadScene(SceneName.TitleScene));
    }
}
