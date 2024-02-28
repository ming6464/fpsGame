using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [Space(10)]
    [SerializeField]
    public string[] AvoidTags;

    [Header("Weapon")]
    [SerializeField]
    private int _curWeaponIndex = -1;

    [SerializeField]
    private Bag[] _bagInfo;

    [Space(10)]
    [SerializeField]
    private List<Weapon_2> _weaponCanPickupList = new();

    [SerializeField]
    private int _indexWeaponCanPickup;

    [Header("Rig")]
    [SerializeField]
    private Animator _rigAnimator;

    [SerializeField]
    private Rig _handIk;


    [Space(10)]
    public int TotalBullet;

    private InputBase _inputBase;
    private Transform _mainCam;

    public Transform PivotRay => _mainCam;
    private float _rotationVelocity;
    private int _grenadeSlotIndex;


    private void Awake()
    {
        _inputBase = new InputBase();
        if (GameConfig.Instance)
        {
            TotalBullet = GameConfig.Instance.GetBagInfo().TotalBullet;
            _grenadeSlotIndex = _bagInfo.Length - 1;
        }
    }

    private void OnEnable()
    {
        _inputBase.Enable();
        LinkInputSystem();
        LinkEvent();
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void OnDisable()
    {
        UnLinkInputSystem();
        UnLinkEvent();
        _inputBase.Disable();
    }

    private void Start()
    {
        LoadWeaponInBag();
    }

    private void Update()
    {
        UpdateWeaponPickup();
    }

    private void LateUpdate()
    {
        if (_curWeaponIndex < 0)
        {
            for (int i = 0; i < _bagInfo.Length; i++)
            {
                if (GetWeaponFromBag(i) != null)
                {
                    ChangeWeapon(i);
                    break;
                }
            }
        }
    }

    private void UpdateWeaponPickup()
    {
        if (_weaponCanPickupList.Count > 0)
        {
            _indexWeaponCanPickup = 0;
            float nearestWeaponDistance =
                Vector3.Distance(transform.position, _weaponCanPickupList[0].transform.position);
            for (int i = 1; i < _weaponCanPickupList.Count; i++)
            {
                float dis = Vector3.Distance(transform.position, _weaponCanPickupList[i].transform.position);
                if (nearestWeaponDistance > dis)
                {
                    _indexWeaponCanPickup = i;
                    nearestWeaponDistance = dis;
                }
            }
            // EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup, new MsgWeapon
            // {
            //     WeaponKey = _weaponCanPickup.WeaponType,
            //     WeaponName = _weaponCanPickup.WeaponName
            // });
        }
        else
        {
            _indexWeaponCanPickup = -1;
            EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup);
        }
    }

    private void LoadWeaponInBag()
    {
        if (_bagInfo.Length == 0 || GameConfig.Instance.GetBagInfo().WeaponNames.Length == 0)
        {
            return;
        }

        foreach (string wpName in GameConfig.Instance.GetBagInfo().WeaponNames)
        {
            GameObject wp = Instantiate(GameConfig.Instance.GetWeaponPrefab(wpName));
            PickUpWeapon(wp.GetComponent<Weapon_2>());
        }

        ChangeWeapon(0);
    }

    private int GetEmptyWeaponSlot()
    {
        for (int i = 0; i < _bagInfo.Length - 1; i++)
        {
            if (_bagInfo[i].BagTf.childCount == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private bool CheckEmptyGrenadeSlot()
    {
        return _bagInfo[_grenadeSlotIndex].BagTf.childCount == 0;
    }

    private bool CheckUsingGrande()
    {
        return _curWeaponIndex == _grenadeSlotIndex;
    }

    private void PickUpWeapon(Weapon_2 weapon)
    {
        if (!weapon)
        {
            return;
        }

        OnWeaponPickupAreaExit(weapon);

        if (weapon.WeaponType == WeaponKEY.Grenade)
        {
            bool grenadeUsing = CheckUsingGrande();
            if (!CheckEmptyGrenadeSlot())
            {
                DropWeapon(_grenadeSlotIndex);
            }

            weapon.PutToBag(this,
                _bagInfo[_grenadeSlotIndex]);
            if (_curWeaponIndex < 0)
            {
                ChangeWeapon(_grenadeSlotIndex);
            }

            return;
        }

        int index = GetEmptyWeaponSlot();
        if (index < 0)
        {
            index = CheckUsingGrande() ? 0 : _curWeaponIndex;
            DropWeapon(index);
        }

        weapon.PutToBag(this, _bagInfo[index]);
        if (_curWeaponIndex < 0)
        {
            ChangeWeapon(index);
        }
    }

    private Weapon_2 GetWeaponFromBag(int index)
    {
        if (_bagInfo.Length < index)
        {
            return null;
        }

        Transform slot = _bagInfo[index].BagTf;
        return slot.childCount == 0 ? null : slot.GetChild(0).GetComponent<Weapon_2>();
    }


    private void ChangeWeapon(int index)
    {
        if (_bagInfo.Length == 0 || _curWeaponIndex == index)
        {
            return;
        }

        Weapon_2 weapon = GetWeaponFromBag(index);
        if (weapon == null)
        {
            return;
        }

        string animName = "Equip_";
        if (_grenadeSlotIndex == index)
        {
            animName += "grenade_";
        }

        weapon.UseWeapon();
        if (_rigAnimator)
        {
            _rigAnimator.Play(animName + weapon.WeaponName);
        }

        if (_curWeaponIndex >= 0)
        {
            GetWeaponFromBag(_curWeaponIndex)?.UnUseWeapon();
        }

        _handIk.weight = 1.0f;
        _curWeaponIndex = index;
    }

    private void DropWeapon(int index)
    {
        if (_curWeaponIndex == -1 || _bagInfo.Length == 0)
        {
            return;
        }

        GetWeaponFromBag(index).RemoveFromBag();
        if (_curWeaponIndex == index)
        {
            _curWeaponIndex = -1;
        }
    }


#region -Input handle-

    private void LinkInputSystem()
    {
        _inputBase.Weapon.Fire.performed += FireOnPerformed;
        _inputBase.Weapon.Fire.canceled += FireOncanceled;
        _inputBase.Weapon.ChangeMeleeWeapon.performed += ChangeMeleeWeaponOnPerformed;
        _inputBase.Weapon.ChangeSecondaryWeapon.performed += ChangeSecondaryWeaponOnPerformed;
        _inputBase.Weapon.ChangePrimaryWeapon.performed += ChangePrimaryWeaponOnPerformed;
        _inputBase.Weapon.ChangeGrenade.performed += ChangeGrenadeOnPerformed;
        _inputBase.Weapon.DropWeapon.performed += DropWeaponOnPerformed;
        _inputBase.Weapon.PickUpWeapon.performed += PickUpWeaponOnPerformed;
        _inputBase.Weapon.ChangeFireMode.performed += ChangeFireModeOnPerformed;
        _inputBase.Weapon.ReloadBullet.performed += ReloadBulletOnPerformed;
        _inputBase.Weapon.Aim.performed += AimOnPerformed;
    }


    private void UnLinkInputSystem()
    {
        _inputBase.Weapon.Fire.performed -= FireOnPerformed;
        _inputBase.Weapon.Fire.canceled -= FireOncanceled;
        _inputBase.Weapon.ChangeMeleeWeapon.performed -= ChangeMeleeWeaponOnPerformed;
        _inputBase.Weapon.ChangeSecondaryWeapon.performed -= ChangeSecondaryWeaponOnPerformed;
        _inputBase.Weapon.ChangePrimaryWeapon.performed -= ChangePrimaryWeaponOnPerformed;
        _inputBase.Weapon.ChangeGrenade.performed -= ChangeGrenadeOnPerformed;
        _inputBase.Weapon.DropWeapon.performed -= DropWeaponOnPerformed;
        _inputBase.Weapon.PickUpWeapon.performed -= PickUpWeaponOnPerformed;
        _inputBase.Weapon.ChangeFireMode.performed -= ChangeFireModeOnPerformed;
        _inputBase.Weapon.ReloadBullet.performed -= ReloadBulletOnPerformed;
        _inputBase.Weapon.Aim.performed -= AimOnPerformed;
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
        try
        {
            PickUpWeapon(_weaponCanPickupList[_indexWeaponCanPickup]);
        }
        catch (Exception e)
        {
            //ignore
        }
    }

    private void DropWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        DropWeapon(_curWeaponIndex);
    }

    private void ChangeGrenadeOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(_grenadeSlotIndex);
    }

    private void ChangePrimaryWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(0);
    }

    private void ChangeSecondaryWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(1);
    }

    private void ChangeMeleeWeaponOnPerformed(InputAction.CallbackContext obj)
    {
        ChangeWeapon(2);
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

    private void OnWeaponPickupAreaEnter(object obj)
    {
        Weapon_2 weapon = obj as Weapon_2;
        if (!weapon)
        {
            return;
        }

        if (_weaponCanPickupList.Contains(weapon))
        {
            return;
        }

        _weaponCanPickupList.Add(weapon);
    }

    private void OnWeaponPickupAreaExit(object obj)
    {
        Weapon_2 weapon = obj as Weapon_2;
        if (!weapon)
        {
            return;
        }

        if (_weaponCanPickupList.Contains(weapon))
        {
            if (_weaponCanPickupList.IndexOf(weapon) == _indexWeaponCanPickup)
            {
                _indexWeaponCanPickup = -1;
            }

            _weaponCanPickupList.Remove(weapon);
        }
    }

#endregion
}

[Serializable]
public class Bag
{
    public Transform BagTf;
    public MultiParentConstraint Constraint;
}