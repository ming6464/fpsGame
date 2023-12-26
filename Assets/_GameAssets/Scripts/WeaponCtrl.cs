using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCtrl : MonoBehaviour
{
    [Header("Sway")] [SerializeField] private Vector2 _swaySensitive;
    [SerializeField] private float _swaySmoothTime;
    [SerializeField] private float _swayResetSmoothTime;
    [SerializeField] private Vector2 _maxAngleRorate = new(11f, 23f);
    [SerializeField] private Vector2 _minAngleRorate = new(-23f, -10f);
    private PlayerCtrl playerCtrl;
    private Vector2 curRotate;
    private Vector2 targeRotate;
    private bool isInit;
    private Vector2 swayVelocity = Vector3.zero;
    private Vector2 swayResetVelocity = Vector3.zero;

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
        targeRotate = Vector2.SmoothDamp(targeRotate, Vector2.zero,
            ref swayResetVelocity, _swayResetSmoothTime);
        curRotate = Vector2.SmoothDamp(curRotate, targeRotate, ref swayVelocity, _swaySmoothTime);
        transform.localRotation = Quaternion.Euler(curRotate.x, curRotate.y, 0);
    }
}