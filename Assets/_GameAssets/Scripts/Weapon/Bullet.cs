using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("BulletInfo")] [SerializeField]
    protected BulletInfo _bulletInfo;

    protected Rigidbody _rigid;

    protected Coroutine bulletTimeLife;

    protected TrailRenderer trailRenderer;

    private bool isCollision;

    protected virtual void Awake()
    {
    }

    protected void Start()
    {
    }

    public void Play(Vector3 start, Vector3 end)
    {
        isCollision = false;
        if (!_rigid)
            if (!transform.TryGetComponent(out _rigid))
            {
                Debug.LogError($"bullet not has rigid body component!");
                return;
            }

        transform.position = start;
        transform.LookAt(end);
        if (!trailRenderer) transform.TryGetComponent(out trailRenderer);

        if (trailRenderer)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = true;
        }

        gameObject.SetActive(true);
        _rigid.WakeUp();
        _rigid.AddForce((end - start).normalized * _bulletInfo.BulletVelocity, ForceMode.VelocityChange);
        bulletTimeLife = StartCoroutine(SetTimeLifeBullet(_bulletInfo.BulletTimeLife));
    }

    protected virtual IEnumerator SetTimeLifeBullet(float timeLife)
    {
        yield return new WaitForSeconds(timeLife);
        StartCoroutine(GoToPool(0));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isCollision) return;
        isCollision = true;
        var tag = other.gameObject.tag;
        if (tag == "Wall" || tag == "Ground")
        {
            StopCoroutine(bulletTimeLife);
            SfxManager.Instance.PlayImpactBullet(transform.position, transform.rotation.eulerAngles,
                PoolKEY.EffectImpact);
            StartCoroutine(GoToPool(0));
        }
    }

    protected IEnumerator GoToPool(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        if (!trailRenderer) transform.TryGetComponent(out trailRenderer);

        if (trailRenderer) trailRenderer.enabled = false;
        _rigid.velocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
        _rigid.Sleep();
        GObj_pooling.Instance.Push(PoolKEY.Bullet, gameObject);
    }
}