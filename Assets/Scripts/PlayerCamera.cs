using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerManager _playerManager;
    
    private void Awake()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
    }

    private void LateUpdate()
    {
        //개선필요.
        transform.position = new Vector3(_playerManager.transform.position.x, _playerManager.transform.position.y, transform.position.z);
    }
}
