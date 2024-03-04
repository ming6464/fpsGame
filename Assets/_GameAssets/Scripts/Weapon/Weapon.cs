using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour
{
    [Header("Weapon")]
    public string WeaponName;

    public bool IsUsing;

    //bag
    protected Bag m_slot;

    //References
    protected DamageSender m_damageSender;

    protected Rigidbody m_rigid;

    protected WeaponHolder m_weaponHolder;


    //weapon
    protected WeaponInfo m_weaponInfo;

    public WeaponKEY WeaponType => m_weaponInfo.WeaponType;
    public Sprite WeaponIcon => m_weaponInfo.IconWeapon;
    protected bool m_canPickupWeapon;
    private InputBase m_inputBase;

    protected virtual void Awake()
    {
        m_inputBase = new InputBase();
        m_canPickupWeapon = true;
        gameObject.layer = 8;
        if (GameConfig.Instance)
        {
            m_weaponInfo = GameConfig.Instance.GetWeaponInfo(WeaponName);
        }

        m_rigid = GetComponent<Rigidbody>();
        m_damageSender = GetComponent<DamageSender>();
        m_damageSender.SetDamage(m_weaponInfo.Dame / m_weaponInfo.BulletsPerShot);
        LinkInput();
    }

    protected virtual void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnClosePauseGamePanel, HandleClosePauseGamePanel);
        EventDispatcher.Instance.RegisterListener(EventID.OnOpenPauseGamePanel, HandleOpenPauseGamePanel);
    }

    protected virtual void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnClosePauseGamePanel, HandleClosePauseGamePanel);
        EventDispatcher.Instance.RemoveListener(EventID.OnOpenPauseGamePanel, HandleOpenPauseGamePanel);
    }

    private void HandleOpenPauseGamePanel(object obj)
    {
        if (!IsUsing || (GameManager.Instance && GameManager.Instance.IsFinishGame))
        {
            return;
        }

        m_inputBase.Disable();
    }

    private void HandleClosePauseGamePanel(object obj)
    {
        if (!IsUsing || (GameManager.Instance && GameManager.Instance.IsFinishGame))
        {
            return;
        }

        m_inputBase.Enable();
    }

    protected virtual void LinkInput()
    {
        m_inputBase.Weapon.Trigger.performed += _ => { OnPullTrigger(); };
        m_inputBase.Weapon.Trigger.canceled += _ => { OnReleaseTrigger(); };
        m_inputBase.Weapon.ChangeFireMode.performed += _ => { OnChangeFireMode(); };
        m_inputBase.Weapon.ReloadBullet.performed += _ => { ReloadBullet(); };
    }

    protected virtual void OnEnable()
    {
        ResetData();
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void ResetData()
    {
        m_canPickupWeapon = true;
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void LateUpdate()
    {
    }

    public virtual void PutToBag(WeaponHolder weaponHolder, Bag slot)
    {
        gameObject.layer = 3;
        m_weaponHolder = weaponHolder;
        m_slot = slot;
        Transform myTf = transform;
        myTf.parent = slot.BagTf;
        myTf.localPosition = Vector3.zero;
        myTf.localRotation = Quaternion.identity;
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        m_rigid.isKinematic = true;
        m_rigid.useGravity = false;
        UnUseWeapon();
    }

    public virtual void RemoveFromBag()
    {
        m_weaponHolder = null;
        m_slot = null;
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        m_rigid.isKinematic = false;
        m_rigid.useGravity = true;
        transform.parent = null;
        gameObject.layer = 8;
        UnUseWeapon();
        m_canPickupWeapon = false;
        Invoke(nameof(DelayCanPickUp), 0.5f);
        m_rigid.AddForce(transform.forward * 30f, ForceMode.Impulse);
    }

    protected void DelayCanPickUp()
    {
        m_canPickupWeapon = true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void UseWeapon()
    {
        m_inputBase.Enable();
        LinkEvent();
        UpdateParent(1);
        IsUsing = true;

        MsgWeapon msg = new()
        {
            WeaponKey = WeaponType, WeaponIcon = WeaponIcon
        };

        if (WeaponType != WeaponKEY.Grenade && WeaponType != WeaponKEY.None)
        {
            Gun gun = GetComponent<Gun>();
            msg.Bullets = gun.Bullets;
            EventDispatcher.Instance.PostEvent(EventID.OnchangeTotalBullets, gun.TotalBullet);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnChangeWeapon, msg);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public virtual void UnUseWeapon()
    {
        m_inputBase.Disable();
        UnLinkEvent();
        UpdateParent(0);
        IsUsing = false;
    }

    private void UpdateParent(int index)
    {
        if (m_slot == null)
        {
            return;
        }

        MultiParentConstraint constraint = m_slot.Constraint;
        if (constraint.data.sourceObjects.Count <= index)
        {
            return;
        }

        WeightedTransformArray sourceObjects = constraint.data.sourceObjects;

        for (int i = 0; i < constraint.data.sourceObjects.Count; i++)
        {
            sourceObjects.SetWeight(i, 0f);
        }

        sourceObjects.SetWeight(index, 1);

        constraint.data.sourceObjects = sourceObjects;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!m_canPickupWeapon)
        {
            return;
        }

        if (other.CompareTag("RangePickUpWeapon"))
        {
            EventDispatcher.Instance.PostEvent(EventID.OnWeaponPickupAreaEnter, this);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RangePickUpWeapon"))
        {
            EventDispatcher.Instance.PostEvent(EventID.OnWeaponPickupAreaExit, this);
        }
    }

    protected virtual void OnPullTrigger()
    {
    }

    protected virtual void OnReleaseTrigger()
    {
    }

    protected virtual void OnChangeFireMode()
    {
    }

    protected virtual void ReloadBullet()
    {
    }
}