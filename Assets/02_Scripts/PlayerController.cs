using System.Runtime.CompilerServices;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerFunctionAnnounced")]
    [SerializeField] private Animator m_Animator;
    [SerializeField] private Rigidbody2D m_Rb;
    private Transform m_Transform;
    [SerializeField] private float characterSpeed = 2f;
    [SerializeField] private float characterJumpSpace = 200.0f;
    public GameObject m_Attack;
    public bool shiftdown = false, isAbleAttack = true, isMovable = true;

    [Header("PlayerJumpFunc")]
    private KeyCode[] jumpKeys = { KeyCode.UpArrow, KeyCode.Space };
    private float lastJumpTime = 0f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.09f;
    [SerializeField] private float groundCheckCooldown = 0.2f;

    [Header("PlayerVariables")]
    [SerializeField] private float attackCoolDown = 0.7f;
    [SerializeField] private int playerHealth = 3;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Transform = GetComponent<Transform>();
        
        m_Attack.SetActive(false);
    }

    void Update()
    {
        ShiftCheck();
        Move();
        JumpCheck();

        if (Input.GetKeyDown(KeyCode.A) && isAbleAttack)
        {
            StartCoroutine(AttackCheck());
        }
    }

    IEnumerator AttackCheck()
    {
        m_Attack.SetActive(true);
        m_Animator.SetTrigger("Attack");
        isAbleAttack = false;
        isMovable = false;

        yield return new WaitForSeconds(attackCoolDown);

        m_Attack.SetActive(false);
        isAbleAttack = true;
        isMovable = true;
    }

    private void ShiftCheck()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftdown = true;
            characterSpeed = 4f;
            m_Animator.SetBool("IsRun", true);
        }
        else
        {
            shiftdown = false;
            characterSpeed = 2f;
            m_Animator.SetBool("IsRun", false);
        }
    }

    private void JumpCheck()
    {
        if ((Time.time - lastJumpTime > groundCheckCooldown) && GroundCheck() && isMovable)
        {
            foreach (var key in jumpKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    m_Animator.SetTrigger("Jump");
                    characterJumpSpace = shiftdown ? 400.0f : 300.0f;
                    m_Rb.AddForce(Vector2.up * characterJumpSpace);
                    lastJumpTime = Time.time;
                    break;
                }
            }
        }
    }
    private bool GroundCheck()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(groundCheckPoint.position, Vector2.down, groundCheckDistance);
        bool gc = false;
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                gc = true;
                return gc;
            }
        }
        return gc;
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(-characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(-1f, 1f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        if (Input.GetKey(KeyCode.RightArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(1f, 1f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        m_Animator.SetBool("IsWalk", false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Door1":
                SceneManager.LoadScene("World2");
                break;

            case "Door2":
                SceneManager.LoadScene("World3");
                break;

            case "Door3":
                SceneManager.LoadScene("End");
                break;
        }
    }
}
