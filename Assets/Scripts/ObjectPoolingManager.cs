using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    [Serializable]
    public class PoolSettings
    {
        public int initSize = 30;
        public int maxSize = 100;
    }
    
    //SO에서 받아오는 것으로 수정 필요.
    [Header("TitleSelect")]
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private PoolSettings selectBtnSettings; //다른 방식으로?
    private ObjectPool<SelectButton> _selectBtnPool;
    
    [SerializeField] private DamageDealer projectilePrefab;
    [SerializeField] private PoolSettings projectileSettings;
    private  ObjectPool<DamageDealer> _projectilePool;
    
    public SelectButton GetSelectBtn()
    {
        var selectBtn = _selectBtnPool.Get();
        selectBtn.ClearSelectBtn();
        return selectBtn;
    }

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

    public void InitProjectilePool()
    {
        _projectilePool = InitPool(projectilePrefab, projectileSettings);
    }
}
