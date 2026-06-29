using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public struct TargetInfo
{
    public Transform Target;
    public float SqrDistance;
    public bool IsValid => Target;
    public  TargetInfo(Transform target, float sqrDistance)
    {
        Target = target;
        SqrDistance = sqrDistance;
    }
}

public class StageManager : MonoBehaviour
{
    private PlayerManager _playerManager;
    [SerializeField] private TMP_Text stageText;
    private PlayerSelected _playerSelected;

    private HashSet<EnemyManager> activeEnemies = new();
    private const float MaxFindRange = 100f;

    [Header("Enemy Spawn")]
    [SerializeField] private EnemyManager enemyBasePrefab;

    [SerializeField] private Collider2D mapCollider;
    [SerializeField] private float nearSpawnMin = 3f;
    [SerializeField] private float nearSpawnMax = 6f;
    [SerializeField] private float farSpawnMin = 10f;
    [SerializeField] private float farSpawnMax = 15f;
    private const int MaxSpawnRetries = 10;
    private const int MaxSpawnCount = 100;
    private const float FirstSpawnDelay = 0.5f;

    [Header("Wave")] 
    [SerializeField] private int waveLevelUp = 0;
    [SerializeField] private int cratePickup = 0;
    private StageUI _stageUI;

    private Dictionary<CollectableType, HashSet<Collectable>> _collectables = new();
    
    private (MainStats stat, int tier)[] _currentUpgradeOptions;
    
    private const int UpgradeOptionCount = 4;
    private int _upgradeRerollPrice;
    private int _upgradeRerollIncrease;
    private int _upgradeRerollCount;
    
    private ItemData _currentCrateItem;
    private int _currentCrateItemPrice;

    private const int StoreOptionCount = 4;
    private int _storeRerollPrice;
    private int _storeRerollIncrease;
    private int _storeRerollCount;
    private StoreData _storeData;
    
    //test
    [SerializeField] private CharacterData testChar;
    [SerializeField] private List<WeaponData> testWeapon;
    [SerializeField] private int testStage;
    [SerializeField] private int testWave;

    private int _currentWave = 1; //1~20...
    private const int LastWave = 20;
    //21~ -> InfiniteMode

