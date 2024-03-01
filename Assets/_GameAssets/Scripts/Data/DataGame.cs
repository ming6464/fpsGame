using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/DataSO")]
public class DataGame : ScriptableObject
{
    public DataSave DataSave;
    public PlayerInfo[] PlayerInfos;
    public WeaponInfo[] WeaponInfos;
    public ZombieInfo[] ZoombieInfos;
    public StageGame[] StageGames;
}

[Serializable]
public class PlayerInfo
{
    public string Name;
    public GameObject ObjectPrefab;
}

[Serializable]
public class DataSave
{
    public string PlayerName;
    public BagInfo BagInfo;
}

[Serializable]
public class BagInfo
{
    public string[] WeaponNames;
    public int TotalBulletRifle;
    public int TotalBulletShotgun;
    public int TotalBulletPistol;
    public int TotalBullet;
}

[Serializable]
public class StageGame
{
    public StageInfo[] StageInfos;
}

[Serializable]
public struct StageInfo
{
    public string ZombieName;
    public int Count;
}