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

    [Header("Register")] [SerializeField] private RegisterPool[] _registerPools;
    private Dictionary<PoolKEY, ObjectPool> gameObjectsDic = new();
    private static Transform myTran;

    public override void Awake()
    {
        base.Awake();
        myTran = transform;
        foreach (var objRegis in _registerPools) UpdateObjSpawn(objRegis.PoolKey, objRegis.ObjectRegister);
    }

    public bool UpdateObjSpawn(PoolKEY poolKey, GameObject gObj)
    {
        if (!gObj)
        {
            Debug.LogError($"Game object in UpdateObjSpawn function of key '{poolKey}' : null!");
            return false;
        }

        if (!gameObjectsDic.ContainsKey(poolKey)) gameObjectsDic.Add(poolKey, new ObjectPool());
        gameObjectsDic[poolKey].UpDateGObjSpawn(gObj);
        return true;
    }

    public bool Push(PoolKEY poolKey, GameObject gObj)
    {
        if (!gameObjectsDic.ContainsKey(poolKey))
        {
            Debug.LogError($"Not found key '{poolKey}'");
            return false;
        }

        if (!gObj)
        {
            Debug.LogError($"Game object in Push function of key '{poolKey}' : null!");
            return false;
        }

        gameObjectsDic[poolKey].PushObj(gObj.transform, poolKey);
        return true;
    }

    public GameObject Pull(PoolKEY poolKey)
    {
        if (!gameObjectsDic.ContainsKey(poolKey))
        {
            Debug.LogError($"Not found key '{poolKey}'");
            return null;
        }

        return gameObjectsDic[poolKey].GetObj(poolKey);
    }

    private class ObjectPool
    {
        public List<Transform> gameObjectsTf = new();
        private Transform gObj_SpawnTf;

        public void UpDateGObjSpawn(GameObject gObj)
        {
            if (gObj_SpawnTf) return;
            var obj = Instantiate(gObj, myTran, true);
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

                if (!PushObj(Instantiate(gObj_SpawnTf, myTran, true), poolKey)) return null;
            }

            var gObj = gameObjectsTf[0];
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
            gObjTf.SetParent(myTran);
            gameObjectsTf.Add(gObjTf);
            return true;
        }
    }
}

[Serializable]
public enum PoolKEY
{
    Bullet,
    EffectImpact
}