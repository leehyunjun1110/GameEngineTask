using System.Collections;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    private enum State
    {
        Idle,
        Move,
        Attack
    }
    private State currentState;

    [Header("isFlying")]
    [SerializeField] private bool isFlying = false;

    [Header("AttackVariables")]
    private Transform player;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("EnemyStats")]
    public int health = 5;
    [SerializeField] private int EnemyAttackDamage = 1;
    [SerializeField] private Animator enemyAnimator;
    private Vector2 originPosition;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private bool isMovable = true;
    private float nowscale;
    private bool isReturning = false;
    public bool isPortalVisible = false;

    private void Start()
    {
        currentState = State.Idle;
        nowscale = transform.localScale.x;
        originPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("플레이어를 찾을 수 없습니다.");
    }

    private void Update()
    {
        if (health <= 0)
        {
            StartCoroutine(DeathCoroutine());
        }

        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Move:
                Move();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    public void Hit()
    {
        enemyAnimator.SetTrigger("Hit");
    }

    private void Idle()
    {
        rb.velocity = Vector2.zero;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange && isMovable)
        {
            currentState = State.Move;
        }
    }

    private void Move()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2 direction;

        if (isReturning)
        {
            // 복귀 중: 원래 자리로 이동
            direction = (originPosition - (Vector2)transform.position).normalized;

            rb.velocity = direction * moveSpeed;
            FlipSprite(direction);

            if (!isFlying) enemyAnimator.SetBool("Run", true);

            // 원래 자리 도달 시 복귀 종료
            if (Vector2.Distance(transform.position, originPosition) < 0.1f)
            {
                rb.velocity = Vector2.zero;
                currentState = State.Idle;
                isReturning = false;
            }
        }
        else if (distanceToPlayer > attackRange && isMovable)
        {
            if (isFlying)
            {
                direction = ((Vector2)player.position - rb.position).normalized;
            }
            else
            {
                float xDirection = player.position.x - transform.position.x;
                direction = new Vector2(xDirection, 0).normalized;
            }

            rb.velocity = direction * moveSpeed;
            FlipSprite(direction);

            if (!isFlying) enemyAnimator.SetBool("Run", true);
        }
        else
        {
            rb.velocity = Vector2.zero;
            if (!isFlying) enemyAnimator.SetBool("Run", false);
            currentState = State.Attack;
        }
    }

    private void Attack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange && isMovable)
        {
            currentState = State.Move;
            return;
        }

        rb.velocity = Vector2.zero;
        Vector2 lookDir = (player.position - transform.position).normalized;
        FlipSprite(lookDir);

        if (!isFlying)
            enemyAnimator.SetBool("Run", false);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            StartCoroutine(PerformAttack());
            lastAttackTime = Time.time;
        }
    }

    private IEnumerator DeathCoroutine()
    {
        isPortalVisible = true;
        enemyAnimator.SetTrigger("Death");
        isMovable = false;
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
        yield break;
    }

    private IEnumerator PerformAttack()
    {
        float duration = 0.2f;
        float timer = 0f;
        bool playerHit = false;

        enemyAnimator.SetTrigger("Attack");

        while (timer < duration)
        {
            if (!playerHit)
            {
                Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange);
                foreach (Collider2D hit in hitPlayers)
                {
                    if (hit.CompareTag("Player"))
                    {
                        Debug.Log("플레이어 공격 성공");
                        PlayerController pc = hit.GetComponent<PlayerController>();
                        if (pc != null)
                        {
                            StartCoroutine(pc.PlayerHurt(EnemyAttackDamage));
                            playerHit = true;
                            break;
                        }
                    }
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void FlipSprite(Vector2 direction)
    {
        if (direction.x != 0)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = direction.x < 0 ? -nowscale : nowscale;
            transform.localScale = localScale;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ReturnZone")) // 트리거 오브젝트에 "ReturnZone" 태그 설정 필요
        {
            isReturning = true;
        }
    }
}