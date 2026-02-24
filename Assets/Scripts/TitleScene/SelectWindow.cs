using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectWindow : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [FormerlySerializedAs("closeBtn")] [SerializeField] private Button backButton;

    private enum SelectWindowState
    {
        CharSelect = 0,
        WeaponSelect = 1,
        DifficultySelect = 2
    }
    
    private SelectWindowState _currentDepth = 0;
    [Header("Char Select")]
    [SerializeField] private GameObject charSelect;
    [SerializeField] private Transform charViewportContent;

    [Header("Weapon Select")] 
    [SerializeField] private GameObject weaponSelect;
    [SerializeField] private Transform weaponViewportContent;
    private List<SelectButton> _weaponSelectList = new();
    
    [Header("Difficulty Select")]
    [SerializeField] private GameObject difficultySelect;
    [SerializeField] private Transform diffViewportContent;
    
    [Header("Char Description")] 
    [SerializeField] private Image charImage;
    [SerializeField] private TMP_Text charName;
    [SerializeField] private TMP_Text charHealth;
    
    [Header("Prefabs")]
    [SerializeField] private Sprite randomSprite;
    [SerializeField] private SelectButton selectButton; //Prefab
 
    
    public GameObject Window => window;
    
    private void Start()
    {
        backButton.onClick.AddListener(OnBackBtnClick);
        
        InitCharSelectBtn();
    }

    public void OpenSelectWindow()
    {
        window.SetActive(true);
        _currentDepth = SelectWindowState.CharSelect;
        
        charSelect.SetActive(true);
        weaponSelect.SetActive(false);
        difficultySelect.SetActive(false);
    }

    private void OnBackBtnClick()
    {
        switch (_currentDepth)
        {
            case SelectWindowState.CharSelect:
            {
                window.SetActive(false);
                break;
            }
            case SelectWindowState.WeaponSelect:
            {
                weaponSelect.SetActive(false); //개선 방안 필요.
                charSelect.SetActive(true);
                break;
            }
            case SelectWindowState.DifficultySelect:
            {
                difficultySelect.SetActive(false);
                weaponSelect.SetActive(true);
                break;
            }
            default: return;
        }
        
        _currentDepth--;
    }
    
    private void InitCharSelectBtn()
    {
        //랜덤 캐릭터 버튼 추가 필요
        //var randomBtn  = Instantiate(selectButton, charViewportContent);
        var randomBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        randomBtn.transform.SetParent(charViewportContent);
        randomBtn.OnBtnPointerEnter += ShowRandomDescription;
        //randomBtn.SelectBtn.onClick.AddListener(RandomButton);
        //randomBtn.SelectBtn.onClick.AddListener(EnterWeaponSelect);
        
        foreach (var (id, charData) in DataManager.Instance.CharDict)
        {
            //var newBtn = Instantiate(selectButton, charViewportContent);
            var newBtn = ObjectPoolingManager.Instance.GetSelectBtn();
            newBtn.transform.SetParent(charViewportContent);
            
            newBtn.SetButtonImg(charData.CharacterSprite);
           
            newBtn.OnBtnPointerEnter += () => ShowCharDescription(id);

            newBtn.SelectBtn.onClick.AddListener(() => EnterWeaponSelect(id));
        }
        
    }

    private void ShowCharDescription(int id)
    {
        var charData = DataManager.Instance.CharDict[id];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;
        charHealth.text = charData.CharMainStats.maxHealth.ToString(CultureInfo.CurrentCulture);
    }

    private void ShowWeaponList(int id)
    {
        foreach (var selectBtn in _weaponSelectList)
        {
            ObjectPoolingManager.Instance.ReleaseSelectBtn(selectBtn);
        }
        
        _weaponSelectList.Clear();


        foreach (var weaponID in DataManager.Instance.CharDict[id].InitWeaponIDList)
        {
            var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
            
            selectBtn.transform.SetParent(weaponViewportContent);
            
            selectBtn.SetButtonImg(DataManager.Instance.WeaponDict[weaponID].Sprite);
            
            _weaponSelectList.Add(selectBtn);
        }
    }

    private void ShowRandomDescription()
    {
        //Localization 주의.
        charImage.sprite = randomSprite;
        charName.text = "랜덤";
        charHealth.text = "?";
    }

    private void SetSelectBtn(int id, Sprite sprite)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(sprite);
        selectBtn.OnBtnPointerEnter += () => ShowCharDescription(id);
        
    }

    private void EnterWeaponSelect(int id)
    {
        ShowCharDescription(id);
        
        _currentDepth = SelectWindowState.WeaponSelect;
        charSelect.SetActive(false);
        weaponSelect.SetActive(true);
        
        //WEAPON...!
        ShowWeaponList(id);
    }
    
    private void RandomCharacter()
    {
        var randIdx = Random.Range(0, DataManager.Instance.CharList.Count);
        var charData = DataManager.Instance.CharList[randIdx];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;
        charHealth.text = charData.CharMainStats.maxHealth.ToString(CultureInfo.CurrentCulture);
    }
}
