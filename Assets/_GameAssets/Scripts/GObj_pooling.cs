using System;
using System.Collections.Generic;
using UnityEngine;

public class GObj_pooling : Singleton<GObj_pooling>
{
    [Serializable]
    public struct RegisterPool
    {
        public PoolKEY PoolKey;
        public GameObject ObjectRegister;
    }

    [Header("Register")]
    [SerializeField]
    private RegisterPool[] _registerPools;

    private Dictionary<PoolKEY, ObjectPool> _gameObjectsDic = new();
    private static Transform _myTran;

    public override void Awake()
    {
        base.Awake();
        _myTran = transform;
        foreach (RegisterPool objRegis in _registerPools)
        {
            UpdateObjSpawn(objRegis.PoolKey, objRegis.ObjectRegister);
        }
    }

    public bool UpdateObjSpawn(PoolKEY poolKey, GameObject gObj)
    {
        if (!gObj)
        {
            Debug.LogError($"Game object in UpdateObjSpawn function of key '{poolKey}' : null!");
            return false;
        }

        if (!_gameObjectsDic.ContainsKey(poolKey))
        {
            _gameObjectsDic.Add(poolKey, new ObjectPool());
        }

        _gameObjectsDic[poolKey].UpDateGObjSpawn(gObj);
        return true;
    }

    public bool UpdateObjSpawn(PoolKEY poolKey, Transform tf)
    {
        if (!tf)
        {
            Debug.LogError($"Game object in UpdateObjSpawn function of key '{poolKey}' : null!");
            return false;
        }

        return UpdateObjSpawn(poolKey, tf.gameObject);
    }

    public bool Push(PoolKEY poolKey, GameObject gObj)
    {
        if (!gObj)
        {
            Debug.LogError($"Game object in Push function of key '{poolKey}' : null!");
            return false;
        }

        if (!_gameObjectsDic.ContainsKey(poolKey))
        {
            Debug.LogError($"Not found key '{poolKey}'");
            return false;
        }

        _gameObjectsDic[poolKey].PushObj(gObj.transform, poolKey);
        return true;
    }

    public bool Push(PoolKEY poolKey, Transform tf)
    {
        if (!tf)
        {
            Debug.LogError($"Game object in Push function of key '{poolKey}' : null!");
            return false;
        }

        return Push(poolKey, tf.gameObject);
    }

    public GameObject Pull(PoolKEY poolKey)
    {
        if (!_gameObjectsDic.ContainsKey(poolKey))
        {
            Debug.LogError($"Not found key '{poolKey}'");
            return null;
        }

        return _gameObjectsDic[poolKey].GetObj(poolKey);
    }

    private class ObjectPool
    {
        public List<Transform> gameObjectsTf = new();
        private Transform gObj_SpawnTf;

        public void UpDateGObjSpawn(GameObject gObj)
        {
            if (gObj_SpawnTf)
            {
                return;
            }

            GameObject obj = Instantiate(gObj, _myTran, true);
            obj.SetActive(false);
            gObj_SpawnTf = obj.transform;
        }

        public GameObject GetObj(PoolKEY poolKey)
        {
            if (gameObjectsTf.Count == 0)
            {
                if (!gObj_SpawnTf)
                {
                    Debug.LogError($"Game object Spawn of key '{poolKey}': null!");
                    return null;
                }

                if (!PushObj(Instantiate(gObj_SpawnTf, _myTran, true), poolKey))
                {
                    return null;
                }
            }

            Transform gObj = gameObjectsTf[0];
            gObj.SetParent(null, true);
            gameObjectsTf.RemoveAt(0);
            return gObj.gameObject;
        }

        public bool PushObj(Transform gObjTf, PoolKEY poolKey)
        {
            if (!gObjTf)
            {
                Debug.LogError($"Game object in PushObj function of key '{poolKey}' : null!");
                return false;
            }

            gObjTf.gameObject.SetActive(false);
            gObjTf.SetParent(_myTran);
            gameObjectsTf.Add(gObjTf);
            return true;
        }
    }
}

[Serializable]
public enum PoolKEY
{
    Bullet,
    EffectImpact,
    Grenade,
    Zombie
}