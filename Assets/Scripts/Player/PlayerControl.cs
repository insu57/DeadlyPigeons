using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    //private StageManager _stageManager;
    private Rigidbody2D _rigidbody2D;
    private InputAction _moveAction;
    private Vector2 _moveInputVector;
    private bool _isFacingRight = true;
    [SerializeField] private GameObject playerSprite; 
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
    }
    
    private void Start()
    {
        _moveAction = InputManager.Instance.Input.Player.Move;
    }

    private void Update()
    {
        MoveInput();
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void MoveInput()
    {
        _moveInputVector = _moveAction.ReadValue<Vector2>();
        
    }

    private void Move()
    {
        if (_moveInputVector.x < 0 && _isFacingRight)
        {
            _isFacingRight = false;
            playerSprite.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (_moveInputVector.x > 0 && !_isFacingRight)
        {
            _isFacingRight = true;
            playerSprite.transform.localScale = new Vector3(1, 1, 1);
        }
        
        _rigidbody2D.linearVelocity = _moveInputVector * 5f;
        
        //맵 제한?
        //1. Wall Collider
        //2. 좌표 제한
        //추후 성능 문제, 떨림 등 문제가 생기면 수정
    }
    
}
