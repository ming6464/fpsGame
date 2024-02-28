using System;
using System.Collections.Generic;
using UnityEngine;

public class VFX_manager : Singleton<VFX_manager>
{
    [Serializable]
    public struct VFXInfo
    {
        public GameObject VFXPrefab;
        public VFXKEY Vfxkey;
    }

    [SerializeField]
    private VFXInfo[] _vfxInfos;

    private List<VFXObject> m_vfxObjects = new();

    private class VFXObject
    {
        private ParticleSystem m_particle;
        private GameObject m_obj;
        private bool m_haveParticle;
        private VFXKEY Vfxkey;
        private bool m_isMuzzleFlash;
        private ParticleSystem[] m_particles;

        [Obsolete("Obsolete")]
        public VFXObject(GameObject obj, VFXKEY key)
        {
            m_obj = obj;
            Vfxkey = key;
            if (key == VFXKEY.MuzzleFlashBig || key == VFXKEY.MuzzleFlashSmall)
            {
                m_isMuzzleFlash = true;
                m_particles = new ParticleSystem[m_obj.transform.GetChildCount()];
                for (int i = 0; i < m_particles.Length; i++)
                {
                    m_obj.transform.GetChild(i).TryGetComponent(out m_particles[i]);
                }
            }
            else
            {
                m_haveParticle = m_obj.TryGetComponent(out ParticleSystem particleSystem);
                if (m_haveParticle)
                {
                    m_particle = particleSystem;
                }
            }
        }

        public bool IsReady(VFXKEY key)
        {
            return m_obj.activeSelf && Vfxkey == key;
        }

        public void SetPosRot(Vector3 pos, Vector3 dir)
        {
            m_obj.transform.position = pos;
            m_obj.transform.forward = dir;
        }

        public void Emit(int count)
        {
            if (!m_haveParticle && !m_isMuzzleFlash)
            {
                return;
            }

            m_obj.SetActive(true);
            if (m_isMuzzleFlash)
            {
                foreach (ParticleSystem particle in m_particles)
                {
                    if (particle == null)
                    {
                        continue;
                    }

                    particle.Emit(count);
                }
            }
            else
            {
                m_particle.Emit(count);
            }
        }

        public void SetLifeTimeByDistance(float distance)
        {
            if (m_haveParticle)
            {
                ParticleSystem.MainModule main = m_particle.main;
                main.startLifetimeMultiplier = distance / main.startSpeedMultiplier;
            }
        }
    }

    public void PlayBullet(Vector3 start, Vector3 end, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        vfxObj.SetPosRot(start, end - start);
        vfxObj.SetLifeTimeByDistance(Vector3.Distance(start, end));
        vfxObj.Emit(1);
    }

    public void PlayEffect(Vector3 position, Vector3 direction, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        vfxObj.SetPosRot(position, direction);
        vfxObj.Emit(1);
    }

    private VFXObject GetVFXObj(VFXKEY key)
    {
        VFXObject vfxObj = m_vfxObjects.Find(x => x.IsReady(key));
        if (vfxObj == null)
        {
            int index = Array.FindIndex(_vfxInfos, x => x.Vfxkey == key);
            if (index < 0)
            {
                return null;
            }

            vfxObj = new VFXObject(Instantiate(_vfxInfos[index].VFXPrefab, transform), _vfxInfos[index].Vfxkey);
            m_vfxObjects.Add(vfxObj);
        }

        return vfxObj;
    }
}

[Serializable]
public enum VFXKEY
{
    MuzzleFlashSmall,
    MuzzleFlashBig,
    Bullet,
    MetalImpact,
    WoodImpact,
    SandImpact,
    StoneImpact,
    None
}