using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    [Serializable]
    private class PoolSettings
    {
        public int initSize = 30;
        public int maxSize = 100;
    }
    
    [Header("TitleSelect")]
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private PoolSettings selectBtnSettings;
    private ObjectPool<SelectButton> _selectBtnPool;
    public SelectButton GetSelectBtn() => _selectBtnPool.Get();
    public void ReleaseSelectBtn(SelectButton selectBtn) => _selectBtnPool.Release(selectBtn);
    
    
    //[Header("Main")] -> Bullet

    protected override void Awake()
    {
        base.Awake();
        
        _selectBtnPool = InitPool(selectBtnPrefab, selectBtnSettings);
    }

    //wip
    private ObjectPool<T> InitPool<T>(T prefab, PoolSettings settings) where T : Component
    {
        var pool = new ObjectPool<T>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: component => component.gameObject.SetActive(true),
            actionOnRelease: component => component.gameObject.SetActive(false),
            actionOnDestroy: component => Destroy(component.gameObject),
            collectionCheck: false,
            defaultCapacity: settings.initSize,
            maxSize: settings.maxSize
            );

        var tempList = new List<T>();
        for (var i = 0; i < settings.initSize; i++)
        {
            var obj = pool.Get();
            tempList.Add(obj);
        }

        foreach (var obj in tempList)
        {
            pool.Release(obj);
        }
        
        return pool;
    }
    
}
