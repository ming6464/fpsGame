using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(GrenadeDamageSender))]
public class Grenade : Weapon
{
    [SerializeField]
    private Transform _main;

    private bool isReleaseTrigger;
    private bool canThrow;
    private bool pinPulled;
    private GrenadeDamageSender grenadeDamageSender;

    protected override void Awake()
    {
        base.Awake();
        if (_animator)
        {
            _animator.enabled = false;
        }

        if (_damageSender && _damageSender.TryGetComponent(out grenadeDamageSender))
        {
            grenadeDamageSender.UpdateExplosionRadius(_curWeaponInfo.ExplosionRadius);
        }
    }

    protected override void OnPullTrigger(object obj)
    {
        base.OnPullTrigger(obj);
        if (pinPulled)
        {
            return;
        }

        pinPulled = true;
        _animator.SetTrigger("Throw");
    }

    protected override void ResetData()
    {
        base.ResetData();
        pinPulled = false;
        isReleaseTrigger = false;
        canThrow = false;
    }

    protected override void OnReleaseTrigger(object obj)
    {
        base.OnReleaseTrigger(obj);
        if (!rigid || !pinPulled)
        {
            return;
        }

        isReleaseTrigger = true;
    }

    private void OnFinishAnimThrow()
    {
        canThrow = true;
    }

    private void Update()
    {
        if (canThrow && isReleaseTrigger && _animator)
        {
            weaponManager.DropWeapon(WeaponType);
            transform.position = _main.position;
            _main.localPosition = Vector3.zero;
            if (_collider.TryGetComponent(out BoxCollider boxCollider))
            {
                boxCollider.center = Vector3.zero;
            }

            pinPulled = true;

            Vector3 dir = Quaternion.Euler(Vector3.left * _curWeaponInfo.ThrowAngle) * transform.forward;

            rigid.AddForce(dir * _curWeaponInfo.ThrowForce, ForceMode.VelocityChange);

            StartCoroutine(DelayDetonation());
        }
    }

    private IEnumerator DelayDetonation()
    {
        yield return new WaitForSeconds(_curWeaponInfo.ExplosionTime);
        if (grenadeDamageSender)
        {
            grenadeDamageSender.Explosive();
        }
    }

    public override void UnUseWeapon()
    {
        base.UnUseWeapon();
        _animator.Play("UnUsedWeapon");
    }

    public override void OnUnUseWeapon()
    {
        base.OnUnUseWeapon();
    }

    public override void PutToBag(WeaponManager wManager, Transform bag)
    {
        base.PutToBag(wManager, bag);
        if (!rigid)
        {
            return;
        }

        rigid.isKinematic = true;
        rigid.useGravity = false;
        if (_collider)
        {
            _collider.enabled = false;
        }

        if (_animator)
        {
            _animator.enabled = true;
        }
    }

    public override void RemoveFromBag()
    {
        base.RemoveFromBag();
        if (!rigid)
        {
            return;
        }

        rigid.isKinematic = false;
        rigid.useGravity = true;
        if (_collider)
        {
            _collider.enabled = true;
        }

        if (_animator)
        {
            _animator.enabled = false;
        }
    }


    protected override void OnTriggerEnter(Collider other)
    {
        if (pinPulled)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (pinPulled)
        {
            return;
        }

        base.OnTriggerExit(other);
    }
}