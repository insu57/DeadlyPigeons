using UnityEngine;

// 넉백을 받는 대상만 구현. EnemyManager만 구현하며, 플레이어(PlayerHurtbox)는 구현하지 않아 넉백을 받지 않는다.
public interface IKnockbackable
{
    // direction: 밀려날 방향(정규화 불필요), force: 임펄스 세기
    public void Knockback(Vector2 direction, float force);
}
