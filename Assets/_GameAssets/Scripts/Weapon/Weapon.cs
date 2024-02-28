using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Information")]
    public string WeaponName;

    [SerializeField]
    protected WeaponInfo _curWeaponInfo;

    [Header("References")]
    [SerializeField]
    protected Collider _collider;

    [SerializeField]
    protected Animator _animator;

    [SerializeField]
    protected DamageSender _damageSender;


    protected bool inBag;
    protected bool isUsing;
    protected WeaponManager weaponManager;
    protected Rigidbody rigid;
    public WeaponKEY WeaponType => _curWeaponInfo.WeaponType;

    protected virtual void Awake()
    {
        _curWeaponInfo = GameConfig.Instance.GetWeaponInfo(WeaponName);
        TryGetComponent(out rigid);
        TryGetComponent(out _animator);
        TryGetComponent(out _damageSender);
        if (!_collider)
        {
            TryGetComponent(out _collider);
        }

        if (_damageSender)
        {
            _damageSender.SetDamage(_curWeaponInfo.Dame / _curWeaponInfo.BulletsPerShot);
        }
    }

    public virtual void UseWeapon()
    {
        Transform myTransform = transform;
        myTransform.localPosition = Vector3.zero;
        myTransform.localRotation = Quaternion.identity;
        myTransform.localScale = Vector3.one;
        gameObject.layer = 3;
        ResetData();
        gameObject.SetActive(true);
    }

    public virtual void UnUseWeapon()
    {
        try
        {
            transform.gameObject.layer = 6;
            isUsing = false;
            EventDispatcher.Instance.RemoveListener(EventID.OnChangeFireMode, OnChangeFireMode);
            EventDispatcher.Instance.RemoveListener(EventID.OnPullTrigger, OnPullTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.ReloadBullet, ReloadBullet);
            EventDispatcher.Instance.RemoveListener(EventID.AimScope, AimScope);
            ResetData();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public virtual void OnUseWeapon()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnPullTrigger, OnPullTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeFireMode, OnChangeFireMode);
        EventDispatcher.Instance.RegisterListener(EventID.ReloadBullet, ReloadBullet);
        EventDispatcher.Instance.RegisterListener(EventID.AimScope, AimScope);
        isUsing = true;
    }

    public virtual void OnUnUseWeapon()
    {
        gameObject.SetActive(!inBag);
    }

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void ResetData()
    {
    }

    public virtual void PutToBag(WeaponManager wManager, Transform bag)
    {
        weaponManager = wManager;
        Transform myTransform = transform;
        myTransform.SetParent(bag);
        inBag = true;
        gameObject.layer = 6;
        gameObject.SetActive(false);
    }

    public virtual void RemoveFromBag()
    {
        UnUseWeapon();
        inBag = false;
        weaponManager = null;
        transform.SetParent(null, false);
        gameObject.SetActive(true);
        gameObject.layer = 8;
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RangePickUpWeapon") && !inBag)
        {
            EventDispatcher.Instance.PostEvent(EventID.OnWeaponPickupAreaEnter, this);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RangePickUpWeapon") && !inBag)
        {
            EventDispatcher.Instance.PostEvent(EventID.OnWeaponPickupAreaExit, this);
        }
    }
}