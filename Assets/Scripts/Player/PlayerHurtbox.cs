using System;
using System.Collections;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour, IDamageable, IPickup
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
    private MaterialPropertyBlock _propBlock;
    private Coroutine _flashCoroutine;

    public event Action<int> OnDamage;
    public event Action<int> OnHeal;
    public event Action<CollectableType> OnGetCollectable;
    private float _invincibleTimer;
    private const float InvincibleDuration = 0.5f;
    private bool IsInvincible => _invincibleTimer > 0f;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    public void Update()
    {
        if(IsInvincible) 
        {
            _invincibleTimer -= Time.deltaTime;
        }
    }
    
    public void Damage(int damage, bool isCrit)
    {
        if(IsInvincible) return;

        AudioManager.Instance.PlaySFX(SfxType.PlayerHit);
        OnDamage?.Invoke(damage);

        _invincibleTimer = InvincibleDuration;

        PlayHitFlash();
    }

    private void PlayHitFlash()
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        SetFlashAmount(1f, hitFlashColor);
        yield return new WaitForSeconds(hitFlashDuration);
        SetFlashAmount(0f);
        _flashCoroutine = null;
    }

    private void SetFlashAmount(float amount, Color? color = null)
    {
        spriteRenderer.GetPropertyBlock(_propBlock);
        if (color.HasValue) _propBlock.SetColor(FlashColorID, color.Value);
        _propBlock.SetFloat(FlashAmountID, amount);
        spriteRenderer.SetPropertyBlock(_propBlock);
    }

    public void Heal(int healAmount)
    {
        OnHeal?.Invoke(healAmount);
    }

    public void DotDamage(int duration, int damage, float tick) { }

    public Transform GetTransform() => transform;
    
    public void Pickup(Collectable collectable)
    {
        if (collectable.CollectableType != CollectableType.Material) //재료 획득(Exp, Money)
        {
            OnGetCollectable?.Invoke(collectable.CollectableType);
            ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
        }
    }
}
