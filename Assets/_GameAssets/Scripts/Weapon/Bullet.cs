using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("BulletInfo")] [SerializeField]
    protected BulletInfo _bulletInfo;

    protected Rigidbody _rigid;

    protected Coroutine bulletTimeLife;

    protected virtual void Awake()
    {
    }

    public void Play(Vector3 start, Vector3 end)
    {
        if (!_rigid)
            if (!transform.TryGetComponent(out _rigid))
            {
                Debug.LogError($"bullet not has rigid body component!");
                return;
            }

        transform.position = start;
        transform.LookAt(end);
        if (transform.TryGetComponent(out TrailRenderer trailRenderer)) trailRenderer.Clear();
        gameObject.SetActive(true);
        _rigid.WakeUp();
        _rigid.AddForce((end - start).normalized * _bulletInfo.BulletVelocity, ForceMode.VelocityChange);
        bulletTimeLife = StartCoroutine(SetTimeLifeBullet(_bulletInfo.BulletTimeLife));
    }

    protected virtual IEnumerator SetTimeLifeBullet(float timeLife)
    {
        yield return new WaitForSeconds(timeLife);
        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
        _rigid.Sleep();
        GObj_pooling.Instance.Push(PoolKEY.Bullet, gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        return;
        var tag = other.gameObject.tag;
        if (tag == "Wall" || tag == "Ground")
        {
            StopCoroutine(bulletTimeLife);
            _rigid.velocity = Vector3.zero;
            _rigid.angularVelocity = Vector3.zero;
            _rigid.Sleep();
            GObj_pooling.Instance.Push(PoolKEY.Bullet, gameObject);
        }
    }
}