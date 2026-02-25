using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public GlobalInput Input{ get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        Input = new GlobalInput();
    }

    private void OnDestroy()
    {
        Input?.Dispose();
    }
}
