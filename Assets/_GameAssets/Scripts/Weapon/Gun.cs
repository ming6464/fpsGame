using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

[RequireComponent(typeof(DamageSender))]
public class Gun : Weapon
{
    public int TotalBullet
    {
        get => m_weaponHolder.TotalBullet;
        set => m_weaponHolder.TotalBullet -= value;
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

    protected override void Start()
    {
        base.Start();
        m_weaponInfo.FireModeOption = m_weaponInfo.FireModeOption.OrderBy(x => (int)x.FireModeType).ToList();
        CurrrentFireMode = m_weaponInfo.FireModeOption.Last();
    }

    protected override void ResetData()
    {
        base.ResetData();
        m_isReloading = false;
        m_isTrigger = false;
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

    private void HandleFire()
    {
        if (!m_isTrigger || m_isResetFire || !m_isCanFire || m_isReloading || Bullets <= 0)
        {
            return;
        }

        StartCoroutine(PrepareFire());
    }

    private IEnumerator PrepareFire()
    {
        for (int i = 0; i < CurrrentFireMode.FireTimes; i++)
        {
            OnFire();
            if (!GameConfig.Instance || !GameConfig.Instance.UnlimitedBullet)
            {
                Bullets--;
                if (Bullets <= 0)
                {
                    yield break;
                }
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(ResetFire());

        if (CurrrentFireMode.FireModeType != FireModeKEY.Automatic)
        {
            m_isCanFire = false;
        }
    }

    private IEnumerator ResetFire()
    {
        m_isResetFire = true;
        yield return new WaitForSeconds(CurrrentFireMode.TimeResetFire);
        m_isResetFire = false;
    }

    protected virtual void OnFire()
    {
        if (MuzzleFlashs.Length > 0)
        {
            foreach (ParticleSystem particleSystem in MuzzleFlashs)
            {
                particleSystem.Emit(1);
            }
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
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask(m_weaponHolder.AvoidTags)))
            {
                Debug.Log($"fire {hit.transform.name}");
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

        // EventDispatcher.Instance.PostEvent(EventID.OnUpdateNumberBulletWeapon,
        //     new MsgWeapon
        //     {
        //         Bullets = _bullets, TotalBullets = _totalBullets,
        //         WeaponKey = _curWeaponInfo.WeaponType
        //     });
    }

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
        if (m_isReloading)
        {
            return;
        }

        base.ReloadBullet();
        if (TotalBullet <= 0)
        {
            return;
        }

        m_isTrigger = false;
        StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        m_isReloading = true;
        yield return new WaitForSeconds(1f);
        int bullet = m_weaponInfo.MagazineCapacity;
        if (TotalBullet < m_weaponInfo.MagazineCapacity)
        {
            bullet = TotalBullet;
        }

        TotalBullet -= bullet;
        Bullets = bullet;
        m_isReloading = false;
        Debug.Log("Finish Reload");
    }
}