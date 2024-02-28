using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private Transform _crossHairTf;

    [SerializeField]
    private Transform _ScopeViewTf;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void ResetCrossHair()
    {
        HandleCrossHair(false);
        HandleAimScope(false);
    }

    public void HandleCrossHair(bool isShow)
    {
        _crossHairTf.gameObject.SetActive(isShow);
    }

    public void HandleAimScope(bool isShow)
    {
        _ScopeViewTf.gameObject.SetActive(isShow);
        HandleCrossHair(false);
    }
}