using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject bullet; //풀링으로 변경
    [SerializeReference] private IWeaponEffect effect;
    private void Update()
    {
        
    }
}
