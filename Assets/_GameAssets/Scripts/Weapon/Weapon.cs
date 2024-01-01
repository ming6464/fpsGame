using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Pivot Informaiton")] [SerializeField]
    protected Vector3 _localPosition;

    [SerializeField] protected Vector3 _localRotation;
    [SerializeField] protected Vector3 _localScale;

    [Header("Weapon Information")] [SerializeField]
    protected string _curFireMode;

    [SerializeField] protected float _curTimeResetFire;
    [SerializeField] protected int _bulletsPerShot;
    [SerializeField] protected WeaponInfo _weaponInfo;
    [Header("State")] [SerializeField] protected bool _canFire;
    [SerializeField] protected bool _isFiring;
    [SerializeField] protected bool _isUsing;
    public WeaponKEY WeaponType => _weaponInfo.WeaponType;
    public string WeaponName => _weaponInfo.WeaponName;
    public int TotalBullets => _weaponInfo.TotalBullets;
    public int Bullets => _weaponInfo.Bullets;

    public Vector3 PositionInBag => _localPosition;
    public Vector3 RotationInBag => _localRotation;
    public Vector3 ScaleInBag => _localScale;

    private List<string> fireModeList = new();
    private Rigidbody rigid;
    private WeaponManager weaponManager;

    protected virtual void Awake()
    {
        _canFire = false;
        transform.TryGetComponent(out rigid);
    }

    protected virtual void Start()
    {
        if (_weaponInfo.Single) fireModeList.Add("Single");
        if (_weaponInfo.Burst) fireModeList.Add("Burst");
        if (_weaponInfo.Automatic) fireModeList.Add("Automatic");

        if (fireModeList.Count > 0)
        {
            _curFireMode = fireModeList.Last();
            UpdateFireMode();
        }


        if (_weaponInfo.MagazineCapacity <= 0) _weaponInfo.MagazineCapacity = 1;
    }

    protected virtual void Update()
    {
        if (!_isUsing) return;
        if (_isFiring)
        {
            if (!_canFire || !_weaponInfo.PivotFireTf) return;
            StartCoroutine(PrepareFire());
            if (_curFireMode != "Automatic") _isFiring = false;
        }

        DrawRay();
    }

    protected virtual void DrawRay()
    {
        var position = weaponManager.StartPointRay.position;
        Debug.DrawRay(position, position + weaponManager.StartPointRay.forward.normalized * _weaponInfo.Range,
            Color.red);
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

    public virtual void PutToBag(WeaponManager weaponManager)
    {
        this.weaponManager = weaponManager;
        if (!rigid) transform.TryGetComponent(out rigid);

        if (rigid)
        {
            rigid.Sleep();
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            rigid.useGravity = false;
        }

        var myTransform = transform;
        myTransform.SetParent(this.weaponManager.transform);
        myTransform.gameObject.layer = 3;
        myTransform.localPosition = PositionInBag;
        myTransform.localScale = ScaleInBag;
        transform.localRotation = Quaternion.Euler(RotationInBag);
        UnUseWeapon();
        gameObject.SetActive(false);
    }

    public virtual void RemoveFromBag()
    {
        weaponManager = null;
        transform.gameObject.layer = 6;
        transform.SetParent(null, true);
        if (!rigid) transform.TryGetComponent(out rigid);

        if (rigid)
        {
            rigid.WakeUp();
            rigid.constraints = RigidbodyConstraints.None;
            rigid.useGravity = true;
        }

        UnUseWeapon();
        gameObject.SetActive(true);
    }

    protected virtual void OnPullTrigger(object obj)
    {
        _isFiring = true;
    }

    protected virtual void OnReleaseTrigger(object obj)
    {
        _isFiring = false;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    protected virtual IEnumerator PrepareFire()
    {
        for (var i = 0; i < _bulletsPerShot; i++)
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

        // Vector3 posEnd;
        var raycastHits = new RaycastHit[weaponManager.AvoidTags.Length + 1];
        if (Physics.RaycastNonAlloc(weaponManager.StartPointRay.position, weaponManager.StartPointRay.forward,
                raycastHits, _weaponInfo.Range) > 0)
            foreach (var ray in raycastHits)
                if (ray.transform && !weaponManager.CheckTagOfAvoidTags(ray.transform.tag))
                {
                    Debug.Log($"Ray impact {ray.transform.name}");
                    ImpactManager.Instance.PlayImpactBullet(ray.point,
                        PoolKEY.EffectImpact);
                    break;
                }

        if (_weaponInfo.EffectFire)
        {
            _weaponInfo.EffectFire.Stop();
            _weaponInfo.EffectFire.Play();
        }
        else
        {
            Debug.LogError("_weaponInfo.EffectFire is null");
        }

        EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
            new MsgWeapon
            {
                Bullets = _weaponInfo.Bullets, TotalBullets = _weaponInfo.TotalBullets,
                WeaponKey = _weaponInfo.WeaponType
            });
        StartCoroutine(UpdateStateCanFire());
    }

    protected virtual IEnumerator UpdateStateCanFire()
    {
        _canFire = false;
        yield return new WaitForSeconds(_curTimeResetFire);
        _canFire = true;
    }

    protected virtual void OnChangeFireMode(object obj)
    {
        if (fireModeList.Count <= 1) return;
        var nextIndex = fireModeList.IndexOf(_curFireMode) + 1;
        if (nextIndex >= fireModeList.Count) nextIndex = 0;
        _curFireMode = fireModeList[nextIndex];
        UpdateFireMode();
    }

    protected virtual void UpdateFireMode()
    {
        _bulletsPerShot = 1;
        switch (_curFireMode)
        {
            case "Single":
                _curTimeResetFire = _weaponInfo.TimeResetFireSingleMode;
                break;
            case "Burst":
                _curTimeResetFire = _weaponInfo.TimeResetFireBurstMode;
                _bulletsPerShot = _weaponInfo.BulletsPerShotOfBurstMode;
                break;
            case "Automatic":
                _curTimeResetFire = _weaponInfo.TimeResetFireAutoMode;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RangePickUpWeapon"))
            EventDispatcher.Instance.PostEvent(EventID.OnWeaponPickupAreaEnter, this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RangePickUpWeapon"))
            EventDispatcher.Instance.PostEvent(EventID.OnWeaponPickupAreaExit, this);
    }
}