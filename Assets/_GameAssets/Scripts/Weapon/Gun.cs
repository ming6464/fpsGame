using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(DamageSender))]
public class Gun : Weapon
{
    public int TotalBullet
    {
        get
        {
            switch (WeaponType)
            {
                case WeaponKEY.Pistol:
                    return m_weaponHolder.TotalBulletPistol;
                case WeaponKEY.Shotgun:
                    return m_weaponHolder.TotalBulletShotgun;
                case WeaponKEY.Rifle:
                    return m_weaponHolder.TotalBulletRifle;
            }

            return 0;
        }
        set
        {
            switch (WeaponType)
            {
                case WeaponKEY.Pistol:
                    m_weaponHolder.TotalBulletPistol = value;
                    break;
                case WeaponKEY.Shotgun:
                    m_weaponHolder.TotalBulletShotgun = value;
                    break;
                case WeaponKEY.Rifle:
                    m_weaponHolder.TotalBulletRifle = value;
                    break;
            }

            EventDispatcher.Instance.PostEvent(EventID.OnchangeTotalBullets, TotalBullet);
        }
    }

    public int Bullets;
    public FireModeInfo CurrrentFireMode;

    [Header("MuzzleFlash")]
    public Transform MuzzleFlashTf;

    public ParticleSystem[] MuzzleFlashs;

    //gun
    private bool m_isTrigger;
    private bool m_isReloading;
    private bool m_isCanFire;
    private bool m_isResetFire;
    private Coroutine m_reloadingCrt;


    protected override void Awake()
    {
        base.Awake();
        Bullets = m_weaponInfo.MagazineCapacity;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_isCanFire = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (m_reloadingCrt != null)
        {
            StopCoroutine(m_reloadingCrt);
            m_reloadingCrt = null;
        }
    }

    protected override void ResetData()
    {
        base.ResetData();
        m_isReloading = false;
        m_isTrigger = false;
        m_isResetFire = false;
        m_isCanFire = true;
    }

    protected override void Start()
    {
        base.Start();
        m_weaponInfo.FireModeOption = m_weaponInfo.FireModeOption.OrderBy(x => (int)x.FireModeType).ToList();
        CurrrentFireMode = m_weaponInfo.FireModeOption.Last();
    }

    public override void UseWeapon()
    {
        base.UseWeapon();
        m_isReloading = false;
        m_isTrigger = false;
        m_isResetFire = false;
        m_isCanFire = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!IsUsing)
        {
            return;
        }

        CheckMagazine();
        HandleFire();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    [Obsolete("Obsolete")]
    private void HandleFire()
    {
        if (!m_isTrigger || m_isResetFire || !m_isCanFire || m_isReloading || Bullets <= 0)
        {
            return;
        }

        if (CurrrentFireMode.FireModeType != FireModeKEY.Automatic)
        {
            m_isCanFire = false;
        }

        m_isResetFire = true;
        Invoke(nameof(ResetFire), CurrrentFireMode.TimeResetFire);
        for (int i = 0; i < CurrrentFireMode.FireTimes; i++)
        {
            OnFire();
            if (!GameConfig.Instance || !GameConfig.Instance.UnlimitedBullet)
            {
                Bullets--;
                EventDispatcher.Instance.PostEvent(EventID.OnChangeBullets, Bullets);
                if (Bullets <= 0)
                {
                    return;
                }
            }
        }
    }

    private void ResetFire()
    {
        m_isResetFire = false;
    }


