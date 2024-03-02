using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [Space(10)]
    public string[] AvoidTags;

    public Sprite RelaxedHandsIcon;

    [Header("Weapon")]
    [SerializeField]
    private int _curWeaponIndex = -1;

    [SerializeField]
    private Bag[] _slotsInfo;

    [Space(10)]
    [SerializeField]
    private List<Weapon> _weaponCanPickupList = new();

    [SerializeField]
    private int _indexWeaponCanPickup;

    [Header("Rig")]
    [SerializeField]
    private Animator _rigAnimator;

    [SerializeField]
    private Rig _handIk;

    [Space(10)]
    public int TotalBulletRifle;

    public int TotalBulletShotgun;
    public int TotalBulletPistol;

    [Header("References")]
    [SerializeField]
    private AimAndPivotScript _aimAndPivotScript;

    [SerializeField]
    private PlayerScript PlayerScript;

    private InputBase m_inputBase;

    public Transform PivotRay => _aimAndPivotScript.PivotRay;
    private int m_grenadeSlotIndex;

    public bool NeedSupplies => TotalBulletRifle < BagInfo.TotalBulletRifle
                                || TotalBulletPistol < BagInfo.TotalBulletPistol ||
                                TotalBulletShotgun < BagInfo.TotalBulletShotgun;

    public BagInfo BagInfo;

    private void Awake()
    {
        m_inputBase = new InputBase();
        LinkInputSystem();
        if (GameConfig.Instance)
        {
            BagInfo = GameConfig.Instance.GetBagInfo();
            TotalBulletRifle = BagInfo.TotalBulletRifle;
            TotalBulletShotgun = BagInfo.TotalBulletShotgun;
            TotalBulletPistol = BagInfo.TotalBulletPistol;
            m_grenadeSlotIndex = _slotsInfo.Length - 1;
        }
    }

    private void OnEnable()
    {
        m_inputBase.Enable();
        LinkEvent();
    }

    private void OnDisable()
    {
        UnLinkEvent();
        m_inputBase.Disable();
    }

    private void Start()
    {
        LoadWeaponInBag();
        if (CheckNearestBagWeapon() < 0)
        {
            _handIk.weight = 0f;
            EventDispatcher.Instance.PostEvent(EventID.OnRelaxedHands, RelaxedHandsIcon);
        }
        else
        {
            EventDispatcher.Instance.PostEvent(EventID.OnRelaxedHands);
        }
    }

    private void Update()
    {
        UpdateWeaponPickup();
    }

    private void LateUpdate()
    {
        if (_curWeaponIndex < 0)
        {
            int index = CheckNearestBagWeapon();
            if (index >= 0)
            {
                ChangeWeapon(index);
            }
        }
    }

    private int CheckNearestBagWeapon()
    {
        for (int i = 0; i < _slotsInfo.Length; i++)
        {
            if (GetWeaponFromBag(i) != null)
            {
                return i;
            }
        }

        return -1;
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
        }
        else
        {
            _indexWeaponCanPickup = -1;
            EventDispatcher.Instance.PostEvent(EventID.OnUpdateWeaponPickup);
        }
    }

    private void LoadWeaponInBag()
    {
        if (_slotsInfo.Length == 0 || GameConfig.Instance.GetBagInfo().WeaponNames.Length == 0)
        {
            return;
        }

        foreach (string wpName in GameConfig.Instance.GetBagInfo().WeaponNames)
        {
            GameObject wp = Instantiate(GameConfig.Instance.GetWeaponPrefab(wpName));
            PickUpWeapon(wp.GetComponent<Weapon>());
        }

        ChangeWeapon(0);
    }

    private int GetEmptyWeaponSlot()
    {
        for (int i = 0; i < _slotsInfo.Length - 1; i++)
        {
            if (_slotsInfo[i].BagTf.childCount == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private bool CheckEmptyGrenadeSlot()
    {
        return _slotsInfo[m_grenadeSlotIndex].BagTf.childCount == 0;
    }

    private bool CheckUsingGrande()
    {
        return _curWeaponIndex == m_grenadeSlotIndex;
    }

    private void PickUpWeapon(Weapon weapon)
    {
        if (!weapon)
        {
            return;
        }

        _handIk.weight = 1f;
        EventDispatcher.Instance.PostEvent(EventID.OnRelaxedHands);

        OnWeaponPickupAreaExit(weapon);

        if (weapon.WeaponType == WeaponKEY.Grenade)
        {
            bool grenadeUsing = CheckUsingGrande();
            if (!CheckEmptyGrenadeSlot())
            {
                DropWeapon(m_grenadeSlotIndex);
            }

            weapon.PutToBag(this,
                _slotsInfo[m_grenadeSlotIndex]);
            if (_curWeaponIndex < 0)
            {
                ChangeWeapon(m_grenadeSlotIndex);
            }

            return;
        }

        int index = GetEmptyWeaponSlot();
        if (index < 0)
        {
            index = CheckUsingGrande() ? 0 : _curWeaponIndex;
            DropWeapon(index);
        }

        weapon.PutToBag(this, _slotsInfo[index]);
        if (_curWeaponIndex < 0)
        {
            ChangeWeapon(index);
        }
    }

    private Weapon GetWeaponFromBag(int index)
    {
        if (_slotsInfo.Length < index)
        {
            return null;
        }

        Transform slot = _slotsInfo[index].BagTf;
        return slot.childCount == 0 ? null : slot.GetChild(0).GetComponent<Weapon>();
    }

    private void ChangeWeapon(int index)
    {
        if (_slotsInfo.Length == 0 || _curWeaponIndex == index)
        {
            return;
        }

        Weapon weapon = GetWeaponFromBag(index);
        if (weapon == null)
        {
            return;
        }

        weapon.UseWeapon();

        if (_rigAnimator)
        {
            _rigAnimator.Play($"Equip_{weapon.WeaponName}");
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
        if (_curWeaponIndex == -1 || _slotsInfo.Length == 0)
        {
            return;
        }

        GetWeaponFromBag(index).RemoveFromBag();
        if (_curWeaponIndex == index)
        {
            _curWeaponIndex = -1;
        }

        if (CheckNearestBagWeapon() < 0)
        {
            _handIk.weight = 0f;
            EventDispatcher.Instance.PostEvent(EventID.OnRelaxedHands, RelaxedHandsIcon);
        }
    }

    public void ThrowGrenade()
    {
        if (!CheckUsingGrande())
        {
            return;
        }

        _curWeaponIndex = -1;
    }

    public void SupplyBullet()
    {
        TotalBulletRifle = BagInfo.TotalBulletRifle;
        TotalBulletShotgun = BagInfo.TotalBulletShotgun;
        TotalBulletPistol = BagInfo.TotalBulletPistol;
        for (int i = 0; i < 3; i++)
        {
            GetWeaponFromBag(i)?.GetComponent<Gun>().Supply();
        }
    }


#region -Input handle-

    private void LinkInputSystem()
    {
        m_inputBase.Weapon.ChangeGun3.performed += ChangeGun3;
        m_inputBase.Weapon.ChangeGun2.performed += ChangeGun2;
        m_inputBase.Weapon.ChangeGun1.performed += ChangeGun1;
        m_inputBase.Weapon.ChangeGrenade.performed += ChangeGrenadeOnPerformed;
        m_inputBase.Weapon.DropWeapon.performed += DropWeaponOnPerformed;
        m_inputBase.Weapon.PickUpWeapon.performed += PickUpWeaponOnPerformed;
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
        ChangeWeapon(m_grenadeSlotIndex);
    }

    private void ChangeGun1(InputAction.CallbackContext obj)
    {
        ChangeWeapon(0);
    }

    private void ChangeGun2(InputAction.CallbackContext obj)
    {
        ChangeWeapon(1);
    }

    private void ChangeGun3(InputAction.CallbackContext obj)
    {
        ChangeWeapon(2);
    }

#endregion

#region -Event handle-

    private void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnWeaponPickupAreaExit, OnWeaponPickupAreaExit);
        EventDispatcher.Instance.RegisterListener(EventID.OnWeaponPickupAreaEnter, OnWeaponPickupAreaEnter);
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishGame, OnFinishGame);
    }

    private void OnFinishGame(object obj)
    {
        m_inputBase.Disable();
        UnLinkEvent();
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnWeaponPickupAreaExit, OnWeaponPickupAreaExit);
        EventDispatcher.Instance.RemoveListener(EventID.OnWeaponPickupAreaEnter, OnWeaponPickupAreaEnter);
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishGame, OnFinishGame);
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

[Serializable]
public enum WeaponKEY
{
    None = 9999,
    Grenade = 2,
    Rifle,
    Shotgun,
    Pistol
}