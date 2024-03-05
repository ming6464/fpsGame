using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIBottomGamePlay : MonoBehaviour
{
    [Header("Info Player")]
    [SerializeField]
    private TextMeshProUGUI _currentHpText;

    [SerializeField]
    private Image _currentWeaponImg;

    [SerializeField]
    private TextMeshProUGUI _currentFireModeText;

    [SerializeField]
    private Image _slideReloadBullet;

    [SerializeField]
    private TextMeshProUGUI _currentBulletsText;

    [SerializeField]
    private TextMeshProUGUI _totalBulletsText;

    [SerializeField]
    private CanvasGroup _bulletInfoCG;

    [Header("Supply")]
    [SerializeField]
    private Image _supplyImage;

    //
    private bool isReloadBullet;

    private void OnEnable()
    {
        LinkEvent();
    }

    private void OnDisable()
    {
        UnLinkEvent();
    }

    private void Update()
    {
        if (isReloadBullet)
        {
            _slideReloadBullet.fillAmount = Mathf.MoveTowards(_slideReloadBullet.fillAmount, 1, Time.deltaTime);
            if (_slideReloadBullet.fillAmount == 1)
            {
                isReloadBullet = false;
                EventDispatcher.Instance.PostEvent(EventID.OnFinishReload);
            }
        }
    }

#region Event

    private void LinkEvent()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeWeapon, OnChangeWeapon);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeHealth, OnChangeHealth);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeBullets, OnChangeBullets);
        EventDispatcher.Instance.RegisterListener(EventID.OnchangeTotalBullets, OnchangeTotalBullets);
        EventDispatcher.Instance.RegisterListener(EventID.OnRelaxedHands, OnRelaxedHands);
        EventDispatcher.Instance.RegisterListener(EventID.OnReloadBullet, OnReloadBullet);
        EventDispatcher.Instance.RegisterListener(EventID.OnUpdateSupplyProgress, OnUpdateSupplyProgress);
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeFireMode, OnChangeFireMode);
    }

    private void OnUpdateSupplyProgress(object obj)
    {
        if (obj == null)
        {
            _supplyImage.gameObject.SetActive(false);
            return;
        }

        _supplyImage.gameObject.SetActive(true);
        float value = (float)obj;
        _supplyImage.fillAmount = value;
    }

    private void OnReloadBullet(object obj)
    {
        isReloadBullet = true;
        _slideReloadBullet.fillAmount = 0f;
    }

    private void OnRelaxedHands(object obj = null)
    {
        if (obj == null)
        {
            return;
        }

        _bulletInfoCG.alpha = 0f;
        _currentWeaponImg.sprite = (Sprite)obj;
        _slideReloadBullet.sprite = (Sprite)obj;
        _slideReloadBullet.fillAmount = 1f;
        OnChangeFireMode(null);
    }

    private void OnchangeTotalBullets(object obj)
    {
        int total = (int)obj;
        _totalBulletsText.text = total.ToString();
    }

    private void OnChangeBullets(object obj)
    {
        int bullet = (int)obj;
        _currentBulletsText.text = bullet.ToString();
        _slideReloadBullet.fillAmount = bullet <= 0 ? 0 : 1f;
    }

    private void OnChangeHealth(object obj)
    {
        int hp = Mathf.RoundToInt((float)obj);
        _currentHpText.text = hp.ToString();
    }

    private void OnChangeWeapon(object obj)
    {
        MsgWeapon msg = (MsgWeapon)obj;
        if (msg == null)
        {
            return;
        }

        isReloadBullet = false;
        _currentWeaponImg.sprite = msg.WeaponIcon;
        _slideReloadBullet.sprite = msg.WeaponIcon;
        _slideReloadBullet.fillAmount = 1f;
        if (msg.WeaponKey == WeaponKEY.Grenade)
        {
            _bulletInfoCG.alpha = 0f;
            _slideReloadBullet.fillAmount = 1f;
            OnChangeFireMode(null);
        }
        else
        {
            _bulletInfoCG.alpha = 1f;
            OnChangeBullets(msg.Bullets);
            _currentFireModeText.gameObject.SetActive(true);
            OnChangeFireMode(msg.FireModeKey);
        }
    }

    private void OnChangeFireMode(object obj)
    {
        if (obj == null)
        {
            _currentFireModeText.gameObject.SetActive(false);
            return;
        }

        _currentFireModeText.gameObject.SetActive(true);
        FireModeKEY key = (FireModeKEY)obj;
        _currentFireModeText.text = key.ToString();
    }

    private void UnLinkEvent()
    {
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeWeapon, OnChangeWeapon);
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeHealth, OnChangeHealth);
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeBullets, OnChangeBullets);
        EventDispatcher.Instance.RemoveListener(EventID.OnchangeTotalBullets, OnchangeTotalBullets);
        EventDispatcher.Instance.RemoveListener(EventID.OnRelaxedHands, OnRelaxedHands);
        EventDispatcher.Instance.RemoveListener(EventID.OnReloadBullet, OnReloadBullet);
        EventDispatcher.Instance.RemoveListener(EventID.OnUpdateSupplyProgress, OnUpdateSupplyProgress);
        EventDispatcher.Instance.RemoveListener(EventID.OnChangeFireMode, OnChangeFireMode);
    }

#endregion
}