    [Obsolete("Obsolete")]
    protected virtual void OnFire()
    {
        if (MuzzleFlashs.Length > 0)
        {
            foreach (ParticleSystem particleSystem in MuzzleFlashs)
            {
                particleSystem.Emit(1);
            }
        }

        if (AudioManager.Instance)
        {
            KeySound key = KeySound.Ak47;
            switch (name)
            {
                case "Bennelli_M4":
                    key = KeySound.Bennelli_M4;
                    break;
                case "Glock_17":
                    key = KeySound.Glock_17;
                    break;
                case "KRISS":
                    key = KeySound.KRISS;
                    break;
                case "M9":
                    key = KeySound.M9;
                    break;
                case "SCAR_H":
                    key = KeySound.SCAR_H;
                    break;
                case "SPAS12":
                    key = KeySound.SPAS12;
                    break;
            }

            AudioManager.Instance.PlaySfx(key);
        }


        Transform pivotTf = m_weaponHolder.PivotRay;
        Vector3 startPosRay = pivotTf.position;
        Vector3 endPos;
        Vector3 dirRay = pivotTf.forward;
        float spreadBullet = CurrrentFireMode.Spread;
        for (int i = 0; i < m_weaponInfo.BulletsPerShot; i++)
        {
            if (i > 0)
            {
                dirRay = Quaternion.Euler(Random.Range(-spreadBullet, spreadBullet),
                    Random.Range(-spreadBullet, spreadBullet), 0f) * pivotTf.forward;
            }

            Ray ray = new(startPosRay, dirRay);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, ~LayerMask.GetMask(m_weaponHolder.AvoidLayers)))
            {
                endPos = hit.point;
                m_damageSender.Send(hit.transform);
                if (VFX_manager.Instance)
                {
                    VFXKEY key = VFXKEY.None;
                    switch (hit.transform.tag.ToLower())
                    {
                        case "metal":
                            key = VFXKEY.MetalImpact;
                            break;
                        case "stone":
                            key = VFXKEY.StoneImpact;
                            break;
                        case "wood":
                            key = VFXKEY.WoodImpact;
                            break;
                        case "sand":
                            key = VFXKEY.SandImpact;
                            break;
                        case "enemy":
                            key = VFXKEY.FleshImpact;
                            break;
                    }

                    VFX_manager.Instance.PlayEffect(endPos, hit.normal, key);
                }
            }
            else
            {
                endPos = startPosRay + dirRay * 100;
            }

            VFX_manager.Instance.PlayBullet(MuzzleFlashTf.position, endPos, VFXKEY.Bullet);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private bool CheckMagazine()
    {
        if (GameConfig.Instance && GameConfig.Instance.UnlimitedBullet)
        {
            return true;
        }

        if (Bullets > 0 && !m_isReloading)
        {
            return true;
        }

        ReloadBullet();
        return false;
    }

    protected override void OnChangeFireMode()
    {
        base.OnChangeFireMode();
        int nextIndex = m_weaponInfo.FireModeOption.IndexOf(CurrrentFireMode) + 1;
        if (nextIndex >= m_weaponInfo.FireModeOption.Count)
        {
            nextIndex = 0;
        }

        CurrrentFireMode = m_weaponInfo.FireModeOption[nextIndex];
    }

    protected override void OnPullTrigger()
    {
        base.OnPullTrigger();
        m_isTrigger = true;
    }

    protected override void OnReleaseTrigger()
    {
        base.OnReleaseTrigger();
        m_isTrigger = false;
        m_isCanFire = true;
    }

    protected override void ReloadBullet()
    {
        if (TotalBullet <= 0 || m_isReloading || Bullets == m_weaponInfo.MagazineCapacity)
        {
            return;
        }

        base.ReloadBullet();
        m_isTrigger = false;
        m_isReloading = true;
        EventDispatcher.Instance.PostEvent(EventID.OnReloadBullet);
    }

    private void OnFinishReload(object obj)
    {
        if (Bullets < 0)
        {
            Bullets = 0;
        }

        int bullet = m_weaponInfo.MagazineCapacity - Bullets;
        if (TotalBullet < bullet)
        {
            bullet = TotalBullet;
        }

        TotalBullet -= bullet;
        Bullets += bullet;
        m_isReloading = false;
        EventDispatcher.Instance.PostEvent(EventID.OnChangeBullets, Bullets);
    }

    protected override void LinkEvent()
    {
        base.LinkEvent();
        EventDispatcher.Instance.RegisterListener(EventID.OnFinishReload, OnFinishReload);
    }

    protected override void UnLinkEvent()
    {
        base.UnLinkEvent();
        EventDispatcher.Instance.RemoveListener(EventID.OnFinishReload, OnFinishReload);
    }

    public void Supply()
    {
        Bullets = m_weaponInfo.MagazineCapacity;
        if (!IsUsing)
        {
            return;
        }

        EventDispatcher.Instance.PostEvent(EventID.OnChangeWeapon, new MsgWeapon
        {
            WeaponKey = WeaponType, WeaponIcon = WeaponIcon, Bullets = Bullets
        });
        EventDispatcher.Instance.PostEvent(EventID.OnchangeTotalBullets, TotalBullet);
    }
}