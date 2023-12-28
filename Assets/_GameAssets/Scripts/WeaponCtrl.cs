using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCtrl : MonoBehaviour
{
    [Header("Base Information")] [SerializeField]
    private Transform _pivotFireTf;

    [SerializeField] private WeaponKEY _weaponType;
    [Header("State")] [SerializeField] private bool _canFire = false;
    [SerializeField] private bool _isFiring;
    [SerializeField] private float _timeResetFire;
    [SerializeField] private bool _isUsing;
    [Header("Bullet")] [SerializeField] private Transform _bulletPrefab;
    [SerializeField] private float _bulletVelocity = 30;
    [SerializeField] private float _bulletTimeLife = 4;
    [SerializeField] private float _totalNumberBullets = 100;
    [SerializeField] private float _totalNumberBulletsOfMagazine = 30;
    [SerializeField] private float _numberBulletsOfMagazine = 30;

    public WeaponKEY WeaponType => _weaponType;

    private void Awake()
    {
        _canFire = false;
    }

    private void Start()
    {
        if (_bulletPrefab) GObj_pooling.Instance.UpdateObjSpawn(PoolKEY.Bullet, _bulletPrefab.gameObject);
    }

    private void Update()
    {
        if (_isUsing && _isFiring) OnFire();
    }

    public void UseWeapon()
    {
        transform.gameObject.SetActive(true);
        EventDispatcher.Instance.RegisterListener(EventID.OnPullTrigger, OnPullTrigger);
        EventDispatcher.Instance.RegisterListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
        _isUsing = true;
        _canFire = true;
    }

    public void UnUseWeapon()
    {
        try
        {
            _isUsing = false;
            _canFire = false;
            _isFiring = false;
            EventDispatcher.Instance.RemoveListener(EventID.OnPullTrigger, OnPullTrigger);
            EventDispatcher.Instance.RemoveListener(EventID.OnReleaseTrigger, OnReleaseTrigger);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void OnDisable()
    {
        UnUseWeapon();
    }

    private void OnPullTrigger(object obj)
    {
        _isFiring = true;
    }

    private void OnReleaseTrigger(object obj)
    {
        _isFiring = false;
    }

    private void OnFire()
    {
        if (!_canFire || !_pivotFireTf) return;
        var curBullet = GObj_pooling.Instance.Pull(PoolKEY.Bullet);
        curBullet.transform.position = _pivotFireTf.position;
        curBullet.transform.localRotation = _pivotFireTf.rotation;
        if (!curBullet.TryGetComponent(out Rigidbody rigidBullet))
        {
            Debug.LogError($"bullet of {transform.name} not has rigid body component!");
            return;
        }

        if (curBullet.TryGetComponent(out TrailRenderer trailRenderer)) trailRenderer.Clear();

        curBullet.SetActive(true);
        rigidBullet.WakeUp();
        rigidBullet.AddForce(curBullet.transform.forward.normalized * _bulletVelocity, ForceMode.Impulse);
        StartCoroutine(UpdateState());
        StartCoroutine(SetTimeLifeBullet(curBullet, _bulletTimeLife));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator SetTimeLifeBullet(GameObject bullet, float timeLife)
    {
        yield return new WaitForSeconds(timeLife);
        if (bullet.TryGetComponent(out Rigidbody rig))
        {
            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
            rig.Sleep();
        }

        GObj_pooling.Instance.Push(PoolKEY.Bullet, bullet);
    }

    private IEnumerator UpdateState()
    {
        _canFire = false;
        yield return new WaitForSeconds(_timeResetFire);
        _canFire = true;
    }
}