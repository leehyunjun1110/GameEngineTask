using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryEnemy : MonoBehaviour
{
    [Header("EnemyAttackVariables")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("EnemyVariables")]
    public float moveSpeed = 3f;
    public int health = 5;
    public bool isPortalVisible = false;

    private Rigidbody2D rb;
    private bool isMovingRight = true, isMovable = true;
    private Animator enemyAni;
    private float currentScale;

    private void Start()
    {
        enemyAni = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentScale = transform.localScale.x;
    }

    private void Update()
    {
        if (isMovable)
        {
            MovingMethod();
        }
    }
    private void MovingMethod()
    {
        if (isMovingRight)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
        }
    }

    public void Hit()
    {
        enemyAni.SetTrigger("Hit");
        if (health <= 0)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
            StartCoroutine(DeathCoroutine());
        }
    }
    private IEnumerator DeathCoroutine()
    {
        isMovable = false;
        isPortalVisible = true;
        enemyAni.SetTrigger("Death");
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Boundary":
                isMovingRight = !isMovingRight;
                transform.localScale = new Vector3(isMovingRight ? currentScale : -currentScale, transform.localScale.y, transform.localScale.z);
                break;
            case "Player":
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    PlayerController player = collision.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        StartCoroutine(player.PlayerHurt(attackDamage));
                        lastAttackTime = Time.time;
                    }
                }
                break;
        }
        
    }
}
