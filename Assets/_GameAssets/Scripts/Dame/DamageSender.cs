using UnityEngine;

public class DamageSender : MonoBehaviour
{
    [SerializeField]
    protected float _damage;

    public virtual void SetDamage(float damage)
    {
        _damage = damage;
    }

    public virtual void Send(Transform objTf)
    {
        if (objTf.TryGetComponent(out DamageReceiver damageReceiver))
        {
            Send(damageReceiver);
        }
    }

    public virtual void Send(DamageReceiver damageReceiver)
    {
        damageReceiver.Reduct(_damage);
    }
}