using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    //WIP
    private PlayerManager _playerManager;
    [SerializeField] private TMP_Text stageText;
    private PlayerSelected _playerSelected;
    
    [field: SerializeField] private List<EnemyManager> activeEnemies = new();
    private const float MaxFindRange = 100f;

    [Header("Enemy Spawn")]
    [SerializeField] private EnemyManager enemyBasePrefab;
    [SerializeField] private float nearSpawnMin = 3f;
    [SerializeField] private float nearSpawnMax = 6f;
    [SerializeField] private float farSpawnMin = 10f;
    [SerializeField] private float farSpawnMax = 15f;

    //test
    [SerializeField] private CharacterData testChar;
    [SerializeField] private List<WeaponData> testWeapon;
    [SerializeField] private int testStage;
    [SerializeField] private int testWave;

    private int _currentWave = 1; //1~20
    
    private void Awake()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
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
        
        //pooling
        ObjectPoolingManager.Instance.InitProjectilePool();
        ObjectPoolingManager.Instance.InitDamageTxtPool();
        ObjectPoolingManager.Instance.InitExplosivePool();

        //웨이브 시작
        var waveData = DataManager.Instance.WaveDataList[_currentWave - 1];
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
        Debug.Log("init stage");
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
        
        _playerManager.InitCharacter(charData,items, weapons);
    }

    private IEnumerator WaveCoroutine(WaveData waveData)
    {
        float elapsed = 0f;
        int totalSpawned = 0;
        int maxSpawn = waveData.EnemySpawnCount;
        
        var spawnTick = new WaitForSeconds(waveData.SpawnTick);

        while (elapsed < waveData.WaveLength && totalSpawned < maxSpawn)
        {
            yield return spawnTick;
            
            elapsed += waveData.SpawnTick;

            int toSpawn = Mathf.Min(waveData.SpawnPerTick, maxSpawn - totalSpawned);
            for (int i = 0; i < toSpawn; i++)
            {
                SpawnEnemy(waveData);
                totalSpawned++;
            }
        }

        Debug.Log($"Wave {waveData.WaveNumber} 스폰 완료 (총 {totalSpawned}마리)");
    }

    private void SpawnEnemy(WaveData waveData)
    {
        if (waveData.Enemies == null || waveData.Enemies.Count == 0) return;
        if (!enemyBasePrefab) { Debug.LogWarning("enemyBasePrefab이 비어 있습니다."); return; }

        var spawnInfo = PickWeightedEnemy(waveData.Enemies);
        var spawnPos = GetSpawnPosition(spawnInfo.spawnLocation);

        //Pooling으로 수정.
        var enemy = Instantiate(enemyBasePrefab, spawnPos, Quaternion.identity);
        enemy.Init(spawnInfo.enemyData, _currentWave, _playerManager.transform);
        enemy.OnDeath += OnEnemyDeath;
        activeEnemies.Add(enemy);
    }

    private EnemySpawnInfo PickWeightedEnemy(List<EnemySpawnInfo> enemies)
    {
        float totalWeight = 0f;
        foreach (var e in enemies) totalWeight += e.weight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        foreach (var e in enemies)
        {
            cumulative += e.weight;
            if (rand <= cumulative) return e;
        }
        return enemies[^1];
    }

    private Vector3 GetSpawnPosition(SpawnLocationType spawnLocation) //스폰 위치
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = spawnLocation == SpawnLocationType.Near
            ? Random.Range(nearSpawnMin, nearSpawnMax) //근거리 스폰
            : Random.Range(farSpawnMin, farSpawnMax); //원거리 스폰

        var playerPos = _playerManager.transform.position;
        return new Vector3(
            playerPos.x + Mathf.Cos(angle) * distance,
            playerPos.y + Mathf.Sin(angle) * distance,
            playerPos.z
        );
    }

    private void OnEnemyDeath(EnemyManager enemy)
    {
        activeEnemies.Remove(enemy);
    }
    
    private void FindClosestEnemy() //가장 가까운 적 찾기
    {
        Transform closest = null;
        float minDistanceSqr = MaxFindRange * MaxFindRange;

        foreach (var activeEnemy in activeEnemies)
        {
            var enemyTransform = activeEnemy.transform;
            
            Vector3 dir = enemyTransform.position - _playerManager.transform.position;
            float distSqr = dir.sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closest = enemyTransform;
            }
        }

        var newTarget = new TargetInfo(closest, minDistanceSqr);
        
        _playerManager.GetClosestEnemy(newTarget);
    }
}
