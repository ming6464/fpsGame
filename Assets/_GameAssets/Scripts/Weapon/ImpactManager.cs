using System;
using System.Collections;
using UnityEngine;

public class ImpactManager : Singleton<ImpactManager>
{
    public override void Awake()
    {
        base.Awake();
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