using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon")] [SerializeField] private string _curWeaponName;
    [SerializeField] private WeaponKEY _curWeaponKey = WeaponKEY.MeleeWeapon;
    [SerializeField] private WeaponCtrl _weaponCanPickup = null;
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
    private Dictionary<WeaponKEY, WeaponCtrl> Weapons = new();

    private void Awake()
    {
        inputBase = new InputBase();
    }

    private void OnEnable()
    {
        LinkInputSystem();
    }

    private void OnDisable()
    {
        UnLinkInputSystem();
    }

    private void Start()
    {
        inputBase.Enable();
        LoadWeapons();
    }

    private void LoadWeapons()
    {
        foreach (Transform child in transform)
            if (child.TryGetComponent(out WeaponCtrl weaponCtrl))
            {
                PickUpWeapon(weaponCtrl);
                if (weaponCtrl.WeaponType == WeaponKEY.PrimaryWeapon ||
                    (_curWeaponKey != WeaponKEY.PrimaryWeapon && weaponCtrl.WeaponType >= _curWeaponKey))
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
        _curWeaponName = Weapons[weaponKey].name;
        return true;
    }

    private bool PickUpWeapon(WeaponCtrl weaponCtrl)
    {
        if (!weaponCtrl)
        {
            Debug.LogError($"weapon not found!");
            return false;
        }

        if (!Weapons.TryAdd(weaponCtrl.WeaponType, null)) DropWeapon(weaponCtrl.WeaponType);
        if (weaponCtrl.transform.TryGetComponent(out Rigidbody rig))
        {
            rig.Sleep();
            rig.constraints = RigidbodyConstraints.FreezeAll;
            rig.useGravity = false;
        }

        weaponCtrl.UnUseWeapon();
        weaponCtrl.transform.gameObject.SetActive(false);
        Weapons[weaponCtrl.WeaponType] = weaponCtrl;
        return true;
    }

    private bool DropWeapon(WeaponKEY weaponKey)
    {
        if (!Weapons.ContainsKey(weaponKey) || !Weapons[weaponKey])
        {
            Debug.LogError($"weapon has key '{weaponKey}' not found!");
            return false;
        }

        Weapons[weaponKey].UnUseWeapon();
        Weapons[weaponKey].transform.SetParent(null, true);
        if (Weapons[weaponKey].transform.TryGetComponent(out Rigidbody rig))
        {
            rig.WakeUp();
            rig.constraints = RigidbodyConstraints.None;
            rig.useGravity = true;
        }

        Weapons[weaponKey] = null;
        foreach (var dic in Weapons)
            if (dic.Value)
            {
                ChangeWeapon(dic.Key);
                break;
            }

        return true;
    }

    private void Update()
    {
        UpdateRotate();
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

    #region -Input handle-

    private void LinkInputSystem()
    {
        inputBase.Weapon.Fire.performed += context => EventDispatcher.Instance.PostEvent(EventID.OnPullTrigger);
        inputBase.Weapon.Fire.canceled += (context) => EventDispatcher.Instance.PostEvent(EventID.OnReleaseTrigger);
        inputBase.Weapon.ChangeMeleeWeapon.performed += context => ChangeWeapon(WeaponKEY.MeleeWeapon);
        inputBase.Weapon.ChangeSecondaryWeapon.performed += context => ChangeWeapon(WeaponKEY.SecondaryWeapon);
        inputBase.Weapon.ChangePrimaryWeapon.performed += context => ChangeWeapon(WeaponKEY.PrimaryWeapon);
        inputBase.Weapon.ChangeExplosives.performed += context => ChangeWeapon(WeaponKEY.Explosives);
        inputBase.Weapon.DropWeapon.performed += context => DropWeapon(_curWeaponKey);
        inputBase.Weapon.PickUpWeapon.performed += context => PickUpWeapon(_weaponCanPickup);
    }

    private void UnLinkInputSystem()
    {
        inputBase.Weapon.Fire.performed -= context => EventDispatcher.Instance.PostEvent(EventID.OnPullTrigger);
        inputBase.Weapon.Fire.canceled -= (context) => EventDispatcher.Instance.PostEvent(EventID.OnReleaseTrigger);
        inputBase.Weapon.ChangeMeleeWeapon.performed -= context => ChangeWeapon(WeaponKEY.MeleeWeapon);
        inputBase.Weapon.ChangeSecondaryWeapon.performed -= context => ChangeWeapon(WeaponKEY.SecondaryWeapon);
        inputBase.Weapon.ChangePrimaryWeapon.performed -= context => ChangeWeapon(WeaponKEY.PrimaryWeapon);
        inputBase.Weapon.ChangeExplosives.performed -= context => ChangeWeapon(WeaponKEY.Explosives);
        inputBase.Weapon.DropWeapon.performed -= context => DropWeapon(_curWeaponKey);
        inputBase.Weapon.PickUpWeapon.performed -= context => PickUpWeapon(_weaponCanPickup);
    }

    #endregion
}

[Serializable]
public enum WeaponKEY
{
    MeleeWeapon = 1,
    SecondaryWeapon = 2,
    PrimaryWeapon = 3,
    Explosives = 4
}