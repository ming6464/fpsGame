using System;
using UnityEngine;

public class GameConfig : Singleton<GameConfig>
{
    public bool UnlimitedBullet;

    public float TimePerStage;

    public bool DisableInitStage;

    [SerializeField]
    private DataGame _dataGame;

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
        return _dataGame.DataSave.BagInfo;
    }

    public DataSave GetDataSave()
    {
        return _dataGame.DataSave;
    }

    public GameObject GetAppointee()
    {
        string name = SaveManager.CharacterNameSelect.ToLower();
        return Array.Find(_dataGame.PlayerInfos, x => x.Name.ToLower() == name)?.ObjectPrefab;
    }

    public bool CheckFinalStage(int stageIndex)
    {
        return _dataGame.StageGames.Length - stageIndex <= 1;
    }

    public StageGame GetStage(int stageIndex)
    {
        return _dataGame.StageGames[stageIndex];
    }

    public GameObject GetWeaponPrefab(string weaponName)
    {
        return GetWeaponInfo(weaponName).WeaponPrefab;
    }
}