using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    //private StageManager _stageManager;
    private Rigidbody2D _rigidbody2D;
    private Vector2 _moveInput;
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
    }
    
    private void Start()
    { 
       
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
    
}
