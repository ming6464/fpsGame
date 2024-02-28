using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : Singleton<GameConfig>
{
    public bool UnlimitedBullet;

    [SerializeField]
    private DataGame _dataGame;

    public override void Update()
    {
        base.Update();
    }

    public WeaponInfo GetWeaponInfo(string weaponName)
    {
        return Array.Find(_dataGame.WeaponInfos, x => x.WeaponName == weaponName);
    }

    public ZombieInfo GetZombieInfo(string name)
    {
        return Array.Find(_dataGame.ZoombieInfos, x => x.Name == name);
    }

    public BagInfo GetBagInfo()
    {
        return _dataGame.BagInfo;
    }

    public GameObject GetWeaponPrefab(string weaponName)
    {
        return GetWeaponInfo(weaponName).WeaponPrefab;
    }
}