using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

public enum Pooling
{
    SelectBtn,
    Projectile,
    DamageTxt,
    Explosion,
    Enemy,
    Collectable,
}

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    private Dictionary<Pooling, PoolingSetting> _settings = new();
    private Dictionary<Pooling, IObjectPool> _pools = new();
    private Dictionary<Pooling, Transform> _containers = new();
    public StringBuilder Sb { get; private set; }
    protected override void Awake()
    {
        base.Awake();

        Sb = new StringBuilder();
        LoadPoolSettings();
    }

    private void LoadPoolSettings()
    {
        PoolingSetting[] settings = Resources.LoadAll<PoolingSetting>("Pooling");

        foreach (var setting in settings)
        {
            _settings[setting.Pooling] = setting;
        }
    }

    private void InitPool<T>(Pooling type) where T : Component
    {
        // 살아있는 풀이 이미 있으면 재사용.
        // 씬 전환으로 컨테이너가 파괴됐으면(stale) 잔여 항목 정리 후 재생성.
        if (_pools.ContainsKey(type))
        {
            if (_containers.TryGetValue(type, out var existing) && existing) return;

            _pools.Remove(type);
            _containers.Remove(type);
        }

        if (!_settings.TryGetValue(type, out var setting))
        {
            Debug.LogError($"No PoolingSetting found for {type}");
            return;
        }

        if (!setting.Prefab.TryGetComponent<T>(out var prefab))
        {
            Debug.LogError($"Can't find {typeof(T).Name} component on {setting.name}");
            return;
        }

        var container = new GameObject($"{type} Pool").transform;
        _containers[type] = container;

        var pool = new ObjectPool<T>(
            createFunc: () => Instantiate(prefab, container),
            actionOnGet: c => c.gameObject.SetActive(true),
            actionOnRelease: c => c.gameObject.SetActive(false),
            actionOnDestroy: c => Destroy(c.gameObject),
            collectionCheck: false,
            defaultCapacity: setting.InitSize,
            maxSize: setting.MaxSize
        );

        var temp = new List<T>(setting.InitSize);
        for (var i = 0; i < setting.InitSize; i++)
            temp.Add(pool.Get());
        foreach (var obj in temp)
            pool.Release(obj);

        _pools[type] = new TypedPool<T>(pool);
    }

    private T GetFromPool<T>(Pooling type) where T : Component
    {
        if (_pools.TryGetValue(type, out var pool) && pool is TypedPool<T> typed)
            return typed.Get();

        Debug.LogError($"Pool {type} is not initialized or type mismatch");
        return null;
    }

    private void ReleaseToPool(Pooling type, Component obj) => _pools[type].Release(obj);

    // ── SelectBtn ──────────────────────────────────────────────
    public void InitSelectBtnPool() => InitPool<SelectButton>(Pooling.SelectBtn);

    public SelectButton GetSelectBtn()
    {
        var btn = GetFromPool<SelectButton>(Pooling.SelectBtn);
        btn.ClearSelectBtn();
        return btn;
    }

    public void ReleaseSelectBtn(SelectButton btn)
    {
        ReleaseToPool(Pooling.SelectBtn, btn);
        btn.transform.SetParent(_containers[Pooling.SelectBtn], false);
    } 

    // ── Projectile ─────────────────────────────────────────────
    private readonly HashSet<Projectile> _activeProjectiles = new();
    public void InitProjectilePool() => InitPool<Projectile>(Pooling.Projectile);

    public Projectile GetProjectile()
    {
        var projectile = GetFromPool<Projectile>(Pooling.Projectile);
        _activeProjectiles.Add(projectile);
        return projectile;
    }

    public void ReleaseProjectile(Projectile projectile)
    {
        _activeProjectiles.Remove(projectile);
        ReleaseToPool(Pooling.Projectile, projectile);
    }

    public void ReleaseAllProjectiles()
    {
        foreach (var projectile in _activeProjectiles)
        {
            ReleaseToPool(Pooling.Projectile, projectile);
        }
        _activeProjectiles.Clear();
    }

    // ── DamageTxt ──────────────────────────────────────────────
    public void InitDamageTxtPool() => InitPool<DamageTxt>(Pooling.DamageTxt);
    public DamageTxt GetDamageTxt() => GetFromPool<DamageTxt>(Pooling.DamageTxt);
    public void ReleaseDamageTxt(DamageTxt damageTxt) => ReleaseToPool(Pooling.DamageTxt, damageTxt);

    // ── Explosion ──────────────────────────────────────────────
    public void InitExplosivePool() => InitPool<Hitbox>(Pooling.Explosion);
    public Hitbox GetExplosion() => GetFromPool<Hitbox>(Pooling.Explosion);
    public void ReleaseExplosion(Hitbox explosion) => ReleaseToPool(Pooling.Explosion, explosion);

    // ── Enemy(Base) ────────────────────────────────────────────── //진행 중
    public void InitEnemyPool() => InitPool<EnemyManager>(Pooling.Enemy);
    public EnemyManager GetEnemyBase() => GetFromPool<EnemyManager>(Pooling.Enemy);
    public void ReleaseEnemy(EnemyManager enemy) => ReleaseToPool(Pooling.Enemy, enemy);
    
    // ── Collectable ────────────────────────────────────────────── //진행 중
    public void InitCollectablePool() => InitPool<Collectable>(Pooling.Collectable);
    public Collectable GetCollectable() => GetFromPool<Collectable>(Pooling.Collectable);
    public void ReleaseCollectable(Collectable collectable) => ReleaseToPool(Pooling.Collectable, collectable);
    
    // ── 내부 풀 래퍼 ───────────────────────────────────────────

    private interface IObjectPool
    {
        void Release(Component obj);
    }

    private class TypedPool<T> : IObjectPool where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public TypedPool(ObjectPool<T> pool) => _pool = pool;

        public T Get() => _pool.Get();
        public void Release(Component obj) => _pool.Release((T)obj);
    }
}
