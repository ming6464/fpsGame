using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkingForwardSpeed;
    [SerializeField] private float _walkingBackwardSpeed;
    [SerializeField] private float _walkingStrafeSpeed;
    [SerializeField] private CharacterController _characterController;

    [Header("Rotation")]
    [SerializeField] private float _viewSensitiveVer;
    [SerializeField] private float _viewSensitiveHor;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _player;

    [Header("Gravity")] 
    [SerializeField] private float _gravity = -9.8f;

    [Header("Jumping")] 
    [SerializeField] private float _jumpHeight;
    private float jumpHeight;
    private bool isJump;

    [Header("Player Stance")] 
    [SerializeField] private Stance _playerStance = Stance.Stand;
    [SerializeField] private float _stanceSmoothing;
    [SerializeField] private StanceInfo[] _stanceInfos;

    private Tweener _changeStanceTweener;
    
    private Stance playerStance
    {
        get => _playerStance;
        set
        {
            if (_playerStance != value)
            {
                changeStance = true;
                _playerStance = value;
            }
        }
    }


    private Vector2 movementCharacter;
    private float rotateX;
    private float rotateY;
    private InputBase inputBase;
    public bool isGrounded;
    private Vector3 velocity;
    private bool checkJumpOnStep;
    private bool changeStance;

    
    private void Awake()
    {
        inputBase = new InputBase();
    }

    private void OnEnable()
    {
        inputBase.Character.Movement.performed += CharacterMovementInputBase;
        inputBase.Character.View.performed += CharacterViewInputBase;
        inputBase.Character.Jump.performed += CharacterJumpInputBase;
        inputBase.Character.Crouch.performed += CharacterCrouchInputBase;
        inputBase.Character.Crouch.canceled += CharacterCrouchInputBase;
        inputBase.Character.Prone.performed += CharacterProneInputBase;
    }
    
    private void OnDisable()
    {
        inputBase.Character.Movement.performed -= CharacterMovementInputBase;
        inputBase.Character.View.performed -= CharacterViewInputBase;
        inputBase.Character.Jump.performed -= CharacterJumpInputBase;
        inputBase.Character.Crouch.performed -= CharacterCrouchInputBase;
        inputBase.Character.Crouch.canceled -= CharacterCrouchInputBase;
        inputBase.Character.Prone.performed -= CharacterProneInputBase;
    }

    private void CharacterProneInputBase(InputAction.CallbackContext obj)
    {
        playerStance = playerStance == Stance.Prone ? Stance.Stand : Stance.Prone;
    }

    private void CharacterCrouchInputBase(InputAction.CallbackContext obj)
    {
        if (obj.canceled)
        {
            if (playerStance == Stance.Crouch)
            {
                playerStance = Stance.Stand;
            }
        }
        else if(playerStance != Stance.Crouch)
        {
            playerStance = Stance.Crouch;
        }
    }
    
    private void CharacterJumpInputBase(InputAction.CallbackContext obj)
    {
        Jump();
    }

    private void CharacterViewInputBase(InputAction.CallbackContext obj)
    {
        UpdateViewCamera(obj.ReadValue<Vector2>());
    }
    
    private void CharacterMovementInputBase(InputAction.CallbackContext obj)
    {
        movementCharacter = obj.ReadValue<Vector2>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputBase.Enable();
        rotateX = _camera.localRotation.eulerAngles.x;
        rotateY = _player.localRotation.eulerAngles.y;
    }

    private void Update()
    {
        CalculateMovement();
        UpdateStance();
        isGrounded = _characterController.isGrounded;
    }

    private void UpdateStance()
    {
        if (changeStance)
        {
            changeStance = false;
            float stanceHeight = Array.Find(_stanceInfos, x => x.StanceType == playerStance).StanceHeight;
            if (_changeStanceTweener.IsActive()) _changeStanceTweener.Kill();
            Debug.Log($"- {stanceHeight}");
            var localPosition = _camera.localPosition;
            _changeStanceTweener = _camera.DOLocalMove(new Vector3(localPosition.x,stanceHeight,localPosition.z), _stanceSmoothing);
        }
    }

    private void CalculateMovement()
    {
        var vtSpeed = Vector3.zero;
        
        //movement
        if (movementCharacter != Vector2.zero)
        {
            if (playerStance == Stance.Prone) playerStance = Stance.Stand;
            var verSpeed = movementCharacter.y * (movementCharacter.y < 0 ? _walkingBackwardSpeed : _walkingForwardSpeed) * Time.deltaTime;
        
            var horSpeed = movementCharacter.x * _walkingStrafeSpeed * Time.deltaTime;

            vtSpeed = _player.TransformDirection(horSpeed, 0f, verSpeed);
        
            _characterController.Move(vtSpeed);
        }
        //movement

        
        //jump

        if (isJump)
        {
            if (playerStance == Stance.Prone) playerStance = Stance.Stand;
            if (Math.Abs(jumpHeight - _jumpHeight) < 0.5f || (checkJumpOnStep && _characterController.isGrounded))
            {
                Debug.Log("1");
                isJump = false;
                velocity.y = 0f;
            }
            else
            {
                checkJumpOnStep = true;
                Debug.Log("2");
                var jumpStep = Mathf.Lerp(jumpHeight, _jumpHeight, Time.deltaTime * 3f);
                _characterController.Move((jumpStep - jumpHeight) * Vector3.up);
                jumpHeight = jumpStep;
            }
        }
        else
        {
            Debug.Log("3");
            //gravity
        
            if (_characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -1f;
            }
            else
            {
                velocity.y += _gravity * Time.deltaTime * Time.deltaTime * 1/2f;
            }
        
            _characterController.Move(velocity);

            //gravity
        }
        //jump
        
        
        


    }

    private void UpdateViewCamera(Vector2 vt)
    {
        rotateX -= vt.y * _viewSensitiveVer  * Time.deltaTime;
        rotateY = vt.x * _viewSensitiveHor * Time.deltaTime;

        rotateX = Math.Clamp(rotateX, -62f, 44f);
        _camera.localRotation = Quaternion.Euler(rotateX, 0, 0);
        
        _player.Rotate(Vector3.up,rotateY);
    }
    
    
    private void Jump()
    {
        if(!_characterController.isGrounded) return;
        checkJumpOnStep = false;
        Debug.Log("Jump");
        jumpHeight = 0f;
        isJump = true;
    }
}

[Serializable]
public enum Stance
{
    Stand,
    Crouch,
    Prone
}

[Serializable]
public class StanceInfo
{
    public Stance StanceType;
    public float StanceHeight;
}
