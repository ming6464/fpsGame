using System;
using UnityEngine;

[Serializable]
public class WeaponInfo
{
    public Transform PivotFireTf;
    public WeaponKEY WeaponType;
    public string WeaponName;
    public int TotalBullets = 100;
    public int Bullets = 30;
    public int MagazineCapacity = 30;
    public float Dame = 10;
    public float Range = 100f;
    [Header("Fire Mode")] public bool Automatic;
    public float TimeResetFireAutoMode;
    public bool Single;
    public float TimeResetFireSingleMode;
    public bool Burst;
    public float TimeResetFireBurstMode;
    public int BulletsPerShotOfBurstMode;
    [Header("Scope")] public bool UseScope;
}