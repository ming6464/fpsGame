using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieDamageReceiver : DamageReceiver
{
    [SerializeField]
    private MinimapPointScript _minimapPoint;

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