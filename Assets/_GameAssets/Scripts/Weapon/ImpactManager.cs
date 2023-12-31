﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactManager : Singleton<ImpactManager>
{
    [SerializeField] private GameObject _efxBulletPrefab;

    private List<EfxObject> efxObjects = new();

    private class EfxObject
    {
        private ParticleSystem particle;
        private GameObject obj;
        private bool haveParticle;

        public EfxObject(GameObject obj)
        {
            this.obj = obj;
            haveParticle = obj.TryGetComponent(out ParticleSystem particleSystem);
            if (haveParticle) particle = particleSystem;
        }

        public bool IsPlaying => obj.activeSelf && (!haveParticle || particle.time < particle.main.duration);

        public void SetPosRot(Vector3 pos, Vector3 dir)
        {
            var transform = obj.transform;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        public void Emit(int count)
        {
            obj.SetActive(true);
            if (haveParticle) particle.Emit(count);
        }

        public void SetLifeTime(float lifeTime)
        {
            if (haveParticle)
            {
                var main = particle.main;
                main.startLifetimeMultiplier = lifeTime;
            }
        }

        public void SetLifeTimeByDistance(float distance)
        {
            if (haveParticle)
            {
                var main = particle.main;
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
        var efxObj = efxObjects.Find(x => !x.IsPlaying);
        if (efxObj == null)
        {
            efxObj = new EfxObject(Instantiate(_efxBulletPrefab));
            efxObjects.Add(efxObj);
        }

        efxObj.SetPosRot(pos, dir);
        efxObj.SetLifeTimeByDistance(distance);
        efxObj.Emit(1);
    }

    public void PlayImpactBullet(Vector3 pos, PoolKEY poolKey)
    {
        var obj = GObj_pooling.Instance.Pull(poolKey);
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
        var obj = GObj_pooling.Instance.Pull(poolKey);
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