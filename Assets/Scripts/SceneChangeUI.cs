using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeUI : MonoBehaviour
{
    [SerializeField] private Button changeButton;
    [SerializeField] private SceneName sceneNameToChange;

    private void Start()
    {
        changeButton.onClick.AddListener(LoadScene);
    }

    private void LoadScene()
    {
        SceneChanger.Instance.LoadScene(sceneNameToChange);
    }
}
