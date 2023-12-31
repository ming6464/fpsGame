using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform _playerTf;
    [Header("TagName")] [SerializeField] private string[] _avoidTags;
    [Header("Weapon")] [SerializeField] private string _curWeaponName;
    [SerializeField] private WeaponKEY _curWeaponKey = WeaponKEY.MeleeWeapon;
    [SerializeField] private Weapon _weaponCanPickup;
    [SerializeField] private List<Weapon> _weaponCanPickupList = new();
    [SerializeField] private Transform _cameraMain;
    [Header("Sway")] [SerializeField] private Vector2 _swaySensitive;
    [SerializeField] private float _swaySmoothTime;
    [SerializeField] private float _swayResetSmoothTime;
    [SerializeField] private Vector2 _maxAngleRorate = new(11f, 23f);
    [SerializeField] private Vector2 _minAngleRorate = new(-23f, -10f);
    private PlayerCtrl playerCtrl;
    private Vector3 curRotate;
    private Vector3 targeRotate;
    private bool isInit;
    private Vector3 swayVelocity = Vector3.zero;
    private Vector3 swayResetVelocity = Vector3.zero;
    private InputBase inputBase;
    private Dictionary<WeaponKEY, Weapon> Weapons = new();
    public Transform StartPointRay => _cameraMain;
    public string[] AvoidTags => _avoidTags;

    private void Awake()
    {
        inputBase = new InputBase();
    }

    private void OnEnable()
    {
        LinkInputSystem();
        LinkEvent();
    }

    private void OnDisable()
    {
        UnLinkInputSystem();
        UnLinkEvent();
    }

    private void Start()
    {
        inputBase.Enable();
        LoadWeapons();
    }

    private void Update()
    {
        UpdateRotate();
        UpdateWeaponPickup();
        if (_curWeaponKey == WeaponKEY.None)
            foreach (var dic in Weapons)
                if (dic.Value)
                {
                    ChangeWeapon(dic.Key);
                    break;
                }
    }

    public bool CheckTagOfAvoidTags(string tagCheck)
    {
        return _avoidTags.Contains(tagCheck);
    }

    private void LoadWeapons()
    {
        foreach (Transform child in transform)
            if (child.TryGetComponent(out Weapon weaponCtrl))
            {
                PickUpWeapon(weaponCtrl);
                if (weaponCtrl.WeaponType == WeaponKEY.PrimaryWeapon ||
                    (_curWeaponKey != WeaponKEY.PrimaryWeapon && weaponCtrl.WeaponType <= _curWeaponKey))
                    ChangeWeapon(weaponCtrl.WeaponType);
            }
    }

    public void Init(PlayerCtrl playerCtrl)
    {
        this.playerCtrl = playerCtrl;
        isInit = true;
    }

    private bool ChangeWeapon(WeaponKEY weaponKey)
    {
        if (!Weapons.ContainsKey(weaponKey) || !Weapons[weaponKey])
        {
            Debug.LogError($"weapon has key '{weaponKey}' not found!");
            return false;
        }

        if (Weapons.ContainsKey(_curWeaponKey) && Weapons[_curWeaponKey] != null)
        {
            Weapons[_curWeaponKey].UnUseWeapon();
            Weapons[_curWeaponKey].gameObject.SetActive(false);
        }

        Weapons[weaponKey].UseWeapon();
        _curWeaponKey = weaponKey;
        _curWeaponName = Weapons[weaponKey].WeaponName;
        EventDispatcher.Instance.PostEvent(EventID.OnChangeWeapon, weaponKey);
        return true;
    }

    private bool PickUpWeapon(Weapon weaponCtrl)
    {
        if (!weaponCtrl)
        {
            Debug.LogError($"weapon not found!");
            return false;
        }

        if (!Weapons.TryAdd(weaponCtrl.WeaponType, null)) DropWeapon(weaponCtrl.WeaponType);
        weaponCtrl.PutToBag(this);
        Weapons[weaponCtrl.WeaponType] = weaponCtrl;
        EventDispatcher.Instance.PostEvent(EventID.OnPickUpWeapon,
            new MsgWeapon
            {
                WeaponKey = weaponCtrl.WeaponType, WeaponName = Weapons[weaponCtrl.WeaponType].WeaponName,
                Bullets = Weapons[weaponCtrl.WeaponType].Bullets,
                TotalBullets = Weapons[weaponCtrl.WeaponType].TotalBullets
            });
        if (_curWeaponKey == WeaponKEY.None) ChangeWeapon(weaponCtrl.WeaponType);
        return true;
    }

    private bool DropWeapon(WeaponKEY weaponKey)
    {
        if (!Weapons.ContainsKey(weaponKey) || !Weapons[weaponKey]) return false;
        Weapons[weaponKey].RemoveFromBag();
        Weapons[weaponKey] = null;
        if (weaponKey == _curWeaponKey) _curWeaponKey = WeaponKEY.None;
        return true;
    }

    private void UpdateRotate()
    {
        if (!isInit) return;
        targeRotate.y += _swaySensitive.x * playerCtrl.InputView.x * Time.deltaTime;
        targeRotate.x -= _swaySensitive.y * playerCtrl.InputView.y * Time.deltaTime;
        targeRotate.x = Math.Clamp(targeRotate.x, _minAngleRorate.x, _maxAngleRorate.x);
        targeRotate.y = Math.Clamp(targeRotate.y, _minAngleRorate.y, _maxAngleRorate.y);
        targeRotate.z = -targeRotate.y * 5;
        targeRotate = Vector3.SmoothDamp(targeRotate, Vector3.zero,
            ref swayResetVelocity, _swayResetSmoothTime);
        curRotate = Vector3.SmoothDamp(curRotate, targeRotate, ref swayVelocity, _swaySmoothTime);
        transform.localRotation = Quaternion.Euler(curRotate);
    }

    private void UpdateWeaponPickup()
    {
        var checkChangeWeaponCanPick = false;
        foreach (var weapon in _weaponCanPickupList)
        {
            if (!_weaponCanPickup)
            {
                _weaponCanPickup = weapon;
                checkChangeWeaponCanPick = true;
                continue;
            }

            if (Vector3.Distance(_playerTf.position, _weaponCanPickup.transform.position) >
                Vector3.Distance(_playerTf.position, weapon.transform.position))
            {
                checkChangeWeaponCanPick = true;
                _weaponCanPickup = weapon;
            }
        }

        if (checkChangeWeaponCanPick)
        {
            EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup, new MsgWeapon
            {
                WeaponKey = _weaponCanPickup.WeaponType,
                WeaponName = _weaponCanPickup.WeaponName
            });
        }
        else if (_weaponCanPickup && _weaponCanPickupList.Count == 0)
        {
            _weaponCanPickup = null;
            EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup);
        }
    }

    private void OnWeaponPickupAreaEnter(object obj)
    {
        var weapon = obj as Weapon;
        if (!weapon) return;
        if (_weaponCanPickupList.Contains(weapon)) return;
        _weaponCanPickupList.Add(weapon);
        Debug.Log($"{weapon.name} enter area");
    }

    private void OnWeaponPickupAreaExit(object obj)
    {
        var weapon = obj as Weapon;
        if (!weapon) return;
        if (_weaponCanPickupList.Contains(weapon))
        {
            _weaponCanPickupList.Remove(weapon);
            Debug.Log($"{weapon.name} exit area");
        }
    }


    #region -Input handle-

    private void LinkInputSystem()
    {
        inputBase.Weapon.Fire.performed += FireOnperformed;
        inputBase.Weapon.Fire.canceled += FireOncanceled;
        inputBase.Weapon.ChangeMeleeWeapon.performed += ChangeMeleeWeaponOnperformed;
        inputBase.Weapon.ChangeSecondaryWeapon.performed += ChangeSecondaryWeaponOnperformed;
        inputBase.Weapon.ChangePrimaryWeapon.performed += ChangePrimaryWeaponOnperformed;
        inputBase.Weapon.ChangeExplosives.performed += ChangeExplosivesOnperformed;
        inputBase.Weapon.DropWeapon.performed += DropWeaponOnperformed;
        inputBase.Weapon.PickUpWeapon.performed += PickUpWeaponOnperformed;
        inputBase.Weapon.ChangeFireMode.performed += ChangeFireModeOnperformed;
        inputBase.Weapon.ReloadBullet.performed += ReloadBulletOnperformed;
    }

    private void UnLinkInputSystem()
    {
        inputBase.Weapon.Fire.performed -= FireOnperformed;
        inputBase.Weapon.Fire.canceled -= FireOncanceled;
        inputBase.Weapon.ChangeMeleeWeapon.performed -= ChangeMeleeWeaponOnperformed;
        inputBase.Weapon.ChangeSecondaryWeapon.performed -= ChangeSecondaryWeaponOnperformed;
        inputBase.Weapon.ChangePrimaryWeapon.performed -= ChangePrimaryWeaponOnperformed;
        inputBase.Weapon.ChangeExplosives.performed -= ChangeExplosivesOnperformed;
        inputBase.Weapon.DropWeapon.performed -= DropWeaponOnperformed;
        inputBase.Weapon.PickUpWeapon.performed -= PickUpWeaponOnperformed;
        inputBase.Weapon.ChangeFireMode.performed -= ChangeFireModeOnperformed;
        inputBase.Weapon.ReloadBullet.performed -= ReloadBulletOnperformed;
    }

    private void ReloadBulletOnperformed(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.ReloadBullet);
    }

    private void ChangeFireModeOnperformed(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.OnChangeFireMode);
    }

    private void PickUpWeaponOnperformed(InputAction.CallbackContext obj)
    {
        PickUpWeapon(_weaponCanPickup);
        _weaponCanPickupList.Remove(_weaponCanPickup);
        EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup);
    }

    private void DropWeaponOnperformed(InputAction.CallbackContext obj)
    {
        DropWeapon(_curWeaponKey);
    }

    private void ChangeExplosivesOnperformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.Explosives);
    }

    private void ChangePrimaryWeaponOnperformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.PrimaryWeapon);
    }

    private void ChangeSecondaryWeaponOnperformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.SecondaryWeapon);
    }

    private void ChangeMeleeWeaponOnperformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.MeleeWeapon);
    }

    private void FireOncanceled(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.OnReleaseTrigger);
    }

    private void FireOnperformed(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.OnPullTrigger);
    }

    #endregion

    #region -Event handle-

    private void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnWeaponPickupAreaExit, OnWeaponPickupAreaExit);
        EventDispatcher.Instance.RegisterListener(EventID.OnWeaponPickupAreaEnter, OnWeaponPickupAreaEnter);
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnWeaponPickupAreaExit, OnWeaponPickupAreaExit);
        EventDispatcher.Instance.RemoveListener(EventID.OnWeaponPickupAreaEnter, OnWeaponPickupAreaEnter);
    }

    #endregion
}

[Serializable]
public enum WeaponKEY
{
    None = 9999,
    PrimaryWeapon = 1,
    SecondaryWeapon = 2,
    MeleeWeapon = 3,
    Explosives = 4
}