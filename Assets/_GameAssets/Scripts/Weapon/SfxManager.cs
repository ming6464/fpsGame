using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : Singleton<SfxManager>
{
    [SerializeField]
    private GameObject _efxBulletPrefab;

    private List<EfxObject> m_efxObjects = new();

    private class EfxObject
    {
        private ParticleSystem particle;
        private GameObject obj;
        private bool haveParticle;

        public EfxObject(GameObject obj)
        {
            this.obj = obj;
            haveParticle = obj.TryGetComponent(out ParticleSystem particleSystem);
            if (haveParticle)
            {
                particle = particleSystem;
            }
        }

        public bool IsPlaying => obj.activeSelf && (!haveParticle || particle.time < particle.main.duration);

        public void SetPosRot(Vector3 pos, Vector3 dir)
        {
            Transform transform = obj.transform;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        public void Emit(int count)
        {
            obj.SetActive(true);
            if (haveParticle)
            {
                particle.Emit(count);
            }
        }

        public void SetLifeTime(float lifeTime)
        {
            if (haveParticle)
            {
                ParticleSystem.MainModule main = particle.main;
                main.startLifetimeMultiplier = lifeTime;
            }
        }

        public void SetLifeTimeByDistance(float distance)
        {
            if (haveParticle)
            {
                ParticleSystem.MainModule main = particle.main;
                main.startLifetimeMultiplier = distance / main.startSpeedMultiplier;
            }
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public void PlayBullet(Vector3 pos, Vector3 dir, float distance)
    {
        EfxObject efxObj = m_efxObjects.Find(x => !x.IsPlaying);
        if (efxObj == null)
        {
            efxObj = new EfxObject(Instantiate(_efxBulletPrefab));
            m_efxObjects.Add(efxObj);
        }

        efxObj.SetPosRot(pos, dir);
        efxObj.SetLifeTimeByDistance(distance);
        efxObj.Emit(1);
    }

    public void PlayBullet(Vector3 start, Vector3 end)
    {
        EfxObject efxObj = m_efxObjects.Find(x => !x.IsPlaying);
        if (efxObj == null)
        {
            efxObj = new EfxObject(Instantiate(_efxBulletPrefab));
            m_efxObjects.Add(efxObj);
        }

        efxObj.SetPosRot(start, end - start);
        efxObj.SetLifeTimeByDistance(Vector3.Distance(start, end));
        efxObj.Emit(1);
    }

    public void PlayImpactBullet(Vector3 pos, PoolKEY poolKey)
    {
        GameObject obj = GObj_pooling.Instance.Pull(poolKey);
        obj.transform.SetParent(null, true);
        obj.transform.position = pos;
        obj.gameObject.SetActive(true);
        if (obj.transform.TryGetComponent(out ParticleSystem particleSystem))
        {
            particleSystem.Stop();
            particleSystem.Play();
            StartCoroutine(DelayPushToPool(particleSystem.main.duration, obj, poolKey));
        }
    }

    public void PlayImpactBullet(Vector3 pos, Vector3 rotate, PoolKEY poolKey)
    {
        GameObject obj = GObj_pooling.Instance.Pull(poolKey);
        obj.transform.SetParent(null, true);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.Euler(rotate);
        obj.gameObject.SetActive(true);
        if (obj.transform.TryGetComponent(out ParticleSystem particleSystem))
        {
            particleSystem.Stop();
            particleSystem.Play();
            StartCoroutine(DelayPushToPool(particleSystem.main.duration, obj, poolKey));
        }
    }

    private IEnumerator DelayPushToPool(float time, GameObject obj, PoolKEY poolKey)
    {
        yield return new WaitForSeconds(time);
        GObj_pooling.Instance.Push(poolKey, obj);
    }
}