using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerDamageReceiver))]
public class PlayerScript : MonoBehaviour
{
    [Header("Player")]
    public float MoveSpeed;

    public float SprintSpeed;
    public float RotationSmoothTime;
    public float SpeedChangeRate;

    [Space(10)]
    public AudioClip LandingAudioClip;

    public AudioClip[] FootstepWalkAudioClips;
    public AudioClip[] FootstepRunAudioClips;

    [Range(0, 1)]
    public float FootstepAudioVolume = 0.5f;

    [Range(0, 2)]
    public float LandingAudioVolume = 1f;

    [Space(10)]
    public float JumpHeight;

    public float Gravity;
    public float TimeJumpNext;
    public float AddForceTimeJumpUp;
    public float TimeOutStepDown;

    [Space(10)]
    public bool Grounded;

    public float GroundedOffset;
    public float GroundRadius;
    public LayerMask GroundLayers;

    [Space(10)]
    public Vector3 HeadOffset;

    public float HeadRadius;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera cameraView1;

    public CinemachineVirtualCamera cameraView2;
    public Transform CinemachineTargetFollow;

    public float TopClamp;
    public float BottomClamp;
    public Vector3 PositionTargetFollow;

    [Header("Aim")]
    public AimAndPivotScript AimAndPivotScript;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector2 _lookView;
    private int _cameraView;
    private bool _isFinishLoadViewV2;

    //player
    private float _speed;
    private float _animationBlendX;
    private float _animationBlendY;
    private float _targetRotation;
    private float _rotationVelocity;
    private float _verticalVelocity;

    //timeout deltaTime
    private float _timeJumpNextDelta;
    private float _addForceTimeJumpUpDelta;
    private float _timeOutStepDownDelta;

    //animation field
    private bool _isJumpingDown;

    //animation IDs
    private int _animIDY;
    private int _animIDX;
    private int _animIDJump;
    private int _animIDGrounded;
    private int _animIDFreeFall;

    //references
    private CharacterController _controller;
    private Animator _animator;
    private InputBase _inputBase;
    private Transform _mainCam;

    //input values
    private Vector2 _move;
    private bool _jump;
    private bool _sprint;

    private void Awake()
    {
        _inputBase = new InputBase();
    }

    private void OnEnable()
    {
        _inputBase.Enable();
        LinkInput();
        LinkEvent();
        CameraSwitcher.RegisterCamera(cameraView1);
        CameraSwitcher.RegisterCamera(cameraView2);
        CameraSwitcher.SwitchCamera(cameraView2);
        AimAndPivotScript.SetUpAim(1);
    }

