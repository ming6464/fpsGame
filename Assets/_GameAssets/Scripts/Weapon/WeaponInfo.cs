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

    [Space(3)]
    [Header("Gun Information")]
    [Header("Bullet")]
    [Min(1f)]
    public int MagazineCapacity = 1;

    [Min(1f)]
    public int BulletsPerShot = 1;

    [Header("Fire Mode")]
    public List<FireModeInfo> FireModeOption;

    [Header("Recoil")]
    [Range(0, 1f)]
    public float MinRecoilPercentage;

    [Range(0.2f, 1)]
    public float MaxRecoilTime = 0.2f;

    [Range(0, 10)]
    public float RecoilAmountX;

    [Range(0, 10)]
    public float RecoilAmountY;

    [Range(0, 1)]
    public float RecoilAmountZ;

    [Header("Scope")]
    public bool UseScope;

    [Space(3)]
    [Header("Grenade Information")]
    [Min(0f)]
    public float ThrowForce;

    [Min(0f)]
    public float ThrowAngle;

    [Min(0f)]
    public float ExplosionTime;

    [Min(0f)]
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
    Burst = 1,
    Automatic = 2
}