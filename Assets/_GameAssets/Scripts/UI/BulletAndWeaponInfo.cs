using System;
using TMPro;
using UnityEngine;

public class BulletAndWeaponInfo : MonoBehaviour
{
    [SerializeField]
    private Color _selectedColor;

    [SerializeField]
    private Color _normalColor;

    [Serializable]
    public struct InfoWeaponUI
    {
        public GameObject Panel;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI WeaponNameText;
        public int Bullets;
        public int TotalBullets;

        [HideInInspector]
        public Color SelectedColor;

        [HideInInspector]
        public Color NormalColor;

        public void PickUp()
        {
            Panel.SetActive(true);
        }

        public void Drop()
        {
            WeaponNameText.text = "";
            Bullets = 0;
            TotalBullets = 0;
            Panel.SetActive(false);
        }

        public void UseWeapon()
        {
            TitleText.fontStyle = FontStyles.Bold;
            WeaponNameText.fontStyle = FontStyles.Bold;
            WeaponNameText.color = SelectedColor;
        }

        public void UnUseWeapon()
        {
            TitleText.fontStyle = FontStyles.Italic;
            WeaponNameText.fontStyle = FontStyles.Italic;
            WeaponNameText.color = NormalColor;
        }
    }

    [Header("Weapon")]
    [SerializeField]
    private InfoWeaponUI _primaryWeaponInfo;

    [SerializeField]
    private InfoWeaponUI _secondaryWeaponInfo;

    [SerializeField]
    private InfoWeaponUI _meleeWeaponInfo;

    [SerializeField]
    private InfoWeaponUI _explosivesInfo;

    [Header("WeaponCanPickupUI")]
    [SerializeField]
    private GameObject _uiPanel;

    [SerializeField]
    private TextMeshProUGUI _weaponNameCanPickupText;

    [SerializeField]
    private TextMeshProUGUI _weaponTypeCanPickupText;

    [Header("Bullet")]
    [SerializeField]
    private TextMeshProUGUI _bullets;

    [SerializeField]
    private TextMeshProUGUI _totalBullets;

    private WeaponKEY currentWeaponKey;

    private void Awake()
    {
        _primaryWeaponInfo.SelectedColor = _selectedColor;
        _primaryWeaponInfo.NormalColor = _normalColor;

        _secondaryWeaponInfo.SelectedColor = _selectedColor;
        _secondaryWeaponInfo.NormalColor = _normalColor;

        _meleeWeaponInfo.SelectedColor = _selectedColor;
        _meleeWeaponInfo.NormalColor = _normalColor;

        _explosivesInfo.SelectedColor = _selectedColor;
        _explosivesInfo.NormalColor = _normalColor;

        _primaryWeaponInfo.Drop();
        _secondaryWeaponInfo.Drop();
        _meleeWeaponInfo.Drop();
        _explosivesInfo.Drop();
    }

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
        if (obj is not MsgWeapon msg || msg.WeaponKey == WeaponKEY.None)
        {
            return;
        }

        Debug.Log("Key pickup " + msg.WeaponKey);

        switch (msg.WeaponKey)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _primaryWeaponInfo.TotalBullets = msg.TotalBullets;
                _primaryWeaponInfo.Bullets = msg.Bullets;
                _primaryWeaponInfo.PickUp();
                _primaryWeaponInfo.UnUseWeapon();
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _secondaryWeaponInfo.TotalBullets = msg.TotalBullets;
                _secondaryWeaponInfo.Bullets = msg.Bullets;
                _secondaryWeaponInfo.UnUseWeapon();
                _secondaryWeaponInfo.PickUp();
                break;
            case WeaponKEY.MeleeWeapon:
                _meleeWeaponInfo.WeaponNameText.text = msg.WeaponName;
                _meleeWeaponInfo.UnUseWeapon();
                _meleeWeaponInfo.PickUp();
                break;
            case WeaponKEY.Grenade:
                _explosivesInfo.WeaponNameText.text = msg.WeaponName;
                _explosivesInfo.TotalBullets = msg.TotalBullets;
                _explosivesInfo.Bullets = msg.Bullets;
                _explosivesInfo.UnUseWeapon();
                _explosivesInfo.PickUp();
                break;
        }
    }

    private void OnDropWeapon(object obj)
    {
        switch ((WeaponKEY)obj)
        {
            case WeaponKEY.PrimaryWeapon:
                _primaryWeaponInfo.Drop();
                break;
            case WeaponKEY.SecondaryWeapon:
                _secondaryWeaponInfo.Drop();
                break;
            case WeaponKEY.MeleeWeapon:
                _meleeWeaponInfo.Drop();
                break;
            case WeaponKEY.Grenade:
                _explosivesInfo.Drop();
                break;
        }
    }

    private void OnChangeWeapon(object obj)
    {
        UnSelectAllWeapon();
        MsgWeapon msg = (MsgWeapon)obj;
        currentWeaponKey = msg.WeaponKey;
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
            case WeaponKEY.Grenade:
                _explosivesInfo.UseWeapon();
                break;
        }

        UpdateNumberBulletUI();
    }

    private void OnUpdateNumberBulletWeapon(object obj)
    {
        if (obj is not MsgWeapon msg)
        {
            return;
        }

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
            case WeaponKEY.Grenade:
                _explosivesInfo.TotalBullets = msg.TotalBullets;
                _explosivesInfo.Bullets = msg.Bullets;
                break;
        }

        if (msg.WeaponKey == currentWeaponKey)
        {
            UpdateNumberBulletUI();
        }
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
            case WeaponKEY.Grenade:
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