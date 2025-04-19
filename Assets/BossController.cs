using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    private enum BossState { Idle, Chase, Attack, Cast, Heal, Dead, Return }

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Slider bossHealthSlider;

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

    [Header("Lightning Skill")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject lightningWarningPrefab;
    [SerializeField] private float lightningCooldown = 5f;
    [SerializeField] private float lightningDelay = 0.5f;
    [SerializeField] private float lightningOffsetY = -0.5f;
    [SerializeField] private Transform lightningSpawnRoot;

    private Transform player;
    private Vector2 originPosition;
    private float currentHealth;
    private float lastLightningTime;
    private float lastHealTime;
    private bool isDead;

    private BossState currentState = BossState.Idle;
    private readonly int[] lightningOffsets = { -1, 1, -2, 2 };

    private Queue<GameObject> warningPool = new Queue<GameObject>();
    private Queue<GameObject> lightningPool = new Queue<GameObject>();

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originPosition = transform.position;
        currentHealth = maxHealth;
        bossHealthSlider.maxValue = maxHealth;
        bossHealthSlider.value = currentHealth;
        bossHealthSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);

        if (currentState == BossState.Return)
        {
            ReturnToOrigin();
            return;
        }

        if (horizontalDistance < detectRange && verticalDistance < 3f)
        {
            if (!bossHealthSlider.gameObject.activeSelf)
                bossHealthSlider.gameObject.SetActive(true);

            bool playerIsLeft = player.position.x < transform.position.x;
            bool bossFacingLeft = transform.localScale.x < 0f;

            if (horizontalDistance <= attackRange)
            {
                currentState = BossState.Attack;
            }
            else if (playerIsLeft != bossFacingLeft)
            {
                currentState = BossState.Chase;
            }
            else if (Time.time - lastLightningTime >= lightningCooldown)
            {
                currentState = BossState.Cast;
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
                AttackPlayer();
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

        float direction = player.position.x < transform.position.x ? -1f : 1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        transform.localScale = new Vector3(direction, 1f, 1f);
    }

    private void ReturnToOrigin()
    {
        float direction = originPosition.x < transform.position.x ? -1f : 1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        transform.localScale = new Vector3(direction, 1f, 1f);

        if (Mathf.Abs(transform.position.x - originPosition.x) < 0.1f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentState = BossState.Idle;
        }
    }

    private void AttackPlayer()
    {
        animator.SetTrigger("Attack");

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 direction = new Vector2(player.position.x - transform.position.x, 0).normalized;
            playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.StartCoroutine(playerController.PlayerHurt(attackDamage));
            }
        }
    }

    private IEnumerator UseLightningSkill()
    {
        animator.SetTrigger("Cast");

        foreach (int offset in lightningOffsets)
        {
            Vector2 warnPos = new Vector2(transform.position.x + offset, lightningSpawnRoot.position.y);
            GameObject warn = GetPooled(warningPool, lightningWarningPrefab);
            warn.transform.position = warnPos;
            warn.SetActive(true);
            yield return new WaitForSeconds(lightningDelay);
            GameObject bolt = GetPooled(lightningPool, lightningPrefab);
            bolt.transform.position = new Vector2(transform.position.x + offset, lightningSpawnRoot.position.y + lightningOffsetY);
            bolt.SetActive(true);
        }
    }

    private GameObject GetPooled(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0 && !pool.Peek().activeInHierarchy)
        {
            return pool.Dequeue();
        }
        GameObject obj = Instantiate(prefab);
        pool.Enqueue(obj);
        return obj;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        bossHealthSlider.value = currentHealth;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        currentState = BossState.Dead;
        rb.velocity = Vector2.zero;
    }

    private IEnumerator Heal()
    {
        animator.SetTrigger("Cast");
        yield return new WaitForSeconds(healCastDelay);

        float lostHealth = maxHealth - currentHealth;
        float healAmount = lostHealth * 0.5f;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        bossHealthSlider.value = currentHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ReturnZone"))
        {
            currentState = BossState.Return;
        }
    }
}
