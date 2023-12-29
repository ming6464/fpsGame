using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Information")] public string CurrentFireMode;
    public float CurrentTimeResetFire;
    public int BulletsPerShot;
    [SerializeField] protected WeaponInfo _weaponInfo;
    [SerializeField] protected BulletInfo _bulletInfo;
    [Header("State")] [SerializeField] protected bool _canFire = false;
    [SerializeField] protected bool _isFiring;
    [SerializeField] protected bool _isUsing;
    public WeaponKEY WeaponType => _weaponInfo.WeaponType;
    public string WeaponName => _weaponInfo.WeaponName;
    public int TotalBullets => _weaponInfo.TotalBullets;
    public int Bullets => _weaponInfo.Bullets;

    private List<string> fireModeList = new();

    protected virtual void Awake()
    {
        _canFire = false;
    }

    protected virtual void Start()
    {
        if (_weaponInfo.Single) fireModeList.Add("Single");
        if (_weaponInfo.Burst) fireModeList.Add("Burst");
        if (_weaponInfo.Automatic) fireModeList.Add("Automatic");

        if (fireModeList.Count > 0)
        {
            CurrentFireMode = fireModeList.Last();
            UpdateFireMode();
        }

        if (_bulletInfo.BulletPrefab)
            GObj_pooling.Instance.UpdateObjSpawn(PoolKEY.Bullet, _bulletInfo.BulletPrefab.gameObject);
        if (_weaponInfo.MagazineCapacity <= 0) _weaponInfo.MagazineCapacity = 1;
    }

    protected virtual void Update()
    {
        if (_isUsing && _isFiring)
        {
            if (!_canFire || !_weaponInfo.PivotFireTf) return;
            StartCoroutine(PrepareFire());
            if (CurrentFireMode != "Automatic") _isFiring = false;
        }
    }

    public virtual void UseWeapon()
    {
        transform.gameObject.SetActive(true);
        EventDispatcher.Instance.RegisterListener(EventID.OnPullTrigger, OnPullTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeFireMode, OnChangeFireMode);
        EventDispatcher.Instance.RegisterListener(EventID.ReloadBullet, ReloadBullet);
        _isUsing = true;
        _canFire = true;
    }

    public virtual void UnUseWeapon()
    {
        try
        {
            _isUsing = false;
            _canFire = false;
            _isFiring = false;
            EventDispatcher.Instance.RemoveListener(EventID.OnChangeFireMode, OnChangeFireMode);
            EventDispatcher.Instance.RemoveListener(EventID.OnPullTrigger, OnPullTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.ReloadBullet, ReloadBullet);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    protected void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
        UnUseWeapon();
    }

    protected virtual void OnPullTrigger(object obj)
    {
        _isFiring = true;
    }

    protected virtual void OnReleaseTrigger(object obj)
    {
        _isFiring = false;
    }

    protected virtual IEnumerator PrepareFire()
    {
        for (var i = 0; i < BulletsPerShot; i++)
        {
            OnFire();
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected virtual void OnFire()
    {
        if (_weaponInfo.Bullets <= 0 && _weaponInfo.TotalBullets <= 0)
        {
            _weaponInfo.Bullets = 0;
            _weaponInfo.TotalBullets = 0;
            return;
        }

        _weaponInfo.Bullets--;
        if (_weaponInfo.Bullets <= 0) ReloadBullet();

        var curBullet = GObj_pooling.Instance.Pull(PoolKEY.Bullet);
        if (!curBullet.TryGetComponent(out Rigidbody rigidBullet))
        {
            Debug.LogError($"bullet of {transform.name} not has rigid body component!");
            return;
        }

        curBullet.transform.position = _weaponInfo.PivotFireTf.position;
        curBullet.transform.localRotation = _weaponInfo.PivotFireTf.rotation;

        if (curBullet.TryGetComponent(out TrailRenderer trailRenderer)) trailRenderer.Clear();

        curBullet.SetActive(true);
        rigidBullet.WakeUp();
        rigidBullet.AddForce(curBullet.transform.forward.normalized * _bulletInfo.BulletVelocity, ForceMode.Impulse);

        EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
            new MsgWeapon
            {
                Bullets = _weaponInfo.Bullets, TotalBullets = _weaponInfo.TotalBullets,
                WeaponKey = _weaponInfo.WeaponType
            });
        StartCoroutine(UpdateStateCanFire());
        StartCoroutine(SetTimeLifeBullet(curBullet, _bulletInfo.BulletTimeLife));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    protected virtual IEnumerator SetTimeLifeBullet(GameObject bullet, float timeLife)
    {
        yield return new WaitForSeconds(timeLife);
        if (bullet.TryGetComponent(out Rigidbody rig))
        {
            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
            rig.Sleep();
        }

        GObj_pooling.Instance.Push(PoolKEY.Bullet, bullet);
    }

    protected virtual IEnumerator UpdateStateCanFire()
    {
        _canFire = false;
        yield return new WaitForSeconds(CurrentTimeResetFire);
        _canFire = true;
    }

    protected virtual void OnChangeFireMode(object obj)
    {
        if (fireModeList.Count <= 1) return;
        var nextIndex = fireModeList.IndexOf(CurrentFireMode) + 1;
        if (nextIndex >= fireModeList.Count) nextIndex = 0;
        CurrentFireMode = fireModeList[nextIndex];
        UpdateFireMode();
    }

    protected virtual void UpdateFireMode()
    {
        BulletsPerShot = 1;
        switch (CurrentFireMode)
        {
            case "Single":
                CurrentTimeResetFire = _weaponInfo.TimeResetFireSingleMode;
                break;
            case "Burst":
                CurrentTimeResetFire = _weaponInfo.TimeResetFireBurstMode;
                BulletsPerShot = _weaponInfo.BulletsPerShotOfBurstMode;
                break;
            case "Automatic":
                CurrentTimeResetFire = _weaponInfo.TimeResetFireAutoMode;
                break;
        }
    }

    protected virtual void ReloadBullet(object obj = null)
    {
        if (_weaponInfo.Bullets >= _weaponInfo.MagazineCapacity || _weaponInfo.TotalBullets <= 0) return;
        if (_weaponInfo.Bullets < 0) _weaponInfo.Bullets = 0;
        var bullets = _weaponInfo.MagazineCapacity - _weaponInfo.Bullets;
        if (bullets >= _weaponInfo.TotalBullets)
        {
            _weaponInfo.Bullets += _weaponInfo.TotalBullets;
            _weaponInfo.TotalBullets = 0;
        }
        else
        {
            _weaponInfo.TotalBullets -= bullets;
            _weaponInfo.Bullets = _weaponInfo.MagazineCapacity;
        }

        EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
            new MsgWeapon
            {
                Bullets = _weaponInfo.Bullets, TotalBullets = _weaponInfo.TotalBullets,
                WeaponKey = _weaponInfo.WeaponType
            });
    }
}