using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _healthText;

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeHealth, OnChangeHealth);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnChangeHealth, OnChangeHealth);
    }

    private void OnChangeHealth(object obj)
    {
        _healthText.text = (string)obj;
    }


    private void Start()
    {
    }
}