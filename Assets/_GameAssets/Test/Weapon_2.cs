using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(DamageSender))]
public class Weapon_2 : MonoBehaviour
{
    [Header("Weapon")]
    public string WeaponName;

    public bool IsUsing;

    //bag
    protected Bag _slot;

    //References
    protected DamageSender _damageSender;

    protected Rigidbody _rigid;

    protected WeaponHolder _weaponHolder;


    //weapon
    protected WeaponInfo _weaponInfo;

    public WeaponKEY WeaponType => _weaponInfo.WeaponType;
    protected bool _canPickupWeapon;

    protected virtual void Awake()
    {
        _canPickupWeapon = true;
        gameObject.layer = 8;
        if (GameConfig.Instance)
        {
            _weaponInfo = GameConfig.Instance.GetWeaponInfo(WeaponName);
        }

        _rigid = GetComponent<Rigidbody>();
        _damageSender = GetComponent<DamageSender>();
        _damageSender.SetDamage(_weaponInfo.Dame / _weaponInfo.BulletsPerShot);
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
        _weaponHolder = weaponHolder;
        _slot = slot;
        Transform myTf = transform;
        myTf.parent = slot.BagTf;
        myTf.localPosition = Vector3.zero;
        myTf.localRotation = Quaternion.identity;
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        _rigid.isKinematic = true;
        _rigid.useGravity = false;
        UnUseWeapon();
    }

    public virtual void RemoveFromBag()
    {
        _weaponHolder = null;
        _slot = null;
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        _rigid.isKinematic = false;
        _rigid.useGravity = true;
        transform.parent = null;
        gameObject.layer = 8;
        UnUseWeapon();
        _canPickupWeapon = false;
        Invoke(nameof(DelayCanPickUp), 0.5f);
    }

    protected void DelayCanPickUp()
    {
        _canPickupWeapon = true;
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

    private void UpdateParent(int index)
    {
        if (_slot == null)
        {
            return;
        }

        MultiParentConstraint constraint = _slot.Constraint;
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!_canPickupWeapon)
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