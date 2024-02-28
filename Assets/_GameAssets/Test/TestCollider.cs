using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestCollider : MonoBehaviour
{
    private InputBase inputBase;

    private void Start()
    {
        inputBase = new InputBase();
        inputBase.Enable();
    }
}