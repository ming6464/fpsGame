using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(PlayerDamageReceiver))]
[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    [Header("Player")]
    public float MoveSpeed;

    public float SprintSpeed;
    public float RotationSmoothTime;
    public float SpeedChangeRate;

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
    public Transform CinemachineTargetFollow;

    public Transform AimTargetTf;

    public float TopClamp;
    public float BottomClamp;
    public Vector3 PositionTargetFollow;

    [Header("Aim")]
    public AimAndPivotScript AimAndPivotScript;

    // cinemachine
    private CinemachineVirtualCamera m_cameraView1;
    private CinemachineVirtualCamera m_cameraView2;
    private float m_cinemachineTargetYaw;
    private float m_cinemachineTargetPitch;
    private Vector2 m_lookView;
    private int m_cameraView;
    private bool m_isFinishLoadViewV2;

    //player
    private float m_speed;
    private float m_animationBlendX;
    private float m_animationBlendY;
    private float m_targetRotation;
    private float m_rotationVelocity;
    private float m_verticalVelocity;

    //timeout deltaTime
    private float m_timeJumpNextDelta;
    private float m_addForceTimeJumpUpDelta;
    private float m_timeOutStepDownDelta;

    //animation field
    private bool m_isJumpingDown;

    //animation IDs
    private int m_animIDY;
    private int m_animIDX;
    private int m_animIDJump;
    private int m_animIDGrounded;
    private int m_animIDFreeFall;
    private int m_animIDDie;

    //references
    private CharacterController m_controller;
    private Animator m_animator;
    private InputBase m_inputBase;
    private Transform m_mainCam;

    //input values
    private Vector2 m_move;
    private bool m_jump;
    private bool m_sprint;


    private void Awake()
    {
        GetComponent<PlayerDamageReceiver>().UpdateHpInfo(GameConfig.Instance.GetDataSave().Hp);
        m_inputBase = new InputBase();
        m_mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        m_cameraView1 = GameObject.FindGameObjectWithTag("Cam1").GetComponent<CinemachineVirtualCamera>();
        m_cameraView2 = GameObject.FindGameObjectWithTag("Cam2").GetComponent<CinemachineVirtualCamera>();
        if (AimTargetTf)
        {
            AimTargetTf.parent = m_mainCam;
            AimTargetTf.SetLocalPositionAndRotation(Vector3.forward * 20f, Quaternion.identity);
        }

        if (m_cameraView1)
        {
            m_cameraView1.Follow = CinemachineTargetFollow;
        }

        if (m_cameraView2)
        {
            m_cameraView2.Follow = CinemachineTargetFollow;
        }

        m_controller = GetComponent<CharacterController>();
        m_animator = GetComponent<Animator>();
        AssignAnimationIDs();
        LinkInput();
        CameraSwitcher.UnRegisterAllCam();
    }

    private void OnEnable()
    {
        m_inputBase.Enable();
        LinkEvent();
        CameraSwitcher.RegisterCamera(m_cameraView1);
        CameraSwitcher.RegisterCamera(m_cameraView2);
    }

    private void OnDisable()
    {
        m_inputBase.Disable();
        UnLinkEvent();
        CameraSwitcher.UnRegisterAllCam();
    }

    private void Start()
    {
        CameraSwitcher.SwitchCamera(m_cameraView2);
        AimAndPivotScript.SetUpAim(1);
        m_cinemachineTargetYaw = CinemachineTargetFollow.rotation.eulerAngles.y;
        m_timeJumpNextDelta = TimeJumpNext;
        m_addForceTimeJumpUpDelta = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.IsPauseGame || GameManager.Instance.IsFinishGame))
        {
            return;
        }

        GroundedCheck();
        JumpAndGravity();
        CollisionAboveCheck();
        Move();
    }

    private void LateUpdate()
    {
        if (GameManager.Instance && (GameManager.Instance.IsPauseGame || GameManager.Instance.IsFinishGame))
        {
            return;
        }

        UpdateRotation();
    }

    private void LinkInput()
    {
        m_inputBase.Character.Movement.performed += input => { m_move = input.ReadValue<Vector2>(); };
        m_inputBase.Character.Jump.performed += _ => { m_jump = true; };
        m_inputBase.Character.Jump.canceled += _ => { m_jump = false; };
        m_inputBase.Character.Sprint.performed += _ => { m_sprint = true; };
        m_inputBase.Character.Sprint.canceled += _ => { m_sprint = false; };
        m_inputBase.Character.View.performed += input => { m_lookView = input.ReadValue<Vector2>(); };
        m_inputBase.Character.SwitchCamera.performed += _ =>
        {
            CameraSwitcher.SwitchNextCamera();
            if (CameraSwitcher.IsActiveCamera(m_cameraView2))
            {
                m_isFinishLoadViewV2 = false;
                m_targetRotation = transform.eulerAngles.y;

                EventDispatcher.Instance.PostEvent(EventID.OnHandleCrossHair, true);

                AimAndPivotScript.SetUpAim(1);
            }
            else
            {
                EventDispatcher.Instance.PostEvent(EventID.OnHandleCrossHair, false);

                AimAndPivotScript.SetUpAim(0);
            }
        };
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnRelaxedHands, OnRelaxedHands);
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame, OnFinishGame);
    }

    private void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnRelaxedHands, OnRelaxedHands);
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishGame, OnFinishGame);
    }

    private void OnFinishGame(object obj)
    {
        if (obj == null)
        {
            return;
        }

        CameraSwitcher.SwitchCamera(m_cameraView1);
        bool result = (bool)obj;
        m_inputBase.Disable();
        UnLinkEvent();
        if (!result && m_animator)
        {
            m_animator.SetTrigger(m_animIDDie);
        }
    }

    private void OnRelaxedHands(object obj = null)
    {
        if (m_animator)
        {
            m_animator.SetLayerWeight(1, obj != null ? 1 : 0);
        }
    }

    private void AssignAnimationIDs()
    {
        m_animIDGrounded = Animator.StringToHash("Grounded");
        m_animIDJump = Animator.StringToHash("Jump");
        m_animIDX = Animator.StringToHash("X");
        m_animIDY = Animator.StringToHash("Y");
        m_animIDFreeFall = Animator.StringToHash("FreeFall");
        m_animIDDie = Animator.StringToHash("Die");
    }


    private void UpdateRotation()
    {
        CinemachineTargetFollow.localPosition = PositionTargetFollow;

        if (m_lookView.sqrMagnitude >= 0.05f)
        {
            m_cinemachineTargetPitch += m_lookView.y;
            if (CameraSwitcher.IsActiveCamera(m_cameraView1))
            {
                m_cinemachineTargetYaw += m_lookView.x;
            }
            else
            {
                m_targetRotation += m_lookView.x;
            }
        }

        m_cinemachineTargetPitch = ClampAngle(m_cinemachineTargetPitch, BottomClamp, TopClamp);


        if (CameraSwitcher.IsActiveCamera(m_cameraView2))
        {
            m_targetRotation = ClampAngle(m_targetRotation, float.MinValue, float.MaxValue);
            transform.rotation = Quaternion.Euler(0, m_targetRotation, 0);
            if (!m_isFinishLoadViewV2)
            {
                m_cinemachineTargetYaw = Mathf.SmoothDampAngle(CinemachineTargetFollow.eulerAngles.y, m_targetRotation,
                    ref m_rotationVelocity, 0.1f);
                if (Mathf.Abs(CinemachineTargetFollow.eulerAngles.y - m_targetRotation) <= 1f)
                {
                    m_isFinishLoadViewV2 = true;
                }
            }
            else
            {
                m_cinemachineTargetYaw = m_targetRotation;
            }
        }
        else
        {
            m_cinemachineTargetYaw = ClampAngle(m_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            AimAndPivotScript.SetUpAim(m_move != Vector2.zero ? 1f : 0f);
        }

        CinemachineTargetFollow.rotation = Quaternion.Euler(m_cinemachineTargetPitch, m_cinemachineTargetYaw, 0f);
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
            m_timeOutStepDownDelta = TimeOutStepDown;
            if (m_animator)
            {
                m_animator.SetBool(m_animIDJump, false);
                m_animator.SetBool(m_animIDFreeFall, false);
            }

            if (m_verticalVelocity <= 0)
            {
                m_verticalVelocity = -2f;
            }

            if (m_addForceTimeJumpUpDelta > 0)
            {
                m_addForceTimeJumpUpDelta -= Time.deltaTime;
                m_jump = false;
                if (m_addForceTimeJumpUpDelta <= 0)
                {
                    m_verticalVelocity = Mathf.Sqrt(-2f * JumpHeight * Gravity);
                }
            }
            else
            {
                if (m_jump && m_timeJumpNextDelta <= 0f)
                {
                    if (m_animator)
                    {
                        m_animator.SetBool(m_animIDJump, true);
                        m_addForceTimeJumpUpDelta = AddForceTimeJumpUp;
                    }
                }
                else if (m_timeJumpNextDelta > 0)
                {
                    m_timeJumpNextDelta -= Time.deltaTime;
                }
            }
        }
        else
        {
            m_timeJumpNextDelta = TimeJumpNext;
            m_timeOutStepDownDelta -= Time.deltaTime;

            if (m_timeOutStepDownDelta <= 0 && m_animator)
            {
                m_animator.SetBool(m_animIDFreeFall, true);
            }

            m_jump = false;
        }

        m_verticalVelocity += Gravity * Time.deltaTime;
    }

    private void GroundedCheck()
    {
        Vector3 myPos = transform.position;
        Vector3 spherePos = new(myPos.x, myPos.y + GroundedOffset, myPos.z);
        Grounded = Physics.CheckSphere(spherePos, GroundRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        if (m_animator)
        {
            m_animator.SetBool(m_animIDGrounded, Grounded);
        }
    }

    private void CollisionAboveCheck()
    {
        if (Physics.CheckSphere(transform.position + HeadOffset, HeadRadius, GroundLayers,
                QueryTriggerInteraction.Ignore) &&
            m_verticalVelocity > 0)
        {
            m_verticalVelocity = 0;
        }
    }

    private void Move()
    {
        if (m_addForceTimeJumpUpDelta > 0 || m_isJumpingDown)
        {
            return;
        }

        float targetSpeed = 0;
        if (m_move != Vector2.zero)
        {
            targetSpeed = m_sprint ? SprintSpeed : MoveSpeed;
        }

        if (m_speed != targetSpeed)
        {
            m_speed = Mathf.Lerp(m_speed, targetSpeed, SpeedChangeRate * Time.deltaTime);
        }

        float dirBlendX = 0;
        if (m_move.x != 0)
        {
            dirBlendX = m_move.x > 0 ? 1 : -1;
        }

        float dirBlendY = 0;
        if (m_move.y != 0)
        {
            dirBlendY = m_move.y > 0 ? 1 : -1;
        }

        m_animationBlendX = Mathf.Lerp(m_animationBlendX, targetSpeed * dirBlendX,
            SpeedChangeRate * Time.deltaTime);
        if (Mathf.Abs(m_animationBlendX) <= 0.01f)
        {
            m_animationBlendX = 0f;
        }

        m_animationBlendY = Mathf.Lerp(m_animationBlendY, targetSpeed * dirBlendY,
            SpeedChangeRate * Time.deltaTime);
        if (Mathf.Abs(m_animationBlendY) <= 0.01f)
        {
            m_animationBlendY = 0f;
        }

        float rotationY = 0;
        if (m_move != Vector2.zero)
        {
            if (m_move.y == 0)
            {
                if (m_move.x > 0)
                {
                    rotationY = 90f;
                }
                else if (m_move.x < 0)
                {
                    rotationY = -90f;
                }
            }
            else
            {
                if (m_move.x > 0)
                {
                    rotationY = m_move.y > 0 ? 45 : 135;
                }
                else if (m_move.x < 0)
                {
                    rotationY = m_move.y > 0 ? -45 : -135;
                }
                else
                {
                    rotationY = m_move.y > 0 ? 0 : 180;
                }
            }
        }

        if (m_move != Vector2.zero && m_mainCam && CameraSwitcher.IsActiveCamera(m_cameraView1))
        {
            m_targetRotation = m_mainCam.eulerAngles.y;
            float rotationAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_targetRotation,
                ref m_rotationVelocity,
                RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotationAngleY, 0f);
        }
        else
        {
            m_rotationVelocity = 0f;
        }

        Vector3 dirNormalized = (Quaternion.Euler(0, rotationY, 0) * transform.forward).normalized;

        m_controller.Move(dirNormalized * (m_speed * Time.deltaTime) +
                          new Vector3(0f, m_verticalVelocity * Time.deltaTime, 0f));
        m_animator.SetFloat(m_animIDX, m_animationBlendX);
        m_animator.SetFloat(m_animIDY, m_animationBlendY);
    }

    private void OnFootStep(int state)
    {
        if (!AudioManager.Instance)
        {
            return;
        }

        if (state == 0)
        {
            AudioManager.Instance.PlaySfx(KeySound.WalkFootStepStone);
        }
        else
        {
            AudioManager.Instance.PlaySfx(KeySound.RunFootStepStone);
        }
    }

    private void OnLand()
    {
        AudioManager.Instance.PlaySfx(KeySound.Landing);
    }

    private void OnStartJumpDown()
    {
        m_isJumpingDown = true;
    }

    private void OnEndJumpDown()
    {
        m_isJumpingDown = false;
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