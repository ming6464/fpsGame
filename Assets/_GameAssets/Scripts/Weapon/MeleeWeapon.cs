using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    protected override void OnPullTrigger(object obj)
    {
        base.OnPullTrigger(obj);
        if (!_animator)
        {
            return;
        }

        _animator.SetBool("Attack", true);
    }

    protected override void OnReleaseTrigger(object obj)
    {
        base.OnReleaseTrigger(obj);
        if (!_animator)
        {
            return;
        }

        _animator.SetBool("Attack", false);
    }

    public override void PutToBag(WeaponManager wManager, Transform bag)
    {
        base.PutToBag(wManager, bag);
        _collider.enabled = false;
        if (!rigid)
        {
            return;
        }

        rigid.useGravity = false;
        rigid.constraints = RigidbodyConstraints.FreezeAll;
    }

    public override void RemoveFromBag()
    {
        base.RemoveFromBag();
        _collider.enabled = true;
        if (!rigid)
        {
            return;
        }

        rigid.useGravity = true;
        rigid.constraints = RigidbodyConstraints.None;
    }

    public override void UnUseWeapon()
    {
        base.UnUseWeapon();
        _animator.Play("UnUsedWeapon");
    }
}