using UnityEngine;

public class SpawnZombieManager : MonoBehaviour
{
    public Transform[] SpawnPoint;

    //
    private int m_indexSpawnPoint;

    // Update is called once per frame

    public void SpawnStage(StageGame stageGame)
    {
        foreach (StageInfo info in stageGame.StageInfos)
        {
            for (int i = 0; i < info.Count; i++)
            {
                Spawn(info.ZombieName);
            }
        }
    }

    private void Spawn(string name)
    {
        if (SpawnPoint.Length == 0 || !GObj_pooling.Instance)
        {
            return;
        }

        GameObject zombie = GObj_pooling.Instance.Pull(GetKeySpawn(name));
        if (zombie == null)
        {
            return;
        }

        m_indexSpawnPoint++;
        if (m_indexSpawnPoint >= SpawnPoint.Length)
        {
            m_indexSpawnPoint = 0;
        }

        zombie.transform.position = SpawnPoint[m_indexSpawnPoint].position;
        zombie.SetActive(true);
    }

    private PoolKEY GetKeySpawn(string name)
    {
        PoolKEY key = PoolKEY.ZombieV1;
        switch (name.ToLower())
        {
            case "zombiev2":
                key = PoolKEY.ZombieV2;
                break;
            case "zombiev3":
                key = PoolKEY.ZombieV3;
                break;
        }

        return key;
    }
}