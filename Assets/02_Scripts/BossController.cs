using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    private enum BossState { Idle, Chase, Attack, Cast, Heal, Dead, Return }

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private AudioSource bas;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float detectRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float knockbackForce = 0.3f;
    [SerializeField] private float healCooldown = 10f;
    [SerializeField] private float healCastDelay = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private int lightningDamage = 2;
    [SerializeField] private float attackDelay = 2.0f;

    [Header("Lightning Skill")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject lightningWarningPrefab;
    [SerializeField] private float lightningCooldown = 6f;
    [SerializeField] private float lightningDelay = 1.0f;
    [SerializeField] private float lightningDuration = 1.5f;
    [SerializeField] private float warningDuration = 0.7f;
    [SerializeField] private float lightningOffsetY = -0.5f;
    [SerializeField] private Transform lightningSpawnRoot;

    private Transform player;
    private Vector2 originPosition;
    private float currentHealth;
    private float lastLightningTime;
    private float lastHealTime;
    private bool isDead;
    private bool isAttacking;

    private BossState currentState = BossState.Idle;
    private readonly int[] lightningOffsets = { -1, 1, -2, 2 };
    private int attackPatternStep = 0;

    private ObjectPool objectPool;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originPosition = transform.position;
        currentHealth = maxHealth;
        bossHealthSlider.maxValue = maxHealth;
        bossHealthSlider.value = currentHealth;
        bossHealthSlider.gameObject.SetActive(false);
        objectPool = GameObject.FindObjectOfType<ObjectPool>();

        if (objectPool == null)
        {
            Debug.LogError("ObjectPool not found in scene! Ensure an ObjectPool component exists.");
        }
    }

    private void Update()
    {
        if (isDead || player == null || isAttacking) return;

        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);

        if (currentState == BossState.Return)
        {
            ReturnToOrigin();
            return;
        }

        if (horizontalDistance < detectRange && verticalDistance < 3f)
        {
            bas.Play();
            if (!bossHealthSlider.gameObject.activeSelf)
                bossHealthSlider.gameObject.SetActive(true);

            if (horizontalDistance <= attackRange)
            {
                if (attackPatternStep % 3 == 2)
                    currentState = BossState.Attack;
                else
                    currentState = BossState.Cast;

                attackPatternStep = (attackPatternStep + 1) % 6;
            }
            else
            {
                currentState = BossState.Chase;
            }
        }
        else if (Time.time - lastHealTime >= healCooldown && currentHealth < maxHealth)
        {
            currentState = BossState.Heal;
        }
        else
        {
            currentState = BossState.Idle;
        }

        HandleState();
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case BossState.Idle:
                animator.SetBool("IsWalk", false);
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case BossState.Chase:
                animator.SetBool("IsWalk", true);
                ChasePlayer();
                break;
            case BossState.Attack:
                animator.SetBool("IsWalk", false);
                StartCoroutine(AttackPlayer());
                break;
            case BossState.Cast:
                animator.SetBool("IsWalk", false);
                StartCoroutine(UseLightningSkill());
                lastLightningTime = Time.time;
                break;
            case BossState.Heal:
                animator.SetBool("IsWalk", false);
                StartCoroutine(Heal());
                lastHealTime = Time.time;
                break;
            case BossState.Return:
                animator.SetBool("IsWalk", true);
                ReturnToOrigin();
                break;
            case BossState.Dead:
                break;
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        float direction = player.position.x < transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(-direction * moveSpeed, rb.velocity.y);
        transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
    }

    private void ReturnToOrigin()
    {
        float direction = originPosition.x < transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(-direction * moveSpeed, rb.velocity.y);
        transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);

        if (Mathf.Abs(transform.position.x - originPosition.x) < 0.1f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentState = BossState.Idle;
        }
    }
    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay * 0.5f);

        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange * 1.2f)
        {
            if (player.TryGetComponent(out Rigidbody2D playerRb))
            {
                Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
                playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            }

            if (player.TryGetComponent(out PlayerController playerController))
            {
                playerController.StartCoroutine(playerController.PlayerHurt(attackDamage));
            }
        }

        yield return new WaitForSeconds(attackDelay * 0.5f);

        isAttacking = false;
    }
    private IEnumerator UseLightningSkill()
    {
        if (objectPool == null)
        {
            Debug.LogError("ObjectPool is null in BossController");
            isAttacking = false;
            yield break;
        }

        isAttacking = true;
        animator.SetTrigger("Cast");

        foreach (int offset in lightningOffsets)
        {
            Vector2 warnPos = new(transform.position.x + offset, (lightningSpawnRoot ? lightningSpawnRoot.position.y : transform.position.y));
            GameObject warn = objectPool.GetFromPool(lightningWarningPrefab);
            warn.transform.position = warnPos;
            objectPool.ReturnToPool(warn, warningDuration);
        }

        yield return new WaitForSeconds(warningDuration);

        foreach (int offset in lightningOffsets)
        {
            Vector2 warnPos = new(transform.position.x + offset, (lightningSpawnRoot ? lightningSpawnRoot.position.y : transform.position.y));
            Vector2 boltPos = warnPos + new Vector2(0, lightningOffsetY);
            GameObject bolt = objectPool.GetFromPool(lightningPrefab);
            bolt.transform.position = boltPos;

            if (bolt.TryGetComponent(out LightningDamage lightning))
                lightning.damage = lightningDamage;

            objectPool.ReturnToPool(bolt, lightningDuration);
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    public IEnumerator TakeDamage(float damage)
    {
        if (isDead) yield break;

        currentHealth -= damage;
        bossHealthSlider.value = currentHealth;
        animator.SetTrigger("Hurt");

        rb.simulated = false;
        yield return new WaitForSeconds(1f);
        rb.simulated = true;

        if (currentHealth <= 0)
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1.5f);
        currentState = BossState.Dead;
        SceneManager.LoadScene("End");
    }

    private IEnumerator Heal()
    {
        isAttacking = true;
        animator.SetTrigger("Cast");
        yield return new WaitForSeconds(healCastDelay);

        float lostHealth = maxHealth - currentHealth;
        float healAmount = lostHealth * 0.5f;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        bossHealthSlider.value = currentHealth;
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ReturnZone"))
            currentState = BossState.Return;
    }
}