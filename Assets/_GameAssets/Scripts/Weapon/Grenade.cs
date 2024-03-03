using UnityEngine;

[RequireComponent(typeof(GrenadeDamageSender))]
public class Grenade : Weapon
{
    [Header("Effect Explosion")]
    public ParticleSystem ExplosionEffect;

    [Header("Audio Info")]
    public float Volume;

    public float MaxDistance;

    [Space(10)]
    [SerializeField]
    private MeshRenderer _meshRenderer;

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
        m_rigid.isKinematic = false;
        m_rigid.useGravity = true;
        //_meshRenderer.enabled = true;
    }


    public override void UseWeapon()
    {
        base.UseWeapon();
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
        //_meshRenderer.enabled = false;
        m_rigid.isKinematic = true;
        m_rigid.useGravity = false;
        m_grenadeDamageSender.Explosive();
        ExplosionEffect.Emit(1);
        Invoke(nameof(DelayToPool), m_weaponInfo.ExplosionTimeout);
        if (AudioManager.Instance)
        {
            Transform player = GameObject.FindWithTag("Player").transform;
            float dis = Vector3.Distance(transform.position, player.position);
            if (dis > MaxDistance)
            {
                return;
            }

            AudioManager.Instance.PlaySfx(KeySound.Grenade_M67, Volume * (1f - dis / MaxDistance));
        }
    }

    private void DelayToPool()
    {
        GObj_pooling.Instance.Push(PoolKEY.Grenade, gameObject);
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
        m_weaponHolder.ThrowGrenade();
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