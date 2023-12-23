using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class PlayerCtrl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkingForwardSpeed;
    [SerializeField] private float _walkingBackwardSpeed;
    [SerializeField] private float _walkingStrafeSpeed;
    [SerializeField] private float _walkingForwardOnAirSpeed;
    [SerializeField] private float _walkingBackwardOnAirSpeed;
    [SerializeField] private float _walkingStrafeOnAirSpeed;
    [SerializeField] private CharacterController _characterController;

    [Header("Rotation")]
    [SerializeField] private float _viewSensitiveVer;
    [SerializeField] private float _viewSensitiveHor;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _player;

    [Header("Gravity")] 
    [SerializeField] private float _gravity = -9.8f;

    [Header("Jumping")] 
    [SerializeField] private float _jumpingHeight;
    [SerializeField] private Vector3 _velocityJumping;
    [SerializeField] private float _smoothJumping;
    [SerializeField] private float _jumpingFalloff;
    [SerializeField] private Vector3 _velocityJumpingFalloff;
    [SerializeField] private float _smoothJumpingFalloff;
    private float jumpHeight;
    private bool isJump;

    [Header("Player Stance")] 
    public Stance PlayerStance = Stance.Stand;
    [SerializeField] private float _stanceSmoothing;
    [SerializeField] private StanceInfo[] _stanceInfos;

    [Header("Raycast Info")] 
    [SerializeField] private Transform _headStandTf;
    [SerializeField] private Transform _headCrouchTf;
    [SerializeField] private Transform _headProneTf;
    [SerializeField] private bool _checkRayStandAndCrouch;
    
    private StanceInfo curStanceInfo = null;
    private Vector3 vtSmoothCam = Vector3.zero;
    private Vector3 vtSmoothCrtCenter = Vector3.zero;
    private float vtSmoothCrtHeight = 0f;
    private Stance curStance
    {
        get => curStanceInfo.StanceType;
        set
        {
            if (curStanceInfo != null && curStanceInfo.StanceType == value) return;
            curStanceInfo = Array.Find(_stanceInfos, x => x.StanceType == value);
            camPos = new Vector3(0, curStanceInfo.StanceHeight, -0.2399998f);
            vtSmoothCam = Vector3.zero;
            vtSmoothCrtCenter = Vector3.zero;
            vtSmoothCrtHeight = 0f;
        }
    }
    private Vector3 camPos;
    private Stance nextStance;
    private Vector2 movementCharacter;
    private float rotateX;
    private float rotateY;
    private InputBase inputBase;
    private Vector3 velocity;
    private bool checkJumpOnStep;

    private void Awake()
    {
        inputBase = new InputBase();
        curStance = PlayerStance;
        nextStance = PlayerStance;
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
        if (!_characterController.isGrounded) return;
        nextStance = nextStance == Stance.Prone ? Stance.Stand : Stance.Prone;
    }

    private void CharacterCrouchInputBase(InputAction.CallbackContext obj)
    {
        if (!_characterController.isGrounded) return;
        if (obj.canceled)
        {
            if (nextStance == Stance.Crouch)
            {
                nextStance = Stance.Stand;
            }
        }
        else if(nextStance != Stance.Crouch)
        {
            nextStance = Stance.Crouch;
        }
    }
    
    private void CharacterJumpInputBase(InputAction.CallbackContext obj)
    {
        if (!_characterController.isGrounded || _checkRayStandAndCrouch) return;
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
        UpdateCamera();
        UpdateStance();
    }

    private void UpdateStance()
    {
        Vector3 startPosRay = Vector3.zero;
        Vector3 endPosRay = Vector3.zero;

        switch (curStance)
        {
            case Stance.Stand:
                startPosRay = _headStandTf.position;
                endPosRay = _headStandTf.position;
                break;
            case Stance.Crouch:
                startPosRay = _headCrouchTf.position;
                endPosRay = _headStandTf.position;
                break;
            case Stance.Prone:
                startPosRay = _headProneTf.position;
                endPosRay = _headCrouchTf.position;
                break;
            
        }

        endPosRay.y += _characterController.stepOffset;
        Debug.DrawLine(startPosRay,endPosRay,Color.red);
        _checkRayStandAndCrouch = Physics.Raycast(startPosRay, Vector3.up, endPosRay.y - startPosRay.y);
        
        
        for (int i = 0; i < 8; i++)
        {
            var vtDir = Quaternion.Euler(0, i * 45, 0) * Vector3.right;
            Debug.DrawRay(endPosRay,vtDir * _characterController.radius,Color.red);
            if (!_checkRayStandAndCrouch)
            {
                _checkRayStandAndCrouch = Physics.Raycast(endPosRay, vtDir, _characterController.radius);
            }
        }

        if (_checkRayStandAndCrouch)
        {
            isJump = false;
            velocity.y = 0f;
        }
        if (curStance == nextStance) return;
        var stanceInfoNext = Array.Find(_stanceInfos, x => x.StanceType == nextStance);
        if (stanceInfoNext.ColliderHeight < curStanceInfo.ColliderHeight)
        {
            curStance = nextStance;
            return;
        }
        switch (curStance)
        {
            case Stance.Crouch:
                if (_checkRayStandAndCrouch) return;
                curStance = nextStance;
                break;
            case Stance.Prone:
                if (_checkRayStandAndCrouch) return;
                curStance = Stance.Crouch;
                break;
        }
    }

    private void UpdateCamera()
    {
        if (_camera.localPosition == camPos) return;
        _camera.localPosition = Vector3.SmoothDamp(_camera.localPosition, camPos,ref vtSmoothCam, _stanceSmoothing);
        _characterController.center = Vector3.SmoothDamp(_characterController.center, curStanceInfo.ColliderCenter,
            ref vtSmoothCrtCenter, _stanceSmoothing);
        _characterController.height = Mathf.SmoothDamp(_characterController.height, curStanceInfo.ColliderHeight,
            ref vtSmoothCrtHeight, _stanceSmoothing);
    }

    private void CalculateMovement()
    {
        //update stance
        if (movementCharacter != Vector2.zero || isJump)
        {
            if (nextStance == Stance.Prone)
            {
                nextStance = Stance.Stand;
                return;
            }
        }

        Vector3 movementVelocity = Vector3.zero;
        

        //movement
        if (movementCharacter != Vector2.zero)
        {
            var walkingBackwardSpeed = _characterController.isGrounded ? _walkingBackwardSpeed : _walkingBackwardOnAirSpeed;
            var walkingForwardSpeed = _characterController.isGrounded ? _walkingForwardSpeed : _walkingForwardOnAirSpeed;
            var walkingStrafeSpeed = _characterController.isGrounded ? _walkingStrafeSpeed : _walkingStrafeOnAirSpeed;
            var verSpeed = movementCharacter.y * ((movementCharacter.y < 0 ? walkingBackwardSpeed : walkingForwardSpeed) + curStanceInfo.SpeedChange) * Time.deltaTime;
        
            var horSpeed = movementCharacter.x * (walkingStrafeSpeed + curStanceInfo.SpeedChange) * Time.deltaTime;

            movementVelocity += _player.TransformDirection(horSpeed, 0f, verSpeed);
        }
        //movement

        
        //jump

        if (isJump)
        {
            if (Math.Abs(jumpHeight - _jumpingHeight) < 0.5f || (checkJumpOnStep && _characterController.isGrounded))
            {
                isJump = false;
                velocity.y = 0f;
            }
            else
            {
                checkJumpOnStep = true;
                var jumpStep = Mathf.Lerp(jumpHeight, _jumpingHeight, Time.deltaTime * 3f);
                movementVelocity += (jumpStep - jumpHeight) * Vector3.up;
                jumpHeight = jumpStep;
            }
        }
        else
        {
            //gravity
            if (_characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -1f;
            }
            else
            {
                velocity.y += _gravity * Time.deltaTime * Time.deltaTime * 1/2f;
            }
            movementVelocity += velocity;
            //gravity
        }
        //jump
        _characterController.Move(movementVelocity);
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
        if (curStance != Stance.Stand)
        {
            nextStance = Stance.Stand;
            return;
        }
        
        checkJumpOnStep = false;
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
    public float ColliderHeight;
    public Vector3 ColliderCenter;
    public float SpeedChange;
}
