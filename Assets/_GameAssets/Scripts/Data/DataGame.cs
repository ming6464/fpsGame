using UnityEngine;

[CreateAssetMenu(menuName = "Data/DataSO")]
public class DataGame : ScriptableObject
{
    public WeaponInfo[] WeaponInfos;
    public BagInfo BagInfo;
    public ZombieInfo[] ZoombieInfos;
}