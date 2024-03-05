using System;
using UnityEngine;

public class GameConfig : Singleton<GameConfig>
{
    public bool UnlimitedBullet;

    public float TimePerStage;

    public bool DisableInitStage;

    [SerializeField]
    private DataGame _dataGame;

    private DataGameSave m_dataGame;

    public override void Awake()
    {
        base.Awake();
        if (SaveManager.FirstOpen)
        {
            SaveManager.FirstOpen = false;
            SaveManager.DataGame = JsonHelper.ToJson(_dataGame);
        }

        m_dataGame = JsonHelper.FromJson<DataGameSave>(SaveManager.DataGame);
    }

    public WeaponInfo GetWeaponInfo(string weaponName)
    {
        return Array.Find(m_dataGame.WeaponInfos, x => x.WeaponName == weaponName);
    }

    public ZombieInfo GetZombieInfo(string name)
    {
        return Array.Find(m_dataGame.ZoombieInfos, x => x.Name == name);
    }

    public BagInfo GetBagInfo()
    {
        return m_dataGame.DataSave.BagInfo;
    }

    public DataSave GetDataSave()
    {
        return m_dataGame.DataSave;
    }

    public GameObject GetAppointee()
    {
        string name = SaveManager.CharacterNameSelect.ToLower();
        return Array.Find(m_dataGame.PlayerInfos, x => x.Name.ToLower() == name)?.ObjectPrefab;
    }

    public bool CheckFinalStage(int stageIndex)
    {
        return m_dataGame.StageGames.Length - stageIndex <= 1;
    }

    public StageGame GetStage(int stageIndex)
    {
        return GetAllStage()[stageIndex];
    }

    public StageGame[] GetAllStage()
    {
        return m_dataGame.StageGames;
    }

    public GameObject GetWeaponPrefab(string weaponName)
    {
        return GetWeaponInfo(weaponName).WeaponPrefab;
    }

    public void UpdateData(int numberRifle, int numberShotgun, int numberPistol, StageGame[] stageGames)
    {
        m_dataGame.DataSave.BagInfo.TotalBulletRifle = numberRifle;
        m_dataGame.DataSave.BagInfo.TotalBulletShotgun = numberShotgun;
        m_dataGame.DataSave.BagInfo.TotalBulletPistol = numberPistol;

        if (stageGames != null)
        {
            m_dataGame.StageGames = stageGames;
        }

        SaveManager.DataGame = JsonHelper.ToJson(m_dataGame);
    }
}