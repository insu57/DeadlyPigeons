using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum Pooling
{
    SelectBtn,
    Projectile,
}

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    //SO에서 받아오는 것으로 수정 필요.
    [Header("TitleSelect")] 
    private PoolingSetting _selectBtnSetting;
    private ObjectPool<SelectButton> _selectBtnPool;
    
    private PoolingSetting _projectileSetting;
    private  ObjectPool<Projectile> _projectilePool;
    

    protected override void Awake()
    {
        base.Awake();
        
        LoadPoolSettings();
    }

    private void LoadPoolSettings()
    {
        PoolingSetting[] settings = Resources.LoadAll<PoolingSetting>("Pooling");

        foreach (var poolingSetting in settings)
        {
            switch (poolingSetting.Pooling)
            {
                case Pooling.SelectBtn: _selectBtnSetting = poolingSetting; break;
                case Pooling.Projectile: _projectileSetting = poolingSetting; break;
            }
        }
    }
    
    //wip
    private ObjectPool<T> InitPool<T>(T prefab, PoolingSetting settings) where T : Component
    {
        var pool = new ObjectPool<T>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: component => component.gameObject.SetActive(true),
            actionOnRelease: component => component.gameObject.SetActive(false),
            actionOnDestroy: component => Destroy(component.gameObject),
            collectionCheck: false,
            defaultCapacity: settings.InitSize,
            maxSize: settings.MaxSize
            );

        var tempList = new List<T>();
        for (var i = 0; i < settings.InitSize; i++)
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
    
    public SelectButton GetSelectBtn()
    {
        var selectBtn = _selectBtnPool.Get();
        selectBtn.ClearSelectBtn();
        return selectBtn;
    }

    public void ReleaseSelectBtn(SelectButton selectBtn) => _selectBtnPool.Release(selectBtn);

    public void InitSelectBtnPool()
    {
        if (_selectBtnSetting.Prefab.TryGetComponent(out SelectButton selectBtn))
        {
            _selectBtnPool = InitPool(selectBtn, _selectBtnSetting);
        }
        else
        {
            Debug.LogError("Can't find selectBtn");
        }
    }
    
    public void InitProjectilePool()
    {
        if (_projectileSetting.Prefab.TryGetComponent(out Projectile projectile))
        {
            _projectilePool = InitPool(projectile, _projectileSetting);
        }
        else
        {
            Debug.LogError("Can't find projectile");
        }
    }

    public Projectile GetProjectile() => _projectilePool.Get();
    public void ReleaseProjectile(Projectile projectile ) => _projectilePool.Release(projectile);
}
