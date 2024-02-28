using System;
using System.Collections;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun_2 : Weapon_2
{
    public int TotalBullet
    {
        get => _weaponHolder.TotalBullet;
        set => _weaponHolder.TotalBullet -= value;
    }

    public int Bullets;
    public FireModeInfo CurrrentFireMode;

    //gun
    private bool _isTrigger;
    private bool _isReloading;
    private bool _isCanFire;
    private bool _isResetFire;

    private Coroutine _reloadingCrt;


    protected override void Awake()
    {
        base.Awake();
        Bullets = _weaponInfo.MagazineCapacity;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _isCanFire = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_reloadingCrt != null)
        {
            StopCoroutine(_reloadingCrt);
            _reloadingCrt = null;
        }
    }

    protected override void Start()
    {
        base.Start();
        _weaponInfo.FireModeOption = _weaponInfo.FireModeOption.OrderBy(x => (int)x.FireModeType).ToList();
        CurrrentFireMode = _weaponInfo.FireModeOption.Last();
    }

    protected override void ResetData()
    {
        base.ResetData();
        _isReloading = false;
        _isTrigger = false;
    }

    protected override void Update()
    {
        base.Update();
        if (!IsUsing)
        {
            return;
        }

        CheckMagazine();
        HandleFire();
    }

    private void HandleFire()
    {
        if (!_isTrigger || _isResetFire || !_isCanFire || _isReloading || Bullets <= 0)
        {
            return;
        }

        StartCoroutine(PrepareFire());
    }

    private IEnumerator PrepareFire()
    {
        for (int i = 0; i < CurrrentFireMode.FireTimes; i++)
        {
            OnFire();
            if (!GameConfig.Instance || !GameConfig.Instance.UnlimitedBullet)
            {
                Bullets--;
                if (Bullets <= 0)
                {
                    yield break;
                }
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(ResetFire());

        if (CurrrentFireMode.FireModeType != FireModeKEY.Automatic)
        {
            _isCanFire = false;
        }
    }

    private IEnumerator ResetFire()
    {
        _isResetFire = true;
        yield return new WaitForSeconds(CurrrentFireMode.TimeResetFire);
        _isResetFire = false;
    }

    protected virtual void OnFire()
    {
        Transform pivotTf = _weaponHolder.PivotRay;
        Vector3 startPosRay = pivotTf.position;
        Vector3 dirRay = pivotTf.forward;
        float spreadBullet = CurrrentFireMode.Spread;
        for (int i = 0; i < _weaponInfo.BulletsPerShot; i++)
        {
            if (i > 0)
            {
                dirRay = Quaternion.Euler(Random.Range(-spreadBullet, spreadBullet),
                    Random.Range(-spreadBullet, spreadBullet), 0f) * pivotTf.forward;
            }

            Ray ray = new(startPosRay, dirRay);
            int layerMask = ~LayerMask.GetMask(_weaponHolder.AvoidTags);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Debug.Log($"fire {hit.transform.name}");
                _damageSender.Send(hit.transform);
                SfxManager.Instance.PlayImpactBullet(hit.point, PoolKEY.EffectImpact);
            }
        }

        // if (_effectFire)
        // {
        //     _effectFire.Stop();
        //     _effectFire.Play();
        // }
        //
        // EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
        //     new MsgWeapon
        //     {
        //         Bullets = _bullets, TotalBullets = _totalBullets,
        //         WeaponKey = _curWeaponInfo.WeaponType
        //     });
    }

    private bool CheckMagazine()
    {
        if (GameConfig.Instance && GameConfig.Instance.UnlimitedBullet)
        {
            return true;
        }

        if (Bullets > 0 && !_isReloading)
        {
            return true;
        }

        ReloadBullet();
        return false;
    }

    protected override void OnChangeFireMode(object obj)
    {
        base.OnChangeFireMode(obj);
        int nextIndex = _weaponInfo.FireModeOption.IndexOf(CurrrentFireMode) + 1;
        if (nextIndex >= _weaponInfo.FireModeOption.Count)
        {
            nextIndex = 0;
        }

        CurrrentFireMode = _weaponInfo.FireModeOption[nextIndex];
    }

    protected override void OnPullTrigger(object obj)
    {
        base.OnPullTrigger(obj);
        _isTrigger = true;
    }

    protected override void OnReleaseTrigger(object obj)
    {
        base.OnReleaseTrigger(obj);
        _isTrigger = false;
        _isCanFire = true;
    }

    protected override void ReloadBullet(object obj = null)
    {
        if (_isReloading)
        {
            return;
        }

        base.ReloadBullet(obj);
        if (TotalBullet <= 0)
        {
            return;
        }

        _isTrigger = false;
        StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        _isReloading = true;
        yield return new WaitForSeconds(1f);
        int bullet = _weaponInfo.MagazineCapacity;
        if (TotalBullet < _weaponInfo.MagazineCapacity)
        {
            bullet = TotalBullet;
        }

        TotalBullet -= bullet;
        Bullets = bullet;
        _isReloading = false;
        Debug.Log("Finish Reload");
    }

    public override void RemoveFromBag()
    {
        base.RemoveFromBag();
        Vector3 dir = transform.forward;
        _rigid.AddForce(dir * 30f, ForceMode.Impulse);
    }
}