using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(GrenadeDamageSender))]
public class Grenade : Weapon
{
    private bool m_pinPulled;
    private GrenadeDamageSender m_grenadeDamageSender;

    protected override void Awake()
    {
        base.Awake();
        m_grenadeDamageSender = (GrenadeDamageSender)m_damageSender;
        m_grenadeDamageSender.UpdateExplosionRadius(m_weaponInfo.ExplosionRadius);
    }

    protected override void OnPullTrigger()
    {
        base.OnPullTrigger();
        m_pinPulled = true;
    }

    protected override void ResetData()
    {
        base.ResetData();
        m_pinPulled = false;
    }

    public override void UnUseWeapon()
    {
        base.UnUseWeapon();
        m_pinPulled = false;
    }

    protected override void OnReleaseTrigger()
    {
        base.OnReleaseTrigger();
        if (!m_pinPulled)
        {
            return;
        }

        OnThrow();
    }


    private void DelayDetonation()
    {
        m_grenadeDamageSender.Explosive();
    }


    protected override void OnTriggerEnter(Collider other)
    {
        if (m_pinPulled)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }

    private void OnThrow()
    {
        Debug.Log("On Throw");
        m_weaponHolder = null;
        m_slot = null;
        if (TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        m_rigid.isKinematic = false;
        m_rigid.useGravity = true;
        transform.parent = null;
        gameObject.layer = 8;
        UnUseWeapon();
        m_rigid.AddForce(transform.forward * 50f, ForceMode.Impulse);
        m_pinPulled = true;
        Invoke(nameof(DelayDetonation), m_weaponInfo.ExplosionTime);
    }
}