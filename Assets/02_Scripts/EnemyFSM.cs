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

    [Header("AttackVariables")]
    private Transform player;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    public GameObject attackCollider;

    [Header("EnemyStats")]
    public float health = 5.0f;
    [SerializeField] private Animator enemyAnimator;
    private Vector2 originPosition;
    private Rigidbody2D rb;
    private float lastAttackTime;

    private void Start()
    {
        currentState = State.Idle;
        originPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("플레이어를 찾을 수 없습니다. 'Player' 태그가 있는지 확인하세요.");
    }

    private void Update()
    {
        if (health < 0f)
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
    private IEnumerator DeathCoroutine()
    {
        enemyAnimator.SetTrigger("Death");
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
        yield break;
    }
    private void Idle()
    {
        rb.velocity = Vector2.zero;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            currentState = State.Move;
        }
    }
    private void Move()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            Vector2 direction = (originPosition - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed;

            FlipSprite(direction);

            if (Vector2.Distance(transform.position, originPosition) < 0.1f)
            {
                rb.velocity = Vector2.zero;
                currentState = State.Idle;
            }
        }
        else if (distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector2.zero;
            currentState = State.Attack;
        }
        else
        {
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            rb.velocity = direction * moveSpeed;

            FlipSprite(direction);
        }
    }
    private void Attack()
    {
        enemyAnimator.SetTrigger("Attack");
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            currentState = State.Move;
            return;
        }

        rb.velocity = Vector2.zero;
        Vector2 lookDir = (player.position - transform.position).normalized;
        FlipSprite(lookDir);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            attackCollider.SetActive(true);
            Debug.Log("공격");
            lastAttackTime = Time.time;
        }
    }

    private void FlipSprite(Vector2 direction)
    {
        if (direction.x != 0)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = direction.x < 0 ? -1f : 1f;
            transform.localScale = localScale;
        }
    }
}