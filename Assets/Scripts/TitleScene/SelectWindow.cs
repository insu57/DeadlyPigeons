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
    private List<Button> _selectBtns = new ();

    [Header("Weapon Select")] 
    [SerializeField] private GameObject weaponSelect;
    [SerializeField] private Transform weaponViewportContent;
    
    [Header("Difficulty Select")]
    [SerializeField] private GameObject difficultySelect;
    [SerializeField] private Transform diffViewportContent;
    
    [Header("Char Description")] 
    [SerializeField] private Image charImage;
    [SerializeField] private TMP_Text charName;
    [SerializeField] private TMP_Text charHealth;
    
    [Header("Prefabs")]
    [SerializeField] private Sprite randomSprite;
    [SerializeField] private CharacterSelectBtn selectBtn; //Prefab
    //test
    [SerializeField] private CharacterDataSO[] chars;
    
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
                weaponSelect.SetActive(false);
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
        var randomBtn  = Instantiate(selectBtn, charViewportContent);
        randomBtn.OnBtnPointerEnter += ShowRandomDescription;
        //randomBtn.SelectBtn.onClick.AddListener(RandomButton);
        randomBtn.SelectBtn.onClick.AddListener(EnterWeaponSelect);
        
        for (int i = 0; i < chars.Length; i++)
        {
            var newBtn = Instantiate(selectBtn, charViewportContent);

            var idx = i;
            newBtn.SetCharacterImg(chars[idx].CharacterSprite);

            newBtn.OnBtnPointerEnter += () => ShowCharDescription(idx);


            //newBtn.SelectBtn.onClick.AddListener(() => ShowCharDescription(idx));
        }
    }

    private void ShowCharDescription(int idx)
    {
        var charData = chars[idx];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterStats.characterName;
        charHealth.text = charData.CharacterStats.maxHealth.ToString(CultureInfo.CurrentCulture);
    }

    private void ShowRandomDescription()
    {
        //Localization 생각.
        charImage.sprite = randomSprite;
        charName.text = "랜덤";
        charHealth.text = "?";
    }

    private void EnterWeaponSelect()
    {
        _currentDepth = SelectWindowState.WeaponSelect;
        charSelect.SetActive(false);
        weaponSelect.SetActive(true);
        
        //WEAPON...!
    }
    
    private void RandomButton()
    {
        var randIdx = Random.Range(0, chars.Length);
        var charData = chars[randIdx];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterStats.characterName;
        charHealth.text = charData.CharacterStats.maxHealth.ToString(CultureInfo.CurrentCulture);
    }
}
