using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon")] [SerializeField] private string _curWeaponName;

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
    private WeaponCtrl curWeapon;
    private InputBase inputBase;

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
    }

    public void Init(PlayerCtrl playerCtrl)
    {
        this.playerCtrl = playerCtrl;
        isInit = true;
    }

    private void Fire(InputAction.CallbackContext callbackContext)
    {
        if (!curWeapon)
        {
            if (transform.childCount == 0 || !transform.GetChild(0).TryGetComponent(out curWeapon))
            {
                Debug.LogError("CurWeapon not found!");
                return;
            }

            _curWeaponName = curWeapon.name;
        }

        curWeapon.Fire();
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
        inputBase.Weapon.Fire.performed += Fire;
    }

    private void UnLinkInputSystem()
    {
        inputBase.Weapon.Fire.performed -= Fire;
    }

    #endregion
}