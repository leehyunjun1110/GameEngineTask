using System.Collections;
using UnityEngine;

public class LightningDamage : MonoBehaviour
{
    public int damage = 2;
    public float activationDelay = 0.05f;
    public float damageCooldown = 1.0f;

    private Collider2D lightningCollider;
    private bool canDamagePlayer = true;

    private void Awake()
    {
        lightningCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // 콜라이더 비활성화 상태로 시작
        if (lightningCollider != null)
            lightningCollider.enabled = false;

        canDamagePlayer = true;

        // 지연 후 콜라이더 활성화
        StartCoroutine(ActivateColliderAfterDelay());
    }

    private IEnumerator ActivateColliderAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        if (lightningCollider != null)
            lightningCollider.enabled = true;
    }

    private void OnDisable()
    {
        // 오브젝트가 풀로 반환될 때 상태 초기화
        if (lightningCollider != null)
            lightningCollider.enabled = false;

        canDamagePlayer = true;
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canDamagePlayer && other.TryGetComponent(out PlayerController player))
        {
            player.StartCoroutine(player.PlayerHurt(damage));
            canDamagePlayer = false;
            StartCoroutine(ResetDamageCooldown());
        }
    }

    private IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        canDamagePlayer = true;
    }
}