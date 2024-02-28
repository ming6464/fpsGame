using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : Weapon
{
    [SerializeField]
    private int _totalBullets = 100;

    [SerializeField]
    private int _bullets = 30;

    [SerializeField]
    [CanBeNull]
    private FireModeInfo _curFireMode;

    [SerializeField]
    [CanBeNull]
    private ParticleSystem _effectFire;

    [SerializeField]
    [CanBeNull]
    private Transform _scopeCam;

    public int TotalBullets => _totalBullets;
    public int Bullets => _bullets;
    private Vector3 targetRecoilValue;
    private Vector3 curRecoilValue;
    private Vector3 velocityRecoil;
    private float timeRecoil;
    private Vector3 curRecoilWeapon;
    private Vector3 targetRecoilWeapon;
    private Vector3 velocityCurRecoilWeapon;
    private Vector3 velocityTargetRecoilWeapon;
    private bool isTrigger;
    private bool isReloading;
    private bool isResetFire;
    private bool canFire;

    [CanBeNull]
    private Coroutine resetFire;

    private bool aimScope;

    protected override void Awake()
    {
        base.Awake();
        if (_curWeaponInfo.MagazineCapacity <= 0)
        {
            _curWeaponInfo.MagazineCapacity = 1;
        }

        if (_bullets > _curWeaponInfo.MagazineCapacity)
        {
            _bullets = _curWeaponInfo.MagazineCapacity;
        }
    }

    protected virtual void Start()
    {
        _curWeaponInfo.FireModeOption = _curWeaponInfo.FireModeOption.OrderBy(x => (int)x.FireModeType).ToList();
        _curFireMode = _curWeaponInfo.FireModeOption.Last();
    }

    protected virtual void Update()
    {
        if (!isUsing)
        {
            return;
        }

        LoadFire();
        ApplyRecoil();
        DrawRay();
    }

    private void DrawRay()
    {
        Debug.DrawRay(weaponManager.PivotRay.position, weaponManager.PivotRay.forward * 999f, Color.red);
    }

    private void LoadFire()
    {
        if (isReloading || !CheckMagazine() || !canFire || !isTrigger || isResetFire)
        {
            return;
        }

        StartCoroutine(PrepareFire());
    }

    private IEnumerator PrepareFire()
    {
        isResetFire = true;
        for (int i = 0; i < _curFireMode.FireTimes; i++)
        {
            if (i > 0)
            {
                yield return new WaitForEndOfFrame();
            }

            _bullets--;
            OnFire();
            RecoilMath();
        }

        Invoke(nameof(ResetFire), _curFireMode.TimeResetFire);

        resetFire = null;

        resetFire = StartCoroutine(ResetFire(_curFireMode.TimeResetFire));

        if (_curFireMode.FireModeType != FireModeKEY.Automatic)
        {
            canFire = false;
        }
    }

    private IEnumerator ResetFire(float timeReset)
    {
        yield return new WaitForSeconds(timeReset);
        isResetFire = false;
    }

    public override void UseWeapon()
    {
        base.UseWeapon();
        if (_curWeaponInfo.UseScope)
        {
            UIManager.Instance.HandleCrossHair(false);
        }

        EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
            new MsgWeapon
            {
                Bullets = _bullets, TotalBullets = _totalBullets,
                WeaponKey = WeaponType
            });
    }

    public override void UnUseWeapon()
    {
        base.UnUseWeapon();
        try
        {
            if (_curWeaponInfo.UseScope)
            {
                UIManager.Instance.HandleAimScope(false);
            }

            if (resetFire != null)
            {
                StopCoroutine(resetFire);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        _animator.Play("UnUsedWeapon");
    }

    protected override void ResetData()
    {
        base.ResetData();
        targetRecoilValue = Vector3.zero;
        curRecoilValue = Vector3.zero;
        velocityRecoil = Vector3.zero;
        timeRecoil = 0f;
        curRecoilWeapon = Vector3.zero;
        targetRecoilWeapon = Vector3.zero;
        velocityCurRecoilWeapon = Vector3.zero;
        velocityTargetRecoilWeapon = Vector3.zero;
        isTrigger = false;
        isReloading = false;
        isResetFire = false;
        canFire = false;
        aimScope = false;
    }

    public override void PutToBag(WeaponManager wManager, Transform bag)
    {
        base.PutToBag(wManager, bag);
        if (!rigid)
        {
            return;
        }

        rigid.isKinematic = true;
        rigid.useGravity = false;
    }

    public override void RemoveFromBag()
    {
        base.RemoveFromBag();
        if (!rigid)
        {
            return;
        }

        rigid.isKinematic = false;
        rigid.useGravity = true;
    }

    protected override void OnPullTrigger(object obj)
    {
        base.OnPullTrigger(obj);
        if (isReloading)
        {
            return;
        }

        timeRecoil = Time.time;
        isTrigger = true;
        canFire = true;
    }

    protected override void OnReleaseTrigger(object obj)
    {
        base.OnReleaseTrigger(obj);
        if (isReloading)
        {
            return;
        }

        isTrigger = false;
    }

    // ReSharper disable Unity.PerformanceAnalysis

    protected virtual bool CheckMagazine()
    {
        if (GameConfig.Instance.UnlimitedBullet)
        {
            return true;
        }

        if (_bullets <= 0)
        {
            if (_totalBullets <= 0)
            {
                _bullets = 0;
                _totalBullets = 0;
            }
            else
            {
                ReloadBullet();
            }

            return false;
        }

        return true;
    }

    protected virtual void OnFire()
    {
        RaycastHit[] raycastHits = new RaycastHit[weaponManager.AvoidTags.Length + 1];
        Vector3 startPosRay = weaponManager.PivotRay.position;
        Vector3 dir = Vector3.zero;
        for (int i = 0; i < _curWeaponInfo.BulletsPerShot; i++)
        {
            if (i > 0)
            {
                dir = Quaternion.Euler(Random.Range(-_curFireMode.Spread, _curFireMode.Spread),
                    Random.Range(-_curFireMode.Spread, _curFireMode.Spread), 0f) * weaponManager.PivotRay.forward;
            }
            else
            {
                dir = weaponManager.PivotRay.forward;
            }

            if (Physics.RaycastNonAlloc(startPosRay, dir,
                    raycastHits, 999f) > 0)
            {
                Array.Sort(raycastHits, (x, y) =>
                {
                    if (!x.transform && !y.transform)
                    {
                        return 0;
                    }

                    if (!x.transform)
                    {
                        return 1;
                    }

                    if (!y.transform)
                    {
                        return -1;
                    }

                    float distanceX = Vector3.Distance(x.transform.position, startPosRay);
                    float distanceY = Vector3.Distance(y.transform.position, startPosRay);

                    return distanceX.CompareTo(distanceY);
                });
                foreach (RaycastHit ray in raycastHits)
                {
                    if (ray.transform && !weaponManager.CheckTagOfAvoidTags(ray.transform.tag))
                    {
                        Debug.Log($"Ray impact {ray.transform.name}");
                        _damageSender.Send(ray.transform);
                        SfxManager.Instance.PlayImpactBullet(ray.point,
                            PoolKEY.EffectImpact);
                        break;
                    }
                }
            }
        }

        if (_effectFire)
        {
            _effectFire.Stop();
            _effectFire.Play();
        }

        EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
            new MsgWeapon
            {
                Bullets = _bullets, TotalBullets = _totalBullets,
                WeaponKey = _curWeaponInfo.WeaponType
            });
    }

    protected virtual void RecoilMath()
    {
        float ratioRecoil = Mathf.Min((Time.time - timeRecoil) / _curWeaponInfo.MaxRecoilTime, 1f);
        ratioRecoil = Mathf.Max(ratioRecoil, _curWeaponInfo.MinRecoilPercentage);
        Vector3 recoilValue = Vector3.zero;
        recoilValue.x = Random.Range(-_curWeaponInfo.RecoilAmountY, -_curWeaponInfo.RecoilAmountY / 2f);
        recoilValue.y = Random.Range(-_curWeaponInfo.RecoilAmountX, _curWeaponInfo.RecoilAmountX);
        recoilValue *= ratioRecoil;
        recoilValue.z = -_curWeaponInfo.RecoilAmountZ * Mathf.Min(0.8f, ratioRecoil);
        targetRecoilValue += recoilValue;
        targetRecoilWeapon += new Vector3(recoilValue.x, 0, recoilValue.z);
    }

    protected virtual void ApplyRecoil()
    {
        if (curRecoilValue != targetRecoilValue)
        {
            Vector3 rotateValue = curRecoilValue;
            curRecoilValue = Vector3.SmoothDamp(curRecoilValue, targetRecoilValue, ref velocityRecoil,
                0.035f);
            rotateValue = curRecoilValue - rotateValue;
        }

        if (targetRecoilWeapon != Vector3.zero || curRecoilWeapon != Vector3.zero)
        {
            curRecoilWeapon = Vector3.SmoothDamp(curRecoilWeapon, targetRecoilWeapon, ref velocityCurRecoilWeapon,
                0.02f);
            targetRecoilWeapon =
                Vector3.SmoothDamp(targetRecoilWeapon, Vector3.zero, ref velocityTargetRecoilWeapon, 0.03f);
            transform.localPosition = new Vector3(0, 0, curRecoilWeapon.z);
            transform.localRotation = Quaternion.Euler(curRecoilWeapon.x, 0f, 0f);
        }
    }

    protected override void AimScope(object obj)
    {
        base.AimScope(obj);
        if (!_curWeaponInfo.UseScope || isReloading)
        {
            return;
        }

        aimScope = !aimScope;
        _animator.SetBool("Aim", aimScope);
        if (!aimScope)
        {
            UIManager.Instance.HandleAimScope(false);
        }
    }

    protected virtual void OnAimScope()
    {
        if (!_curWeaponInfo.UseScope || isReloading)
        {
            return;
        }

        if (_curWeaponInfo.UseScope && _scopeCam)
        {
            _scopeCam.position = weaponManager.PivotRay.position;
            Vector3 vt = _scopeCam.localPosition;
            vt.z = 0;
            _scopeCam.localPosition = vt;
        }

        UIManager.Instance.HandleAimScope(true);
    }

    protected override void OnChangeFireMode(object obj)
    {
        base.OnChangeFireMode(obj);
        int nextIndex = _curWeaponInfo.FireModeOption.IndexOf(_curFireMode) + 1;
        if (nextIndex >= _curWeaponInfo.FireModeOption.Count)
        {
            nextIndex = 0;
        }

        _curFireMode = _curWeaponInfo.FireModeOption[nextIndex];
    }

    protected override void ReloadBullet(object obj = null)
    {
        base.ReloadBullet();
        if (_bullets >= _curWeaponInfo.MagazineCapacity || _totalBullets <= 0)
        {
            return;
        }

        _animator.SetBool("Reload", true);
        if (_curWeaponInfo.UseScope)
        {
            aimScope = false;
            _animator.SetBool("Aim", aimScope);
            UIManager.Instance.HandleAimScope(false);
        }

        isReloading = true;
        isTrigger = false;
        canFire = false;
        isResetFire = false;
        StopCoroutine(resetFire);
    }

    protected virtual void OnReload()
    {
        if (_bullets < 0)
        {
            _bullets = 0;
        }

        int bullets = _curWeaponInfo.MagazineCapacity - _bullets;
        if (bullets >= _totalBullets)
        {
            _bullets += _totalBullets;
            _totalBullets = 0;
        }
        else
        {
            _totalBullets -= bullets;
            _bullets = _curWeaponInfo.MagazineCapacity;
        }

        _animator.SetBool("Reload", false);
        isReloading = false;
        EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
            new MsgWeapon
            {
                Bullets = _bullets, TotalBullets = _totalBullets,
                WeaponKey = _curWeaponInfo.WeaponType
            });
    }
}