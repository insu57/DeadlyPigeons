using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    //need weapon id
    [SerializeField] private string weaponName;
    public string WeaponName => weaponName;

    [SerializeField] private Sprite weaponSprite;
    public Sprite WeaponSprite => weaponSprite;

}
