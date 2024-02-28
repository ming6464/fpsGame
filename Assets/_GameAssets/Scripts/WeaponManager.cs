using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("TagName")]
    [SerializeField]
    private string[] _avoidTags;

    [Header("Weapon")]
    [SerializeField]
    private WeaponKEY _curWeaponKey = WeaponKEY.None;

    [SerializeField]
    private Weapon _weaponCanPickup;

    [SerializeField]
    private List<Weapon> _weaponCanPickupList = new();

    private Transform _mainCam;

    private InputBase inputBase;

    private bool isChangeWeapon;
    private Vector3 swayBagVelocity;

    private Dictionary<WeaponKEY, Weapon> weapons = new();
    public Transform PivotRay => _mainCam;
    public string[] AvoidTags => _avoidTags;

    private void Awake()
    {
        inputBase = new InputBase();
    }

    private void OnEnable()
    {
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
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
        UIManager.Instance.ResetCrossHair();
        LoadWeapons();
    }

    private void Update()
    {
        UpdateRotate();
        UpdateWeaponPickup();
        if (_curWeaponKey == WeaponKEY.None && weapons.Count > 0)
        {
            foreach (KeyValuePair<WeaponKEY, Weapon> dic in weapons)
            {
                if (dic.Value)
                {
                    ChangeWeapon(dic.Key);
                    break;
                }
            }
        }
    }

    public bool CheckTagOfAvoidTags(string tagCheck)
    {
        return _avoidTags.Contains(tagCheck);
    }

    private void LoadWeapons()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out Weapon weaponCtrl))
            {
                PickUpWeapon(weaponCtrl);
                if (weaponCtrl.WeaponType == WeaponKEY.PrimaryWeapon ||
                    (_curWeaponKey != WeaponKEY.PrimaryWeapon && weaponCtrl.WeaponType <= _curWeaponKey))
                {
                    ChangeWeapon(weaponCtrl.WeaponType);
                }
            }
        }
    }

    private bool ChangeWeapon(WeaponKEY weaponKey)
    {
        if (!weapons.ContainsKey(weaponKey) || !weapons[weaponKey])
        {
            Debug.LogError($"weapon has key '{weaponKey}' not found!");
            return false;
        }

        if (weaponKey == _curWeaponKey)
        {
            return false;
        }

        if (weapons.ContainsKey(_curWeaponKey) && weapons[_curWeaponKey] != null)
        {
            weapons[_curWeaponKey].UnUseWeapon();
        }

        UIManager.Instance.ResetCrossHair();
        weapons[weaponKey].UseWeapon();
        _curWeaponKey = weaponKey;
        isChangeWeapon = true;
        transform.localRotation = Quaternion.Euler(60, 0, 0);
        EventDispatcher.Instance.PostEvent(EventID.OnChangeWeapon,
            new MsgWeapon() { WeaponKey = weaponKey, UseScope = weapons[weaponKey] });
        return true;
    }

    private bool PickUpWeapon(Weapon weaponCtrl)
    {
        if (!weaponCtrl)
        {
            Debug.LogError($"weapon not found!");
            return false;
        }

        if (!weapons.TryAdd(weaponCtrl.WeaponType, null))
        {
            DropWeapon(weaponCtrl.WeaponType);
        }

        OnWeaponPickupAreaExit(weaponCtrl);
        weaponCtrl.PutToBag(this, transform);
        weapons[weaponCtrl.WeaponType] = weaponCtrl;

        MsgWeapon msg = new()
            { WeaponKey = weaponCtrl.WeaponType, WeaponName = weapons[weaponCtrl.WeaponType].WeaponName };
        if (weapons[weaponCtrl.WeaponType].transform.TryGetComponent(out Gun gun))
        {
            msg.Bullets = gun.Bullets;
            msg.TotalBullets = gun.TotalBullets;
        }

        EventDispatcher.Instance.PostEvent(EventID.OnPickUpWeapon, msg);

        if (_curWeaponKey == WeaponKEY.None)
        {
            ChangeWeapon(weaponCtrl.WeaponType);
        }

        return true;
    }

    public bool DropWeapon(WeaponKEY weaponKey)
    {
        if (!weapons.ContainsKey(weaponKey) || !weapons[weaponKey])
        {
            return false;
        }

        UIManager.Instance.ResetCrossHair();
        weapons[weaponKey].RemoveFromBag();
        weapons[weaponKey] = null;
        if (weaponKey == _curWeaponKey)
        {
            _curWeaponKey = WeaponKEY.None;
        }

        EventDispatcher.Instance.PostEvent(EventID.OnDropWeapon, weaponKey);

        return true;
    }

    private void UpdateRotate()
    {
        if (isChangeWeapon)
        {
            transform.localRotation = Quaternion.Euler(Vector3.SmoothDamp(transform.localRotation.eulerAngles,
                Vector3.zero,
                ref swayBagVelocity, 0.2f));
            if (Vector3.Distance(transform.localRotation.eulerAngles, Vector3.zero) <= 0.5f)
            {
                transform.localRotation = quaternion.Euler(Vector3.zero);
                isChangeWeapon = false;
                if (weapons[_curWeaponKey])
                {
                    weapons[_curWeaponKey].OnUseWeapon();
                }
            }
        }
    }

    private void UpdateWeaponPickup()
    {
        bool checkChangeWeaponCanPick = false;
        foreach (Weapon weapon in _weaponCanPickupList)
        {
            if (!_weaponCanPickup)
            {
                _weaponCanPickup = weapon;
                checkChangeWeaponCanPick = true;
                continue;
            }

            if (Vector3.Distance(transform.position, _weaponCanPickup.transform.position) >
                Vector3.Distance(transform.position, weapon.transform.position))
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
        Weapon weapon = obj as Weapon;
        if (!weapon)
        {
            return;
        }

        if (_weaponCanPickupList.Contains(weapon))
        {
            return;
        }

        _weaponCanPickupList.Add(weapon);
        Debug.Log($"{weapon.name} enter area");
    }

    private void OnWeaponPickupAreaExit(object obj)
    {
        Weapon weapon = obj as Weapon;
        if (!weapon)
        {
            return;
        }

        if (_weaponCanPickupList.Contains(weapon))
        {
            _weaponCanPickupList.Remove(weapon);
            Debug.Log($"{weapon.name} exit area");
        }
    }


#region -Input handle-

    private void LinkInputSystem()
    {
        inputBase.Weapon.Fire.performed += FireOnPerformed;
        inputBase.Weapon.Fire.canceled += FireOncanceled;
        inputBase.Weapon.ChangeMeleeWeapon.performed += ChangeMeleeWeaponOnPerformed;
        inputBase.Weapon.ChangeSecondaryWeapon.performed += ChangeSecondaryWeaponOnPerformed;
        inputBase.Weapon.ChangePrimaryWeapon.performed += ChangePrimaryWeaponOnPerformed;
        inputBase.Weapon.ChangeGrenade.performed += ChangeGrenadeOnPerformed;
        inputBase.Weapon.DropWeapon.performed += DropWeaponOnPerformed;
        inputBase.Weapon.PickUpWeapon.performed += PickUpWeaponOnPerformed;
        inputBase.Weapon.ChangeFireMode.performed += ChangeFireModeOnPerformed;
        inputBase.Weapon.ReloadBullet.performed += ReloadBulletOnPerformed;
        inputBase.Weapon.Aim.performed += AimOnPerformed;
    }


    private void UnLinkInputSystem()
    {
        inputBase.Weapon.Fire.performed -= FireOnPerformed;
        inputBase.Weapon.Fire.canceled -= FireOncanceled;
        inputBase.Weapon.ChangeMeleeWeapon.performed -= ChangeMeleeWeaponOnPerformed;
        inputBase.Weapon.ChangeSecondaryWeapon.performed -= ChangeSecondaryWeaponOnPerformed;
        inputBase.Weapon.ChangePrimaryWeapon.performed -= ChangePrimaryWeaponOnPerformed;
        inputBase.Weapon.ChangeGrenade.performed -= ChangeGrenadeOnPerformed;
        inputBase.Weapon.DropWeapon.performed -= DropWeaponOnPerformed;
        inputBase.Weapon.PickUpWeapon.performed -= PickUpWeaponOnPerformed;
        inputBase.Weapon.ChangeFireMode.performed -= ChangeFireModeOnPerformed;
        inputBase.Weapon.ReloadBullet.performed -= ReloadBulletOnPerformed;
        inputBase.Weapon.Aim.performed -= AimOnPerformed;
    }

    private void AimOnPerformed(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.AimScope);
    }

    private void ReloadBulletOnPerformed(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.ReloadBullet);
    }

    private void ChangeFireModeOnPerformed(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.OnChangeFireMode);
    }

    private void PickUpWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        PickUpWeapon(_weaponCanPickup);
        _weaponCanPickupList.Remove(_weaponCanPickup);
        EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup);
    }

    private void DropWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        DropWeapon(_curWeaponKey);
    }

    private void ChangeGrenadeOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.Grenade);
    }

    private void ChangePrimaryWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.PrimaryWeapon);
    }

    private void ChangeSecondaryWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.SecondaryWeapon);
    }

    private void ChangeMeleeWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(WeaponKEY.MeleeWeapon);
    }

    private void FireOncanceled(InputAction.CallbackContext obj)
    {
        EventDispatcher.Instance.PostEvent(EventID.OnReleaseTrigger);
    }

    private void FireOnPerformed(InputAction.CallbackContext obj)
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
    Grenade = 4
}