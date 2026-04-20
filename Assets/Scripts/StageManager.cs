using System.Collections.Generic;
using System.Text;
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
    
    [field: SerializeField] private List<Transform> activeEnemies = new();
    private const float MaxFindRange = 100f;
    
    //test
    [SerializeField] private CharacterData testChar;
    [SerializeField] private List<WeaponData> testWeapon;
    [SerializeField] private int testStage;

    private void Awake()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
    }
    
    private void Start()
    {
        InitStage();

        //test
        var sb = new StringBuilder();
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
        
        //pooling
        ObjectPoolingManager.Instance.InitProjectilePool();
        ObjectPoolingManager.Instance.InitDamageTxtPool();
        ObjectPoolingManager.Instance.InitExplosivePool();
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

    private void FindClosestEnemy()
    {
        Transform closest = null;
        float minDistanceSqr = MaxFindRange * MaxFindRange;

        foreach (var activeEnemy in activeEnemies)
        {
            Vector3 dir = activeEnemy.position - _playerManager.transform.position;
            float distSqr = dir.sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closest = activeEnemy;
            }
        }

        var newTarget = new TargetInfo(closest, minDistanceSqr);
        
        _playerManager.GetClosestEnemy(newTarget);
    }
}
