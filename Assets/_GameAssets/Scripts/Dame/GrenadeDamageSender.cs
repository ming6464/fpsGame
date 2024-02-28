using UnityEngine;

public class GrenadeDamageSender : DamageSender
{
    [SerializeField]
    private float _radius;

    public void UpdateExplosionRadius(float radius)
    {
        _radius = radius;
    }

    public void Explosive()
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position, _radius))
        {
            Send(col.transform);
        }

        GObj_pooling.Instance.Push(PoolKEY.Grenade, gameObject);
    }
}