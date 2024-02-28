using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieDamageReceiver : DamageReceiver
{
    protected override void OnDead()
    {
        base.OnDead();
        if (!TryGetComponent(out Animator _animator))
        {
            return;
        }

        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        _animator.SetTrigger($"Die{Random.Range(1, 3)}");
    }
}