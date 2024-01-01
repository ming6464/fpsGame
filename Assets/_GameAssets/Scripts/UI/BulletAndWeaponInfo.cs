using System;
using TMPro;
using UnityEngine;

public class BulletAndWeaponInfo : MonoBehaviour
{
    [Serializable]
    public struct InfoWeaponUI
    {
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI WeaponNameText;
        public int Bullets;
        public int TotalBullets;

        public void ResetInfo()
        {
            WeaponNameText.text = "";
            Bullets = 0;
            TotalBullets = 0;
        }

        public void UseWeapon()
        {
            TitleText.fontStyle = FontStyles.Bold;
            WeaponNameText.fontStyle = FontStyles.Bold;
        }

        public void UnUseWeapon()
        {
            TitleText.fontStyle = FontStyles.Italic;
            WeaponNameText.fontStyle = FontStyles.Italic;
        }
    }

    [Header("Weapon")] [SerializeField] private InfoWeaponUI _primaryWeaponInfo;
    [SerializeField] private InfoWeaponUI _secondaryWeaponInfo;
    [SerializeField] private InfoWeaponUI _meleeWeaponInfo;
    [SerializeField] private InfoWeaponUI _explosivesInfo;

    [Header("WeaponCanPickupUI")] [SerializeField]
    private GameObject _uiPanel;

    [SerializeField] private TextMeshProUGUI _weaponNameCanPickupText;
    [SerializeField] private TextMeshProUGUI _weaponTypeCanPickupText;
    [Header("Bullet")] [SerializeField] private TextMeshProUGUI _bullets;
    [SerializeField] private TextMeshProUGUI _totalBullets;
    private WeaponKEY currentWeaponKey;

    private void OnEnable()
    {
        if (EventDispatcher.Instance)
        {
            EventDispatcher.Instance.RegisterListener(EventID.OnPickUpWeapon, OnPickUpWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnDropWeapon, OnDropWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnChangeWeapon, OnChangeWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnUpdateNumberBulletWeapon, OnUpdateNumberBulletWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnUpdateWeaponPickup, OnUpdateWeaponPickup);
        }
    }

    private void OnDisable()
    {
        if (EventDispatcher.Instance)
        {
            EventDispatcher.Instance.RemoveListener(EventID.OnPickUpWeapon, OnPickUpWeapon);
            EventDispatcher.Instance.RemoveListener(EventID.OnDropWeapon, OnDropWeapon);
            EventDispatcher.Instance.RemoveListener(EventID.OnChangeWeapon, OnChangeWeapon);
            EventDispatcher.Instance.RemoveListener(EventID.OnUpdateNumberBulletWeapon, OnUpdateNumberBulletWeapon);
            EventDispatcher.Instance.RemoveListener(EventID.OnUpdateWeaponPickup, OnUpdateWeaponPickup);
        }
    }

    private void OnPickUpWeapon(object obj)
    {
        if (obj is not MsgWeapon msg) return;
        switch (msg.WeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _primaryWeaponInfo.TotalBullets = msg.TotalBullets;
                _primaryWeaponInfo.Bullets = msg.Bullets;
                _primaryWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _secondaryWeaponInfo.TotalBullets = msg.TotalBullets;
                _secondaryWeaponInfo.Bullets = msg.Bullets;
                _secondaryWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.MeleeWeapon:
                _meleeWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _meleeWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.Explosives:
                _explosivesInfo.WeaponNameText.text = msg.WeaponName;
                _explosivesInfo.TotalBullets = msg.TotalBullets;
                _explosivesInfo.Bullets = msg.Bullets;
                _explosivesInfo.UnUseWeapon();
                break;
        }
    }

    private void OnDropWeapon(object obj)
    {
        switch ((WeaponKEY)obj)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.ResetInfo();
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.ResetInfo();
                break;
            case WeaponKEY.MeleeWeapon:
                _meleeWeaponInfo.ResetInfo();
                break;
            case WeaponKEY.Explosives:
                _explosivesInfo.ResetInfo();
                break;
        }
    }

    private void OnChangeWeapon(object obj)
    {
        UnSelectAllWeapon();
        currentWeaponKey = (WeaponKEY)obj;
        switch (currentWeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.UseWeapon();
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.UseWeapon();
                break;
            case WeaponKEY.MeleeWeapon:
                _meleeWeaponInfo.UseWeapon();
                break;
            case WeaponKEY.Explosives:
                _explosivesInfo.UseWeapon();
                break;
        }

        UpdateNumberBulletUI();
    }

    private void OnUpdateNumberBulletWeapon(object obj)
    {
        if (obj is not MsgWeapon msg) return;
        switch (msg.WeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.TotalBullets = msg.TotalBullets;
                _primaryWeaponInfo.Bullets = msg.Bullets;
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.TotalBullets = msg.TotalBullets;
                _secondaryWeaponInfo.Bullets = msg.Bullets;
                break;
            case WeaponKEY.Explosives:
                _explosivesInfo.TotalBullets = msg.TotalBullets;
                _explosivesInfo.Bullets = msg.Bullets;
                break;
        }

        if (msg.WeaponKey == currentWeaponKey) UpdateNumberBulletUI();
    }

    private void UpdateNumberBulletUI()
    {
        _totalBullets.gameObject.SetActive(true);
        _bullets.gameObject.SetActive(true);
        switch (currentWeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _totalBullets.text = _primaryWeaponInfo.TotalBullets.ToString();
                _bullets.text = _primaryWeaponInfo.Bullets.ToString();
                break;
            case WeaponKEY.SecondaryWeapon:
                _totalBullets.text = _secondaryWeaponInfo.TotalBullets.ToString();
                _bullets.text = _secondaryWeaponInfo.Bullets.ToString();
                break;
            case WeaponKEY.Explosives:
                _totalBullets.text = _explosivesInfo.TotalBullets.ToString();
                _bullets.text = _explosivesInfo.Bullets.ToString();
                break;
            case WeaponKEY.MeleeWeapon:
                _totalBullets.gameObject.SetActive(false);
                _bullets.gameObject.SetActive(false);
                break;
        }
    }

    private void UnSelectAllWeapon()
    {
        _primaryWeaponInfo.UnUseWeapon();
        _secondaryWeaponInfo.UnUseWeapon();
        _meleeWeaponInfo.UnUseWeapon();
        _explosivesInfo.UnUseWeapon();
    }

    private void OnUpdateWeaponPickup(object obj)
    {
        if (obj is not MsgWeapon msg)
        {
            _uiPanel.SetActive(false);
            return;
        }

        _uiPanel.SetActive(true);
        _weaponNameCanPickupText.text = msg.WeaponName;
        _weaponTypeCanPickupText.text = msg.WeaponKey.ToString();
    }
}