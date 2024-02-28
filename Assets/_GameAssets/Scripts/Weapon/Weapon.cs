using System;
using Unity.Mathematics;
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
    protected bool m_canPickupWeapon;

    protected virtual void Awake()
    {
        m_canPickupWeapon = true;
        gameObject.layer = 8;
        if (GameConfig.Instance)
        {
            m_weaponInfo = GameConfig.Instance.GetWeaponInfo(WeaponName);
        }

        m_rigid = GetComponent<Rigidbody>();
        m_damageSender = GetComponent<DamageSender>();
        m_damageSender.SetDamage(m_weaponInfo.Dame / m_weaponInfo.BulletsPerShot);
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

    public virtual void UseWeapon()
    {
        UpdateParent(1);
        IsUsing = true;
        EventDispatcher.Instance.RegisterListener(EventID.OnPullTrigger, OnPullTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeFireMode, OnChangeFireMode);
        EventDispatcher.Instance.RegisterListener(EventID.ReloadBullet, ReloadBullet);
        EventDispatcher.Instance.RegisterListener(EventID.AimScope, AimScope);
    }

    public virtual void UnUseWeapon()
    {
        UpdateParent(0);
        IsUsing = false;
        try
        {
            EventDispatcher.Instance.RemoveListener(EventID.OnChangeFireMode, OnChangeFireMode);
            EventDispatcher.Instance.RemoveListener(EventID.OnPullTrigger, OnPullTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.ReloadBullet, ReloadBullet);
            EventDispatcher.Instance.RemoveListener(EventID.AimScope, AimScope);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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

    protected virtual void OnPullTrigger(object obj)
    {
    }

    protected virtual void OnReleaseTrigger(object obj)
    {
    }

    protected virtual void AimScope(object obj)
    {
    }

    protected virtual void OnChangeFireMode(object obj)
    {
    }

    protected virtual void ReloadBullet(object obj = null)
    {
    }
}