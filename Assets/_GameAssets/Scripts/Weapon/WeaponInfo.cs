using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponInfo
{
    public string WeaponName;
    public WeaponKEY WeaponType;
    public GameObject WeaponPrefab;
    public float Dame;
    public Sprite IconWeapon;

    [Space(3)]
    [Header("Gun Information")]
    [Header("Bullet")]
    [Min(1f)]
    public int MagazineCapacity = 1;

    [Min(1f)]
    public int BulletsPerShot = 1;

    [Header("Fire Mode")]
    public List<FireModeInfo> FireModeOption;

    [Space(3)]
    [Header("Grenade Information")]
    [Min(0f)]
    public float ExplosionTime;

    public float ExplosionTimeout;

    public float ExplosionRadius;
}


[Serializable]
public class FireModeInfo
{
    public FireModeKEY FireModeType;
    public float TimeResetFire;
    public int FireTimes;
    public float Spread;
}

[Serializable]
public enum FireModeKEY
{
    Single = 0,
    Automatic = 2,
    None
}