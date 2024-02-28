using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float _walkingForwardSpeed;

    [SerializeField]
    private float _walkingBackwardSpeed;

    [SerializeField]
    private float _walkingStrafeSpeed;

    [SerializeField]
    private float _walkingForwardOnAirSpeed;

    [SerializeField]
    private float _walkingBackwardOnAirSpeed;

    [SerializeField]
    private float _walkingStrafeOnAirSpeed;

    [SerializeField]
    private CharacterController _characterController;

    [Header("Rotation")]
    [SerializeField]
    private Vector2 _viewSensitive;

    [SerializeField]
    private Transform _camera;

    [SerializeField]
    private Transform _player;


    [Header("Jumping")]
    [SerializeField]
    private Vector3 _jumpingHeight;

    [SerializeField]
    private float _jumpingSmoothTime;

    [SerializeField]
    private Vector3 _jumpingFalloff;

    [SerializeField]
    private float _jumpingFalloffSmoothTime;

    [Header("Player Stance")]
    public Stance PlayerStance = Stance.Stand;

    [SerializeField]
    private float _stanceSmoothing;

    [SerializeField]
    private StanceInfo[] _stanceInfos;

    [Header("Raycast Info")]
    [SerializeField]
    private Transform _headStandTf;

    [SerializeField]
    private Transform _headCrouchTf;

    [SerializeField]
    private Transform _headProneTf;

    [SerializeField]
    private bool _checkRayStandAndCrouch;

    [Header("Weapon")]
    [SerializeField]
    private WeaponManager _weaponManager;

    private Vector2 inputView = Vector2.zero;
    public Vector2 InputView => inputView;

    private StanceInfo curStanceInfo;
    private Vector3 vtSmoothCam = Vector3.zero;
    private Vector3 vtSmoothCrtCenter = Vector3.zero;
    private float vtSmoothCrtHeight;

    private Vector3 vtRotate;
    private Vector2 curRotate;

    private Stance curStance
    {
        get => curStanceInfo.StanceType;
        set
        {
            if (curStanceInfo != null && curStanceInfo.StanceType == value)
            {
                return;
            }

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
    private InputBase inputBase;

    private Vector3 vt_jump;
    private Vector3 jumpHeight;
    private bool isJump;
    private Vector3 vtJumping;
    private Vector3 vtJumpingFalloff;

    private void Awake()
    {
        inputBase = new InputBase();
        curStance = PlayerStance;
        nextStance = PlayerStance;
    }

    private void OnEnable()
    {
        LinkInputSystem();
    }

    private void OnDisable()
    {
        UnlinkInputSystem();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputBase.Enable();
        curRotate.x = _camera.localRotation.eulerAngles.x;
        curRotate.y = _player.localRotation.eulerAngles.y;
    }

    private void Update()
    {
        UpdateViewCamera();
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
        Debug.DrawLine(startPosRay, endPosRay, Color.red);
        _checkRayStandAndCrouch = Physics.Raycast(startPosRay, Vector3.up, endPosRay.y - startPosRay.y);


        // for (int i = 0; i < 8; i++)
        // {
        //     Vector3 vtDir = Quaternion.Euler(0, i * 45, 0) * Vector3.right;
        //     Debug.DrawRay(endPosRay, vtDir * _characterController.radius, Color.red);
        //     if (!_checkRayStandAndCrouch)
        //     {
        //         _checkRayStandAndCrouch = Physics.Raycast(endPosRay, vtDir, _characterController.radius);
        //     }
        // }

        if (_checkRayStandAndCrouch)
        {
            isJump = false;
        }

        if (curStance == nextStance)
        {
            return;
        }

        StanceInfo stanceInfoNext = Array.Find(_stanceInfos, x => x.StanceType == nextStance);
        if (stanceInfoNext.ColliderHeight < curStanceInfo.ColliderHeight)
        {
            curStance = nextStance;
            return;
        }

        switch (curStance)
        {
            case Stance.Crouch:
                if (_checkRayStandAndCrouch)
                {
                    return;
                }

                curStance = nextStance;
                break;
            case Stance.Prone:
                if (_checkRayStandAndCrouch)
                {
                    return;
                }

                curStance = Stance.Crouch;
                break;
        }
    }

    private void UpdateCamera()
    {
        if (_camera.localPosition == camPos)
        {
            return;
        }

        _camera.localPosition = Vector3.SmoothDamp(_camera.localPosition, camPos, ref vtSmoothCam, _stanceSmoothing);
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
            float walkingBackwardSpeed =
                _characterController.isGrounded ? _walkingBackwardSpeed : _walkingBackwardOnAirSpeed;
            float walkingForwardSpeed =
                _characterController.isGrounded ? _walkingForwardSpeed : _walkingForwardOnAirSpeed;
            float walkingStrafeSpeed = _characterController.isGrounded ? _walkingStrafeSpeed : _walkingStrafeOnAirSpeed;
            float verSpeed = movementCharacter.y *
                             ((movementCharacter.y < 0 ? walkingBackwardSpeed : walkingForwardSpeed) +
                              curStanceInfo.SpeedChange) * Time.deltaTime;

            float horSpeed = movementCharacter.x * (walkingStrafeSpeed + curStanceInfo.SpeedChange) * Time.deltaTime;

            movementVelocity += _player.TransformDirection(horSpeed, 0f, verSpeed);
        }
        //movement


        //jump
        vt_jump = Vector3.zero;
        if (isJump)
        {
            if (Math.Abs(jumpHeight.y - _jumpingHeight.y) <= 0.5f)
            {
                isJump = false;
                jumpHeight = Vector3.zero;
                vtJumping = Vector3.zero;
                vtJumpingFalloff = Vector3.zero;
            }
            else
            {
                if (jumpHeight.y < 0)
                {
                    jumpHeight = Vector3.zero;
                }

                Vector3 jumpStep = Vector3.SmoothDamp(jumpHeight, _jumpingHeight, ref vtJumping, _jumpingSmoothTime);
                vt_jump += jumpStep - jumpHeight;
                jumpHeight = jumpStep;
            }
        }
        else
        {
            if (_characterController.isGrounded)
            {
                jumpHeight = Vector3.zero;
                vt_jump = Vector3.down;
                vtJumpingFalloff = Vector3.zero;
            }
            else
            {
                if (jumpHeight.y > 0 || vt_jump.y > 0)
                {
                    jumpHeight = Vector3.zero;
                    vt_jump = Vector3.down;
                    vtJumpingFalloff = Vector3.zero;
                }

                vt_jump = Vector3.SmoothDamp(jumpHeight, _jumpingFalloff, ref vtJumpingFalloff,
                    _jumpingFalloffSmoothTime);
            }
        }

        movementVelocity += vt_jump;
        //jump
        _characterController.Move(movementVelocity);
    }

    private void UpdateViewCamera()
    {
        curRotate.x -= inputView.y * _viewSensitive.y * Time.deltaTime;
        curRotate.y += inputView.x * _viewSensitive.x * Time.deltaTime;

        curRotate.x = Math.Clamp(curRotate.x, -85f, 85f);
        _camera.localRotation = Quaternion.Euler(curRotate.x, 0, 0);

        _player.localRotation = Quaternion.Euler(0, curRotate.y, 0);
    }

    public void RotateCam(Vector2 vtRotate)
    {
        curRotate += vtRotate;
    }

    private void Jump()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }

        if (curStance != Stance.Stand)
        {
            nextStance = Stance.Stand;
            return;
        }

        isJump = true;
        jumpHeight = Vector3.zero;
        vtJumping = Vector3.zero;
        vtJumpingFalloff = Vector3.zero;
    }

