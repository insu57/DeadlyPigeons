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

    [Header("Enemy Spawn")] [SerializeField]
    private EnemyManager enemyBasePrefab;

    [SerializeField] private Collider2D mapCollider;
    [SerializeField] private float nearSpawnMin = 3f;
    [SerializeField] private float nearSpawnMax = 6f;
    [SerializeField] private float farSpawnMin = 10f;
    [SerializeField] private float farSpawnMax = 15f;
    private const int MaxSpawnRetries = 10;

    [Header("Wave")]
    [SerializeField] private int waveLevelUp = 0;
    [SerializeField] private int cratePickup = 0;
    private StageUI _stageUI;
    private (MainStats stat, int tier)[] _currentUpgradeOptions;
    //수정?
    private const int UpgradeOptionCount = 4;
    
    //시트로 관리?

    /*/[SerializeField] private TierLevelWeightConfig[] tierWeightConfigs =
    {
        new() { baseWeight = 100, minLevel = 0, perLevel = 0, maxChance = 100 },
        new() { baseWeight = 0, minLevel = 2, perLevel = 6, maxChance = 60 },
        new() { baseWeight = 0, minLevel = 4, perLevel = 2, maxChance = 25 },
        new() { baseWeight = 0, minLevel = 8, perLevel = 0.23f, maxChance = 8 },
    }; //Config 수정?*/

  

    //test
    [SerializeField] private CharacterData testChar;
    [SerializeField] private List<WeaponData> testWeapon;
    [SerializeField] private int testStage;
    [SerializeField] private int testWave;

    private int _currentWave = 1; //1~20

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
        _stageUI.OnSelectStatUpgrade += HandleOnSelectStatUpgrade;
        
        //웨이브 시작
        var waveData = DataManager.Instance.WaveDataList[_currentWave - 1];
        _stageUI.SetCurrentWaveText(_currentWave);
        StartCoroutine(WaveCoroutine(waveData));
        
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
        _stageUI.Init(_playerManager, UpgradeOptionCount);
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

        _playerManager.OnPlayerLevelUp += OnLevelUp;
        _playerManager.OnCratePickup += OnCratePickup;
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

        float elapsed = 0f;
        int totalSpawned = 0;
        int maxSpawn = waveData.EnemySpawnCount;
        var spawnTick = new WaitForSeconds(waveData.SpawnTick);

        while (elapsed < waveData.WaveLength && totalSpawned < maxSpawn)
        {
            yield return spawnTick;

            elapsed += waveData.SpawnTick;

            var leftTime = waveData.WaveLength - elapsed;
            _stageUI.UpdateWaveTimer(leftTime);

            int toSpawn = Mathf.Min(waveData.SpawnPerTick, maxSpawn - totalSpawned);
            for (int i = 0; i < toSpawn; i++)
            {
                SpawnEnemy(waveData);
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

        //_stageUI.OpenStoreUI(true);
        //개선방안?
        HandleOnWaveEnd();
    }

    private void HandleOnWaveEnd()
    {
        if (waveLevelUp > 0)
        {
            ShowStatUpgradePanel();
        }
        else if (cratePickup > 0)
        {
            _stageUI.OpenCrateUI(null, 0); //WIP
        }
        else
        {
            _stageUI.OpenStoreUI();
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
                var tier = RollUpgradeTier(upgradeLv); //확률(가중치)에 따른 티어 뽑기.
                _currentUpgradeOptions[i] = (allStats[i], tier);
            }
        }

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

    private float[] GetUpgradeTierWeights(int level)
    {
        var tierLevelWeightConfigs = DataManager.Instance.LvUpStatUpgradeWeight.TierLevelWeightConfigs;
        int n = tierLevelWeightConfigs.Length;
        var raw = new float[n];

        // 높은 티어부터 "해당 티어 이상이 나올 확률" 계산
        for (int i = n - 1; i > 0; i--)
        {
            var config = tierLevelWeightConfigs[i];
            raw[i] = level < config.minLevel
                ? 0f
                : Mathf.Clamp(config.baseWeight + config.perLevel * (level - config.minLevel), 0f, config.maxChance);
        }
        
        // 실제 확률 = raw[i] - raw[i+1]
        var chances = new float[n];
        for (int i = 0; i < n - 1; i++)
            chances[i] = raw[i] - raw[i + 1];
        chances[n - 1] = raw[n - 1];

        return chances;
    }

    private int RollUpgradeTier(int level) //티어 뽑기
    {
        float[] weights = GetUpgradeTierWeights(level);
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
        
        HandleOnWaveEnd();//웨이브 종료 처리(업그레이드, 상자...)
    }

    private void ShowCrateSelectPanel()
    {
        
    }
    
    private void SpawnEnemy(WaveData waveData)
    {
        if (waveData.Enemies == null || waveData.Enemies.Count == 0) return;
        
        if (!enemyBasePrefab) { Debug.LogWarning("enemyBasePrefab이 비어 있습니다."); return; }

        var spawnInfo = PickWeightedEnemy(waveData.Enemies);
        var spawnPos = GetSpawnPosition(spawnInfo.spawnLocation);
        
        var enemy = ObjectPoolingManager.Instance.GetEnemyBase();
        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.Init(spawnInfo.enemyData, _currentWave, _playerManager.transform);
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
        activeEnemies.Remove(enemy);//활성화 된 적 리스트에서 제거.
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
}
