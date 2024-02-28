using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRootMotion : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float timeScale;

    private void Update()
    {
        Time.timeScale = timeScale;
    }
}