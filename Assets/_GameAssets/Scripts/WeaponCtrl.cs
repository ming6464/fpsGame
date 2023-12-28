using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCtrl : MonoBehaviour
{
    [Header("Base Information")] [SerializeField]
    private Transform _pivotFireTf;

    [Header("State")] [SerializeField] private bool _canFire = true;
    [SerializeField] private float _timeResetFire;
    [Header("Bullet")] [SerializeField] private Transform _bulletPrefab;
    [SerializeField] private float _bulletVelocity = 30;
    [SerializeField] private float _bulletTimeLife = 4;
    [SerializeField] private float _totalNumberBullets = 100;
    [SerializeField] private float _totalNumberBulletsOfMagazine = 30;
    [SerializeField] private float _numberBulletsOfMagazine = 30;

    private void Start()
    {
        GObj_pooling.Instance.UpdateObjSpawn(PoolKEY.Bullet, _bulletPrefab.gameObject);
        _canFire = true;
    }

    public void Fire()
    {
        if (!_canFire) return;
        var curBullet = GObj_pooling.Instance.Pull(PoolKEY.Bullet);
        curBullet.transform.position = _pivotFireTf.position;
        curBullet.transform.localRotation = _pivotFireTf.rotation;
        if (!curBullet.transform.TryGetComponent(out Rigidbody rigidBullet))
        {
            Debug.LogError($"bullet of {transform.name} not has rigid body component!");
            return;
        }

        curBullet.SetActive(true);
        rigidBullet.AddForce(curBullet.transform.forward.normalized * _bulletVelocity, ForceMode.Impulse);
        StartCoroutine(UpdateState());
        StartCoroutine(SetTimeLifeBullet(curBullet, _bulletTimeLife));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator SetTimeLifeBullet(GameObject bullet, float timeLife)
    {
        yield return new WaitForSeconds(timeLife);
        GObj_pooling.Instance.Push(PoolKEY.Bullet, bullet);
    }

    private IEnumerator UpdateState()
    {
        _canFire = false;
        yield return new WaitForSeconds(_timeResetFire);
        _canFire = true;
    }
}