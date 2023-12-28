using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
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

    public void Init(PlayerCtrl playerCtrl)
    {
        this.playerCtrl = playerCtrl;
        isInit = true;
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
}