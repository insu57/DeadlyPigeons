using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    //private StageManager _stageManager;
    private Rigidbody2D _rigidbody2D;
    private Vector2 _moveInput;
    public event Action OnShowInfoUI;
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
    }
    
    private void Start()
    { 
        InputManager.Instance.Input.Global.Menu.performed += ShowInfoUI;
        //InputManager.Instance.Input.Player
    }

    private void Update()
    {
        _moveInput = InputManager.Instance.Input.Player.Move.ReadValue<Vector2>();
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        _rigidbody2D.linearVelocity = _moveInput * 5f;
    }

    private void ShowInfoUI(InputAction.CallbackContext context)
    {
        InputManager.Instance.Input.UI.Enable();
        InputManager.Instance.Input.Player.Disable();
        
        OnShowInfoUI?.Invoke();
    }
}
