using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    TitleScene,
    MainScene
}

public class SceneChanger : Singleton<SceneChanger>
{
    //개선 필요
    public void LoadScene(SceneName sceneName)
    {
        string sceneString = SceneNameChangeToString(sceneName);
        
        if (sceneString == "")
        {
            Debug.LogWarning("No scene name provided");
            return; //Error
        }
        
        
        StartCoroutine(LoadSceneProcess(sceneString));
    }

    private string SceneNameChangeToString(SceneName sceneName)
    {
        switch (sceneName)
        {
            case SceneName.TitleScene: return "TitleScene";
            case SceneName.MainScene: return "MainScene";
            default: return "";
        }
    }
    
    
    private IEnumerator LoadSceneProcess(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
    
}
