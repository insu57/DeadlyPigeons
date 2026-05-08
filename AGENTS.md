# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

Brotato-inspired top-down survival shooter built in Unity. Player survives waves of enemies using multiple weapons and passive items.

## Unity Commands

This is a Unity project — there is no CLI build command. Open the project in Unity Editor and use:
- **Play**: Enter Play Mode in Editor to test
- **Build**: File > Build Settings > Build
- Unity version: check `ProjectSettings/ProjectVersion.txt`

## Architecture

### Scene Flow
`TitleScene` → character/weapon selection → `MainScene` (gameplay). `SceneChanger` (Singleton) carries selected character data between scenes.

### Core Singletons
| Class | Responsibility |
|---|---|
| `DataManager` | Loads and caches all ScriptableObjects (WeaponData, ItemData, CharacterData, etc.) |
| `ObjectPoolingManager` | Object pools for Projectile, DamageTxt, Hitbox (explosion), SelectButton |
| `InputManager` | Wraps Unity's new Input System |
| `SceneChanger` | Async scene loading; holds `PlayerSelected` (CharacterData) across scenes |
| `StageManager` | Stage init; 매 프레임 `FindClosestEnemy()`로 가장 가까운 적을 `TargetInfo` 구조체로 `PlayerManager.GetClosestEnemy()`에 전달 |

### TargetInfo (`StageManager.cs`)
`TargetInfo` 구조체: `Target (Transform)`, `SqrDistance (float)`, `IsValid (bool)`. `StageManager.FindClosestEnemy()`가 생성해 `PlayerManager.GetClosestEnemy()`로 전달한다. `PlayerWeapon`이 이를 이용해 가장 가까운 적을 조준한다.

### Player System (`Assets/Scripts/Player/`)
- **`PlayerManager`** — top-level orchestrator. Initializes weapons (up to 6 slots) and items from `CharacterData`. Manages weapon class bonuses via `WeaponClassDict`. Listens to stat changes and propagates them to weapons.
- **`PlayerStat`** (implements `IDamageable`) — stat authority. Holds base, multiplier, and final dictionaries for `MainStat` and `SubStat` enums. Final value formula: `base * (1 + multiplier/100)`. Fires `OnChangeMainStats` / `OnChangeSubStats` events on any change.
- **`PlayerWeapon`** — per-weapon controller attached as a child component. Rotates to face the nearest enemy, runs its own attack timer (`baseSpeed / (1 + 0.01 * attackSpeedStat)`), and fires either a `Hitbox` (melee) or `Projectile` (ranged) from the pool.
- **`PlayerControl`** — movement via `Rigidbody2D` at 5f speed using new Input System.

### Enemy System (`Assets/Scripts/Enemy/`)
State machine pattern. `EnemyManager` holds a `Dictionary<EnemyStateType, IEnemyState>` and delegates `Update`/`FixedUpdate` to the active state. 모든 적 설정은 `EnemyData` (ScriptableObject)에서 읽는다.

**`EnemyData`** — 적의 모든 설정값 보유:
- `ID`, `EnemyName`
- `BaseHealth`, `HealthPerWave` — HP 웨이브 스케일: `BaseHealth + HealthPerWave * (wave - 1)`
- `BaseDamage`, `DamagePerWave` — 공격력 웨이브 스케일: `BaseDamage + DamagePerWave * (wave - 1)`
- `BaseSpeed`, `KnockbackResistance`
- `MaterialsDrop`, `ConsumableDropChance`, `LootCrateDropChance` — 드롭 관련
- `InitWave` — 이 적이 등장하기 시작하는 웨이브
- `InitialState` (`EnemyStateType`) — 시작 상태
- `Transitions` (`StateTransition[]`) — 전환 조건 목록. 각 항목은 `condition` / `threshold` / `targetState`로 구성

`EnemyManager.CheckTransitions()`가 매 프레임 조건을 순서대로 평가하고, 충족되면 즉시 전환 후 중단(return). 이미 해당 상태면 스킵. `_currentWave`는 스폰 시 주입 예정(WIP). `StageManager`에 `testWave` 인스펙터 필드로 테스트 가능.

**전환 조건 (`TransitionCondition`)**:
- `HealthBelow` — 현재 HP% < threshold
- `PlayerNear` — 플레이어 거리 < threshold
- `PlayerFar` — 플레이어 거리 > threshold

**구현된 상태**:
- **`ChaseState`** — 플레이어 방향으로 직선 이동 (속도 3f)
- **`KiteState`** — Hysteresis 방식으로 이동 제어. 거리 < `FleeDistance(4f)`: 도망, 거리 > `ApproachDistance(6f)`: 접근, 그 사이 구간(4~6f)은 현재 모드 유지(떨림 방지). + 1.5초마다 투사체 발사. `EnemyProjectile` 레이어가 Physics2D 충돌 매트릭스에서 Player 레이어와 충돌하도록 설정 필요

