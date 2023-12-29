using System;
using UnityEngine;

[Serializable]
public class BulletInfo
{
    public Transform BulletPrefab;
    public float BulletVelocity = 30;
    public float BulletTimeLife = 4;
}