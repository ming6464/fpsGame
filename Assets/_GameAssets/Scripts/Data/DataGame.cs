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
    public int TotalBulletRifle = 200;
    public int TotalBulletShotgun = 50;
    public int TotalBulletPistol = 50;
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