**새 state 추가 방법**: `IEnemyState` 구현 → `EnemyStateType` enum에 값 추가 → `EnemyManager.Start()`에 `_enemyStates[EnemyStateType.Xxx] = new XxxState()` 등록.

### Weapon & Item Data (ScriptableObjects)
- **`WeaponData`** — stat struct with tier scaling (tier 1–4), attack type (Sweep/Thrust/ranged), weapon class, and a list of `WeaponEffectType` entries.
- **`ItemData`** — flat stat bonuses + multipliers. Character passive items share ID with `CharacterData`.
- **`WeaponClassBonusData`** — bonuses that activate when 2+ weapons of the same class are equipped. Checked by `PlayerManager` whenever loadout changes.
- **`WaveData`** — 웨이브 1개의 스폰 설정: `WaveNumber`, `WaveLength`, `SpawnTick`, `EnemySpawnCount`, `SpawnPerTick`, `List<EnemyData> Enemies`. `EnemySpawnInfo` struct (`enemyData` + `weight`)도 정의되어 있으나 현재 WaveData에서는 미사용(WIP).
- CSV → ScriptableObject sync methods exist (`SyncDataCSV()`) but are editor-only (`#if UNITY_EDITOR`).

### Damage Pipeline
1. Attacker gets a pooled `Projectile` or `Hitbox`, initializes it with `ProjectileInitData` / `AttackInit()`.
2. On `OnTriggerEnter2D`, calls `IDamageable.Damage()` on the target.
3. Each `IWeaponEffect` in the effect list executes (`Burning` → DoT coroutine on `EnemyManager`, `Explosive` → pool a `Hitbox` AoE, `Piercing` / `Bounces` → modify projectile counters).
4. `EnemyManager.Damage()` / `PlayerStat.Damage()` spawn a pooled `DamageTxt`.

### Weapon Effects (`Assets/Scripts/Items/`)
All implement `IWeaponEffect`: `Init`, `SetExecuteData`, `Execute` (called on hit), `AttackEnd`, `Remove`.
- `Burning` — starts `DotDamage` coroutine on target.
- `Explosive` — spawns pooled `Hitbox` with AoE.
- `Piercing` — increments projectile pierce count.
- `Bounces` — increments projectile bounce count.

### UI
- **`PlayerInfoUI`** — main HUD. 무기/아이템 슬롯 표시, 툴팁 패널 제어, 스탯 업데이트를 `PlayerStatInfo[]`에 위임. `ClassInfo`로 무기 클래스 보너스 표시. Tab 키로 상태창 토글.
- **`PlayerStatInfo`** (`Assets/Scripts/MainUI/`, prefab: `PlayerStatinfo`) — 스탯 표시 패널 컴포넌트. MainStat/SubStat 탭 버튼으로 전환. `InitStatGrid()`로 `PlayerStatTxt` 항목 생성. `UpdateMainStat()` / `UpdateSubStat()`는 값에 따라 텍스트 색상 변경(양수=초록, 0=흰색, 음수=빨강). `PlayerInfoUI`가 배열로 보유하여 복수 패널 지원.
- **`InfoPanel`** — hover tooltip for weapons and items.
- **`TitleUI`** — title screen state machine (main menu → select → options).

### Object Pooling
All pools are configured via `PoolingSetting` ScriptableObjects in `Resources/Pooling/`. Call `InitXxxPool()` before first use. Pools use `UnityEngine.Pool.ObjectPool<T>` internally.

## Key Enums (defined in ScriptableObjects / StatUtil)

```
MainStat:   MaxHP, HealthRegen, HealthAbsorb, Armor, DodgeChance, Speed,
            Damage, Melee, Ranged, Elemental, Engineering, Tactical,
            AttackSpeed, CritChance, Range, Luck, Harvest

SubStat:    ConsumableHeal, XPGain, ItemPrice, PickUpRange, ExplosiveDamage,
            ExplosiveSize, Bounces, Piercing, PiercingDamage, FreeRerolls,
            Enemies, EnemiesSpeed, RerollPrice, Knockback

WeaponClass: Precise, Blunt, Primitive, Gun, Medieval, Blade, Heavy, Elemental
AttackType:  None, Sweep, Thrust
```

## Code Editing Guidelines

- 코드 수정 시 기존 주석은 최대한 유지한다. 삭제하거나 덮어쓰지 않는다.

## Known WIP / Incomplete Areas
- `PlayerStat.Damage` / `Heal` — stubbed, not implemented.
- Level-up system — referenced but not implemented.
- Map boundaries in `PlayerControl` — TODO comment.
- `ObjectPoolingManager` — single shared projectile pool used by both player and enemies; separate pools may be needed.
- `PlayerCamera` — marked for improvement.
- `WaveData` 스폰 시스템 — ScriptableObject 정의만 완료. 실제 스폰 로직(`StageManager` 연동) 미구현. `EnemySpawnInfo.weight`(가중치 기반 스폰)도 미사용.
