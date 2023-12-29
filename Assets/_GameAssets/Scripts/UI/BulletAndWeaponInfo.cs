using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BulletAndWeaponInfo : MonoBehaviour
{
    [Serializable]
    public struct InfoWeapon
    {
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI WeaponNameText;
        public int NumberBulletsOfMagazine;
        public int TotalNumberBullets;

        public void ResetInfo()
        {
            WeaponNameText.text = "";
            NumberBulletsOfMagazine = 0;
            TotalNumberBullets = 0;
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

    [Header("Weapon")] [SerializeField] private InfoWeapon _primaryWeaponInfo;
    [SerializeField] private InfoWeapon _secondaryWeaponInfo;
    [SerializeField] private InfoWeapon _meleeWeaponInfo;
    [SerializeField] private InfoWeapon _explosivesInfo;
    [Header("Bullet")] [SerializeField] private TextMeshProUGUI _numberBulletsOfMagazine;
    [SerializeField] private TextMeshProUGUI _totalNumberBullets;

    private void OnEnable()
    {
        if (EventDispatcher.Instance)
        {
            EventDispatcher.Instance.RegisterListener(EventID.OnPickUpWeapon, OnPickUpWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnDropWeapon, OnDropWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnChangeWeapon, OnChangeWeapon);
            EventDispatcher.Instance.RegisterListener(EventID.OnUpdateNumberBulletWeapon, OnUpdateNumberBulletWeapon);
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
        }
    }

    private void OnPickUpWeapon(object obj)
    {
        var msg = obj as MsgWeapon;
        switch (msg.WeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _primaryWeaponInfo.TotalNumberBullets = msg.TotalNumberBullets;
                _primaryWeaponInfo.NumberBulletsOfMagazine = msg.NumberBulletsOfMagazine;
                _primaryWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _secondaryWeaponInfo.TotalNumberBullets = msg.TotalNumberBullets;
                _secondaryWeaponInfo.NumberBulletsOfMagazine = msg.NumberBulletsOfMagazine;
                _secondaryWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.MeleeWeapon:
                _meleeWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _meleeWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.Explosives:
                _explosivesInfo.WeaponNameText.text = msg.WeaponName;
                _explosivesInfo.TotalNumberBullets = msg.TotalNumberBullets;
                _explosivesInfo.NumberBulletsOfMagazine = msg.NumberBulletsOfMagazine;
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
        switch ((WeaponKEY)obj)
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
    }

    private void OnUpdateNumberBulletWeapon(object obj)
    {
        var msg = obj as MsgWeapon;
        switch (msg.WeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.TotalNumberBullets = msg.TotalNumberBullets;
                _primaryWeaponInfo.NumberBulletsOfMagazine = msg.NumberBulletsOfMagazine;
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.TotalNumberBullets = msg.TotalNumberBullets;
                _secondaryWeaponInfo.NumberBulletsOfMagazine = msg.NumberBulletsOfMagazine;
                break;
            case WeaponKEY.Explosives:
                _explosivesInfo.TotalNumberBullets = msg.TotalNumberBullets;
                _explosivesInfo.NumberBulletsOfMagazine = msg.NumberBulletsOfMagazine;
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
}