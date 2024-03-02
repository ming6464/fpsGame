using UnityEngine;

public class ReceiveSupplyBullet : MonoBehaviour
{
    [SerializeField]
    private WeaponHolder _weaponHolder;


    public bool CheckNeedSupply()
    {
        return _weaponHolder.NeedSupplies;
    }

    public void Supply()
    {
        _weaponHolder.SupplyBullet();
    }
}