    private void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnRelaxedHands, OnRelaxedHands);
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnRelaxedHands, OnRelaxedHands);
    }

    private void OnRelaxedHands(object obj)
    {
        bool check = (bool)obj;
        if (_animator)
        {
            _animator.SetLayerWeight(1, check ? 1 : 0);
        }
    }

    private void OnDisable()
    {
        _inputBase.Disable();
        UnLinkEvent();
        CameraSwitcher.UnRegisterCamera(cameraView1);
        CameraSwitcher.UnRegisterCamera(cameraView2);
    }

    private void LinkInput()
    {
        _inputBase.Character.Movement.performed += input => { _move = input.ReadValue<Vector2>(); };
        _inputBase.Character.Jump.performed += _ => { _jump = true; };
        _inputBase.Character.Jump.canceled += _ => { _jump = false; };
        _inputBase.Character.Sprint.performed += _ => { _sprint = true; };
        _inputBase.Character.Sprint.canceled += _ => { _sprint = false; };
        _inputBase.Character.View.performed += input => { _lookView = input.ReadValue<Vector2>(); };
        _inputBase.Character.SwitchCamera.performed += _ =>
        {
            CameraSwitcher.SwitchNextCamera();
            if (CameraSwitcher.IsActiveCamera(cameraView2))
            {
                _isFinishLoadViewV2 = false;
                _targetRotation = transform.eulerAngles.y;
                // if (UIManager.Instance)
                // {
                //     UIManager.Instance.HandleCrossHair(true);
                // }

                AimAndPivotScript.SetUpAim(1);
            }
            else
            {
                // if (UIManager.Instance)
                // {
                //     UIManager.Instance.HandleCrossHair(false);
                // }

                AimAndPivotScript.SetUpAim(0);
            }
        };
    }

    // Start is called before the first frame update
    private void Start()
    {
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _cinemachineTargetYaw = CinemachineTargetFollow.rotation.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        TryGetComponent(out _controller);
        TryGetComponent(out _animator);
        AssignAnimationIDs();
        _timeJumpNextDelta = TimeJumpNext;
        _addForceTimeJumpUpDelta = 0f;
    }

    private void AssignAnimationIDs()
    {
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDX = Animator.StringToHash("X");
        _animIDY = Animator.StringToHash("Y");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
    }

    // Update is called once per frame
    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        CollisionAboveCheck();
        Move();
    }

    private void LateUpdate()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        CinemachineTargetFollow.localPosition = PositionTargetFollow;

        if (_lookView.sqrMagnitude >= 0.1f)
        {
            _cinemachineTargetPitch += _lookView.y;
            if (CameraSwitcher.IsActiveCamera(cameraView1))
            {
                _cinemachineTargetYaw += _lookView.x;
            }
            else
            {
                _targetRotation += _lookView.x;
            }
        }

        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);


        if (CameraSwitcher.IsActiveCamera(cameraView2))
        {
            _targetRotation = ClampAngle(_targetRotation, float.MinValue, float.MaxValue);
            transform.rotation = Quaternion.Euler(0, _targetRotation, 0);
            if (!_isFinishLoadViewV2)
            {
                _cinemachineTargetYaw = Mathf.SmoothDampAngle(CinemachineTargetFollow.eulerAngles.y, _targetRotation,
                    ref _rotationVelocity, 0.1f);
                if (Mathf.Abs(CinemachineTargetFollow.eulerAngles.y - _targetRotation) <= 1f)
                {
                    _isFinishLoadViewV2 = true;
                }
            }
            else
            {
                _cinemachineTargetYaw = _targetRotation;
            }
        }
        else
        {
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            AimAndPivotScript.SetUpAim(_move != Vector2.zero ? 1f : 0f);
        }

        CinemachineTargetFollow.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
    }

    private float ClampAngle(float target, float min, float max)
    {
        if (target >= 360)
        {
            target -= 360;
        }
        else if (target <= -360)
        {
            target += 360;
        }

        return Mathf.Clamp(target, min, max);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _timeOutStepDownDelta = TimeOutStepDown;
            if (_animator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            if (_verticalVelocity <= 0)
            {
                _verticalVelocity = -2f;
            }

            if (_addForceTimeJumpUpDelta > 0)
            {
                _addForceTimeJumpUpDelta -= Time.deltaTime;
                _jump = false;
                if (_addForceTimeJumpUpDelta <= 0)
                {
                    _verticalVelocity = Mathf.Sqrt(-2f * JumpHeight * Gravity);
                }
            }
            else
            {
                if (_jump && _timeJumpNextDelta <= 0f)
                {
                    if (_animator)
                    {
                        _animator.SetBool(_animIDJump, true);
                        _addForceTimeJumpUpDelta = AddForceTimeJumpUp;
                    }
                }
                else if (_timeJumpNextDelta > 0)
                {
                    _timeJumpNextDelta -= Time.deltaTime;
                }
            }
        }
        else
        {
            _timeJumpNextDelta = TimeJumpNext;
            _timeOutStepDownDelta -= Time.deltaTime;

            if (_timeOutStepDownDelta <= 0 && _animator)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }

            _jump = false;
        }

        _verticalVelocity += Gravity * Time.deltaTime;
    }

    private void GroundedCheck()
    {
        Vector3 myPos = transform.position;
        Vector3 spherePos = new(myPos.x, myPos.y + GroundedOffset, myPos.z);
        Grounded = Physics.CheckSphere(spherePos, GroundRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        if (_animator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CollisionAboveCheck()
    {
        if (Physics.CheckSphere(transform.position + HeadOffset, HeadRadius, GroundLayers,
                QueryTriggerInteraction.Ignore) &&
            _verticalVelocity > 0)
        {
            _verticalVelocity = 0;
        }
    }

    private void Move()
    {
        if (_addForceTimeJumpUpDelta > 0 || _isJumpingDown)
        {
            return;
        }

        float targetSpeed = 0;
        if (_move != Vector2.zero)
        {
            targetSpeed = _sprint ? SprintSpeed : MoveSpeed;
        }

        if (_speed != targetSpeed)
        {
            _speed = Mathf.Lerp(_speed, targetSpeed, SpeedChangeRate * Time.deltaTime);
        }

        float dirBlendX = 0;
        if (_move.x != 0)
        {
            dirBlendX = _move.x > 0 ? 1 : -1;
        }

        float dirBlendY = 0;
        if (_move.y != 0)
        {
            dirBlendY = _move.y > 0 ? 1 : -1;
        }

        _animationBlendX = Mathf.Lerp(_animationBlendX, targetSpeed * dirBlendX,
            SpeedChangeRate * Time.deltaTime);
        if (Mathf.Abs(_animationBlendX) <= 0.01f)
        {
            _animationBlendX = 0f;
        }

        _animationBlendY = Mathf.Lerp(_animationBlendY, targetSpeed * dirBlendY,
            SpeedChangeRate * Time.deltaTime);
        if (Mathf.Abs(_animationBlendY) <= 0.01f)
        {
            _animationBlendY = 0f;
        }

        float rotationY = 0;
        if (_move != Vector2.zero)
        {
            if (_move.y == 0)
            {
                if (_move.x > 0)
                {
                    rotationY = 90f;
                }
                else if (_move.x < 0)
                {
                    rotationY = -90f;
                }
            }
            else
            {
                if (_move.x > 0)
                {
                    rotationY = _move.y > 0 ? 45 : 135;
                }
                else if (_move.x < 0)
                {
                    rotationY = _move.y > 0 ? -45 : -135;
                }
                else
                {
                    rotationY = _move.y > 0 ? 0 : 180;
                }
            }
        }

        if (_move != Vector2.zero && _mainCam && CameraSwitcher.IsActiveCamera(cameraView1))
        {
            _targetRotation = _mainCam.eulerAngles.y;
            float rotationAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                ref _rotationVelocity,
                RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotationAngleY, 0f);
        }
        else
        {
            _rotationVelocity = 0f;
        }

        Vector3 dirNormalized = (Quaternion.Euler(0, rotationY, 0) * transform.forward).normalized;

        _controller.Move(dirNormalized * (_speed * Time.deltaTime) +
                         new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
        _animator.SetFloat(_animIDX, _animationBlendX);
        _animator.SetFloat(_animIDY, _animationBlendY);
    }

    private void OnFootStep(int state)
    {
        if ((state == 0 && FootstepWalkAudioClips.Length == 0) || (state == 1 && FootstepRunAudioClips.Length == 0))
        {
            return;
        }

        AudioClip clip;
        if (state == 0)
        {
            clip = FootstepWalkAudioClips[Random.Range(0, FootstepWalkAudioClips.Length)];
        }
        else
        {
            clip = FootstepRunAudioClips[Random.Range(0, FootstepRunAudioClips.Length)];
        }

        AudioSource.PlayClipAtPoint(clip, transform.TransformPoint(_controller.center),
            FootstepAudioVolume);
    }

    private void OnLand()
    {
        if (LandingAudioClip)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center),
                LandingAudioVolume);
        }
    }

    private void OnStartJumpDown()
    {
        _isJumpingDown = true;
    }

    private void OnEndJumpDown()
    {
        _isJumpingDown = false;
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded)
        {
            Gizmos.color = transparentGreen;
        }
        else
        {
            Gizmos.color = transparentRed;
        }

        Vector3 myPos = transform.position;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(myPos.x, myPos.y + GroundedOffset, myPos.z),
            GroundRadius);

        Gizmos.DrawSphere(
            myPos + HeadOffset,
            HeadRadius);
    }
}