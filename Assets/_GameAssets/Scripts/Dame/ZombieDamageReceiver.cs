using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieDamageReceiver : DamageReceiver
{
    [SerializeField]
    private MinimapPointScript _minimapPoint;

    private Zombie m_zombie;

    private void Awake()
    {
        m_zombie = transform.GetComponent<Zombie>();
    }

    public override void Reduct(float reduct)
    {
        if (_hp > 0 && m_zombie)
        {
            m_zombie.CheckDistanceFromPlayer();
        }

        base.Reduct(reduct);
    }

    protected override void OnDead()
    {
        base.OnDead();
        transform.GetComponent<Collider>().enabled = false;
        EventDispatcher.Instance.PostEvent(EventID.OnKilledZombie, transform.GetComponent<Zombie>().Name);
        transform.GetComponent<Animator>().SetTrigger($"Die{Random.Range(1, 3)}");
        if (_minimapPoint)
        {
            _minimapPoint.OnDead();
        }
    }
}