#region -InputSystem handler-

    private void LinkInputSystem()
    {
        inputBase.Character.Movement.performed += CharacterMovementInputBase;
        inputBase.Character.View.performed += CharacterViewInputBase;
        inputBase.Character.Jump.performed += CharacterJumpInputBase;
        inputBase.Character.Crouch.performed += CharacterCrouchInputBase;
        inputBase.Character.Crouch.canceled += CharacterCrouchInputBase;
        inputBase.Character.Prone.performed += CharacterProneInputBase;
    }

    private void UnlinkInputSystem()
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
        if (!_characterController.isGrounded)
        {
            return;
        }

        nextStance = nextStance == Stance.Prone ? Stance.Stand : Stance.Prone;
    }

    private void CharacterCrouchInputBase(InputAction.CallbackContext obj)
    {
        if (!_characterController.isGrounded)
        {
            return;
        }

        if (obj.canceled)
        {
            if (nextStance == Stance.Crouch)
            {
                nextStance = Stance.Stand;
            }
        }
        else if (nextStance != Stance.Crouch)
        {
            nextStance = Stance.Crouch;
        }
    }

    private void CharacterJumpInputBase(InputAction.CallbackContext obj)
    {
        if (!_characterController.isGrounded || _checkRayStandAndCrouch)
        {
            return;
        }

        Jump();
    }

    private void CharacterViewInputBase(InputAction.CallbackContext obj)
    {
        inputView = obj.ReadValue<Vector2>();
    }

    private void CharacterMovementInputBase(InputAction.CallbackContext obj)
    {
        movementCharacter = obj.ReadValue<Vector2>();
    }

#endregion
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