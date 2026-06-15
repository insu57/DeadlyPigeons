using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnLocationType { Near, Far }

[Serializable]
public struct EnemySpawnInfo
{
    public EnemyData enemyData;
    public float weight; //0~1 사이의 값.
    public SpawnLocationType spawnLocation;
}

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [field: SerializeField] public int WaveNumber { get; private set; } //웨이브 1~20 
    [field: SerializeField] public float WaveLength { get; private set; } //웨이브 시간. 조절필요
    [field: SerializeField] public List<EnemySpawnInfo> Enemies { get; private set; } //스폰할 적 + 가중치
    [field: SerializeField] public List<EnemyData> BossEnemies { get; private set; }
    [field: SerializeField] public float SpawnTick { get; private set; } //스폰 틱
    //[field: SerializeField] public int EnemySpawnCount { get; private set; } //웨이브 당 스폰 수?
    [field: SerializeField] public int SpawnPerTick { get; private set; } //틱 당 스폰 수
    
#if UNITY_EDITOR
    public void SyncWaveData(int waveLength, List<EnemySpawnInfo> enemies, List<EnemyData> bossEnemies, 
        float spawnTick, int spawnPerTick)
    {
        WaveLength = waveLength;
        Enemies = enemies;
        BossEnemies = bossEnemies;
        SpawnTick = spawnTick;
        SpawnPerTick = spawnPerTick;
    }
#endif
}
