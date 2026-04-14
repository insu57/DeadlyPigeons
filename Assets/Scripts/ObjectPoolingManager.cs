using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

public enum Pooling
{
    SelectBtn,
    Projectile,
    DamageTxt,
    Explosion
}

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    [Header("TitleSelect")] 
    private PoolingSetting _selectBtnSetting;
    private ObjectPool<SelectButton> _selectBtnPool;
    
    private PoolingSetting _projectileSetting;
    private  ObjectPool<Projectile> _projectilePool;
    
    private PoolingSetting _damageTxtSetting;
    private ObjectPool<DamageTxt> _damageTxtPool;

    private PoolingSetting _explosionSetting;
    private ObjectPool<Hitbox> _explosionPool;
    
    //늘어나는 풀링 오브젝트 -> 개선 방안?
    
    private StringBuilder _sb;

    protected override void Awake()
    {
        base.Awake();

        _sb = new StringBuilder();
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
                case Pooling.DamageTxt: _damageTxtSetting = poolingSetting; break;
                case Pooling.Explosion: _explosionSetting =  poolingSetting; break;
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
    
    public void InitDamageTxtPool()
    {
        if (_damageTxtSetting.Prefab.TryGetComponent(out DamageTxt damageTxt))
        {
            _damageTxtPool = InitPool(damageTxt, _damageTxtSetting);
        }
        else
        {
            Debug.LogError("Can't find damageTxt");
        }
    }

    public DamageTxt GetDamageTxt()
    {
        var dmgTxt = _damageTxtPool.Get();
        dmgTxt.Init(_sb);
        return dmgTxt;
    } 
    public void ReleaseDamageTxt(DamageTxt damageTxt) => _damageTxtPool.Release(damageTxt);

    public void InitExplosivePool()
    {
        if (_explosionSetting.Prefab.TryGetComponent(out Hitbox explosion))
        {
            _explosionPool = InitPool(explosion, _explosionSetting);
        }
        else
        {
            Debug.LogError("Can't find Explosion");
        }
    }

    public Hitbox GetExplosion(int damage, float scale)
    {
        var explosion = _explosionPool.Get();
        explosion.SetScale(scale);
        explosion.AttackInit(damage, false, null);
        return explosion;
    }
}

