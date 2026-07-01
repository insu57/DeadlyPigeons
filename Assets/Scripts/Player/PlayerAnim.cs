using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private static readonly int IsWalk = Animator.StringToHash("IsWalk");
    [SerializeField] private Animator animator;

    public void SetWalk(bool isWalk)
    {
        animator.SetBool(IsWalk, isWalk);
    }
}
