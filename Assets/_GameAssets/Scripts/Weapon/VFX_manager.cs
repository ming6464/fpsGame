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

        [Obsolete("Obsolete")]
        public VFXObject(GameObject obj, VFXKEY key)
        {
            m_obj = obj;
            Vfxkey = key;
            m_haveParticle = m_obj.TryGetComponent(out ParticleSystem particleSystem);
            if (m_haveParticle)
            {
                m_particle = particleSystem;
            }
        }

        public bool IsReady(VFXKEY key)
        {
            return m_obj.activeSelf && Vfxkey == key;
        }

        public void SetPos(Vector3 pos)
        {
            m_obj.transform.position = pos;
        }

        public void SetPosRot(Vector3 pos, Vector3 dir)
        {
            SetPos(pos);
            m_obj.transform.forward = dir;
        }

        public void Emit(int count)
        {
            if (!m_haveParticle)
            {
                return;
            }

            m_obj.SetActive(true);
            m_particle.Emit(count);
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

    [Obsolete("Obsolete")]
    public void PlayBullet(Vector3 start, Vector3 end, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        vfxObj.SetPosRot(start, end - start);
        vfxObj.SetLifeTimeByDistance(Vector3.Distance(start, end));
        vfxObj.Emit(1);
    }

    [Obsolete("Obsolete")]
    public void PlayEffect(Vector3 position, Vector3 direction, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        if (vfxObj == null)
        {
            return;
        }

        vfxObj.SetPosRot(position, direction);
        vfxObj.Emit(1);
    }

    [Obsolete("Obsolete")]
    public void PlayEffect(Vector3 position, VFXKEY key)
    {
        VFXObject vfxObj = GetVFXObj(key);
        if (vfxObj == null)
        {
            return;
        }

        vfxObj.SetPos(position);
        vfxObj.Emit(1);
    }

    [Obsolete("Obsolete")]
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
    Bullet,
    MetalImpact,
    WoodImpact,
    SandImpact,
    StoneImpact,
    None,
    FleshImpact,
    GrenadeImpact,
    BazookaImpact
}