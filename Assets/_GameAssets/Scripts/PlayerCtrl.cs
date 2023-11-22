using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float _gravityAmount;
    [SerializeField] private float _gravityMin;
    private float playerGravity;

    [Header("Jumping")] 
    [SerializeField] private float _jumpingHeight;
    [SerializeField] private float _jumpingFalloff;
    [SerializeField] private Vector3 jumpingForce;
    [SerializeField] private Vector3 jumpingForceVelocity;
    
    private Vector2 _movementCharacter;
    private float rotateX;
    private float rotateY;
    private InputBase inputBase;

    
    private void Awake()
    {
        inputBase = new InputBase();
    }

    private void OnEnable()
    {
        inputBase.Character.Movement.performed += CharacterMovementInputBase;
        inputBase.Character.View.performed += CharacterViewInputBase;
        inputBase.Character.Jump.performed += CharacterJumpInputBase;
    }

    private void OnDisable()
    {
        inputBase.Character.Movement.performed -= CharacterMovementInputBase;
        inputBase.Character.View.performed -= CharacterViewInputBase;
        inputBase.Character.Jump.performed -= CharacterJumpInputBase;
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
        _movementCharacter = obj.ReadValue<Vector2>();
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
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        var verSpeed = _movementCharacter.y * (_movementCharacter.y < 0 ? _walkingBackwardSpeed : _walkingForwardSpeed) * Time.deltaTime;
        
        var horSpeed = _movementCharacter.x * _walkingStrafeSpeed * Time.deltaTime;

        var vtSpeed = _player.TransformDirection(horSpeed, 0f, verSpeed);
        

        
        if (playerGravity > _gravityMin)
        {
            playerGravity -= _gravityAmount * Time.deltaTime;
        }
        
        if (playerGravity < -1 && _characterController.isGrounded)
        {
            playerGravity = -1;
        }

        vtSpeed.y += playerGravity;
        
        _characterController.Move(vtSpeed);
    }

    private void UpdateViewCamera(Vector2 vt)
    {
        rotateX -= vt.y * _viewSensitiveVer  * Time.deltaTime;
        rotateY = vt.x * _viewSensitiveHor * Time.deltaTime;

        rotateX = Math.Clamp(rotateX, -62f, 44f);
        _camera.localRotation = Quaternion.Euler(rotateX, 0, 0);
        
        
        
        _player.Rotate(Vector3.up,rotateY);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp()
    }
    
    private void Jump()
    {
        if(!_characterController.isGrounded) return;
        Debug.Log("Jump");
        jumpingForce = Vector3.up * _jumpingHeight;
    }
}
