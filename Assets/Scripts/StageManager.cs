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
    
    private HashSet<EnemyManager> activeEnemies = new();
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

    private Vector3 GetSpawnPosition(SpawnLocationType spawnLocation) //스폰 위치
    {
        float angle = Random.Range(0f, Mathf.PI * 2f); //무작위 각도
        float distance = spawnLocation == SpawnLocationType.Near
            ? Random.Range(nearSpawnMin, nearSpawnMax) //근거리 스폰
            : Random.Range(farSpawnMin, farSpawnMax); //원거리 스폰

        var playerPos = _playerManager.transform.position;
        return new Vector3(
            playerPos.x + Mathf.Cos(angle) * distance, //각도와 거리로 좌표 계산
            playerPos.y + Mathf.Sin(angle) * distance,
            playerPos.z
        );
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