    private void Awake()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
        _stageUI = FindFirstObjectByType<StageUI>();
    }

    private void Start()
    {
        InitStage();

        //테스트용.
        if (_playerSelected == null)
        {
            _playerSelected = new PlayerSelected
            {
                CharID = testChar.ID,
                WeaponIDList = new List<int>(),
                ItemIDList = new List<int>(),
                StageID = testStage
            };

            foreach (var weaponData in testWeapon)
            {
                _playerSelected.WeaponIDList.Add(weaponData.ID);
            }

            _playerSelected.ItemIDList.Add(testChar.ID);
        }

        //플레이어 초기화
        InitPlayer();

        //Pooling Initialize
        ObjectPoolingManager.Instance.InitProjectilePool();
        ObjectPoolingManager.Instance.InitDamageTxtPool();
        ObjectPoolingManager.Instance.InitExplosivePool();
        ObjectPoolingManager.Instance.InitEnemyPool();
        ObjectPoolingManager.Instance.InitCollectablePool();

        //StageUI
        InitStageUi();

        _storeData = new StoreData
        {
            Money = 0, RerollPrice = 0
        };
        var items = new List<(ItemData itemData, int price, int idx)>();
        var weapons = new List<(CurrentWeaponStat weaponStat, int price, int idx)>();
        var isSold = new List<bool>();
        for (int i = 0; i < StoreOptionCount; i++)
        {
            isSold.Add(false);
        }
        _storeData.Items = items;
        _storeData.Weapons = weapons;
        _storeData.IsSold = isSold;

        _collectables[CollectableType.Material] = new HashSet<Collectable>();
        _collectables[CollectableType.Crate] = new HashSet<Collectable>();
        _collectables[CollectableType.Meat] =  new HashSet<Collectable>();
        
        //웨이브 시작
        StartWave();
    }

    private void Update()
    {
        FindClosestEnemy();
    }

    private void InitStage()
    {
        _playerSelected = SceneChanger.Instance.PlayerSelected;
        InputManager.Instance.Input.Player.Enable();
        InputManager.Instance.Input.UI.Disable();
        InputManager.Instance.Input.Global.Enable();

        //상점 UI
        _stageUI.Init(_playerManager, UpgradeOptionCount, StoreOptionCount);
    }

    private void InitPlayer() //플레이어 초기화.
    {
        var charData = DataManager.Instance.CharDict[_playerSelected.CharID];
        List<WeaponData> weapons = new();
        foreach (var weaponID in _playerSelected.WeaponIDList)
        {
            weapons.Add(DataManager.Instance.WeaponDict[weaponID]);
        }

        List<ItemData> items = new();
        foreach (var itemID in _playerSelected.ItemIDList)
        {
            items.Add(DataManager.Instance.ItemDict[itemID]);
        }

        _playerManager.InitCharacter(charData, items, weapons);

        _playerManager.OnUpdateLevel += _ => OnLevelUp();
        _playerManager.OnCratePickup += OnCratePickup;
        _playerManager.OnUpdateMoney += money => _stageUI.UpdateMoney(money);
        _playerManager.OnPlayerDeath += () => OnStageEnd(false);
    }

    private void InitStageUi()
    {
        _stageUI.OnSelectStatUpgrade += HandleOnSelectStatUpgrade;
        _stageUI.OnRerollUpgrade += HandleOnRerollUpgrade;
        _stageUI.OnUseCrateItem += HandleOnUseCrateItem;
        _stageUI.OnSellCrateItem += HandleOnSellCrateItem;
        _stageUI.OnStorePanelShowClassInfo += HandleOnStorePanelShowClassInfo;
        _stageUI.OnRerollStore += HandleOnRerollStore;
        _stageUI.OnBuyStore += HandleOnBuyStore;
        _stageUI.OnNextWave += HandleOnNextWave;
        _stageUI.OnCombineWeapon += HandleOnCombineWeapon;
        _stageUI.OnRecycleWeapon += HandleOnRecycleWeapon;
    }

    private void StartWave()
    {
        _playerManager.transform.position = Vector3.zero;
        var waveData = DataManager.Instance.WaveDataList[_currentWave - 1];
        _stageUI.SetCurrentWaveText(_currentWave);
        StartCoroutine(WaveCoroutine(waveData));
    }
    
    private void OnLevelUp()
    {
        waveLevelUp++;
        _stageUI.UpdateLvUpCount(waveLevelUp);
    }

    private void OnCratePickup()
    {
        cratePickup++;
        _stageUI.UpdateCrateCount(cratePickup);
    }

    private IEnumerator WaveCoroutine(WaveData waveData)
    {
        _playerManager.WavePlayerInit(); //웨이브 시작시 플레이어 초기화.
        
        AudioManager.Instance.PlayBGM(BgmType.Stage);

        SpawnBosses(waveData);  //웨이브 시작시 보스 전부 스폰
        float elapsed = 0f;
        float spawnTimer = 0f;
        float spawnInterval = FirstSpawnDelay;
        int totalSpawned = 0;

        while (elapsed < waveData.WaveLength)
        {
            yield return null;

            elapsed += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            //남은 시간 매 프레임 갱신
            var leftTime = Mathf.Max(0f, waveData.WaveLength - elapsed);
            _stageUI.UpdateWaveTimer(leftTime);

            //스폰 틱마다 적 스폰
            if (spawnTimer < spawnInterval) continue;
            spawnTimer -= spawnInterval;
            spawnInterval = waveData.SpawnTick;

            if (totalSpawned > MaxSpawnCount) continue;

            int toSpawn = waveData.SpawnPerTick;
            for (int i = 0; i < toSpawn; i++)
            {
                if (waveData.Enemies == null || waveData.Enemies.Count == 0) break;
                var spawnInfo = PickWeightedEnemy(waveData.Enemies);
                SpawnEnemy(spawnInfo.enemyData, spawnInfo.spawnLocation);

                totalSpawned++;
            }
        }
        //Wave 종료

        WaveEnd();
    }
    

    private void WaveEnd()
    {
        //레벨 업.
        //아이템 획득.
        foreach (var enemy in activeEnemies)
        {
            ObjectPoolingManager.Instance.ReleaseEnemy(enemy);
        }
        
        activeEnemies.Clear();

        if (_currentWave == LastWave)
        {
            OnStageEnd(true);
            return;
        }
        //무한모드 추후 추가.

        foreach (var (collectableType, hashSet) in _collectables)
        {
            if (collectableType == CollectableType.Crate)
            {
                foreach (var collectable in hashSet)
                {
                    cratePickup++;
                    ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
                }   
            }
            else
            { 
                //material, meat -> stock(재료 획득할 때 마다 2배로 획득하고 감소)
                foreach (var collectable in hashSet) 
                {
                    _playerManager.GetStock(); //stock표시 추가 필요
                    ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
                }
            }
            hashSet.Clear();
        }
        
        HandleOnWaveEnd();
    }

    private void HandleOnWaveEnd()
    {
        AudioManager.Instance.PlayBGM(BgmType.Store);
        
        SetupRerollPrice();

        OpenWaveEndUI();
    }

    private void SetupRerollPrice()
    {
        var rerollIncreasePerLv = DataManager.Instance.ProgressionTierWeightData.RerollIncreasePerProgression;
        var firstRerollPricePerLv = DataManager.Instance.ProgressionTierWeightData.FirstRerollPricePerProgression;

        _upgradeRerollIncrease =
            Mathf.FloorToInt(rerollIncreasePerLv * (_playerManager.CurrentLevel - waveLevelUp + 1));
        //리롤 증가 = 내림(레벨 * 계수(레벨 당 리롤 증가)) (리롤할 때 마다 증가)
        if (_upgradeRerollIncrease < 1) _upgradeRerollIncrease = 1;
        _upgradeRerollPrice = Mathf.FloorToInt(firstRerollPricePerLv * (_playerManager.CurrentLevel - waveLevelUp + 1))
                              + _upgradeRerollIncrease;
        //초기 리롤 가격 = 내림(레벨 * 계수(레벨 당 초기 리롤)) + 리롤 증가
        

        _storeRerollIncrease = Mathf.FloorToInt(rerollIncreasePerLv * _currentWave);
        if (_storeRerollIncrease < 1) _storeRerollIncrease = 1;
        _storeRerollPrice = Mathf.FloorToInt(firstRerollPricePerLv * _currentWave) + _storeRerollIncrease;
     
    }

    private void OpenWaveEndUI()
    {
        if (waveLevelUp > 0)
        {
            ShowStatUpgradePanel();
        }
        else if (cratePickup > 0)
        {
            ShowCrateSelectPanel();
        }
        else
        {
            ShowStorePanel();
        }
    }

    private void ShowStatUpgradePanel()
    {
        int upgradeLv = _playerManager.CurrentLevel - waveLevelUp + 1;//기준이 될 레벨.
      
        //스탯 섞기 (Fisher-Yates 셔플)
        var allStats = new List<MainStats>();
        for (int i = 0; i < (int)MainStats.None; i++)
            allStats.Add((MainStats)i);

        for (int i = allStats.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allStats[i], allStats[j]) = (allStats[j], allStats[i]);
        }
        //0~3의 무작위 스탯 리스트.
        
        _currentUpgradeOptions = new (MainStats stat, int tier)[UpgradeOptionCount];
        
        int guaranteedTier = GetGuaranteedTier(upgradeLv);
        if (guaranteedTier > 0) //확정 티어
        {
            for (int i = 0; i < UpgradeOptionCount; i++)
            {
                _currentUpgradeOptions[i] = (allStats[i], guaranteedTier);
            }
        }
        else
        {
            for (int i = 0; i < UpgradeOptionCount; i++)
            {
                var tier = RollTier(upgradeLv); //확률(가중치)에 따른 티어 뽑기.
                _currentUpgradeOptions[i] = (allStats[i], tier);
            }
        }

        bool canReroll = false;
        
        if(_playerManager.GetMoney >= _upgradeRerollPrice) canReroll = true;
        
        _stageUI.UpdateUpgradeRerollPrice(_upgradeRerollPrice, canReroll);

        _stageUI.OpenUpgradeUI(_currentUpgradeOptions);
    }

    private int GetGuaranteedTier(int level)
    {
        foreach (var config in DataManager.Instance.GuaranteedLvUpStatTier.GuaranteedTierConfigs)
        {
            if (config.level == level) return config.tier;
        }

        if (level % 5 == 0 && level > 25)
        {
            return 4; //25이상의 5의 배수 레벨 -> 티어4 확정
        }
        //csv수정 필요?
        return 0;
    }

    private float[] GetTierWeights(int progression)
    {
        var tierWeightConfigs = DataManager.Instance.ProgressionTierWeightData.TierWeightConfigs;
        int n = tierWeightConfigs.Length;
        var raw = new float[n];

        // 높은 티어부터 "해당 티어 이상이 나올 확률" 계산
        for (int i = n - 1; i >= 0; i--)
        {
            var config = tierWeightConfigs[i];
            raw[i] = progression < config.minProgression
                ? 0f
                : Mathf.Clamp(config.baseWeight + config.perProgression * (progression - config.minProgression + 1), 
                    0f, config.maxChance);
        }
        
        // 실제 확률 = raw[i] - raw[i+1]
        var chances = new float[n];
        for (int i = 0; i < n - 1; i++)
            chances[i] = raw[i] - raw[i + 1];
        chances[n - 1] = raw[n - 1];
        
        return chances;
    }

    private int RollTier(int progression) 
    {
        //티어 뽑기 - 레벨/웨이브
        float[] weights = GetTierWeights(progression);
        float total = weights.Sum();
        float rand = Random.Range(0f, total);

        float cumulative = 0;
        
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative)
            {
                return i + 1; //Tier 1~4
            }
        }

        return 1;
    }

    private void HandleOnSelectStatUpgrade(int optionIdx)
    {
        var (stat, tier) = _currentUpgradeOptions[optionIdx];
        _playerManager.GetStatUpgrade(stat, tier);
        waveLevelUp--;//잔여 레벨업 업그레이드 감소.
        
        OpenWaveEndUI();//웨이브 종료 처리(업그레이드, 상자...)
    }

    private void HandleOnRerollUpgrade()
    {
        //RerollCost...
        if(_playerManager.GetMoney < _upgradeRerollPrice) return;
        
        _playerManager.ChangeMoney(-_upgradeRerollPrice);

        _upgradeRerollPrice += _upgradeRerollIncrease;
        
        ShowStatUpgradePanel();
    }
    
    private void ShowCrateSelectPanel()
    {
        //아이템 뽑기.
        var tier = RollTier(_currentWave);
        
        var itemList = DataManager.Instance.ItemTierDict[tier];
        var randIdx =  Random.Range(0, itemList.Count);
        
        var itemData = itemList[randIdx];
        _currentCrateItem = itemData;
        
        //Price계산 (구매가의 절반)
        _currentCrateItemPrice = Mathf.FloorToInt(GetItemPrice(itemData) / 2f);

        _stageUI.OpenCrateUI(itemData, _currentCrateItemPrice);
    }

    private void ShowStorePanel()
    {
        _storeData.Items.Clear();
        _storeData.Weapons.Clear();
        for (int idx = 0; idx < StoreOptionCount; idx++)
        {
            var isItem = Random.Range(0, 2);
            var tier = RollTier(_currentWave);
            if (isItem == 1)
            {
                //Item
                var itemDataList = DataManager.Instance.ItemTierDict[tier];
                var itemDataIdx = Random.Range(0, itemDataList.Count);
                var itemData = itemDataList[itemDataIdx];
                var price = GetItemPrice(itemData);
                _storeData.Items.Add((itemData, price, idx));
            }
            else
            {
                //Weapon
                var weaponDataList = DataManager.Instance.WeaponTierDict[tier];
                var weaponDataIdx = Random.Range(0, weaponDataList.Count);
                var weaponData = weaponDataList[weaponDataIdx];
                var price = GetWeaponPrice(weaponData, tier);

                var weaponStat = _playerManager.GetWeaponStat(weaponData, tier);
                
                _storeData.Weapons.Add((weaponStat,price, idx));
            }
            
            _storeData.IsSold[idx]= false;
        }

        _storeData.Money = _playerManager.GetMoney;
        _storeData.RerollPrice = _storeRerollPrice;
        
        _stageUI.OpenStoreUI(_storeData);
    }
    
    private void HandleOnUseCrateItem()
    {
        _playerManager.AddItem(_currentCrateItem);

        cratePickup--;
        
        OpenWaveEndUI();
    }

    private void HandleOnSellCrateItem()
    {
        _playerManager.ChangeMoney(_currentCrateItemPrice);

        cratePickup--;
        
        OpenWaveEndUI();
    }
    
    private int GetItemPrice(ItemData itemData)
    {
        var basePrice = itemData.Price;
        var itemPriceStat = _playerManager.GetItemPriceStat;
        var finalPrice = (basePrice + _currentWave + (basePrice * 0.1f * _currentWave)) 
            * (100 + itemPriceStat) / 100f;
        
        return Mathf.FloorToInt(finalPrice);
    }

    private int GetWeaponPrice(WeaponData weaponData, int tier)
    {
        var initTier = weaponData.WeaponStat.initTier;
        var tierIdx = tier - initTier;
        var basePrice = weaponData.WeaponStat.prices[tierIdx];
        var itemPriceStat = _playerManager.GetItemPriceStat;
        var finalPrice = (basePrice + _currentWave + (basePrice * 0.1f * _currentWave)) 
            * (100 + itemPriceStat) / 100f;
        
        return Mathf.FloorToInt(finalPrice);
    }

    private void HandleOnStorePanelShowClassInfo(int idx)
    {
        foreach (var (itemData, _, itemIdx) in _storeData.Items)
        {
            if (itemIdx == idx)
            {
                var classes = new List<ItemClass> { itemData.ItemClass }; //수정 필요.

                _stageUI.ShowStorePanelClassInfo(classes, idx);
                return;
            }
        }

        foreach (var (weaponStat, _, weaponIdx) in _storeData.Weapons)
        {
            if (weaponIdx == idx)
            {
                var classes = weaponStat.WeaponData.WeaponStat.classes;
                
                _stageUI.ShowStorePanelClassInfo(classes, idx);
                return;
            }
        }
    }

    private void HandleOnRerollStore()
    {
        if(_storeRerollPrice > _playerManager.GetMoney) return;
        
        _playerManager.ChangeMoney(-_storeRerollPrice);
        
        _storeRerollPrice += _storeRerollIncrease;
        
        ShowStorePanel();
    }

    private void HandleOnBuyStore(int idx)
    {
        foreach (var (itemData, price, itemIdx) in _storeData.Items)
        {
            if (itemIdx == idx)
            {
                if(price > _playerManager.GetMoney) return;
                
                AudioManager.Instance.PlaySFX(SfxType.Purchase);
                
                _playerManager.ChangeMoney(-price);
                _playerManager.AddItem(itemData);
                
                _stageUI.DisableStorePanelOnBuy(idx);

                _storeData.IsSold[idx] = true;
                
                UpdateStorePanelCanBuy();
                return;
            }
        }

        foreach (var (currentWeaponStat, price, weaponIdx) in _storeData.Weapons)
        {
            if (weaponIdx == idx)
            {
                if(price > _playerManager.GetMoney) return;
                if(_playerManager.WeaponIsFull) return;
                
                AudioManager.Instance.PlaySFX(SfxType.Purchase);
                
                _playerManager.ChangeMoney(-price);
                _playerManager.AddWeapon(currentWeaponStat.WeaponData, currentWeaponStat.Tier);
                
                _stageUI.DisableStorePanelOnBuy(idx);
                
                _storeData.IsSold[idx] = true;

                UpdateStorePanelCanBuy();
                return;
            }
        }
    }
    
    private void UpdateStorePanelCanBuy()
    {
        foreach (var (_, price, idx) in _storeData.Items)
        {
            if(_storeData.IsSold[idx]) continue;
            var canBuy = price <= _playerManager.GetMoney;
            _stageUI.UpdateStorePanelCanBuy(idx, price, canBuy);
        }

        foreach (var (_, price, idx) in _storeData.Weapons)
        {
            if(_storeData.IsSold[idx]) continue;
            var canBuy = price <= _playerManager.GetMoney;
            _stageUI.UpdateStorePanelCanBuy(idx, price, canBuy);
        }
    }

    private void HandleOnNextWave()
    {
        _currentWave++;
        StartWave();
    }

    private void HandleOnCombineWeapon(int idx)
    {
        _playerManager.CombineWeapon(idx);
    }

    private void HandleOnRecycleWeapon(int idx)
    {
        _playerManager.RecycleWeapon(idx);
    }
    
    private void SpawnBosses(WaveData waveData) //웨이브 시작시 보스 전부 스폰.
    {
        if (waveData.BossEnemies == null || waveData.BossEnemies.Count == 0) return;

        foreach (var bossData in waveData.BossEnemies)
        {
            if (!bossData) continue;
            SpawnEnemy(bossData, SpawnLocationType.Far);
        }
    }
    

    private void SpawnEnemy(EnemyData enemyData, SpawnLocationType spawnLocation)
    {
        if (!enemyData) return;
        if (!enemyBasePrefab) { Debug.LogWarning("enemyBasePrefab이 비어 있습니다."); return; }

        var spawnPos = GetSpawnPosition(spawnLocation);

        var enemy = ObjectPoolingManager.Instance.GetEnemyBase();
        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.Init(enemyData, _currentWave, _playerManager.transform);
        enemy.OnDropCollectable += OnDropCollectable;
        enemy.OnDeath += OnEnemyDeath;
        activeEnemies.Add(enemy);
    }

    private EnemySpawnInfo PickWeightedEnemy(List<EnemySpawnInfo> enemies) //적 선택
    {
        float totalWeight = 0f;
        foreach (var e in enemies) totalWeight += e.weight;

        //가중치를 기반으로 스폰될 적 선택.
        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var e in enemies)
        {
            cumulative += e.weight;
            if (rand <= cumulative) return e;
        }
        
        return enemies[^1];
    }

    private Vector3 GetSpawnPosition(SpawnLocationType spawnLocation)
    {
        var playerPos = _playerManager.transform.position;
        
        for (int i = 0; i < MaxSpawnRetries; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = spawnLocation == SpawnLocationType.Near
                ? Random.Range(nearSpawnMin, nearSpawnMax)
                : Random.Range(farSpawnMin, farSpawnMax);

            var candidate = new Vector3(
                playerPos.x + Mathf.Cos(angle) * distance,
                playerPos.y + Mathf.Sin(angle) * distance,
                playerPos.z
            );

            if (!mapCollider || mapCollider.OverlapPoint(candidate))
                return candidate;
        }
        
        return playerPos;
    }

    private void OnEnemyDeath(EnemyManager enemy) //적 사망 처리
    {
        enemy.OnDeath -= OnEnemyDeath;
        enemy.OnDropCollectable -= OnDropCollectable;
        activeEnemies.Remove(enemy);//활성화 된 적 리스트에서 제거.
    }

    private void OnDropCollectable(Collectable collectable, CollectableType collectableType)
    {
        _collectables[collectableType].Add(collectable);
        collectable.OnPickup += OnCollectablePickup;
    }

    private void OnCollectablePickup(Collectable collectable, CollectableType collectableType)
    {
        _collectables[collectableType].Remove(collectable);
    }

    private void FindClosestEnemy() //가장 가까운 적 찾기 => 개선 점?
    {
        Transform closest = null;
        float minDistanceSqr = MaxFindRange * MaxFindRange;//최대 탐색 범위(제곱)

        foreach (var activeEnemy in activeEnemies) //활성화 된 적 리스트
        {
            var enemyTransform = activeEnemy.transform;
            
            Vector3 dir = enemyTransform.position - _playerManager.transform.position; //방향 벡터.
            float distSqr = dir.sqrMagnitude; //거리(제곱)

            if (distSqr < minDistanceSqr) //최소 거리(가장 가까운 적)
            {
                minDistanceSqr = distSqr;
                closest = enemyTransform;
            }
        }

        var newTarget = new TargetInfo(closest, minDistanceSqr);
        
        _playerManager.GetClosestEnemy(newTarget);
    }

    private void OnStageEnd(bool isClear)
    {
        Time.timeScale = 0f;
        //정지
        //1.메인화면(타이틀) 씬전환
        //2.빠른 재시작(같은 캐릭터, 무기, 난이도)
        
        _stageUI.StageEnd(isClear);
        //필요한 데이터?
    }
}
